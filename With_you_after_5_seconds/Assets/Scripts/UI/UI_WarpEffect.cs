using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_WarpEffect : MonoBehaviour
{
    /// <summary> �\�[�X�������Ƃ��̃����v���[�g </summary>

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
    /// ���[�v�G�t�F�N�g���J�n����
    /// StageUIScript.cs ����Ă�
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
    /// ���[�v�G�t�F�N�g���X�V����
    /// �����̎��ɂȂ�����A�I��������iEndEffect���Ăԁj
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

        // �N���A��ʂɑJ��܂ł̎��Ԃɍ��킹�āA���X�ɏo��
        alpha += (Time.deltaTime / _StageUI.ClearStayTime);
        _Image.material.SetFloat("_Alpha", alpha);
    }

    /// <summary>
    /// �G�t�F�N�g���I��������
    /// UpdateEffect()����Ă΂��
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
