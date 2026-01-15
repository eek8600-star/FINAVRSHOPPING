using UnityEngine;
using Unity.XR.CoreUtils;

public class VRHeightAdjuster : MonoBehaviour
{
    public XROrigin xrOrigin;
    public CharacterController characterController;
    
    [Header("Settings")]
    public float minHeight = 0.5f; 
    public float maxHeight = 2.5f;
    public float heightOffset = 0.1f; // 稍微高一点，避免头顶穿模

    void Update()
    {
        UpdateCharacterShape();
    }

    void UpdateCharacterShape()
    {
        if (xrOrigin == null || characterController == null) return;

        // --- 1. 高度处理 (Y轴) ---
        float headHeight = xrOrigin.CameraInOriginSpaceHeight;
        float targetHeight = Mathf.Clamp(headHeight + heightOffset, minHeight, maxHeight);
        characterController.height = targetHeight;

        // --- 2. 中心点处理 (X, Y, Z轴) ---
        // 关键点：获取摄像机在 Rig 里的局部坐标
        Vector3 cameraLocalPos = xrOrigin.Camera.transform.localPosition;

        // 计算新的中心点：
        // X 和 Z：跟随头显的水平位置 (解决探身无法靠近物体的问题)
        // Y：始终保持在高度的一半
        Vector3 newCenter = new Vector3(
            cameraLocalPos.x, 
            characterController.height / 2 + characterController.skinWidth, 
            cameraLocalPos.z
        );

        characterController.center = newCenter;
    }
}