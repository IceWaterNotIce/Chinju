using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> bgmClips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> sfxClips = new List<AudioClip>();

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        // 實現單例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化音頻源
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // 加載保存的音量設置
        LoadVolumeSettings();
    }

    #region Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateMixerVolumes();
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateMixerVolumes();
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateMixerVolumes();
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
    }

    private void UpdateMixerVolumes()
    {
        // 將線性音量轉換為對數音量（dB）
        if (audioMixer != null)
        {
            audioMixer.SetFloat("Master", LinearToDecibel(masterVolume));
            audioMixer.SetFloat("BGM", LinearToDecibel(bgmVolume));
            audioMixer.SetFloat("SFX", LinearToDecibel(sfxVolume));
        }
        else
        {
            // 如果沒有使用 AudioMixer，直接設置 AudioSource 音量
            bgmSource.volume = masterVolume * bgmVolume;
            sfxSource.volume = masterVolume * sfxVolume;
        }
    }

    private float LinearToDecibel(float linear)
    {
        // 將線性音量值轉換為對數值（dB）
        // 0-1 映射到 -80dB 到 0dB
        return linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        UpdateMixerVolumes();
    }
    #endregion

    #region BGM Control
    public void PlayBGM(int index)
    {
        if (index >= 0 && index < bgmClips.Count)
        {
            bgmSource.clip = bgmClips[index];
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM index {index} out of range!");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    public void ChangeBGM(AudioClip newClip)
    {
        bgmSource.clip = newClip;
        bgmSource.Play();
    }
    #endregion

    #region SFX Control
    public void PlaySFX(int index)
    {
        if (index >= 0 && index < sfxClips.Count)
        {
            sfxSource.PlayOneShot(sfxClips[index]);
        }
        else
        {
            Debug.LogWarning($"SFX index {index} out of range!");
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXAtPosition(int index, Vector3 position)
    {
        if (index >= 0 && index < sfxClips.Count)
        {
            AudioSource.PlayClipAtPoint(sfxClips[index], position, masterVolume * sfxVolume);
        }
    }
    #endregion

    #region Utility Methods
    public void AddBGMClip(AudioClip clip)
    {
        if (!bgmClips.Contains(clip))
        {
            bgmClips.Add(clip);
        }
    }

    public void AddSFXClip(AudioClip clip)
    {
        if (!sfxClips.Contains(clip))
        {
            sfxClips.Add(clip);
        }
    }

    public void ClearAllClips()
    {
        bgmClips.Clear();
        sfxClips.Clear();
    }
    #endregion
}