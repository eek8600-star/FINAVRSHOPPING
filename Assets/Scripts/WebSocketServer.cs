using NativeWebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class WebSocketServer : MonoBehaviour
{
    private WebSocket webSocket;
    public TaskManager taskManager;
    public string serverIP = "192.168.3.198";
    public int serverPort = 8080;
    public int difficulty = 2;
    public int spawnTruePlace;

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
                SceneManager.LoadScene("TEWCPL_Scene"); // �滻����ĳ�����
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

            ControllerStart();
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

    void ControllerStart()
    {
        taskManager.currentTaskItems = new List<string>();
        taskManager.displayedHints.Clear();

        // ��������Ʒ����ʾ
        foreach (Transform t in taskManager.transform) Destroy(t.gameObject);
        foreach (Transform t in transform) Destroy(t.gameObject);
        foreach (Transform t in taskManager.displayParent) Destroy(t.gameObject);

        int spawnCount = taskManager.itemSpawnPoints.Length;
        int tasksCount;
        tasksCount = difficulty;
        // ȷ��������Ʒ�����������ڷŵ�����
        tasksCount = Mathf.Clamp(tasksCount, 1, spawnCount);

        // ���ѡ��������Ʒ���ֵ�λ������
        List<int> taskIndices = new List<int>();
        while (taskIndices.Count < tasksCount)
        {
            int idx = Random.Range(0, spawnCount);  // Unity Random.Range ���ȡ [0, spawnCount-1]:contentReference[oaicite:3]{index=3}
            if (!taskIndices.Contains(idx))
                taskIndices.Add(idx);
        }
        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawn = taskManager.itemSpawnPoints[i];
            GameObject prefab = taskManager.allItemPrefabs[Random.Range(0, taskManager.allItemPrefabs.Count)];
            GameObject item = Instantiate(prefab, spawn.position, spawn.rotation, transform);
            // ��ȡ��Ʒ����
            ItemInteraction interaction = item.GetComponent<ItemInteraction>();
            string itemName = interaction != null ? interaction.itemName : prefab.name;

            // ��λ��Ϊ������Ʒ
            if (taskIndices.Contains(i))
            {
                taskManager.currentTaskItems.Add(itemName);

                // ������������
                int index = taskManager.displayedHints.Count;
                int row = index / 3;
                int col = index % 3;

                // ����λ�ã�ÿ����Ʒ��X�������ţ�Z��������
                Vector3 position = new Vector3(0.2f, -row * 0.7f + 0.2f, col * 0.7f - 0.4f);

                // ��չʾ�����ɶ�Ӧ����ʾ��Ʒ
                GameObject displayItem = Instantiate(prefab, taskManager.displayParent);
                displayItem.transform.localPosition = position;
                displayItem.transform.localRotation = Quaternion.Euler(0, 90, 0);

                // �Ŵ�1.3��
                displayItem.transform.localScale *= 1.3f;

                taskManager.DisableInteraction(displayItem);

                if (!taskManager.displayedHints.ContainsKey(itemName))
                    taskManager.displayedHints[itemName] = new List<GameObject>();
                taskManager.displayedHints[itemName].Add(displayItem);

                Debug.Log($"������Ʒ: {itemName}");
            }
            else
            {
                Debug.Log($"������Ʒ: {itemName}");
            }
        }
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
