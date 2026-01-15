using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioOnDistance : MonoBehaviour
{
    public Transform player;              // 玩家 Transform（XR Origin 或摄像机）
    public AudioClip audioClip;           // 要播放的音频
    public float triggerDistance = 1f;    // 触发播放的移动距离
    public bool playOnlyOnce = true;      // 是否只播放一次

    private Vector3 lastPosition;
    private float distanceMoved = 0f;
    private bool hasPlayed = false;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("未设置 player Transform");
            enabled = false;
            return;
        }
        lastPosition = player.position;
    }

    void Update()
    {
        float movedThisFrame = Vector3.Distance(player.position, lastPosition);
        distanceMoved += movedThisFrame;
        lastPosition = player.position;

        if (!hasPlayed || !playOnlyOnce)
        {
            if (distanceMoved >= triggerDistance)
            {
                // 播放音频
                AudioCueManager.Instance.PlaySFX(audioClip);
                hasPlayed = true;
                Debug.Log("music get");
                // 停止检测和执行 Update
                if (playOnlyOnce)
                    enabled = false;
                else
                    distanceMoved = 0f;
            }
        }
    }
}
