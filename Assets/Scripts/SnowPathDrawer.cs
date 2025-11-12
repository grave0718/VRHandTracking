using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SnowPathDrawer : MonoBehaviour
{
    [Header("Required Components")]
    public ComputeShader snowComputeShader;
    public RenderTexture snowRT;

    [Tooltip("그리기 효과를 적용할 손의 Renderer")]
    public Renderer handRenderer;

    [Tooltip("그릴 때 적용할 손의 머티리얼")]
    public Material drawingMaterial;

    [Header("Dependencies")]
    [SerializeField]
    private GaugeManager gaugeManager; // 게이지 관리자

    [SerializeField]
    private UndoRedoManager undoRedoManager; // ★ 추가: 언두/리두 매니저

    [Header("Drawing Settings")]
    public float spotSize = 5f;
    public float maxSnowHeight = 0.3f;
    public float minVelocity = 0.01f;
    public float depletionRate = 1f; // 초당 게이지 소모량

    public AudioClip drawingSound;
    private AudioSource audioSource;

    // Compute Shader 프로퍼티 이름들
    private const string snowImageProperty = "snowImage";
    private const string colorValueProperty = "colorValueToAdd";
    private const string resolutionProperty = "resolution";
    private const string positionXProperty = "positionX";
    private const string positionYProperty = "positionY";
    private const string spotSizeProperty = "spotSize";
    private const string drawSpotKernel = "DrawSpot";

    private Vector2Int position;
    private SnowController snowController;
    private GameObject[] snowControllerObjs;
    private Vector3 lastPosition;

    // ★ 추가: 스트로크 경계 검출용
    private bool wasDrawing = false;
    private bool isDrawingNow = false;
    private RenderTexture activeRTDuringStroke = null; // 현재 스트로크가 진행 중인 표면
    private RenderTexture lastUsedRTThisFrame = null; // 이번 프레임에 실제로 사용된 표면

    private Material originalHandMaterial;

    private void Awake()
    {
        // "SnowGround" 태그를 가진 모든 오브젝트를 찾음
        snowControllerObjs = GameObject.FindGameObjectsWithTag("SnowGround");
        lastPosition = transform.position;

        if (gaugeManager == null)
            Debug.LogError(
                "GaugeManager가 SnowPathDrawer에 연결되지 않았습니다! Inspector에서 연결해주세요."
            );

        audioSource = GetComponent<AudioSource>();
        if (drawingSound != null)
        {
            audioSource.clip = drawingSound;
        }
        audioSource.playOnAwake = false;
        audioSource.loop = true;

        // 손 머티리얼 변경 초기화
        if (handRenderer != null)
        {
            // 시작할 때의 머티리얼을 '원본'으로 저장합니다.
            originalHandMaterial = handRenderer.material;
            if (drawingMaterial == null) {
                Debug.LogWarning("drawingMaterial이 연결되지 않아 손 머티리얼 변경 기능을 사용할 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning(
                "Hand Renderer가 연결되지 않아 손 시각 효과를 사용할 수 없습니다."
            );
        }
        if (undoRedoManager == null)
        {
            Debug.LogWarning(
                "UndoRedoManager가 연결되지 않았습니다. Undo/Redo가 작동하지 않습니다."
            );
        }
        else
        {
            // [ ★★★ 핵심 수정 ★★★ ]
            // FixedUpdate가 시작되기 전에(그림 그리기 전에),
            // "깨끗한" 상태의 모든 RT를 미리 등록합니다.
            Debug.Log(
                $"[HIST] {snowControllerObjs.Length}개의 눈 표면을 찾았습니다. 모두 등록합니다..."
            );
            foreach (var obj in snowControllerObjs)
            {
                SnowController sc = obj.GetComponent<SnowController>();
                if (sc != null && sc.snowRT != null)
                {
                    // 각 컨트롤러가 가진 RT를 Undo 매니저에 등록
                    undoRedoManager.RegisterSurface(sc.snowRT);
                }
                else
                {
                    Debug.LogWarning(
                        $"SnowGround 오브젝트 {obj.name}에 SnowController 또는 snowRT가 없습니다!"
                    );
                }
            }
        }
    }

    private void FixedUpdate()
    {
        float velocity = Vector3.Distance(transform.position, lastPosition) / Time.fixedDeltaTime;

        bool anyDrawnThisFrame = false;
        bool handCurrentlyInDrawingZone = false; // 손이 눈 영역 안에 있는지 여부 (속도 무관)
        lastUsedRTThisFrame = null;

        // 모든 눈 바닥 오브젝트를 순회
        for (int i = 0; i < snowControllerObjs.Length; i++)
        {
            // 너무 멀리 있는 바닥은 무시
            if (
                Vector3.Distance(snowControllerObjs[i].transform.position, transform.position)
                > spotSize * 5f
            )
                continue;

            float groundY = snowControllerObjs[i].transform.position.y;
            float handY = transform.position.y;

            // 손이 유효한 눈 높이 범위 안에 있는지 확인 (속도 무관)
            if (handY >= groundY && handY <= groundY + maxSnowHeight)
            {
                handCurrentlyInDrawingZone = true;
            }

            // 그리기 조건: 1. 높이 범위, 2. 최소 속도, 3. 게이지 보유
            if (handY >= groundY && handY <= groundY + maxSnowHeight && velocity > minVelocity)
            {
                if (gaugeManager != null && gaugeManager.HasGauge)
                {
                    // 그리기 실행
                    snowController = snowControllerObjs[i].GetComponent<SnowController>();
                    snowRT = snowController.snowRT;
                    UpdateDrawPosition();
                    DrawOnRenderTexture();

                    // 게이지 소모
                    gaugeManager.ConsumeGauge(depletionRate * Time.fixedDeltaTime);

                    anyDrawnThisFrame = true;
                    lastUsedRTThisFrame = snowRT;
                }
            }
        }

        // ===== 손 머티리얼 로직 (위치 및 게이지 기반, 깜빡임 방지를 위해 속도 조건 제외) =====
        if (handRenderer != null)
        {
            if (handCurrentlyInDrawingZone && gaugeManager != null && gaugeManager.HasGauge && drawingMaterial != null)
            {
                // 손이 유효한 눈 영역 안에 있고 게이지가 있으면 그리기 머티리얼 적용
                if (handRenderer.material != drawingMaterial)
                {
                    handRenderer.material = drawingMaterial;
                }
            }
            else if (handRenderer.material != originalHandMaterial) // 이미 원본 머티리얼이 아니라면 변경
            {
                // 그렇지 않으면 원본 머티리얼로 복원
                handRenderer.material = originalHandMaterial;
            }
        }

        // ===== 스트로크 경계 처리 =====
        isDrawingNow = anyDrawnThisFrame;

        // (1) 스트로크 시작: 이전 프레임엔 안 그렸고, 이번 프레임엔 그림
        if (!wasDrawing && isDrawingNow)
        {
            activeRTDuringStroke = lastUsedRTThisFrame;
            if (audioSource != null && drawingSound != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }


            if (undoRedoManager != null && activeRTDuringStroke != null)
            {
                // 표면이 처음이면 등록(베이스 스냅샷 확보)
                // undoRedoManager.RegisterSurface(activeRTDuringStroke);
                // 시작 시에는 커밋하지 않음 (끝날 때 1회 커밋)
                // Debug.Log("[DRAW] Stroke Start");
            }
        }

        // (2) 스트로크 중 표면이 바뀌는 경우(멀티 바닥 사이를 넘나드는 경우) → 이전 표면 커밋 후 새 표면에서 새 스트로크 시작
        if (
            wasDrawing
            && isDrawingNow
            && activeRTDuringStroke != null
            && lastUsedRTThisFrame != null
            && activeRTDuringStroke != lastUsedRTThisFrame
        )
        {
            if (undoRedoManager != null)
            {
                undoRedoManager.CommitStroke(activeRTDuringStroke); // 이전 표면 커밋
                // undoRedoManager.RegisterSurface(lastUsedRTThisFrame);       // 새 표면 등록
            }
            activeRTDuringStroke = lastUsedRTThisFrame;
            // Debug.Log("[DRAW] Surface switched mid-stroke → Commit & Start new");
        }

        // (3) 스트로크 종료: 이번 프레임엔 안 그렸고, 이전 프레임엔 그리고 있었음
        if (wasDrawing && !isDrawingNow)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            if (undoRedoManager != null && activeRTDuringStroke != null)
            {
                undoRedoManager.CommitStroke(activeRTDuringStroke);
                // Debug.Log("[DRAW] Stroke End → Commit");
            }
            activeRTDuringStroke = null;
        }

        wasDrawing = isDrawingNow;
        lastPosition = transform.position;
    }

    // 그릴 위치를 렌더 텍스처 좌표로 변환
    void UpdateDrawPosition()
    {
        float scaleX = snowController.transform.localScale.x;
        float scaleZ = snowController.transform.localScale.z;

        float snowPosX = snowController.transform.position.x;
        float snowPosZ = snowController.transform.position.z;

        int posX = (int)(
            snowRT.width / 2 - (((transform.position.x - snowPosX) * snowRT.width / 2) / scaleX)
        );
        int posY = (int)(
            snowRT.height / 2 - (((transform.position.z - snowPosZ) * snowRT.height / 2) / scaleZ)
        );

        position = new Vector2Int(posX, posY);
    }

    // 컴퓨트 셰이더를 이용해 렌더 텍스처에 그림
    void DrawOnRenderTexture()
    {
        if (snowRT == null || snowComputeShader == null)
            return;

        int kernel_handle = snowComputeShader.FindKernel(drawSpotKernel);

        snowComputeShader.SetTexture(kernel_handle, snowImageProperty, snowRT);
        snowComputeShader.SetFloat(colorValueProperty, 0); // 0으로 설정하여 지우기(기존 로직 유지)
        snowComputeShader.SetFloat(resolutionProperty, snowRT.width);
        snowComputeShader.SetFloat(positionXProperty, position.x);
        snowComputeShader.SetFloat(positionYProperty, position.y);
        snowComputeShader.SetFloat(spotSizeProperty, spotSize);

        snowComputeShader.Dispatch(kernel_handle, snowRT.width / 8, snowRT.height / 8, 1);
    }
}
