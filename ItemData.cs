using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;

    [Header("Prefab Settings")]
    public GameObject itemPrefab;
    public GameObject worldPrefab;
}
