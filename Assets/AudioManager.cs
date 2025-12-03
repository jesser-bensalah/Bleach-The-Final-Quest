using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] playlist;
    public AudioSource audioSource;
    private int musicIndex = 0;

    public AudioMixerGroup soundEffectMixer;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de AudioManager dans la scène");
            return;
        }

        instance = this;
    }

    void Start()
    {
        if (playlist.Length > 0 && audioSource != null)
        {
            audioSource.clip = playlist[0];
            audioSource.Play();
        }
    }

    void Update()
    {
        if (audioSource != null && !audioSource.isPlaying && playlist.Length > 0)
        {
            PlayNextSong();
        }
    }

    void PlayNextSong()
    {
        if (playlist.Length == 0) return;

        musicIndex = (musicIndex + 1) % playlist.Length;
        audioSource.clip = playlist[musicIndex];
        audioSource.Play();
    }

    public AudioSource PlayClipAt(AudioClip clip, Vector3 pos)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioClip est null dans PlayClipAt");
            return null;
        }

        // Crée un GameObject caché pour éviter l'affichage d'image
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = pos;

        // IMPORTANT: Cache complètement le GameObject
        tempGO.hideFlags = HideFlags.HideAndDontSave;

        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;

        // Vérifier si le mixer est assigné
        if (soundEffectMixer != null)
            audioSource.outputAudioMixerGroup = soundEffectMixer;

        // Désactive spatialBlend pour éviter les effets 3D
        audioSource.spatialBlend = 0f;

        audioSource.Play();
        Destroy(tempGO, clip.length);
        return audioSource;
    }
}