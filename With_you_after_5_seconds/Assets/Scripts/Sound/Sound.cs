using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// サウンド管理
public class Sound
{
    // SEチャンネル数
    const int SE_CHANNEL = 10;

    // サウンド種別
    enum eType
    {
        BGM, // BGM
        SE,  // SE
    }

    // シングルトン
    static Sound _Singleton = null;

    // インスタンス取得
    public static Sound GetInstance()
    {
        // null であれば右辺を返す
        return _Singleton ?? (_Singleton = new Sound());
    }

    // サウンド再生のためのゲームオブジェクト
    GameObject _Object = null;
    // サウンドリソース
    AudioSource _SourceBGM = null; // BGM
    AudioSource _SourceSeDefault = null; // SE（チャンネル）
    AudioSource[] _SourceSEArray; // SE（チャンネル）
    // BGM にアクセスするためのテーブル
    Dictionary<string, _Data> _PoolBGM = new Dictionary<string, _Data>();
    // SE にアクセスするためのテーブル
    Dictionary<string, _Data> _PoolSE = new Dictionary<string, _Data>();

    /// <summary>
    /// 保持するデータ
    /// </summary>
    class _Data
    {
        // アクセスするようのキー
        public string Key;
        // リソース名
        public string ResName;
        // AudioClip
        public AudioClip Clip;

        // コンストラクタ
        public _Data(string key, string res)
        {
            Key = key;
            ResName = "BGM_SE/2D/" + res;
            // AudioClip の取得
            Clip = Resources.Load(ResName) as AudioClip;
        }
    }

    // コンストラクタ
    public Sound()
    {
        // チャンネル確保
        _SourceSEArray = new AudioSource[SE_CHANNEL];
    }

    // AudioSource を取得する
    AudioSource _GetAudioSource(eType type, int channel = -1)
    {
        if (_Object == null)
        {
            // GameObject がなければ作る
            _Object = new GameObject("Sound");
            // 破棄しないようにする
            GameObject.DontDestroyOnLoad(_Object);
            // AudioSource を作成
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
            {   // チャンネル指定
                return _SourceSEArray[channel];
            }
            else
            {   // デフォルト
                return _SourceSeDefault;
            }
        }
    }

    // サウンドのロード
    // ※Resources/Sounds フォルダに配置すること
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
        {   // すでに登録済みなのでいったん消す
            _PoolBGM.Remove(key);
        }
        _PoolBGM.Add(key, new _Data(key, resName));
    }
    void _LoadSE(string key, string resName)
    {
        if (_PoolSE.ContainsKey(key))
        {   // すでに登録済みなのでいったん消す
            _PoolSE.Remove(key);
        }
        _PoolSE.Add(key, new _Data(key, resName));
    }

    /// <summary>
    /// BGM の再生
    /// ※ 事前に LoadBGM でロードしておくこと
    /// </summary>
    public static bool PlayBGM(string key, float volume)
    {
        return GetInstance()._PlayBGM(key, volume);
    }
    bool _PlayBGM(string key, float volume)
    {
        if (_PoolBGM.ContainsKey(key) == false)
        {   // 対応するキーがない
            return false;
        }

        // いったん止める
        _StopBGM();

        // リソースの取得
        var _data = _PoolBGM[key];

        // 再生
        var source = _GetAudioSource(eType.BGM);
        source.loop = true;
        source.clip = _data.Clip;
        source.volume = volume;
        source.Play();

        return true;
    }
    // BGM の停止
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
    /// SE の再生
    /// ※ 事前に LoadSEでロードしておくこと
    /// </summary>
    public static bool PlaySE(string key, float volume, int channel = -1)
    {
        return GetInstance()._PlaySE(key, volume, channel);
    }
    bool _PlaySE(string key, float volume, int channel = -1)
    {
        if (_PoolSE.ContainsKey(key) == false)
        {   // 対応するキーがない
            return false;
        }

        // リソースの取得
        var _data = _PoolSE[key];

        if (0 <= channel && channel < SE_CHANNEL)
        {   // チャンネル指定
            var source = _GetAudioSource(eType.SE, channel);
            source.clip = _data.Clip;
            source.volume = volume;
            source.Play();
        }
        else
        {   // デフォルトで再生
            var source = _GetAudioSource(eType.SE);
            source.volume = volume;
            source.PlayOneShot(_data.Clip);
        }

        return true;
    }

    /// <summary>
    /// 音源の一括ロード
    /// </summary>
    /// <param name="BGMName"> BGMの名前</param>
    /// <param name="SENames"> SEの名前（複数宣言可能だが、SE_CHANNELの数値と一致させる必要がある）</param>
    public void AllSoundsLoad(string BGMName, string[] SENames)
    {
        // BGMのロード
        LoadBGM(BGMName, BGMName);

        for (int i = 0; i < SENames.Length; i++)
        {
            LoadSE(SENames[i], SENames[i]);
        }
    }

    /// <summary>
    /// BGMとSEのオブジェクトを削除しデータを初期化
    /// シーン遷移するときなどに定義しないとAudioSourceが圧迫する
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
    /// BGMのボリュームを落とす
    /// </summary>
    /// <param name="deltaTime"> 1フレームの時間 </param>
    /// <param name="speed">     ボリュームアップする早さ </param>
    /// <param name="maxVolume"> 最大ボリューム </param>
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
    /// BGMのボリュームを落とす
    /// </summary>
    /// <param name="deltaTime"> 1フレームの時間</param>
    /// <param name="speed">     ボリュームダウンする早さ</param>
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
    /// SEが再生中か確認するための関数
    /// </summary>
    /// <param name="key">アクセスキー</param>
    /// <returns>アクセスしたSEが再生中かどうかのフラグ</returns>
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
