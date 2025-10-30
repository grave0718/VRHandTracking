using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 이 스크립트가 붙은 오브젝트는 반드시 Collider가 있어야 합니다.
[RequireComponent(typeof(Collider))]
public class GameEnd : MonoBehaviour
{
    [Header("활성화할 UI")]
    [Tooltip("트리거에 닿았을 때 활성화할 UI 패널")]
    public GameObject panelToActivate;

    [Header("터치 감지")]
    [Tooltip("트리거를 발동시킬 오브젝트의 태그 (예: VR 손)")]
    public string handTag = "PlayerHand";

    void Start()
    {
        // 1. 이 오브젝트의 Collider를 'Is Trigger'로 강제 설정합니다.
        //    이것이 켜져 있어야 OnTriggerEnter가 작동합니다.
        GetComponent<Collider>().isTrigger = true;

        // 2. 패널이 연결되었는지 확인하고, 시작 시 비활성화합니다.
        if (panelToActivate != null)
        {
            panelToActivate.SetActive(false);
        }
        else
        {
            Debug.LogError($"[GameEnd] {gameObject.name}: 활성화할 패널(panelToActivate)이 연결되지 않았습니다!");
        }
    }

    // 3. 다른 Collider가 이 오브젝트의 트리거 영역에 들어왔을 때 호출됩니다.
    private void OnTriggerEnter(Collider other)
    {
        // 닿은 오브젝트의 태그가 설정된 handTag와 일치하는지 확인합니다.
        if (other.CompareTag(handTag))
        {
            // 활성화할 패널이 제대로 연결되어 있다면
            if (panelToActivate != null)
            {
                // 패널을 활성화합니다.
                panelToActivate.SetActive(true);
                Debug.Log($"[GameEnd] {other.name}이(가) 트리거에 닿았습니다. {panelToActivate.name} 패널을 활성화합니다.");
            }
        }
    }
}

