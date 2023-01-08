using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.UI;
using UnityEditor;
public class ComicCameraController : MonoBehaviour
{
    /// <summary>
    /// ブラックアウト情報構造体
    /// </summary>
    [System.Serializable]
    class BlackOut
    {
        [SerializeField]
        [Header("暗転(明転)を始めるときのPathPos")]
        float timing;
        [SerializeField]
        [Header("暗転(明転)の速さ")]
        float speed;
        [SerializeField]
        [Header("どこまで暗転(明転)するか")]
        float rate;

        public float Timing() { return timing; }
        public float Speed() { return speed; }
        public float Rate() { return rate; }
    }
    /// <summary>
    /// 透明化情報構造体
    /// </summary>
    [System.Serializable]
    class ChangeImage
    {
        [Header("透明化する画像")]
        public GameObject image;
        [Header("透明化を始めるときのPathPos")]
        [SerializeField] float timing;
        [Header("透明化の速さ")]
        [SerializeField] float speed;

        [System.NonSerialized]
        public Renderer renderer;

        public float Timing() { return timing; }
        public float Speed() { return speed; }
    }

    [SerializeField] CinemachineVirtualCamera vcam;
    [SerializeField] GameData gameData;

    [Header("何回目の漫画ですか？(0 Start)")]
    [SerializeField] int comicNum;
    [Header("この漫画が終わったときに移動するシーン名")]
    [SerializeField] string nextSceneName;

    [SerializeField] float comicBGMStartPos = 0f;

    [SerializeField] ChangeImage[] changeImage;
    [SerializeField] BlackOut[] blackOuts;

    [Header("基本のカメラ移動スピード")]
    [SerializeField] float defaultIncreaseSpeed = 1;
    [Header("X = startpos, Y = endpos, Z = intervalSpeed")] 
    [SerializeField] Vector3[] pathInterval = new Vector3[1];
    [Header("終了時のPathPosition")]
    [SerializeField]float endPos = 14f;
    [Header("カメラのローテーションをOFFにするタイミング")]
    [SerializeField]float rotationChangeTiming;

    CinemachineTrackedDolly dolly;
    GameObject canvas;
    Image blackScreen;
    float position, intervalIncreaseSpeed, timeCount, waitAfterComic = 3f;
    
    int changingNum, blackOutNum;

    bool isPlayingBGM;

    public float GetPathPosition() { return position; }
    // Start is called before the first frame update
    void Start()
    {
        gameData.EditorStart();

        timeCount = 0;
        blackOutNum = 0;
        position = 0;
        intervalIncreaseSpeed = 0;
        changingNum = 0;
        dolly = vcam.GetCinemachineComponent<CinemachineTrackedDolly>();

        blackScreen = GameObject.Find("BlackScreen").GetComponent<Image>();

        for (int i = 0; i < changeImage.Length; i++)
            changeImage[i].renderer = changeImage[i].image.GetComponent<Renderer>();

        canvas = GameObject.Find("PressBtoStart");
        canvas.SetActive(false);

        isPlayingBGM = false;
    }

    // Update is called once per frame
    void Update()
    {
     
        //パスポジションの増加割合変更
        bool isOverWrite = false;
        for(int i = 0; i < pathInterval.Length; i++)
            if (pathInterval[i].x <= position && pathInterval[i].y > position)
            {
                intervalIncreaseSpeed = pathInterval[i].z;
                isOverWrite = true;
            }
        
        if (!isOverWrite)
            intervalIncreaseSpeed = 0;

        position += (defaultIncreaseSpeed + intervalIncreaseSpeed) * Time.deltaTime;
        dolly.m_PathPosition = position;

        //画像切り替え
        if (changingNum < changeImage.Length)
            if (changeImage[changingNum].Timing() < position)
            {

                changeImage[changingNum].renderer.material.color -= new Color(0, 0, 0, changeImage[changingNum].Speed() * Time.deltaTime);
                if (changeImage[changingNum].renderer.material.color.a <= 0)
                    changingNum++;

            }

        //ブラックスクリーン調整
        if (blackOutNum < blackOuts.Length)
            if (position > blackOuts[blackOutNum].Timing())
            {
                blackScreen.color += new Color(0, 0, 0, blackOuts[blackOutNum].Speed() * Time.deltaTime);
                if ((blackOuts[blackOutNum].Rate() <= blackScreen.color.a && blackOuts[blackOutNum].Speed() > 0) || (blackOuts[blackOutNum].Rate() >= blackScreen.color.a && blackOuts[blackOutNum].Speed() < 0))
                    blackOutNum++;
            }

        //カメラのLookAt対象をなくす
        if(position > rotationChangeTiming)
        {
            vcam.LookAt = null;
            vcam.transform.eulerAngles = new Vector3(0, 180, 0);
        }

        //BGM再生処理
        if(position >= comicBGMStartPos && !isPlayingBGM)
        {
            //最後
            if(comicNum == 4)
            {
                Sound.LoadBGM("EndingBGM", "EndingBGM");
                Sound.PlayBGM("EndingBGM", 0.3f);
            }
            else
            {
                Sound.LoadBGM("ComicBGM", "ComicBGM");
                Sound.PlayBGM("ComicBGM", 0.3f);
            }

            isPlayingBGM = true;
        }

        
        if(position >= endPos - 1f)
        {
            Sound.VolumeDownBGM(Time.deltaTime, 8f);
        }

        //終了処理
        if(position >= endPos)
        {
            timeCount += Time.deltaTime;

            if (timeCount >= waitAfterComic)
            {
                if (gameData.showStartStory[comicNum])
                {
                    gameData.MeinMenuTransition();
                    SceneManager.LoadScene("TitleScene");
                    Sound.StopBGM();
                }
                else
                {
                    gameData.ShowStartStoryComp(comicNum);
                    gameData.ChangeCutInTransition();
                    SceneManager.LoadScene(nextSceneName);
                    Sound.StopBGM();
                }
            }
        }
    }


}
