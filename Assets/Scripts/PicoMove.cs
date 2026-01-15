using UnityEngine;
using Unity.XR.PXR; 

public class PicoMove : MonoBehaviour
{
    [Header("设置")]
    public float moveSpeed = 2.0f;
    public CharacterController characterController; 
    public Transform headCamera; 

    [Header("调试")]
    public bool isDataValid = false; 
    public bool isPointing = false;

    // --- 新增：调试数值，方便你在Inspector里看每一根手指的数据 ---
    [Header("实时距离监测 (只读)")]
    public float indexDistDebug;
    public float middleDistDebug;

    private HandJointLocations jointLocations = new HandJointLocations(); 
    private HandJointLocation[] jointDataArray = new HandJointLocation[26]; 

    void Start()
    {
        jointLocations.jointLocations = jointDataArray;
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (headCamera == null) headCamera = Camera.main.transform;
    }

    void Update()
    {
        bool gotData = PXR_HandTracking.GetJointLocations(HandType.HandLeft, ref jointLocations);
        isDataValid = gotData;

        if (gotData)
        {
            if (CheckPointingGesture())
            {
                isPointing = true;
                MoveForward();
            }
            else
            {
                isPointing = false;
            }
        }
    }

    bool CheckPointingGesture()
    {
        // 安全检查
        if (jointLocations.jointLocations == null || jointLocations.jointLocations.Length < 26) return false;

        // --- 1. 获取关键节点坐标 ---
        // 根据你提供的枚举：
        // Wrist=1, ThumbTip=5, IndexTip=10, MiddleTip=15, RingTip=20, LittleTip=25
        Vector3 wristPos = ToVector3(jointLocations.jointLocations[1].pose.Position);
        
        Vector3 thumbTip = ToVector3(jointLocations.jointLocations[5].pose.Position);   // 新增拇指
        Vector3 indexTip = ToVector3(jointLocations.jointLocations[10].pose.Position);
        Vector3 middleTip = ToVector3(jointLocations.jointLocations[15].pose.Position);
        Vector3 ringTip = ToVector3(jointLocations.jointLocations[20].pose.Position);
        Vector3 littleTip = ToVector3(jointLocations.jointLocations[25].pose.Position);

        // --- 2. 计算距离 ---
        float thumbDist = Vector3.Distance(thumbTip, wristPos);
        float indexDist = Vector3.Distance(indexTip, wristPos);
        float middleDist = Vector3.Distance(middleTip, wristPos);
        float ringDist = Vector3.Distance(ringTip, wristPos);
        float littleDist = Vector3.Distance(littleTip, wristPos);

        // 输出到 Inspector 方便调试
        indexDistDebug = indexDist;
        middleDistDebug = middleDist;

        // --- 3. 严格判定逻辑 ---

        // A. 绝对阈值判定 (比之前更严格)
        // 食指必须伸得够直 (之前是 0.12, 改为 0.13)
        bool isIndexStraight = indexDist > 0.13f; 
        
        // 其他手指必须握得够紧 (之前是 0.10, 改为 0.08)
        bool isMiddleCurled = middleDist < 0.08f;
        bool isRingCurled = ringDist < 0.08f;
        bool isLittleCurled = littleDist < 0.08f;

        // B. 拇指干扰判定 (新增)
        // 拇指不能伸太直，防止“手枪”手势或“张手”误触
        // 拇指通常较短，所以阈值设为 0.12 左右，或者要求它比食指短
        bool isThumbRelaxed = thumbDist < 0.12f;

        // C. 相对差值判定 (这是防误触的核心)
        // 食指伸出的距离，必须比中指多出至少 3厘米 (0.03f)
        // 这能确保你是真的在“指”，而不是半握拳
        bool isIndexProminent = indexDist > (middleDist + 0.03f);

        // --- 4. 最终组合 ---
        return isIndexStraight 
            && isMiddleCurled 
            && isRingCurled 
            && isLittleCurled 
            && isThumbRelaxed 
            && isIndexProminent; // 必须满足所有条件
    }

    void MoveForward()
    {
        if (headCamera == null || characterController == null) return;
        Vector3 dir = headCamera.forward;
        dir.y = 0;
        dir.Normalize();
        characterController.Move(dir * moveSpeed * Time.deltaTime);
    }

    Vector3 ToVector3(Vector3f v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
}