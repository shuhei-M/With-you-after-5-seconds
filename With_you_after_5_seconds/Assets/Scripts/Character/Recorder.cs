using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Recorder : MonoBehaviour
{
	#region define
	/// <summary>
	/// �c���f�[�^�N���X
	/// </summary>
	[Serializable]
	public class AfterimageData
	{
		// �ʒu�̃��X�g
		private List<Vector3> posLists = new List<Vector3>();
		// �p�x���X�g
		private List<Quaternion> rotLists = new List<Quaternion>();
		// �A�j���[�V�����p�����[�^DeltaTime�l
		private List<float> deltaTimeLists = new List<float>();
		//�@�A�j���[�V�����p�����[�^Blend�l
		private List<float> blendLists = new List<float>();
		// �A�j���[�V�����p�����[�^��IsLadder
		private List<bool> IsLadderLists = new List<bool>();
		// State���X�g
		private List<PlayerState> playerStateLists = new List<PlayerState>();
		// �A�N�V�����^�C�v ���X�g
		private List<int> actionTypeLists = new List<int>();

		//--- �Z�b�^ ---//
		public void Add_PosLists(Vector3 psition) { posLists.Add(psition); }
		public void Add_RotLists(Quaternion quaternion) { rotLists.Add(quaternion); }
		public void Add_DeltaTimeLists(float deltaTime) { deltaTimeLists.Add(deltaTime); }
		public void Add_BlendLists(float blend) { blendLists.Add(blend); }
		public void Add_IsLadderLists(bool isLadder) { IsLadderLists.Add(isLadder); }
		public void Add_PlayerStateLists(PlayerState playerState) { playerStateLists.Add(playerState); }
		public void Add_ActionTypeLists(int actionType) { actionTypeLists.Add(actionType); }

		//--- �Q�b�^ ---//
		public Vector3 Get_PosLists(int index) { return posLists[index]; }
		public Quaternion Get_RotLists(int index) { return rotLists[index]; }
		public float Get_DeltaTimeLists(int index) { return deltaTimeLists[index]; }
		public float Get_BlendLists(int index) { return blendLists[index]; }
		public bool Get_IsLadderLists(int index) { return IsLadderLists[index]; }
		public PlayerState Get_PlayerStateLists(int index) { return playerStateLists[index]; }
		public int Get_ActionTypeLists(int index) { return actionTypeLists[index]; }

		//--- ���X�g�̃N���A ---//
		public void Clear_AllLists()
        {
			posLists.Clear();
			rotLists.Clear();
			deltaTimeLists.Clear();
			blendLists.Clear();
			IsLadderLists.Clear();
			playerStateLists.Clear();
			actionTypeLists.Clear();
		}

		//--- ���X�g���̗v�f����Ԃ��i��\����posLists�j ---//
		public int Count_Lists() { return posLists.Count; }
	}
	#endregion

	#region serialize field
	[SerializeField] private StageController _StageManager;

	//�@�ۑ�����f�[�^�̍ő吔
	[SerializeField] private int _MaxDataNum = 2000;
	//�@�L�^�Ԋu
	[SerializeField] private float _RecordDuration = 0.005f;
	#endregion

	#region field
	/// <summary> �v���C���[�ɃA�^�b�`���ꂽ�R���|�[�l���g���擾���邽�߂̕ϐ��Q </summary>
	private GameObject _Player;
	private PlayerBehaviour _PlayerController;
	private Animator _PlayerAnimator;

	/// <summary> �c���ɃA�^�b�`���ꂽ�R���|�[�l���g���擾���擾���邽�߂̕ϐ��Q </summary>
	private GameObject _Afterimage;
	private AfterimageBehaviour _AfterimageController;

	/// <summary> �f�[�^�̕ۑ��ɕK�v�ȕϐ��Q </summary>
	//�@���݋L�����Ă��邩�ǂ���
	private bool _IsRecord;

	//�@�o�ߎ���
	private float _ElapsedTime = 0f;
	//�@�c���f�[�^
	private AfterimageData[] _AfterimageData;

	public float _AfterimagePlayTime = 5f;
	float _Time;
	bool _TimeCount = true;

	int _RecordSwitcher;
	#endregion

	#region property
	/// <summary>
	/// �v���C���[�̋������ۑ����ꂽ�N���X�ɃA�N�Z�X���邽�߂̃C���f�N�T
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public AfterimageData this[int index]
	{
		get { return _AfterimageData[index]; }
	}
	#endregion

	#region Unity function
	void Start()
	{
		/// <summary> �v���C���[�̃f�[�^���擾���� </summary>
		_Player = _StageManager._Player;
		_PlayerController = _Player.GetComponent<PlayerBehaviour>();
		_PlayerAnimator = _Player.GetComponent<Animator>();

		/// <summary> �c���̃f�[�^���擾���� </summary>
		_Afterimage = _StageManager._Afterimage; 
		_AfterimageController = _Afterimage.GetComponent<AfterimageBehaviour>();

		_Time = 0f;
		StartRecord();

		_RecordSwitcher = 0;
	}

	// Update is called once per frame
	void Update()
	{
		if (_TimeCount)
			_Time += Time.deltaTime;

		if (_Time >= _AfterimagePlayTime)
		{
			_Afterimage.SetActive(true);
			_TimeCount = false;
			_Time = 0f;
		}

		if (_IsRecord)
		{
			_ElapsedTime += Time.deltaTime;

			if (_ElapsedTime >= _RecordDuration)
			{
				_AfterimageData[_RecordSwitcher].Add_PosLists(_Player.transform.position);
				_AfterimageData[_RecordSwitcher].Add_RotLists(_Player.transform.rotation);
				_AfterimageData[_RecordSwitcher].Add_DeltaTimeLists(_PlayerAnimator.GetFloat("DeltaTime"));
				_AfterimageData[_RecordSwitcher].Add_BlendLists(_PlayerAnimator.GetFloat("MoveBlend"));
				_AfterimageData[_RecordSwitcher].Add_IsLadderLists(_PlayerAnimator.GetBool("IsLadder"));

				_AfterimageData[_RecordSwitcher].Add_PlayerStateLists(_PlayerController.State);
				_AfterimageData[_RecordSwitcher].Add_ActionTypeLists(_PlayerAnimator.GetInteger("ActionType"));

				_ElapsedTime = 0f;

				//�@�f�[�^�ۑ������ő吔�𒴂�����ۑ��ꏊ�ύX
				if (_AfterimageData[_RecordSwitcher].Count_Lists() >= _MaxDataNum)
				{
					if (_RecordSwitcher == 0)
						_RecordSwitcher = 1;
					else
						_RecordSwitcher = 0;
				}
			}
		}
	}
	#endregion

	#region private function
	/// <summary> public ���� private �ɕύX�����̂ŃG���[���o����ύX���鎖 </summary>

	/// <summary>
	/// �L�����N�^�[�f�[�^�̕ۑ�
	/// </summary>
	private void StartRecord()
	{
		//�@�ۑ����鎞�̓S�[�X�g�̍Đ����~
		StopAllCoroutines();
		StopGhost();
		_IsRecord = true;
		_ElapsedTime = 0f;
		_AfterimageData = new AfterimageData[2];
		_AfterimageData[0] = new AfterimageData();
		_AfterimageData[1] = new AfterimageData();
		//ghostData= new GhostData();
		Debug.Log("StartRecord");
	}

	/// <summary>
	/// �L�����N�^�[�f�[�^�̕ۑ��̒�~
	/// </summary>
	private void StopRecord()
	{
		_IsRecord = false;
		Debug.Log("StopRecord");
	}

	/// <summary>
	/// �S�[�X�g�̒�~
	/// </summary>
	private void StopGhost()
	{
		Debug.Log("StopGhost");
		StopAllCoroutines();
		_Afterimage.SetActive(false);
	}
	#endregion
}
