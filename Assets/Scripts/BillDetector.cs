using System.Collections.Generic;
using UnityEngine;

public class BillDetector : MonoBehaviour
{
    private List<PayOption> detectedBills = new List<PayOption>();
    public ShoppingCart cart;                  // 购物车组件
    public TaskManager taskManager;    // 引用任务管理器


    private void OnTriggerEnter(Collider other)
    {
        PayOption bill = other.GetComponent<PayOption>();
        if (bill != null && !detectedBills.Contains(bill))
        {
            detectedBills.Add(bill);
            taskManager.OnCheckoutCompleted(true);
        }
    }

}
