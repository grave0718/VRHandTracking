using UnityEngine;

public class FIxRotate : MonoBehaviour
{
    [Tooltip("오브젝트의 회전 고정 방식")]
    public RotationFixMode rotationMode = RotationFixMode.FaceCameraUpright; // 기본값: 카메라를 바라보되 수직 유지

    [Tooltip("WorldFixed 모드일 때 오브젝트가 유지할 월드 회전 (예: Quaternion.identity는 월드 축에 정렬)")]
    public Quaternion worldFixedRotation = Quaternion.identity; // 기본값: 월드 축에 정렬 (월드 정면)

    public enum RotationFixMode
    {
        WorldFixed,         // 월드 공간에 고정된 회전
        FaceCameraUpright   // 카메라를 바라보되, 기울어지지 않고 항상 수직 유지
    }

    void LateUpdate()
    {
        // 이 스크립트는 오브젝트의 위치를 변경하지 않습니다.
        // 위치는 부모 오브젝트(예: Head)에 의해 결정되거나 다른 스크립트에서 설정되어야 합니다.

        // 오브젝트의 월드 회전을 고정된 값으로 설정합니다.
        // 이렇게 하면 부모(머리)의 회전과 관계없이 오브젝트의 월드 회전이 유지됩니다.
        switch (rotationMode)
        {
            case RotationFixMode.WorldFixed:
                transform.rotation = worldFixedRotation;
                break;
            case RotationFixMode.FaceCameraUpright:
                if (Camera.main != null)
                {
                    // 카메라의 정면 방향을 가져오되, Y축 회전만 사용하고 X, Z축 회전은 무시하여 수직을 유지합니다.
                    Vector3 cameraForward = Camera.main.transform.forward;
                    cameraForward.y = 0; // Y축 방향 성분을 0으로 만들어 기울어지지 않게 합니다.
                    if (cameraForward == Vector3.zero) // 카메라가 정확히 위나 아래를 볼 때를 대비
                    {
                        cameraForward = Vector3.forward; // 기본적으로 월드 정면을 바라보게 합니다.
                    }
                    transform.rotation = Quaternion.LookRotation(cameraForward, Vector3.up);
                }
                else
                {
                    // 카메라가 없으면 기본 월드 고정 회전 사용
                    transform.rotation = worldFixedRotation;
                }
                break;
        }
    }
}
