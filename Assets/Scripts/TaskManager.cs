using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;

public class TaskManager : MonoBehaviour
{
    public NPCBehaviour npcBehaviour; // 拖拽或自动查找
    public List<GameObject> allItemPrefabs;
    public Transform[] itemSpawnPoints;
    public ShoppingCart cart;
    public Checkout checkout;
    public List<string> currentTaskItems;
    public Transform displayParent;
    public AudioSource audioSource;
    public GameObject targetObject;  // 你想激活的物体
    public GameObject coinPrefab;
    public GameObject arrow;
    public AudioClip welcomeClip;
    public AudioClip collectStartClip;
    public AudioClip allCollectedClip;
    public AudioClip checkoutSuccessClip;
    public AudioClip checkoutFailClip;
    public Transform CoinspawnPoint;               // 生成位置
    public SwitchMode modeSwitcher;
    public List<string> itemNames;
    public List<AudioClip> itemClips;
    private Dictionary<string, AudioClip> itemAudioDict;
    private List<GameObject> spawnedCoins = new List<GameObject>();

    // 新增：记录本轮任务物品实际生成的位置
    public List<Transform> currentTaskItemPositions = new List<Transform>();

    public Dictionary<string, List<GameObject>> displayedHints = new Dictionary<string, List<GameObject>>();
    private int round = 0;
    private bool isCheckoutPhase = false;

    private float roundStartTime;
    private List<float> roundCompletionTimes = new List<float>();
    [System.Serializable]
    public class RoundData
    {
        public List<float> roundCompletionTimes;
        public string token;
    }
    void Start()
    {
        itemAudioDict = new Dictionary<string, AudioClip>();
        for (int i = 0; i < Mathf.Min(itemNames.Count, itemClips.Count); i++)
            itemAudioDict[itemNames[i]] = itemClips[i];

        StartNewRound();
    }

    void Update()
    {
        if (isCheckoutPhase)
        {
            // 激活物体
            targetObject.SetActive(true);
            arrow.SetActive(true);
        }
        else
        {
            // 如果需要在非结账阶段隐藏物体
            targetObject.SetActive(false);
            arrow.SetActive(false);
        }
    }
    void StartNewRound()
    {
        round++;
        isCheckoutPhase = false;

        currentTaskItems = new List<string>();
        currentTaskItemPositions.Clear();
        displayedHints.Clear();

        // 清理旧商品和提示
        foreach (Transform t in transform) Destroy(t.gameObject);
        foreach (Transform t in displayParent) Destroy(t.gameObject);
        foreach (GameObject coin in spawnedCoins)
        {
            if (coin != null)
                Destroy(coin);
        }
        spawnedCoins.Clear();

        // 记录本轮开始时间
        roundStartTime = Time.time;

        // 根据轮次和完成时间确定任务物品数量比例
        int spawnCount = itemSpawnPoints.Length;
        int tasksCount;
        if (round == 1)
        {
            tasksCount = Mathf.RoundToInt(spawnCount * 1f / 10f);
        }
        else if (round == 2)
        {
            tasksCount = Mathf.RoundToInt(spawnCount * 4f / 15f);
        }
        else
        {
            // 动态调整：比较最近两轮时间
            float tPrev = roundCompletionTimes[roundCompletionTimes.Count - 2];
            float tLast = roundCompletionTimes[roundCompletionTimes.Count - 1];
            // 如果用时减少（玩家表现提高），提高任务比例，否则降低
            float ratio = (tLast < tPrev) ? 4f / 13f : 1f / 10f;  // 示例比例，可根据需求调整
            tasksCount = Mathf.RoundToInt(spawnCount * ratio);
        }
        // 确保任务物品数量不超过摆放点总数
        tasksCount = Mathf.Clamp(tasksCount, 1, spawnCount);

        // 随机选择任务物品出现的位置索引
        List<int> taskIndices = new List<int>();
        while (taskIndices.Count < tasksCount)
        {
            int idx = Random.Range(0, spawnCount);  // Unity Random.Range 随机取 [0, spawnCount-1]:contentReference[oaicite:3]{index=3}
            if (!taskIndices.Contains(idx))
                taskIndices.Add(idx);
        }

        // 在每个摆放点生成商品
        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawn = itemSpawnPoints[i];
            GameObject prefab = allItemPrefabs[Random.Range(0, allItemPrefabs.Count)];
            GameObject item = Instantiate(prefab, spawn.position, spawn.rotation, transform);
            // 获取物品名称
            ItemInteraction interaction = item.GetComponent<ItemInteraction>();
            string itemName = interaction != null ? interaction.itemName : prefab.name;

            // 该位置为任务物品
            if (taskIndices.Contains(i))
            {
                currentTaskItems.Add(itemName);
                currentTaskItemPositions.Add(spawn); // 记录任务物品生成点

                // 计算行列索引
                int index = displayedHints.Count;
                int row = index / 3;
                int col = index % 3;

                // 计算位置：每个物品在X方向按列排，Z方向按行排
                Vector3 position = new Vector3(0.2f, -row * 0.7f+0.2f, col * 0.7f-0.4f);

                // 在展示区生成对应的提示物品
                GameObject displayItem = Instantiate(prefab, displayParent);
                displayItem.transform.localPosition = position;
                displayItem.transform.localRotation =Quaternion.Euler(0, 90, 0);

                // 放大1.3倍
                displayItem.transform.localScale *= 1.3f;

                DisableInteraction(displayItem);

                if (!displayedHints.ContainsKey(itemName))
                    displayedHints[itemName] = new List<GameObject>();
                displayedHints[itemName].Add(displayItem);

                Debug.Log($"Round {round} 任务物品: {itemName}");
            }
            else
            {
                // 非任务商品（干扰物），不加入任务列表，仅实例化即可
                Debug.Log($"Round {round} 干扰物品: {itemName}");
            }
        }
        if (npcBehaviour != null)
        {
            // 1. 传递位置列表
            npcBehaviour.currentTaskItemPositions = new List<Transform>(currentTaskItemPositions);
            
            // 2. ⚠️非常重要：开启新的一轮时，务必重置 NPC 的任务索引
            // 你需要在 NPCBehaviour 中把 currentTaskIndex 改为 public 或者写一个 Reset 方法
            // 这里假设你用反射或者把变量改为了public，或者像下面这样写一个重置逻辑：
            
            // 建议在 NPCBehaviour 增加一个 public void ResetRound() 方法来处理索引归零
            // 临时解决办法（如果 currentTaskIndex 是 private）：
            // 你现在的代码里 SetGuideTarget 会重置 inactivityTimer，但没重置 index。
            // 建议你修改 NPCBehaviour 脚本，增加下面这行：
             npcBehaviour.ResetTaskIndex(); // 需要你在 NPC 脚本里加这个方法
        }

        // 播放欢迎音并开始倒计时提示
        audioSource.PlayOneShot(welcomeClip);
        Invoke(nameof(PlayCollectStartHint), 5f);

        cart.ResetCart();
        checkout.ResetCheckout();
    }

    void PlayCollectStartHint()
    {
        audioSource.PlayOneShot(collectStartClip);
    }

    void PlayCollectEnterCheck()
    {
        audioSource.PlayOneShot(allCollectedClip);
    }

    public void DisableInteraction(GameObject item)
    {
        var grab = item.GetComponent<XRGrabInteractable>();
        if (grab) Destroy(grab);
        foreach (var c in item.GetComponentsInChildren<Collider>())
            c.enabled = false;
        var rb = item.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void OnItemCollected(string itemName, int price)
    {
        if (isCheckoutPhase) return;

        if (currentTaskItems.Contains(itemName))
        {
            currentTaskItems.Remove(itemName);
            if (displayedHints.ContainsKey(itemName) && displayedHints[itemName].Count > 0)
            {
                GameObject display = displayedHints[itemName][0];
                displayedHints[itemName].RemoveAt(0);
                Destroy(display);
            }
        }
        else
        {
            Debug.Log($"收集到的商品 \"{itemName}\" 不在当前任务列表中，忽略此项。");
        }
        if (itemAudioDict.ContainsKey(itemName))
            audioSource.PlayOneShot(itemAudioDict[itemName]);
        if (npcBehaviour != null)
        {
            npcBehaviour.TaskItemCollected();
        }
        // 所有任务物品收集完成
        if (currentTaskItems.Count == 0)
        {
            // 记录本轮完成时间
            float timeUsed = Time.time - roundStartTime;
            roundCompletionTimes.Add(timeUsed);
            Debug.Log($"Round {round} 完成时间: {timeUsed:F2}s");
            roundCompletionTimes.Add(timeUsed);
            if (modeSwitcher.CurrentMode == SwitchMode.GameMode.ModeA)
            {
                // 启动协程发送数据
                Invoke(nameof(PlayCollectEnterCheck), 1f);
                Invoke(nameof(EnterCheckoutPhase), 2f);
            }
            else if(modeSwitcher.CurrentMode == SwitchMode.GameMode.ModeB)
            {
                Invoke(nameof(StartNewRound), 4f);
            }
        }
    }

    void EnterCheckoutPhase()
    {
        isCheckoutPhase = true;
        int total = cart.GetTotalPrice();
        checkout.StartCheckout(total);
        if (coinPrefab != null && CoinspawnPoint != null)
        {
            GameObject newCoin = Instantiate(coinPrefab, CoinspawnPoint.position, CoinspawnPoint.rotation);
            spawnedCoins.Add(newCoin);
        }
    }

    public void OnCheckoutCompleted(bool success)
    {
        if (success)
        {
            audioSource.PlayOneShot(checkoutSuccessClip);
            // 延迟开始新一轮
            Invoke(nameof(StartNewRound), 10f);
        }
        else
        {
            Debug.Log("结账失败，重新结账或重置");
            audioSource.PlayOneShot(checkoutFailClip);
        }
    }

    public void ControllerStart(int currentDifficulty)
    {
        currentTaskItems = new List<string>();
        displayedHints.Clear();

        int difficulty = currentDifficulty;
        foreach (Transform t in transform) Destroy(t.gameObject);
        foreach (Transform t in transform) Destroy(t.gameObject);
        foreach (Transform t in displayParent) Destroy(t.gameObject);

        int spawnCount = itemSpawnPoints.Length;
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
            Transform spawn = itemSpawnPoints[i];
            GameObject prefab = allItemPrefabs[Random.Range(0, allItemPrefabs.Count)];
            GameObject item = Instantiate(prefab, spawn.position, spawn.rotation, transform);
            ItemInteraction interaction = item.GetComponent<ItemInteraction>();
            string itemName = interaction != null ? interaction.itemName : prefab.name;

            // ��λ��Ϊ������Ʒ
            if (taskIndices.Contains(i))
            {
                currentTaskItems.Add(itemName);

                // ������������
                int index = displayedHints.Count;
                int row = index / 3;
                int col = index % 3;

                // ����λ�ã�ÿ����Ʒ��X�������ţ�Z��������
                Vector3 position = new Vector3(0.2f, -row * 0.7f + 0.2f, col * 0.7f - 0.4f);

                // ��չʾ�����ɶ�Ӧ����ʾ��Ʒ
                GameObject displayItem = Instantiate(prefab, displayParent);
                displayItem.transform.localPosition = position;
                displayItem.transform.localRotation = Quaternion.Euler(0, 90, 0);

                // �Ŵ�1.3��
                displayItem.transform.localScale *= 1.3f;

                DisableInteraction(displayItem);

                if (!displayedHints.ContainsKey(itemName))
                    displayedHints[itemName] = new List<GameObject>();
                displayedHints[itemName].Add(displayItem);

                Debug.Log($"������Ʒ: {itemName}");
            }
            else
            {
                Debug.Log($"������Ʒ: {itemName}");
            }
        }
    }

}
