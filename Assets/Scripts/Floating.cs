using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
        public float floatSpeed = 1.0f;


    public float floatHeight = 0.5f;

    // 오브젝트의 시작 위치를 저장할 변수
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // 오브젝트의 위치를 업데이트합니다. X와 Z는 그대로 두고 Y만 변경합니다.
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
}
