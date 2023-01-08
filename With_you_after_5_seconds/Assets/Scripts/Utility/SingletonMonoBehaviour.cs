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
					Debug.Log("が追加されているGameObjectが存在しません。");
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
			Debug.Log("は既に他のGameObjectに 追加されているため、コンポーネントを破棄しました。\n"
				+" アタッチされているGameObjectは" + Instance.gameObject.name + " です。\n");
			return;
		}
	}
}
