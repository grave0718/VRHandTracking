using UnityEngine;

// 이 스크립트는 "손으로 터치할" 오브젝트에 붙입니다.
// 이 오브젝트에는 반드시 Collider와 AudioSource가 있어야 합니다.
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class SandTriggerButton : MonoBehaviour
{
    [Header("타겟 설정")]
    [Tooltip("텍스처를 바꿀 메인 SandSwitch 스크립트 (모래 오브젝트에 있음)")]
    public SandSwitch targetSandSwitch;

    [Header("동작 설정")]
    [Tooltip("이 버튼을 누르면 바뀔 텍스처 인덱스 (0, 1, 2...)")]
    public int textureIndexToSelect = 0;

    [Header("사운드 및 터치 설정")]
    [Tooltip("터치 시 재생할 소리")]
    public AudioClip touchSound;
    [Tooltip("중복 터치를 방지하기 위한 쿨다운 시간(초)")]
    public float cooldownTime = 0.5f; // 0.5초 쿨다운
    [Tooltip("VR 손 오브젝트에 설정된 태그")]
    public string handTag = "PlayerHand";

    private AudioSource _audioSource;
    private float _nextReadyTime = 0f; // 다음 터치가 가능한 시간을 기록

    void Awake()
    {
        // 1. 오디오 소스 컴포넌트 가져오기
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false; // 시작 시 자동 재생 방지

        // 2. 실수 방지: 이 스크립트가 붙은 오브젝트의 Collider는
        //    반드시 Is Trigger가 켜져 있어야 합니다.
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[SandTriggerButton] {gameObject.name}의 Collider가 Is Trigger가 아닙니다. 자동으로 설정합니다.");
            col.isTrigger = true;
        }

        // 3. 실수 방지: 타겟이 설정되었는지 확인
        if (targetSandSwitch == null)
        {
            Debug.LogError($"[SandTriggerButton] {gameObject.name}에 targetSandSwitch가 연결되지 않았습니다!");
        }
    }

    // 4. 핵심: 손이 닿으면 SandSwitch에게 명령을 내리고 소리를 재생합니다.
    private void OnTriggerEnter(Collider other)
    {
        // 현재 시간이 다음 준비 시간보다 크고, 타겟이 있고, 닿은 것이 "손"이라면
        if (Time.time >= _nextReadyTime && targetSandSwitch != null && other.CompareTag(handTag))
        {
            // 다음 준비 시간을 현재 시간 + 쿨다운 시간으로 설정
            _nextReadyTime = Time.time + cooldownTime;

            Debug.Log($"손이 {gameObject.name}에 닿음! -> SandSwitch에게 {textureIndexToSelect}번 텍스처로 변경하라고 명령");
            
            // SandSwitch 스크립트의 public 함수를 호출합니다.
            targetSandSwitch.SelectByIndex(textureIndexToSelect);

            // 사운드가 할당되어 있다면 재생
            if (touchSound != null)
            {
                _audioSource.PlayOneShot(touchSound);
            }
        }
    }
}

