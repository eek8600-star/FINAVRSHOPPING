using NativeWebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WebSocketClient : MonoBehaviour
{
    private WebSocket webSocket;
    public string serverIP = "192.168.3.198";
    public int serverPort = 8080;
    public int difficulty = 2;
    public int spawnTruePlace;

    // 🆕 新增：NPC脚本的引用
    public NPCBehaviour npcBehaviour;

    async void Start()
    {
        // 自动查找场景里的NPC（防止你忘记拖拽）
        if (npcBehaviour == null)
        {
            npcBehaviour = FindObjectOfType<NPCBehaviour>();
        }

        webSocket = new WebSocket($"ws://{serverIP}:{serverPort}");

        webSocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        webSocket.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            Debug.Log("Message from server: " + msg);

            // 保持原有的逻辑
            if (msg == "increase")
            {
                difficulty++;
            }
            else if (msg == "decrease")
            {
                difficulty = Math.Max(1, difficulty - 1);
            }
            else if (msg == "1")
            {
                SceneManager.LoadScene("TEWCPL_Scene");
            }
            else if (msg == "2")
            {
                SceneManager.LoadScene("Test");
            }
            else if (msg == "3")
            {
                SceneManager.LoadScene("Market");
            }
            else if (msg == "comfort_music")
            {
                if (AudioCueManager.Instance != null && AudioCueManager.Instance.comfortMusicClip != null)
                {
                    AudioCueManager.Instance.PlayMusicLoop(AudioCueManager.Instance.comfortMusicClip);
                }
            }
            else if (msg == "market_sound")
            {
                if (AudioCueManager.Instance != null && AudioCueManager.Instance.marketSoundClip != null)
                {
                    AudioCueManager.Instance.PlayMusicLoop(AudioCueManager.Instance.marketSoundClip);
                }
            }
            
            // 🆕 新增：NPC控制指令
            // ------------------------------------------------
            else if (msg == "GUIDE")
            {
                // 如果场景切换了，可能需要重新找一下NPC
                if (npcBehaviour == null) npcBehaviour = FindObjectOfType<NPCBehaviour>();
                
                if (npcBehaviour != null)
                {
                    npcBehaviour.ForceStartGuide();
                }
            }
            else if (msg == "IDLE")
            {
                if (npcBehaviour == null) npcBehaviour = FindObjectOfType<NPCBehaviour>();

                if (npcBehaviour != null)
                {
                    npcBehaviour.ForceIdle();
                }
            }
            else if (msg == "ENCOURAGE")
            {
                if (npcBehaviour == null) npcBehaviour = FindObjectOfType<NPCBehaviour>();

                if (npcBehaviour != null)
                {
                    npcBehaviour.PlayEncouragement();
                }
            }
            // ------------------------------------------------

            Debug.Log("Current difficulty: " + difficulty);
        };

        webSocket.OnError += (errMsg) =>
        {
            Debug.Log("WebSocket Error: " + errMsg);
        };
        webSocket.OnClose += (code) =>
        {
            Debug.Log("WebSocket Closed");
        };
        await webSocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        webSocket?.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        await webSocket.Close();
    }
}