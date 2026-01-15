using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform playerCamera; 

    [Header("Movement Settings")]
    public float followDistance = 1.5f;
    public float followAngle = 45f; 
    public float smoothTime = 0.25f;
    public float bobbingAmplitude = 0.1f;
    public float bobbingFrequency = 1.5f;
    
    // 内部变量
    private Vector3 velocity = Vector3.zero;
    private float bobbingTimer = 0f;
    private Vector3 lastPlayerPos;
    
    public enum NPCState { Idle, Guide, Wait }
    

    [Header("State Machine")]
    public NPCState state = NPCState.Idle;
    private Transform guideTarget; // 当前引导目标

    [Header("Auto Guide Settings")]
    public float inactivityTimer = 0f;
    public float inactivityThreshold = 30f;
    public float playerMoveThreshold = 0.05f;
    
    [Header("Animation")]
    public Animator animator; // 🆕 记得在Inspector里把Animator组件拖进去

    [Header("Task System")]
    // ⚠️重要：请在Inspector中把所有要买的物品按顺序拖进这个列表
    public List<Transform> currentTaskItemPositions; 
    private int currentTaskIndex = 0; // 记录当前进行到第几个任务

    [Header("Effects")]
    public Light guideLight; 
    public AudioSource guideAudioSource;
    public AudioClip guideClip; // 引导语音
    public AudioClip successClip; // 成功音效
    public AudioClip wrongClip;   // 错误音效

    void Start()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
        
        if (playerCamera != null)
            lastPlayerPos = playerCamera.position;
            
        if (guideLight != null)
            guideLight.enabled = false;
    }

    void Update()
    {
        CheckInactivity(); // 1. 检测发呆
        HandleAutoGuide(); // 2. 触发自动引导
        HandleStateMachine(); // 3. 状态机逻辑
        UpdateAnimation(); // 🆕 每帧更新动画参数
    }

    // 检测玩家是否发呆
    void CheckInactivity()
    {
        if (playerCamera == null) return;

        float moveDist = Vector3.Distance(playerCamera.position, lastPlayerPos);
        // 如果移动距离很小，且处于Idle状态，才计时
        if (moveDist < playerMoveThreshold && state == NPCState.Idle)
        {
            inactivityTimer += Time.deltaTime;
        }
        else
        {
            inactivityTimer = 0f;
        }
        lastPlayerPos = playerCamera.position;
    }

    // 处理自动引导触发
    void HandleAutoGuide()
    {
        // 只有在Idle状态，且超时，且任务列表有效，且没有超出任务总数时触发
        if (state == NPCState.Idle && 
            inactivityTimer > inactivityThreshold && 
            currentTaskItemPositions != null && 
            currentTaskIndex < currentTaskItemPositions.Count)
        {
            // 修复点：使用 currentTaskIndex 而不是写死的 [0]
            SetGuideTarget(currentTaskItemPositions[currentTaskIndex]); 
        }
    }

    void HandleStateMachine()
    {
        switch (state)
        {
            case NPCState.Idle:
                TurnOffLight(); // 确保离开Wait状态后光是关的
                if (playerCamera != null)
                {
                    // 计算目标位置
                    Vector3 forward = playerCamera.forward;
                    Vector3 targetDir = (Quaternion.AngleAxis(followAngle, Vector3.up) * forward).normalized;
                    Vector3 targetPos = playerCamera.position + targetDir * followDistance;

                    // 呼吸浮动
                    bobbingTimer += Time.deltaTime * bobbingFrequency * Mathf.PI * 2f;
                    targetPos.y += Mathf.Sin(bobbingTimer) * bobbingAmplitude;

                    // 移动
                    transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

                    // 始终看向玩家（只转Y轴）
                    Vector3 lookPos = playerCamera.position;
                    lookPos.y = transform.position.y;
                    transform.LookAt(lookPos);
                }
                break;

            case NPCState.Guide:
                TurnOffLight();
                if (guideTarget != null)
                {
                    // 飞向目标
                    transform.position = Vector3.SmoothDamp(transform.position, guideTarget.position, ref velocity, smoothTime);
                    
                    // 看向目标
                    Vector3 lookPos = guideTarget.position;
                    lookPos.y = transform.position.y; 
                    transform.LookAt(lookPos);

                    // 到达判断
                    if (Vector3.Distance(transform.position, guideTarget.position) < 1.0f)
                    {
                        state = NPCState.Wait;
                    }
                }
                else
                {
                    // 防御性代码：如果没有目标，直接切回Idle
                    state = NPCState.Idle;
                }
                break;

            case NPCState.Wait:
                // 在Wait状态下，开启光照并指向物体
                if (guideLight != null && guideTarget != null)
                {
                    if (guideLight.enabled == false) 
                    {
                        Debug.Log("尝试开灯！目标：" + guideTarget.name);
                    }
                    // -------------------------------------------
                    guideLight.enabled = true;
                    // 修复点：不要修改light的position，只修改旋转让它指向商品
                    // 假设light是NPC手部的子物体
                    guideLight.transform.LookAt(guideTarget.position);
                }

                if (playerCamera != null)
                {
                    // 身体慢慢转向玩家，表示在等待
                    Vector3 dir = playerCamera.position - transform.position;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 2f);
                    }
                }
                break;
        }
    }

    // 辅助方法：关灯
    void TurnOffLight()
    {
        if (guideLight != null && guideLight.enabled)
            guideLight.enabled = false;
    }

    // --- 公开方法供外部调用 ---

    void UpdateAnimation()
    {
        if (animator == null) return;

        // 获取当前 NPC 的实际移动速度（velocity 是 SmoothDamp 计算出来的）
        // magnitude 将向量转换为标量（速度值）
        float currentSpeed = velocity.magnitude;

        // 将速度传给 Animator 的 "Speed" 参数
        animator.SetFloat("Speed", currentSpeed);
    }
    public void SetGuideTarget(Transform target)
    {
        if(target == null) return;
        
        guideTarget = target;
        state = NPCState.Guide;
        inactivityTimer = 0f; // 重置计时器

        if (guideAudioSource != null && guideClip != null)
            guideAudioSource.PlayOneShot(guideClip);
    }

    // 🆕 新增：当玩家成功拿到物品时调用此方法
    public void TaskItemCollected()
    {
        // 播放成功音效
        if (guideAudioSource != null && successClip != null)
            guideAudioSource.PlayOneShot(successClip);

        // 任务进度+1
        currentTaskIndex++;
        if (animator != null)
        {
            animator.SetTrigger("Success");
        }

        // 停止引导，回到跟随状态
        state = NPCState.Idle;
        inactivityTimer = 0f; 
        
        // 可选：如果还有下一个任务，可以说一句“下一个”
        if(currentTaskIndex >= currentTaskItemPositions.Count)
        {
            Debug.Log("所有任务已完成！");
        }
    }

    // 🆕 新增：玩家拿错东西时调用
    public void TaskItemWrong()
    {
         if (guideAudioSource != null && wrongClip != null)
            guideAudioSource.PlayOneShot(wrongClip);
         
         if (animator != null)
         {
            animator.SetTrigger("Fail");
         }
    }

    // --- 在 NPCBehaviour.cs 的末尾添加以下方法 ---

// 1. 强制开启引导（对应指令 "GUIDE"）
    public void ForceStartGuide()
    {
        // 只有当任务列表有效时才引导
        if (currentTaskItemPositions != null && currentTaskIndex < currentTaskItemPositions.Count)
        {
            // 获取当前应该去的物品位置
            Transform target = currentTaskItemPositions[currentTaskIndex];
            SetGuideTarget(target);
         Debug.Log("Web指令：强制开始引导");
        }
        else
        {
         Debug.LogWarning("无法引导：任务列表为空或已完成所有任务");
        }
    }

    // 2. 强制回到跟随状态（对应指令 "IDLE"）
    public void ForceIdle()
    {
        state = NPCState.Idle;
        inactivityTimer = 0f; // 重置发呆计时
    
        // 如果手部光照开着，关掉它
        if (guideLight != null) guideLight.enabled = false;
    
        Debug.Log("Web指令：强制回到跟随状态");
    }

    // 3. 播放鼓励（对应指令 "ENCOURAGE"）
    public void PlayEncouragement()
    {
        // 这里借用成功音效，你也可以单独定义一个 encourageClip
        if (guideAudioSource != null && successClip != null)
        {
            guideAudioSource.PlayOneShot(successClip);
        }
    
        if (animator != null)
        {
            animator.SetTrigger("Success");
        }
    
    Debug.Log("Web指令：播放鼓励反馈");
    }
    public void ResetTaskIndex()
    {
        currentTaskIndex = 0;
        state = NPCState.Idle;
        inactivityTimer = 0f;
        // 如果有手电筒，确保关闭
        if (guideLight != null) guideLight.enabled = false;
        Debug.Log("NPC 任务进度已重置");
    }
}