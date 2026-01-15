using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAudioCue : MonoBehaviour
{
    public AudioClip audioClip;
    public bool playOnce = true;
    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!hasPlayed || !playOnce))
        {
            AudioCueManager.Instance.PlaySFX(audioClip);
            hasPlayed = true;
            Debug.Log("entersuccess");
        }
    }
}
