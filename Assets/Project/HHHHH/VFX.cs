using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VFX : MonoBehaviour
{
    [Header("VFX Settings")]
    [Tooltip("拖入需要实例化的特效Prefab")]
    public GameObject effectPrefab;

    [Tooltip("设置特效生成的目标位置")]
    public Transform targetPosition;

    [Header("Effect Control")]
    [Tooltip("是否销毁生成的特效对象（否则禁用）")]
    [SerializeField] bool destroyEffect = true;

    [Tooltip("特效存活时间（秒）")]
    [SerializeField] float effectLifetime = 2f;

    [Tooltip("每个物体的重置冷却时间（秒），设为 -1 表示永不重置")]
    [SerializeField] float resetCooldown = -1f;

    // 存储已触发过的物体 ID
    private HashSet<int> triggeredObjectIDs = new HashSet<int>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Product"))
        {
            int id = other.GetInstanceID();

            if (triggeredObjectIDs.Contains(id))
                return; // 已触发过，跳过

            // 标记为已触发
            triggeredObjectIDs.Add(id);

            // 可选：移动触发器自身
            if (targetPosition != null)
                transform.position = targetPosition.position;

            // 实例化特效
            if (effectPrefab != null)
            {
                GameObject spawnedEffect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
                var deactivator = spawnedEffect.AddComponent<AutoDeactivate>();
                deactivator.Setup(destroyEffect, effectLifetime);
            }

            // 启用重置冷却（如果设置了冷却时间）
            if (resetCooldown > 0f)
                StartCoroutine(ResetObjectCooldown(id, resetCooldown));
        }
    }

    IEnumerator ResetObjectCooldown(int id, float delay)
    {
        yield return new WaitForSeconds(delay);
        triggeredObjectIDs.Remove(id); // 重置，允许再次触发
    }

    // 自动销毁/禁用特效对象
    class AutoDeactivate : MonoBehaviour
    {
        private bool shouldDestroy;
        private float lifetime;

        public void Setup(bool destroy, float time)
        {
            shouldDestroy = destroy;
            lifetime = time;
            StartCoroutine(DeactivateRoutine());
        }

        IEnumerator DeactivateRoutine()
        {
            yield return new WaitForSeconds(lifetime);

            if (shouldDestroy)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}