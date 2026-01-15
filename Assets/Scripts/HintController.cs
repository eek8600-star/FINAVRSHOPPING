using System.Collections.Generic;
using UnityEngine;

public class HintController : MonoBehaviour
{
    public GameObject hintIconPrefab;             // 图标预制体
    public Transform iconDisplayParent;           // 图标的展示区域（如 HintPanel）
    private Dictionary<string, GameObject> hints = new Dictionary<string, GameObject>();

    // 显示图标（添加一个新的）
    public void ShowIcon(string itemName)
    {
        if (!hints.ContainsKey(itemName))
        {
            GameObject icon = Instantiate(hintIconPrefab, iconDisplayParent);
            icon.name = itemName + "_Hint";

            // 设置图标上显示的文字（可选）
            HintIcon hi = icon.GetComponent<HintIcon>();
            if (hi != null)
                hi.SetTarget(itemName);

            hints[itemName] = icon;
        }

        hints[itemName].SetActive(true);
    }

    // 删除图标（收集成功时调用）
    public void RemoveIcon(string itemName)
    {
        if (hints.ContainsKey(itemName))
        {
            Destroy(hints[itemName]);
            hints.Remove(itemName);
        }
    }

    // 重置图标（开始新一轮时）
    public void ClearAllIcons()
    {
        foreach (var icon in hints.Values)
        {
            Destroy(icon);
        }
        hints.Clear();
    }
}
