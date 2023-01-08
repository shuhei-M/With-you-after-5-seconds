using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_WarpEffect : MonoBehaviour
{
    /// <summary> ソースを書くときのレンプレート </summary>

    #region define

    #endregion

    #region serialize field

    #endregion

    #region field
    private bool _IsStoped = true;
    private Image _Image;
    private StageUIScript _StageUI;
    #endregion

    #region property

    #endregion

    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        _Image = this.GetComponent<Image>();
        var color = new Color();
        color.a = 0.0f;
        _Image.color = color;
        _StageUI = transform.parent.gameObject.GetComponent<StageUIScript>();

        StartEffect();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_IsStoped) UpdateEffect();
    }
    #endregion

    #region public function
    /// <summary>
    /// ワープエフェクトを開始する
    /// StageUIScript.cs から呼ぶ
    /// </summary>
    public void StartEffect()
    {
        _Image.material.SetFloat("_Alpha", 0.0f);
        var color = new Color();
        color.a = 0.5f;
        _Image.color = color;
        _IsStoped = false;
    }

    #endregion

    #region private function
    /// <summary>
    /// ワープエフェクトを更新する
    /// ただの紙になったら、終了させる（EndEffectを呼ぶ）
    /// </summary>
    private void UpdateEffect()
    {
        float alpha = _Image.material.GetFloat("_Alpha");
        if (alpha >= 1.0f) 
        {
            _Image.material.SetFloat("_Alpha", 1.0f);
            EndEffect();
            return;
        }

        // クリア画面に遷るまでの時間に合わせて、徐々に出現
        alpha += (Time.deltaTime / _StageUI.ClearStayTime);
        _Image.material.SetFloat("_Alpha", alpha);
    }

    /// <summary>
    /// エフェクトを終了させる
    /// UpdateEffect()から呼ばれる
    /// </summary>
    private void EndEffect()
    {
        var color = new Color();
        color.a = 0.0f;
        _Image.color = color;

        _Image.material.SetFloat("_Alpha", 0.0f);

        _Image.enabled = false;
        _IsStoped = true;
    }
    #endregion
}
