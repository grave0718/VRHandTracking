using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq를 사용하기 위해 추가

// 이 스크립트는 왼손 컨트롤러 앵커(LeftHandAnchor 등)에 붙입니다.
public class PalmUpUIController : MonoBehaviour
{
    [Header("숨김/표시할 3D 오브젝트")]
    [Tooltip("손바닥을 볼 때 표시할 오브젝트들 (예: Undo/Redo 버튼)")]
    public List<GameObject> relatedObjects;

    [Header("감지 조건")]
    [Tooltip("손바닥이 위를 향했다고 인정할 각도 (0~1 사이, 1에 가까울수록 수직)")]
    [Range(0f, 1f)]
    public float palmUpThreshold = 0.8f;

    // [ ----- 여기를 추가했습니다 ----- ]
    // relatedObjects 및 그 자식들에게 포함된 모든 Renderer 목록
    private List<Renderer> _renderersToToggle;
    // [ ------------------------------- ]

    void Start()
    {
        // 1. 렌더러 목록을 미리 찾아 저장합니다 (성능 최적화)
        _renderersToToggle = new List<Renderer>();
        if (relatedObjects != null)
        {
            foreach (GameObject obj in relatedObjects)
            {
                if (obj != null)
                {
                    // GetComponentsInChildren를 사용해
                    // 해당 오브젝트와 그 모든 자식의 Renderer를 찾아 리스트에 추가
                    _renderersToToggle.AddRange(obj.GetComponentsInChildren<Renderer>());
                }
            }
        }

        // 2. 시작 시 모두 안 보이게 설정
        SetAllVisible(false);
    }

    void Update()
    {
        // 1. 손바닥 방향 체크 (이전과 동일)
        float dotDown = Vector3.Dot(this.transform.up, Vector3.down);

        // 2. 임계값과 비교
        bool shouldShow = (dotDown > palmUpThreshold);

        // 3. 현재 렌더러 상태와 다를 때만 SetAllVisible 호출 (최적화)
        if (_renderersToToggle != null && _renderersToToggle.Count > 0)
        {
            // 첫 번째 렌더러의 상태를 대표로 확인
            if (shouldShow != _renderersToToggle[0].enabled)
            {
                SetAllVisible(shouldShow);
            }
        }
    }

    // [ ----- 여기를 수정했습니다 ----- ]
    // SetActive(false) 대신 renderer.enabled = false/true를 호출
    private void SetAllVisible(bool isVisible)
    {
        if (_renderersToToggle == null) return;

        // 리스트에 있는 모든 렌더러의 'enabled' 속성을 변경
        foreach (Renderer rend in _renderersToToggle)
        {
            if (rend != null)
            {
                rend.enabled = isVisible;
            }
        }
    }
}