using UnityEngine;
using UnityEngine.UI;   // UI Button
using System;
using System.IO;

public class SnowRTSaveUI : MonoBehaviour
{
    [Header("UI")]
    public Button saveButton;              // 버튼 연결

    [Header("저장 옵션")]
    public string folderName = "SavedRenders";   // Assets/ 아래 폴더
    public string baseFileName = "SnowRT";       // 파일 베이스명
    public bool addTimestamp = true;             // 타임스탬프 추가 여부

    [Header("RT 소스 (비워도 자동 탐색)")]
    public RenderTexture directRT;         // 수동 지정시 이걸 우선 사용
    public SnowPathDrawer pathDrawer;      // 없으면 자동 Find
    public SnowController snowController;  // 없으면 자동 Find

    void Awake()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveNow);
        else
            Debug.LogWarning("[SnowRTSaveUI] saveButton이 지정되지 않았습니다. 인스펙터에서 연결하세요.");
    }

    // 버튼 OnClick에 연결되는 메서드
    public void SaveNow()
    {
        var rt = GetCurrentRT();
        if (rt == null)
        {
            Debug.LogWarning("[SnowRTSaveUI] 사용할 RenderTexture를 찾지 못했습니다. (directRT / pathDrawer.snowRT / snowController.snowRT)");
            return;
        }

        string folderPath = Path.Combine(Application.dataPath, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"[SnowRTSaveUI] 폴더 생성: {folderPath}");
        }

        string fileName = addTimestamp
            ? $"{baseFileName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png"
            : $"{baseFileName}.png";

        string filePath = Path.Combine(folderPath, fileName);
        SaveRenderTextureToPNG(rt, filePath);
        Debug.Log($"[SnowRTSaveUI] 저장 완료: {filePath}");
    }

    RenderTexture GetCurrentRT()
    {
        // 1) 직접 지정
        if (directRT != null)
        {
            Debug.Log("[RT] directRT 사용");
            return directRT;
        }

        // 2) SnowPathDrawer에서
        if (pathDrawer == null) pathDrawer = FindObjectOfType<SnowPathDrawer>();
        if (pathDrawer != null && pathDrawer.snowRT != null)
        {
            Debug.Log("[RT] pathDrawer.snowRT 사용");
            return pathDrawer.snowRT;
        }

        // 3) SnowController에서
        if (snowController == null) snowController = FindObjectOfType<SnowController>();
        if (snowController != null && snowController.snowRT != null)
        {
            Debug.Log("[RT] snowController.snowRT 사용");
            return snowController.snowRT;
        }

        // 4) 태그 기반(프로젝트에서 SnowGround 태그를 쓴다면)
        var grounds = GameObject.FindGameObjectsWithTag("SnowGround");
        foreach (var g in grounds)
        {
            var sc = g.GetComponent<SnowController>();
            if (sc != null && sc.snowRT != null)
            {
                snowController = sc;
                Debug.Log("[RT] SnowGround 태그로 탐색 → snowController.snowRT 사용");
                return sc.snowRT;
            }
        }

        return null;
    }

    void SaveRenderTextureToPNG(RenderTexture rt, string filePath)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);

        RenderTexture.active = prev;
        Destroy(tex);
    }
}
