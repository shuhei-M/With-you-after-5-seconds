using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : SingletonMonoBehaviour<EffectManager>
{
	/// <summary> ソースを書くときのレンプレート </summary>

	#region define
	/// <summary>
	/// エフェクトID ( 手動で追加する)
	/// </summary>
	public enum EffectID : int
	{
		None = -1,
		Teleportation,
		Float,
		BookLight,
		Fire,
	}
	#endregion

	#region serialize field
	[SerializeField] private List<GameObject> _EffectList = new List<GameObject>();
	#endregion

	#region field

	#endregion

	#region property

	#endregion

	#region Unity function
	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		
	}
	#endregion

	#region public function
	/// <summary>
	/// エフェクト再生
	/// </summary>
	/// <param name="id">エフェクトID</param>
	/// <param name="position">再生ポジション</param>
	/// <returns></returns>
	public GameObject Play(EffectID id, Vector3 position)
	{
		if (id == EffectID.None)
		{
			return null;
		}

		var index = (int)id;
		var prefab = _EffectList[index];
		if (index < 0 || _EffectList.Count <= index)
		{
			Debug.Log("indexが不正な値です!!!!!!!!!!!!!!!!!!!!!!");
			return null;
		}
		if (prefab == null)
		{
			Debug.Log("prefabが設定されていません!!!!!!!!!!!!!!!");
			return null;
		}
		var obj = Instantiate(prefab, position, Quaternion.identity);
		obj.transform.SetParent(transform);
		return obj;
	}
	#endregion

	#region private function

	#endregion
}
