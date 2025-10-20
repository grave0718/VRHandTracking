using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure Undo/Redo manager for shader-based painting.
/// - Records strokes as lists of (uv, value, size).
/// - Rebuild() calls your provided callbacks to clear and redraw.
/// How to wire:
///   manager.SetCallbacks(ClearToInitial, DrawSpotAtUV);
///   // BeginStroke(value,size) → AddPoint(uv) ... → EndStroke()
///   // Undo(), Redo()
/// </summary>
public class UndoRedoManager : MonoBehaviour
{
    [Header("Point filtering")]
    [Tooltip("Minimum UV distance between recorded points in a stroke to reduce over-sampling.")]
    [SerializeField] private float minUvDistance = 0.0015f;

    // ===== Public API =====
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    /// <summary>Set your paint callbacks.</summary>
    /// <param name="clearToInitial">Clear RT to initial state (e.g., Graphics.Blit(initialMap, rt))</param>
    /// <param name="drawSpotAtUV">Draw a spot at uv ∈ [0,1]^2 with value & size (value: e.g., 0 for erase)</param>
    public void SetCallbacks(Action clearToInitial, Action<Vector2, float, float> drawSpotAtUV)
    {
        _clearToInitial = clearToInitial ?? throw new ArgumentNullException(nameof(clearToInitial));
        _drawSpotAtUV = drawSpotAtUV ?? throw new ArgumentNullException(nameof(drawSpotAtUV));
    }

    /// <summary>Start a new stroke. Example: BeginStroke(0f, brushSize) for eraser.</summary>
        public void BeginStroke(float value, float size)
    {
        if (_drawing) return;
        _drawing = true;
        _current = _listPool.Get();
        _currentValue = value;
        _currentSize = size;
        _lastRecorded = new Vector2(9999, 9999);

        Debug.Log($"[UndoRedo] BeginStroke  value={value} size={size}  undo={_undo.Count} redo={_redo.Count}");
    }

    public void AddPoint(Vector2 uv)
    {
        if (!_drawing) return;
        if (_current.Count > 0 && (uv - _lastRecorded).sqrMagnitude < (minUvDistance * minUvDistance))
            return;

        _current.Add(new Op { uv = uv, value = _currentValue, size = _currentSize });
        _lastRecorded = uv;

        _drawSpotAtUV?.Invoke(uv, _currentValue, _currentSize);
    }

    public void EndStroke()
    {
        if (!_drawing) return;
        _drawing = false;

        if (_current != null && _current.Count > 0)
        {
            _undo.Push(_current);
            // 새 작업이 들어오면 redo는 사라지는 게 정상 동작임
            int cleared = _redo.Count;
            while (_redo.Count > 0) _listPool.Release(_redo.Pop());

            Debug.Log($"[UndoRedo] EndStroke  pushedPoints={_current.Count}  undo={_undo.Count}  redoCleared={cleared}");
        }
        else
        {
            _listPool.Release(_current);
            Debug.Log("[UndoRedo] EndStroke  (no points, nothing pushed)");
        }
        _current = null;
    }

    public void Undo()
    {
        if (_undo.Count == 0)
        {
            Debug.LogWarning("[UndoRedo] Undo() called but undo stack is empty.");
            return;
        }

        var s = _undo.Pop();
        _redo.Push(s);
        Debug.Log($"[UndoRedo] Undo  moved stroke points={s.Count}  undo={_undo.Count}  redo={_redo.Count}");
        Rebuild();
    }

    public void Redo()
    {
        if (_redo.Count == 0)
        {
            Debug.LogWarning("[UndoRedo] Redo() called but redo stack is empty.");
            return;
        }

        var s = _redo.Pop();
        _undo.Push(s);
        Debug.Log($"[UndoRedo] Redo  moved stroke points={s.Count}  undo={_undo.Count}  redo={_redo.Count}");
        Rebuild();
    }

    public void Rebuild()
    {
        if (_clearToInitial == null || _drawSpotAtUV == null)
        {
            Debug.LogWarning("[UndoRedo] Rebuild() but callbacks not set.");
            return;
        }

        _clearToInitial.Invoke();

        var arr = _undo.ToArray(); // 최신→과거
        int totalOps = 0;
        for (int i = arr.Length - 1; i >= 0; --i)
        {
            var stroke = arr[i];
            totalOps += stroke.Count;
            for (int j = 0; j < stroke.Count; j++)
            {
                var op = stroke[j];
                _drawSpotAtUV(op.uv, op.value, op.size);
            }
        }
        Debug.Log($"[UndoRedo] Rebuild done  strokes={arr.Length}  totalOps={totalOps}");
    }

    // ===== Internal =====
    private struct Op
    {
        public Vector2 uv;
        public float value;
        public float size;
    }

    private readonly Stack<List<Op>> _undo = new();
    private readonly Stack<List<Op>> _redo = new();
    private List<Op> _current;
    private float _currentValue, _currentSize;
    private bool _drawing;
    private Vector2 _lastRecorded;

    private Action _clearToInitial;
    private Action<Vector2, float, float> _drawSpotAtUV;

    // Very small List pool to reduce GC churn
    private readonly ListPool _listPool = new(64);

    private sealed class ListPool
    {
        private readonly Stack<List<Op>> _pool = new();
        private readonly int _defaultCapacity;

        public ListPool(int defaultCapacity) => _defaultCapacity = Mathf.Max(8, defaultCapacity);

        public List<Op> Get()
        {
            if (_pool.Count > 0)
            {
                var l = _pool.Pop();
                l.Clear();
                return l;
            }
            return new List<Op>(_defaultCapacity);
        }

        public void Release(List<Op> list)
        {
            list.Clear();
            _pool.Push(list);
        }
    }
}
