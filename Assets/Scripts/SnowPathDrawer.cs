using UnityEngine;

public class SnowPathDrawer : MonoBehaviour
{
    [Header("Required Components")]
    public ComputeShader snowComputeShader;
    public RenderTexture snowRT;

    [Header("Dependencies")]
    [SerializeField] private GaugeManager gaugeManager; // 게이지 관리자 연결

    [Header("Drawing Settings")]
    public float spotSize = 5f;
    public float maxSnowHeight = 0.3f;
    public float minVelocity = 0.01f;
    public float depletionRate = 1f; // 초당 게이지 소모량

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

    private void Awake()
    {
        // "SnowGround" 태그를 가진 모든 오브젝트를 찾음
        snowControllerObjs = GameObject.FindGameObjectsWithTag("SnowGround");
        lastPosition = transform.position;

        // GaugeManager가 연결되었는지 확인
        if (gaugeManager == null)
        {
            Debug.LogError("GaugeManager가 SnowPathDrawer에 연결되지 않았습니다! Inspector 창에서 연결해주세요.");
        }
    }

    private void FixedUpdate()
    {
        // 오브젝트의 속도 계산
        float velocity = Vector3.Distance(transform.position, lastPosition) / Time.fixedDeltaTime;

        // 모든 눈 바닥 오브젝트를 순회
        for (int i = 0; i < snowControllerObjs.Length; i++)
        {
            // 너무 멀리 있는 바닥은 무시
            if (Vector3.Distance(snowControllerObjs[i].transform.position, transform.position) > spotSize * 5f)
            {
                continue;
            }

            float groundY = snowControllerObjs[i].transform.position.y;
            float handY = transform.position.y;

            // 그리기 조건: 1. 높이가 맞고, 2. 최소 속도 이상으로 움직이고, 3. 게이지가 남아있을 때
            if (handY >= groundY && handY <= groundY + maxSnowHeight && velocity > minVelocity)
            {
                if (gaugeManager != null && gaugeManager.HasGauge)
                {
                    // 그리기 실행
                    snowController = snowControllerObjs[i].GetComponent<SnowController>();
                    snowRT = snowController.snowRT;
                    UpdateDrawPosition();
                    DrawOnRenderTexture();

                    // 게이지 소모 요청
                    gaugeManager.ConsumeGauge(depletionRate * Time.fixedDeltaTime);
                }
            }
        }
        lastPosition = transform.position;
    }

    // 그릴 위치를 렌더 텍스처 좌표로 변환
    void UpdateDrawPosition()
    {
        float scaleX = snowController.transform.localScale.x;
        float scaleZ = snowController.transform.localScale.z;

        float snowPosX = snowController.transform.position.x;
        float snowPosZ = snowController.transform.position.z;

        int posX = (int)(snowRT.width / 2 - (((transform.position.x - snowPosX) * snowRT.width / 2) / scaleX));
        int posY = (int)(snowRT.height / 2 - (((transform.position.z - snowPosZ) * snowRT.height / 2) / scaleZ));

        position = new Vector2Int(posX, posY);
    }

    // 컴퓨트 셰이더를 이용해 렌더 텍스처에 그림
    void DrawOnRenderTexture()
    {
        if (snowRT == null || snowComputeShader == null) return;

        int kernel_handle = snowComputeShader.FindKernel(drawSpotKernel);

        snowComputeShader.SetTexture(kernel_handle, snowImageProperty, snowRT);
        snowComputeShader.SetFloat(colorValueProperty, 0); // 0으로 설정하여 지우기
        snowComputeShader.SetFloat(resolutionProperty, snowRT.width);
        snowComputeShader.SetFloat(positionXProperty, position.x);
        snowComputeShader.SetFloat(positionYProperty, position.y);
        snowComputeShader.SetFloat(spotSizeProperty, spotSize);

        snowComputeShader.Dispatch(kernel_handle, snowRT.width / 8, snowRT.height / 8, 1);
    }
}