using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))] // AudioSource 컴포넌트 강제 추가
public class ResetSandSwipe : MonoBehaviour
{
    [Header("초기화 대상")]
    [Tooltip("초기화할 SandSwitch 스크립트 (모래 바닥 오브젝트에 있음)")]
    public SandSwitch sandSwitch;

    [Header("사운드 및 터치 설정")]
    public AudioClip touchSound;
    [Tooltip("터치할 손의 태그")]
    public string handTag = "PlayerHand";
    [Tooltip("중복 터치 방지 쿨다운")]
    public float cooldownTime = 1.0f;

    private AudioSource _audioSource;
    private float _nextReadyTime = 0f; // 다음 터치가 가능한 시간을 기록

    void Awake() // Start 대신 Awake 사용
    {
        // 1. 오디오 소스 컴포넌트 가져오기
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false; // 시작 시 자동 재생 방지

        // 2. Collider를 Trigger로 설정
        GetComponent<Collider>().isTrigger = true;

        // 3. SandSwitch가 연결되었는지 확인
        if (sandSwitch == null)
        {
            Debug.LogError($"[{nameof(ResetSandSwipe)}] sandSwitch가 연결되지 않았습니다! 인스펙터에서 연결해주세요.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 현재 시간이 다음 준비 시간보다 크고, 지정된 태그의 손이 닿았는지 확인
        if (Time.time >= _nextReadyTime && other.CompareTag(handTag))
        {
            // 다음 준비 시간을 현재 시간 + 쿨다운 시간으로 설정
            _nextReadyTime = Time.time + cooldownTime;

            Debug.Log("모래 텍스처 리셋 트리거 작동!");

            // 사운드가 할당되어 있다면 재생
            if (touchSound != null)
            {
                _audioSource.PlayOneShot(touchSound);
            }

            // SandSwitch의 ResetOverlay 함수 호출
            if (sandSwitch != null)
            {
                sandSwitch.ResetOverlay();
            }
        }
    }
}
