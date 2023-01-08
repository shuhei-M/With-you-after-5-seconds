using System.Collections.Generic;

/// <summary>
/// ステートマシン
/// </summary>
/// <typeparam name="TOwner">このステートマシンを所有するクラス</typeparam>
public class StateMachine<TOwner>
{
    #region define
    /// <summary>
    /// ステートを表すクラス
    /// </summary>
    public abstract class State
    {
        // このステートマシンを管理しているステートマシン
        protected StateMachine<TOwner> StateMachine => stateMachine;
        internal StateMachine<TOwner> stateMachine;

        // 遷移の一覧
        internal Dictionary<int, State> transitions = new Dictionary<int, State>();

        // このステートのオーナー
        protected TOwner Owner => stateMachine.Owner;

        /// <summary>
        /// ステート開始
        /// </summary>
        /// <param name="prevState">開始されるステート</param>
        internal void Enter(State prevState)
        {
            OnEnter(prevState);
        }

        /// <summary>
        /// ステートを開始したときに呼ばれる
        /// </summary>
        /// <param name="prevState">開始されるステート</param>
        protected virtual void OnEnter(State prevState) { }

        // ステートの更新
        internal void Update()
        {
            OnUpdate();
        }

        // 毎フレーム呼ばれる
        protected virtual void OnUpdate() { }

        /// <summary>
        /// ステート終了
        /// </summary>
        /// <param name="nextState">次に遷移するステート</param>
        internal void Exit(State nextState)
        {
            OnExit(nextState);
        }

        /// <summary>
        /// ステートを終了したときに呼ばれる
        /// </summary>
        /// <param name="nextState">次に遷移するステート</param>
        protected virtual void OnExit(State nextState) { }

        // ステートの名前
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        protected string _name;
    }

    /// <summary>
    /// どのステートからでも特定のステートへ遷移できるようにするためのステート
    /// </summary>
    public sealed class AnyState : State { }
    #endregion

    #region field
    // ステートリスト
    private LinkedList<State> states = new LinkedList<State>();
    #endregion

    #region property
    // このステートマシンのオーナー
    public TOwner Owner { get; }

    // 現在のステート
    public State CurrentState { get; private set; }
    #endregion


    #region public function
    /// <summary>
    /// ステートマシンを初期化する
    /// </summary>
    /// <param name="owner">ステートマシンのオーナー</param>
    public StateMachine(TOwner owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// ステートを追加する（ジェネリック版）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Add<T>() where T : State, new()
    {
        var state = new T();
        state.stateMachine = this;
        states.AddLast(state);
        return state;
    }

    /// <summary>
    /// 特定のステートを取得、無ければ生成する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetOrAddState<T>() where T : State, new()
    {
        foreach (var state in states)
        {
            if (state is T result)
            {
                return result;
            }
        }
        return Add<T>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TFrom">遷移前のステート</typeparam>
    /// <typeparam name="TTo">遷移先のステート</typeparam>
    /// <param name="eventId">イベントID</param>
    public void AddTransition<TFrom, TTo>(int eventId)
        where TFrom : State, new()
        where TTo : State, new()
    {
        var from = GetOrAddState<TFrom>();
        if (from.transitions.ContainsKey(eventId))
        {
            // 同じイベントのIDを定義済み
            throw new System.ArgumentException(
                $"ステート'{nameof(TFrom)}'に対してイベントID'{eventId.ToString()}'の遷移は定義済です");
        }

        var to = GetOrAddState<TTo>();
        from.transitions.Add(eventId, to);
    }

    /// <summary>
    /// どのステートからでも特定のステートへ遷移できるイベントを追加する
    /// </summary>
    /// <typeparam name="TTo">遷移先のステート</typeparam>
    /// <param name="eventId">イベントID</param>
    public void AddAnyTransition<TTo>(int eventId) where TTo : State, new()
    {
        AddTransition<AnyState, TTo>(eventId);
    }

    /// <summary>
    /// ステートマシンの実行を開始する（ジェネリック版）
    /// </summary>
    /// <typeparam name="TFirst">開始時のステート</typeparam>
    public void Start<TFirst>() where TFirst : State, new()
    {
        Start(GetOrAddState<TFirst>());
    }

    /// <summary>
    /// ステートマシンの実行を開始する
    /// </summary>
    /// <param name="firstState">開始時のステート</param>
    public void Start(State firstState)
    {
        CurrentState = firstState;
        CurrentState.Enter(null);
    }

    public void Update()
    {
        CurrentState.Update();
    }

    /// <summary>
    /// イベントを発行する
    /// </summary>
    /// <param name="eventId">イベントID</param>
    public void Dispatch(int eventId)
    {
        State to;
        if (!CurrentState.transitions.TryGetValue(eventId, out to))
        {
            if (!GetOrAddState<AnyState>().transitions.TryGetValue(eventId, out to))
            {
                // イベントに対応する遷移が見つからなかった。
                return;
            }
        }
        Change(to);
    }
    #endregion


    #region private function
    /// <summary>
    /// ステートを変更する
    /// </summary>
    /// <param name="nextState">遷移先のステート</param>
    private void Change(State nextState)
    {
        CurrentState.Exit(nextState);
        nextState.Enter(CurrentState);
        CurrentState = nextState;
    }
    #endregion
}
