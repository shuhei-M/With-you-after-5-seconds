using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HintButton : MonoBehaviour, IButtonComponent
{
    /// <summary> �\�[�X�������Ƃ��̃����v���[�g </summary>

    #region define
    private enum ShowHint : int
    {
        Close,
        Open,
    }
    #endregion

    #region serialize field
    //�q���g�{�^���̉摜�e�[�u��
    [SerializeField] private  Sprite[] _Images;
    #endregion

    #region field
    private PlayerBehaviour _Player;

    private Button _Button;
    private AnimatorStateInfo _AnimStateInfo;
    private float _IntervalTime = 0.0f;
    private bool _CanUseButton  = true;

    private bool _PrevCanShowHint;

    // �摜�𓮓I�ɕς������{�^���̐錾
    private Image _BtnImage;

    /// <summary> �q���g�M�~�b�N </summary>
    HintGimmick _HintGimmick;
    HintGimmick.StateEnum _CurrentHintState;
    HintGimmick.StateEnum _PrevHintState;
    #endregion

    #region property

    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _Player = GameObject.Find("Player").GetComponent<PlayerBehaviour>();

        _Button = GetComponent<Button>();
        _AnimStateInfo = _Button.animator.GetCurrentAnimatorStateInfo(0);

        _BtnImage = this.GetComponent<Image>();
        _BtnImage.sprite = _Images[(int)ShowHint.Close];
        _PrevCanShowHint = false;

        // �q���g�M�~�b�N�̎擾
        var gimmicks = GameObject.Find("Gimmicks").gameObject;
        var hintGimmickSet = gimmicks.transform.Find("HintGimmick").gameObject;
        _HintGimmick = hintGimmickSet.transform.Find("HintArea").gameObject.GetComponent<HintGimmick>();
        _CurrentHintState = _HintGimmick.State;
    }

    // Update is called once per frame
    void Update()
    {
        _CurrentHintState = _HintGimmick.State;

        //UpdateImage();

        //UpdateButtonState();
        UpdateState();

        _PrevHintState = _CurrentHintState;
    }
    #endregion

    #region public function

    #endregion

    #region private function
    /// <summary>
    /// �{�^���ɕ\������Image�̍X�V
    /// </summary>
    private void UpdateImage()
    {
        if (!_PrevCanShowHint && _HintGimmick.CanShowHint)
        {
            _BtnImage.sprite = _Images[(int)ShowHint.Open];
        }
        else if (_PrevCanShowHint && !_HintGimmick.CanShowHint)
        {
            _BtnImage.sprite = _Images[(int)ShowHint.Close];
        }

        _PrevCanShowHint = _HintGimmick.CanShowHint;
    }

    /// <summary>
    /// �{�^����������Ă��邩�ǂ������́A�{�^���̏�Ԃ��X�V
    /// </summary>
    private void UpdateButtonState()
    {
        _AnimStateInfo = _Button.animator.GetCurrentAnimatorStateInfo(0);

        if (!_CanUseButton) _IntervalTime += Time.deltaTime;
        if(_IntervalTime > 5.0f)
        {
            _IntervalTime = 0.0f;
            _CanUseButton = true;
        }

        switch (_HintGimmick.CanShowHint)
        {
            case true:
                {
                    // �q���g�{�^���������ꂽ��
                    if (Input.GetButtonDown("Hint"))
                    {
                        // ���ɉ�����Ă����Ԃł���΃��^�[��
                        if (_AnimStateInfo.IsName("Pressed")) return;
                        // ���Ƀq���g�I�u�W�F�N�g���o�肵�Ă���ꍇ�ɂ̓��^�[��
                        if (!_CanUseButton) return;
                        // �v���C���[���A�N�V�����{�^���𗘗p�ł��Ȃ��X�e�[�g�ł���΃��^�[��
                        if (!(_Player.State == PlayerState.Idle || _Player.State == PlayerState.Move ||  _Player.State == PlayerState.Hint)) return;

                        _Button.animator.SetTrigger("Pressed");
                        _CanUseButton = false;
                        Invoke(nameof(ToNormal_ButtonState), 0.15f);
                    }
                }
                break;
            case false:
                {
                }
                break;
        }
    }

    /// <summary>
    /// ��Ԃ̕ύX
    /// </summary>
    private void ChangeState()
    {
        //Debug.Log(_CurrentHintState);

        switch (_CurrentHintState)
        {
            case HintGimmick.StateEnum.Locked:
                {
                }
                break;
            case HintGimmick.StateEnum.Disabled:
                {
                    _Button.animator.ResetTrigger("Disabled");
                    _Button.animator.SetTrigger("Normal");
                }
                break;
            case HintGimmick.StateEnum.Useable:
                {
                }
                break;
            case HintGimmick.StateEnum.Finish:
                {
                    _Button.animator.SetTrigger("Pressed");
                     _CanUseButton = false;
                    Invoke(nameof(ToNormal_ButtonState), 0.15f);
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
        
        switch (_CurrentHintState)
        {
            case HintGimmick.StateEnum.Locked:
                {
                    if (!_AnimStateInfo.IsName("Disabled"))
                    {
                        _Button.animator.ResetTrigger("Normal");
                        _Button.animator.SetTrigger("Disabled");
                    }
                }
                break;
            case HintGimmick.StateEnum.Disabled:
                {
                    UpdateImage();
                }
                break;
            case HintGimmick.StateEnum.Useable:
                {
                    UpdateImage();
                }
                break;
            case HintGimmick.StateEnum.Finish:
                {
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
        return (_PrevHintState != _CurrentHintState);
    }

    /// <summary>
    /// Normal�X�e�[�g�ɕύX
    /// </summary>
    private void ToNormal_ButtonState()
    {
        _Button.animator.SetTrigger("Disabled");   // "Normal"
        _Button.animator.ResetTrigger("Pressed");
    }
    #endregion

    #region IPlayGimmickComponent
    /// <summary>
    /// �M�~�b�N������A�v���C���[�̃����o�֐����Ăяo�����߂̃C���^�[�t�F�C�X
    /// </summary>
    /// <summary> �{�^�����������Ƃ� </summary>
    public void GetButtonDown()
    {

    }

    /// <summary> �{�^����b�����Ƃ� </summary>
    public void GetButtonUp()
    {

    }
    #endregion
}
