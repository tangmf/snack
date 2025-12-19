using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioSource audioObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Plays a single audio clip
    public void PlayAudioClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(audioObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    // Plays a random audio clip from an array
    public void PlayRandomAudioClip(AudioClip[] audioClips, Transform spawnTransform, float volume)
    {
        int rand = Random.Range(0, audioClips.Length);
        AudioSource audioSource = Instantiate(audioObject, spawnTransform.position, Quaternion.identity);
        
        audioSource.clip = audioClips[rand];
        audioSource.volume = volume;
        audioSource.Play();

        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayLoop(AudioClip clip, float volume)
    {
        if (audioObject.clip == clip && audioObject.isPlaying) return;

        audioObject.clip = clip;
        audioObject.volume = volume;
        audioObject.loop = true;
        audioObject.Play();
    }

    public void StopLoop()
    {
        audioObject.Stop();
        audioObject.clip = null;
        audioObject.loop = false;
    }

    public void SetVolume(float volume)
    {
        if (audioObject != null)
            audioObject.volume = volume;
    }
}
