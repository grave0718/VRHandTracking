using UnityEngine;
using System.Collections;

// 이 스크립트를 "Undo 버튼", "Redo 버튼" 역할을 할 
// 3D 오브젝트 (왼손에 붙일)에 각각 추가합니다.
[RequireComponent(typeof(Collider))]
public class UndoRedoTrigger : MonoBehaviour
{
    // 이 트리거가 Undo를 할지, Redo를 할지 인스펙터에서 선택
    public enum TriggerAction
    {
        Undo,
        Redo
    }

    [Header("1. 기능 설정")]
    [Tooltip("이 트리거가 실행할 동작 (Undo 또는 Redo)")]
    public TriggerAction action = TriggerAction.Undo;

    [Header("2. 연결")]
    [Tooltip("씬에 있는 UndoRedoManager 스크립트를 여기에 연결")]
    public UndoRedoManager historyManager;

    [Header("3. 충돌 설정")]
    [Tooltip("이 버튼을 누를 손의 태그 (예: 오른손)")]
    public string handTag = "PlayerHand";

    [Header("4. 옵션")]
    [Tooltip("한 번 터치 후 다음 터치까지의 대기 시간 (중복 실행 방지)")]
    public float cooldownTime = 0.5f; // 0.5초

    private bool _isReady = true; // 쿨다운 상태 플래그

    void Start()
    {
        // 1. 매니저가 연결되었는지 확인
        if (historyManager == null)
        {
            // 씬에서 자동으로 찾아보기
            historyManager = FindObjectOfType<UndoRedoManager>();
            if (historyManager == null)
            {
                Debug.LogError($"[UndoRedoTrigger] {gameObject.name}에 'historyManager'가 연결되지 않았습니다!");
            }
        }

        // 2. 이 오브젝트의 Collider를 'Is Trigger'로 강제 설정
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[UndoRedoTrigger] {gameObject.name}의 Collider가 'Is Trigger'가 아닙니다. 자동으로 활성화합니다.");
            col.isTrigger = true;
        }
    }

    // 3. 핵심 로직
    private void OnTriggerEnter(Collider other)
    {
        // 쿨다운이 끝났고, 지정된 태그의 손이 닿았다면
        if (_isReady && other.CompareTag(handTag))
        {
            if (historyManager != null)
            {
                // 설정된 동작에 따라 매니저의 함수 호출
                if (action == TriggerAction.Undo)
                {
                    Debug.Log("UNDO 실행!");
                    historyManager.Undo(); // 매니저의 public 함수 호출
                }
                else if (action == TriggerAction.Redo)
                {
                    Debug.Log("REDO 실행!");
                    historyManager.Redo(); // 매니저의 public 함수 호출
                }

                // 쿨다운 시작 (중복 터치 방지)
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