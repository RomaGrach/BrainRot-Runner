using System.Linq;
using UnityEngine;
using TMPro;

public class AudioManager : MonoBehaviour
{

    [Header("UI: кнопки звука")]
    [SerializeField]
    private TMP_Text[] soundToggleLabels;  // Drag & drop все TextMeshPro на эти слоты в инспекторе


    // ---------------------- INSTANCE ----------------------
    public static AudioManager Instance { get; private set; }

    [Header("Music Source")]
    [SerializeField] private AudioSource musicSource;

    [Header("Music Volumes")]
    [Range(0f, 1f)] public float menuMusicVolume = 0.5f;
    [Range(0f, 1f)] public float levelMusicVolume = 0.6f;
    [Range(0f, 1f)] public float shopMusicVolume = 0.5f;

    [Header("Music Tracks")]
    public AudioClip[] menuTracks;
    public AudioClip[] levelTracks;
    public AudioClip[] shopTracks;

    [Header("SFX Pools")]
    [SerializeField] private int sfxPoolSize = 10;
    private AudioSource[] sfxSources;
    private int sfxIndex;

    [Header("SFX Clips & Volumes")]
    public AudioClip[] buttonClickClips;
    [Range(0f, 1f)] public float buttonClickVolume = 1f;

    public AudioClip[] coinPickupClips;
    [Range(0f, 1f)] public float coinPickupVolume = 1f;

    public AudioClip[] damageClips;
    [Range(0f, 1f)] public float damageVolume = 1f;

    public AudioClip[] levelStartClips;
    [Range(0f, 1f)] public float levelStartVolume = 1f;

    public AudioClip[] levelEndClips;
    [Range(0f, 1f)] public float levelEndVolume = 1f;

    public AudioClip[] shopOpenClips;
    [Range(0f, 1f)] public float shopOpenVolume = 1f;

    public AudioClip[] shopCloseClips;
    [Range(0f, 1f)] public float shopCloseVolume = 1f;

    public AudioClip[] purchaseClips;
    [Range(0f, 1f)] public float purchaseVolume = 1f;

    [Header("Pitch Randomization")]
    [Range(0.8f, 1.2f)] public float pitchMin = 0.9f;
    [Range(0.8f, 1.2f)] public float pitchMax = 1.1f;

    private float volumeMultiplier = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Инициализация пула SFX
        sfxSources = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            var go = new GameObject($"SFXSource_{i}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            sfxSources[i] = src;
        }

        if (musicSource != null)
            musicSource.loop = true;
    }

    public void ToggleSoundAndRefreshUI()
    {
        // Переключаем звук (ваш уже реализованный метод)
        ToggleMute();

        // Решаем, что писать в тексте
        bool isOn = volumeMultiplier > 0f;
        string status = isOn ? "Звук: включен" : "Звук: выключен";

        // Обновляем все подключённые TMP-тексты
        foreach (var label in soundToggleLabels)
        {
            if (label != null)
                label.text = status;
        }
    }

    // ---------------------- MUSIC ----------------------

    public void PlayMenuMusic() => PlayMusic(menuTracks, menuMusicVolume);
    public void PlayLevelMusic() => PlayMusic(levelTracks, levelMusicVolume);
    public void PlayShopMusic() => PlayMusic(shopTracks, shopMusicVolume);
    public void StopMusic() => musicSource?.Stop();

    private void PlayMusic(AudioClip[] pool, float baseVol)
    {
        if (musicSource == null || pool == null || pool.Length == 0) return;
        if (musicSource.isPlaying && pool.Contains(musicSource.clip)) return;
        musicSource.clip = pool[Random.Range(0, pool.Length)];
        musicSource.volume = baseVol * volumeMultiplier;
        musicSource.Play();
    }

    public void ToggleMute()
    {
        volumeMultiplier = (volumeMultiplier > 0f) ? 0f : 1f;
        if (musicSource != null && musicSource.clip != null)
        {
            float vol = menuTracks.Contains(musicSource.clip) ? menuMusicVolume :
                        levelTracks.Contains(musicSource.clip) ? levelMusicVolume :
                        shopTracks.Contains(musicSource.clip) ? shopMusicVolume : 0f;
            musicSource.volume = vol * volumeMultiplier;
        }
        foreach (var src in sfxSources) src.volume = volumeMultiplier;
    }

    // ---------------------- SFX ----------------------

    public void PlayButtonClick() => PlayRandomSFX(buttonClickClips, buttonClickVolume);
    public void PlayCoinPickup() => PlayRandomSFX(coinPickupClips, coinPickupVolume);
    public void PlayDamage() => PlayRandomSFX(damageClips, damageVolume);
    public void PlayLevelStart() => PlayRandomSFX(levelStartClips, levelStartVolume);
    public void PlayLevelEnd() => PlayRandomSFX(levelEndClips, levelEndVolume);
    public void PlayShopOpen() => PlayRandomSFX(shopOpenClips, shopOpenVolume);
    public void PlayShopClose() => PlayRandomSFX(shopCloseClips, shopCloseVolume);
    public void PlayPurchase() => PlayRandomSFX(purchaseClips, purchaseVolume);

    private void PlayRandomSFX(AudioClip[] pool, float baseVol)
    {
        if (pool == null || pool.Length == 0) return;
        var clip = pool[Random.Range(0, pool.Length)];
        PlayOneShot(clip, baseVol);
    }

    private void PlayOneShot(AudioClip clip, float baseVol)
    {
        if (clip == null) return;
        var src = sfxSources[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxPoolSize;
        src.pitch = Random.Range(pitchMin, pitchMax);
        src.volume = baseVol * volumeMultiplier;
        src.PlayOneShot(clip, src.volume);
    }
}
