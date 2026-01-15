using System.Collections.Generic;
using UnityEngine;

public class ShoppingCart : MonoBehaviour
{
    public TaskManager taskManager;    // 引用任务管理器
    private List<ItemInteraction> itemsInCart = new List<ItemInteraction>();
    private int totalPrice = 0;

    // 重置购物车状态
    public void ResetCart()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        itemsInCart.Clear();
        totalPrice = 0;
    }

    // 添加商品
    public void AddItem(ItemInteraction item)
    {
        if (taskManager.currentTaskItems.Contains(item.itemName))
        {
            itemsInCart.Add(item);
            totalPrice += item.price;
            // 将商品附加到购物车下（可作为子对象）
            item.transform.SetParent(this.transform);
            // 通知任务管理器此物品已收集
            taskManager.OnItemCollected(item.itemName, item.price);
        }
        else
        {
            Debug.Log($"[{item.itemName}] 不在当前任务清单中，未添加到购物车。");
        }
    }
    // 获取总价
    public int GetTotalPrice()
    {
        return totalPrice;
    }

    // 在碰撞器内检测商品
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"触发器检测到碰撞体进入：{other.name}，标签为：{other.tag}");

        if (other.CompareTag("Product"))
        {
            ItemInteraction item = other.GetComponent<ItemInteraction>();

            if (item != null)
            {
                Debug.Log($"检测到任务物品：{item.itemName}");
                AddItem(item);
            }
            else
            {
                Debug.LogWarning($"物品 {other.name} 没有挂载 ItemInteraction 脚本，忽略");
            }
        }
    }
}
