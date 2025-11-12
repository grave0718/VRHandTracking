using System.Collections; // 코루틴(Cooldown)을 위해 필요
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialNext : MonoBehaviour
{
    [Header("1. 필수 연결")]
    [Tooltip("튜토리얼을 관리하는 TutorialManager 스크립트")]
    public TutorialManager tutorialManager;

    [Header("2. 추가 기능 연결 (선택)")]
    [Tooltip("튜토리얼 7번 인덱스에서 사용할 TeleportManager")]
    public TeleportManager teleportManager;

    [Tooltip("튜토리얼 동안 비활성화할 '다시하기' 버튼 (GameEnd)")]
    public GameObject gameEndButton;

    [Tooltip("튜토리얼 동안 비활성화할 추가 버튼 (예: GameFin)")]
    public GameObject gameFinButton;

    [Tooltip("튜토리얼 인덱스 4에서 활성화할 오브젝트")]
    public GameObject objectToActivateOnIndex4;

    [Tooltip("튜토리얼 인덱스 6에서 활성화할 오브젝트")]
    public GameObject objectToActivateOnIndex6;

    [Header("3. 터치 감지")]
    [Tooltip("버튼을 터치할 손의 태그")]
    public string handTag = "PlayerHand";

    [Tooltip("중복 터치를 방지하기 위한 쿨다운 시간(초)")]
    public float cooldownTime = 3.0f;

    private bool _isReady = true; // 쿨다운 상태 플래그

    void Start()
    {
        // 1. 필수 매니저 연결 확인 및 자동 찾기
        if (tutorialManager == null)
        {
            tutorialManager = FindObjectOfType<TutorialManager>();
            if (tutorialManager == null)
            {
                Debug.LogError(
                    $"[TutorialNext] {gameObject.name}에 'tutorialManager'가 연결되지 않았습니다!"
                );
            }
        }

        // 2. 선택적 매니저 자동 찾기
        if (teleportManager == null)
        {
            teleportManager = FindObjectOfType<TeleportManager>();
            if (teleportManager == null)
            {
                Debug.LogWarning(
                    $"[TutorialNext] TeleportManager를 찾을 수 없습니다. 텔레포트 기능이 작동하지 않을 수 있습니다."
                );
            }
        }

        // GameEnd 버튼 비활성화 (인스펙터에서 연결된 경우)
        if (gameEndButton != null)
        {
            gameEndButton.GetComponent<Collider>().enabled = false;
            Debug.Log("[TutorialNext] 튜토리얼 시작: GameEnd 버튼의 충돌을 비활성화합니다.");
        }

        // GameFin 버튼 비활성화 (인스펙터에서 연결된 경우)
        if (gameFinButton != null)
        {
            gameFinButton.GetComponent<Collider>().enabled = false;
            Debug.Log("[TutorialNext] 튜토리얼 시작: GameFin 버튼의 충돌을 비활성화합니다.");
        }

        // 3. 이 오브젝트의 Collider를 'Is Trigger'로 강제 설정
        GetComponent<Collider>().isTrigger = true;

        if (objectToActivateOnIndex4 != null)
            objectToActivateOnIndex4.SetActive(false);
        if (objectToActivateOnIndex6 != null)
            objectToActivateOnIndex6.SetActive(false);
    }
    void Update(){
                        var objectRenderer = GetComponent<Renderer>();
       var nextIndex = tutorialManager.TutorialIndex;
        if(nextIndex == 8){

            objectToActivateOnIndex6.SetActive(false);
            objectRenderer.enabled = true;
        }
    }

    // 4. 트리거 감지 및 로직 처리
    private void OnTriggerEnter(Collider other)
    {
        // 쿨다운이 끝났고, 지정된 태그의 손이 닿았고, 매니저가 연결되어 있다면
        if (_isReady && other.CompareTag(handTag) && tutorialManager != null)
        {
            Debug.Log("튜토리얼 다음 버튼 트리거 작동!");

            // 다음 튜토리얼로 넘어가기 전 현재 인덱스 저장
            int currentIndex = tutorialManager.TutorialIndex;
            if (objectToActivateOnIndex4 != null)
                objectToActivateOnIndex4.SetActive(false);
            if (objectToActivateOnIndex6 != null)
                objectToActivateOnIndex6.SetActive(false);

            // 다음 튜토리얼 표시
            tutorialManager.ShowNextTutorial();

            // 넘어간 후의 인덱스 가져오기
            int nextIndex = tutorialManager.TutorialIndex;

            // --- 요청된 추가 기능 실행 ---



            // 1. 튜토리얼 인덱스 7 (8번째 이미지)에서 텔레포트 실행 및 버튼 상태 변경
            if (nextIndex == 7)
            {
                var objectRenderer = GetComponent<Renderer>();
                if (objectRenderer != null)
                {
                    objectRenderer.enabled = false;
                }

                objectToActivateOnIndex6.SetActive(true);
                if (teleportManager != null)
                {
                    // teleportManager.TeleportPlayer();
                    Debug.Log("[TutorialNext] 튜토리얼 인덱스 7: 플레이어를 텔레포트했습니다.");
                }
                else
                {
                    Debug.LogWarning(
                        "[TutorialNext] TeleportManager가 연결되지 않아 텔레포트할 수 없습니다."
                    );
                }

                // GameEnd(다시하기) 버튼 활성화
                if (gameEndButton != null)
                {
                    var gameEndCollider = gameEndButton.GetComponent<Collider>();
                    if (gameEndCollider != null)
                    {
                        gameEndCollider.enabled = true;
                        Debug.Log(
                            "[TutorialNext] 튜토리얼 인덱스 7: GameEnd 버튼의 충돌을 활성화합니다."
                        );
                    }
                }

                // 이 오브젝트(다음 버튼)의 충돌 비활성화
                GetComponent<Collider>().enabled = false;
                Debug.Log("[TutorialNext] 튜토리얼 인덱스 7: 다음 버튼의 충돌을 비활성화합니다.");
            }

            // 2. 특정 인덱스에서 오브젝트 활성화
            if (nextIndex == 4 && objectToActivateOnIndex4 != null)
            {
                objectToActivateOnIndex4.SetActive(true);
                Debug.Log(
                    $"[TutorialNext] 튜토리얼 인덱스 4: '{objectToActivateOnIndex4.name}' 활성화."
                );
            }

            if (nextIndex == 6 && objectToActivateOnIndex6 != null)
            {
                objectToActivateOnIndex6.SetActive(true);
                Debug.Log(
                    $"[TutorialNext] 튜토리얼 인덱스 6: '{objectToActivateOnIndex6.name}' 활성화."
                );
            }

            // 튜토리얼의 마지막 인덱스를 넘어서면 (모든 이미지가 꺼지면) 튜토리얼 종료
            if (nextIndex >= tutorialManager.TutorialImages.Count)
            {
                gameObject.SetActive(false);
                // GameEnd 버튼 활성화
                if (gameEndButton != null)
                {
                    gameEndButton.GetComponent<Collider>().enabled = true;
                    Debug.Log("[TutorialNext] 튜토리얼 종료: GameEnd 버튼의 충돌을 활성화합니다.");
                }
                // GameFin 버튼 활성화
                if (gameFinButton != null)
                {
                    gameFinButton.GetComponent<Collider>().enabled = true;
                    Debug.Log("[TutorialNext] 튜토리얼 종료: GameFin 버튼의 충돌을 활성화합니다.");
                }
            }
            // 쿨다운 시작
            StartCoroutine(CooldownRoutine());
        }
    }

    // 5. 중복 실행 방지를 위한 쿨다운 코루틴
    private IEnumerator CooldownRoutine()
    {
        _isReady = false;
        yield return new WaitForSeconds(cooldownTime);
        _isReady = true;
    }
}
