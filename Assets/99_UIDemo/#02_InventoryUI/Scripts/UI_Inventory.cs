using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    private static UI_Inventory instance;
    public  static UI_Inventory Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [SerializeField] Transform  QuickSlotArea;
    [SerializeField] GameObject ItemSlotPrefab;

    // �̰� �ٲܰŸ� UI ũ�� �������� ��������� ��
    int _initialInventorySize = 15;

    List<UI_ItemSlot> _inventory = new List<UI_ItemSlot>();

    int _focusIndex = 0;

    void Start()
    {
        for(int i = 0; i < _initialInventorySize; ++i)
        {
            GameObject go = Instantiate(ItemSlotPrefab, QuickSlotArea);

            _inventory.Add(go.GetComponent<UI_ItemSlot>());
        }

    }

    void Update()
    {
        ItemFocusing();
    }

    void ItemFocusing()
    {
        // ������ ������ = +Y
        // �ڷ�   ������ = -Y

        Vector2 mouseWheelDelta = Input.mouseScrollDelta;

        if(mouseWheelDelta.y != 0)
        {
            _focusIndex -= (int)mouseWheelDelta.y;
        }
        if(_focusIndex < 0)
        {
            _focusIndex += _initialInventorySize;
        }
        if(_focusIndex >= 15)
        {
            _focusIndex -= _initialInventorySize;
        }

        _inventory[_focusIndex].Activate();

        for(int i = 0; i < _initialInventorySize; ++i)
        {
            if(i == _focusIndex)
            {
                _inventory[_focusIndex].Activate();
            }
            else
            {
                _inventory[i].Deactivate();
            }
        }
    }

    public bool AddItem(Item item)
    {
        foreach (UI_ItemSlot slot in _inventory)
        {
            // ������ ������� �ʴٸ�
            if(slot.IsEmpty() == false)
            {
                // ���Կ� �� �ִ� �������� �ֿ� �������̶� ����, 64�� �̸��̶��
                if(slot.GetHavingItemName() == item.name && slot.IsFull() == false)
                {
                    slot.AddItem(item);
                    return true;
                }
            }
            else
            {
                // ������ ����ִٸ�
                slot.AddItem(item);
                return true;
            }
        }

        // �̹� ��� �ִ� �͵� ����, �κ��丮�� ���� ��
        return false;
    }
    public Item GetItem()
    {
        if (_inventory[_focusIndex].IsEmpty() == false)
        {
            return _inventory[_focusIndex].GetItem();
        }

        return null;
    }
}