using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video; // VideoPlayer를 사용하기 위해 필요
using UnityEngine.SceneManagement; // Scene 전환을 위해 필요

// 이 스크립트가 붙은 게임 오브젝트에 VideoPlayer 컴포넌트가 없으면 자동으로 추가합니다.
[RequireComponent(typeof(VideoPlayer))]
public class SceneMove : MonoBehaviour
{
    [Header("씬 전환 설정")]
    [Tooltip("재생이 끝나면 이동할 씬의 이름")]
    public string sceneNameToLoad;

    private VideoPlayer videoPlayer;

    void Awake()
    {
        // VideoPlayer 컴포넌트를 가져옵니다.
        videoPlayer = GetComponent<VideoPlayer>();

        // 씬 이름이 비어있는지 확인합니다.
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogError("[SceneMove] 'sceneNameToLoad'에 이동할 씬 이름이 지정되지 않았습니다!");
        }
    }

    void OnEnable()
    {
        // 비디오 재생이 끝났을 때 호출될 함수(OnVideoEnd)를 이벤트에 등록합니다.
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnDisable()
    {
        // 스크립트가 비활성화될 때 이벤트 등록을 해제합니다. (메모리 누수 방지)
        videoPlayer.loopPointReached -= OnVideoEnd;
    }

    /// <summary>
    /// VideoPlayer의 loopPointReached 이벤트가 발생하면 호출되는 함수입니다.
    /// </summary>
    /// <param name="vp">이벤트를 발생시킨 VideoPlayer</param>
    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("비디오 재생이 완료되었습니다. 씬을 전환합니다: " + sceneNameToLoad);

        // 지정된 씬으로 전환합니다.
        if (!string.IsNullOrEmpty(sceneNameToLoad))
        {
            SceneManager.LoadScene(sceneNameToLoad);
        }
    }
}
