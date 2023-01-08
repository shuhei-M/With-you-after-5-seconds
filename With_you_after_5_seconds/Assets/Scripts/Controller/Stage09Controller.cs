using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Stage09Controller : MonoBehaviour
{
    /// <summary> �\�[�X�������Ƃ��̃����v���[�g </summary>

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

	/// <summary> Cinemachine�̂��߂̕ϐ��Q </summary>
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

	/// <summary> �v���C���[���ړ������邽�߂̕ϐ��Q </summary>
	private int _TargetPointIndex;
	private CharacterController _PlayerCharaCon;
	private Animator _PlayerAnimator;
	private Vector3 _MoveVec;

	/// <summary> Cinemachine�̂��߂̕ϐ��Q </summary>
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

		// �v���C���[�łȂ���΃��^�[��
		if (player == null) return;

		player.InputSetActive(false);
		_IsStartAutoMode = true;
    }
    #endregion

    #region public function

    #endregion

    #region private function
    /// <summary>
    /// ��Ԃ̕ύX
    /// </summary>
    /// <param name="next">���̏��</param>
    private void ChangeState(StateEnum next)
	{
		// �ȑO�̏�Ԃ�ێ�
		var prev = _State;
		// ���̏�ԂɕύX����
		_State = next;

		// ���O���o��
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
					//// �N���A���p�̃J�����ɐ؂�ւ���
					//_ClearCamera.Priority = 70;

					_MoveVec = GetMoveVec();

					_PlayerAnimator.SetFloat("DeltaTime", 1.0f);
					_PlayerAnimator.SetFloat("MoveBlend", 1.0f);
				}
				break;
			case StateEnum.Stop:
				{
					// �v���C���[�̃A�j���[�V�������A�C�h����Ԃɂ���
					_PlayerAnimator.SetFloat("DeltaTime", 0.0f);
					_PlayerAnimator.SetFloat("MoveBlend", 0.0f);

					// �v���C���[�̈ʒu���A�{��ǂ�ł���ʒu�ɂ���B
					_Player.gameObject.transform.position = _WayPoints[_WayPoints.Length - 1].position;
					_Player.gameObject.transform.LookAt(new Vector3(0.0f, 0.0f, 3.0f));

					// Cinemachine �̃Z�b�g�A�b�v
					// _ClearCamera.LookAt = _Player.gameObject.transform;
					_StopCamera.Priority = 70;

					// 0.5�b��ɖ{����p�[�e�B�N���𔭂���
					// �R���[�`���̋N��
					StartCoroutine(DelayCoroutine(_StopTime - 2.0f, () =>
					{
						Vector3 generatePos = _TrackingTarget.position + new Vector3(0.0f, 0.8f, 0.0f);
						EffectManager.Instance.Play(EffectManager.EffectID.BookLight, generatePos);
					}));

					// 1�b��ɃJ�����̗�����ړ����J�n����
					// �R���[�`���̋N��
					StartCoroutine(DelayCoroutine(_StopTime, () =>
					{
						// n�b��ɂ����̏��������s�����
						ChangeState(StateEnum.SpiralCamera);
					}));
				}
				break;
			case StateEnum.SpiralCamera:
				{
					// �N���A���p�̃J�����ɐ؂�ւ���
					_ClearCamera.Priority = 80;

					// BGM���Đ�����
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
	/// ��Ԗ��̖��t���[���Ă΂�鏈��
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
						// �R���[�`���̋N��
						StartCoroutine(DelayCoroutine(2.0f, () =>
						{
							// n�b��ɂ����̏��������s�����
							ChangeState(StateEnum.EndingCutIn);
						}));
						_IsFinish = true;
					}
				}
				break;
			case StateEnum.EndingCutIn:
				{
					// �t�F�[�h�A�E�g����������
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
	/// ��莞�Ԍ�ɏ������Ăяo���R���[�`��
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
	/// �ړI�n�փv���C���[�𓙑��ňړ�������
	/// </summary>
	private void PlayerMoveToWayPoint()
    {
		_PlayerCharaCon.Move(_MoveVec * _PlayerMoveSpeeds[_TargetPointIndex] * Time.deltaTime);

		TragetPointUpdate();
	}

	/// <summary>
	/// �v���C���[�̈ړ��x�N�g�������߂�
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
	/// �ړ���̃|�C���g�ɋ߂Â��΁A���̈ړ���ɍ����ւ���B
	/// </summary>
	private void TragetPointUpdate()
    {
		float distance = (_WayPoints[_TargetPointIndex].position - _Player.gameObject.transform.position).magnitude;

		// �߂Â��Ă��Ȃ���΁A�ȉ��̏����͍s��Ȃ�
		if (distance > 0.5f) return;

		_TargetPointIndex++;

		// �ŏI�|�C���g�ɂ��Ă����
		if(_TargetPointIndex >= _WayPoints.Length)
        {
			_MoveVec = Vector3.zero;
			ChangeState(StateEnum.Stop);
			return;
        }

		_MoveVec = GetMoveVec();
    }

	/// <summary>
	/// ��ʂ�^�����Ƀt�F�[�h�A�E�g������
	/// </summary>
	private bool TryFadeOutUpdate()
    {
		bool CanUpdate = true;   // �܂�Update�o���邩�ǂ���

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
