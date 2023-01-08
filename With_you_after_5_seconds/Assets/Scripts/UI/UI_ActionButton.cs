using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ActionButton : MonoBehaviour, IButtonComponent
{
    /// <summary> ソースを書くときのレンプレート </summary>

    #region define

    #endregion

    #region serialize field
    /// <summary> アクションボタンの画像テーブル </summary>
    [SerializeField] private Sprite[] _Images;
    #endregion

    #region field
    private PlayerBehaviour _Player;
    private ActionType _PrevActionType;

    private Button _Button;
    private AnimatorStateInfo _AnimStateInfo;
    private bool _IsPushed = false;

    // 画像を動的に変えたいボタンの宣言
    private Image _BtnImage;
    #endregion

    #region property

    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _Player = GameObject.Find("Player").GetComponent<PlayerBehaviour>();
        _PrevActionType = ActionType.Default;

        _Button = GetComponent<Button>();
        _AnimStateInfo = _Button.animator.GetCurrentAnimatorStateInfo(0);

        _BtnImage = this.GetComponent<Image>();
        _BtnImage.sprite = _Images[(int)ActionType.Default];
    }

    // Update is called once per frame
    void Update()
    {
        UpdateImage();

        UpdateButtonState();
    }
    #endregion

    #region public function
    public void DebugMessage()
    {
        Debug.Log("Action : " + _Player.ActionType);
    }
    #endregion

    #region private function
    /// <summary>
    /// ボタンに表示するImageの更新
    /// </summary>
    private void UpdateImage()
    {
        // アクションタイプが変わっていたら
        if (_PrevActionType != _Player.ActionType)
        {
            _BtnImage.sprite = _Images[(int)_Player.ActionType];
            _PrevActionType = _Player.ActionType;   // ひとつ前のアクションタイプを保存
        }
    }

    /// <summary>
    /// ボタンが押されているかどうか等の、ボタンの状態を更新
    /// </summary>
    private void UpdateButtonState()
    {
        _AnimStateInfo = _Button.animator.GetCurrentAnimatorStateInfo(0);

        // PushOrPullステート以外なら
        if (_Player.ActionType != ActionType.PushOrPull)
        {
            // 押されていない、且つ、アニメーションが"Pressed"なら、アニメーションを"Normal"に戻す
            if (!_IsPushed && _AnimStateInfo.IsName("Pressed")) _Button.animator.SetTrigger("Normal");
        }
    }

    /// <summary>
    /// Normalステートに変更
    /// </summary>
    private void ToNormal_ButtonState()
    {
        _IsPushed = false;
        _Button.animator.SetTrigger("Normal");
        _Button.animator.ResetTrigger("Pressed");
    }
    #endregion

    #region IPlayGimmickComponent
    /// <summary>
    /// ギミック側から、プレイヤーのメンバ関数を呼び出すためのインターフェイス
    /// </summary>
    /// 
    /// <summary>
    /// ボタンを押したとき
    /// </summary>
    public void GetButtonDown()
    {
        switch (_Player.ActionType)
        {
            case ActionType.Default:
            case ActionType.Button:
            case ActionType.Torch:
                {
                    // 既に押されている状態であればリターン
                    if (_AnimStateInfo.IsName("Pressed")) return;
                    // プレイヤーがアクションボタンを利用できないステートであればリターン
                    if (!(_Player.State == PlayerState.Idle || _Player.State == PlayerState.Move ||  _Player.State == PlayerState.Action)) return;

                    _IsPushed = true;
                    _Button.animator.ResetTrigger("Normal");
                    _Button.animator.SetTrigger("Pressed");
                    Invoke(nameof(ToNormal_ButtonState), 0.15f);
                }
                break;
            case ActionType.PushOrPull:
                {
                    _Button.animator.SetTrigger("Pressed"); _IsPushed = true;
                }
                break;
        }
    }

    /// <summary>
    /// ボタンを話したとき
    /// </summary>
    public void GetButtonUp()
    {
        // PushOrPullステートでなければ処理は行わない
        if (_Player.ActionType != ActionType.PushOrPull) return;
        if (Input.GetButtonUp("Action")) { _Button.animator.SetTrigger("Normal"); _IsPushed = false; }
    }
    #endregion
}
