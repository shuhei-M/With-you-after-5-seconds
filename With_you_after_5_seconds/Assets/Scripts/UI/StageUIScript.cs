using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI.Extensions;

public class StageUIScript : UI_Effect
{
    #region serialize field
    [SerializeField] private GameData gameData;
    [Space(3)]

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pausePlayButton;
    [Space(3)]

    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject[] howToPlayPagePanel;
    [SerializeField] private Button cancelButton;
    [SerializeField] float fadeSec = 2f;
    [Space(3)]

    [SerializeField] private GameObject retryPanel;
    [SerializeField] private Button retryNoButton;
    [Space(3)]

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float time = 500f;
    [Space(3)]

    [SerializeField] private GameObject clearPanel;
    [SerializeField] private Button nextStageButton;
    [SerializeField] private float clearStayTime = 2.0f;
    [Space(3)]

    [SerializeField] private GameObject brackFadePanel;
    #endregion


    #region private member
    FadeState fadeState = FadeState.None;
    GameObject selectButton;
    GameObject selectedButton; // �}�E�X�Ȃǂ������ŃR���g���[���[���삪�o���Ȃ��Ȃ�̂�h�����߂ɕۊǂ��Ă����p
    bool howToPlayFlag = false;
    bool retryFlag = false;
    bool nowEffecting = false;

    float timeLimit;

    Scene nowStage;

    EventSystem eventSystem;

    /// <summary> �C���Q�[�����̃f�t�H���g�̃p�l���i�����j </summary>
    private GameObject _DefaultPanel;
    private InGame _CurrentInGameState;
    private InGame _PrevInGameState;

    /// <summary> �N���A��ʕ\���O�̃y�[�W�߂��艉�o�̃p�l���i�����j </summary>
    private BookUI _ClearBookUI;
    private GameObject _WarpEffectObj;
    private UI_WarpEffect _WarpEffect;
    private DirectingScript _DirectingScript;

    /// <summary> Stage�ւ̑J�ڎ��̃y�[�W�߂��艉�o�i�����j </summary>
    //private bool _IsStartFirstCutIn = false;
    private GameObject _FirstCutInPanel;
    private BookUI _FirstCutInUI;
    //private float _FirstCutInTime;

    /// <summary> ���g���C���̃y�[�W�߂��艉�o�i�����j </summary>RetryPanel
    private UI_RetryPanel _RetryPanel;
    #endregion


    #region propaty
    public float ClearStayTime { get { return clearStayTime; } }
    #endregion


    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        gameData.EditorStart();
        
        // ���ʉ����ꊇ���[�h
        SoundSet();

        pausePanel.GetComponent<PauseUIAnimation>().PauseUISetUp();

        timeLimit = time;

        nowStage = SceneManager.GetActiveScene();

        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        selectButton = pausePlayButton.gameObject;

        // ����
        _FirstCutInPanel = transform.Find("FirstCutInPanel").gameObject;
        _FirstCutInUI = _FirstCutInPanel.GetComponent<BookUI>();
        _DefaultPanel = transform.Find("DefaultPanel").gameObject;
        _DefaultPanel.SetActive(false);
        _ClearBookUI = clearPanel.GetComponent<BookUI>();

        _CurrentInGameState = InGame.None;
        _PrevInGameState = _CurrentInGameState;

        _WarpEffectObj = transform.Find("WarpEffectPanel").gameObject;
        _WarpEffect = _WarpEffectObj.GetComponent<UI_WarpEffect>();

        _DirectingScript = GameObject.Find("DirectingObj").GetComponent<DirectingScript>();

        _RetryPanel = transform.Find("RetryPanel").gameObject.GetComponent<UI_RetryPanel>();
        _RetryPanel.gameObject.SetActive(false);

        // �X�e�[�W09�̏ꍇ
        if(nowStage.name == "Stage09")
        {
            _FirstCutInPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _CurrentInGameState = gameData.InGameState;
        UpdateState();

        FadeEffect();

        if (gameData.InGameState == InGame.Pause || gameData.InGameState == InGame.GoalCompletion)
        {
            // ���I�����Ă�����̂� First Selected �ɐ����㏑������
            // �I����Ԃ���������Ă��܂������ɍēx�I����Ԃ��擾�ł���悤�ɂ���
            ReadCurrentSelectedButton();
            ReferenceSelectedButton();
        }

        if (howToPlayFlag)
        {
            cancelButton.Select();
        }
        
        if(!howToPlayFlag && !retryFlag)
        {
            // �Q�[���N���A������Q�[�����̉��o������ǉ������ۂ�
            // �|�[�Y��ʂɍs�����ǂ����̏������ύX�̉\����
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown("joystick button 7"))
            {
                PauseActiveChange();
            }
        }

        // �ЂƂO�̃C���Q�[���X�e�[�g�Ƃ��ĕۑ�
        _PrevInGameState = _CurrentInGameState;
    }
    #endregion


    #region Coroutine
    /// <summary>
    /// ���g���C�p�R���[�`��
    /// </summary>
    private IEnumerator RetryCoroutine()
    {
        yield return new WaitForSeconds(_ClearBookUI.TurnTime);

        // ���݂̃V�[�����ēǂݍ���
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator MainMenuTransitionCorutine()
    {
        float time = 0f;

        eventSystem.enabled = false;

        while(time <= fadeSec)
        {
            FadeInUI(ref brackFadePanel, fadeSec, true);
            if (_CurrentInGameState == InGame.Pause)
            {
                Sound.VolumeDownBGM(Time.unscaledDeltaTime, 0.3f);
            }
            yield return new WaitForEndOfFrame();
            time += Time.unscaledDeltaTime;
        }

        Time.timeScale = 1f;
        gameData.MeinMenuTransition();
        SceneManager.LoadScene("TitleScene");
    }
    #endregion


    #region private function
    /// <summary>
    /// ��Ԃ̕ύX
    /// </summary>
    private void ChangeState()
    {
        // ���O���o��
        Debug.Log("ChangeState " + _PrevInGameState + "-> " + _CurrentInGameState);

        switch (_CurrentInGameState)
        {
            case InGame.CutIn:
                {
                    _DefaultPanel.SetActive(false);
                }
                break;
            case InGame.ChangeStartView:
                {
                    _FirstCutInPanel.SetActive(false);
                }
                break;
            case InGame.EntryPlayer:
                {
                }
                break;
            case InGame.PlayGame:
                {
                    _DefaultPanel.SetActive(true);
                }
                break;
            case InGame.Pause:
                {
                }
                break;
            case InGame.EntryGoal:
                {
                    _DefaultPanel.SetActive(false);
                }
                break;
            case InGame.InGoal:
                {
                    Sound.PlaySE("Warp", 0.5f);

                    ////���o�ǉ��̂��߁A���̎��_�ŃN���A�p�l�����I���ɂ���i�����j
                    //clearPanel.SetActive(true);
                    //_WarpEffectObj.SetActive(true);
                    //// _WarpEffect.StartEffect();
                    //StartCoroutine(StayClearUI());
                }
                break;
            case InGame.GoalCompletion:
                {
                    Sound.StopBGM();
                    if (nowStage.name != "Stage08")
                    {
                        Sound.PlaySE("StageClear", 1f);
                    }
                }
                break;
        }

    }

    /// <summary>
    /// ��Ԗ��̖��t���[���Ă΂�鏈��
    /// </summary>
    private void UpdateState()
    {
        if (IsEntryThisState()) { ChangeState(); return; }

        switch (_CurrentInGameState)
        {
            case InGame.CutIn:
                {
                    // �X�e�[�W09�̏ꍇ
                    if (nowStage.name == "Stage09")
                    {
                        gameData.ChangeStartViewTransition();
                        return;
                    }

                    // ����ȊO�̃V�[��
                    CutInEffectUpdate();
                }
                break;
            case InGame.ChangeStartView:
                {
                }
                break;
            case InGame.EntryPlayer:
                {
                }
                break;
            case InGame.PlayGame:
                {
                    // �`�[�g�R�}���h
                    if (Input.GetKey(KeyCode.C) && Input.GetKeyDown(KeyCode.Return))
                    {
                        StageClear();
                    }

                    // �������Ԃ��X�V����
                    timeLimit -= Time.deltaTime;
                    timerText.text = "Time:" + (int)timeLimit;
                }
                break;
            case InGame.Pause:
                {
                }
                break;
            case InGame.EntryGoal:
                {
                    Sound.VolumeDownBGM(Time.deltaTime, 0.6f);
                }
                break;
            case InGame.InGoal:
                {
                    Sound.VolumeDownBGM(Time.deltaTime, 0.6f);

                    // �J��������������Ă�����
                    if (_DirectingScript.IsStartWarpEffect && !_WarpEffectObj.activeSelf)
                    {
                        //���o�ǉ��̂��߁A���̎��_�ŃN���A�p�l�����I���ɂ���i�����j
                        clearPanel.SetActive(true);
                        _WarpEffectObj.SetActive(true);
                        // _WarpEffect.StartEffect();
                        StartCoroutine(StayClearUI());
                    }
                }
                break;
            case InGame.GoalCompletion:
                {
                    if (nowStage.name == "Stage08")
                    {   // �t�@�C�i���}���K�ցI
                        FadeInUI(ref brackFadePanel, fadeSec * 2);

                        if (brackFadePanel.GetComponent<Image>().color.a == 1f)
                        {
                            //gameData.MeinMenuTransition();
                            //SceneManager.LoadScene("TitleScene");

                            gameData.ChangeCutInTransition();
                            SceneManager.LoadScene("Stage09");
                        }
                    }
                    else
                    {
                        // �y�[�W���߂��鉉�o���s���i�����j
                        _ClearBookUI.TurnPageUpdate();
                    }

                    // �}�E�X���������ۂɁA�i�s�s�\�ɂȂ邱�Ƃ�h���B
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                        _ClearBookUI.ToUseableButton();
                }
                break;
        }
    }

    /// <summary>
    /// ���傤�ǂ��̃X�e�[�g�ɓ����������ǂ���
    /// </summary>
    /// <returns></returns>
    private bool IsEntryThisState()
    {
        return (_PrevInGameState != _CurrentInGameState);
    }

    void SoundSet()
    {
        Sound.LoadSE("PencilSelect", "ButtonSelect");
        Sound.LoadSE("PencilPush", "ButtonPush");
        Sound.LoadSE("StageClear", "StageClear");
        Sound.LoadSE("Warp", "warp");
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
    /// �J�b�g�C���G�t�F�N�g���X�V����
    /// </summary>
    private void CutInEffectUpdate()
    {
        // �I����Ă�����
        if (_FirstCutInUI.IsFinishEffect)
        {
            gameData.ChangeStartViewTransition();
            _FirstCutInPanel.SetActive(false);
            return;
        }
    }

    /// <summary>
    /// �X�e�[�W�N���A�܂ň�莞�ԑ҂�
    /// </summary>
    /// <returns></returns>
    IEnumerator StayClearUI()
    {
        yield return new WaitForSeconds(clearStayTime);
        _ClearBookUI.DisplayBGImage();
        if(nowStage.name != "Stage08")
        {
            // �y�[�W���߂��鉉�o���J�n������i�����j
            _ClearBookUI.GoToNextPage();
        }
        StageClear();
    }

    // �N���A�������̏���
    void StageClear()
    {
        gameData.GoalCompletionTransition();
        clearPanel.SetActive(true);
        selectButton = nextStageButton.gameObject;
        nextStageButton.Select();

        if (nowStage.name == "SampleScene")
            return;

        for (int i = 0; i < gameData.stageData.Length; i++)
        {
            if (gameData.stageData[i].sceneName == nowStage.name)
            {
                // �N���A�����X�e�[�W�̏���ۑ�����
                // �A�C�e���@�\�͂܂�����Ă��Ȃ��̂� false ������悤���Ă���i�v�ύX�j
                gameData.ClearStageDataStorage(i, false);
                return;
            }
        }
    }


    bool ComicCheck()
    {
        if (nowStage.name == gameData.stageScene[1] && !gameData.showStartStory[1])
        {
            SceneManager.LoadScene("Comic02");
            return true;
        }
        else if(nowStage.name == gameData.stageScene[3] && !gameData.showStartStory[2])
        {
            SceneManager.LoadScene("Comic03");
            return true;
        }
        else if (nowStage.name == gameData.stageScene[5] && !gameData.showStartStory[3])
        {
            SceneManager.LoadScene("Comic04");
            return true;
        }
        else if (nowStage.name == gameData.stageScene[7] && !gameData.showStartStory[4])
        {
            //SceneManager.LoadScene("");
            //return true;

            return false;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// �t�F�[�h�G�t�F�N�g����
    /// </summary>
    void FadeEffect()
    {
        if (fadeState == FadeState.HowToPlay)
        {
            nowEffecting = true;

            if (howToPlayFlag)
            {
                PanelToFade(ref pausePanel, ref howToPlayPanel, ref fadeState, pausePanel, fadeSec, true, true);
            }
            else
            {
                PanelToFade(ref howToPlayPanel, ref pausePanel, ref fadeState, pausePanel, fadeSec, false, true);
            }


            if (!howToPlayPanel.activeInHierarchy)
            {
                selectButton = pausePlayButton.gameObject;
                pausePlayButton.Select();
                HowToPlayEffectReset(ref howToPlayPanel);
            }
        }
        else if (fadeState == FadeState.Reset)
        {
            nowEffecting = true;

            if (retryFlag)
            {
                PanelToFade(ref pausePanel, ref retryPanel, ref fadeState, pausePanel, fadeSec, true, true);

                if (retryPanel.GetComponent<Image>().color.a >= 1f)
                {
                    selectButton = retryNoButton.gameObject;
                    retryNoButton.Select();
                }
            }
            else
            {
                PanelToFade(ref retryPanel, ref pausePanel, ref fadeState, pausePanel, fadeSec, false, true);
            }


            if (!retryPanel.activeInHierarchy)
            {
                selectButton = pausePlayButton.gameObject;
                pausePlayButton.Select();
            }
        }
        else
        {
            nowEffecting = false;
        }
    }
    #endregion


    #region Button function
    // �|�[�Y�{�^�����������Ƃ��̏���
    void PauseActiveChange()
    {
        // �v���C���������̓|�[�Y��ʒ��Ȃ珈�����s��Ȃ�
        if (gameData.InGameState != InGame.PlayGame && gameData.InGameState != InGame.Pause)
            return;

        var pauseUIAnim = pausePanel.GetComponent<PauseUIAnimation>();

        // �J�b�g�������Ȃ珈�����s��Ȃ�
        if (!pauseUIAnim.CutOK)
            return;

        // �|�[�Y��ʂ�\�������Ƃ��̏���
        if (gameData.InGameState != InGame.Pause)
        {
            gameData.PauseTransition();
            pausePanel.SetActive(true);
            pauseUIAnim.CutOK = false;
            pauseUIAnim.CutInPause();
        }
        else
        {
            pauseUIAnim.CutOK = false;
            pauseUIAnim.CutOutPause();
        }
    }

    // ---------- �|�[�Y��ʒ��̃{�^������ ---------- //
    public void ClickPlayButton()
    {
        Sound.PlaySE("PencilPush", 1f);
        pausePanel.GetComponent<PauseUIAnimation>().CutOutPause();
    }

    public void ClickHowToPlayButton()
    {
        Sound.PlaySE("PencilPush", 1f);
        fadeState = FadeState.HowToPlay;
        howToPlayFlag = true;
        cancelButton.Select();
    }

    public void MoveNextPage()
    {
        if (nowEffecting)
            return;

        if (Input.GetAxis("Horizontal") > 0f || Input.GetAxis("R_Horizontal") > 0f || Input.GetAxis("D-Pad_Horizontal") > 0f)
        {
            GoNextPageHTP(ref howToPlayPagePanel);
        }
    }

    public void MoveBackPage()
    {
        if (nowEffecting)
            return;

        if (Input.GetAxis("Horizontal") < 0f || Input.GetAxis("R_Horizontal") < 0f || Input.GetAxis("D-Pad_Horizontal") < 0f)
        {
            GoBackPageHTP(ref howToPlayPagePanel);
        }
    }

    public void ClickCancelButton()
    {
        if (nowEffecting)
            return;

        fadeState = FadeState.HowToPlay;
        howToPlayFlag = false;
    }

    //if(_PaperFlip == -_PaperRange)
    // ---------- �N���A��ʒ��̃{�^������ ---------- //
    public void ClickNextStageButton()
    {
        Sound.PlaySE("PencilPush", 1f);

        // �y�[�W�߂��艉�o���I����Ă��Ȃ���΁A�ȉ��͎��s���Ȃ��i�����j
        if (gameData.InGameState == InGame.GoalCompletion && !_ClearBookUI.IsFinishEffect)
        {
            return;
        }

        if (nowStage.name == "SampleScene")
        {
            Time.timeScale = 1f;
            gameData.ChangeCutInTransition();
            SceneManager.LoadScene(nowStage.name);
            return;
        }

        // �}���K�̃��[�r�[�ɍs����
        if (ComicCheck())
        {
            return;
        }

        for (int i = 0; i < gameData.stageData.Length; i++)
        {
            if (nowStage.name == gameData.stageData[i].sceneName)
            {
                Time.timeScale = 1f;
                gameData.ChangeCutInTransition();
                SceneManager.LoadScene(gameData.stageData[i + 1].sceneName);
                return;
            }
        }
    }

    // ---------- ���g���C�m�F��ʂ̃{�^������ ---------- //
    public void RetryNoButton()
    {
        retryFlag = false;
        fadeState = FadeState.Reset;
        Sound.PlaySE("PencilPush", 1f);
    }

    public void RetryYesButton()
    {
        RetryFunction();
    }


    // ---------- �|�[�Y�E�N���A��ʋ��ʂ̃{�^������ ---------- //
    public void ClickStageSelectButton()
    {
        // �y�[�W�߂��艉�o���I����Ă��Ȃ���΁A�ȉ��͎��s���Ȃ��i�����j
        if (gameData.InGameState == InGame.GoalCompletion && !_ClearBookUI.IsFinishEffect)
        {
            return;
        }

        Time.timeScale = 1f;
        gameData.StageSelectTransition();
        Sound.BGMAndSEResets();
        SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// ���C�����j���[�ֈړ�����
    /// �X�e�[�W�N���A���ɕ\��
    /// </summary>
    public void ClickMeinMenuButton()
    {
        // �y�[�W�߂��艉�o���I����Ă��Ȃ���΁A�ȉ��͎��s���Ȃ��i�����j
        if (gameData.InGameState == InGame.GoalCompletion && !_ClearBookUI.IsFinishEffect)
        {
            return;
        }

        Sound.PlaySE("PencilPush", 1f);
        StartCoroutine(MainMenuTransitionCorutine());

        //Time.timeScale = 1f;
        //gameData.MeinMenuTransition();
        //SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// ���݂̃X�e�[�W�ɍĒ��킷��
    /// �X�e�[�W�N���A���ɕ\��
    /// </summary>
    public void ClickRetryButton()
    {
        if (gameData.InGameState == InGame.Pause)
        {
            retryFlag = true;
            fadeState = FadeState.Reset;
            Sound.PlaySE("PencilPush", 1f);

            //Time.timeScale = 1f;
            //gameData.ChangeCutInTransition();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // �y�[�W�߂��艉�o���I����Ă��Ȃ���΁A�ȉ��͎��s���Ȃ�
        if (gameData.InGameState == InGame.GoalCompletion && !_ClearBookUI.IsFinishEffect)
        {
            Debug.Log("wwwwwwwwwwwwwwwwwwww");
            return;
        }

        _RetryPanel.gameObject.SetActive(true);
        clearPanel.SetActive(false);
        Sound.BGMAndSEResets();

        //_ClearBookUI.GoToNextPage();

        //// ���g���C�R���[�`�����N��
        //StartCoroutine(RetryCoroutine());
    }

    public void RetryFunction()
    {
        Time.timeScale = 1f;
        gameData.ChangeCutInTransition();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Select()
    {
        if (selectButton != null && eventSystem.GetComponent<EventSystem>().currentSelectedGameObject == selectButton)
        {
            selectButton = null;
        }
        else
        {
            Sound.PlaySE("PencilSelect", 0.75f);
        }
    }
    #endregion
}
