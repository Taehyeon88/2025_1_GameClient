#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePath = "";                                  //JSON ���� ��� ���ڿ� ��
    private string outputFolder = "Assets/ScriptableObjects/items";    //��� SO������ ��� ��
    private bool createDatabase = true;                                //������ ���̽��� ����� �������� ���� bool ��

    [MenuItem("Tools/JSON to Scriptable Objects")]

    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objects");
    }

    private void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable Object Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Select JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }

        EditorGUILayout.LabelField("Selected File : ", jsonFilePath);
        EditorGUILayout.Space();
        outputFolder = EditorGUILayout.TextField("Output Folder :", outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Database Asset", createDatabase);
        EditorGUILayout.Space();

        if (GUILayout.Button("Convert to Scriptable Objects"))
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a JSON file firest!", "OK");
                return;
            }
            ConvertJsonToSriptableObjects();
        }
    }

    private void ConvertJsonToSriptableObjects()                      //JSON ������ ScriptableObject ���Ϸ� ��ȯ �����ִ� �Լ�
    {
        //���� ����
        if (!Directory.Exists(outputFolder))                         //���� ��ġ�� Ȯ���ϰ� ������ �����Ѵ�.
        {
            Directory.CreateDirectory(outputFolder);
        }
        //JSON ���� �б�
        string jsonText = File.ReadAllText(jsonFilePath);            //JSON ������ �д´�.

        try
        {
            //JSON �Ľ�
            List<ItemData> itemDatasList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdItems = new List<ItemSO>();          //ItemSO ����Ʈ ����

            //�� ������ �����͸� ��ũ���ͺ� ������Ʈ�� ��ȯ
            foreach (ItemData itemData in itemDatasList)
            {
                ItemSO itemSo = ScriptableObject.CreateInstance<ItemSO>();

                //������ ����
                itemSo.id = itemData.id;
                itemSo.itemName = itemData.itemName;
                itemSo.nameEng = itemData.nameEng;
                itemSo.description = itemData.description;

                //������ ��ȯ
                if (Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSo.itemType = parsedType;
                }
                else
                {
                    Debug.Log($"������ {itemData.itemName}�� ��ȣ�ϴ� �ʴ� ������Ÿ�� : {itemData.itemTypeString}");
                }
                itemSo.price = itemData.price;
                itemSo.power = itemData.power;
                itemSo.level = itemData.level;
                itemSo.isStackable = itemData.isStackable;

                //������ �ε�(��ΰ� �ִ� ���)
                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSo.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSo.icon == null)
                    {
                        Debug.LogWarning($"������ '{itemData.nameEng}'�� �������� ã�� �� �����ϴ�. : {itemData.iconPath}");
                    }
                }

                //��ũ���ͺ� ������Ʈ ���� - ID�� 4�ڸ� ���ڷ� ������
                string assetPath = $"{outputFolder}/item_{itemData.id.ToString("D4")}_{itemData.nameEng}.asset";
                AssetDatabase.CreateAsset( itemSo, assetPath );

                //���� �̸� ����
                itemSo.name = $"Item_{itemData.id.ToString("D4")} + {itemData.nameEng}";
                createdItems.Add(itemSo );

                EditorUtility.SetDirty( itemSo );

            }

            //�����ͺ��̽� ����
            if (createDatabase && createdItems.Count > 0)
            {
                ItemDatabaseSO database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
                database.items = createdItems;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/ItemDatabase.asset");
                EditorUtility.SetDirty( database );
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Sucess", $"Created {createdItems.Count} scriptable objects!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error" , $"Failed to Convert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ���� : {e}");
        }
    }
}
#endif
