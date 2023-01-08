using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class UserSettings : Storage.ISerializer
{
	// 文字列キャッシュ
	// プロパティに直接定義するとプロパティアクセスの度に無駄なメモリアロケートが発生する
	[System.NonSerialized]
	private const string magic_ = "UserSettings_180101";
	//[System.NonSerialized]
	//private const string fileName_ = "/UserSettings";
	[System.NonSerialized]
	private const string fileName_ = "/SaveData";

	// インターフェース実装
	public string magic { get { return UserSettings.magic_; } }    // マジックNo.
	public string fileName { get { return UserSettings.fileName_; } } // 保存先
	public System.Type type { get { return typeof(UserSettings); } }   // JSONデシリアライズ用型宣言

	public Storage.FORMAT format { get { return Storage.FORMAT.BINARY; } } // 保存形式
	public bool encrypt { get { return true; } }                  // 暗号化するか（任意）
	public bool backup { get { return true; } }

	// 更新データ
	public StageData[] stageData = new StageData[8];   // ステージのクリア情報
	public int notClearStageNum = 0;                    // クリアできていないステージ
	public bool startGame = false;                      // ゲーム開始時か
	public bool[] showStartStory = new bool[5];					// スタートストーリーを見ているか

	/// <summary>
	/// 複製
	/// </summary>
	public Storage.ISerializer Clone()
	{
		return this.MemberwiseClone() as Storage.ISerializer;
	}

	public void Clear()
	{
		for(int i = 0; i < stageData.Length; i++)
        {
			stageData[i].ClearData();
        }
		notClearStageNum = 0;
		startGame = false;

		for(int i = 0; i < showStartStory.Length; i++)
			showStartStory[i] = false;
		Debug.Log("ローカルストレージをクリアしました。\n");
	}
}