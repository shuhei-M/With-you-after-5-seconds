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
    //---------- �����т�����ʎ��̏��� ----------//

    /// <summary>
    /// ���̃y�[�W�ֈړ�
    /// </summary>
    /// <param name="howToPlayPageScreen"> �����т�����ʂ��� </param>
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
    /// �O�̃y�[�W�ֈړ�
    /// </summary>
    /// <param name="howToPlayPageScreen"> �����т�����ʂ��� </param>
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
    /// �����т�����ʂ̃A�N�e�B�u��Ԃ����Z�b�g�i�G�t�F�N�g��Ԃ����Z�b�g�\��j
    /// </summary>
    /// <param name="howToPlayPageScreen"> �����т�����ʂ��� </param>
    protected void HowToPlayPageScreenReset(ref GameObject[] howToPlayPageScreen)
    {
        for (int i = 0; i < howToPlayPageScreen.Length; i++)
        {
            // 1�y�[�W�ڂ��A�N�e�B�u��ԂȂ炻�̂܂ܖ߂�
            if (howToPlayPageScreen[i].activeInHierarchy && i == 0)
                return;

            // 2�y�[�W�ڈȍ~���A�N�e�B�u��ԂȂ�1�y�[�W�ڂ̂݃A�N�e�B�u�����Ė߂�
            if (howToPlayPageScreen[i].activeInHierarchy)
            {
                howToPlayPageScreen[0].SetActive(true);
                howToPlayPageScreen[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// �����т�����ʂ̃G�t�F�N�g���������Z�b�g
    /// </summary>
    /// <param name="howToPlayScreen"> �����т�����ʂ��� </param>
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
    /// UI �̃A���t�@�l�� 1 �ɕύX����֐�
    /// </summary>
    /// <param name="UIs"> �A���t�@�l�� 1 �ɂ���UI�I�u�W�F�N�g </param>
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
    /// UI��؂�ւ���֐�(���ԂɃt�F�[�h�C�� �� �t�F�[�h�A�E�g)
    /// </summary>
    /// <param name="OutObj">    �t�F�[�h�A�E�g���� UI �I�u�W�F�N�g���� </param>
    /// <param name="InObj">     �t�F�[�h�C������ UI �I�u�W�F�N�g���� </param>
    /// <param name="fadeState"> �t�F�[�h�X�e�[�g�i��ԁj</param>
    /// <param name="fadeSec">   ���o�����ɂ����鎞�ԁi�t�F�[�h�C�����鎞�� �~ �t�F�[�h�A�E�g���鎞�ԁj</param>
    /// <param name="timeScale"> ���Ԃ��~�߂Ă��邩 true = �~�߂Ă���@false = �����Ă��� </param>
    protected void UIToFade(ref GameObject OutObj, ref GameObject InObj, ref FadeState fadeState, float fadeSec, bool timeScale = false)
    {
        var outObjFirstChildImg = OutObj.transform.GetChild(0).gameObject.GetComponent<Image>();
        var inObjFirstChildImg = InObj.transform.GetChild(0).gameObject.GetComponent<Image>();

        if (outObjFirstChildImg.color.a > 0)
        {   // �\������ UI ���Ƀt�F�[�h�A�E�g
            FadeOutUIs(ref OutObj, fadeSec, timeScale);
        }
        else
        {   // �t�F�[�h�A�E�g�������I��莟�掟�\������ UI ���t�F�[�h�C��
            if (OutObj.activeInHierarchy)
            {
                OutObj.SetActive(false);
                InObj.SetActive(true);
            }

            FadeInUIs(ref InObj, fadeSec, timeScale);

            if (inObjFirstChildImg.color.a >= 1)
            {   // UI ����ւ��������I��������Ԃ�������
                fadeState = FadeState.None;
            }
        }
    }

    /// <summary>
    /// Panel��؂�ւ���֐�(�����Ƀt�F�[�h�C���E�t�F�[�h�A�E�g)
    /// </summary>
    /// <param name="OutObj">       �t�F�[�h�A�E�g���� UI �I�u�W�F�N�g </param>
    /// <param name="InObj">        �t�F�[�h�C������ UI �I�u�W�F�N�g </param>
    /// <param name="fadeState">    ���̏������s�����̃t���O�B�����I������t���O�𗎂Ƃ��B</param>
    /// <param name="backImgObj">   OutObj �� InObj �Ŕw�ʂɂ�����I�u�W�F�N�g</param>
    /// <param name="fadeSec">      ���o�����ɂ����鎞�� </param>
    /// <param name="outObjActive"> �w�ʂ�UI���c�����itrue: �c��, false: �����j</param>
    /// <param name="timeScale">    ���Ԃ��~�߂Ă��邩 true = �~�߂Ă���@false = �����Ă��� </param>
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
            {   // �^�C�g������Ȃ���ʂ��t�F�[�h�C���E�t�F�[�h�A�E�g
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
            {   // �^�C�g������Ȃ���ʂ��t�F�[�h�C���E�t�F�[�h�A�E�g
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
    /// UI�̃t�F�[�h�A�E�g����
    /// </summary>
    /// <param name="OutObj">  �t�F�[�h�A�E�g���� UI �I�u�W�F�N�g���� </param>
    /// <param name="fadeSec"> ���o�����ɂ����鎞�� </param>
    /// <param name="timeScale"> ���Ԃ��~�߂Ă��邩 true = �~�߂Ă���@false = �����Ă��� </param>
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
    /// 1 �� UI �����t�F�[�h�A�E�g����
    /// </summary>
    /// <param name="OutObj">  �t�F�[�h�A�E�g���� UI �I�u�W�F�N�g </param>
    /// <param name="fadeSec"> ���o�����ɂ����鎞�� </param>
    /// <param name="timeScale"> ���Ԃ��~�߂Ă��邩 true = �~�߂Ă���@false = �����Ă��� </param>
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
    /// UI�̃t�F�[�h�C������
    /// </summary>
    /// <param name="InObj">   �t�F�[�h�C������ UI �I�u�W�F�N�g���� </param>
    /// <param name="fadeSec"> ���o�����ɂ����鎞�� </param>
    /// <param name="timeScale"> ���Ԃ��~�߂Ă��邩 true = �~�߂Ă���@false = �����Ă��� </param>
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
    /// 1 �� UI �����t�F�[�h�C������
    /// </summary>
    /// <param name="OutObj">  �t�F�[�h�C������ UI �I�u�W�F�N�g���� </param>
    /// <param name="fadeSec"> ���o�����ɂ����鎞�� </param>
    /// <param name="timeScale"> ���Ԃ��~�߂Ă��邩 true = �~�߂Ă���@false = �����Ă��� </param>
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
    /// UI �_�ŏ���
    /// </summary>
    /// <param name="flashingImg">       �_�ł��� UI �� Image </param>
    /// <param name="flashingRepetTime"> �_�Łi�����Ă��܂ł́j���� </param>
    /// <param name="timeScale"> ���Ԃ��~�߂Ă��邩 true = �~�߂Ă���@false = �����Ă��� </param>
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
    /// Panel�̃t�F�[�h�A�E�g����
    /// </summary>
    /// <param name="OutObj">  �t�F�[�h�A�E�g���� UI �I�u�W�F�N�g </param>
    /// <param name="fadeSec"> ���o�����ɂ����鎞�� </param>
    /// <param name="inner">   ��Ԑe�̃I�u�W�F�N�g�i�匳��Panel�j�����̏������s���Ă��邩 </param>
    /// <param name="timeScale"> ���Ԃ��~�߂Ă��邩 true = �~�߂Ă���@false = �����Ă��� </param>
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
            {   // �q�I�u�W�F�N�g������΂��̃I�u�W�F�N�g���t�F�[�h�A�E�g
                FadeOutPanel(ref childObj, fadeSec, true, timeScale);
            }
            if (!childObj.activeInHierarchy)
            {   // �A�N�e�B�u�ł͂Ȃ��I�u�W�F�N�g�͏����̕K�v�Ȃ�
                continue;
            }

            var vp = childObj.GetComponent<VideoPlayer>();
            if (vp != null)
            {
                childObj.SetActive(false);
            }

            var img = childObj.GetComponent<Image>();

            if (img == null)
            {   // Image���Ȃ��̂ł���Ώ����̕K�v�Ȃ�
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
    /// Panel�̃t�F�[�h�C������
    /// </summary>
    /// <param name="InObj">   �t�F�[�h�C������ UI �I�u�W�F�N�g </param>
    /// <param name="fadeSec"> ���o�����ɂ����鎞�� </param>
    /// <param name="inner">   ��Ԑe�̃I�u�W�F�N�g�i�匳��Panel�j�����̏������s���Ă��邩 </param>
    /// <param name="timeScale"> ���Ԃ��~�߂Ă��邩 true = �~�߂Ă���@false = �����Ă��� </param>
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
            {   // �q�I�u�W�F�N�g������΂��̃I�u�W�F�N�g���t�F�[�h�C��
                FadeInPanel(ref childObj, fadeSec, true, timeScale);
            }
            if (!childObj.activeInHierarchy)
            {   // �A�N�e�B�u�ł͂Ȃ��I�u�W�F�N�g�͏����̕K�v�Ȃ�
                continue;
            }

            var vp = childObj.GetComponent<VideoPlayer>();
            if (vp != null)
            {
                childObj.SetActive(false);
            }

            var img = childObj.GetComponent<Image>();

            if (img == null)
            {   // Image���Ȃ��̂ł���Ώ����̕K�v�Ȃ�
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
    /// UI �̃A���t�@�l��255 �Ƀ��Z�b�g���鏈��
    /// </summary>
    /// <param name="AlphaOnUIObj"> �A���t�@�l�����Z�b�g(255)����I�u�W�F�N�g </param>
    /// <param name="active">       �A�N�e�B�u�ɂ��邩 </param>
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
    /// UI �̃A���t�@�l��0 �Ƀ��Z�b�g���鏈��
    /// </summary>
    /// <param name="alphaOffUIObj"> �A���t�@�l�����Z�b�g(0)����I�u�W�F�N�g </param>
    /// <param name="active">        �A�N�e�B�u�ɂ��邩 </param>
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
