using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> musicTracks;
    private int currentTrackIndex = 0;

    void Start()
    {
        if (musicTracks.Count > 0)
        {
            PlayTrack(currentTrackIndex);
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            NextTrack();
        }
    }

    void PlayTrack(int index)
    {
        if (index < 0 || index >= musicTracks.Count) return;
        audioSource.clip = musicTracks[index];
        audioSource.Play();
    }

    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Count;
        PlayTrack(currentTrackIndex);
    }
}
