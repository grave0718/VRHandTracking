using UnityEngine;
using System.IO;        
using System.Collections; 
using System; 

[RequireComponent(typeof(Collider))]
public class LoadSavedImageTrigger : MonoBehaviour
{
    [Header("1. 연결")]
    public SnowRTSaveUI snowSaveManager;
    public Renderer completeImageRenderer;

    [Header("2. 터치 감지")]
    public string handTag = "PlayerHand";
    public float cooldownTime = 1.0f; 

    private bool _isReady = true;
    private Texture2D _loadedTexture; 

    void Start()
    {
        if (snowSaveManager == null)
            Debug.LogError($"[LoadImageTrigger] {gameObject.name}: 'snowSaveManager'가 연결되지 않았습니다!");
        
        if (completeImageRenderer == null)
        {
            Debug.LogError($"[LoadImageTrigger] {gameObject.name}: 'completeImageRenderer'가 연결되지 않았습니다!");
        }
        else
        {
            completeImageRenderer.gameObject.SetActive(false);
        }
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isReady && other.CompareTag(handTag))
        {
            if (snowSaveManager == null || completeImageRenderer == null) return;
            Debug.Log("이미지 불러오기 트리거 작동!");
            StartCoroutine(LoadAndApplyImageRoutine());
        }
    }

    // [ ----- 이 함수 전체를 수정했습니다 (CS1625 에러 해결) ----- ]
    private IEnumerator LoadAndApplyImageRoutine()
    {
        _isReady = false; // 쿨다운 즉시 시작

        try
        {
            string filePath = snowSaveManager.lastSavedFilePath; 

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Debug.LogWarning($"[LoadImageTrigger] 파일 경로가 유효하지 않습니다: {filePath}");
            }
            else
            {
                // 파일 경로가 유효할 때만 로직 실행
                byte[] fileData = File.ReadAllBytes(filePath);

                if (_loadedTexture != null)
                {
                    Destroy(_loadedTexture);
                }
                
                _loadedTexture = new Texture2D(2, 2); 
                
                if (_loadedTexture.LoadImage(fileData)) 
                {
                    Debug.Log($"[LoadImageTrigger] {completeImageRenderer.gameObject.name}의 머티리얼에 텍스처 적용!");
                    completeImageRenderer.material.mainTexture = _loadedTexture;
                    completeImageRenderer.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("[LoadImageTrigger] PNG 데이터로 텍스처를 로드하는데 실패했습니다.");
                    Destroy(_loadedTexture); 
                }
            }
        }
        catch (Exception e) 
        {
            Debug.LogError($"[LoadImageTrigger] 파일 읽기 오류: {e.Message}");
            // 오류가 발생해도 쿨다운은 아래에서 실행됨
        }

        // 'try'가 성공하든 'catch'가 실행되든
        // 코루틴의 마지막에 'finally' 대신 쿨다운을 실행합니다.
        yield return new WaitForSeconds(cooldownTime);
        _isReady = true;
    }
    // [ ---------------------------------------------------- ]

    void OnDestroy()
    {
        if (_loadedTexture != null)
        {
            Destroy(_loadedTexture);
        }
    }
}