using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;                        // FileStream,BinaryWriter,BinaryReader,FileInfo
using System.Runtime.Serialization.Formatters.Binary;	// BinaryFormatter
using System.Security.Cryptography;     // AesManaged,Rfc2898DeriveBytes,MD5CryptoServiceProvider
using System.Threading;					// ThreadPool

namespace Storage
{
    /// <summary>
	/// ストレージアクセス結果
	/// </summary>
	public enum IO_RESULT
    {
        NONE = 0,           // 未実行終了（データ不備）

        SAVE_SUCCESS = 1,   // 成功
        SAVE_FAILED = -1,   // 失敗（保存ファイルの破損等、データがおかしい）

        LOAD_SUCCESS = 10,  // 成功
        LOAD_FAILED = -10,  // 失敗（保存ファイルの破損等、データがおかしい）
    }

	/// <summary>
	/// 保存形式（今回は BINARY しか使わないが念のため JSON も残しとく）
	/// </summary>
	public enum FORMAT
    {
        BINARY,
        JSON,
    }

    /// <summary>
	/// シリアライズするクラスで要求する設定インターフェイス
	/// </summary>
	public interface ISerializer
    {
        string magic { get; }       // マジックNo. ※見られてもいいものにする
        string fileName { get; }    // 保存先
        FORMAT format { get; }      // 保存形式
        System.Type type { get; }   // JSONデシリアライズ用型宣言
        bool encrypt { get; }       // 暗号化するか
        bool backup { get; }        // バックアップを取るか

        ISerializer Clone();        // インスタンスの複製
    }

    /// <summary>
	/// スレッド受け渡し用情報
	/// </summary>
	public struct DataInfo
    {
        public ISerializer serializer;      // シリアライズクラス
        public string filePath;             // 保存先
        public FinishHandler finishHandler; // アクセス完了ハンドラ
        public bool async;                  // 非同期
    }

    /// <summary>
    /// 保存完了ハンドラ
    /// </summary>
    /// <param name="ret">結果</param>
    /// <param name="serializer">保存クラス参照</param>
    public delegate void FinishHandler(IO_RESULT ret, ref DataInfo dataInfo);

    public sealed class StorageManager
	{
        #region MEMBER
        private const int HASH_SIZE = 16;   // MD5は128bit
        private const int SALT_SIZE = 32;   // ソルトの長さ(byte) ※AesManagedの要求値に固定
        private const int IV_SIZE = 16;     // 初期化ベクターの長さ(byte) ※AesManagedの要求値に固定
        private const int ITERATIONS = 50;  // 多いほど強度が高いが処理時間とトレードオフ
        private const string BACKUP_KEY = ".dup";       // バックアップファイル追加キー
        private volatile object sync = new object();    // 同期オブジェクト

        private readonly WaitCallback saveThreadHandler = null; // 保存処理デリゲートキャッシュ
        private readonly WaitCallback loadThreadHandler = null; // 読込処理デリゲートキャッシュ
        private readonly MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider(); // 改ざんチェック用ハッシュジェネレータ
        private readonly Rfc2898DeriveBytes deriveBytes = null;         // キー作成用乱数ジェネレータ
        private readonly byte[] mySalt = null;                          // 更新ソルトキャッシュ
        private readonly byte[] saltBuffer = new byte[SALT_SIZE];       // ソルト読込用バッファ
        private readonly byte[] ivBuffer = new byte[IV_SIZE];           // 初期化ベクター読込用バッファ
        private readonly BinaryFormatter bf = new BinaryFormatter();    // クラスシリアライザー

        private bool isAccessing = false;               // 保存中フラグ（必ずクリティカルセクションをはる）
		#endregion

		#region MAIN FUNCTION
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public StorageManager()
		{
			// デリゲートキャッシュ（暗黙のアロケートの削減）
			this.saveThreadHandler = new WaitCallback(this.SaveThreadMain);
			this.loadThreadHandler = new WaitCallback(this.LoadThreadMain);
			// パスワードをPASSWORD等の変数名や名前で持つと抜かれやすいので注意
			// 今回ソルト値はアプリ起動毎に更新
			this.deriveBytes = new Rfc2898DeriveBytes("10cA1_IJUf_ZOUn118_DeM0", SALT_SIZE, ITERATIONS);
			this.mySalt = this.deriveBytes.Salt;

#if UNITY_IOS
			// シリアライズ（BinaryFormatter）を有効にさせる
			System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
		}

		/// <summary>
		/// シリアライズ保存
		/// </summary>
		/// <param name="saveInterface">シリアライズするクラス</param>
		/// <param name="finishHandler">終了ハンドラ（null指定可）</param>
		/// <param name="async">非同期実行するか</param>
		public void Save(ISerializer saveInterface, FinishHandler finishHandler, bool async = true)
		{
			DataInfo dataInfo = new DataInfo();
			dataInfo.serializer = saveInterface;
#if UNITY_EDITOR 
			dataInfo.filePath = Directory.GetCurrentDirectory() + saveInterface.fileName; //Editor上では普通にカレントディレクトリを確認し、新たにローカルストレージ用のファイルを作成
#else
			dataInfo.filePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + saveInterface.fileName;//EXEを実行したカレントディレクトリ (ショートカット等でカレントディレクトリが変わるのでこの方式で)
#endif
			dataInfo.finishHandler = finishHandler;
			dataInfo.async = async;

			if (saveInterface == null || string.IsNullOrEmpty(saveInterface.fileName))
			{
				this.FinishAccessing(IO_RESULT.NONE, ref dataInfo);
				return;
			}

			// 非同期処理
			if (async)
				ThreadPool.QueueUserWorkItem(this.saveThreadHandler, dataInfo);
			else
				this.SaveThreadMain(dataInfo);
		}

		/// <summary>
		/// デシリアライズ読込
		/// </summary>
		/// <param name="saveInterface">デシリアライズするクラス</param>
		/// <param name="finishHandler">終了ハンドラ（null指定可）</param>
		/// <param name="async">非同期で実行するか</param>
		public void Load(ISerializer loadInterface, FinishHandler finishHandler, bool async = true)
		{
			DataInfo dataInfo = new DataInfo();
			dataInfo.serializer = loadInterface;
#if UNITY_EDITOR
			dataInfo.filePath = Directory.GetCurrentDirectory() + loadInterface.fileName; //Editor上では普通にカレントディレクトリを確認し、新たにローカルストレージ用のファイルを作成
#else
			dataInfo.filePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + loadInterface.fileName;//EXEを実行したカレントディレクトリ (ショートカット等でカレントディレクトリが変わるのでこの方式で)
#endif
			dataInfo.finishHandler = finishHandler;
			dataInfo.async = async;

			if (loadInterface == null || string.IsNullOrEmpty(loadInterface.fileName) || !File.Exists(dataInfo.filePath))
			{
				this.FinishAccessing(IO_RESULT.NONE, ref dataInfo);
				return;
			}

			if (async)
				ThreadPool.QueueUserWorkItem(this.loadThreadHandler, dataInfo);
			else
				this.LoadThreadMain(dataInfo);
		}

		/// <summary>
		/// 削除
		/// </summary>
		/// <param name="serializer">シリアライズクラス</param>
		public void Delete(ISerializer serializer)
		{
			// ロードしたら即削除
#if UNITY_EDITOR
			File.Delete(Directory.GetCurrentDirectory() + serializer.fileName); //Editor上では普通にカレントディレクトリを確認し、新たにローカルストレージ用のファイルを作成
#else
			File.Delete(AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + serializer.fileName); //EXEを実行したカレントディレクトリ (ショートカット等でカレントディレクトリが変わるのでこの方式で)
#endif
        }

        /// <summary>
        /// ファイルがあるか
        /// </summary>
        /// <param name="serializer">シリアライズクラス</param>
        public bool Exists(ISerializer serializer)
		{
#if UNITY_EDITOR
			string path = Directory.GetCurrentDirectory() + serializer.fileName; //Editor上では普通にカレントディレクトリを確認し、新たにローカルストレージ用のファイルを作成
#else
			string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + serializer.fileName;//EXEを実行したカレントディレクトリ (ショートカット等でカレントディレクトリが変わるのでこの方式で)
#endif
			if (File.Exists(path))
			{
				FileInfo fi = new FileInfo(path);
				// ※ファイル書き込み中にクラッシュすると0byteのファイルが出来る
				if (fi.Length > 0)
					Debug.Log("ローカルストレージ => 存在する"); return true;
			}

			Debug.Log("ローカルストレージ => 存在しない");
			return false;
		}
		#endregion

		#region PUBLIC_FUNCTION
		/// <summary>
		/// ファイルアクセス中か
		/// </summary>
		public bool IsAccessing()
		{
			// 複数リクエストが来てもいいように外部からの参照もロック
			lock (this.sync)
			{
				return this.isAccessing;
			}
		}
		#endregion

		#region PRIVATE FUNCTION
		/// <summary>
		/// ストレージアクセス終了処理
		/// </summary>
		/// <param name="ret">結果</param>
		/// <param name="dataInfo">保存情報</param>
		private void FinishAccessing(IO_RESULT ret, ref DataInfo dataInfo)
		{
			// 失敗時はバックアップを参照
			switch (ret)
			{
				case IO_RESULT.LOAD_FAILED:
					if (!dataInfo.filePath.Contains(BACKUP_KEY))
					{
						string backupPath = dataInfo.filePath + BACKUP_KEY;
						if (File.Exists(backupPath))
						{
							lock (this.sync)
							{
								this.isAccessing = false;
							}

							dataInfo.filePath = backupPath;
							if (dataInfo.async)
								ThreadPool.QueueUserWorkItem(this.loadThreadHandler, dataInfo);
							else
								this.LoadThreadMain(dataInfo);

							return;
						}
					}
					break;
#if DEMO
				case IO_RESULT.LOAD_SUCCESS:
					dataInfo.backup = File.Exists(dataInfo.filePath + BACKUP_KEY);
					break;
#endif
				case IO_RESULT.SAVE_SUCCESS:
					// バックアップ生成
					if (dataInfo.serializer.backup)
					{
						try
						{
							File.Copy(dataInfo.filePath, dataInfo.filePath + BACKUP_KEY, true);
						}
						catch (System.Exception e)
						{
							Debug.LogError("BACKUP FAILED : " + dataInfo.filePath + "\n" + e.Message);
						}
					}
					else
					{
						File.Delete(dataInfo.filePath + BACKUP_KEY);
					}
					break;
			}

			if (dataInfo.finishHandler != null)
				dataInfo.finishHandler(ret, ref dataInfo);

			lock (this.sync)
			{
				this.isAccessing = false;
			}

#if DEBUG
			string message = "ACCESS FILE => " + dataInfo.filePath + "\n..........";
			switch (ret)
			{
				case IO_RESULT.SAVE_FAILED:
				case IO_RESULT.LOAD_FAILED:
					message += "<color=yellow>";
					break;
				case IO_RESULT.SAVE_SUCCESS:
				case IO_RESULT.LOAD_SUCCESS:
					message += "<color=white>";
					break;
			}
			Debug.Log(message + ret + "</color>");
#endif
		}

		/// <summary>
		/// 保存処理
		/// </summary>
		private void SaveThreadMain(object state)
		{
			DataInfo dataInfo = (DataInfo)state;
			int size = 0;
			byte[] hash = null;
			byte[] binary = null;

			// 他のリクエスト消化待ち
			while (dataInfo.async && this.IsAccessing())
			{
				Thread.Sleep(30);
			}

			lock (this.sync)
			{
				this.isAccessing = true;
			}

			// 書き込むバイナリデータの作成
			try
			{
				using (MemoryStream inMs = new MemoryStream())
				{
					switch (dataInfo.serializer.format)
					{
						case FORMAT.BINARY:
							// シリアライズ
							this.bf.Serialize(inMs, dataInfo.serializer);
							break;
						case FORMAT.JSON:
							using (BinaryWriter bw = new BinaryWriter(inMs))
							{
								string json = JsonUtility.ToJson(dataInfo.serializer, true);
								bw.Write(json);
							}
							break;
					}
					binary = inMs.ToArray();
					//size = (int)inMs.Position;
					size = binary.Length;
					hash = this.md5.ComputeHash(binary);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError("SERIALIZE FAILED\n" + e.Message);
				this.FinishAccessing(IO_RESULT.SAVE_FAILED, ref dataInfo);
				return;
			}

			// ストレージ書込
			try
			{
				using (FileStream outFs = File.Create(dataInfo.filePath))
				{
					using (BinaryWriter writer = new BinaryWriter(outFs))
					{
						// ヘッダ書込
						writer.Write(dataInfo.serializer.magic);
						writer.Write(size);
						writer.Write(hash);
#if DEMO
						writer.Write(dataInfo.serializer.encrypt);
						writer.Write((int)dataInfo.serializer.format);
#endif

						// 暗号化して書込
						if (dataInfo.serializer.encrypt)
						{
							bool success = this.EncryptFile(outFs, binary, size);
							// 暗号化失敗
							if (!success)
							{
								this.FinishAccessing(IO_RESULT.SAVE_FAILED, ref dataInfo);
								return;
							}
						}
						// そのまま書込
						else
						{
							writer.Write(binary);
						}
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError("WRITE FAILED\n" + e.Message);
				this.FinishAccessing(IO_RESULT.SAVE_FAILED, ref dataInfo);
				return;
			}

			// 完了
			this.FinishAccessing(IO_RESULT.SAVE_SUCCESS, ref dataInfo);

#if DEBUG
			// 保存ファイルサイズ確認
			FileInfo fi = new FileInfo(dataInfo.filePath);
			Debug.Log("..........FILE_SIZE " + fi.Length + "\n..........SAVE_SIZE " + size);
#endif
		}

		/// <summary>
		/// 読込処理
		/// </summary>
		private void LoadThreadMain(object state)
		{
			DataInfo dataInfo = (DataInfo)state;
			int size = 0;
			byte[] hash = null;
			byte[] binary = null;
			bool encrypt = dataInfo.serializer.encrypt;
			FORMAT format = dataInfo.serializer.format;

			// 他のリクエスト消化待ち
			while (dataInfo.async && this.IsAccessing())
			{
				Thread.Sleep(30);
			}

			lock (this.sync)
			{
				this.isAccessing = true;
			}

			// ストレージ読込
			try
			{
				using (FileStream inFs = File.OpenRead(dataInfo.filePath))
				{
					using (BinaryReader reader = new BinaryReader(inFs))
					{
						// ヘッダ読込
						string magic = reader.ReadString();
						// マジックNo.不一致
						if (magic != dataInfo.serializer.magic)
						{
							Debug.LogWarning("CHANGED MAGIC NUMBER\n");
							this.FinishAccessing(IO_RESULT.LOAD_FAILED, ref dataInfo);
							return;
						}
						size = reader.ReadInt32();
						hash = reader.ReadBytes(HASH_SIZE);
						binary = new byte[size];
#if DEMO
						encrypt = dataInfo.encrypt = reader.ReadBoolean();
						format = dataInfo.format = (FORMAT)reader.ReadInt32();
#endif

						// 復号化して読込
						if (encrypt)
						{
							bool success = this.DecryptFile(inFs, binary, size);
							// 復号化失敗
							if (!success)
							{
								this.FinishAccessing(IO_RESULT.LOAD_FAILED, ref dataInfo);
								return;
							}
						}
						// そのまま読込
						else
						{
							inFs.Read(binary, 0, size);
						}
					}

					// ハッシュチェック
					if (!this.CheckHash(this.md5.ComputeHash(binary), hash))
					{
						Debug.LogWarning("HASH MISMATCH\n");
						this.FinishAccessing(IO_RESULT.LOAD_FAILED, ref dataInfo);
						return;
					}

					// デシリアライズ
					using (MemoryStream outMs = new MemoryStream(binary))
					{
						switch (format)
						{
							case FORMAT.BINARY:
								dataInfo.serializer = this.bf.Deserialize(outMs) as ISerializer;
								break;
							case FORMAT.JSON:
								using (BinaryReader br = new BinaryReader(outMs))
								{
									string json = br.ReadString();
									System.Type type = dataInfo.serializer.type;
									dataInfo.serializer = JsonUtility.FromJson(json, type) as ISerializer;
								}
								break;
						}
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError("READ FAILED\n" + e.Message);
				this.FinishAccessing(IO_RESULT.LOAD_FAILED, ref dataInfo);
				return;
			}

			// 完了
			this.FinishAccessing(IO_RESULT.LOAD_SUCCESS, ref dataInfo);

#if DEBUG
			// 読込ファイルサイズ確認
			FileInfo fi = new FileInfo(dataInfo.filePath);
			Debug.Log("..........FILE_SIZE " + fi.Length + "\n..........SAVE_SIZE " + size);
#endif
		}

		/// <summary>
		/// ファイルを暗号化して書き込む
		/// </summary>
		/// <param name="outFs">保存するファイルストリーム</param>
		/// <param name="bs">保存するバイト列</param>
		/// <param name="size">保存するサイズ</param>
		private bool EncryptFile(Stream outFs, byte[] bs, int size)
		{
			bool success = true;

			using (AesManaged aes = new AesManaged())
			{
				try
				{
					// 今回の起動時のソルトを準備
					this.deriveBytes.Salt = this.mySalt;
					this.deriveBytes.Reset();
					// パスワードからキーの作成、ソルトは自動作成とする
					outFs.Write(this.deriveBytes.Salt, 0, SALT_SIZE);
					aes.Key = this.deriveBytes.GetBytes(aes.KeySize / 8);

					// 初期化ベクターの生成
					aes.GenerateIV();
					outFs.Write(aes.IV, 0, IV_SIZE);

					// 暗号化書き込み
					using (ICryptoTransform encryptor = aes.CreateEncryptor())
					{
						using (CryptoStream cryptStrm = new CryptoStream(outFs, encryptor, CryptoStreamMode.Write))
						{
							cryptStrm.Write(bs, 0, size);
							Debug.Log("ENCRYPT SUCCESS\n");
						}//cryptStrm.Close();
					}//encryptor.Dispose();
				}
				catch (System.Exception e)
				{
					Debug.LogError("ENCRYPT FAILED\n" + e.Message);
					success = false;
				}
			}//aes.Clear();

			return success;
		}

		/// <summary>
		/// ファイルを復号化して読み込む
		/// </summary>
		/// <param name="inFs">読み込むファイルストリーム</param>
		/// <param name="bs">読み込むバイト列</param>
		/// <param name="size">読み込むサイズ</param>
		private bool DecryptFile(Stream inFs, byte[] bs, int size)
		{
			bool success = true;

			using (AesManaged aes = new AesManaged())
			{
				try
				{
					inFs.Read(this.saltBuffer, 0, SALT_SIZE);
					this.deriveBytes.Salt = this.saltBuffer;
					this.deriveBytes.Reset();
					aes.Key = this.deriveBytes.GetBytes(aes.KeySize / 8);

					inFs.Read(this.ivBuffer, 0, IV_SIZE);
					aes.IV = this.ivBuffer;

					// 複合化読み込み
					using (ICryptoTransform decryptor = aes.CreateDecryptor())
					{
						using (CryptoStream cryptStrm = new CryptoStream(inFs, decryptor, CryptoStreamMode.Read))
						{
							cryptStrm.Read(bs, 0, size);
							Debug.Log("DECRYPT SUCCESS\n");
						}//cryptStrm.Close();
					}//decryptor.Dispose();
				}
				catch (System.Exception e)
				{
					Debug.LogError("DECRYPT FAILED\n" + e.Message);
					success = false;
				}
			}//aes.Clear();

			return success;
		}

		/// <summary>
		/// ハッシュチェック
		/// </summary>
		/// <returns>一致したか</returns>
		/// <param name="a">チェック対象</param>
		/// <param name="b">チェック対象</param>
		private bool CheckHash(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
				return false;

			int size = a.Length;
			for (int i = 0; i < size; ++i)
			{
				if (a[i] != b[i])
					return false;
			}

			return true;
		}
		#endregion
	}
}

