using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アニメーションステートマシンビヘイビア
/// PlayerAnimatorControllerの、PushOrPullステート以外のステートにアタッチ
/// </summary>
public class ASMB_Actions : StateMachineBehaviour
{
    /// <summary> ソースを書くときのレンプレート </summary>

    #region define

    #endregion

    #region serialize field
    private PlayerBehaviour _Player;
    #endregion

    #region field

    #endregion

    #region property

    #endregion

    #region Unity function
    private void Awake()
    {
        _Player = GameObject.Find("Player").gameObject.GetComponent<PlayerBehaviour>();
    }

    //状態が変わった時に実行
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    //毎フレーム実行(※最初と最後のフレームを除く)
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    //状態が終わる時(変わる直前)に実行
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _Player.OnAttackFinish();
    }

    #endregion

    #region public function

    #endregion

    #region private function

    #endregion
}
