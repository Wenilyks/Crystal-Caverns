using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UI_Inventory : MonoBehaviour
{
    private Inventory inventory;
    private Transform itemSlotContainer;
    private Transform itemSlotTemplate;
    private Item selectedItem;
    private Transform selectedItemSlotRectTransform;

    private void Awake()
    {
        itemSlotContainer = transform.Find("ItemSlotContainer");
        itemSlotTemplate = itemSlotContainer.Find("ItemSlotTemplate");
        if (itemSlotContainer == null)
        {
            Debug.Log("lol");
        }
    }
    public void SetInventory(Inventory inventory)
    {
        itemSlotContainer = transform.Find("ItemSlotContainer");
        itemSlotTemplate = itemSlotContainer.Find("ItemSlotTemplate");

        this.inventory = inventory;
        inventory.OnItemListChanged += () => RefreshInventoryItems();

        RefreshInventoryItems();
    }

    public void RefreshInventoryItems()
    {
        foreach (Transform child in itemSlotContainer)
        {
            if (child == itemSlotTemplate)
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        int x = 0;
        int y = 0;

        float itemSlotCellSizeX = 88;
        float itemSlotCellSizeY = 91;

        selectedItem = null;
        selectedItemSlotRectTransform = null;

        foreach (Item item in inventory.GetItems())
        {
            RectTransform itemSlotRectTransform = Instantiate(itemSlotTemplate, itemSlotContainer).GetComponent<RectTransform>();

            itemSlotRectTransform.gameObject.SetActive(true);
            itemSlotRectTransform.anchoredPosition = new Vector2(x * itemSlotCellSizeX, y * itemSlotCellSizeY * -1);
            Image image = itemSlotRectTransform.GetComponent<Image>();

            Button button = itemSlotRectTransform.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectItem(item, itemSlotRectTransform));

            TextMeshProUGUI uiText = itemSlotRectTransform.Find("ItemAmountTxt").GetComponent<TextMeshProUGUI>();
            if (uiText != null)
                Debug.Log("it is fine");
            if (item.amount > 1)
                uiText.SetText(item.amount.ToString());
            else
                uiText.SetText("");

                image.sprite = item.GetSprite();
            x++;

            if (x > 3)
            {
                x = 0;
                y++;
            }
        }
    }

    private void SelectItem(Item item, Transform itemSlotRectTransform)
    {
        Debug.Log("Item selected");
        selectedItem = item;
        selectedItemSlotRectTransform = itemSlotRectTransform;
    }

    public Item GetSelectedItem()
    {
        return selectedItem;
    }
}
