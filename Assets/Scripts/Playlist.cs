using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(AudioSource))]
public class Playlist : MonoBehaviour {
    AudioSource source;
    public List<AudioClip> tracks = new List<AudioClip>();
    int trackIndex = 999;

    // Use this for initialization
    void Awake() {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update() {
        if (!source.isPlaying || Input.GetKeyDown(KeyCode.Space)) {
            if (++trackIndex >= tracks.Count)
                trackIndex = 0;
            source.clip = tracks[trackIndex];
            source.Play();
            TerrainGenerator.thi.resetMaxes();
        }
    }

    public void AddURL(string url) {
        if (url != "") {
            StartCoroutine(AddURLRoutine(url));
        }
    }

    IEnumerator AddURLRoutine(string url) {
        WWW www = new WWW(url);
        while (!www.isDone)
            yield return null;

        AudioClip clip = www.GetAudioClip(false, true);
        while (clip.loadState != AudioDataLoadState.Failed && clip.loadState != AudioDataLoadState.Loaded)
            yield return null;
        if (clip != null) {
            tracks.Add(clip);
        }
    }
}
