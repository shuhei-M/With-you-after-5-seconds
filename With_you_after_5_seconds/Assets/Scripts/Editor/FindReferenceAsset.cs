using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FindAssetData
{
    public string AssetName { get; }
    public string AssetPath { get; }
    public string AssetGuid { get; }
    public bool Folout { get; set; }

    public FindAssetData(string name, string path, string guid)
    {
        AssetName = name;
        AssetPath = path;
        AssetGuid = guid;
    }
}

public class FindReferenceAsset : EditorWindow
{
    private const string findAssetsType =
        @"t:scene t:prefab t:timelineAsset t:animatorcontroller t:Material";

    private static Dictionary<FindAssetData, List<FindAssetData>> findResult
        = new Dictionary<FindAssetData, List<FindAssetData>>();

    private Vector2 scrollPosition = Vector2.zero;
    private bool foldout;

    [MenuItem("Assets/参照を探す", false)]
    public static void FindAssets()
    {
        Object[] selectList = Selection.objects;
        if (selectList == null || selectList.Length == 0)
        {
            Debug.Log("There are no selection objects");
            return;
        }
        findResult.Clear();

        // 選択したアセットのパスやGUIDを読み込む
        List<FindAssetData> selectAssets = new List<FindAssetData>();
        foreach (var selectItem in selectList)
        {
            string path = AssetDatabase.GetAssetPath(selectItem);
            string guid = AssetDatabase.AssetPathToGUID(path);
            selectAssets.Add(new FindAssetData(selectItem.name, path, guid));
        }

        // プロジェクト内にある、検索対象のアセットをすべて取得する
        var ids = AssetDatabase.FindAssets(findAssetsType);
        foreach (string assetGuid in ids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

            // 対象のアセットと依存関係にあるアセットをすべて取得する
            var deps = AssetDatabase.GetDependencies(assetPath);
            foreach (var dependAssetPath in deps)
            {
                if (assetPath == dependAssetPath) continue;

                string dependGuid = AssetDatabase.AssetPathToGUID(dependAssetPath);
                for (int i = 0; i < selectAssets.Count; i++)
                {
                    if (!findResult.ContainsKey(selectAssets[i]))
                    {
                        findResult.Add(selectAssets[i], new List<FindAssetData>());
                    }

                    // 選択したアセットと同じ GUID であれば、参照されているということになる
                    if (dependGuid == selectAssets[i].AssetGuid)
                    {
                        string hitName = AssetDatabase.LoadMainAssetAtPath(assetPath).name;
                        FindAssetData hit = new FindAssetData(hitName, assetPath, assetGuid);

                        findResult[selectAssets[i]].Add(hit);
                    }
                }
            }
        }

        GetWindow<FindReferenceAsset>();
    }

    private void OnGUI()
    {
        if (findResult == null || findResult.Count == 0) return;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (KeyValuePair<FindAssetData, List<FindAssetData>> pair in findResult)
        {
            int referenceCount = pair.Value == null ? 0 : pair.Value.Count;
            if (pair.Key.Folout = EditorGUILayout.Foldout(pair.Key.Folout, pair.Key.AssetName + " : " + referenceCount + "個"))
            {
                foreach (var item in pair.Value)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);

                    EditorGUIUtility.SetIconSize(Vector2.one * 16);

                    Object target = AssetDatabase.LoadAssetAtPath<Object>(item.AssetPath);

                    // アセットが Prefab なのか Sceneなのか判別が付きやすくするためにアイコンの表示
                    GUIContent guiContent = new GUIContent(
                        item.AssetName,
                        EditorGUIUtility.ObjectContent(target, target.GetType()).image
                    );

                    if (GUILayout.Button(guiContent, "Label"))
                    {
                        Selection.objects = new[] { target };
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
