using UnityEngine;

[CreateAssetMenu(fileName = "item", menuName = "NewItem")]
public class FP_Item : ScriptableObject
{
    public string ItemName;
    public Sprite Icon;
    public int maxStackSize;
    public GameObject itemPrefab;
    public GameObject handItemPrefab;
}
