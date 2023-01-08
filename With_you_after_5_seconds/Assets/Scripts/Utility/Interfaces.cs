using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region IPlayGimmickComponent
/// <summary>
/// ギミック側から、プレイヤーのメンバ関数を呼び出すためのインターフェイス
/// </summary>
public interface IPlayGimmick
{
    Vector3 Get_InputVelocity();
    Vector3 Get_WorldVelocity();
    Transform Get_Transform();
    void OFF_CharacterController();
    void ON_CharacterController();
    public ActionType ActionTypeP { get; set; }
    PlayerState Get_PlayerState();
    void InputSetActive(bool enabled);
}
#endregion

#region IPlayGimmickComponent
/// <summary>
/// ギミック側から、プレイヤーのメンバ関数を呼び出すためのインターフェイス
/// </summary>
public interface IButtonComponent
{
    /// <summary>
    /// ボタンを押したとき
    /// </summary>
    public void GetButtonDown();

    /// <summary>
    /// ボタンを話したとき
    /// </summary>
    public void GetButtonUp();
}
#endregion
