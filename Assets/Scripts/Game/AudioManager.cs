using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // Ini adalah pola Singleton, agar AudioManager mudah diakses dari mana saja
    public static AudioManager Instance;
    [SerializeField] private AudioMixer mainAudioMixer;
    // AudioSource adalah komponen yang akan memainkan musik
    [SerializeField] private AudioSource backgroundMusicSource;
    // Ini adalah array yang akan menampung semua lagu Anda
    [SerializeField] private AudioClip[] musicClips;

    private int lastPlayedIndex = -1;

    void Awake()
    {
        // Pastikan hanya ada satu instance dari AudioManager
        if (Instance == null)
        {
            Instance = this;
            // Agar tidak hancur saat berpindah scene
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Cek jika AudioSource tidak sedang memainkan lagu (berhenti)
        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
        {
            PlayRandomMusic();
        }
    }

    public void PlayRandomMusic()
    {
        if (musicClips.Length == 0)
        {
            Debug.LogWarning("Tidak ada lagu yang ditambahkan ke AudioManager!");
            return;
        }

        int randomIndex;
        do
        {
            // Pilih indeks acak yang berbeda dari lagu sebelumnya
            randomIndex = Random.Range(0, musicClips.Length);
        } while (randomIndex == lastPlayedIndex);

        lastPlayedIndex = randomIndex;
        backgroundMusicSource.clip = musicClips[randomIndex];
        backgroundMusicSource.Play();
    }
    
    public void SetMasterVolume(float volume)
    {
        // Parameter volume di AudioMixer menggunakan skala desibel, jadi kita perlu mengubahnya
        mainAudioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }
    
    public void SetMusicVolume(float volume)
    {
        // Pastikan 'MusicVolume' sama persis dengan nama Exposed Parameter di Audio Mixer
        mainAudioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSfxVolume(float volume)
    {
        // Pastikan 'SfxVolume' sama persis dengan nama Exposed Parameter di Audio Mixer
        mainAudioMixer.SetFloat("SfxVolume", Mathf.Log10(volume) * 20);
    }
}