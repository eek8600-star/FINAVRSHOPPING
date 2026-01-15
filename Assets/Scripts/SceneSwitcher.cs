using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string sceneNameToLoad;
    public float fadeDuration = 1.5f;
    private bool isLoading = false;


    private void OnTriggerEnter(Collider other)
    {
        if (!isLoading && other.CompareTag("Player"))
        {
            isLoading = true;
            StartCoroutine(FadeAndLoadScene());
        }
    }

    IEnumerator FadeAndLoadScene()
    {

        // 异步加载
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNameToLoad);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        // 可延迟激活场景
        yield return new WaitForSeconds(0.5f);
        asyncLoad.allowSceneActivation = true;
    }
}
