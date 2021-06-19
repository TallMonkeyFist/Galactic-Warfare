using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AudioPair
{
    public Vector3 position;
    public int team;
}

public class AISoundManager : MonoBehaviour
{
    public static AISoundManager Singleton = null;

    public List<AudioPair> audioPositions = new List<AudioPair>();
    public List<AIHearing> aiListeners = new List<AIHearing>();

    private void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Debug.Log("Tried to instantiate a second AI Sound Manager\nDestroying the new instance");
            Destroy(this);
        }
    }

    private void FixedUpdate()
    {
        foreach(AudioPair _audio in audioPositions)
        {
            foreach(AIHearing _listener in aiListeners)
            {
                _listener.AddAudioPosition(_audio.position, _audio.team);
            }
        }
        
        audioPositions.Clear();
    }

    public void AddAudioListener(AIHearing ai)
    {
        aiListeners.Add(ai);
    }

    public void RemoveAudioListener(AIHearing ai)
    {
        aiListeners.Remove(ai);
    }

    public void AddAudioPosition(Vector3 _position, int team)
    {
        AudioPair audioPair = new AudioPair();
        audioPair.team = team;
        audioPair.position = _position;
        audioPositions.Add(audioPair);
    }

    public static void RegisterSoundAtLocation(Vector3 _position, int team = 0)
    {
        if(Singleton == null) 
        {
            Debug.LogWarning("There is no AI Sound Manager Present");
            return; 
        }
        Singleton.AddAudioPosition(_position, team);
    }

    private static AISoundManager createInstance()
    {
        Debug.LogWarning("Creating a new AI Sound Manager");
        GameObject soundManager = new GameObject("AISoundManager");
        soundManager.transform.SetParent(null);
        AISoundManager manager = soundManager.AddComponent<AISoundManager>();

        return manager;
    }

    private void OnDestroy()
    {
        for(int i = aiListeners.Count - 1; i >= 0; i--)
        {
            AIHearing temp = aiListeners[i];
            aiListeners.Remove(aiListeners[i]);
            Destroy(temp);
            Singleton = null;
        }
    }

}
