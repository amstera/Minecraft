using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crafting : MonoBehaviour
{
    public GameObject Panel;
    public List<CraftItem> CraftItems1;
    public List<int> CraftItemAmounts1;
    public List<CraftItem> CraftItems2;
    public List<int> CraftItemAmounts2;
    public List<CraftItem> CraftedItems;

    void OnEnable()
    {
        UpdateCrafting();
    }

    private void UpdateCrafting()
    {
        List<InventoryBlock> inventory = Inventory.Instance.GetInventoryBlocks();
        int i = 0;
        foreach (Transform crafting in Panel.transform)
        {
            int j = 0;
            foreach (Transform piece in crafting.transform)
            {
                RawImage rawImage = piece.GetComponent<RawImage>();
                Text text = piece.GetComponent<Text>();
                Button button = piece.GetComponent<Button>();

                if (rawImage != null && rawImage.texture == null)
                {
                    rawImage.texture = j == 0 ? CraftItems1[i].Image : j == 3 ? CraftItems2[i].Image : CraftedItems[i].Image;
                }
                else if (text != null && j < 6)
                {
                    text.text = j == 2 ? CraftItemAmounts1[i].ToString() : CraftItemAmounts2[i].ToString();
                }
                else if (button != null)
                {
                    int craftItemAmount1 = inventory.Find(inv => inv.Block == CraftItems1[i].Type)?.Count ?? 0;
                    int craftItemAmount2 = inventory.Find(inv => inv.Block == CraftItems2[i].Type)?.Count ?? 0;
                    button.interactable = craftItemAmount1 >= CraftItemAmounts1[i] && craftItemAmount2 >= CraftItemAmounts2[i];
                    button.onClick.RemoveAllListeners();
                    if (button.interactable)
                    {
                        int index = i;
                        button.onClick.AddListener(delegate { CraftItem(index); });
                    }
                    Color color = button.GetComponent<Image>().color;
                    color.a = button.interactable ? 1 : 0.25f;
                    button.GetComponent<Image>().color = color;
                }
                j++;
            }
            i++;
        }
    }

    private void CraftItem(int index)
    {
        for (int i = 0; i < CraftItemAmounts1[index]; i++)
        {
            Inventory.Instance.Remove(CraftItems1[index].Type);
        }
        for (int i = 0; i < CraftItemAmounts2[index]; i++)
        {
            Inventory.Instance.Remove(CraftItems2[index].Type);
        }
        Inventory.Instance.Add(CraftedItems[index].Type);

        UpdateCrafting();
    }
}

[System.Serializable]
public class CraftItem
{
    public Blocks Type;
    public Texture Image;
}
