using UnityEngine;
using UnityEngine.UI;
using System; // DateTime을 위해 필요
using System.IO;

public class RenderTextureToPNG : MonoBehaviour
{
    [Header("UI")]
    public Button saveButton;

    [Header("저장 옵션")]
    public string folderName = "SavedRenders";
    public string baseFileName = "SnowRT";
    public bool addTimestamp = true;

    [Header("RT 소스 (비워도 자동 탐색)")]
    public RenderTexture directRT;
    public SnowPathDrawer pathDrawer;
    public SnowController snowController;

    // [ ----- 여기를 추가했습니다 (에러 1번 해결) ----- ]
    [Space(10)]
    [Header("출력 (읽기 전용)")]
    [Tooltip("마지막으로 저장된 파일의 전체 경로")]
    public string lastSavedFilePath;
    // [ -------------------------------------------- ]


    void Awake()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveNow);
        else
            Debug.LogWarning("[SnowRTSaveUI] saveButton이 지정되지 않았습니다. 인스펙터에서 연결하세요.");
    }

    public void SaveNow()
    {
        var rt = GetCurrentRT();
        if (rt == null)
        {
            Debug.LogWarning("[SnowRTSaveUI] 사용할 RenderTexture를 찾지 못했습니다.");
            return;
        }

        string folderPath = Path.Combine(Application.dataPath, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileName = addTimestamp
            ? $"{baseFileName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png"
            : $"{baseFileName}.png";

        string filePath = Path.Combine(folderPath, fileName);
        SaveRenderTextureToPNG(rt, filePath);
        Debug.Log($"[SnowRTSaveUI] 저장 완료: {filePath}");

        // [ ----- 여기를 추가했습니다 (에러 1번 해결) ----- ]
        // 방금 저장한 경로를 public 변수에 저장
        lastSavedFilePath = filePath;
        // [ -------------------------------------------- ]
    }

    RenderTexture GetCurrentRT()
    {
        if (directRT != null) return directRT;
        if (pathDrawer == null) pathDrawer = FindObjectOfType<SnowPathDrawer>();
        if (pathDrawer != null && pathDrawer.snowRT != null) return pathDrawer.snowRT;
        if (snowController == null) snowController = FindObjectOfType<SnowController>();
        if (snowController != null && snowController.snowRT != null) return snowController.snowRT;
        var grounds = GameObject.FindGameObjectsWithTag("SnowGround");
        foreach (var g in grounds) {
            var sc = g.GetComponent<SnowController>();
            if (sc != null && sc.snowRT != null) {
                snowController = sc;
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