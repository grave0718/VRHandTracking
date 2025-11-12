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

    // relatedObjects 및 그 자식들에게 포함된 모든 Renderer 목록
    private List<Renderer> _renderersToToggle;
    // 안 보일 때 충돌을 막기 위해 Collider 목록도 함께 관리합니다.
    private List<Collider> _collidersToToggle;

    void Start()
    {
        // 1. 렌더러와 콜라이더 목록을 미리 찾아 저장합니다 (성능 최적화)
        _renderersToToggle = new List<Renderer>();
        _collidersToToggle = new List<Collider>();
        if (relatedObjects != null)
        {
            foreach (GameObject obj in relatedObjects)
            {
                if (obj != null)
                {
                    // GetComponentsInChildren를 사용해 해당 오브젝트와 그 모든 자식의 컴포넌트를 찾아 리스트에 추가
                    _renderersToToggle.AddRange(obj.GetComponentsInChildren<Renderer>());
                    _collidersToToggle.AddRange(obj.GetComponentsInChildren<Collider>());
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

    // SetActive(false) 대신 renderer.enabled = false/true를 호출
    private void SetAllVisible(bool isVisible)
    {
        if (_renderersToToggle != null)
        {
            // 리스트에 있는 모든 렌더러의 'enabled' 속성을 변경
            foreach (Renderer rend in _renderersToToggle)
            {
                if (rend != null)
                {
                    rend.enabled = isVisible;
                }
            }
        }

        // 콜라이더의 활성화 상태도 함께 변경하여 충돌을 제어합니다.
        if (_collidersToToggle != null)
        {
            foreach (Collider coll in _collidersToToggle)
            {
                if (coll != null) coll.enabled = isVisible;
            }
        }
    }
}
