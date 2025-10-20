using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SandSwitch : MonoBehaviour
{
    [Header("Renderer & Slot")]
    public Renderer targetRenderer;   // 모래 MeshRenderer
    public int materialIndex = 0;     // 모래가 쓰는 머티리얼 슬롯(대부분 0)

    [Header("Overlay Textures")]
    public List<Texture2D> textures;  // 전환할 텍스처들(3장)

    [Header("UI (Optional)")]
    public Button btnRandom, btn1, btn2, btn3;

    [Header("Effect")]
    public float fadeTime = 0.25f;

    // Shader Graph property names (Blackboard와 반드시 동일)
    static readonly int ID_OverlayTex   = Shader.PropertyToID("_OverlayTex");
    static readonly int ID_OverlayAlpha = Shader.PropertyToID("_OverlayAlpha");

    Material _mat;         // 실제로 화면에 그려지는 인스턴스 머티리얼
    int _currentIndex = -1;
    Coroutine _fade;

    void Awake()
    {
        if (!targetRenderer)
        {
            Debug.LogError("[SandSwitch] targetRenderer not set.");
            enabled = false; return;
        }

        // 1) 슬롯 존재 확인
        var shared = targetRenderer.sharedMaterials;
        if (materialIndex < 0 || materialIndex >= shared.Length)
        {
            Debug.LogError($"[SandSwitch] materialIndex {materialIndex} out of range. count={shared.Length}");
            enabled = false; return;
        }

        // 2) 인스턴스화: sharedMaterials를 복사해 material로 교체 (다른 오브젝트와 단절)
        var inst = targetRenderer.materials;     // <- 이 순간 인스턴스 생성
        // 필요시 새 머티리얼로 한번 더 복제해 명확히 분리
        inst[materialIndex] = new Material(inst[materialIndex]);
        targetRenderer.materials = inst;
        _mat = targetRenderer.materials[materialIndex];

        // 3) 프로퍼티 존재 확인
        bool okTex   = _mat.HasProperty(ID_OverlayTex);
        bool okAlpha = _mat.HasProperty(ID_OverlayAlpha);
        Debug.Log($"[SandSwitch] Bound mat='{_mat.name}' id={_mat.GetInstanceID()}  hasTex={okTex}  hasAlpha={okAlpha}");

        if (!okTex || !okAlpha)
        {
            Debug.LogError("[SandSwitch] Shader Graph에 _OverlayTex / _OverlayAlpha 프로퍼티가 없습니다. Blackboard 이름을 확인하세요.");
            enabled = false; return;
        }

        // 초기 알파 0
        _mat.SetFloat(ID_OverlayAlpha, 0f);

        // 버튼 연결
        if (btnRandom) btnRandom.onClick.AddListener(SelectRandom);
        if (btn1) btn1.onClick.AddListener(() => SelectByIndex(0));
        if (btn2) btn2.onClick.AddListener(() => SelectByIndex(1));
        if (btn3) btn3.onClick.AddListener(() => SelectByIndex(2));

        Debug.Log($"[SandSwitch] Start: textures.Count={(textures==null? -1 : textures.Count)}");
    }

    public void SelectRandom()
    {
        if (textures == null || textures.Count == 0) return;
        int next = (_currentIndex < 0) ? Random.Range(0, textures.Count)
                                       : GetRandomExcept(_currentIndex, textures.Count);
        SelectByIndex(next);
    }

    public void SelectByIndex(int index)
    {
        int count = (textures == null) ? -1 : textures.Count;
        if (textures == null || index < 0 || index >= count)
        {
            Debug.LogWarning($"[SandSwitch] Invalid index: {index}. Textures.Count={count}");
            return;
        }
        if (_fade != null) StopCoroutine(_fade);
        _fade = StartCoroutine(SwapRoutine(index));
    }

    IEnumerator SwapRoutine(int nextIndex)
    {
        // 1) 알파 ↓
        float fromA = _mat.GetFloat(ID_OverlayAlpha);
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            _mat.SetFloat(ID_OverlayAlpha, Mathf.Lerp(fromA, 0f, t / fadeTime));
            yield return null;
        }
        _mat.SetFloat(ID_OverlayAlpha, 0f);

        // 2) 텍스처 교체
        var tex = textures[nextIndex];
        _mat.SetTexture(ID_OverlayTex, tex);
        Debug.Log($"[SandSwitch] Overlay switched to {nextIndex} ({tex?.name ?? "null"}) on mat id={_mat.GetInstanceID()}");

        // 3) 알파 ↑
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            _mat.SetFloat(ID_OverlayAlpha, Mathf.Lerp(0f, 1f, t / fadeTime));
            yield return null;
        }
        _mat.SetFloat(ID_OverlayAlpha, .5f);

        _currentIndex = nextIndex;
        _fade = null;
    }

    int GetRandomExcept(int except, int count)
    {
        int r; do { r = Random.Range(0, count); } while (r == except);
        return r;
    }
}
