using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// 继承自 ActionBasedController，这样我们可以直接替换掉原有的组件
public class PicoHandFilter : ActionBasedController
{
    [Header("Hand Tracking Stability")]
    [Tooltip("当物理手势丢失(松开)后，维持抓取状态的缓冲时间(秒)")]
    public float debounceTime = 0.3f;

    // 记录上一次检测到“捏合”的时间
    private float _lastPinchTime;

    // 重写父类的输入更新方法
    protected override void UpdateInput(XRControllerState controllerState)
    {
        // 1. 先执行父类的逻辑
        // 这很重要！因为它会帮我们处理位置(Position)、旋转(Rotation)和其他按键
        base.UpdateInput(controllerState);

        // 2. 获取父类刚刚读到的“Select(抓取)”状态
        bool isPhysicalPinching = controllerState.selectInteractionState.active;

        // 3. 开始我们的防抖逻辑
        if (isPhysicalPinching)
        {
            // 如果物理上正在捏合，更新最后活跃时间
            _lastPinchTime = Time.time;
        }
        else
        {
            // 如果物理上松开了，我们检查是不是在“缓冲期”内
            if (Time.time - _lastPinchTime < debounceTime)
            {
                // 【核心黑魔法】：虽然物理松开了，但我们骗系统说“还在捏着”
                
                // 设置值为 1.0 (完全按下)
                controllerState.selectInteractionState.value = 1.0f;
                // 设置状态为 Active
                controllerState.selectInteractionState.active = true;
            }
            else
            {
                // 超过了缓冲期，那就真的松开吧
                // (父类已经把 active 设为 false 了，这里不需要额外操作，但为了保险可以显式设置)
                controllerState.selectInteractionState.value = 0.0f;
                controllerState.selectInteractionState.active = false;
            }
        }
    }
}