using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartStoryScript : MonoBehaviour
{
    float waitTime = 0f;    // �f�o�b�O�p �ҋ@����
    public GameData gameData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        waitTime += Time.deltaTime;

        if (waitTime > 3.0f)
        {
            gameData.ShowStartStoryComp(0);
            // gameData.ChangeStartViewTransition();
            gameData.ChangeCutInTransition();
            SceneManager.LoadScene("Stage01");
        }
    }
}
