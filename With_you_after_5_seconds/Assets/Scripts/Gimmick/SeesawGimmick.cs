using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeesawGimmick : MonoBehaviour
{
    #region define
    enum SeesawState : int
    {
        Stop,
        MinusPoint,
        PulsPoint,
        ToBalance,
    }
    #endregion

    #region serialize field
    [SerializeField] private SeesawSencor _PulsPoint;
    [SerializeField] private SeesawSencor _MinusPoint;

    [Header("�V�[�\�[�̓����X�s�[�h")]
    [SerializeField, Range(40.0f, 100.0f)] private float _DownForce = 60.0f;
    #endregion

    #region field
    private float _RotateX;
    //private float _DownForce = 40.0f;
    private float _LimitRotation = 30.0f;

    private SeesawState _SeesawCurrentState;
    private SeesawState _SeesawPrevState;
    private Quaternion _StartQuaternion;
    private float _CurrentRotateX = 0.0f;
    private float _StopArea;
    #endregion

    #region property

    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _RotateX = 0;
        _StartQuaternion = transform.rotation;
        _StopArea = 0.05f;
    }

    // Update is called once per frame
    void Update()
    {
        _RotateX = 0.0f;

        _CurrentRotateX = GetRotateX();

        SetSeesawState();

        UpdateState();
        MoveSeesaw();
        _SeesawPrevState = _SeesawCurrentState;
    }
    #endregion

    #region public function

    #endregion

    #region private function
    /// <summary>
    /// ��Ԃ̕ύX
    /// </summary>
    private void ChangeState()
    {
        // ���O���o��
        Debug.Log("ChangeState " + _SeesawPrevState + "-> " + _SeesawCurrentState);

        switch (_SeesawCurrentState)
        {
            case SeesawState.Stop:
                {
                    transform.rotation = _StartQuaternion;
                }
                break;
            case SeesawState.MinusPoint:
                {
                }
                break;
            case SeesawState.PulsPoint:
                {
                }
                break;
            case SeesawState.ToBalance:
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
        if (IsEntryThisState()) { ChangeState(); return; }

        switch (_SeesawCurrentState)
        {
            case SeesawState.Stop:
                {
                }
                break;
            case SeesawState.MinusPoint:
                {
                    _RotateX -= _DownForce * Time.deltaTime;
                }
                break;
            case SeesawState.PulsPoint:
                {
                    _RotateX += _DownForce * Time.deltaTime;
                }
                break;
            case SeesawState.ToBalance:
                {
                    if (_CurrentRotateX > _StopArea)
                    {
                        _RotateX -= _DownForce * 0.5f * Time.deltaTime;
                        if (_CurrentRotateX + _RotateX < _StopArea)
                        {
                            transform.rotation = _StartQuaternion;
                            _SeesawCurrentState = SeesawState.Stop;
                        }
                    }
                    else if (_CurrentRotateX < -_StopArea)
                    {
                        _RotateX += _DownForce * 0.5f * Time.deltaTime;
                        if (_CurrentRotateX + _RotateX > -_StopArea)
                        {
                            transform.rotation = _StartQuaternion;
                            _SeesawCurrentState = SeesawState.Stop;
                        }
                    }
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
        return (_SeesawPrevState != _SeesawCurrentState);
    }

    private void SetSeesawState()
    {
        bool ToStop = (_CurrentRotateX < _StopArea && _CurrentRotateX > -_StopArea);

        // �V�[�\�[�̗����Ƀv���C���[�Ǝc��������Ă���ꍇ
        if(_MinusPoint.CanRiding && _PulsPoint.CanRiding)
        {
            if (ToStop) { _SeesawCurrentState = SeesawState.Stop; return; }
            _SeesawCurrentState = SeesawState.ToBalance;
        }
        // �Б��ɂ̂ݏ���Ă���@
        else if(_MinusPoint.CanRiding)
        {
            _SeesawCurrentState = SeesawState.MinusPoint;
        }
        // �Б��ɂ̂ݏ���Ă���A
        else if (_PulsPoint.CanRiding)
        {
            _SeesawCurrentState = SeesawState.PulsPoint;
        }
        // �ǂ���ɂ�����Ă��Ȃ��ꍇ
        else if(!_MinusPoint.CanRiding && !_PulsPoint.CanRiding)
        {
            if (ToStop) { _SeesawCurrentState = SeesawState.Stop; return; }
            _SeesawCurrentState = SeesawState.ToBalance;
        }

        //// Stop�X�e�[�g����̑J��
        //if (_SeesawPrevState == SeesawState.Stop)
        //{
        //    if (_MinusPoint.CanRiding)
        //    {
        //        _SeesawCurrentState = SeesawState.MinusPoint;
        //    }
        //    else if (_PulsPoint.CanRiding)
        //    {
        //        _SeesawCurrentState = SeesawState.PulsPoint;
        //    }
        //}
        //// �V�[�\�[�̕Б��ɂ̂݉���������Ă���X�e�[�g����̑J��
        //else if (_SeesawPrevState == SeesawState.MinusPoint
        //     || _SeesawPrevState == SeesawState.PulsPoint)
        //{
        //    if ((_MinusPoint.CanRiding && _PulsPoint.CanRiding)
        //    ||!(_MinusPoint.CanRiding || _PulsPoint.CanRiding))
        //    {
        //        _SeesawCurrentState = SeesawState.ToBalance;
        //    }
        //}
        //// �߂�X�e�[�g����̑J��
        //else if (_SeesawPrevState == SeesawState.ToBalance)
        //{
        //    if (ToStop)
        //    {
        //        transform.rotation = _StartQuaternion;
        //        _SeesawCurrentState = SeesawState.Stop;
        //    }
        //}
    }

    private float GetRotateX()
    {
        Vector3 rotationVec = transform.rotation.eulerAngles;
        float currentRotateX = rotationVec.x;
        if (currentRotateX >= 180.0f) currentRotateX = currentRotateX - 360.0f;   // �p�x��-180�`180�͈̔͂Ɏ��߂�
        return currentRotateX;
    }

    private void MoveSeesaw()
    {
        if (_SeesawCurrentState == SeesawState.Stop) return;

        float currentRotateX = GetRotateX();
        // ��]�����𒴂��Ă��Ȃ���Ή�]
        if ((_RotateX > 0.0f && currentRotateX <= _LimitRotation)
            || (_RotateX < 0.0f && currentRotateX >= -_LimitRotation))
        {
            transform.Rotate(_RotateX, 0, 0);
        }
        else
        {
            Vector3 localAngle = transform.localEulerAngles;
            Debug.Log(localAngle.x);
        }
    }
    #endregion
}
