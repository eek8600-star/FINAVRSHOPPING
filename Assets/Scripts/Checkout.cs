using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Checkout : MonoBehaviour
{
    public TaskManager taskManager;          // 引用任务管理器
    public  TextMeshProUGUI priceText;            // 显示总价格的TextMeshPro
    private int currentTotal;

    // 初始化结账界面
    public void StartCheckout(int totalPrice)
    {
        currentTotal = totalPrice;
        priceText.text = $"总价：{totalPrice} 元";  // 使用TextMeshPro显示中文文本:contentReference[oaicite:10]{index=10}
    }

    public void ResetCheckout()
    {
        currentTotal = 0;

        // 清空文本显示
        if (priceText != null)
            priceText.text = "";
    }
    

}
