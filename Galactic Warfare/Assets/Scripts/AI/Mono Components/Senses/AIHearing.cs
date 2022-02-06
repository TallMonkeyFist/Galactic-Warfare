using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AudioEvent
{
    public Vector3 position;
    public float timeStamp;
    public int team;

    public AudioEvent(float time, Vector3 pos)
    {
        position = pos;
        timeStamp = time;
        team = 0;
    }
}

public class AIHearing : MonoBehaviour
{
    [Tooltip("Max distance that the AI can hear from")]
    [SerializeField] private float audioDistance = 30.0f;

    [HideInInspector] public int team;

    private List<AudioPair> unregisteredAudioPositions = new List<AudioPair>();

    private float sqrAudioDistance;
    private bool audioEventPolled = true;
    private AudioEvent lastAudioEvent = new AudioEvent(0, Vector3.positiveInfinity);

    private void Awake()
    {
        sqrAudioDistance = audioDistance * audioDistance;
    }

    private void Start()
    {
        if (AISoundManager.Singleton != null)
        {
            AISoundManager.Singleton.AddAudioListener(this);
        }
    }

    private void OnDestroy()
    {
        if(AISoundManager.Singleton != null)
        {
            AISoundManager.Singleton.RemoveAudioListener(this);
        }
    }

    private void FixedUpdate()
    {
        UpdateAudioEvents();

        unregisteredAudioPositions.Clear();
    }

    private void UpdateAudioEvents()
    {
        Vector3 lastAudioPosition = Vector3.positiveInfinity;

        foreach(AudioPair audio in unregisteredAudioPositions)
        {
            if ((audio.position - transform.position).sqrMagnitude < sqrAudioDistance && audio.team != team)
            {
                lastAudioPosition = audio.position;
            }
        }

        if(!lastAudioPosition.Equals(Vector3.positiveInfinity))
        {
            AudioEvent lastEvent = new AudioEvent();
            lastEvent.position = lastAudioPosition;
            lastEvent.timeStamp = Time.time;

            lastAudioEvent = lastEvent;

            audioEventPolled = false;
        }
    }

    public void AddAudioPosition(Vector3 _position, int team)
    {
        AudioPair pair = new AudioPair();
        pair.position = _position;
        pair.team = team;
        unregisteredAudioPositions.Add(pair);
    }

    public void SetAudioRange(float _range)
    {
        audioDistance = _range;
        sqrAudioDistance = _range * _range;
    }

    public bool TryGetLastAudioEvent(out AudioEvent audio)
    {
        audio = new AudioEvent(0, Vector3.positiveInfinity);

        if(!audioEventPolled && !lastAudioEvent.position.Equals(Vector3.positiveInfinity))
        {
            audio.timeStamp = lastAudioEvent.timeStamp;
            audio.position = lastAudioEvent.position;
            audio.team = lastAudioEvent.team;
            audioEventPolled = true;
            return true;
        }

        return false;
    }
}
