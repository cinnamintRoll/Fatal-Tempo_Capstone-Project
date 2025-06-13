using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{

    [SerializeField] List<AudioClip> clipList;
    [SerializeField] AudioSource audioSource;
    [SerializeField] bool PlayOnEnable;

    private void Start()
    {
        audioSource.ignoreListenerPause = true;
    }

    private void OnEnable()
    {
        if (PlayOnEnable)
        {
           PlayRandomSound();
        }
    }
    public void PlayRandomSound()
    {
        if (clipList == null || clipList.Count == 0 || audioSource == null)
        {
            Debug.LogWarning("Missing AudioSource or AudioClips!");
            return;
        }

        int randomIndex = Random.Range(0, clipList.Count);
        audioSource.PlayOneShot(clipList[randomIndex]);
    }
}
