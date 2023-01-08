using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using State = StateMachine<PlayerBehaviour>.State;

#region define
/// <summary>
/// 他クラスからプレイヤーのステートを取得するためのenum
/// </summary>
public enum PlayerState : int
{
    Idle,
    Move,
    Action,
    Action_PushOrPull,
    Ride,
    Hint,
    Dead
}

/// <summary>
/// プレイヤーにセットされているアクションタイプ
/// アクションボタンを押すと、セットされたタイプのアクションを行う
/// </summary>
public enum ActionType : int
{
    Default,
    Button,
    Torch,
    PushOrPull
}
#endregion


/// <summary>
/// ステート管理用。
/// </summary>
public partial class PlayerBehaviour
{
    #region define
    /// <summary> ステートマシーンのトランジション用のイベントキー </summary>
    private enum Event : int
    {
        // 移動開始
        StartMove,
        // 移動中止
        StopMove,

        // 攻撃開始
        StartAction,
        // 攻撃終了
        FinishAction,

        // 押す・引く開始
        StartAction_PushOrPll,
        // 押す・引く終了
        FinishAction_PushOrPll,

        // ヒント開始
        StartHint,
        // ヒント終了
        FinishHint,

        // 乗り開始
        StartRide,
        // 乗り中止
        StopRide,

        // 死亡
        Dead,
    }
    #endregion

    #region field
    /// <summary> プレイヤー挙動管理のためのステートマシーン </summary>
    private StateMachine<PlayerBehaviour> _StateMachine;

    /// <summary> どのステートかを示すenum </summary>
    private PlayerState _State;

    /// <summary> セットされているアクションタイプを示すenum </summary>
    private ActionType _ActionType;
    #endregion

    #region property
    /// <summary> 外部からプレイヤーの現在の状態を取得する </summary>
    public PlayerState State { get { return _State; } }

    /// <summary> 外部からセットされたアクションタイプを取得する </summary>
    public ActionType ActionType { get { return _ActionType; } }
    #endregion

    #region public function
    /// <summary>
    /// アクションのモーションが終了したら、StateAttackからStateIdelへ遷移する。
    /// アクションのアニメーションの指定したクリップから呼び出す
    /// </summary>
    public void OnAttackFinish()
    {
        _StateMachine.Dispatch((int)Event.FinishAction);
    }
    #endregion

    #region private function
    /// <summary>
    /// ステートマシンの設定を行う（Startメソッドで呼び出すよう）
    /// </summary>
    private void SetUpStateMachine()
    {
        _StateMachine = new StateMachine<PlayerBehaviour>(this);

        // （Idel→Move）
        _StateMachine.AddTransition<StateIdle, StateMove>((int)Event.StartMove);
        // （Move→Idel）
        _StateMachine.AddTransition<StateMove, StateIdle>((int)Event.StopMove);

        // （Idel→Action）指定のボタンが押されたらアクション開始
        _StateMachine.AddTransition<StateIdle, StateAction>((int)Event.StartAction);
        // （Move→Action）指定のボタンが押されたらアクション開始
        _StateMachine.AddTransition<StateMove, StateAction>((int)Event.StartAction);

        // （Action→Move）攻撃モーションが終了次第、待機に戻る
        _StateMachine.AddTransition<StateAction, StateIdle>((int)Event.FinishAction);

        // （Action→Action_PushOrPll）
        _StateMachine.AddTransition<StateAction, StateAction_PushOrPull>((int)Event.StartAction_PushOrPll);
        // （Move→Action_PushOrPll）
        _StateMachine.AddTransition<StateMove, StateAction_PushOrPull>((int)Event.StartAction_PushOrPll);
        // （Idle→Action_PushOrPll）
        _StateMachine.AddTransition<StateIdle, StateAction_PushOrPull>((int)Event.StartAction_PushOrPll);
        // （Action_PushOrPll→Idle）
        _StateMachine.AddTransition<StateAction_PushOrPull, StateIdle>((int)Event.FinishAction_PushOrPll);

        // （Idel→Hint）指定のボタンが押されたらヒント開始（Idel→Hint）
        _StateMachine.AddTransition<StateIdle, StateHint>((int)Event.StartHint);
        // （Move→Hint）指定のボタンが押されたらヒント開始（Move→Hint）
        _StateMachine.AddTransition<StateMove, StateHint>((int)Event.StartHint);

        // （Hint→Idel）ヒント表示が終了次第、待機に戻る
        _StateMachine.AddTransition<StateHint, StateIdle>((int)Event.FinishHint);

        // （Idel→Ride）
        _StateMachine.AddTransition<StateIdle, StateRide>((int)Event.StartRide);
        // （Ride→Move）
        _StateMachine.AddTransition<StateRide, StateMove>((int)Event.StopRide);

        // 死亡
        _StateMachine.AddAnyTransition<StateDead>((int)Event.Dead);

        _StateMachine.Start<StateIdle>();
        _State = PlayerState.Idle;
    }

    /// <summary>
    /// どのアクションに遷移するか決定する
    /// </summary>
    void ToAnyAction()
    {
        switch (_ActionType)
        {
            case ActionType.Default:
            case ActionType.Button:
            case ActionType.Torch:
                {
                    _StateMachine.Dispatch((int)Event.StartAction);
                }
                break;
            case ActionType.PushOrPull:
                {
                    _StateMachine.Dispatch((int)Event.StartAction_PushOrPll);
                }
                break;
            default:
                Debug.Log("不測の事態 in Actoinステート");
                break;
        }
    }
    #endregion


    #region StateIdle class
    /// <summary>
    /// 待機
    /// </summary>
    private class StateIdle : State
    {
        public StateIdle()
        {
            _name = "Idle";
        }

        protected override void OnEnter(State prevState)
        {
            //Debug.Log("Enter " + Name);
            Owner._State = PlayerState.Idle;
            // Idleステートに入っても、モノを引っ張るアニメーションが再生されている場合は、
            // 強制的にIdleアニメーションに逃がす。
            if(Owner._StateInfo.IsName("PushOrPull")) Owner._Animator.SetTrigger("ToIdle");
            Owner._Animator.ResetTrigger("ToMove");
        }

        protected override void OnUpdate()
        {
            if (Owner._StageManager.RideSencor_.CanRiding)
            {
                StateMachine.Dispatch((int)Event.StartRide);
                return;
            }

            if (Owner._ActionType != ActionType.Default
                && Input.GetButtonDown("Action"))
            {
                // 何かしらのアクションステートへ遷移
                Owner.ToAnyAction();
                return;
            }

            if (!Owner._HintGimmick.IsAliveHint || Owner._IsInputable) Owner.MovePlayer();
            if(Owner._IsInputable) Owner._Animator.SetFloat("DeltaTime", Owner._DeltaTime);

            // Lスティックが一定時間以上傾けられた場合、Moveステートに移動
            if (Owner._DeltaTime >= 0.1f) StateMachine.Dispatch((int)Event.StartMove);

            if (Owner._HintGimmick.IsAliveHint /*&& Input.GetButtonDown("Hint")*/)
                StateMachine.Dispatch((int)Event.StartHint);

            // メインカメラのフォーカス具合を調整する
            Owner.UpdateLensVerticalFOV();

            //if (!HintController.IsAliveHint && Owner._deltaTime >= 0.1f) StateMachine.Dispatch((int)Event.StartMove);
        }

        protected override void OnExit(State nextState)
        {
            //Debug.Log("Exit " + Name);
            Owner._Animator.ResetTrigger("ToIdle");
        }
    }
    #endregion

    #region StateMove class
    /// <summary>
    /// 移動
    /// </summary>
    private class StateMove : State
    {
        public StateMove()
        {
            _name = "Move";
        }

        protected override void OnEnter(State prevState)
        {
            //Debug.Log("Enter " + Name);
            Owner._State = PlayerState.Move;
            // Idleステートに入っても、モノを引っ張るアニメーションが再生されている場合は、
            // 強制的にIdleアニメーションに逃がす。
            Owner._Animator.ResetTrigger("ToAction");
            if (Owner._StateInfo.IsName("PushOrPull")) Owner._Animator.SetTrigger("ToIdle");
            //Owner._animator.ResetTrigger("ToRide");
        }

        protected override void OnUpdate()
        {
            // 梯子上りモーションに切り替えるかどうか
            JudgeClimbLadder();

            if (Owner._ActionType != ActionType.Default
                && Input.GetButtonDown("Action"))
            {
                // 何かしらのアクションステートへ遷移
                Owner.ToAnyAction();
                return;
            }

            Owner.MovePlayer();
            //Owner._animator.SetFloat("DeltaTime", Owner._deltaTime);

            if (Owner._DeltaTime < 0.1f) StateMachine.Dispatch((int)Event.StopMove);

            if (Owner._HintGimmick.IsAliveHint/* && Input.GetButtonDown("Hint")*/)
                StateMachine.Dispatch((int)Event.StartHint);

            // メインカメラのフォーカス具合を調整する
            Owner.UpdateLensVerticalFOV();
        }

        protected override void OnExit(State nextState)
        {
            //Debug.Log("Exit " + Name);
            Owner._Animator.ResetTrigger("ToRide");
        }

        /// <summary>
        /// 梯子を上っているかどうか判定する
        /// 結果に応じて、モーションを切り替える
        /// </summary>
        private void JudgeClimbLadder()
        {
            // 梯子に上り始めたらモーションを切り替える
            if (!Owner._Animator.GetBool("IsLadder") && Owner._IsLadder)
                Owner._Animator.SetBool("IsLadder", true);
            // 梯子から降りたらモーションを切り替える
            else if (Owner._Animator.GetBool("IsLadder") && !Owner._IsLadder)
                Owner._Animator.SetBool("IsLadder", false);
        }
    }
    #endregion

    #region StateAction class
    /// <summary>
    /// アクション
    /// </summary>
    private class StateAction : State
    {
        ActionType action;

        public StateAction()
        {
            _name = "Action";
        }

        protected override void OnEnter(State prevState)
        {
            Owner._Animator.SetInteger("ActionType", (int)Owner._ActionType/*action*/);
            Owner._Animator.SetTrigger("ToAction");
            Owner._State = PlayerState.Action;

            Owner._ActionButton.GetButtonDown();
        }

        protected override void OnUpdate()
        {
            if (!Owner._CharaCon.isGrounded)
            {
                Owner._CharaCon.Move(new Vector3(0, Physics.gravity.y * Time.deltaTime, 0));
            }
        }

        protected override void OnExit(State nextState)
        {
            //Debug.Log("Exit " + Name);
            //Owner._actionType = ActionType.Default;
        }
    }
    #endregion

    #region StateAction_PushOrPull class
    /// <summary>
    /// 押す・引くアクション
    /// </summary>
    private class StateAction_PushOrPull : State
    {
        float time = 0.0f;

        public StateAction_PushOrPull()
        {
            _name = "Action_PushOrPull";
        }

        protected override void OnEnter(State prevState)
        {
            Owner._State = PlayerState.Action_PushOrPull;
            Owner._Animator.SetInteger("ActionType", (int)Owner._ActionType/*action*/);
            Owner._Animator.SetTrigger("ToAction");

            time = 0.0f;
            Owner._ActionButton.GetButtonDown();
        }

        protected override void OnUpdate()
        {
            if (Owner._Velocity.magnitude == 0.0f)
            {
                time += Time.deltaTime;
            }
            else
            {
                time = 0.0f;
            }

            Owner.MovePlayer();
            if (Input.GetButtonUp("Action")
                || Owner._ActionType != ActionType.PushOrPull
                /*|| time > 1.0f*/)
            {
                StateMachine.Dispatch((int)Event.FinishAction_PushOrPll);
            }

        }

        protected override void OnExit(State nextState)
        {
            //Debug.Log("Exit " + Name);
            //Owner._actionType = ActionType.Default;
            Owner._ActionButton.GetButtonUp();
            Owner._Animator.SetTrigger("ToIdle");
        }
    }
    #endregion

    #region StateRide class
    /// <summary>
    /// 乗り
    /// </summary>
    private class StateRide : State
    {
        private Vector3 ridePosition;
        private float inputTime = 0.0f;

        public StateRide()
        {
            _name = "Ride";
        }

        protected override void OnEnter(State prevState)
        {
            //Debug.Log("Enter " + Name);
            Owner._Animator.SetTrigger("ToRide");
            Owner._State = PlayerState.Ride;

            SetRidePosition();

            Owner.transform.position = ridePosition;

            Owner._CharaCon.enabled = false;
        }

        protected override void OnUpdate()
        {
            if (Owner._Velocity.magnitude > 0.2f)
            {
                inputTime += Time.deltaTime;
                if (inputTime > 0.001f)
                {
                    inputTime = 0.0f;
                    StateMachine.Dispatch((int)Event.StopRide);
                    return;
                }
            }
            else
            {
                inputTime = 0.0f;
            }

            //if (Owner._velocity.magnitude > 0.1f)
            //{
            //    StateMachine.Dispatch((int)Event.StopRide);
            //    return;
            //}

            SetRidePosition();
            Owner.transform.position = ridePosition;
        }

        protected override void OnExit(State nextState)
        {
            //Debug.Log("Exit " + Name);
            Owner._CharaCon.enabled = true;
            Owner._Animator.SetTrigger("ToMove");
        }

        private void SetRidePosition()
        {
            ridePosition.x = Owner._StageManager._AfterimageTransform.position.x;
            ridePosition.y = Owner._StageManager._AfterimageTransform.position.y + 2.1f;
            ridePosition.z = Owner._StageManager._AfterimageTransform.position.z;
        }
    }
    #endregion

    #region StateHint class
    /// <summary>
    /// ヒントを見る
    /// </summary>
    private class StateHint : State
    {
        public StateHint()
        {
            _name = "Hint";
        }

        protected override void OnEnter(State prevState)
        {
            Owner._State = PlayerState.Hint;
            Owner._Animator.SetFloat("DeltaTime", 0.0f);
            Owner._Animator.SetFloat("MoveBlend", 0.0f);
        }

        protected override void OnUpdate()
        {
            if (!Owner._HintGimmick.IsAliveHint) StateMachine.Dispatch((int)Event.FinishHint);

            // メインカメラのフォーカス具合を調整する
            Owner.UpdateLensVerticalFOV();
        }

        protected override void OnExit(State nextState)
        {
            //Debug.Log("Exit " + Name);
        }
    }
    #endregion

    #region StateDead class
    //--- 死亡 ---//
    private class StateDead : State
    {
        public StateDead()
        {
            _name = "Dead";
        }

        protected override void OnEnter(State prevState)
        {
            Owner._State = PlayerState.Dead;
            Owner._Animator.SetTrigger("To" + Name);
            //Debug.Log("Enter " + Name);

            Owner._IsAlive = false;
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnExit(State nextState)
        {
            //Debug.Log("Exit " + Name);
        }
    }
    #endregion
}
