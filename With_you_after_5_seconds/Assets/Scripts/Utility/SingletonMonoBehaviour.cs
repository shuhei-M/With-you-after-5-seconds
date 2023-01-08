using System;
using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _Instance;

	public static bool Exists
	{
		get
		{
			return _Instance != null;
		}
	}

	public static T Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = FindObjectOfType<T>();
				if (_Instance == null)
				{
					Debug.Log("���ǉ�����Ă���GameObject�����݂��܂���B");
				}
			}

			return _Instance;
		}
	}

	virtual protected void Awake()
	{
		if (this != Instance)
		{
			Destroy(this);
			Debug.Log("�͊��ɑ���GameObject�� �ǉ�����Ă��邽�߁A�R���|�[�l���g��j�����܂����B\n"
				+" �A�^�b�`����Ă���GameObject��" + Instance.gameObject.name + " �ł��B\n");
			return;
		}
	}
}
