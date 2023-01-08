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

    [Header("シーソーの動くスピード")]
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
    /// 状態の変更
    /// </summary>
    private void ChangeState()
    {
        // ログを出す
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
    /// 状態毎の毎フレーム呼ばれる処理
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
    /// ちょうどそのステートに入った所かどうか
    /// </summary>
    /// <returns></returns>
    private bool IsEntryThisState()
    {
        return (_SeesawPrevState != _SeesawCurrentState);
    }

    private void SetSeesawState()
    {
        bool ToStop = (_CurrentRotateX < _StopArea && _CurrentRotateX > -_StopArea);

        // シーソーの両側にプレイヤーと残像が乗っている場合
        if(_MinusPoint.CanRiding && _PulsPoint.CanRiding)
        {
            if (ToStop) { _SeesawCurrentState = SeesawState.Stop; return; }
            _SeesawCurrentState = SeesawState.ToBalance;
        }
        // 片側にのみ乗っている①
        else if(_MinusPoint.CanRiding)
        {
            _SeesawCurrentState = SeesawState.MinusPoint;
        }
        // 片側にのみ乗っている②
        else if (_PulsPoint.CanRiding)
        {
            _SeesawCurrentState = SeesawState.PulsPoint;
        }
        // どちらにも乗っていない場合
        else if(!_MinusPoint.CanRiding && !_PulsPoint.CanRiding)
        {
            if (ToStop) { _SeesawCurrentState = SeesawState.Stop; return; }
            _SeesawCurrentState = SeesawState.ToBalance;
        }

        //// Stopステートからの遷移
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
        //// シーソーの片側にのみ何かが乗っているステートからの遷移
        //else if (_SeesawPrevState == SeesawState.MinusPoint
        //     || _SeesawPrevState == SeesawState.PulsPoint)
        //{
        //    if ((_MinusPoint.CanRiding && _PulsPoint.CanRiding)
        //    ||!(_MinusPoint.CanRiding || _PulsPoint.CanRiding))
        //    {
        //        _SeesawCurrentState = SeesawState.ToBalance;
        //    }
        //}
        //// 戻りステートからの遷移
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
        if (currentRotateX >= 180.0f) currentRotateX = currentRotateX - 360.0f;   // 角度を-180～180の範囲に収める
        return currentRotateX;
    }

    private void MoveSeesaw()
    {
        if (_SeesawCurrentState == SeesawState.Stop) return;

        float currentRotateX = GetRotateX();
        // 回転制限を超えていなければ回転
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
