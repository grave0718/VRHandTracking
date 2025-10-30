using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResetSandSwipe : MonoBehaviour
{
    [Header("초기화 대상")]
    [Tooltip("초기화할 SandSwitch 스크립트 (모래 바닥 오브젝트에 있음)")]
    public SandSwitch sandSwitch;

    [Header("터치 감지")]
    [Tooltip("터치할 손의 태그")]
    public string handTag = "PlayerHand";
    [Tooltip("중복 터치 방지 쿨다운")]
    public float cooldownTime = 1.0f;

    private bool _isReady = true;

    void Start()
    {
        // Collider를 Trigger로 설정
        GetComponent<Collider>().isTrigger = true;

        // SandSwitch가 연결되었는지 확인
        if (sandSwitch == null)
        {
            Debug.LogError($"[{nameof(ResetSandSwipe)}] sandSwitch가 연결되지 않았습니다! 인스펙터에서 연결해주세요.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 쿨다운이 준비되었고, 지정된 태그의 손이 닿았는지 확인
        if (_isReady && other.CompareTag(handTag))
        {
            Debug.Log("모래 텍스처 리셋 트리거 작동!");
            
            // SandSwitch의 ResetOverlay 함수 호출
            if (sandSwitch != null)
            {
                sandSwitch.ResetOverlay();
            }

            // 쿨다운 시작
            StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        _isReady = false;
        yield return new WaitForSeconds(cooldownTime);
        _isReady = true;
    }
}
