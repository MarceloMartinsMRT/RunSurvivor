using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource audioSource;
    private AudioSource sfxLoopSource;

    private static AudioClip shootClip;
    private static AudioClip xpPickupClip;
    private static AudioClip hitClip;
    private static AudioClip levelUpClip;
    private static AudioClip chestClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = true; // Permite tocar sons na tela de Level Up e Baú pausados

        // Garante que a câmera possui um AudioListener
        if (Camera.main != null && Camera.main.GetComponent<AudioListener>() == null)
        {
            Camera.main.gameObject.AddComponent<AudioListener>();
        }

        InitializeClips();
    }

    private void InitializeClips()
    {
        if (shootClip == null) shootClip = GenerateShootClip();
        if (xpPickupClip == null) xpPickupClip = GenerateXpPickupClip();
        if (hitClip == null) hitClip = GenerateHitClip();
        if (levelUpClip == null) levelUpClip = GenerateLevelUpClip();
        if (chestClip == null) chestClip = GenerateChestClip();
    }

    public static void PlayShoot(float volume = 0.45f)
    {
        if (Instance == null) CreateInstance();
        if (Instance != null && shootClip != null)
        {
            Instance.audioSource.pitch = Random.Range(0.92f, 1.08f);
            Instance.audioSource.PlayOneShot(shootClip, volume);
        }
    }

    public static void PlayXpPickup(float volume = 0.55f)
    {
        if (Instance == null) CreateInstance();
        if (Instance != null && xpPickupClip != null)
        {
            Instance.audioSource.pitch = Random.Range(0.95f, 1.06f);
            Instance.audioSource.PlayOneShot(xpPickupClip, volume);
        }
    }

    public static void PlayHit(float volume = 0.55f)
    {
        if (Instance == null) CreateInstance();
        if (Instance != null && hitClip != null)
        {
            Instance.audioSource.pitch = Random.Range(0.88f, 1.12f);
            Instance.audioSource.PlayOneShot(hitClip, volume);
        }
    }

    public static void PlayLevelUp(float volume = 0.75f)
    {
        if (Instance == null) CreateInstance();
        if (Instance != null && levelUpClip != null)
        {
            Instance.audioSource.pitch = 1.0f;
            Instance.audioSource.PlayOneShot(levelUpClip, volume);
        }
    }

    public static void PlayChest(float volume = 0.85f)
    {
        if (Instance == null) CreateInstance();
        if (Instance != null && chestClip != null)
        {
            Instance.audioSource.pitch = 1.0f;
            Instance.audioSource.PlayOneShot(chestClip, volume);
        }
    }

    private static void CreateInstance()
    {
        GameObject soundObj = new GameObject("SoundManager");
        Instance = soundObj.AddComponent<SoundManager>();
    }

    // --- SÍNTESE PROCEDURAL DE ÁUDIO VIA PCM ---

    private static AudioClip GenerateShootClip()
    {
        int sampleRate = 44100;
        float duration = 0.11f;
        int count = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(780f, 160f, t / duration);
            phase += 2f * Mathf.PI * freq / sampleRate;
            float env = 1f - (t / duration);
            data[i] = (Mathf.Sin(phase) * 0.7f + Mathf.PingPong(phase / Mathf.PI, 1f) * 0.3f) * env * env;
        }

        AudioClip clip = AudioClip.Create("SFX_Shoot", count, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip GenerateXpPickupClip()
    {
        int sampleRate = 44100;
        float duration = 0.13f;
        int count = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float freq = (t < 0.045f) ? 1046.5f : 1568f; // C6 para G6 (duplo sino brilhante)
            phase += 2f * Mathf.PI * freq / sampleRate;
            float env = Mathf.Exp(-t * 24f);
            data[i] = Mathf.Sin(phase) * env;
        }

        AudioClip clip = AudioClip.Create("SFX_XpPickup", count, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip GenerateHitClip()
    {
        int sampleRate = 44100;
        float duration = 0.09f;
        int count = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float env = 1f - (t / duration);
            float thud = Mathf.Sin(2f * Mathf.PI * 85f * (1f - t / duration) * t);
            float noise = (Random.value * 2f - 1f) * 0.6f;
            data[i] = (thud * 0.6f + noise * 0.4f) * env * env;
        }

        AudioClip clip = AudioClip.Create("SFX_Hit", count, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip GenerateLevelUpClip()
    {
        int sampleRate = 44100;
        float duration = 0.52f;
        int count = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[count];
        float[] freqs = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
        float noteTime = duration / 4f;
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t / noteTime), 0, 3);
            float freq = freqs[noteIndex];
            phase += 2f * Mathf.PI * freq / sampleRate;
            float localT = t - (noteIndex * noteTime);
            float env = Mathf.Exp(-localT * (noteIndex == 3 ? 4.5f : 8f));
            data[i] = (Mathf.Sin(phase) * 0.8f + Mathf.Sin(phase * 2f) * 0.2f) * env;
        }

        AudioClip clip = AudioClip.Create("SFX_LevelUp", count, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip GenerateChestClip()
    {
        int sampleRate = 44100;
        float duration = 0.65f;
        int count = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[count];
        float phase1 = 0f, phase2 = 0f, phase3 = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            // Acorde triunfal maior (C5, G5, C6) com shimmer
            phase1 += 2f * Mathf.PI * 523.25f / sampleRate;
            phase2 += 2f * Mathf.PI * 783.99f / sampleRate;
            phase3 += 2f * Mathf.PI * 1046.50f / sampleRate;
            float env = Mathf.Exp(-t * 4.2f);
            float wave = (Mathf.Sin(phase1) + Mathf.Sin(phase2) + Mathf.Sin(phase3)) * 0.33f;
            data[i] = wave * env;
        }

        AudioClip clip = AudioClip.Create("SFX_Chest", count, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
