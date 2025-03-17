using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;

public class ItemDataLoader : MonoBehaviour
{
    [SerializeField]
    private string jsonFileName = "Items";            //Resourecs 폴더에서 가져올 JSON파일 이름

    private List<ItemData> itemList;

    // Start is called before the first frame update
    void Start()
    {
        LoadItemData();
    }

    void LoadItemData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName);  //TextAsset형태로 json파일을 로딩한다.

        if (jsonFile != null)                                          //파일을 읽을 때, null값 처리를 한다.
        {
            //원본 텍스트에서 UTF-8로 변환 처리
            byte[] bytes = Encoding.Default.GetBytes(jsonFile.text);
            string correntText = Encoding.UTF8.GetString(bytes);

            //변환된 텍스트 사용
            itemList = JsonConvert.DeserializeObject<List<ItemData>>(correntText);

            Debug.Log($"로드된 아이템 수 : {itemList.Count}");

            foreach (var item in itemList)
            {
                Debug.Log($"아이템 : {EncodeKorean(item.itemName)}, 설명 : {EncodeKorean(item.description)}");
            }
        }
        else
        {
            Debug.Log($"JSON 파일을 찾을 수 없습니다 : {jsonFileName}");
        }
    }

    //한글 인코딩을 위한 헬퍼 함수
    private string EncodeKorean(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";         //텍스트가 NULL값이면 함수를 끝낸다.
        byte[] bytes = Encoding.Default.GetBytes(text);    //string을 Byte배열로 변환한 후
        return Encoding.UTF8.GetString(bytes);             //인코딩을 UTF8로 바꾼다.
    }
}
