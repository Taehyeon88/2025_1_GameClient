#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

public enum ConversionType
{
    Items,
    Dialogs
}
[Serializable]
public class DialogRowData
{
    public int? id;                //int?�� Nullable<int>�� ��� ǥ���̴�. �����ϸ� null ���� ���� �� �ִ� �������� �˴ϴ�.
    public string characterName;
    public string text;
    public int? nextId;
    public string protraitPath;
    public string choiceText;
    public int? choiceNextId;
}

public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePath = "";                                  //JSON ���� ��� ���ڿ� ��
    private string outputFolder = "Assets/ScriptableObjects/items";    //��� SO������ ��� ��
    private bool createDatabase = true;                                //������ ���̽��� ����� �������� ���� bool ��
    private ConversionType conversionType = ConversionType.Items;


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

        //��ȯ Ÿ���� ����
        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type:", conversionType);

        //Ÿ�Կ� ���� �⺻ ��� ���� ����
        if (conversionType == ConversionType.Items)
        {
            outputFolder = "Assets/ScriptableObjects/items";
        }
        else if(conversionType == ConversionType.Dialogs)
        {
            outputFolder = "Assets/ScriptableObjects/Dialogs";
        }

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

            switch (conversionType)
            {
                case ConversionType.Items:
                    ConvertJsonToItemSriptableObjects();
                    break;
                case ConversionType.Dialogs:
                    ConvertJsonToDialogScriptaleObjects();
                    break;
            }
        }
    }

    private void ConvertJsonToItemSriptableObjects()                      //JSON ������ ScriptableObject ���Ϸ� ��ȯ �����ִ� �Լ�
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

    //��ȭ JSON�� ��ũ���ͺ� ������Ʈ�� ��ȯ
    private void ConvertJsonToDialogScriptaleObjects()
    {
        //���� ����
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON ���� �б�
        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            //JSON �Ľ�
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(jsonText);

            //��ȭ ������ �籸��
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> creatDialogs = new List<DialogSO>();

            //1�ܰ� : ��ȭ �׺�����
            foreach (var rowData in rowDataList)
            {
                //id �ִ� ���� ��ȭ�� ó��
                if (rowData.id.HasValue)
                {
                    DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                    //������ ����
                    dialogSO.id = rowData.id.Value;
                    dialogSO.characterName = rowData.characterName;
                    dialogSO.text = rowData.text;
                    dialogSO.nextld = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                    dialogSO.portraitPath = rowData.protraitPath;
                    dialogSO.choices = new List<DialogChoiceSO>();

                    //�ʻ�ȭ �ε�(��ΰ� �ִ� ���)
                    if (!string.IsNullOrEmpty(rowData.protraitPath))
                    {
                        dialogSO.portrait = Resources.Load<Sprite>(rowData.protraitPath);

                        if (dialogSO.portrait == null)
                        {
                            Debug.Log($"��ȭ {rowData.id}�� �ʻ�ȭ�� ã�� �� �����ϴ�");
                        }
                    }
                    //dialogMap�� �߰�
                    dialogMap[dialogSO.id] = dialogSO;
                    creatDialogs.Add(dialogSO);
                }
            }
            
            //2�ܰ� : ������ �׸� ó�� �� ����
            foreach (var rowData in rowDataList)
            {
                //id�� ���� choiceText�� �ִ� ���� �������� ���
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextId.HasValue)
                {
                    //���� ���� ID�� �θ� ID�� ���(���ӵǴ� �������� ���)
                    int parentId = -1;

                    //���� ������ �ٷ� ���� �ִ� ��ȭ (id�� �ִ� �׸�)�� ã��
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }

                    //�θ� ID�� ã�� ���߰ų� �θ� ID�� -1�� ��� (ù��° �׸�)
                    if (parentId == -1)
                    {
                        Debug.LogWarning($"������ '{rowData.choiceText}'�� �θ� ��ȭ�� ã�� �� �����ϴ�.");
                    }
                    if (dialogMap.TryGetValue(parentId, out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextId = rowData.choiceNextId.Value;

                        //������ ���� ����
                        string choiceAssetPath = $"{outputFolder}/Choice_{parentId}_{parentDialog.choices.Count + 1}.asset";

                        Debug.Log(choiceAssetPath);

                        AssetDatabase.CreateAsset(choiceSO, choiceAssetPath );
                        EditorUtility.SetDirty(choiceSO);

                        parentDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.Log($"������ '{rowData.choiceText}'�� ������ ��ȭ (ID : {parentId})�� ã�� �� �����ϴ�.");
                    }
                }
            }

            //3�ܰ� : ��ȭ ��ũ���ͺ� ������Ʈ ����
            foreach (var dialog in creatDialogs)
            {
                //��ũ���ͺ� ������Ʈ ���� - ID�� 4�ڸ� ���ڷ� ������
                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset( dialog, assetPath );

                //���� �̸� ����
                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty ( dialog );
            }

            //������ ���̽� ����
            if (createDatabase && creatDialogs.Count > 0)
            {
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                database.dialogs = creatDialogs;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/DialogDatabase.asset");
                EditorUtility.SetDirty( database );
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Created {creatDialogs.Count} dialog scriptable objects", "Ok");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Faild to convert JSON: {e.Message}", "Ok");
            Debug.LogError($"JSON ��ȯ ���� : {e}");
        }
    }
}
#endif
