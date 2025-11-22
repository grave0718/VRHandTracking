using UnityEngine;
using System.Collections;

// 버튼 역할을 할 오브젝트에 이 스크립트를 추가하세요.
// AudioSource와 Collider 컴포넌트가 자동으로 추가됩니다.
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class SoundButton : MonoBehaviour
{
    [Header("사운드 설정")]
    [Tooltip("터치 시 재생할 오디오 클립")]
    public AudioClip touchSound;

    [Header("터치 설정")]
    [Tooltip("터치할 손의 태그")]
    public string handTag = "PlayerHand";
    
    [Tooltip("중복 재생 방지를 위한 쿨다운 시간")]
    public float cooldownTime = 0.5f;

    // 오디오 재생을 위한 AudioSource 컴포넌트
    private AudioSource audioSource;
    private bool isReady = true; // 쿨다운 상태

    void Awake()
    {
        // 1. AudioSource 컴포넌트를 가져옵니다.
        audioSource = GetComponent<AudioSource>();

        // 2. 게임 시작 시 소리가 자동으로 재생되지 않도록 설정합니다.
        audioSource.playOnAwake = false;

        // 3. Collider가 트리거로 설정되어 있는지 확인하고, 아니면 자동으로 설정합니다.
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}의 Collider가 Is Trigger가 아닙니다. 자동으로 설정합니다.");
            col.isTrigger = true;
        }
    }

    // 다른 Collider가 이 오브젝트의 트리거 영역에 들어왔을 때 호출됩니다.
    private void OnTriggerEnter(Collider other)
    {
        // 쿨다운이 끝났고, 닿은 오브젝트의 태그가 설정한 손 태그와 일치하는지 확인합니다.
        if (isReady && other.CompareTag(handTag))
        {
            // 할당된 오디오 클립이 있고, AudioSource 컴포넌트가 있다면 소리를 재생합니다.
            if (touchSound != null && audioSource != null)
            {
                // PlayOneShot: 다른 소리가 재생 중이어도 이 소리를 중첩해서 재생합니다.
                // 버튼 클릭처럼 짧은 효과음에 적합합니다.
                audioSource.PlayOneShot(touchSound);

                Debug.Log($"{gameObject.name} 터치! '{touchSound.name}' 사운드 재생.");
            }
            
            // 쿨다운 시작
            StartCoroutine(CooldownRoutine());
        }
    }

    // 짧은 시간 동안 중복 입력을 방지하는 코루틴
    private IEnumerator CooldownRoutine()
    {
        isReady = false;
        yield return new WaitForSeconds(cooldownTime);
        isReady = true;
    }
}
