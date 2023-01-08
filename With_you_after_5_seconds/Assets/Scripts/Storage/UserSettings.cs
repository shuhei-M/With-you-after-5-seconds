using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class UserSettings : Storage.ISerializer
{
	// ������L���b�V��
	// �v���p�e�B�ɒ��ڒ�`����ƃv���p�e�B�A�N�Z�X�̓x�ɖ��ʂȃ������A���P�[�g����������
	[System.NonSerialized]
	private const string magic_ = "UserSettings_180101";
	//[System.NonSerialized]
	//private const string fileName_ = "/UserSettings";
	[System.NonSerialized]
	private const string fileName_ = "/SaveData";

	// �C���^�[�t�F�[�X����
	public string magic { get { return UserSettings.magic_; } }    // �}�W�b�NNo.
	public string fileName { get { return UserSettings.fileName_; } } // �ۑ���
	public System.Type type { get { return typeof(UserSettings); } }   // JSON�f�V���A���C�Y�p�^�錾

	public Storage.FORMAT format { get { return Storage.FORMAT.BINARY; } } // �ۑ��`��
	public bool encrypt { get { return true; } }                  // �Í������邩�i�C�Ӂj
	public bool backup { get { return true; } }

	// �X�V�f�[�^
	public StageData[] stageData = new StageData[8];   // �X�e�[�W�̃N���A���
	public int notClearStageNum = 0;                    // �N���A�ł��Ă��Ȃ��X�e�[�W
	public bool startGame = false;                      // �Q�[���J�n����
	public bool[] showStartStory = new bool[5];					// �X�^�[�g�X�g�[���[�����Ă��邩

	/// <summary>
	/// ����
	/// </summary>
	public Storage.ISerializer Clone()
	{
		return this.MemberwiseClone() as Storage.ISerializer;
	}

	public void Clear()
	{
		for(int i = 0; i < stageData.Length; i++)
        {
			stageData[i].ClearData();
        }
		notClearStageNum = 0;
		startGame = false;

		for(int i = 0; i < showStartStory.Length; i++)
			showStartStory[i] = false;
		Debug.Log("���[�J���X�g���[�W���N���A���܂����B\n");
	}
}