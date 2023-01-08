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
    /// �u���b�N�A�E�g���\����
    /// </summary>
    [System.Serializable]
    class BlackOut
    {
        [SerializeField]
        [Header("�Ó](���])���n�߂�Ƃ���PathPos")]
        float timing;
        [SerializeField]
        [Header("�Ó](���])�̑���")]
        float speed;
        [SerializeField]
        [Header("�ǂ��܂ňÓ](���])���邩")]
        float rate;

        public float Timing() { return timing; }
        public float Speed() { return speed; }
        public float Rate() { return rate; }
    }
    /// <summary>
    /// ���������\����
    /// </summary>
    [System.Serializable]
    class ChangeImage
    {
        [Header("����������摜")]
        public GameObject image;
        [Header("���������n�߂�Ƃ���PathPos")]
        [SerializeField] float timing;
        [Header("�������̑���")]
        [SerializeField] float speed;

        [System.NonSerialized]
        public Renderer renderer;

        public float Timing() { return timing; }
        public float Speed() { return speed; }
    }

    [SerializeField] CinemachineVirtualCamera vcam;
    [SerializeField] GameData gameData;

    [Header("����ڂ̖���ł����H(0 Start)")]
    [SerializeField] int comicNum;
    [Header("���̖��悪�I������Ƃ��Ɉړ�����V�[����")]
    [SerializeField] string nextSceneName;

    [SerializeField] float comicBGMStartPos = 0f;

    [SerializeField] ChangeImage[] changeImage;
    [SerializeField] BlackOut[] blackOuts;

    [Header("��{�̃J�����ړ��X�s�[�h")]
    [SerializeField] float defaultIncreaseSpeed = 1;
    [Header("X = startpos, Y = endpos, Z = intervalSpeed")] 
    [SerializeField] Vector3[] pathInterval = new Vector3[1];
    [Header("�I������PathPosition")]
    [SerializeField]float endPos = 14f;
    [Header("�J�����̃��[�e�[�V������OFF�ɂ���^�C�~���O")]
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
     
        //�p�X�|�W�V�����̑��������ύX
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

        //�摜�؂�ւ�
        if (changingNum < changeImage.Length)
            if (changeImage[changingNum].Timing() < position)
            {

                changeImage[changingNum].renderer.material.color -= new Color(0, 0, 0, changeImage[changingNum].Speed() * Time.deltaTime);
                if (changeImage[changingNum].renderer.material.color.a <= 0)
                    changingNum++;

            }

        //�u���b�N�X�N���[������
        if (blackOutNum < blackOuts.Length)
            if (position > blackOuts[blackOutNum].Timing())
            {
                blackScreen.color += new Color(0, 0, 0, blackOuts[blackOutNum].Speed() * Time.deltaTime);
                if ((blackOuts[blackOutNum].Rate() <= blackScreen.color.a && blackOuts[blackOutNum].Speed() > 0) || (blackOuts[blackOutNum].Rate() >= blackScreen.color.a && blackOuts[blackOutNum].Speed() < 0))
                    blackOutNum++;
            }

        //�J������LookAt�Ώۂ��Ȃ���
        if(position > rotationChangeTiming)
        {
            vcam.LookAt = null;
            vcam.transform.eulerAngles = new Vector3(0, 180, 0);
        }

        //BGM�Đ�����
        if(position >= comicBGMStartPos && !isPlayingBGM)
        {
            //�Ō�
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

        //�I������
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
