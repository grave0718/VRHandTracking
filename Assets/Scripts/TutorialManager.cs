using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("튜토리얼 이미지 리스트")]
    [Tooltip("튜토리얼에 사용할 이미지(UI Panel 등) 게임 오브젝트들을 순서대로 연결하세요.")]
    public List<GameObject> TutorialImages;


    [Tooltip("현재 활성화된 튜토리얼 이미지의 인덱스입니다.")]
    public int TutorialIndex { get; private set; } = 0;

    // Start is called before the first frame update
    void Start()
    {
        UpdateTutorialDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        /// <summary>
        /// 현재 TutorialIndex에 해당하는 이미지만 활성화하고 나머지는 비활성화합니다.
        /// </summary>
    }

    void UpdateTutorialDisplay()
    {
        if (TutorialImages == null || TutorialImages.Count == 0)
        {
            Debug.LogWarning(
                "[TutorialManager] 튜토리얼 이미지가 없습니다. TutorialImages 리스트를 채워주세요."
            );
            return;
        }

        // 인덱스 범위 확인
        TutorialIndex = Mathf.Clamp(TutorialIndex, 0, TutorialImages.Count );
        

        for (int i = 0; i < TutorialImages.Count; i++)
        {
            if (TutorialImages[i] != null)
            {
                TutorialImages[i].SetActive(i == TutorialIndex);
            }
        }
        Debug.Log(
            $"[TutorialManager] 현재 튜토리얼 이미지: {TutorialIndex + 1}/{TutorialImages.Count}"
        );
    }

    /// <summary>
    /// 다음 튜토리얼 이미지로 이동합니다.
    /// </summary>
    public void ShowNextTutorial()
    {
        if (TutorialIndex < TutorialImages.Count )
        {
            TutorialIndex++;
            UpdateTutorialDisplay();
        }
    }

    /// <summary>
    /// 이전 튜토리얼 이미지로 이동합니다.
    /// </summary>
    public void ShowPreviousTutorial()
    {
        if (TutorialIndex > 0)
        {
            TutorialIndex--;
            UpdateTutorialDisplay();
        }
    }
}
