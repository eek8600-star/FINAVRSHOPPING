using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCueManager : MonoBehaviour
{
    public static AudioCueManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;  // ��������
    public AudioSource sfxSource;    // ��ʾ�� / ����Ч

    [Header("Music Clips")]
    public AudioClip comfortMusicClip; // 舒适音乐
    public AudioClip marketSoundClip;  // 现实超市背景音

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ��ѡ���糡����������
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ���ű������֣�ѭ�����ţ�
    /// </summary>
    public void PlayMusicLoop(AudioClip musicClip)
    {
        if (musicSource.clip == musicClip && musicSource.isPlaying)
            return; // ���ڲ�����ͬ������

        musicSource.loop = true;
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    /// <summary>
    /// ������ʾ������ѭ�������Զ����ǰһ����ʾ����
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource.isPlaying && sfxSource.clip == clip)
            return; // �����ظ�����

        sfxSource.Stop();
        sfxSource.clip = clip;
        sfxSource.loop = false;
        sfxSource.Play();
    }

    /// <summary>
    /// �ӳٲ�����ʾ��
    /// </summary>
    public void PlaySFXDelayed(AudioClip clip, float delay)
    {
        StartCoroutine(PlaySFXDelayedCoroutine(clip, delay));
    }

    private IEnumerator PlaySFXDelayedCoroutine(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(clip);
    }
}
