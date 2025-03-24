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
    private string jsonFilePath = "";                                  //JSON 파일 경로 문자열 값
    private string outputFolder = "Assets/ScriptableObjects/items";    //출력 SO파일을 경로 값
    private bool createDatabase = true;                                //데이터 베이스를 사용할 것인지에 대한 bool 값

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

    private void ConvertJsonToSriptableObjects()                      //JSON 파일을 ScriptableObject 파일로 변환 시켜주는 함수
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
}
#endif
