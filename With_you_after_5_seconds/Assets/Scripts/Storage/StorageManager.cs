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
	/// �X�g���[�W�A�N�Z�X����
	/// </summary>
	public enum IO_RESULT
    {
        NONE = 0,           // �����s�I���i�f�[�^�s���j

        SAVE_SUCCESS = 1,   // ����
        SAVE_FAILED = -1,   // ���s�i�ۑ��t�@�C���̔j�����A�f�[�^�����������j

        LOAD_SUCCESS = 10,  // ����
        LOAD_FAILED = -10,  // ���s�i�ۑ��t�@�C���̔j�����A�f�[�^�����������j
    }

	/// <summary>
	/// �ۑ��`���i����� BINARY �����g��Ȃ����O�̂��� JSON ���c���Ƃ��j
	/// </summary>
	public enum FORMAT
    {
        BINARY,
        JSON,
    }

    /// <summary>
	/// �V���A���C�Y����N���X�ŗv������ݒ�C���^�[�t�F�C�X
	/// </summary>
	public interface ISerializer
    {
        string magic { get; }       // �}�W�b�NNo. �������Ă��������̂ɂ���
        string fileName { get; }    // �ۑ���
        FORMAT format { get; }      // �ۑ��`��
        System.Type type { get; }   // JSON�f�V���A���C�Y�p�^�錾
        bool encrypt { get; }       // �Í������邩
        bool backup { get; }        // �o�b�N�A�b�v����邩

        ISerializer Clone();        // �C���X�^���X�̕���
    }

    /// <summary>
	/// �X���b�h�󂯓n���p���
	/// </summary>
	public struct DataInfo
    {
        public ISerializer serializer;      // �V���A���C�Y�N���X
        public string filePath;             // �ۑ���
        public FinishHandler finishHandler; // �A�N�Z�X�����n���h��
        public bool async;                  // �񓯊�
    }

    /// <summary>
    /// �ۑ������n���h��
    /// </summary>
    /// <param name="ret">����</param>
    /// <param name="serializer">�ۑ��N���X�Q��</param>
    public delegate void FinishHandler(IO_RESULT ret, ref DataInfo dataInfo);

    public sealed class StorageManager
	{
        #region MEMBER
        private const int HASH_SIZE = 16;   // MD5��128bit
        private const int SALT_SIZE = 32;   // �\���g�̒���(byte) ��AesManaged�̗v���l�ɌŒ�
        private const int IV_SIZE = 16;     // �������x�N�^�[�̒���(byte) ��AesManaged�̗v���l�ɌŒ�
        private const int ITERATIONS = 50;  // �����قǋ��x���������������Ԃƃg���[�h�I�t
        private const string BACKUP_KEY = ".dup";       // �o�b�N�A�b�v�t�@�C���ǉ��L�[
        private volatile object sync = new object();    // �����I�u�W�F�N�g

        private readonly WaitCallback saveThreadHandler = null; // �ۑ������f���Q�[�g�L���b�V��
        private readonly WaitCallback loadThreadHandler = null; // �Ǎ������f���Q�[�g�L���b�V��
        private readonly MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider(); // ������`�F�b�N�p�n�b�V���W�F�l���[�^
        private readonly Rfc2898DeriveBytes deriveBytes = null;         // �L�[�쐬�p�����W�F�l���[�^
        private readonly byte[] mySalt = null;                          // �X�V�\���g�L���b�V��
        private readonly byte[] saltBuffer = new byte[SALT_SIZE];       // �\���g�Ǎ��p�o�b�t�@
        private readonly byte[] ivBuffer = new byte[IV_SIZE];           // �������x�N�^�[�Ǎ��p�o�b�t�@
        private readonly BinaryFormatter bf = new BinaryFormatter();    // �N���X�V���A���C�U�[

        private bool isAccessing = false;               // �ۑ����t���O�i�K���N���e�B�J���Z�N�V�������͂�j
		#endregion

		#region MAIN FUNCTION
		/// <summary>
		/// �R���X�g���N�^
		/// </summary>
		public StorageManager()
		{
			// �f���Q�[�g�L���b�V���i�Öق̃A���P�[�g�̍팸�j
			this.saveThreadHandler = new WaitCallback(this.SaveThreadMain);
			this.loadThreadHandler = new WaitCallback(this.LoadThreadMain);
			// �p�X���[�h��PASSWORD���̕ϐ����▼�O�Ŏ��Ɣ�����₷���̂Œ���
			// ����\���g�l�̓A�v���N�����ɍX�V
			this.deriveBytes = new Rfc2898DeriveBytes("10cA1_IJUf_ZOUn118_DeM0", SALT_SIZE, ITERATIONS);
			this.mySalt = this.deriveBytes.Salt;

#if UNITY_IOS
			// �V���A���C�Y�iBinaryFormatter�j��L���ɂ�����
			System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
		}

		/// <summary>
		/// �V���A���C�Y�ۑ�
		/// </summary>
		/// <param name="saveInterface">�V���A���C�Y����N���X</param>
		/// <param name="finishHandler">�I���n���h���inull�w��j</param>
		/// <param name="async">�񓯊����s���邩</param>
		public void Save(ISerializer saveInterface, FinishHandler finishHandler, bool async = true)
		{
			DataInfo dataInfo = new DataInfo();
			dataInfo.serializer = saveInterface;
#if UNITY_EDITOR 
			dataInfo.filePath = Directory.GetCurrentDirectory() + saveInterface.fileName; //Editor��ł͕��ʂɃJ�����g�f�B���N�g�����m�F���A�V���Ƀ��[�J���X�g���[�W�p�̃t�@�C�����쐬
#else
			dataInfo.filePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + saveInterface.fileName;//EXE�����s�����J�����g�f�B���N�g�� (�V���[�g�J�b�g���ŃJ�����g�f�B���N�g�����ς��̂ł��̕�����)
#endif
			dataInfo.finishHandler = finishHandler;
			dataInfo.async = async;

			if (saveInterface == null || string.IsNullOrEmpty(saveInterface.fileName))
			{
				this.FinishAccessing(IO_RESULT.NONE, ref dataInfo);
				return;
			}

			// �񓯊�����
			if (async)
				ThreadPool.QueueUserWorkItem(this.saveThreadHandler, dataInfo);
			else
				this.SaveThreadMain(dataInfo);
		}

		/// <summary>
		/// �f�V���A���C�Y�Ǎ�
		/// </summary>
		/// <param name="saveInterface">�f�V���A���C�Y����N���X</param>
		/// <param name="finishHandler">�I���n���h���inull�w��j</param>
		/// <param name="async">�񓯊��Ŏ��s���邩</param>
		public void Load(ISerializer loadInterface, FinishHandler finishHandler, bool async = true)
		{
			DataInfo dataInfo = new DataInfo();
			dataInfo.serializer = loadInterface;
#if UNITY_EDITOR
			dataInfo.filePath = Directory.GetCurrentDirectory() + loadInterface.fileName; //Editor��ł͕��ʂɃJ�����g�f�B���N�g�����m�F���A�V���Ƀ��[�J���X�g���[�W�p�̃t�@�C�����쐬
#else
			dataInfo.filePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + loadInterface.fileName;//EXE�����s�����J�����g�f�B���N�g�� (�V���[�g�J�b�g���ŃJ�����g�f�B���N�g�����ς��̂ł��̕�����)
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
		/// �폜
		/// </summary>
		/// <param name="serializer">�V���A���C�Y�N���X</param>
		public void Delete(ISerializer serializer)
		{
			// ���[�h�����瑦�폜
#if UNITY_EDITOR
			File.Delete(Directory.GetCurrentDirectory() + serializer.fileName); //Editor��ł͕��ʂɃJ�����g�f�B���N�g�����m�F���A�V���Ƀ��[�J���X�g���[�W�p�̃t�@�C�����쐬
#else
			File.Delete(AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + serializer.fileName); //EXE�����s�����J�����g�f�B���N�g�� (�V���[�g�J�b�g���ŃJ�����g�f�B���N�g�����ς��̂ł��̕�����)
#endif
        }

        /// <summary>
        /// �t�@�C�������邩
        /// </summary>
        /// <param name="serializer">�V���A���C�Y�N���X</param>
        public bool Exists(ISerializer serializer)
		{
#if UNITY_EDITOR
			string path = Directory.GetCurrentDirectory() + serializer.fileName; //Editor��ł͕��ʂɃJ�����g�f�B���N�g�����m�F���A�V���Ƀ��[�J���X�g���[�W�p�̃t�@�C�����쐬
#else
			string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + serializer.fileName;//EXE�����s�����J�����g�f�B���N�g�� (�V���[�g�J�b�g���ŃJ�����g�f�B���N�g�����ς��̂ł��̕�����)
#endif
			if (File.Exists(path))
			{
				FileInfo fi = new FileInfo(path);
				// ���t�@�C���������ݒ��ɃN���b�V�������0byte�̃t�@�C�����o����
				if (fi.Length > 0)
					Debug.Log("���[�J���X�g���[�W => ���݂���"); return true;
			}

			Debug.Log("���[�J���X�g���[�W => ���݂��Ȃ�");
			return false;
		}
		#endregion

		#region PUBLIC_FUNCTION
		/// <summary>
		/// �t�@�C���A�N�Z�X����
		/// </summary>
		public bool IsAccessing()
		{
			// �������N�G�X�g�����Ă������悤�ɊO������̎Q�Ƃ����b�N
			lock (this.sync)
			{
				return this.isAccessing;
			}
		}
		#endregion

		#region PRIVATE FUNCTION
		/// <summary>
		/// �X�g���[�W�A�N�Z�X�I������
		/// </summary>
		/// <param name="ret">����</param>
		/// <param name="dataInfo">�ۑ����</param>
		private void FinishAccessing(IO_RESULT ret, ref DataInfo dataInfo)
		{
			// ���s���̓o�b�N�A�b�v���Q��
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
					// �o�b�N�A�b�v����
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
		/// �ۑ�����
		/// </summary>
		private void SaveThreadMain(object state)
		{
			DataInfo dataInfo = (DataInfo)state;
			int size = 0;
			byte[] hash = null;
			byte[] binary = null;

			// ���̃��N�G�X�g�����҂�
			while (dataInfo.async && this.IsAccessing())
			{
				Thread.Sleep(30);
			}

			lock (this.sync)
			{
				this.isAccessing = true;
			}

			// �������ރo�C�i���f�[�^�̍쐬
			try
			{
				using (MemoryStream inMs = new MemoryStream())
				{
					switch (dataInfo.serializer.format)
					{
						case FORMAT.BINARY:
							// �V���A���C�Y
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

			// �X�g���[�W����
			try
			{
				using (FileStream outFs = File.Create(dataInfo.filePath))
				{
					using (BinaryWriter writer = new BinaryWriter(outFs))
					{
						// �w�b�_����
						writer.Write(dataInfo.serializer.magic);
						writer.Write(size);
						writer.Write(hash);
#if DEMO
						writer.Write(dataInfo.serializer.encrypt);
						writer.Write((int)dataInfo.serializer.format);
#endif

						// �Í������ď���
						if (dataInfo.serializer.encrypt)
						{
							bool success = this.EncryptFile(outFs, binary, size);
							// �Í������s
							if (!success)
							{
								this.FinishAccessing(IO_RESULT.SAVE_FAILED, ref dataInfo);
								return;
							}
						}
						// ���̂܂܏���
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

			// ����
			this.FinishAccessing(IO_RESULT.SAVE_SUCCESS, ref dataInfo);

#if DEBUG
			// �ۑ��t�@�C���T�C�Y�m�F
			FileInfo fi = new FileInfo(dataInfo.filePath);
			Debug.Log("..........FILE_SIZE " + fi.Length + "\n..........SAVE_SIZE " + size);
#endif
		}

		/// <summary>
		/// �Ǎ�����
		/// </summary>
		private void LoadThreadMain(object state)
		{
			DataInfo dataInfo = (DataInfo)state;
			int size = 0;
			byte[] hash = null;
			byte[] binary = null;
			bool encrypt = dataInfo.serializer.encrypt;
			FORMAT format = dataInfo.serializer.format;

			// ���̃��N�G�X�g�����҂�
			while (dataInfo.async && this.IsAccessing())
			{
				Thread.Sleep(30);
			}

			lock (this.sync)
			{
				this.isAccessing = true;
			}

			// �X�g���[�W�Ǎ�
			try
			{
				using (FileStream inFs = File.OpenRead(dataInfo.filePath))
				{
					using (BinaryReader reader = new BinaryReader(inFs))
					{
						// �w�b�_�Ǎ�
						string magic = reader.ReadString();
						// �}�W�b�NNo.�s��v
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

						// ���������ēǍ�
						if (encrypt)
						{
							bool success = this.DecryptFile(inFs, binary, size);
							// ���������s
							if (!success)
							{
								this.FinishAccessing(IO_RESULT.LOAD_FAILED, ref dataInfo);
								return;
							}
						}
						// ���̂܂ܓǍ�
						else
						{
							inFs.Read(binary, 0, size);
						}
					}

					// �n�b�V���`�F�b�N
					if (!this.CheckHash(this.md5.ComputeHash(binary), hash))
					{
						Debug.LogWarning("HASH MISMATCH\n");
						this.FinishAccessing(IO_RESULT.LOAD_FAILED, ref dataInfo);
						return;
					}

					// �f�V���A���C�Y
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

			// ����
			this.FinishAccessing(IO_RESULT.LOAD_SUCCESS, ref dataInfo);

#if DEBUG
			// �Ǎ��t�@�C���T�C�Y�m�F
			FileInfo fi = new FileInfo(dataInfo.filePath);
			Debug.Log("..........FILE_SIZE " + fi.Length + "\n..........SAVE_SIZE " + size);
#endif
		}

		/// <summary>
		/// �t�@�C�����Í������ď�������
		/// </summary>
		/// <param name="outFs">�ۑ�����t�@�C���X�g���[��</param>
		/// <param name="bs">�ۑ�����o�C�g��</param>
		/// <param name="size">�ۑ�����T�C�Y</param>
		private bool EncryptFile(Stream outFs, byte[] bs, int size)
		{
			bool success = true;

			using (AesManaged aes = new AesManaged())
			{
				try
				{
					// ����̋N�����̃\���g������
					this.deriveBytes.Salt = this.mySalt;
					this.deriveBytes.Reset();
					// �p�X���[�h����L�[�̍쐬�A�\���g�͎����쐬�Ƃ���
					outFs.Write(this.deriveBytes.Salt, 0, SALT_SIZE);
					aes.Key = this.deriveBytes.GetBytes(aes.KeySize / 8);

					// �������x�N�^�[�̐���
					aes.GenerateIV();
					outFs.Write(aes.IV, 0, IV_SIZE);

					// �Í�����������
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
		/// �t�@�C���𕜍������ēǂݍ���
		/// </summary>
		/// <param name="inFs">�ǂݍ��ރt�@�C���X�g���[��</param>
		/// <param name="bs">�ǂݍ��ރo�C�g��</param>
		/// <param name="size">�ǂݍ��ރT�C�Y</param>
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

					// �������ǂݍ���
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
		/// �n�b�V���`�F�b�N
		/// </summary>
		/// <returns>��v������</returns>
		/// <param name="a">�`�F�b�N�Ώ�</param>
		/// <param name="b">�`�F�b�N�Ώ�</param>
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

