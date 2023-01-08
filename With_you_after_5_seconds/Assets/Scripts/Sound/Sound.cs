using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �T�E���h�Ǘ�
public class Sound
{
    // SE�`�����l����
    const int SE_CHANNEL = 10;

    // �T�E���h���
    enum eType
    {
        BGM, // BGM
        SE,  // SE
    }

    // �V���O���g��
    static Sound _Singleton = null;

    // �C���X�^���X�擾
    public static Sound GetInstance()
    {
        // null �ł���ΉE�ӂ�Ԃ�
        return _Singleton ?? (_Singleton = new Sound());
    }

    // �T�E���h�Đ��̂��߂̃Q�[���I�u�W�F�N�g
    GameObject _Object = null;
    // �T�E���h���\�[�X
    AudioSource _SourceBGM = null; // BGM
    AudioSource _SourceSeDefault = null; // SE�i�`�����l���j
    AudioSource[] _SourceSEArray; // SE�i�`�����l���j
    // BGM �ɃA�N�Z�X���邽�߂̃e�[�u��
    Dictionary<string, _Data> _PoolBGM = new Dictionary<string, _Data>();
    // SE �ɃA�N�Z�X���邽�߂̃e�[�u��
    Dictionary<string, _Data> _PoolSE = new Dictionary<string, _Data>();

    /// <summary>
    /// �ێ�����f�[�^
    /// </summary>
    class _Data
    {
        // �A�N�Z�X����悤�̃L�[
        public string Key;
        // ���\�[�X��
        public string ResName;
        // AudioClip
        public AudioClip Clip;

        // �R���X�g���N�^
        public _Data(string key, string res)
        {
            Key = key;
            ResName = "BGM_SE/2D/" + res;
            // AudioClip �̎擾
            Clip = Resources.Load(ResName) as AudioClip;
        }
    }

    // �R���X�g���N�^
    public Sound()
    {
        // �`�����l���m��
        _SourceSEArray = new AudioSource[SE_CHANNEL];
    }

    // AudioSource ���擾����
    AudioSource _GetAudioSource(eType type, int channel = -1)
    {
        if (_Object == null)
        {
            // GameObject ���Ȃ���΍��
            _Object = new GameObject("Sound");
            // �j�����Ȃ��悤�ɂ���
            GameObject.DontDestroyOnLoad(_Object);
            // AudioSource ���쐬
            _SourceBGM = _Object.AddComponent<AudioSource>();
            _SourceBGM.volume = 0.1f;
            _SourceSeDefault = _Object.AddComponent<AudioSource>();
            for (int i = 0; i < SE_CHANNEL; i++)
            {
                _SourceSEArray[i] = _Object.AddComponent<AudioSource>();
            }
        }

        if (type == eType.BGM)
        {   // BGM
            return _SourceBGM;
        }
        else
        {   // SE
            if (0 <= channel && channel < SE_CHANNEL)
            {   // �`�����l���w��
                return _SourceSEArray[channel];
            }
            else
            {   // �f�t�H���g
                return _SourceSeDefault;
            }
        }
    }

    // �T�E���h�̃��[�h
    // ��Resources/Sounds �t�H���_�ɔz�u���邱��
    public static void LoadBGM(string key, string resName)
    {
        GetInstance()._LoadBGM(key, resName);
    }
    public static void LoadSE(string key, string resName)
    {
        GetInstance()._LoadSE(key, resName);
    }
    void _LoadBGM(string key, string resName)
    {
        if (_PoolBGM.ContainsKey(key))
        {   // ���łɓo�^�ς݂Ȃ̂ł����������
            _PoolBGM.Remove(key);
        }
        _PoolBGM.Add(key, new _Data(key, resName));
    }
    void _LoadSE(string key, string resName)
    {
        if (_PoolSE.ContainsKey(key))
        {   // ���łɓo�^�ς݂Ȃ̂ł����������
            _PoolSE.Remove(key);
        }
        _PoolSE.Add(key, new _Data(key, resName));
    }

    /// <summary>
    /// BGM �̍Đ�
    /// �� ���O�� LoadBGM �Ń��[�h���Ă�������
    /// </summary>
    public static bool PlayBGM(string key, float volume)
    {
        return GetInstance()._PlayBGM(key, volume);
    }
    bool _PlayBGM(string key, float volume)
    {
        if (_PoolBGM.ContainsKey(key) == false)
        {   // �Ή�����L�[���Ȃ�
            return false;
        }

        // ��������~�߂�
        _StopBGM();

        // ���\�[�X�̎擾
        var _data = _PoolBGM[key];

        // �Đ�
        var source = _GetAudioSource(eType.BGM);
        source.loop = true;
        source.clip = _data.Clip;
        source.volume = volume;
        source.Play();

        return true;
    }
    // BGM �̒�~
    public static bool StopBGM()
    {
        return GetInstance()._StopBGM();
    }
    bool _StopBGM()
    {
        _GetAudioSource(eType.BGM).Stop();

        return true;
    }

    public static bool StopSE(int channel = -1)
    {
        return GetInstance()._StopSE(channel);
    }

    bool _StopSE(int channel = -1)
    {
        _GetAudioSource(eType.SE, channel).Stop();
        return true;
    }

    /// <summary>
    /// SE �̍Đ�
    /// �� ���O�� LoadSE�Ń��[�h���Ă�������
    /// </summary>
    public static bool PlaySE(string key, float volume, int channel = -1)
    {
        return GetInstance()._PlaySE(key, volume, channel);
    }
    bool _PlaySE(string key, float volume, int channel = -1)
    {
        if (_PoolSE.ContainsKey(key) == false)
        {   // �Ή�����L�[���Ȃ�
            return false;
        }

        // ���\�[�X�̎擾
        var _data = _PoolSE[key];

        if (0 <= channel && channel < SE_CHANNEL)
        {   // �`�����l���w��
            var source = _GetAudioSource(eType.SE, channel);
            source.clip = _data.Clip;
            source.volume = volume;
            source.Play();
        }
        else
        {   // �f�t�H���g�ōĐ�
            var source = _GetAudioSource(eType.SE);
            source.volume = volume;
            source.PlayOneShot(_data.Clip);
        }

        return true;
    }

    /// <summary>
    /// �����̈ꊇ���[�h
    /// </summary>
    /// <param name="BGMName"> BGM�̖��O</param>
    /// <param name="SENames"> SE�̖��O�i�����錾�\�����ASE_CHANNEL�̐��l�ƈ�v������K�v������j</param>
    public void AllSoundsLoad(string BGMName, string[] SENames)
    {
        // BGM�̃��[�h
        LoadBGM(BGMName, BGMName);

        for (int i = 0; i < SENames.Length; i++)
        {
            LoadSE(SENames[i], SENames[i]);
        }
    }

    /// <summary>
    /// BGM��SE�̃I�u�W�F�N�g���폜���f�[�^��������
    /// �V�[���J�ڂ���Ƃ��Ȃǂɒ�`���Ȃ���AudioSource����������
    /// </summary>
    public static bool BGMAndSEResets()
    {
        return GetInstance()._BGMAndSEResets();
    }
    bool _BGMAndSEResets()
    {
        MonoBehaviour.Destroy(_Object);
        _PoolBGM.Clear();
        _PoolSE.Clear();

        return true;
    }

    /// <summary>
    /// BGM�̃{�����[���𗎂Ƃ�
    /// </summary>
    /// <param name="deltaTime"> 1�t���[���̎��� </param>
    /// <param name="speed">     �{�����[���A�b�v���鑁�� </param>
    /// <param name="maxVolume"> �ő�{�����[�� </param>
    /// <returns></returns>
    public static bool VolumeUpBGM(float deltaTime, float speed, float maxVolume = 1f)
    {
        return GetInstance()._VolumeUpBGM(deltaTime, speed, maxVolume);
    }
    bool _VolumeUpBGM(float deltaTime, float speed, float maxVolume)
    {
        var BGM = _GetAudioSource(eType.BGM);
        BGM.volume += (deltaTime * speed) * 0.01f;

        if (BGM.volume < 0f)
        {
            BGM.volume = 0f;
        }

        return true;
    }

    /// <summary>
    /// BGM�̃{�����[���𗎂Ƃ�
    /// </summary>
    /// <param name="deltaTime"> 1�t���[���̎���</param>
    /// <param name="speed">     �{�����[���_�E�����鑁��</param>
    /// <returns></returns>
    public static bool VolumeDownBGM(float deltaTime, float speed)
    {
        return GetInstance()._VolumeDownBGM(deltaTime, speed);
    }
    bool _VolumeDownBGM(float deltaTime, float speed)
    {
        var BGM = _GetAudioSource(eType.BGM);
        BGM.volume -= (deltaTime * speed) * 0.01f;

        if (BGM.volume < 0f)
        {
            BGM.volume = 0f;
        }

        return true;
    }

    /// <summary>
    /// SE���Đ������m�F���邽�߂̊֐�
    /// </summary>
    /// <param name="key">�A�N�Z�X�L�[</param>
    /// <returns>�A�N�Z�X����SE���Đ������ǂ����̃t���O</returns>
    public static bool IsPlayingSE(int channel = -1)
    {
        return GetInstance()._IsPlayingSE(channel);
    }

    bool _IsPlayingSE(int channel = -1)
    {
        bool isPlaying = false;

        var source = _GetAudioSource(eType.SE, channel);
        isPlaying = source.isPlaying;

        return isPlaying;
    }
}
