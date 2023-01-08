using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Stage09Controller : MonoBehaviour
{
    /// <summary> ソースを書くときのレンプレート </summary>

    #region define
    public enum StateEnum
    {
        None,
        Play,
        AutoMode,
		Stop,
		SpiralCamera,
		EndingCutIn,
    }
    #endregion

    #region serialize field
    [SerializeField] private PlayerBehaviour _Player = null;

	[SerializeField] private Transform[] _WayPoints;
	[SerializeField] private float[] _PlayerMoveSpeeds;

	[SerializeField, Range(1, 5)] float _StopTime = 1.0f;

	/// <summary> Cinemachineのための変数群 </summary>
	[SerializeField] private CinemachineVirtualCamera _StopCamera;
	[SerializeField] private CinemachineVirtualCamera _ClearCamera;
	[SerializeField, Range(0, 1)] private float _DollySpeed;
	[SerializeField] private Transform _TrackingTarget;
	[SerializeField, Range(0, 1)] private float _ChangeTargetLimit = 0.7f;

	[SerializeField] private GameObject _FadeOutPanel;
	#endregion

	#region field
	private StateEnum _State;
	private bool _IsStartAutoMode;
	private bool _IsFinish;

	/// <summary> プレイヤーを移動させるための変数群 </summary>
	private int _TargetPointIndex;
	private CharacterController _PlayerCharaCon;
	private Animator _PlayerAnimator;
	private Vector3 _MoveVec;

	/// <summary> Cinemachineのための変数群 </summary>
	private CinemachineTrackedDolly _Dolly;

	Image _FadeOutImage;
	#endregion

	#region property
	public StateEnum State { get { return _State; } }
    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
		_IsStartAutoMode = false;
		_IsFinish = false;

		_TargetPointIndex = 0;
		_PlayerCharaCon = _Player.gameObject.GetComponent<CharacterController>();
		_PlayerAnimator = _Player.gameObject.GetComponent<Animator>();

		_Dolly = _ClearCamera.GetCinemachineComponent<CinemachineTrackedDolly>();

		_FadeOutImage = _FadeOutPanel.GetComponent<Image>();

		ChangeState(StateEnum.Play);
    }

    // Update is called once per frame
    void Update()
    {
		UpdateState();
	}

    private void OnTriggerEnter(Collider other)
    {
		IPlayGimmick player = other.GetComponent<IPlayGimmick>();

		// プレイヤーでなければリターン
		if (player == null) return;

		player.InputSetActive(false);
		_IsStartAutoMode = true;
    }
    #endregion

    #region public function

    #endregion

    #region private function
    /// <summary>
    /// 状態の変更
    /// </summary>
    /// <param name="next">次の状態</param>
    private void ChangeState(StateEnum next)
	{
		// 以前の状態を保持
		var prev = _State;
		// 次の状態に変更する
		_State = next;

		// ログを出す
		Debug.Log("ChangeState " + prev + "-> " + next);

		switch (_State)
		{
			case StateEnum.None:
				{
				}
				break;
			case StateEnum.Play:
				{
				}
				break;
			case StateEnum.AutoMode:
				{
					//// クリア時用のカメラに切り替える
					//_ClearCamera.Priority = 70;

					_MoveVec = GetMoveVec();

					_PlayerAnimator.SetFloat("DeltaTime", 1.0f);
					_PlayerAnimator.SetFloat("MoveBlend", 1.0f);
				}
				break;
			case StateEnum.Stop:
				{
					// プレイヤーのアニメーションをアイドル状態にする
					_PlayerAnimator.SetFloat("DeltaTime", 0.0f);
					_PlayerAnimator.SetFloat("MoveBlend", 0.0f);

					// プレイヤーの位置を、本を読んでいる位置にする。
					_Player.gameObject.transform.position = _WayPoints[_WayPoints.Length - 1].position;
					_Player.gameObject.transform.LookAt(new Vector3(0.0f, 0.0f, 3.0f));

					// Cinemachine のセットアップ
					// _ClearCamera.LookAt = _Player.gameObject.transform;
					_StopCamera.Priority = 70;

					// 0.5秒後に本からパーティクルを発する
					// コルーチンの起動
					StartCoroutine(DelayCoroutine(_StopTime - 2.0f, () =>
					{
						Vector3 generatePos = _TrackingTarget.position + new Vector3(0.0f, 0.8f, 0.0f);
						EffectManager.Instance.Play(EffectManager.EffectID.BookLight, generatePos);
					}));

					// 1秒後にカメラの螺旋状移動を開始する
					// コルーチンの起動
					StartCoroutine(DelayCoroutine(_StopTime, () =>
					{
						// n秒後にここの処理が実行される
						ChangeState(StateEnum.SpiralCamera);
					}));
				}
				break;
			case StateEnum.SpiralCamera:
				{
					// クリア時用のカメラに切り替える
					_ClearCamera.Priority = 80;

					// BGMを再生する
					Sound.LoadBGM("EndingBGM", "EndingBGM");
					Sound.PlayBGM("EndingBGM", 0.3f);
				}
				break;
			case StateEnum.EndingCutIn:
				{
				}
				break;
		}
	}

	/// <summary>
	/// 状態毎の毎フレーム呼ばれる処理
	/// </summary>
	private void UpdateState()
	{
		switch (_State)
		{
			case StateEnum.None:
				{
				}
				break;
			case StateEnum.Play:
				{
					if (_IsStartAutoMode) ChangeState(StateEnum.AutoMode);
				}
				break;
			case StateEnum.AutoMode:
				{
					PlayerMoveToWayPoint();
					//_Dolly.m_PathPosition += Time.deltaTime * _DollySpeeds[0];
				}
				break;
			case StateEnum.Stop:
				{
					if(Input.GetKeyDown(KeyCode.Backspace)) ChangeState(StateEnum.SpiralCamera);
				}
				break;
			case StateEnum.SpiralCamera:
				{
					_Dolly.m_PathPosition += Time.deltaTime * _DollySpeed;
					if (_Dolly.m_PathPosition > _ChangeTargetLimit)
					{
						_ClearCamera.LookAt = _TrackingTarget;
					}

					if (!_IsFinish && _Dolly.m_PathPosition >= 1.0f)
					{
						// コルーチンの起動
						StartCoroutine(DelayCoroutine(2.0f, () =>
						{
							// n秒後にここの処理が実行される
							ChangeState(StateEnum.EndingCutIn);
						}));
						_IsFinish = true;
					}
				}
				break;
			case StateEnum.EndingCutIn:
				{
					// フェードアウトしきったら
					if (!TryFadeOutUpdate())
					{
						Sound.StopBGM();
						SceneManager.LoadScene("Comic05");
					}
				}
				break;
		}
	}

	/// <summary>
	/// 一定時間後に処理を呼び出すコルーチン
	/// </summary>
	/// <param name="seconds"></param>
	/// <param name="action"></param>
	/// <returns></returns>
	private IEnumerator DelayCoroutine(float seconds, Action action)
	{
		yield return new WaitForSeconds(seconds);
		action?.Invoke();
	}

	/// <summary>
	/// 目的地へプレイヤーを等速で移動させる
	/// </summary>
	private void PlayerMoveToWayPoint()
    {
		_PlayerCharaCon.Move(_MoveVec * _PlayerMoveSpeeds[_TargetPointIndex] * Time.deltaTime);

		TragetPointUpdate();
	}

	/// <summary>
	/// プレイヤーの移動ベクトルを求める
	/// </summary>
	/// <returns></returns>
	private Vector3 GetMoveVec()
    {
		Vector3 moveVec = Vector3.zero;

		moveVec = (_WayPoints[_TargetPointIndex].position - _Player.gameObject.transform.position).normalized;
		_Player.gameObject.transform.LookAt(_WayPoints[_TargetPointIndex]);

		return moveVec;
	}

	/// <summary>
	/// 移動先のポイントに近づけば、次の移動先に差し替える。
	/// </summary>
	private void TragetPointUpdate()
    {
		float distance = (_WayPoints[_TargetPointIndex].position - _Player.gameObject.transform.position).magnitude;

		// 近づいていなければ、以下の処理は行わない
		if (distance > 0.5f) return;

		_TargetPointIndex++;

		// 最終ポイントについていれば
		if(_TargetPointIndex >= _WayPoints.Length)
        {
			_MoveVec = Vector3.zero;
			ChangeState(StateEnum.Stop);
			return;
        }

		_MoveVec = GetMoveVec();
    }

	/// <summary>
	/// 画面を真っ黒にフェードアウトさせる
	/// </summary>
	private bool TryFadeOutUpdate()
    {
		bool CanUpdate = true;   // まだUpdate出来るかどうか

		float alpha = _FadeOutImage.color.a;
		alpha += Time.deltaTime;

		if(alpha > 1.0f)
        {
			alpha = 1.0f;
			CanUpdate = false;
		}

		_FadeOutImage.color = new Color(
			_FadeOutImage.color.r,
			_FadeOutImage.color.g,
			_FadeOutImage.color.b,
			alpha);

		return CanUpdate;
	}
	#endregion
}
