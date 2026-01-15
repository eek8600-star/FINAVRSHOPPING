using System;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VRWebServer : MonoBehaviour
{
    public static VRWebServer Instance { get; private set; }
    public int port = 12345; // 保持端口一致
    
    private HttpListener listener;
    private Thread serverThread;
    private bool isRunning = false;

    public TaskManager taskManager; 
    public NPCBehaviour npcBehaviour;
    [Header("UI 设置")]
    public TextMeshProUGUI ipDisplayText; 
    public int difficulty = 2;
    private System.Collections.Concurrent.ConcurrentQueue<Action> mainThreadActions = new System.Collections.Concurrent.ConcurrentQueue<Action>();

    // 🔥 核心修改：将 HTML 直接写成字符串，避开安卓文件读取坑
    private string htmlContent = @"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>VR游戏控制面板</title>
<style>
  * { margin: 0; padding: 0; box-sizing: border-box; }
  body { font-family: sans-serif; background-color: #f5f5f5; min-height: 100vh; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #333; }
  .controller-container { background: #fff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); padding: 20px; text-align: center; width: 90%; max-width: 500px; }
  h1 { font-size: 1.5rem; margin-bottom: 20px; border-bottom: 2px solid #3498db; padding-bottom: 10px; }
  .control-group { margin-bottom: 20px; }
  .title { text-align: left; font-weight: bold; margin-bottom: 10px; border-left: 4px solid #3498db; padding-left: 10px; }
  .btn-row { display: flex; gap: 10px; justify-content: center; flex-wrap: wrap; }
  button { padding: 10px 15px; border: 1px solid #3498db; background: #fff; color: #3498db; border-radius: 4px; cursor: pointer; flex: 1; min-width: 80px; }
  button:hover, button:active, button.active { background: #3498db; color: #fff; }
  .status { margin-top: 20px; padding: 10px; background: #f8f9fa; border: 1px solid #ddd; border-radius: 4px; font-size: 0.9rem; }
</style>
</head>
<body>
<div class='controller-container'>
  <h1>VR 控制台</h1>
  
  <div class='control-group'>
    <div class='title'>难度</div>
    <div class='btn-row'>
      <button onclick=""send('increase')"">增加难度 (+)</button>
      <button onclick=""send('decrease')"">降低难度 (-)</button>
    </div>
  </div>

  <div class='control-group'>
    <div class='title'>场景切换</div>
    <div class='btn-row'>
      <button onclick=""send('1')"">场景 1</button>
      <button onclick=""send('2')"">场景 2</button>
      <button onclick=""send('3')"">场景 3</button>
    </div>
  </div>

  <div class='control-group'>
    <div class='title'>NPC 指令</div>
    <div class='btn-row'>
      <button onclick=""send('GUIDE')"">引导</button>
      <button onclick=""send('IDLE')"">待机</button>
      <button onclick=""send('ENCOURAGE')"">鼓励</button>
    </div>
  </div>

  <div class='control-group'>
    <div class='title'>背景音乐</div>
    <div class='btn-row'>
      <button id='btn-comfort' onclick=""send('comfort_music')"">舒适音乐</button>
      <button id='btn-market' onclick=""send('market_sound')"">超市背景音</button>
    </div>
  </div>

  <div class='status' id='status'>连接就绪</div>
</div>

<script>
  function send(cmd) {
    document.getElementById('status').innerText = '发送中... ' + cmd;
    fetch('/api/control?cmd=' + cmd)
      .then(res => res.json())
      .then(data => {
         document.getElementById('status').innerText = '执行成功: ' + cmd + ' (难度:' + data.difficulty + ')';
         if(cmd === 'comfort_music') highlight('btn-comfort');
         if(cmd === 'market_sound') highlight('btn-market');
      })
      .catch(err => {
         document.getElementById('status').innerText = '发送失败，请刷新';
      });
  }
  function highlight(id) {
    document.querySelectorAll('button').forEach(b => b.classList.remove('active'));
    var btn = document.getElementById(id);
    if(btn) btn.classList.add('active');
  }
</script>
</body>
</html>
";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start() { FindSceneReferences(); StartServer(); }
    void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; ForceStopServer(); }
    private void OnApplicationQuit() { ForceStopServer(); }

    private void ForceStopServer()
    {
        isRunning = false;
        if (listener != null) { try { listener.Stop(); listener.Close(); } catch { } listener = null; }
        if (serverThread != null) { try { serverThread.Abort(); } catch { } serverThread = null; }
        Debug.Log("Server 资源释放");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { FindSceneReferences(); }
    private void FindSceneReferences() { taskManager = FindObjectOfType<TaskManager>(); npcBehaviour = FindObjectOfType<NPCBehaviour>(); }
    void Update() { while (mainThreadActions.TryDequeue(out Action action)) action.Invoke(); }

    private void StartServer()
    {
        try
        {
            string localIP = GetLocalIPAddress();
            if (localIP.StartsWith("Error") || localIP.StartsWith("127")) {
                if (ipDisplayText != null) ipDisplayText.text = $"网络错误: {localIP}"; return;
            }

            listener = new HttpListener();
            listener.Prefixes.Add($"http://{localIP}:{port}/");
            listener.Start();
            isRunning = true;
            serverThread = new Thread(HandleIncomingConnections);
            serverThread.Start();
            
            string url = $"http://{localIP}:{port}";
            if (ipDisplayText != null) ipDisplayText.text = $":\n<color=yellow>{url}</color>";
        }
        catch (Exception e)
        {
            if (ipDisplayText != null) ipDisplayText.text = $":\n{e.Message}";
        }
    }

    private void HandleIncomingConnections()
    {
        while (isRunning && listener != null && listener.IsListening)
        {
            try { var context = listener.GetContext(); ProcessRequest(context); } catch { }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest req = context.Request;
        HttpListenerResponse resp = context.Response;
        string url = req.Url.AbsolutePath;
        string responseString = "";
        string contentType = "text/html";

        // 🔥 核心修改：直接返回字符串，不读文件
        if (url == "/" || url == "/index.html" || url == "/favicon.ico")
        {
            responseString = htmlContent;
        }
        else if (url.StartsWith("/api/control"))
        {
            contentType = "application/json";
            string cmd = req.QueryString["cmd"];
            mainThreadActions.Enqueue(() => HandleCommand(cmd));
            responseString = $"{{\"status\":\"ok\", \"received\":\"{cmd}\", \"difficulty\":{difficulty}}}";
        }

        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        resp.ContentType = contentType;
        resp.ContentLength64 = buffer.Length;
        resp.OutputStream.Write(buffer, 0, buffer.Length);
        resp.OutputStream.Close();
    }

    private void HandleCommand(string cmd)
    {
        Debug.Log($"CMD: {cmd}");
        bool needRefresh = false;
        switch (cmd)
        {
            case "increase": difficulty++; needRefresh = true; break;
            case "decrease": difficulty = Math.Max(1, difficulty - 1); needRefresh = true; break;
            case "1": SceneManager.LoadScene("TEWCPL_Scene"); break;
            case "2": SceneManager.LoadScene("Test"); break;
            case "3": SceneManager.LoadScene("Market"); break;
            case "comfort_music":
                if (AudioCueManager.Instance?.comfortMusicClip != null) AudioCueManager.Instance.PlayMusicLoop(AudioCueManager.Instance.comfortMusicClip); break;
            case "market_sound":
                if (AudioCueManager.Instance?.marketSoundClip != null) AudioCueManager.Instance.PlayMusicLoop(AudioCueManager.Instance.marketSoundClip); break;
            case "GUIDE": npcBehaviour?.ForceStartGuide(); break;
            case "IDLE": npcBehaviour?.ForceIdle(); break;
            case "ENCOURAGE": npcBehaviour?.PlayEncouragement(); break;
        }
        if (needRefresh && taskManager != null) taskManager.ControllerStart(difficulty);
    }

    private string GetLocalIPAddress()
    {
        try {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    string s = ip.ToString();
                    if (!s.StartsWith("127") && !s.StartsWith("0")) return s;
                }
            }
        } catch { }
        return "127.0.0.1";
    }
}