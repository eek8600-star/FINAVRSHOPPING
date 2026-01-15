using UnityEngine;
using Unity.XR.CoreUtils;

public class CharacterFollowHead : MonoBehaviour
{
    public XROrigin xrOrigin; // 拖入你的 XR Origin 对象
    public CharacterController characterController;
    
    [Header("Settings")]
    public float minHeight = 0.5f; // 胶囊体最小高度
    public float maxHeight = 2.5f; // 胶囊体最大高度
    
    // 这是一个微调值，避免胶囊体顶端正好切在眼睛上导致画面穿模
    // 设为 0.1f 表示胶囊体比头显稍微高一点点
    public float heightOffset = 0.1f; 

    void Update()
    {
        FollowHead();
    }

    void FollowHead()
    {
        if (xrOrigin == null || characterController == null) return;

        // --- 1. 获取头显在 XR Origin 里的局部位置 ---
        // 这里的 Camera.transform.localPosition 就是你现实走动产生的偏移量
        Vector3 headLocalPos = xrOrigin.Camera.transform.localPosition;

        // --- 2. 同步高度 (Height) ---
        // 算出头显高度，并加上一点偏移量，限制在合理范围内
        float targetHeight = Mathf.Clamp(xrOrigin.CameraInOriginSpaceHeight + heightOffset, minHeight, maxHeight);
        characterController.height = targetHeight;

        // --- 3. 同步中心点 (Center) ---
        // 核心步骤：将胶囊体的中心设定为头显的 X 和 Z 坐标
        // Y 轴中心通常是高度的一半
        Vector3 newCenter = new Vector3(
            headLocalPos.x, 
            characterController.height / 2 + characterController.skinWidth, 
            headLocalPos.z
        );

        characterController.center = newCenter;
    }
}