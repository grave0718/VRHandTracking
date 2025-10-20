using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SandOverlayToggle : MonoBehaviour
{
    [Header("Target")]
    public Material sandMat;           // 모래 머티리얼(위 쉐이더가 들어있는 머티리얼)
    public Texture2D sourceTexture;    // 원본 컬러 이미지(에셋/런타임 로드)

    [Header("Optional")]
    public bool convertOnCPU = false;  // true면 CPU에서 미리 흑백으로 변환해 넣음(성능 최소화용)
    public Button uiButton;            // UI 버튼(월드/스크린 캔버스 모두 가능)
    public float fadeTime = 0.25f;     // 부드럽게 켜고 끄기

    Texture2D _runtimeGrayscale;

    void Awake()
    {
        if (sandMat == null)
        {
            Debug.LogError("[SandOverlayToggle] sandMat not assigned.");
            enabled = false;
            return;
        }

        if (sourceTexture == null)
        {
            Debug.LogWarning("[SandOverlayToggle] sourceTexture not assigned. Overlay will be empty.");
        }
        else
        {
            if (convertOnCPU)
            {
                _runtimeGrayscale = MakeGrayscaleCopy(sourceTexture);
                sandMat.SetTexture("_OverlayTex", _runtimeGrayscale);
            }
            else
            {
                // 쉐이더에서 그레이스케일 처리 → 컬러 원본 바로 사용
                sandMat.SetTexture("_OverlayTex", sourceTexture);
            }
        }

        // 초기 비활성
        sandMat.SetFloat("_OverlayEnabled", 0f);
        sandMat.SetFloat("_OverlayAlpha", 0f);

        if (uiButton != null)
            uiButton.onClick.AddListener(ToggleOverlay);
    }

    void Update()
    {
        // 키보드 S로도 테스트
        if (Input.GetKeyDown(KeyCode.S))
            ToggleOverlay();
    }

    public void ToggleOverlay()
    {
        float enabledNow = sandMat.GetFloat("_OverlayEnabled");
        bool turnOn = enabledNow < 0.5f;

        sandMat.SetFloat("_OverlayEnabled", turnOn ? 1f : 0f);
        StopAllCoroutines();
        StartCoroutine(FadeOverlay(turnOn ? 0f : 1f, turnOn ? 1f : 0f, fadeTime));

        Debug.Log($"[SandOverlayToggle] Overlay {(turnOn ? "ON" : "OFF")}");
    }

    IEnumerator FadeOverlay(float from, float to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / time);
            sandMat.SetFloat("_OverlayAlpha", a);
            yield return null;
        }
        sandMat.SetFloat("_OverlayAlpha", to);
    }

    // CPU로 그레이스케일 복사본 만들기(선택)
    Texture2D MakeGrayscaleCopy(Texture2D src)
    {
        var readable = GetReadableCopy(src);
        var cols = readable.GetPixels32();
        for (int i = 0; i < cols.Length; i++)
        {
            var c = cols[i];
            // ITU-R BT.601 가중치
            byte g = (byte)Mathf.Clamp(Mathf.RoundToInt(c.r * 0.299f + c.g * 0.587f + c.b * 0.114f), 0, 255);
            cols[i] = new Color32(g, g, g, c.a);
        }
        var tex = new Texture2D(readable.width, readable.height, TextureFormat.RGBA32, false);
        tex.SetPixels32(cols);
        tex.Apply(false, false);
        return tex;
    }

    // 읽기 불가 텍스처 대비 안전 복사본
    Texture2D GetReadableCopy(Texture2D src)
    {
        RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(src, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0,0,src.width,src.height), 0, 0);
        copy.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return copy;
    }

    // 필요 시 외부에서 타일/오프셋 제어
    public void SetOverlayTilingOffset(Vector2 tiling, Vector2 offset)
    {
        sandMat.SetVector("_OverlayTiling", new Vector4(tiling.x, tiling.y, offset.x, offset.y));
    }
}
