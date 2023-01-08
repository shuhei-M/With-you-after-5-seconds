using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Recorder : MonoBehaviour
{
	#region define
	/// <summary>
	/// 残像データクラス
	/// </summary>
	[Serializable]
	public class AfterimageData
	{
		// 位置のリスト
		private List<Vector3> posLists = new List<Vector3>();
		// 角度リスト
		private List<Quaternion> rotLists = new List<Quaternion>();
		// アニメーションパラメータDeltaTime値
		private List<float> deltaTimeLists = new List<float>();
		//　アニメーションパラメータBlend値
		private List<float> blendLists = new List<float>();
		// アニメーションパラメータのIsLadder
		private List<bool> IsLadderLists = new List<bool>();
		// Stateリスト
		private List<PlayerState> playerStateLists = new List<PlayerState>();
		// アクションタイプ リスト
		private List<int> actionTypeLists = new List<int>();

		//--- セッタ ---//
		public void Add_PosLists(Vector3 psition) { posLists.Add(psition); }
		public void Add_RotLists(Quaternion quaternion) { rotLists.Add(quaternion); }
		public void Add_DeltaTimeLists(float deltaTime) { deltaTimeLists.Add(deltaTime); }
		public void Add_BlendLists(float blend) { blendLists.Add(blend); }
		public void Add_IsLadderLists(bool isLadder) { IsLadderLists.Add(isLadder); }
		public void Add_PlayerStateLists(PlayerState playerState) { playerStateLists.Add(playerState); }
		public void Add_ActionTypeLists(int actionType) { actionTypeLists.Add(actionType); }

		//--- ゲッタ ---//
		public Vector3 Get_PosLists(int index) { return posLists[index]; }
		public Quaternion Get_RotLists(int index) { return rotLists[index]; }
		public float Get_DeltaTimeLists(int index) { return deltaTimeLists[index]; }
		public float Get_BlendLists(int index) { return blendLists[index]; }
		public bool Get_IsLadderLists(int index) { return IsLadderLists[index]; }
		public PlayerState Get_PlayerStateLists(int index) { return playerStateLists[index]; }
		public int Get_ActionTypeLists(int index) { return actionTypeLists[index]; }

		//--- リストのクリア ---//
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

		//--- リスト内の要素数を返す（代表してposLists） ---//
		public int Count_Lists() { return posLists.Count; }
	}
	#endregion

	#region serialize field
	[SerializeField] private StageController _StageManager;

	//　保存するデータの最大数
	[SerializeField] private int _MaxDataNum = 2000;
	//　記録間隔
	[SerializeField] private float _RecordDuration = 0.005f;
	#endregion

	#region field
	/// <summary> プレイヤーにアタッチされたコンポーネントを取得するための変数群 </summary>
	private GameObject _Player;
	private PlayerBehaviour _PlayerController;
	private Animator _PlayerAnimator;

	/// <summary> 残像にアタッチされたコンポーネントを取得を取得するための変数群 </summary>
	private GameObject _Afterimage;
	private AfterimageBehaviour _AfterimageController;

	/// <summary> データの保存に必要な変数群 </summary>
	//　現在記憶しているかどうか
	private bool _IsRecord;

	//　経過時間
	private float _ElapsedTime = 0f;
	//　残像データ
	private AfterimageData[] _AfterimageData;

	public float _AfterimagePlayTime = 5f;
	float _Time;
	bool _TimeCount = true;

	int _RecordSwitcher;
	#endregion

	#region property
	/// <summary>
	/// プレイヤーの挙動が保存されたクラスにアクセスするためのインデクサ
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
		/// <summary> プレイヤーのデータを取得する </summary>
		_Player = _StageManager._Player;
		_PlayerController = _Player.GetComponent<PlayerBehaviour>();
		_PlayerAnimator = _Player.GetComponent<Animator>();

		/// <summary> 残像のデータを取得する </summary>
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

				//　データ保存数が最大数を超えたら保存場所変更
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
	/// <summary> public から private に変更したのでエラーが出たら変更する事 </summary>

	/// <summary>
	/// キャラクターデータの保存
	/// </summary>
	private void StartRecord()
	{
		//　保存する時はゴーストの再生を停止
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
	/// キャラクターデータの保存の停止
	/// </summary>
	private void StopRecord()
	{
		_IsRecord = false;
		Debug.Log("StopRecord");
	}

	/// <summary>
	/// ゴーストの停止
	/// </summary>
	private void StopGhost()
	{
		Debug.Log("StopGhost");
		StopAllCoroutines();
		_Afterimage.SetActive(false);
	}
	#endregion
}
