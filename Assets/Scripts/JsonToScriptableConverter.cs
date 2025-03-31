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
    public int? id;                //int?는 Nullable<int>의 축약 표현이다. 선언하면 null 값도 가질 수 있는 정수형이 됩니다.
    public string characterName;
    public string text;
    public int? nextId;
    public string protraitPath;
    public string choiceText;
    public int? choiceNextId;
}

public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePath = "";                                  //JSON 파일 경로 문자열 값
    private string outputFolder = "Assets/ScriptableObjects/items";    //출력 SO파일을 경로 값
    private bool createDatabase = true;                                //데이터 베이스를 사용할 것인지에 대한 bool 값
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

        //변환 타입을 선택
        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type:", conversionType);

        //타입에 따라 기본 출력 폴더 설정
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

    private void ConvertJsonToItemSriptableObjects()                      //JSON 파일을 ScriptableObject 파일로 변환 시켜주는 함수
    {
        //폴더 생성
        if (!Directory.Exists(outputFolder))                         //폴더 위치를 확인하고 없으면 생성한다.
        {
            Directory.CreateDirectory(outputFolder);
        }
        //JSON 파일 읽기
        string jsonText = File.ReadAllText(jsonFilePath);            //JSON 파일을 읽는다.

        try
        {
            //JSON 파싱
            List<ItemData> itemDatasList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdItems = new List<ItemSO>();          //ItemSO 리스트 생성

            //각 아이템 데이터를 스크립터블 오브젝트로 변환
            foreach (ItemData itemData in itemDatasList)
            {
                ItemSO itemSo = ScriptableObject.CreateInstance<ItemSO>();

                //데이터 복사
                itemSo.id = itemData.id;
                itemSo.itemName = itemData.itemName;
                itemSo.nameEng = itemData.nameEng;
                itemSo.description = itemData.description;

                //열거형 변환
                if (Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSo.itemType = parsedType;
                }
                else
                {
                    Debug.Log($"아이템 {itemData.itemName}에 유호하는 않는 아이템타입 : {itemData.itemTypeString}");
                }
                itemSo.price = itemData.price;
                itemSo.power = itemData.power;
                itemSo.level = itemData.level;
                itemSo.isStackable = itemData.isStackable;

                //아이콘 로드(경로가 있는 경우)
                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSo.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSo.icon == null)
                    {
                        Debug.LogWarning($"아이템 '{itemData.nameEng}'의 아이콘을 찾을 수 없습니다. : {itemData.iconPath}");
                    }
                }

                //스크립터블 오브젝트 저장 - ID를 4자리 숫자로 포멧팅
                string assetPath = $"{outputFolder}/item_{itemData.id.ToString("D4")}_{itemData.nameEng}.asset";
                AssetDatabase.CreateAsset( itemSo, assetPath );

                //에셋 이름 지정
                itemSo.name = $"Item_{itemData.id.ToString("D4")} + {itemData.nameEng}";
                createdItems.Add(itemSo );

                EditorUtility.SetDirty( itemSo );

            }

            //데이터베이스 생성
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
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }

    //대화 JSON을 스크립터블 오브젝트로 변환
    private void ConvertJsonToDialogScriptaleObjects()
    {
        //폴더 생성
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON 파일 읽기
        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            //JSON 파싱
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(jsonText);

            //대화 데이터 재구성
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> creatDialogs = new List<DialogSO>();

            //1단계 : 대화 항복생성
            foreach (var rowData in rowDataList)
            {
                //id 있는 행은 대화로 처리
                if (rowData.id.HasValue)
                {
                    DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                    //데이터 복사
                    dialogSO.id = rowData.id.Value;
                    dialogSO.characterName = rowData.characterName;
                    dialogSO.text = rowData.text;
                    dialogSO.nextld = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                    dialogSO.portraitPath = rowData.protraitPath;
                    dialogSO.choices = new List<DialogChoiceSO>();

                    //초상화 로드(경로가 있는 경우)
                    if (!string.IsNullOrEmpty(rowData.protraitPath))
                    {
                        dialogSO.portrait = Resources.Load<Sprite>(rowData.protraitPath);

                        if (dialogSO.portrait == null)
                        {
                            Debug.Log($"대화 {rowData.id}의 초상화를 찾을 수 없습니다");
                        }
                    }
                    //dialogMap에 추가
                    dialogMap[dialogSO.id] = dialogSO;
                    creatDialogs.Add(dialogSO);
                }
            }
            
            //2단계 : 선택지 항목 처리 및 열결
            foreach (var rowData in rowDataList)
            {
                //id가 없고 choiceText가 있는 행은 선택지로 사용
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextId.HasValue)
                {
                    //이전 행의 ID를 부모 ID로 사용(연속되는 선택지의 경우)
                    int parentId = -1;

                    //이전 선택지 바로 위에 있는 대화 (id가 있는 항목)을 찾음
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }

                    //부모 ID를 찾지 못했거나 부모 ID가 -1인 경우 (첫번째 항목)
                    if (parentId == -1)
                    {
                        Debug.LogWarning($"선택지 '{rowData.choiceText}'의 부모 대화를 찾을 수 없습니다.");
                    }
                    if (dialogMap.TryGetValue(parentId, out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextId = rowData.choiceNextId.Value;

                        //선택지 에셋 저장
                        string choiceAssetPath = $"{outputFolder}/Choice_{parentId}_{parentDialog.choices.Count + 1}.asset";

                        Debug.Log(choiceAssetPath);

                        AssetDatabase.CreateAsset(choiceSO, choiceAssetPath );
                        EditorUtility.SetDirty(choiceSO);

                        parentDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.Log($"선택지 '{rowData.choiceText}'를 연결할 대화 (ID : {parentId})를 찾을 수 없습니다.");
                    }
                }
            }

            //3단계 : 대화 스크립터블 오브젝트 저장
            foreach (var dialog in creatDialogs)
            {
                //스크립터블 오브젝트 저장 - ID를 4자리 숫자로 포맷팅
                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset( dialog, assetPath );

                //에셋 이름 저장
                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty ( dialog );
            }

            //데이터 베이스 생성
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
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }
}
#endif
