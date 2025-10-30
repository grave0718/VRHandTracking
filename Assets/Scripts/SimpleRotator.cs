using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    // 인스펙터(Inspector) 창에서 회전 속도를 조절할 수 있습니다.
    [Tooltip("초당 회전할 각도 (Y축 기준)")]
    public float rotationSpeed = 5.0f;

    // Update는 매 프레임마다 호출됩니다.
    void Update()
    {
        // transform.Rotate(x, y, z)
        // Y축(Vector3.up)을 기준으로 매 프레임 회전시킵니다.
        // Time.deltaTime을 곱해서 컴퓨터 성능(FPS)과 관계없이 일정한 속도를 보장합니다.
        transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f);

        // 아래 코드를 사용해도 동일하게 Y축 기준으로 회전합니다.
        // transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}