using UnityEngine;

// 道具資料：在 Project 視窗右鍵 > Create > Game > Item 建立
[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "道具";
    public Sprite icon;
    [TextArea] public string description;
}
