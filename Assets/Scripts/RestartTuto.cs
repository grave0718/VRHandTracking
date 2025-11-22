using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartTuto : MonoBehaviour
{
    public TutorialManager tutorialManager;
    public GameObject tutorialNextButton;

    public string handTag = "PlayerHand";
    public GameObject tutorialEndButton;

    [Header("텍스처 변경 설정")]
    [Tooltip("인덱스가 7일 때 적용할 텍스처")]
    public Texture textureForIndex7;

    [Tooltip("평상시(인덱스 7이 아닐 때) 적용할 기본 텍스처")]
    public Texture defaultTexture;

    private Renderer objectRenderer;
    // Start is called before the first frame update
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError("[RestartTuto] Renderer 컴포넌트를 찾을 수 없습니다!");
            enabled = false; // 렌더러가 없으면 스크립트 비활성화
            return;
        }

        // 기본 텍스처 설정
        if (defaultTexture != null)
        {
            objectRenderer.material.mainTexture = defaultTexture;
        }
        if (tutorialManager == null)
        {
            tutorialManager = FindObjectOfType<TutorialManager>();
            if (tutorialManager == null)
            {
                Debug.LogError(
                    $"[TutorialNext] {gameObject.name}에 'tutorialManager'가 연결되지 않았습니다!"
                );
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (tutorialManager == null || objectRenderer == null)
            return;

        var currentIndex = tutorialManager.TutorialIndex;

        if(currentIndex == 7){
            gameObject.SetActive(false);
        }
        // 인덱스 값에 따라 텍스처 변경
        if (currentIndex >7)
        {
            gameObject.SetActive(true);
            // 인덱스가 7일 때, 그리고 현재 텍스처가 변경할 텍스처와 다를 때만 변경
            if (textureForIndex7 != null && objectRenderer.material.mainTexture != textureForIndex7)
            {
                objectRenderer.material.mainTexture = textureForIndex7;
            }
        }
        else
        {
            // 인덱스가 7이 아닐 때, 그리고 현재 텍스처가 기본 텍스처와 다를 때만 변경
            if (defaultTexture != null && objectRenderer.material.mainTexture != defaultTexture)
            {
                objectRenderer.material.mainTexture = defaultTexture;
            }
        }



        if (currentIndex >= tutorialManager.TutorialImages.Count)
            {
                gameObject.SetActive(false);
                // GameEnd 버튼 활성화
            }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(handTag) && tutorialManager != null)
        {
            tutorialManager.ShowPreviousTutorial();
        }
    }
}
