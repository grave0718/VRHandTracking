using UnityEngine;
using UnityEngine.Rendering; 

[RequireComponent(typeof(Collider))] // 이 스크립트는 Collider가 꼭 필요함
public class TeleportManager : MonoBehaviour
{
    [Header("대상 설정")]
    [Tooltip("텔레포트시킬 대상 (예: 플레이어의 XR Origin)")]
    public Transform playerToTeleport;

    [Header("목표 지점")]
    [Tooltip("텔레포트시킬 '도착' 지점 (예: end Position 오브젝트)")]
    public Transform targetDestination;

    [Header("추가 액션")]
    [Tooltip("렌즈 플레어를 끌 디렉셔널 라이트 (오브젝트 자체를 드래그)")]
    public Light directionalLight;

    private void Awake()
    {
        // 1. 트리거 콜라이더 확인
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[TriggerTeleportZone] {name} 오브젝트의 Collider가 'Is Trigger'에 체크되어 있지 않습니다. 자동으로 true로 설정합니다.", this);
            col.isTrigger = true;
        }

        // 2. 필수 슬롯 확인
        if (playerToTeleport == null)
            Debug.LogError($"[TriggerTeleportZone] {name}: 'Player To Teleport'가 할당되지 않았습니다!", this);
        if (targetDestination == null)
            Debug.LogError($"[TriggerTeleportZone] {name}: 'Target Destination'이 할당되지 않았습니다!", this);
        
        if (directionalLight == null)
            Debug.LogWarning($"[TriggerTeleportZone] {name}: 'Directional Light'가 할당되지 않았습니다. 렌즈 플레어 끄기 기능이 작동하지 않습니다.", this);
    }

    /// <summary>
    /// 플레이어를 지정된 도착 지점으로 즉시 텔레포트시킵니다.
    /// </summary>
    public void TeleportPlayer()
    {
        // 1. 필수 대상들이 모두 할당되었는지 확인
        if (playerToTeleport == null || targetDestination == null)
        {
            Debug.LogError($"[TeleportManager] 텔레포트 실패: 'playerToTeleport' 또는 'targetDestination'이 할당되지 않았습니다.");
            return;
        }

        // 2. 텔레포트 실행
        playerToTeleport.SetPositionAndRotation(
            targetDestination.position,
            targetDestination.rotation
        );
        Debug.Log($"[TeleportManager] {playerToTeleport.name}을(를) {targetDestination.name} 위치로 텔레포트했습니다.");

        // 3. 렌즈 플레어 찾아서 비활성화
        if (directionalLight != null)
        {
            var flare = directionalLight.GetComponent<UnityEngine.Rendering.LensFlareComponentSRP>();
            if (flare != null)
            {
                flare.enabled = false;
                Debug.Log($"[TeleportManager] {directionalLight.name}의 렌즈 플레어를 비활성화했습니다.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 필수 대상들이 모두 할당되었는지 확인
        if (playerToTeleport == null || targetDestination == null) return;

        // 2. 트리거에 닿은 오브젝트가 'playerToTeleport'가 맞는지 확인
        if (other.transform.root == playerToTeleport.root)
        {
            // 텔레포트 로직을 공용 함수로 호출
            TeleportPlayer();
        }
    }
}
