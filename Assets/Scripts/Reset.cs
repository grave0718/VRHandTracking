using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Reset : MonoBehaviour
{
    [Header("1. 초기화 대상")]
    [Tooltip("게이지를 관리하는 GaugeManager (없으면 자동으로 찾음)")]
    public GaugeManager gaugeManager;

    [Tooltip("Undo/Redo를 관리하는 UndoRedoManager (없으면 자동으로 찾음)")]
    public UndoRedoManager undoRedoManager;

    [Tooltip("모래 텍스처를 관리하는 SandSwitch (없으면 자동으로 찾음)")]
    public SandSwitch sandSwitch;

    public LoadSavedImageTrigger loadSavedImageTrigger;
    public Light lensFlareLight;

    [Tooltip("플레이어(XR Origin)의 Transform")]
    public Transform playerOrigin;

    [Tooltip("플레이어를 되돌릴 시작 위치 Transform")]
    public Transform startPosition;

    [Header("2. 초기화 효과")]
    [Tooltip("초기화 시 활성화할 파도 효과 오브젝트 (Simple Tide)")]
    public GameObject waterWaveObject;

    [Tooltip("효과 재생 후 실제 초기화까지의 지연 시간(초)")]
    public float resetDelay = 3.0f;

    [Header("3. 사용 횟수 제한")]
    [Tooltip(
        "다시하기 버튼을 사용할 수 있는 총 횟수. 튜토리얼에서 1회 사용되므로, 게임 중 1회 더 사용하려면 2로 설정하세요. 0 이하면 무제한입니다."
    )]
    public int maxResetCount = 0;
    private int _resetsUsed = 0;

    [Header("4. 튜토리얼 연동 (선택)")]
    [Tooltip("튜토리얼 진행 중 특정 단계에서 다음으로 넘기기 위한 TutorialManager")]
    public TutorialManager tutorialManager;

    [Tooltip("튜토리얼 7단계 이후 다시 활성화할 TutorialNext 버튼 오브젝트")]
    public GameObject tutorialNextButton;
    public GameObject tutorialEndButton;
    public GameObject tutorialRestartButton;

    [Header("5. 터치 감지")]
    [Tooltip("터치할 손의 태그")]
    public string handTag = "PlayerHand";

    private bool _isReady = true;
    private SnowController[] snowControllers; // 모든 눈 바닥 컨트롤러


    public int isTuto = 0;

    void Start()
    {
        // Collider를 Trigger로 설정
        GetComponent<Collider>().isTrigger = true;

        // 필수 컴포넌트 자동 찾기 (연결 안됐을 경우)
        if (loadSavedImageTrigger == null)
            loadSavedImageTrigger = FindObjectOfType<LoadSavedImageTrigger>();
        if (loadSavedImageTrigger == null)
            Debug.LogWarning(
                $"[{nameof(Reset)}] LoadSavedImageTrigger를 찾을 수 없어 불러온 이미지 초기화가 불가능합니다.",
                this
            );

        if (lensFlareLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    lensFlareLight = l;
                    break;
                }
            }
            if (lensFlareLight == null)
                Debug.LogWarning(
                    $"[{nameof(Reset)}] lensFlareLight가 연결되지 않았습니다. 렌즈 플레어 재활성화 기능이 작동하지 않습니다.",
                    this
                );
        }
        if (gaugeManager == null)
            gaugeManager = FindObjectOfType<GaugeManager>();
        if (gaugeManager == null)
            Debug.LogError($"[{nameof(Reset)}] GaugeManager를 찾을 수 없습니다!", this);

        if (undoRedoManager == null)
            undoRedoManager = FindObjectOfType<UndoRedoManager>();
        if (undoRedoManager == null)
            Debug.LogWarning(
                $"[{nameof(Reset)}] UndoRedoManager를 찾을 수 없어 히스토리 초기화가 불가능합니다.",
                this
            );

        if (sandSwitch == null)
            sandSwitch = FindObjectOfType<SandSwitch>();
        if (sandSwitch == null)
            Debug.LogWarning(
                $"[{nameof(Reset)}] SandSwitch를 찾을 수 없어 모래 텍스처 초기화가 불가능합니다.",
                this
            );

        // 튜토리얼 연동 자동 찾기 (선택 사항이므로 경고 없음)
        if (tutorialManager == null)
        {
            tutorialManager = FindObjectOfType<TutorialManager>();
        }

        if (tutorialNextButton == null)
        {
            TutorialNext tn = FindObjectOfType<TutorialNext>();
            if (tn != null)
            {
                tutorialNextButton = tn.gameObject;
            }
        }

        // 모든 SnowController 찾기
        var snowGrounds = GameObject.FindGameObjectsWithTag("SnowGround");
        snowControllers = new SnowController[snowGrounds.Length];
        for (int i = 0; i < snowGrounds.Length; i++)
        {
            snowControllers[i] = snowGrounds[i].GetComponent<SnowController>();
        }
        if (snowControllers.Length == 0)
            Debug.LogWarning(
                $"[{nameof(Reset)}] SnowController를 가진 'SnowGround' 태그 오브젝트를 찾을 수 없습니다.",
                this
            );

        if (playerOrigin == null)
            Debug.LogError($"[{nameof(Reset)}] playerOrigin이 연결되지 않았습니다!", this);
        if (startPosition == null)
            Debug.LogError($"[{nameof(Reset)}] startPosition이 연결되지 않았습니다!", this);

        if (waterWaveObject == null)
            Debug.LogWarning(
                $"[{nameof(Reset)}] waterWaveObject가 연결되지 않았습니다. 파도 효과가 재생되지 않습니다.",
                this
            );
        else
            // 시작할 때 파도 오브젝트를 비활성화 상태로 만듭니다.
            waterWaveObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isReady && other.CompareTag(handTag))
        {
            // 사용 횟수 제한 체크
            if (maxResetCount > 0 && _resetsUsed >= maxResetCount)
            {
                Debug.Log(
                    $"[Reset] 다시하기 횟수를 모두 소진했습니다. (사용: {_resetsUsed} / 최대: {maxResetCount})"
                );
                // 여기에 횟수 소진 시 피드백(소리 등)을 추가할 수 있습니다.
                return;
            }

            // 튜토리얼 연동 로직 추가
            if (tutorialManager != null && tutorialManager.TutorialIndex == 7)
            {
                Debug.Log(
                    "[Reset] 튜토리얼 7단계에서 트리거됨. 다음 튜토리얼로 진행하고 '다음' 버튼을 다시 활성화합니다."
                );
                // tutorialEndButton.SetActive(true);
                tutorialRestartButton.SetActive(true);
                tutorialNextButton.SetActive(true);
               
                tutorialManager.ShowNextTutorial();
                if (tutorialNextButton != null)
                {
                    var nextButtonCollider = tutorialNextButton.GetComponent<Collider>();
                    if (nextButtonCollider != null)
                    {
                        nextButtonCollider.enabled = true;
                    }

                    var nextButtonRenderer = tutorialNextButton.GetComponent<Renderer>();

                    //nextButtonRenderer.enabled = false;

                }
                else
                {
                    Debug.LogWarning(
                        "[Reset] tutorialNextButton이 연결되지 않아 '다음' 버튼을 다시 활성화할 수 없습니다."
                    );
                }
            }

            Debug.Log("리셋 시퀀스 시작!");
            StartCoroutine(ResetSequenceCoroutine());
        }
    }

    private IEnumerator ResetSequenceCoroutine()
    {
        // 1. 시퀀스 시작, 재실행 방지
        _isReady = false;

        // 2. 즉시 실행: 플레이어 위치 초기화
        if (playerOrigin != null && startPosition != null)
        {
            playerOrigin.position = startPosition.position;
            playerOrigin.rotation = startPosition.rotation;
            Debug.Log("플레이어 위치가 즉시 초기화되었습니다.");
        }

        // 3. 즉시 실행: 파도 효과 활성화
        if (waterWaveObject != null)
        {
            waterWaveObject.SetActive(true);
            Debug.Log("파도 효과를 활성화합니다.");
        }
        if (lensFlareLight != null)
        {
            var flare = lensFlareLight.GetComponent<UnityEngine.Rendering.LensFlareComponentSRP>();
            if (flare != null)
            {
                flare.enabled = true;
                Debug.Log($"[{nameof(Reset)}] 렌즈 플레어를 재활성화했습니다.");
            }
        }

        // 4. 지연 시간만큼 대기
        yield return new WaitForSeconds(resetDelay);

        // 5. 지연 후 실행: 나머지 모든 항목 초기화
        Debug.Log($"{resetDelay}초 후, 나머지 항목들을 초기화합니다.");

        // 5a. 게이지 초기화
        if (gaugeManager != null)
        {
            gaugeManager.ResetGauge();
            Debug.Log("게이지가 초기화되었습니다.");
        }

        // 5b. 모든 렌더 텍스처 초기화 (하얗게 만들기)
        if (snowControllers != null)
        {
            foreach (var sc in snowControllers)
            {
                if (sc != null)
                {
                    sc.SetRTColorToWhite();
                }
            }

            // 5e. 불러온 이미지 및 버튼 비활성화
            if (loadSavedImageTrigger != null)
            {
                loadSavedImageTrigger.DeactivateLoadedImageAndButtons();
                Debug.Log($"[{nameof(Reset)}] 불러온 이미지와 관련 버튼을 비활성화했습니다.");
            }
            Debug.Log("모든 눈 텍스처가 초기화되었습니다.");
        }

        // 5c. Undo/Redo 기록 초기화
        if (undoRedoManager != null)
        {
            undoRedoManager.ClearAllHistory();
            Debug.Log("Undo/Redo 기록이 초기화되었습니다.");

            // 히스토리를 비운 후, 모든 표면을 다시 등록하여 초기 상태 스냅샷을 새로 만듭니다.
            if (snowControllers != null)
            {
                foreach (var sc in snowControllers)
                {
                    if (sc != null && sc.snowRT != null)
                    {
                        undoRedoManager.RegisterSurface(sc.snowRT);
                    }
                }
                Debug.Log("초기화된 텍스처를 새 히스토리의 시작점으로 등록했습니다.");
            }
        }

        // 5d. 모래 텍스처 오버레이 초기화
        if (sandSwitch != null)
        {
            sandSwitch.ResetOverlay();
            Debug.Log("모래 텍스처가 초기화되었습니다.");
        }

        // 5e. 파도 효과 비활성화
        if (waterWaveObject != null)
        {
            waterWaveObject.SetActive(false);
            Debug.Log("파도 효과를 비활성화합니다.");
        }

        // 6. 사용 횟수 증가
        if (maxResetCount > 0)
        {
            _resetsUsed++;
        }

        // 7. 시퀀스 완료, 다시 사용 가능하도록 설정
        _isReady = true;
        Debug.Log("리셋 시퀀스 완료. 다시 사용할 수 있습니다.");
    }
}
