using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public GameObject Selected;
    public GameObject Toolbelt;

    public Texture DirtTexture;
    public Texture WoodTexture;
    public Texture StoneTexture;

    private List<Blocks> _blocks = new List<Blocks>();
    private List<ToolbeltRef> _toolbeltRefs = new List<ToolbeltRef>();
    private int _selectedIndex;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        GetToolbeltRefs();
    }

    void Update()
    {
        bool scrolled = false;
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            _selectedIndex++;
            scrolled = true;
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            _selectedIndex--;
            scrolled = true;
        }

        if (scrolled)
        {
            List<InventoryBlock> inventory = GetInventory();
            if (_selectedIndex >= inventory.Count)
            {
                _selectedIndex = 0;
            }
            else if (_selectedIndex < 0)
            {
                _selectedIndex = Mathf.Max(0, inventory.Count - 1);
            }

            UpdateInventory();
        }
    }

    public void Add(Blocks block)
    {
        if (block != Blocks.Empty)
        {
            _blocks.Add(block);

            UpdateInventory();
        }
    }

    public void Remove(Blocks block)
    {
        if (block != Blocks.Empty)
        {
            _blocks.Remove(block);

            UpdateInventory();
        }
    }

    public Blocks GetSelectedBlock()
    {
        List<InventoryBlock> inventory = GetInventory();
        if (_selectedIndex >= inventory.Count)
        {
            return Blocks.Empty;
        }

        return inventory[_selectedIndex].Block;
    }

    private void GetToolbeltRefs()
    {
        for (int i = 0; i < 9; i++)
        {
            Transform slot = Toolbelt.transform.Find((i + 1).ToString());

            RawImage image = slot.Find("Material").GetComponent<RawImage>();
            Text text = slot.Find("Amount").GetComponent<Text>();

            _toolbeltRefs.Add(new ToolbeltRef
            {
                Position = slot.position,
                Image = image,
                Text = text
            });
        }

        UpdateInventory();
    }

    private void UpdateInventory()
    {
        List<InventoryBlock> inventory = GetInventory();

        if (_selectedIndex > 0 && inventory.Count <= _selectedIndex)
        {
            _selectedIndex--;
        }

        for (int i = 0; i < 9; i++)
        {
            RawImage image = _toolbeltRefs[i].Image;
            Text text = _toolbeltRefs[i].Text;

            if (i == _selectedIndex && inventory.Count > 0)
            {
                Selected.transform.position = _toolbeltRefs[i].Position;
                Selected.SetActive(true);
            }
            else if (inventory.Count == 0)
            {
                Selected.SetActive(false);
            }

            if (inventory.Count > i)
            {
                image.gameObject.SetActive(true);

                Texture texture = null;
                if (inventory[i].Block == Blocks.Dirt)
                {
                    texture = DirtTexture;
                }
                else if (inventory[i].Block == Blocks.Wood)
                {
                    texture = WoodTexture;
                }
                else if (inventory[i].Block == Blocks.Stone)
                {
                    texture = StoneTexture;
                }

                image.texture = texture;
                if (inventory[i].Count > 1)
                {
                    text.text = inventory[i].Count.ToString();
                    text.gameObject.SetActive(true);
                }
                else
                {
                    text.gameObject.SetActive(false);
                }
            }
            else
            {
                image.gameObject.SetActive(false);
                text.gameObject.SetActive(false);
            }
        }
    }

    private List<InventoryBlock> GetInventory()
    {
        List<InventoryBlock> inventory = new List<InventoryBlock>();

        IEnumerable<IGrouping<Blocks, Blocks>> orderedBlocks = _blocks.GroupBy(b => b);
        foreach (IGrouping<Blocks, Blocks> block in orderedBlocks)
        {
            inventory.Add(new InventoryBlock
            {
                Block = block.Key,
                Count = block.Count()
            });
        }

        return inventory;
    }
}

public class InventoryBlock
{
    public Blocks Block { get; set; }
    public int Count { get; set; }
}

public class ToolbeltRef
{
    public Vector3 Position { get; set; }
    public RawImage Image { get; set; }
    public Text Text { get; set; }
}