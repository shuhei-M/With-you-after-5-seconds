using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HintButton : MonoBehaviour, IButtonComponent
{
    /// <summary> ソースを書くときのレンプレート </summary>

    #region define
    private enum ShowHint : int
    {
        Close,
        Open,
    }
    #endregion

    #region serialize field
    //ヒントボタンの画像テーブル
    [SerializeField] private  Sprite[] _Images;
    #endregion

    #region field
    private PlayerBehaviour _Player;

    private Button _Button;
    private AnimatorStateInfo _AnimStateInfo;
    private float _IntervalTime = 0.0f;
    private bool _CanUseButton  = true;

    private bool _PrevCanShowHint;

    // 画像を動的に変えたいボタンの宣言
    private Image _BtnImage;

    /// <summary> ヒントギミック </summary>
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

        // ヒントギミックの取得
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
    /// ボタンに表示するImageの更新
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
    /// ボタンが押されているかどうか等の、ボタンの状態を更新
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
                    // ヒントボタンが押されたら
                    if (Input.GetButtonDown("Hint"))
                    {
                        // 既に押されている状態であればリターン
                        if (_AnimStateInfo.IsName("Pressed")) return;
                        // 既にヒントオブジェクトが出願している場合にはリターン
                        if (!_CanUseButton) return;
                        // プレイヤーがアクションボタンを利用できないステートであればリターン
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
    /// 状態の変更
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
    /// 状態毎の毎フレーム呼ばれる処理
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
    /// ちょうどそのステートに入った所かどうか
    /// </summary>
    /// <returns></returns>
    private bool IsEntryThisState()
    {
        return (_PrevHintState != _CurrentHintState);
    }

    /// <summary>
    /// Normalステートに変更
    /// </summary>
    private void ToNormal_ButtonState()
    {
        _Button.animator.SetTrigger("Disabled");   // "Normal"
        _Button.animator.ResetTrigger("Pressed");
    }
    #endregion

    #region IPlayGimmickComponent
    /// <summary>
    /// ギミック側から、プレイヤーのメンバ関数を呼び出すためのインターフェイス
    /// </summary>
    /// <summary> ボタンを押したとき </summary>
    public void GetButtonDown()
    {

    }

    /// <summary> ボタンを話したとき </summary>
    public void GetButtonUp()
    {

    }
    #endregion
}
