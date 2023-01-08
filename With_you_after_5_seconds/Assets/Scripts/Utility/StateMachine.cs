using System.Collections.Generic;

/// <summary>
/// �X�e�[�g�}�V��
/// </summary>
/// <typeparam name="TOwner">���̃X�e�[�g�}�V�������L����N���X</typeparam>
public class StateMachine<TOwner>
{
    #region define
    /// <summary>
    /// �X�e�[�g��\���N���X
    /// </summary>
    public abstract class State
    {
        // ���̃X�e�[�g�}�V�����Ǘ����Ă���X�e�[�g�}�V��
        protected StateMachine<TOwner> StateMachine => stateMachine;
        internal StateMachine<TOwner> stateMachine;

        // �J�ڂ̈ꗗ
        internal Dictionary<int, State> transitions = new Dictionary<int, State>();

        // ���̃X�e�[�g�̃I�[�i�[
        protected TOwner Owner => stateMachine.Owner;

        /// <summary>
        /// �X�e�[�g�J�n
        /// </summary>
        /// <param name="prevState">�J�n�����X�e�[�g</param>
        internal void Enter(State prevState)
        {
            OnEnter(prevState);
        }

        /// <summary>
        /// �X�e�[�g���J�n�����Ƃ��ɌĂ΂��
        /// </summary>
        /// <param name="prevState">�J�n�����X�e�[�g</param>
        protected virtual void OnEnter(State prevState) { }

        // �X�e�[�g�̍X�V
        internal void Update()
        {
            OnUpdate();
        }

        // ���t���[���Ă΂��
        protected virtual void OnUpdate() { }

        /// <summary>
        /// �X�e�[�g�I��
        /// </summary>
        /// <param name="nextState">���ɑJ�ڂ���X�e�[�g</param>
        internal void Exit(State nextState)
        {
            OnExit(nextState);
        }

        /// <summary>
        /// �X�e�[�g���I�������Ƃ��ɌĂ΂��
        /// </summary>
        /// <param name="nextState">���ɑJ�ڂ���X�e�[�g</param>
        protected virtual void OnExit(State nextState) { }

        // �X�e�[�g�̖��O
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        protected string _name;
    }

    /// <summary>
    /// �ǂ̃X�e�[�g����ł�����̃X�e�[�g�֑J�ڂł���悤�ɂ��邽�߂̃X�e�[�g
    /// </summary>
    public sealed class AnyState : State { }
    #endregion

    #region field
    // �X�e�[�g���X�g
    private LinkedList<State> states = new LinkedList<State>();
    #endregion

    #region property
    // ���̃X�e�[�g�}�V���̃I�[�i�[
    public TOwner Owner { get; }

    // ���݂̃X�e�[�g
    public State CurrentState { get; private set; }
    #endregion


    #region public function
    /// <summary>
    /// �X�e�[�g�}�V��������������
    /// </summary>
    /// <param name="owner">�X�e�[�g�}�V���̃I�[�i�[</param>
    public StateMachine(TOwner owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// �X�e�[�g��ǉ�����i�W�F�l���b�N�Łj
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
    /// ����̃X�e�[�g���擾�A������ΐ�������
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
    /// <typeparam name="TFrom">�J�ڑO�̃X�e�[�g</typeparam>
    /// <typeparam name="TTo">�J�ڐ�̃X�e�[�g</typeparam>
    /// <param name="eventId">�C�x���gID</param>
    public void AddTransition<TFrom, TTo>(int eventId)
        where TFrom : State, new()
        where TTo : State, new()
    {
        var from = GetOrAddState<TFrom>();
        if (from.transitions.ContainsKey(eventId))
        {
            // �����C�x���g��ID���`�ς�
            throw new System.ArgumentException(
                $"�X�e�[�g'{nameof(TFrom)}'�ɑ΂��ăC�x���gID'{eventId.ToString()}'�̑J�ڂ͒�`�ςł�");
        }

        var to = GetOrAddState<TTo>();
        from.transitions.Add(eventId, to);
    }

    /// <summary>
    /// �ǂ̃X�e�[�g����ł�����̃X�e�[�g�֑J�ڂł���C�x���g��ǉ�����
    /// </summary>
    /// <typeparam name="TTo">�J�ڐ�̃X�e�[�g</typeparam>
    /// <param name="eventId">�C�x���gID</param>
    public void AddAnyTransition<TTo>(int eventId) where TTo : State, new()
    {
        AddTransition<AnyState, TTo>(eventId);
    }

    /// <summary>
    /// �X�e�[�g�}�V���̎��s���J�n����i�W�F�l���b�N�Łj
    /// </summary>
    /// <typeparam name="TFirst">�J�n���̃X�e�[�g</typeparam>
    public void Start<TFirst>() where TFirst : State, new()
    {
        Start(GetOrAddState<TFirst>());
    }

    /// <summary>
    /// �X�e�[�g�}�V���̎��s���J�n����
    /// </summary>
    /// <param name="firstState">�J�n���̃X�e�[�g</param>
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
    /// �C�x���g�𔭍s����
    /// </summary>
    /// <param name="eventId">�C�x���gID</param>
    public void Dispatch(int eventId)
    {
        State to;
        if (!CurrentState.transitions.TryGetValue(eventId, out to))
        {
            if (!GetOrAddState<AnyState>().transitions.TryGetValue(eventId, out to))
            {
                // �C�x���g�ɑΉ�����J�ڂ�������Ȃ������B
                return;
            }
        }
        Change(to);
    }
    #endregion


    #region private function
    /// <summary>
    /// �X�e�[�g��ύX����
    /// </summary>
    /// <param name="nextState">�J�ڐ�̃X�e�[�g</param>
    private void Change(State nextState)
    {
        CurrentState.Exit(nextState);
        nextState.Enter(CurrentState);
        CurrentState = nextState;
    }
    #endregion
}
