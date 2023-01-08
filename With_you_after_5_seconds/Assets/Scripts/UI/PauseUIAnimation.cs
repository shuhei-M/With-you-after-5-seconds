using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseUIAnimation : MonoBehaviour
{
    #region Serialize Field
    [SerializeField] private Button playBtton;
    [SerializeField] private float backAlpha = 200f;
    #endregion


    #region Public Field
    public GameData gamedata;
    #endregion


    #region Field
    Animator animator;
    Image buckImg;
    float animaSpeed = 1.0f;
    float backCol_a = 0f;
    #endregion


    #region Propaty
    public bool CutOK { get; set; }
    #endregion


    #region Unity function
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // カットイン最中
        if (animator.GetBool("CutIn") && backCol_a < (backAlpha / 256f))
        {
            BackFadeIn();
            return;
        }

        // カットアウト最中
        if (!animator.GetBool("CutIn") && backCol_a > 0f)
        {
            BackFadeOut();
        }
        else if (!animator.GetBool("CutIn") && gamedata.InGameState == InGame.Pause)
        {
            gamedata.PlayGameTransition();
            Time.timeScale = 1f;
            this.gameObject.SetActive(false);
        }
    }
    #endregion


    #region Public Method
    public void PauseUISetUp()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        buckImg = GetComponent<Image>();
        CutOK = true;
    }

    public void CutInPause()
    {
        Time.timeScale = 0f;

        animator.SetBool("CutIn", true);

        playBtton.Select();
    }

    public void CutOutPause()
    {
        animator.SetBool("CutIn", false);
    }
    #endregion

    #region Method
    void BackFadeIn()
    {
        var r = buckImg.color.r;
        var g = buckImg.color.g;
        var b = buckImg.color.b;
        var a = buckImg.color.a;

        a += (Time.unscaledDeltaTime / animaSpeed);

        if (a > (backAlpha / 256f))
        {
            a = (backAlpha / 256f);
            CutOK = true;
        }

        buckImg.color = new Color(r, g, b, a);

        backCol_a = a;
    }

    void BackFadeOut()
    {
        var r = buckImg.color.r;
        var g = buckImg.color.g;
        var b = buckImg.color.b;
        var a = buckImg.color.a;

        a -= (Time.unscaledDeltaTime / animaSpeed);

        if (a < 0f)
        {
            a = 0f;
            CutOK = true;
        }

        buckImg.color = new Color(r, g, b, a);

        backCol_a = a;
    }
    #endregion
}
