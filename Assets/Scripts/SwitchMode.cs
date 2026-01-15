using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SwitchMode : MonoBehaviour
{
    // 定义两种模式
    public enum GameMode { ModeA, ModeB }
    private GameMode currentMode = GameMode.ModeB;
    public GameMode CurrentMode => currentMode;

    public void OnButtonPressed(SelectEnterEventArgs args)
    {
        ToggleMode();
        Debug.Log("Current Mode: " + currentMode);
    }
    private void ToggleMode()
    {
        // 切换模式
        currentMode = (currentMode == GameMode.ModeA) ? GameMode.ModeB : GameMode.ModeA;
        Debug.Log("Switched to: " + currentMode);
    }
}
