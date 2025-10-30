using UnityEngine;
using System.Collections; // IEnumerator 사용을 위해 추가

// 이 스크립트를 터치 피드백을 주고 싶은 3D 버튼 오브젝트에 붙입니다.
// 이 오브젝트에는 반드시 Collider와 Renderer가 있어야 합니다.
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))] // 메시 렌더러가 필요
public class TouchFeedbackButton : MonoBehaviour
{
    [Header("1. 텍스처 설정")]
    [Tooltip("버튼의 기본(평상시) 텍스처")]
    public Texture defaultTexture;
    [Tooltip("버튼이 터치되었을 때 표시할 텍스처")]
    public Texture pressedTexture;

    [Header("2. 터치 감지 설정")]
    [Tooltip("버튼을 터치할 VR 손 오브젝트의 태그")]
    public string handTag = "PlayerHand";
    [Tooltip("한 번 터치 후 다음 터치 피드백까지의 쿨다운 시간")]
    public float cooldownTime = 0.3f; // 0.3초

    private Renderer _buttonRenderer;
    private Material _buttonMaterial;
    private bool _isReady = true; // 쿨다운 상태 플래그

    void Awake() // Start 대신 Awake에서 초기화 (다른 스크립트보다 먼저 실행되도록)
    {
        _buttonRenderer = GetComponent<Renderer>();
        if (_buttonRenderer == null)
        {
            Debug.LogError($"[TouchFeedbackButton] {gameObject.name}에 Renderer 컴포넌트가 없습니다!");
            enabled = false;
            return;
        }

        // 재질 인스턴스를 얻어서 공유되지 않도록 함
        // (같은 재질을 사용하는 다른 오브젝트의 텍스처가 바뀌는 것을 방지)
        _buttonMaterial = _buttonRenderer.material; 
        
        // 초기 텍스처 설정
        if (_buttonMaterial != null && defaultTexture != null)
        {
            _buttonMaterial.mainTexture = defaultTexture;
        }

        // Collider의 Is Trigger가 활성화되어 있는지 확인
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[TouchFeedbackButton] {gameObject.name}의 Collider가 Is Trigger가 아닙니다. 자동으로 설정합니다.");
            col.isTrigger = true;
        }
    }

    // 손이 트리거 영역에 들어왔을 때 호출
    private void OnTriggerEnter(Collider other)
    {
        // 쿨다운이 끝났고, 지정된 태그의 손이 닿았다면
        if (_isReady && other.CompareTag(handTag))
        {
            // 텍스처 변경
            if (_buttonMaterial != null && pressedTexture != null)
            {
                _buttonMaterial.mainTexture = pressedTexture;
                Debug.Log($"{gameObject.name}이(가) 터치되었습니다! 텍스처 변경.");
            }

            // 쿨다운 시작 (중복 터치 피드백 방지)
            StartCoroutine(ResetTextureAfterCooldown());
        }
    }

    // 텍스처를 일정 시간 후 원래대로 되돌리는 코루틴
    private IEnumerator ResetTextureAfterCooldown()
    {
        _isReady = false; // 쿨다운 시작
        yield return new WaitForSeconds(cooldownTime); // 지정된 시간만큼 대기

        // 쿨다운이 끝나면 텍스처를 원래대로 되돌리고 다시 터치 가능하게 설정
        if (_buttonMaterial != null && defaultTexture != null)
        {
            _buttonMaterial.mainTexture = defaultTexture;
        }
        _isReady = true; // 쿨다운 끝
    }
}