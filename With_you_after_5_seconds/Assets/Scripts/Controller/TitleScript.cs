using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI.Extensions;
using System;

public class TitleScript : UI_Effect
{
    #region SERIALIZE FIELD MEMBER
    [Header("�^�C�g���Ɩڎ�")]
    [SerializeField] [Tooltip("�^�C�g���Ɩڎ��̉��")] private GameObject titleAndMenuScreen;
    [SerializeField] [Tooltip("�^�C�g����UI���܂Ƃ߂Ă�e�I�u�W�F�N�g")] private GameObject titleUIs;
    [SerializeField] [Tooltip("���C�����j���[��UI���܂Ƃ߂Ă�e�I�u�W�F�N�g")] private GameObject mainMenuUIs;
    [SerializeField] [Tooltip("�u�{�^���������Ă��������v�̓_�ł�����Image")] private Image pressAnyButton;
    [SerializeField] [Tooltip("�Â�����{�^��")] private Button continueButton;
    [SerializeField] [Tooltip("�V�[���J�n���̃t�F�[�h�C���Ɍ����鎞��")] private float startFadeSec = 3.0f;
    [SerializeField] [Tooltip("�؂�ւ��ɂ����鎞��")] private float fadeSec = 1.0f;
    [SerializeField] [Tooltip("�؂�ւ��ɂ����鎞��")] private float flashingRepetTime = 2.0f;

    [Space(3)]
    [Header("�͂��߂���")]
    [SerializeField] [Tooltip("���Z�b�g���")] private GameObject resetScreen;
    [SerializeField] [Tooltip("���Z�b�g�m�F���")] private GameObject resetConfirmation;
    [SerializeField] [Tooltip("���Z�b�g�m����")] private GameObject toReset;
    [SerializeField] [Tooltip("���Z�b�g�m�F��ʂ́u�������v�{�^��")] private Button noButton;

    [Space(3)]
    [Header("�X�e�[�W�Z���N�g")]
    [SerializeField] [Tooltip("�X�e�[�W�Z���N�g��ʂ̐e")] private GameObject stageSelectScreens;
    [SerializeField] [Tooltip("�X�e�[�W�Z���N�g��ʂ̊e�y�[�W")] private GameObject[] stageSelectPageScreen;
    [SerializeField] [Tooltip("�e�X�e�[�W�Z���N�g��ʃy�[�W�ɂ���{�^���̐�")] private int[] pageButtonNum;
    [SerializeField] [Tooltip("�X�e�[�W�J�ڂ���{�^��")] private Button[] stageButton;
    [SerializeField] [Tooltip("�X�e�[�W01�̃{�^��")] private Button stage01Button;

    [Space(3)]
    [Header("�����т���")]
    [SerializeField] [Tooltip("�����т�����ʂ̐e")] private GameObject howToPlayScreen;
    [SerializeField] [Tooltip("�����т�����ʂ̊e�y�[�W")] private GameObject[] howToPlayPageScreen;
    [SerializeField] [Tooltip("�L�����Z���p�̃{�^��")] private Button cancelButton;

    [Space(3)]
    [Header("�`�[�g���")]
    [SerializeField] [Tooltip("�N���A�`�[�g�g�p�\�����")] private GameObject cheatOnImage;

    [Space(3)]
    [Header("���̑�")]
    [SerializeField] [Tooltip("�C�x���g�V�X�e��")] private GameObject blackFadePanel;
    [SerializeField] [Tooltip("�C�x���g�V�X�e��")] private EventSystem eventSystem;
    #endregion


    #region PUBLIC MEMBER
    public GameData gameData;
    #endregion


    #region PRIVATE MEMBER
    StageSelectPage[] stageSelectPage;
    GameObject selectedButton;

    GameObject nowActiveScreen;
    Button selectButton;
    FadeState fadeState = FadeState.Brack;
    float goTime; // �}���K�V�[���ɑJ�ڂ���܂ł̌o�ߎ���
    bool nowEffecting = false;
    bool resetScreenFlag = false; // true: ResetScreen�\���@false: ResetScreen��\��
    bool goNextScene = false;
    bool goShowStartStory = false; // �}���K�V�[���ɑJ�ڂ��邩
    bool stayScene = false; // �f�o�b�O�p�A�͂��߂���Ń^�C�g���V�[�����ēǂݍ��݂��邩�ǂ����������t���O
    bool goSampleSceen = false; // �f�o�b�O�p�A�X�e�[�W�I���ŃT���v���V�[���ɍs����悤�ɂ���B

    BookUI _BookUI;
    #endregion


    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        gameData.Start();�@// ���[�J���X�g���[�W�Ɋւ���X�^�[�g����

        gameData.StartData(); // �f�[�^�̓Ǎ��E����

        StageDataStorage(); // �X�e�[�W�Z���N�g��ʂ̃f�[�^���N���X�Ɋi�[
        
        ActiveScreenCheck();// �|�[�Y��ʁE�N���A��ʂ���X�e�[�W�Z���N�g�֑J�ڂ�������UI����

        SoundSet();

        _BookUI = stageSelectScreens.GetComponent<BookUI>();
    }

    // Update is called once per frame
    void Update()
    {
        gameData.Update();�@// ���[�J���X�g���[�W�Ɋւ���A�b�v�f�[�g����

        // �^�C�g����ʂ̃{�^������
        if (gameData.OutGameState == OutGame.Title && Input.anyKey)
        {
            PressAnyKey();
        }

        // �}���K�V�[���Ƀt�F�[�h���Ȃ���J��
        if (goShowStartStory)
        {
            GoShowStartStory();
            return;
        }

        // �V�[���J�ڎ��{�^���������󂯕t���Ȃ��悤�ɂ���
        if (goNextScene)
        {
            eventSystem.enabled = false;
            return;
        }

        // �t�F�[�h�G�t�F�N�g�g�p���邩����
        FadeSelect();

        if (gameData.OutGameState != OutGame.Title && !nowEffecting)
        {
            // ���I�����Ă�����̂� First Selected �ɐ����㏑������
            // �I����Ԃ���������Ă��܂������ɍēx�I����Ԃ��擾�ł���悤�ɂ���
            ReadCurrentSelectedButton();
            ReferenceSelectedButton();
        }

        // �e��ʎ��̃A�b�v�f�[�g����
        if (gameData.OutGameState == OutGame.Title)
        {
            FlashingUI(ref pressAnyButton, flashingRepetTime);
        }
        else if (gameData.OutGameState == OutGame.HowToPlay)
        {   // �����т����̉�ʂ������ꍇ��ɃL�����Z���ł���悤�ɂ��Ă���
            cancelButton.Select();
        }


        // �`�[�g�őS�X�e�[�W���
        if (Input.GetKeyDown(KeyCode.C))
        {
            gameData.AllClearData();
            cheatOnImage.SetActive(true);
            StartCoroutine("SceneReset");
        }

        // R ����������N���A����������
        if (Input.GetKeyDown(KeyCode.R))
        {
            stayScene = !stayScene;
            Debug.Log("�͂��߂����������^�C�g���V�[���ēǂݍ��� (" + stayScene + ")\n"
                       + "true�Ń^�C�g���ēǂݍ��݁Afalse�Œʏ�ʂ�V�[���J��");
        }

        // F1 ����������ǂ̃X�e�[�W��I��ł��T���v���V�[���֑J��
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (!goSampleSceen)
            {
                goSampleSceen = true;
                Debug.Log("�X�e�[�W�I��������ƃT���v���V�[�����N������܂��B");
            }
            else
            {
                goSampleSceen = false;
                Debug.Log("�X�e�[�W�I���Œʏ�ʂ�J�ڂ��܂��B");
            }
        }
    }
    #endregion


    #region Coroutine
    // �`�[�g�őS�X�e�[�W���������A�R���[�`���� 2 �b����
    // �e�L�X�g���A�N�e�B�u����TitelScene���ĂтȂ���
    IEnumerator SceneReset()
    {
        yield return new WaitForSeconds(2.0f);
        gameData.MeinMenuTransition();
        SceneManager.LoadScene("TitleScene");
    }

    // �͂��߂���ɂ����ۂɃV�[���J�ڂ���܂ŏ����҂�
    // ���o����������΂Ȃ�������
    IEnumerator ResetWaitTime()
    {
        yield return new WaitForSeconds(2.0f);
        gameData.StartStoryTransition();
        goShowStartStory = true;
    }
    #endregion


    #region PRIVATE METHOD
    /// <summary>
    /// �X�e�[�W�Z���N�g��ʂ̃f�[�^���N���X�Ɋi�[
    /// </summary>
    void StageDataStorage()
    {
        stageSelectPage = new StageSelectPage[stageSelectPageScreen.Length];
        int buttonCount = 0;
        for (int i = 0; i < stageSelectPage.Length; i++)
        {
            stageSelectPage[i] = new StageSelectPage();

            // stageSelectScreen �֊i�[
            stageSelectPage[i].stageSelectScreen = stageSelectPageScreen[i];

            // stageButton, buttonActive �֊i�[
            stageSelectPage[i].stageButton = new GameObject[pageButtonNum[i]];
            stageSelectPage[i].buttonActive = new bool[pageButtonNum[i]];
            for (int j = 0; j < pageButtonNum[i]; j++)
            {
                // stageButton�̏���
                if (buttonCount < stageButton.Length)
                {   // �e�y�[�W���ŕ\������{�^���̃f�[�^���i�[
                    stageSelectPage[i].stageButton[j] = stageButton[buttonCount].gameObject;
                }
                else
                {
                    Debug.LogWarning("�X�e�[�W�Z���N�g�̃f�[�^�i�[���A�͈͊O�ɃA�N�Z�X���悤�Ƃ��܂����B");
                }

                // buttonActive�̏���
                if (buttonCount == 0 || buttonCount <= gameData.notClearStageNum)
                {
                    stageSelectPage[i].buttonActive[j] = true;
                }
                else
                {
                    stageSelectPage[i].buttonActive[j] = false;
                }

                buttonCount++;
            }

            // active �֊i�[ (��ԍŏ��̂؁[�W�̂݃A�N�e�B�u�������)
            if (i == 0)
            {
                stageSelectPage[i].stageActive = true;
            }
            else
            {
                stageSelectPage[i].stageActive = false;
            }
        }
    }

    /// <summary>
    /// �|�[�Y��ʁE�N���A��ʂ���X�e�[�W�Z���N�g�֑J�ڂ������̏���
    /// </summary>
    void ActiveScreenCheck()
    {
        if (gameData.OutGameState == OutGame.MeinMenu)
        {
            titleUIs.SetActive(false);
            mainMenuUIs.SetActive(true);
            ShowAlphaUI(ref mainMenuUIs);
            selectButton = continueButton;

            nowActiveScreen = titleAndMenuScreen;
        }
    }

    /// <summary>
    /// �g���T�E���h��ǂݍ���
    /// </summary>
    void SoundSet()
    {
        Sound.LoadBGM("TitleSceneBGM", "TitleSceneBGM");
        Sound.LoadSE("PencilSelect", "ButtonSelect");
        Sound.LoadSE("PencilPush", "ButtonPush");

        Sound.PlayBGM("TitleSceneBGM", 0.05f);
    }

    /// <summary>
    /// ������I����Ԃ̃{�^���������Ȃ��Ă��܂����Ƃ��ɃX�e�B�b�N�����
    /// �I�𒆂̃{�^�����ēx�I����Ԃł���悤�ۊǂ���֐�
    /// �܂��A�����I����Ԃ��������擾���AInputModule���삪�؂�ւ���Ă�����ɑ���ł���悤�ۑ�
    /// </summary>
    void ReadCurrentSelectedButton()
    {
        if (eventSystem.currentSelectedGameObject != null)
        {
            selectedButton = eventSystem.currentSelectedGameObject.gameObject;
            eventSystem.firstSelectedGameObject = selectedButton;
        }
    }

    /// <summary>
    /// �I����Ԃ̃{�^�����Ȃ��Ƃ��ɃX�e�B�b�N����ōēx�I����Ԃɂ���֐�
    /// </summary>
    void ReferenceSelectedButton()
    {
        if (eventSystem.GetComponent<EventSystem>().currentSelectedGameObject == null)
        {
            if (Input.GetAxis(eventSystem.GetComponent<StandaloneInputModule>().horizontalAxis) != 0 ||
                Input.GetAxis(eventSystem.GetComponent<StandaloneInputModule>().verticalAxis) != 0)
            {
                selectedButton.GetComponent<Button>().Select();
            }
        }
    }

    /// <summary>
    /// �t�F�[�h�G�t�F�N�g�g�p���邩����
    /// </summary>
    void FadeSelect()
    {
        if (fadeState == FadeState.Brack)
        {   // �����V�[���N����
            nowEffecting = true;

            FadeOutUI(ref blackFadePanel, startFadeSec);

            if (blackFadePanel.GetComponent<Image>().color.a == 0f)
            {
                fadeState = FadeState.None;
            }

            if (fadeState == FadeState.None && selectButton != null)
            {
                selectButton.Select();
            }
        }
        else if (fadeState == FadeState.Title)
        {   // ��������ʑJ�ڎ�
            nowEffecting = true;

            UIToFade(ref titleUIs, ref mainMenuUIs, ref fadeState, fadeSec);

            if (fadeState == FadeState.None)
            {
                selectButton.Select();
            }
        }
        else if (fadeState == FadeState.StageSelect)
        {   // �������˃X�e�[�W�Z���N�g or �X�e�[�W�Z���N�g�˂�����
            if (!nowEffecting)
            {
                // �K�v�������1�y�[�W�ڂ֖߂��i�����j
                ResetBookScript();
            }

            nowEffecting = true;            

            if (gameData.OutGameState != OutGame.MeinMenu)
            {
                PanelToFade(ref titleAndMenuScreen, ref stageSelectScreens, ref fadeState, titleAndMenuScreen, fadeSec, false);
            }
            else
            {
                PanelToFade(ref stageSelectScreens, ref titleAndMenuScreen, ref fadeState, titleAndMenuScreen, fadeSec, false);
            }

            if (fadeState == FadeState.None)
            {
                selectButton.Select();
            }

            if (!stageSelectScreens.activeInHierarchy)
            {
                StageSelectScreenEffectReset();
            }
        }
        else if (fadeState == FadeState.HowToPlay)
        {   // �������˂����т��� or �����т����˂�����
            nowEffecting = true;

            if (gameData.OutGameState != OutGame.MeinMenu)
            {
                PanelToFade(ref titleAndMenuScreen, ref howToPlayScreen, ref fadeState, titleAndMenuScreen, fadeSec, true);
            }
            else
            {
                PanelToFade(ref howToPlayScreen, ref titleAndMenuScreen, ref fadeState, titleAndMenuScreen, fadeSec, false);
            }

            if (fadeState == FadeState.None)
            {
                selectButton.Select();
            }

            if (!howToPlayScreen.activeInHierarchy)
            {
                HowToPlayEffectReset(ref howToPlayScreen);
            }
        }
        else if (fadeState == FadeState.Reset)
        {   // �������˃��Z�b�g�m�F��� or ���Z�b�g�m�F��ʁ˂�����
            nowEffecting = true;

            if (resetScreenFlag)
            {
                PanelToFade(ref titleAndMenuScreen, ref resetScreen, ref fadeState, titleAndMenuScreen, fadeSec, true);
            }
            else
            {
                PanelToFade(ref resetScreen, ref titleAndMenuScreen, ref fadeState, titleAndMenuScreen, fadeSec, false);
            }

            if (fadeState == FadeState.None)
            {
                selectButton.Select();
            }
        }
        else
        {   // �J�ڂ��Ă��Ȃ����
            nowEffecting = false;
        }
    }

    void GoShowStartStory()
    {
        FadeInUI(ref blackFadePanel, fadeSec);
        Sound.VolumeDownBGM(Time.deltaTime, 0.2f);
        goTime += Time.deltaTime;

        if (goTime >= fadeSec / 2f)
        {
            Sound.BGMAndSEResets();
            gameData.StartStoryTransition();
            SceneManager.LoadScene("Comic01");
        }
    }

    /// <summary>
    /// ����ҁF����
    /// 2�y�[�W�ڂ�I�������܂܁A���̃��j���[��ʂ֑J�����ꍇ�ɁA
    /// 1�y�[�W�ڂ֖߂�����
    /// </summary>
    private void ResetBookScript()
    {
        if (_BookUI.CurrentPage > 1) return;

        Image image = stageSelectPageScreen[1].GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1.0f);

        _BookUI.BackToPrevPage();
    }

    /// <summary>
    /// �X�e�[�W�Z���N�g��ʂ̃G�t�F�N�g���������Z�b�g
    /// </summary>
    void StageSelectScreenEffectReset()
    {
        for (int i = 0; i < stageSelectPage.Length; i++)
        {
            for (int j = 0; j < pageButtonNum[i]; j++)
            {
                // ���o����
                if (i == 0)
                {
                    UIAlphaOffReset(ref stageSelectPage[i].stageButton[j], true);
                }
                else
                {
                    UIAlphaOnReset(ref stageSelectPage[i].stageButton[j], false);
                }
            }

            // active �֊i�[ (��ԍŏ��̂؁[�W�̂݃A�N�e�B�u�������)
            if (i == 0)
            {
                stageSelectPage[i].stageActive = true;
                UIAlphaOffReset(ref stageSelectPage[i].stageSelectScreen, true);
            }
            else
            {
                stageSelectPage[i].stageActive = false;
                UIAlphaOnReset(ref stageSelectPage[i].stageSelectScreen, false);
            }
        }

        //// �K�v�������1�y�[�W�ڂ֖߂��i�����j
        //ResetBookScript();
    }
    #endregion


    #region PUBLIC BUTTON METHOD
    //---------- ���C�����j���[��ʎ��̏��� ----------//
    public void ClickBeginStartButton()
    {
        fadeState = FadeState.Reset;
        resetScreenFlag = true;
        selectButton = noButton;
        Sound.PlaySE("PencilPush", 1f);
    }

    public void ClickContinueButton()
    {
        fadeState = FadeState.StageSelect;
        selectButton = stage01Button;
        nowActiveScreen = stageSelectScreens;
        Sound.PlaySE("PencilPush", 1f);
        gameData.StageSelectTransition();

        StageButtonActivity();
    }

    public void ClickHowToPlayButton()
    {
        fadeState = FadeState.HowToPlay;
        selectButton = cancelButton;
        nowActiveScreen = howToPlayScreen;
        Sound.PlaySE("PencilPush", 1f);
        gameData.HowToPlayTransition();
    }

    public void ClickStoryButton()
    {
        goShowStartStory = true;
        eventSystem.enabled = false;
    }

    public void ClickEndGameButton()
    {
        UnityEngine.Application.Quit();
    }

    //---------- �X�e�[�W�I����ʎ��̏��� ----------//
    public void ClickStageButton()
    {
        // �V�[���J�ڊJ�n
        goNextScene = true;

        // �T���v���V�[���ړ��w�����������ꍇ
        if (goSampleSceen)
        {
            gameData.ChangeCutInTransition();
            SceneManager.LoadScene("SampleScene");
            return;
        }

        // ���ׂẴX�e�[�W�̗p�ӂ��o������for���������O������
        string scenename = eventSystem.currentSelectedGameObject.gameObject.name;
        for (int i = 0; i < stageButton.Length; i++)
        {
            if (gameData.stageData[i].sceneName == scenename)
            {
                if (gameData.stageData[i].selectOK)
                {
                    if (ComicCheck(scenename))
                        return;

                    gameData.ChangeCutInTransition();
                    Sound.BGMAndSEResets();
                    SceneManager.LoadScene(scenename);
                    return;
                }
                else
                {
                    StartCoroutine("NotSelectClick");
                    return;
                }
            }
        }

        // �X�e�[�W�������Ȃ����i�K�ł͈ȉ��̏���������
        gameData.ChangeCutInTransition();
        SceneManager.LoadScene("SampleScene");
    }

    //---------- ���Z�b�g��ʎ��̏��� ----------//
    public void ClickNoButton()
    {
        fadeState = FadeState.Reset;
        resetScreenFlag = false;
        selectButton = continueButton;
        Sound.PlaySE("PencilPush", 1f);
    }

    public void ClickYesButton()
    {
        resetConfirmation.SetActive(false);
        toReset.SetActive(true);
        goNextScene = true;
        gameData.ResetData();
        if (stayScene)
        {   // �f�o�b�O�p�A�X�g�[���[�V�[���ɔ�΂��Ƀ^�C�g���V�[�����ēǂݍ���
            StartCoroutine("SceneReset");
        }
        else
        {
            StartCoroutine("ResetWaitTime");
        }
    }

    //---------- ���ʏ��� ----------//
    public void MoveNextPage()
    {
        if (nowEffecting)
            return;

        if (Input.GetAxis("Horizontal") > 0f || Input.GetAxis("R_Horizontal") > 0f || Input.GetAxis("D-Pad_Horizontal") > 0f)
        {
            if (nowActiveScreen == stageSelectScreens)
            {
                GoNextPageStageSelect();
            }
            else if (nowActiveScreen == howToPlayScreen)
            {
                GoNextPageHTP(ref howToPlayPageScreen);
            }
        }
    }

    public void MoveBackPage()
    {
        if (nowEffecting)
            return;

        if (Input.GetAxis("Horizontal") < 0f || Input.GetAxis("R_Horizontal") < 0f || Input.GetAxis("D-Pad_Horizontal") < 0f)
        {
            if (nowActiveScreen == stageSelectScreens)
            {
                GoBackPageStageSelect();
            }
            else if (nowActiveScreen == howToPlayScreen)
            {
                GoBackPageHTP(ref howToPlayPageScreen);
            }
        }
    }

    public void ClickCancelButton()
    {
        if (nowEffecting)
            return;

        if (nowActiveScreen == stageSelectScreens)
        {
            StageSelectScreenReset();
            fadeState = FadeState.StageSelect;
        }
        else if (nowActiveScreen == howToPlayScreen)
        {
            fadeState = FadeState.HowToPlay;
        }

        selectButton = continueButton;
        nowActiveScreen = titleAndMenuScreen;
        gameData.MeinMenuTransition();
    }

    public void Select()
    {
        if (selectButton != null && eventSystem.GetComponent<EventSystem>().currentSelectedGameObject == selectButton.gameObject)
        {
            selectButton = null;
        }
        else
        {
            Sound.PlaySE("PencilSelect", 0.75f);
        }
    }
    #endregion


    #region PRIVATE BUTTON METHOD
    // �ȉ��̊e��ʂ��Ƃ̏������e�͉��o�̎d���ɂ���ėv�ύX
    //---------- �^�C�g����ʎ��̏��� ----------//
    void PressAnyKey()
    {
        if (nowEffecting)
            return;

        if (gameData.showStartStory[0])
        {
            fadeState = FadeState.Title;
            selectButton = continueButton;
            Sound.PlaySE("PencilPush", 1f);
            gameData.MeinMenuTransition();
        }
        else
        {
            goNextScene = true;
            goShowStartStory = true;
        }
    }

    //---------- �X�e�[�W�I����ʎ��̏��� ----------//
    void GoNextPageStageSelect()
    {
        _BookUI.GoToNextPage();
        
        for (int i = 0; i < stageSelectPage.Length - 1; i++)
        {
            if (stageSelectPage[i].stageActive)
            {
                // ���̉�ʂ̈�ԎႢ�X�e�[�W���������ĂȂ���΁A�ȉ��̏����͂��Ȃ�
                if (!stageSelectPage[i + 1].buttonActive[0])
                    return;

                // ���̃y�[�W��UI���������Ă��镨�͂��ׂăA�N�e�B�u��
                stageSelectPage[i + 1].stageSelectScreen.SetActive(true);
                stageSelectPage[i + 1].stageActive = true;
                for(int j = 0; j < stageSelectPage[i + 1].stageButton.Length; j++)
                {
                    if(stageSelectPage[i + 1].buttonActive[j])
                    {
                        stageSelectPage[i + 1].stageButton[j].SetActive(true);
                    }
                    else
                    {
                        stageSelectPage[i + 1].stageButton[j].SetActive(false);
                    }
                }
                //// ���\������UI�����ׂĔ�A�N�e�B�u��
                //stageSelectPage[i].stageSelectScreen.SetActive(false);
                //stageSelectPage[i].stageActive = false;
                //for (int j = 0; j < stageSelectPage[i].stageButton.Length; j++)
                //{
                //    stageSelectPage[i].stageButton[j].SetActive(false);
                //}
                // �{�^���̑I����Ԃ��w��
                stageSelectPage[i + 1].stageButton[0].GetComponent<Button>().Select();

                return;
            }
        }
    }

    void GoBackPageStageSelect()
    {
        _BookUI.BackToPrevPage();

        for (int i = 1; i < stageSelectPage.Length; i++)
        {
            if (stageSelectPage[i].stageActive)
            {
                // ���̃y�[�W��UI�����ׂăA�N�e�B�u��
                stageSelectPage[i - 1].stageSelectScreen.SetActive(true);
                stageSelectPage[i - 1].stageActive = true;
                for (int j = 0; j < stageSelectPage[i - 1].stageButton.Length; j++)
                {
                    if (stageSelectPage[i - 1].buttonActive[j])
                    {
                        stageSelectPage[i - 1].stageButton[j].SetActive(true);
                    }
                    else
                    {
                        stageSelectPage[i - 1].stageButton[j].SetActive(false);
                    }
                }
                //// ���\������UI�����ׂĔ�A�N�e�B�u��
                //stageSelectPage[i].stageSelectScreen.SetActive(false);
                //stageSelectPage[i].stageActive = false;
                //for (int j = 0; j < stageSelectPage[i].stageButton.Length; j++)
                //{
                //    stageSelectPage[i].stageButton[j].SetActive(false);
                //}
                // �{�^���̑I����Ԃ��w��
                var buttonNum = stageSelectPage[i - 1].stageButton.Length - 1;
                stageSelectPage[i - 1].stageButton[buttonNum].GetComponent<Button>().Select();

                return;
            }
        }
    }

    void StageSelectScreenReset()
    {
        int buttonCount = 0;
        for (int i = 0; i < stageSelectPage.Length; i++)
        {
            // stageSelectScreen �֊i�[
            stageSelectPage[i].stageSelectScreen = stageSelectPageScreen[i];

            // stageButton, buttonActive �֊i�[
            for (int j = 0; j < pageButtonNum[i]; j++)
            {
                // stageButton�̏���
                if (buttonCount < stageButton.Length)
                {   // �e�y�[�W���ŕ\������{�^���̃f�[�^���i�[
                    stageSelectPage[i].stageButton[j] = stageButton[buttonCount].gameObject;
                }
                else
                {
                    Debug.LogWarning("�X�e�[�W�Z���N�g�̃f�[�^�i�[���A�͈͊O�ɃA�N�Z�X���悤�Ƃ��܂����B");
                }

                // buttonActive�̏���
                if (buttonCount == 0 || buttonCount <= gameData.notClearStageNum)
                {
                    stageSelectPage[i].buttonActive[j] = true;
                }
                else
                {
                    stageSelectPage[i].buttonActive[j] = false;
                }

                buttonCount++;
            }
        }
    }

    // �X�e�[�W�Z���N�g���̃X�e�[�W�{�^�����A�N�e�B�u�ɂ��邩�̏���
    void StageButtonActivity()
    {
        for (int i = 0; i < stageSelectPage[0].buttonActive.Length; i++)
        {
            if (stageSelectPage[0].buttonActive[i])
            {
                stageSelectPage[0].stageButton[i].SetActive(true);
            }
            else
            {
                stageSelectPage[0].stageButton[i].SetActive(false);
            }
        }
    }

    bool ComicCheck(string selectStage)
    {
        if (selectStage == gameData.stageScene[2] && !gameData.showStartStory[1])
        {
            Sound.BGMAndSEResets();
            SceneManager.LoadScene("Comic02");
            return true;
        }
        else if (selectStage == gameData.stageScene[4] && !gameData.showStartStory[2])
        {
            Sound.BGMAndSEResets();
            SceneManager.LoadScene("Comic03");
            return true;
        }
        else if (selectStage == gameData.stageScene[6] && !gameData.showStartStory[3])
        {
            Sound.BGMAndSEResets();
            SceneManager.LoadScene("Comic04");
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}

public class StageSelectPage
{
    public GameObject stageSelectScreen;
    public GameObject[] stageButton;      // ���̃X�e�[�W�Z���N�g��ʂŎg���{�^��
    public bool[] buttonActive;           // �X�e�[�W�{�^�����I���\��
    public bool stageActive;              // �X�e�[�W��ʂ��A�N�e�B�u��Ԃ�
}