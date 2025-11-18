using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartTuto : MonoBehaviour
{


    public TutorialManager tutorialManager;
        public GameObject tutorialNextButton;

            public string handTag = "PlayerHand";
    public GameObject tutorialEndButton;
    // Start is called before the first frame update
    void Start()
    {
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
        var nextIndex = tutorialManager.TutorialIndex;

    }




private void OnTriggerEnter(Collider other){

    if (other.CompareTag(handTag) && tutorialManager != null){


        tutorialManager.ShowPreviousTutorial();
    }
}
}
