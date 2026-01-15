using UnityEngine;
using TMPro;

public class HintIcon : MonoBehaviour
{
    public TextMeshProUGUI textLabel;

    public void SetTarget(string itemName)
    {
        if (textLabel != null)
        {
            textLabel.text = itemName;
        }
    }
}
