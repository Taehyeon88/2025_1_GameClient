using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/item")]
public class ItemSO : ScriptableObject
{
    //기존 GemeDB Excel 에서 선언한 변수를 나열한다.
    public int id;
    public string itemName;
    public string nameEng;
    public string description;
    public ItemType itemType;
    public int price;
    public int power;
    public int level;
    public bool isStackable;
    public Sprite icon;        //실제로 사용할 스프라이트 선언

    public override string ToString()
    {
        return $"[{id}] {itemName} ({itemType}) - 가격 : {price}골드, 속성 : {power}";
    }

    public string DisplayName
    {
        get { return string.IsNullOrEmpty(nameEng) ? itemName : nameEng; }
    }

}
