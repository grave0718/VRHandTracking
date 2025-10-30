using UnityEngine;
using System.Collections; // 코루틴(Cooldown)을 위해 필요

// 이 스크립트를 '이미지 저장' 버튼 역할을 할 3D 오브젝트에 붙입니다.
[RequireComponent(typeof(Collider))]
public class SaveImageTrigger : MonoBehaviour
{
    [Header("1. 연결")]
    [Tooltip("이미지 저장을 담당하는 SnowRTSaveUI 스크립트")]
    public SnowRTSaveUI snowSaveManager;

    [Header("2. 터치 감지")]
    [Tooltip("터치할 손의 태그")]
    public string handTag = "PlayerHand";
    [Tooltip("중복 터치 방지 쿨다운 (파일 저장은 시간이 걸릴 수 있음)")]
    public float cooldownTime = 1.0f; // 1초

    private bool _isReady = true;

    void Start()
    {
        // 1. 매니저가 연결되었는지 확인
        if (snowSaveManager == null)
        {
            // 씬에서 자동으로 찾아보기
            snowSaveManager = FindObjectOfType<SnowRTSaveUI>();
            if (snowSaveManager == null)
            {
                Debug.LogError($"[SaveImageTrigger] {gameObject.name}에 'snowSaveManager'가 연결되지 않았습니다!");
            }
        }

        // 2. 이 오브젝트의 Collider를 'Is Trigger'로 강제 설정
        GetComponent<Collider>().isTrigger = true;
    }

    // 3. 트리거 감지
    private void OnTriggerEnter(Collider other)
    {
        // 쿨다운이 끝났고, 지정된 태그의 손이 닿았다면
        if (_isReady && other.CompareTag(handTag))
        {
            if (snowSaveManager != null)
            {
                Debug.Log("이미지 저장 트리거 작동! -> SaveNow() 호출");
                
                // SnowRTSaveUI의 public 함수를 호출합니다.
                snowSaveManager.SaveNow();

                // 쿨다운 시작
                StartCoroutine(CooldownRoutine());
            }
        }
    }
    
    // 4. 중복 실행 방지를 위한 쿨다운 코루틴
    private IEnumerator CooldownRoutine()
    {
        _isReady = false;
        yield return new WaitForSeconds(cooldownTime);
        _isReady = true;
    }
}