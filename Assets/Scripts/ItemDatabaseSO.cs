using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Database")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();        //ItemSO�� ����Ʈ�� �����Ѵ�.

    //ĳ���� ���� ����
    private Dictionary<int, ItemSO> itemsById;             //ID�� ������ ã�� ���� ĳ��
    private Dictionary<string, ItemSO> itemsByName;        //�̸����� ������ ã��

    public void Initionalize()                             //�ʱ� ���� �Լ�
    {
        itemsById = new Dictionary<int, ItemSO>();         //���� ���� �߱� ������ Dictionary �Ҵ�
        itemsByName = new Dictionary<string, ItemSO>();

        foreach (var item in items)                        //items ����Ʈ�� ���� �Ǿ� �ִ� ���� ������ Dictionary�� �Է��Ѵ�.
        {
            itemsById[item.id] = item;
            itemsByName[item.name] = item;
        }
    }

    //ID�� ������ ã��
    public ItemSO GetItemById(int id)
    {
        if (itemsById == null)                          //itemsById�� ĳ���� �Ǿ� ���� �ʴٸ� �ʱ�ȭ �Ѵ�. 
        {
            Initionalize();
        }
        if(itemsById.TryGetValue(id, out var item))     //id ���� ã�Ƽ� ItemSO�� �����Ѵ�.
            return item;

        return null;                                    //���� ��� Null
    }

    public ItemSO GetItemByName(string name)
    {
        if (itemsByName == null)
        {
            Initionalize();
        }
        if(itemsByName.TryGetValue(name, out var item))
            return item;

        return null;
    }

    //Ÿ������ ������ ���͸�
    public List<ItemSO> GetItemByType(ItemType type)
    {
        return items.FindAll(item => item.itemType == type);
    }
}
