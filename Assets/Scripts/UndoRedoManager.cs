using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UndoRedoManager : MonoBehaviour
{
    [Header("History Settings")]
    [Tooltip("표면(RenderTexture)당 저장할 최대 스냅샷 수 (Undo 가능 횟수)")]
    public int maxHistoryPerSurface = 20;

    [Header("Debug")]
    [Tooltip("간단 실행 로그를 콘솔에 출력")]
    public bool enableDebug = true;

    // 표면(RenderTexture)별 스택
    private Dictionary<RenderTexture, Stack<RenderTexture>> _undoMap =
        new Dictionary<RenderTexture, Stack<RenderTexture>>();
    private Dictionary<RenderTexture, Stack<RenderTexture>> _redoMap =
        new Dictionary<RenderTexture, Stack<RenderTexture>>();
    private Dictionary<RenderTexture, RenderTexture> _baseMap =
        new Dictionary<RenderTexture, RenderTexture>(); // 초기 상태 스냅샷

    // 최근 사용 표면(버튼 입력 시 대상)
    private RenderTexture _lastActiveRT;

    // ---------- 외부 호출 API ----------

    /// <summary>표면을 처음 사용할 때 등록 (초기 스냅샷 확보)</summary>
    public void RegisterSurface(RenderTexture rt)
    {
        if (rt == null) return;
        if (_undoMap.ContainsKey(rt)) return;

        _undoMap[rt] = new Stack<RenderTexture>();
        _redoMap[rt] = new Stack<RenderTexture>();

        var baseSnap = CloneRT(rt);
        _baseMap[rt] = baseSnap;

        if (enableDebug) Debug.Log($"[HIST] Surface registered {RtInfo(rt)}");
    }

    /// <summary>스트로크 종료 시 현재 상태를 커밋(히스토리 푸시)</summary>
    public void CommitStroke(RenderTexture rt)
    {
        if (rt == null) return;
        if (!_undoMap.ContainsKey(rt)) RegisterSurface(rt);

        _undoMap[rt].Push(CloneRT(rt));
        _redoMap[rt].Clear();               // 새로운 브랜치 시작
        _lastActiveRT = rt;

        TrimHistory(rt);

        if (enableDebug) Debug.Log($"[HIST] Commit stroke {RtInfo(rt)}  Undo={_undoMap[rt].Count}");
    }

    public void Undo() { Undo(_lastActiveRT); }
    public void Redo() { Redo(_lastActiveRT); }

    /// <summary>특정 표면 되돌리기</summary>
    public void Undo(RenderTexture rt)
    {
        if (rt == null || !_undoMap.ContainsKey(rt)) { WarnNoSurface("Undo"); return; }

        var undo = _undoMap[rt];
        var redo = _redoMap[rt];

        if (undo.Count == 0)
        {
            if (_baseMap.TryGetValue(rt, out var baseSnap))
            {
                redo.Push(CloneRT(rt));
                Blit(baseSnap, rt);
                _lastActiveRT = rt;
                if (enableDebug) Debug.Log("[UNDO] → Base Snapshot 복원");
            }
            return;
        }

        redo.Push(CloneRT(rt));           // 현재 상태를 redo로 이동
        var prev = undo.Pop();            // 이전 스냅샷 적용
        Blit(prev, rt);
        _lastActiveRT = rt;

        if (enableDebug) Debug.Log("[UNDO] 실행됨");
    }

    /// <summary>특정 표면 다시 실행</summary>
    public void Redo(RenderTexture rt)
    {
        if (rt == null || !_redoMap.ContainsKey(rt)) { WarnNoSurface("Redo"); return; }

        var undo = _undoMap[rt];
        var redo = _redoMap[rt];

        if (redo.Count == 0) return;

        undo.Push(CloneRT(rt));           // 현재 상태를 undo로 이동
        var next = redo.Pop();            // 다음 스냅샷 적용
        Blit(next, rt);
        _lastActiveRT = rt;

        if (enableDebug) Debug.Log("[REDO] 실행됨");
    }

    // ---------- UI 버튼 래퍼 ----------
    public void OnUndoButton()
    {
        if (_lastActiveRT == null && enableDebug)
            Debug.LogWarning("[WARN] 최근 표면이 없습니다. 먼저 그림을 그려 표면을 활성화하세요.");
        Undo();
    }

    public void OnRedoButton()
    {
        if (_lastActiveRT == null && enableDebug)
            Debug.LogWarning("[WARN] 최근 표면이 없습니다. Undo 이후에 Redo를 사용하세요.");
        Redo();
    }

    // ---------- 내부 유틸 ----------

    private static RenderTexture CloneRT(RenderTexture src)
    {
        if (src == null) return null;
        var desc = src.descriptor;
        var dst = new RenderTexture(desc);
        dst.name = src.name + "_snap";
        dst.Create();
        Graphics.Blit(src, dst);
        return dst;
    }

    private static void Blit(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst);
    }

    /// <summary>히스토리 초과 시 가장 오래된 스냅샷 1개 제거(메모리 해제 포함)</summary>
    private void TrimHistory(RenderTexture rt)
    {
        var undo = _undoMap[rt];
        if (undo.Count <= maxHistoryPerSurface) return;

        // Stack 바닥(가장 오래된) 1개 제거 위해 재구성
        var arr = undo.ToArray(); // top->bottom
        var oldest = arr[arr.Length - 1];

        SafeDisposeRT(oldest);

        // oldest 제외하여 다시 스택 구성
        var rebuilt = new Stack<RenderTexture>(arr.Take(arr.Length - 1));
        _undoMap[rt] = rebuilt;

        if (enableDebug) Debug.Log($"[HIST] Trimmed oldest. Undo={_undoMap[rt].Count}");
    }

    private static void SafeDisposeRT(RenderTexture rt)
    {
        if (rt == null) return;
        if (rt.IsCreated()) rt.Release();
        Object.Destroy(rt);
    }

    private static string RtInfo(RenderTexture rt)
    {
        if (rt == null) return "(RT: null)";
        return $"(RT: {rt.name}, {rt.width}x{rt.height}, {rt.format})";
    }

    private void WarnNoSurface(string op)
    {
        if (enableDebug) Debug.LogWarning($"[WARN] {op}: 대상 표면이 없습니다. 최근에 그림을 그렸는지 확인하세요.");
    }
}
