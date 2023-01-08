using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Storage;

// �Q�[���̃N���A�󋵂�X�e�[�W�����Ǘ�����N���X
[CreateAssetMenu(fileName = "game_data", menuName =
        "ScriptableObjects/GameData", order = 1)]
public partial class GameData : ScriptableObject
{
    #region DEFINE
    /// <summary>
	/// �������
	/// </summary>
	private enum STATE
    {
        IDLE,
        SAVING,
        LOADING,
        ASYNC_WAIT,
        FINISH,
    }
    #endregion


    #region DATA
    public string[] stageScene = {"Stage01", "Stage02", "Stage03", "Stage04", "Stage05",
                                  "Stage06", "Stage07", "Stage08"};
    public int notClearStageNum;
    public StageData[] stageData = new StageData[8];
    public bool Editor_StartGame = false;      // �Q�[�����J�n�����ۂɗ��Ă�t���O
    public bool[] showStartStory = new bool[5]; // �X�^�[�g�X�g�[���[��������
    #endregion


    #region MEMBER
    private StorageManager storageManager = null;   // �X�g���[�W����
    private UserSettings usedSettings = null;       // ���݂̃f�[�^
    private UserSettings procSettings = null;       // �������̃f�[�^
    private FinishHandler ioHandler = null;         // �X�g���[�W�A�N�Z�X�R�[���o�b�N
    private STATE state = STATE.IDLE;               // �������
    private IO_RESULT result = IO_RESULT.NONE;      // ����
    private float ioTime = 0f;                      // �����J�n����
    private bool loadFlag;
    #endregion


    #region STORAGE MAIN FUNCTION
    /// <summary>
	/// ������
	/// </summary>
	public void Start()
    {   // TitleScript.cs �� Start() �ŌĂ�
        this.ioHandler = new FinishHandler(this.IOHandler);
        this.storageManager = new StorageManager();
        this.usedSettings = this.procSettings = new UserSettings();

        // ��O
        this.UpdateDataInfo((IO_RESULT)999);

        loadFlag = false;
    }

    /// <summary>
	/// �X�V
	/// </summary>
	public void Update()
    {   // TitleScript.cs �� Update() �ŌĂ�
        // �L���v�`��
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.P))
            ScreenCapture.CaptureScreenshot("capture.png");
#endif

        switch (this.state)
        {
            case STATE.IDLE:
                break;
            case STATE.ASYNC_WAIT:
                IEnumerator iEnumerator = AsyncWait();
                iEnumerator.MoveNext();                 //MonoBehaviour.StartCoroutine(this.AsyncWait()); �̑���
                this.state = STATE.FINISH;
                break;
            case STATE.FINISH:
                break;
        }
    }
    #endregion


    #region STORAGE PRIVATE FUNCTION
    /// <summary>
	/// �����R�[���o�b�N
	/// </summary>
	/// <param name="ret">����</param>
	/// <param name="dataInfo">���ʏ��</param>
	private void IOHandler(IO_RESULT ret, ref DataInfo dataInfo)
    {
        this.result = ret;
        switch (ret)
        {
            case IO_RESULT.SAVE_SUCCESS:
                // �ۑ�����
                if (dataInfo.async)
                {
                    this.state = STATE.ASYNC_WAIT;
                    break;
                }
                this.UpdateDataInfo(ret);
                this.state = STATE.IDLE;
                break;
            case IO_RESULT.SAVE_FAILED:
                // �ۑ����s
                if (dataInfo.async)
                {
                    this.state = STATE.ASYNC_WAIT;
                    break;
                }
                this.state = STATE.IDLE;
                break;
            case IO_RESULT.LOAD_SUCCESS:
                // �Ǎ�����
                this.procSettings = dataInfo.serializer as UserSettings;
                if (dataInfo.async)
                {
                    this.state = STATE.ASYNC_WAIT;
                    break;
                }
                this.UpdateDataInfo(ret);
                this.state = STATE.IDLE;
                break;
            case IO_RESULT.LOAD_FAILED:
                // �Ǎ����s
                if (dataInfo.async)
                {
                    this.state = STATE.ASYNC_WAIT;
                    break;
                }
                this.state = STATE.IDLE;
                break;
            case IO_RESULT.NONE:
                // �f�[�^�s��
                Debug.LogWarning("�f�[�^�ɕs������...�B");
                this.state = STATE.IDLE;
                break;
        }
    }

    /// <summary>
	/// �񓯊������҂�
	/// </summary>
	private IEnumerator AsyncWait()
    {
        // �񓯊��Œ�҂����ԁi���ۑ���Ǎ��̃A�j���[�V��������莞�ԕ\�������邽�߁j
        while ((Time.realtimeSinceStartup - this.ioTime) < 1.8f)
            yield return null;

        this.UpdateDataInfo(this.result);
    }

    /// <summary>
	/// �f�[�^���X�V
	/// </summary>
	/// <param name="result">����</param>
	private void UpdateDataInfo(IO_RESULT result)
    {
        UserSettings us = this.usedSettings = this.procSettings;
        //us.format = this.control.saveJson.isOn ? FORMAT.JSON : FORMAT.BINARY;
        //us.encrypt = this.control.saveEncrypt.isOn;
        //us.backup = this.control.saveBackup.isOn;
        if (loadFlag)
        {
            for(int i = 0; i < stageData.Length; i++)
            {
                stageData[i] = us.stageData[i];
            }
            notClearStageNum = us.notClearStageNum;
            for (int i = 0; i < showStartStory.Length; i++)
                showStartStory[i] = us.showStartStory[i];
            loadFlag = false;
        }
        this.state = STATE.IDLE;

        switch (result)
        {
            case IO_RESULT.NONE:
                Debug.LogWarning("�����s�I���i�f�[�^�s���j");
                break;
            case IO_RESULT.SAVE_SUCCESS:
            case IO_RESULT.LOAD_SUCCESS:
                Debug.Log("����");
                break;
            case IO_RESULT.SAVE_FAILED:
            case IO_RESULT.LOAD_FAILED:
                Debug.LogError("���s�i�ۑ��t�@�C���̔j�����A�f�[�^�����������j");
                break;
            default:
                break;
        }
    }
    #endregion


    #region DATA PUBLIC METHOD
    // �V�[���J�n���f�[�^�̓Ǎ��E�������s������
    public void StartData()
    {
#if UNITY_EDITOR
        // �G�f�B�^�[��ŏ���N�����̃t���O�� false �ɂ�����A
        // ���s�t�@�C���N������z�肵�Ď��s�����
        if (!Editor_StartGame)
        {
            SetFirstData();
            Save();
        }
#endif
        if (this.storageManager.Exists(usedSettings))
        {
            Load();
        }
        else
        {
            // �����f�[�^������āA���[�J���X�g���[�W���쐬
            SetFirstData();
            Save();
        }

#if UNITY_EDITOR
        // �G�f�B�^�[��ŏ���N�����̃t���O�� false �ɂ�����A
        // ���s�t�@�C���N������z�肵�Ď��s�����
        if (!Editor_StartGame)
        {
            TitleTransition();
            Editor_StartGame = true;
            return;
        }
#endif
        if(OutGameState != OutGame.MeinMenu)
        {
            TitleTransition();
        }
    }

    // �G�f�B�^���Ń^�C�g���ȊO����X�^�[�g����ƃG���[����������
    // �ꍇ������̂ŁA�����h�����߂̊֐�
    // StageUIScript.cs �ŌĂяo�����Ă���
    public void EditorStart()
    {
#if UNITY_EDITOR
        if (storageManager == null)
        {
            Start();
            Load();
        }
#endif
    }

    // �X�e�[�W�̃N���A��ԂƃA�C�e���擾��Ԃ����Z�b�g
    public void ResetData() 
    {
        SetFirstData();
        Save();
    }

    // �X�e�[�W�̃N���A��ԂƃA�C�e���擾��Ԃ�S�Ăł��Ă����ԂɃA�E��
    public void AllClearData()
    {
        notClearStageNum = 8;

        for (int i = 0; i < stageData.Length; i++)
        {
            stageData[i].selectOK = true;
            stageData[i].itemGetFlag = true;
        }

        for(int i = 0; i < showStartStory.Length; i++)
        {
            showStartStory[i] = true;
        }

        // �f�[�^�̕ۑ�
        Save();
    }

    // �X�^�[�g�X�g�[���[���������̏���
    public void ShowStartStoryComp(int num)
    {
        showStartStory[num] = true;
        Save();
    }

    // �N���A�����X�e�[�W�̎��̃X�e�[�W���ł���悤�Ɋi�[
    // �܂��A�A�C�e�����Ƃ��Ă����炻�̃f�[�^���i�[
    public void ClearStageDataStorage(int stageNum, bool itemGet)
    {
        // �����������Ă��Ȃ��X�e�[�W�ȊO�͏������s��Ȃ�
        if (stageNum != notClearStageNum)
            return;

        notClearStageNum++;
        stageData[stageNum + 1].selectOK = true;
        if (itemGet)
        {
            stageData[stageNum].itemGetFlag = true;
        }

        // �f�[�^�̕ۑ�
        Save();
    }
    #endregion


    #region DATA PRIVATE METHOD
    // �����f�[�^�����
    void SetFirstData()
    {
        notClearStageNum = 0;

        for (int i = 0; i < stageData.Length; i++)
        {
            if (stageData[i] == null)
            {
                stageData[i] = new StageData();
            }

            if (i == 0)
            {
                stageData[i].SetData(true, false, stageScene[i]);
            }
            else
            {
                stageData[i].SetData(false, false, stageScene[i]);
            }
        }

        for(int i = 0; i < showStartStory.Length; i++)
            showStartStory[i] = false;
    }
#endregion


    #region CONTROL FUNCTION
    // �f�[�^��ۑ�����֐�
    void Save()
    {
        // �ۑ��ݒ�
        this.ioTime = Time.realtimeSinceStartup;

        // �ۑ��ݒ�i���f���p�̐ݒ�ύX�j
        this.procSettings = this.usedSettings.Clone() as UserSettings;
        UserSettings us = this.procSettings;

        // ���e�X�V
        for(int i = 0; i < us.stageData.Length; i++)
        {
            us.stageData[i] = this.stageData[i];
        }
        us.notClearStageNum = this.notClearStageNum;
        for (int i = 0; i < showStartStory.Length; i++)
            us.showStartStory[i] = this.showStartStory[i];

        // �ۑ��i��FinishHandler��null�ł��j
        bool async = false; // �񓯊������͂��Ȃ�
        this.storageManager.Save(this.procSettings, this.ioHandler, async);
    }

    // �f�[�^��ǂݍ��ފ֐�
    public void Load()
    {
        this.ioTime = Time.realtimeSinceStartup;

        loadFlag = true;

        // �Ǎ�
        bool async = false; // �񓯊������͂��Ȃ�
        this.storageManager.Load(this.usedSettings, this.ioHandler, async);
    }
    #endregion
}

[System.Serializable]
public class StageData
{
    public bool selectOK;
    public bool itemGetFlag;
    public string sceneName;

    public void ClearData()
    {
        selectOK = false;
        itemGetFlag = false;
        sceneName = null;
    }

    public void SetData(bool select, bool item, string scene)
    {
        selectOK = select;
        itemGetFlag = item;
        sceneName = scene;
    }
}
