using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionScript : MonoBehaviour
{
    #region SerializeField
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Material _transitionIn;
    #endregion


    #region PublicField
    public GameData gameData;
    #endregion


    #region Field
    string loadMethodName;
    int spriteNum;
    #endregion


    #region Property
    public bool FadeInOK { get; set; }
    public bool FadeOutOK { get; set; }
    #endregion


    #region Public method
    // フェードアウト
    public void FadeOutTransition(int spriteNum)
    {
        if (!FadeOutOK)
            return;

        FadeOutOK = false;
        gameObject.SetActive(true);
        this.spriteNum = spriteNum;
        OutAnimate(_transitionIn, 1.0f);
    }

    // フェードイン
    public void FadeInTransition(int spriteNum)
    {
        if (!FadeInOK)
            return;

        FadeInOK = false;
        gameObject.SetActive(true);
        this.spriteNum = spriteNum;
        StartCoroutine(InAnimate(_transitionIn, 1.0f));
    }
    #endregion


    #region Coroutine
    IEnumerator OutAnimate(Material material, float time)
    {
        var image = GetComponent<Image>();
        image.sprite = _sprites[spriteNum];
        GetComponent<Image>().material = material;
        float current = 0;
        while (current < time)
        {
            material.SetFloat("_Alpha", current / time);
            yield return new WaitForEndOfFrame();
            current += Time.deltaTime;
        }
        material.SetFloat("_Alpha", 1);

        FadeOutOK = true;
    }

    IEnumerator InAnimate(Material material, float time)
    {
        var image = GetComponent<Image>();
        image.sprite = _sprites[spriteNum];
        GetComponent<Image>().material = material;
        float current = 0;
        while (current < time)
        {
            material.SetFloat("_Alpha", 1 - current / time);
            yield return new WaitForEndOfFrame();
            current += Time.deltaTime;
        }
        material.SetFloat("_Alpha", 0);

        FadeInOK = true;
        gameObject.SetActive(false);
    }
    #endregion
}
