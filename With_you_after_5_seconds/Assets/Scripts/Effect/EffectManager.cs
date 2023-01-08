using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : SingletonMonoBehaviour<EffectManager>
{
	/// <summary> �\�[�X�������Ƃ��̃����v���[�g </summary>

	#region define
	/// <summary>
	/// �G�t�F�N�gID ( �蓮�Œǉ�����)
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
	/// �G�t�F�N�g�Đ�
	/// </summary>
	/// <param name="id">�G�t�F�N�gID</param>
	/// <param name="position">�Đ��|�W�V����</param>
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
			Debug.Log("index���s���Ȓl�ł�!!!!!!!!!!!!!!!!!!!!!!");
			return null;
		}
		if (prefab == null)
		{
			Debug.Log("prefab���ݒ肳��Ă��܂���!!!!!!!!!!!!!!!");
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
