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

    public bool convertOnCPU = true;
    public float fadeTime = 0.25f;

    // Shader Graph property names (Blackboard와 반드시 동일)
    static readonly int ID_OverlayTex   = Shader.PropertyToID("_OverlayTex");
    static readonly int ID_OverlayAlpha = Shader.PropertyToID("_OverlayAlpha");


 List<Texture2D> _runtimeGrayscaleTextures;
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

        if (textures != null && textures.Count > 0)
        {
            if (convertOnCPU)
            {
                _runtimeGrayscaleTextures = new List<Texture2D>();
                foreach (var tex in textures)
                {
                    _runtimeGrayscaleTextures.Add(MakeGrayscaleCopy(tex));
                }
                Debug.Log($"[SandSwitch] Converted {textures.Count} textures to grayscale on CPU.");
            }
        }
        else
        {
            Debug.LogWarning("[SandSwitch] No overlay textures assigned.");
        }


        // 초기 알파 0
        // _mat.SetFloat(ID_OverlayAlpha, 0f);

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
        // _fade = StartCoroutine(SwapRoutine(index));
        SetTextureOnly(index);
    }

    void SetTextureOnly(int nextIndex)
    {
        var tex = (convertOnCPU && _runtimeGrayscaleTextures != null && nextIndex < _runtimeGrayscaleTextures.Count)
            ? _runtimeGrayscaleTextures[nextIndex]
            : textures[nextIndex];
        _mat.SetTexture(ID_OverlayTex, tex);
        Debug.Log($"[SandSwitch] Overlay switched to {nextIndex} ({tex?.name ?? "null"}) on mat id={_mat.GetInstanceID()}");
        _currentIndex = nextIndex;
    }

        public void ResetOverlay()
    {
        // if (_mat != null)
        // {
            _mat.SetTexture(ID_OverlayTex, null);
            _currentIndex = -1; // 현재 인덱스도 초기화
            Debug.Log($"[SandSwitch] Overlay texture has been reset on mat id={_mat.GetInstanceID()}");
        // }
    }

  IEnumerator SwapRoutine(int nextIndex)
    {
        // 1) 알파 ↓ (기존과 동일)
        float fromA = _mat.GetFloat(ID_OverlayAlpha);
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            _mat.SetFloat(ID_OverlayAlpha, Mathf.Lerp(fromA, 0f, t / fadeTime));
            yield return null;
        }
        _mat.SetFloat(ID_OverlayAlpha, 0f);

        // 2) 텍스처 교체 (기존과 동일)
        var tex = textures[nextIndex];
        _mat.SetTexture(ID_OverlayTex, tex);
        Debug.Log($"[SandSwitch] Overlay switched to {nextIndex} ({tex?.name ?? "null"}) on mat id={_mat.GetInstanceID()}");

        // 3) 알파 ↑ (수정된 부분)
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        
        float targetAlpha = 0.38f; // <<< 원하시는 최종 알파값 (원본 머티리얼과 동일하게)

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            // 0f에서 1f가 아닌, targetAlpha까지만 자연스럽게 올라가도록 수정
            _mat.SetFloat(ID_OverlayAlpha, Mathf.Lerp(0f, targetAlpha, t / fadeTime));
            yield return null;
        }
        
        // 페이드가 끝난 후 최종 알파값으로 정확히 고정
        _mat.SetFloat(ID_OverlayAlpha, targetAlpha); 

        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        _currentIndex = nextIndex;
        _fade = null;
    }

    int GetRandomExcept(int except, int count)
    {
        int r; do { r = Random.Range(0, count); } while (r == except);
        return r;
    }

#region Grayscale Conversion
    Texture2D MakeGrayscaleCopy(Texture2D src)
    {
        if (src == null) return null;
        var readable = GetReadableCopy(src);
        var cols = readable.GetPixels32();
        for (int i = 0; i < cols.Length; i++)
        {
            var c = cols[i];
            // ITU-R BT.601 가중치
            byte g = (byte)Mathf.Clamp(Mathf.RoundToInt(c.r * 0.299f + c.g * 0.587f + c.b * 0.114f), 0, 255);
            cols[i] = new Color32(g, g, g, c.a);
        }
        var tex = new Texture2D(readable.width, readable.height, TextureFormat.RGBA32, false, true);
        tex.name = src.name + "_Grayscale";
        tex.SetPixels32(cols);
        tex.Apply(false, false);

        // GetReadableCopy()가 임시 복사본을 만들었다면 파괴하여 메모리 누수 방지
        if (!ReferenceEquals(src, readable))
        {
            Destroy(readable);
        }

        return tex;
    }

    // 읽기 불가 텍스처 대비 안전 복사본
    Texture2D GetReadableCopy(Texture2D src)
    {
        if (src.isReadable) return src;

        RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(src, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D readableText = new Texture2D(src.width, src.height);
        readableText.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return readableText;
    }
    #endregion
}

