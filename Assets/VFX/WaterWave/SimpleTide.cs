using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 가장 쉬운 "파도 전진/후퇴" 연출 + (옵션) 한쪽 에지 고정
[RequireComponent(typeof(MeshRenderer))]
public class SimpleTide : MonoBehaviour
{
    [Header("진행 방향 (오브젝트의 로컬 Z를 권장)")]
    public bool useLocalZ = true; // true면 Z, false면 X 방향으로 파도

    [Header("길이(미터)")]
    public float minLength = 1.0f;   // 가장 뒤로 갔을 때 길이
    public float maxLength = 4.0f;   // 가장 앞으로 왔을 때 길이

    [Header("타이밍")]
    public float cycleSeconds = 4.0f;  // 왕복 주기(초)
    public float phaseOffset = 0f;     // 시작 위상(여러 개 깔면 조금씩 다르게)

    [Header("앞/뒤 이동(선택)")]
    public bool alsoMove = true;       // 스케일만 바꾸면 밋밋할 수 있어요
    public float moveAmplitude = 0.2f; // 이동 폭(미터)

    // ── 추가: 에지 고정 옵션 ───────────────────────────────
    [Header("에지 고정(선택)")]
    public bool anchorEdge = false;          // 켜면 지정한 에지가 제자리 유지
    public enum Edge { Min, Max }            // Min: 로컬 -축 쪽 에지, Max: 로컬 +축 쪽 에지
    public Edge whichEdge = Edge.Max;        // 예) 오른쪽(로컬 +X)을 고정하려면 useLocalZ=false + Max

    Vector3 baseScale;
    Vector3 basePos;

    // 내부: 앵커 계산용
    MeshRenderer mr;
    Vector3 anchoredWorldPos;

    void Start()
    {
        baseScale = transform.localScale;
        basePos   = transform.position;

        mr = GetComponent<MeshRenderer>();

        // 시작 시점의 "고정할 에지" 월드 좌표 저장
        anchoredWorldPos = GetAnchorWorldPosition();
    }

    void Update()
    {
        if (cycleSeconds <= 0f) return;

        // 0..1로 오가는 값
        float t = Mathf.Sin((Time.time + phaseOffset) * (Mathf.PI * 2f) / cycleSeconds) * 0.5f + 0.5f;

        // 목표 길이
        float targetLen = Mathf.Lerp(minLength, maxLength, t);
        targetLen = Mathf.Max(0.0001f, targetLen);

        // 로컬 스케일 업데이트 (X 또는 Z 한 축만)
        Vector3 s = baseScale;
        if (useLocalZ) s.z = targetLen; else s.x = targetLen;
        transform.localScale = s;

        // 선택: 파도 진행 방향으로 살짝 앞/뒤 이동
        if (alsoMove && moveAmplitude > 0f)
        {
            Vector3 dir = useLocalZ ? transform.forward : transform.right; // 로컬 축 → 월드
            float offset = Mathf.Sin((Time.time + phaseOffset) * (Mathf.PI * 2f) / cycleSeconds) * moveAmplitude;
            transform.position = basePos + dir * offset;
        }

        // ★ 추가: 에지 고정 보정
        if (anchorEdge)
        {
            Vector3 curAnchor = GetAnchorWorldPosition(); // 스케일/이동 반영된 현재 에지 위치
            Vector3 delta = anchoredWorldPos - curAnchor; // 시작 위치와의 차이
            transform.position += delta;                   // 그만큼 반대로 이동해 에지 고정
        }
    }

    // 현재 상태에서 "고정할 에지"의 월드 좌표 계산
    Vector3 GetAnchorWorldPosition()
    {
        if (mr == null) mr = GetComponent<MeshRenderer>();

        // 진행 축의 월드 방향
        Vector3 worldDir = (useLocalZ ? transform.forward : transform.right).normalized;

        // 렌더러 바운즈 기준 중심과 길이
        Vector3 centerW = mr.bounds.center;
        float worldLenAlongDir = Vector3.Dot(
            mr.bounds.size,
            new Vector3(Mathf.Abs(worldDir.x), Mathf.Abs(worldDir.y), Mathf.Abs(worldDir.z))
        );

        // Min(로컬 -축) / Max(로컬 +축) 에지 위치
        return (whichEdge == Edge.Min)
            ? centerW - worldDir * (worldLenAlongDir * 0.5f)
            : centerW + worldDir * (worldLenAlongDir * 0.5f);
    }
}
