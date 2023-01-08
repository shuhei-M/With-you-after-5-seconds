using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UI_Effect : MonoBehaviour
{
    #region DEFINE
    protected enum FadeState
    {
        None = -1,
        Brack,
        Title,
        StageSelect,
        HowToPlay,
        Reset,
    }
    #endregion


    #region PRIVATE MEMBER
    private float flashTime = 0f;
    #endregion


    #region Coroutine
    //protected IEnumerator FadeOut(GameObject FadePanel, float time)
    //{
    //    var color = FadePanel.GetComponent<Image>().color;
    //    var red = color.r;
    //    var green = color.g;
    //    var blue = color.b;
    //    var alpha = color.a;

    //    float current = 0;
    //    while (current < time)
    //    {
    //        //material.SetFloat("_Alpha", current / time);
    //        alpha += current / time;
    //        FadePanel.GetComponent<Image>().color = new Color(red, green, blue, alpha);
    //        yield return new WaitForEndOfFrame();
    //        current += Time.deltaTime;
    //    }
    //    FadePanel.GetComponent<Image>().color = new Color(red, green, blue, 1);
    //}

    //protected IEnumerator InAnimate(Material material, float time)
    //{
    //    var image = GetComponent<Image>();
    //    image.sprite = _sprites[spriteNum];
    //    GetComponent<Image>().material = material;
    //    float current = 0;
    //    while (current < time)
    //    {
    //        material.SetFloat("_Alpha", 1 - current / time);
    //        yield return new WaitForEndOfFrame();
    //        current += Time.deltaTime;
    //    }
    //    material.SetFloat("_Alpha", 0);

    //    FadeInOK = true;
    //    gameObject.SetActive(false);
    //}
    #endregion


    #region PROTECTED METHOD
    //---------- あそびかた画面時の処理 ----------//

    /// <summary>
    /// 次のページへ移動
    /// </summary>
    /// <param name="howToPlayPageScreen"> あそびかた画面たち </param>
    protected void GoNextPageHTP(ref GameObject[] howToPlayPageScreen)
    {
        for (int i = 0; i < howToPlayPageScreen.Length - 1; i++)
        {
            if (howToPlayPageScreen[i].activeInHierarchy)
            {
                howToPlayPageScreen[i + 1].SetActive(true);
                howToPlayPageScreen[i].SetActive(false);
                return;
            }
        }
    }

    /// <summary>
    /// 前のページへ移動
    /// </summary>
    /// <param name="howToPlayPageScreen"> あそびかた画面たち </param>
    protected void GoBackPageHTP(ref GameObject[] howToPlayPageScreen)
    {
        for (int i = 1; i < howToPlayPageScreen.Length; i++)
        {
            if (howToPlayPageScreen[i].activeInHierarchy)
            {
                howToPlayPageScreen[i - 1].SetActive(true);
                howToPlayPageScreen[i].SetActive(false);
                return;
            }
        }
    }

    /// <summary>
    /// あそびかた画面のアクティブ状態をリセット（エフェクト状態もリセット予定）
    /// </summary>
    /// <param name="howToPlayPageScreen"> あそびかた画面たち </param>
    protected void HowToPlayPageScreenReset(ref GameObject[] howToPlayPageScreen)
    {
        for (int i = 0; i < howToPlayPageScreen.Length; i++)
        {
            // 1ページ目がアクティブ状態ならそのまま戻る
            if (howToPlayPageScreen[i].activeInHierarchy && i == 0)
                return;

            // 2ページ目以降がアクティブ状態なら1ページ目のみアクティブ化して戻る
            if (howToPlayPageScreen[i].activeInHierarchy)
            {
                howToPlayPageScreen[0].SetActive(true);
                howToPlayPageScreen[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// あそびかた画面のエフェクト処理をリセット
    /// </summary>
    /// <param name="howToPlayScreen"> あそびかた画面たち </param>
    protected void HowToPlayEffectReset(ref GameObject howToPlayScreen)
    {
        UIAlphaOffReset(ref howToPlayScreen, false);

        for (int i = 0; i < howToPlayScreen.transform.childCount; i++)
        {
            var childObj = howToPlayScreen.transform.GetChild(i).gameObject;

            if (i == 0 || i == howToPlayScreen.transform.childCount - 1)
            {
                UIAlphaOffReset(ref childObj, true);

                if (childObj.transform.childCount != 0)
                {
                    for(int j = 0; j< childObj.transform.childCount; j++)
                    {
                        var obj = childObj.transform.GetChild(j).gameObject;
                        UIAlphaOffReset(ref obj, true);
                    }
                }
            }
            else
            {
                UIAlphaOnReset(ref childObj, false);

                if (childObj.transform.childCount != 0)
                {
                    for (int j = 0; j < childObj.transform.childCount; j++)
                    {
                        var obj = childObj.transform.GetChild(j).gameObject;
                        UIAlphaOnReset(ref obj, true);
                    }
                }
            }
        }
    }
    #endregion


    #region PROTECTED EFFECT METHOD
    /// <summary>
    /// UI のアルファ値を 1 に変更する関数
    /// </summary>
    /// <param name="UIs"> アルファ値を 1 にするUIオブジェクト </param>
    protected void ShowAlphaUI(ref GameObject UIs)
    {
        for (int i = 0; i < UIs.transform.childCount; i++)
        {
            var img = UIs.transform.GetChild(i).gameObject.GetComponent<Image>();
            var col = img.color;
            var r = col.r;
            var g = col.g;
            var b = col.b;
            var a = 1f;
            img.color = new Color(r, g, b, a);
        }
    }

    /// <summary>
    /// UIを切り替える関数(順番にフェードイン → フェードアウト)
    /// </summary>
    /// <param name="OutObj">    フェードアウトする UI オブジェクトたち </param>
    /// <param name="InObj">     フェードインする UI オブジェクトたち </param>
    /// <param name="fadeState"> フェードステート（状態）</param>
    /// <param name="fadeSec">   演出処理にかける時間（フェードインする時間 × フェードアウトする時間）</param>
    /// <param name="timeScale"> 時間を止めているか true = 止めている　false = 動いている </param>
    protected void UIToFade(ref GameObject OutObj, ref GameObject InObj, ref FadeState fadeState, float fadeSec, bool timeScale = false)
    {
        var outObjFirstChildImg = OutObj.transform.GetChild(0).gameObject.GetComponent<Image>();
        var inObjFirstChildImg = InObj.transform.GetChild(0).gameObject.GetComponent<Image>();

        if (outObjFirstChildImg.color.a > 0)
        {   // 表示中の UI を先にフェードアウト
            FadeOutUIs(ref OutObj, fadeSec, timeScale);
        }
        else
        {   // フェードアウト処理が終わり次第次表示する UI をフェードイン
            if (OutObj.activeInHierarchy)
            {
                OutObj.SetActive(false);
                InObj.SetActive(true);
            }

            FadeInUIs(ref InObj, fadeSec, timeScale);

            if (inObjFirstChildImg.color.a >= 1)
            {   // UI 入れ替え処理が終わったら状態をかえる
                fadeState = FadeState.None;
            }
        }
    }

    /// <summary>
    /// Panelを切り替える関数(同時にフェードイン・フェードアウト)
    /// </summary>
    /// <param name="OutObj">       フェードアウトする UI オブジェクト </param>
    /// <param name="InObj">        フェードインする UI オブジェクト </param>
    /// <param name="fadeState">    この処理を行うかのフラグ。処理終了次第フラグを落とす。</param>
    /// <param name="backImgObj">   OutObj と InObj で背面にあたるオブジェクト</param>
    /// <param name="fadeSec">      演出処理にかける時間 </param>
    /// <param name="outObjActive"> 背面のUIを残すか（true: 残す, false: 消す）</param>
    /// <param name="timeScale">    時間を止めているか true = 止めている　false = 動いている </param>
    protected void PanelToFade(ref GameObject OutObj, ref GameObject InObj, ref FadeState fadeState, GameObject backImgObj, float fadeSec, bool outObjActive, bool timeScale = false)
    {
        var outObjFirstChildImg = OutObj.gameObject.GetComponent<Image>();
        var inObjFirstChildImg = InObj.gameObject.GetComponent<Image>();

        if (!InObj.activeInHierarchy)
        {
            InObj.SetActive(true);
        }

        if (InObj == backImgObj)
        {
            if (outObjFirstChildImg.color.a > 0)
            {   // タイトルじゃない画面をフェードイン・フェードアウト
                FadeOutPanel(ref OutObj, fadeSec, false, timeScale);
            }
            else
            {
                if (!outObjActive)
                    OutObj.SetActive(false);
                fadeState = FadeState.None;
            }
        }
        else
        {
            if (inObjFirstChildImg.color.a < 1)
            {   // タイトルじゃない画面をフェードイン・フェードアウト
                FadeInPanel(ref InObj, fadeSec, false, timeScale);
            }
            else
            {
                if (!outObjActive)
                    OutObj.SetActive(false);
                fadeState = FadeState.None;
            }
        }
    }

    /// <summary>
    /// UIのフェードアウト処理
    /// </summary>
    /// <param name="OutObj">  フェードアウトする UI オブジェクトたち </param>
    /// <param name="fadeSec"> 演出処理にかける時間 </param>
    /// <param name="timeScale"> 時間を止めているか true = 止めている　false = 動いている </param>
    protected void FadeOutUIs(ref GameObject OutObj, float fadeSec, bool timeScale = false)
    {
        bool firstUIFadeOk = false;

        for (int i = 0; i < OutObj.transform.childCount; i++)
        {
            var childObj = OutObj.transform.GetChild(i).gameObject;

            if (firstUIFadeOk)
            {
                var img = childObj.GetComponent<Image>();
                var red = img.color.r;
                var green = img.color.g;
                var blue = img.color.b;
                var alpha = 0f;

                img.color = new Color(red, green, blue, alpha);
            }
            else
            {
                FadeOutUI(ref childObj, fadeSec, timeScale);

                var alpha = childObj.GetComponent<Image>().color.a;
                if (alpha <= 0f)
                {
                    firstUIFadeOk = true;
                }
            }
        }
    }

    /// <summary>
    /// 1 つの UI だけフェードアウトする
    /// </summary>
    /// <param name="OutObj">  フェードアウトする UI オブジェクト </param>
    /// <param name="fadeSec"> 演出処理にかける時間 </param>
    /// <param name="timeScale"> 時間を止めているか true = 止めている　false = 動いている </param>
    protected void FadeOutUI(ref GameObject OutObj, float fadeSec, bool timeScale = false)
    {
        var img = OutObj.GetComponent<Image>();
        var red = img.color.r;
        var green = img.color.g;
        var blue = img.color.b;
        var alpha = img.color.a;

        if (timeScale)
        {
            alpha -= Time.unscaledDeltaTime / (fadeSec / 2);
        }
        else
        {
            alpha -= Time.deltaTime / (fadeSec / 2);
        }
        

        if (alpha <= 0f)
        {
            alpha = 0f;
        }

        img.color = new Color(red, green, blue, alpha);
    }

    /// <summary>
    /// UIのフェードイン処理
    /// </summary>
    /// <param name="InObj">   フェードインする UI オブジェクトたち </param>
    /// <param name="fadeSec"> 演出処理にかける時間 </param>
    /// <param name="timeScale"> 時間を止めているか true = 止めている　false = 動いている </param>
    protected void FadeInUIs(ref GameObject InObj, float fadeSec, bool timeScale = false)
    {
        bool firstUIFadeOk = false;

        for (int i = 0; i < InObj.transform.childCount; i++)
        {
            var childObj = InObj.transform.GetChild(i).gameObject;
            

            if (firstUIFadeOk)
            {
                var img = childObj.GetComponent<Image>();
                var red = img.color.r;
                var green = img.color.g;
                var blue = img.color.b;
                var alpha = 1f;

                img.color = new Color(red, green, blue, alpha);
            }
            else
            {
                FadeInUI(ref childObj, fadeSec, timeScale);

                var alpha = childObj.GetComponent<Image>().color.a;

                if (alpha >= 1)
                {
                    firstUIFadeOk = true;
                }
            }
        }
    }

    /// <summary>
    /// 1 つの UI だけフェードインする
    /// </summary>
    /// <param name="OutObj">  フェードインする UI オブジェクトたち </param>
    /// <param name="fadeSec"> 演出処理にかける時間 </param>
    /// <param name="timeScale"> 時間を止めているか true = 止めている　false = 動いている </param>
    protected void FadeInUI(ref GameObject OutObj, float fadeSec, bool timeScale = false)
    {
        var img = OutObj.GetComponent<Image>();
        var red = img.color.r;
        var green = img.color.g;
        var blue = img.color.b;
        var alpha = img.color.a;

        if (timeScale)
        {
            alpha += Time.unscaledDeltaTime / (fadeSec / 2);
        }
        else
        {
            alpha += Time.deltaTime / (fadeSec / 2);
        }

        if (alpha >= 1f)
        {
            alpha = 1f;
        }

        img.color = new Color(red, green, blue, alpha);
    }

    /// <summary>
    /// UI 点滅処理
    /// </summary>
    /// <param name="flashingImg">       点滅する UI の Image </param>
    /// <param name="flashingRepetTime"> 点滅（消えてつくまでの）時間 </param>
    /// <param name="timeScale"> 時間を止めているか true = 止めている　false = 動いている </param>
    protected void FlashingUI(ref Image flashingImg, float flashingRepetTime, bool timeScale = false)
    {
        var r = flashingImg.color.r;
        var g = flashingImg.color.g;
        var b = flashingImg.color.b;
        var a = flashingImg.color.a;

        if (timeScale)
        {
            flashTime += Time.unscaledDeltaTime;
        }
        else
        {
            flashTime += Time.deltaTime;
        }

        if (flashTime > flashingRepetTime)
        {
            a = 1f;
            flashingImg.color = new Color(r, g, b, a);

            flashTime = 0f;
        }
        else if (flashTime > flashingRepetTime / 2)
        {
            if (timeScale)
            {
                a += Time.unscaledDeltaTime / (flashingRepetTime / 2);
            }
            else
            {
                a += Time.deltaTime / (flashingRepetTime / 2);
            }
            flashingImg.color = new Color(r, g, b, a);
        }
        else
        {
            if (timeScale)
            {
                a -= Time.unscaledDeltaTime / (flashingRepetTime / 2);
            }
            else
            {
                a -= Time.deltaTime / (flashingRepetTime / 2);
            }
            flashingImg.color = new Color(r, g, b, a);
        }
    }

    /// <summary>
    /// Panelのフェードアウト処理
    /// </summary>
    /// <param name="OutObj">  フェードアウトする UI オブジェクト </param>
    /// <param name="fadeSec"> 演出処理にかける時間 </param>
    /// <param name="inner">   一番親のオブジェクト（大元のPanel）がこの処理を行っているか </param>
    /// <param name="timeScale"> 時間を止めているか true = 止めている　false = 動いている </param>
    protected void FadeOutPanel(ref GameObject OutObj, float fadeSec, bool inner, bool timeScale = false)
    {
        bool firstUIFadeOk = false;

        if (!inner)
        {
            var outObjImg = OutObj.GetComponent<Image>();

            var outObjRed = outObjImg.color.r;
            var outObjGreen = outObjImg.color.g;
            var outObjBlue = outObjImg.color.b;
            var outObjAlpha = outObjImg.color.a;

            if (firstUIFadeOk)
            {
                outObjAlpha = 0f;
                outObjImg.color = new Color(outObjRed, outObjGreen, outObjBlue, outObjAlpha);
            }
            else
            {
                if (timeScale)
                {
                    outObjAlpha -= Time.unscaledDeltaTime / (fadeSec / 2);
                }
                else
                {
                    outObjAlpha -= Time.deltaTime / (fadeSec / 2);
                }

                if (outObjAlpha <= 0)
                {
                    outObjAlpha = 0f;
                    firstUIFadeOk = true;
                }

                outObjImg.color = new Color(outObjRed, outObjGreen, outObjBlue, outObjAlpha);
            }
        }

        for (int i = 0; i < OutObj.transform.childCount; i++)
        {
            var childObj = OutObj.transform.GetChild(i).gameObject;

            if (OutObj.transform.childCount > 0)
            {   // 子オブジェクトがいればそのオブジェクトもフェードアウト
                FadeOutPanel(ref childObj, fadeSec, true, timeScale);
            }
            if (!childObj.activeInHierarchy)
            {   // アクティブではないオブジェクトは処理の必要なし
                continue;
            }

            var vp = childObj.GetComponent<VideoPlayer>();
            if (vp != null)
            {
                childObj.SetActive(false);
            }

            var img = childObj.GetComponent<Image>();

            if (img == null)
            {   // Imageがないのであれば処理の必要なし
                continue;
            }

            var red = img.color.r;
            var green = img.color.g;
            var blue = img.color.b;
            var alpha = img.color.a;

            if (firstUIFadeOk)
            {
                alpha = 0f;
                img.color = new Color(red, green, blue, alpha);
            }
            else
            {
                if (timeScale)
                {
                    alpha -= Time.unscaledDeltaTime / (fadeSec / 2);
                }
                else
                {
                    alpha -= Time.deltaTime / (fadeSec / 2);
                }

                if (i == 0 && alpha <= 0)
                {
                    alpha = 0f;
                    firstUIFadeOk = true;
                }

                img.color = new Color(red, green, blue, alpha);
            }
        }
    }

    /// <summary>
    /// Panelのフェードイン処理
    /// </summary>
    /// <param name="InObj">   フェードインする UI オブジェクト </param>
    /// <param name="fadeSec"> 演出処理にかける時間 </param>
    /// <param name="inner">   一番親のオブジェクト（大元のPanel）がこの処理を行っているか </param>
    /// <param name="timeScale"> 時間を止めているか true = 止めている　false = 動いている </param>
    protected void FadeInPanel(ref GameObject InObj, float fadeSec, bool inner, bool timeScale = false)
    {
        bool firstUIFadeOk = false;

        if (!inner)
        {
            var outObjImg = InObj.GetComponent<Image>();

            var outObjRed = outObjImg.color.r;
            var outObjGreen = outObjImg.color.g;
            var outObjBlue = outObjImg.color.b;
            var outObjAlpha = outObjImg.color.a;

            if (firstUIFadeOk)
            {
                outObjAlpha = 0f;
                outObjImg.color = new Color(outObjRed, outObjGreen, outObjBlue, outObjAlpha);
            }
            else
            {
                if (timeScale)
                {
                    outObjAlpha += Time.unscaledDeltaTime / (fadeSec / 2);
                }
                else
                {
                    outObjAlpha += Time.deltaTime / (fadeSec / 2);
                }

                if (outObjAlpha >= 1)
                {
                    outObjAlpha = 1f;
                    firstUIFadeOk = true;
                }

                outObjImg.color = new Color(outObjRed, outObjGreen, outObjBlue, outObjAlpha);
            }
        }

        for (int i = 0; i < InObj.transform.childCount; i++)
        {
            var childObj = InObj.transform.GetChild(i).gameObject;

            if (InObj.transform.childCount > 0)
            {   // 子オブジェクトがいればそのオブジェクトもフェードイン
                FadeInPanel(ref childObj, fadeSec, true, timeScale);
            }
            if (!childObj.activeInHierarchy)
            {   // アクティブではないオブジェクトは処理の必要なし
                continue;
            }

            var vp = childObj.GetComponent<VideoPlayer>();
            if (vp != null)
            {
                childObj.SetActive(false);
            }

            var img = childObj.GetComponent<Image>();

            if (img == null)
            {   // Imageがないのであれば処理の必要なし
                continue;
            }

            var red = img.color.r;
            var green = img.color.g;
            var blue = img.color.b;
            var alpha = img.color.a;

            if (firstUIFadeOk)
            {
                alpha = 1f;
                img.color = new Color(red, green, blue, alpha);
            }
            else
            {
                if (timeScale)
                {
                    alpha += Time.unscaledDeltaTime / (fadeSec / 2);
                }
                else
                {
                    alpha += Time.deltaTime / (fadeSec / 2);
                }

                if (i == 0 && alpha >= 1)
                {
                    alpha = 1f;
                    firstUIFadeOk = true;
                }

                img.color = new Color(red, green, blue, alpha);
            }
        }
    }

    /// <summary>
    /// UI のアルファ値を255 にリセットする処理
    /// </summary>
    /// <param name="AlphaOnUIObj"> アルファ値をリセット(255)するオブジェクト </param>
    /// <param name="active">       アクティブにするか </param>
    protected void UIAlphaOnReset(ref GameObject alphaOnUIObj, bool active)
    {
        var vp = alphaOnUIObj.GetComponent<VideoPlayer>();
        if (vp != null)
        {
            alphaOnUIObj.SetActive(active);
            return;
        }

        var img = alphaOnUIObj.GetComponent<Image>();

        if (img == null)
            return;

        var red = img.color.r;
        var green = img.color.g;
        var blue = img.color.b;
        var alpha = 1f;

        img.color = new Color(red, green, blue, alpha);

        if (active)
        {
            alphaOnUIObj.SetActive(true);
        }
        else
        {
            alphaOnUIObj.SetActive(false);
        }
    }

    /// <summary>
    /// UI のアルファ値を0 にリセットする処理
    /// </summary>
    /// <param name="alphaOffUIObj"> アルファ値をリセット(0)するオブジェクト </param>
    /// <param name="active">        アクティブにするか </param>
    protected void UIAlphaOffReset(ref GameObject alphaOffUIObj, bool active)
    {
        var vp = alphaOffUIObj.GetComponent<VideoPlayer>();
        if (vp != null)
        {
            alphaOffUIObj.SetActive(active);
            return;
        }

        var img = alphaOffUIObj.GetComponent<Image>();

        if (img == null)
            return;

        var red = img.color.r;
        var green = img.color.g;
        var blue = img.color.b;
        var alpha = 0f;

        img.color = new Color(red, green, blue, alpha);

        if (active)
        {
            alphaOffUIObj.SetActive(true);
        }
        else
        {
            alphaOffUIObj.SetActive(false);
        }
    }
    #endregion


    #region PRIVATE METHOD
    
    #endregion
}
