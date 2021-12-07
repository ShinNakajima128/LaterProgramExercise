using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;


/// <summary>
/// 音関連を管理するクラス
/// </summary>
public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    [Header("タイトル画面のScene名")]
    [SerializeField] 
    string m_titleScene = "Title";

    [Header("プレイ画面のScene名")]
    [SerializeField] 
    string m_mainScene = "Main";

    [Header("リザルト画面のScene名")]
    [SerializeField] 
    string m_resultScene = "Result";

    [Header("マスター音量")]
    [SerializeField, Range(0f, 1f)]
    float m_masterVolume = 1.0f;

    [Header("BGMの音量")]
    [SerializeField, Range(0f, 1f)] 
    float m_bgmVolume = 0.1f;

    [Header("SEの音量")]
    [SerializeField, Range(0f, 1f)] 
    float m_seVolume = 1.0f;

    [Header("VOICEの音量")]
    [SerializeField, Range(0f, 1f)] 
    float m_voiceVolume = 1.0f;

    [Header("BGMリスト")]
    [SerializeField]
    AudioClip[] m_bgms = null;

    [Header("SEリスト")]
    [SerializeField] 
    AudioClip[] m_ses = null;

    [Header("VOICEリスト")]
    [SerializeField] 
    AudioClip[] m_voices = null;

    [Header("BGMのAudioSource")]
    [SerializeField]
    AudioSource m_bgmAudioSource = null;

    [Header("SEのAudioSource")]
    [SerializeField]
    AudioSource m_seAudioSource = null;

    [Header("VOICEのAudioSource")]
    [SerializeField] 
    AudioSource m_voiceAudioSource = null;

    [Header("デバッグ用")]
    [SerializeField]
    bool m_debug = false;

    /// <summary> マスター音量時のフラグ </summary>
    bool masterVolumeChange = false;
    /// <summary> BGM音量時のフラグ </summary>
    bool bgmVolumeChange = false;
    /// <summary> SE音量時のフラグ </summary>
    bool seVolumeChange = false;
    /// <summary> ボイス音量時のフラグ </summary>
    bool voiceVolumeChange = false;
    Dictionary<string, int> bgmIndex = new Dictionary<string, int>();
    Dictionary<string, int> seIndex = new Dictionary<string, int>();
    Dictionary<string, int> voiceIndex = new Dictionary<string, int>();

    public float GetMasterVolume { get => m_masterVolume; }
    public float GetBgmVolume { get => m_bgmVolume; }
    public float GetSeVolume { get => m_seVolume; }
    public float GetVoiceVolume { get => m_voiceVolume; }

    void Awake()
    {
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < m_bgms.Length; i++)
        {
            bgmIndex.Add(m_bgms[i].name, i);
        }

        for (int i = 0; i < m_ses.Length; i++)
        {
            seIndex.Add(m_ses[i].name, i);
        }

        for (int i = 0; i < m_voices.Length; i++)
        {
            voiceIndex.Add(m_ses[i].name, i);
        }
    }

    private void Start()
    {
        if (m_debug)
        {
            PlayBgmByName("");
        }

        if (Instance && !m_debug)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (SceneManager.GetActiveScene().name == m_titleScene)
            {
                PlayBgmByName("");
            }
            else if (SceneManager.GetActiveScene().name == m_mainScene)
            {
                PlayBgmByName("");
            }
            else if (SceneManager.GetActiveScene().name == m_resultScene)
            {
                PlayBgmByName("");
            }
        }  
    }

    /// <summary>
    /// Sceneが遷移した時にBGMを変更する
    /// </summary>
    /// <param name="nextScene">遷移後のScene</param>
    /// <param name="mode"></param>
    void OnSceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Title":
                PlayBgmByName("");
                break;
            case "Main":
                PlayBgmByName("");
                break;
            case "Result":
                PlayBgmByName("");
                break;
        }
    }

    void Update()
    {
        VolumeChanger();
    }

    /// <summary>
    /// 指定したBGMを再生する
    /// </summary>
    /// <param name="name"> BGMの名前 </param>
    public void PlayBgmByName(string name)
    {
        PlayBgm(GetBgmIndex(name));
    }

    /// <summary>
    /// 指定したSEを再生する
    /// </summary>
    /// <param name="name"> SEの名前 </param>
    public void PlaySeByName(string name)
    {
        PlaySe(GetSeIndex(name));
    }

    /// <summary>
    /// 指定したボイスを再生する
    /// </summary>
    /// <param name="name"> ボイスの名前 </param>
    public void PlayVoiceByName(string name)
    {
        PlayVoice(GetVoiceIndex(name));
    }

    /// <summary>
    /// 指定した場所でSEを再生する
    /// </summary>
    /// <param name="name"> SEの名前 </param>
    public void PlaySeAtPointByName(string name, Vector3 point)
    {
        PlaySeAtPoint(GetSeIndex(name), point);
    }

    /// <summary>
    /// 指定した場所でボイスを再生する
    /// </summary>
    /// <param name="name"> ボイスの名前 </param>
    public void PlayVoiceAtPointByName(string name, Vector3 point)
    {
        PlayVoiceAtPoint(GetVoiceIndex(name), point);
    }

    /// <summary>
    /// BGMの再生を停止する
    /// </summary>
    public void StopBgm()
    {
        m_bgmAudioSource.Stop();
        m_bgmAudioSource.clip = null;
    }

    /// <summary>
    /// SEの再生を停止する
    /// </summary>
    public void StopSe()
    {
        m_seAudioSource.Stop();
        m_seAudioSource.clip = null;
    }

    /// <summary>
    /// ボイスの再生を停止する
    /// </summary>
    public void StopVoice()
    {
        m_voiceAudioSource.Stop();
        m_voiceAudioSource.clip = null;
    }

    /// <summary>
    /// 各音量を変更する
    /// </summary>
    public void VolumeChanger()
    {
        if (m_bgmAudioSource && bgmVolumeChange || m_bgmAudioSource && masterVolumeChange)
        {
            m_bgmAudioSource.volume = m_bgmVolume * m_masterVolume;
            if (masterVolumeChange) masterVolumeChange = false;
            if(bgmVolumeChange) bgmVolumeChange = false;
        }

        if (m_seAudioSource && seVolumeChange || m_seAudioSource && masterVolumeChange)
        {
            m_seAudioSource.volume = m_seVolume * m_masterVolume;
            if (masterVolumeChange) masterVolumeChange = false;
            if (seVolumeChange) seVolumeChange = false;
        }

        if (m_voiceAudioSource && voiceVolumeChange || m_voiceAudioSource && masterVolumeChange)
        {
            m_voiceAudioSource.volume = m_voiceVolume * m_masterVolume;
            if (masterVolumeChange) masterVolumeChange = false;
            if (voiceVolumeChange) voiceVolumeChange = false;
        }
    }
    
    /// <summary>
    /// マスター音量を変更する
    /// </summary>
    /// <param name="masterValue"> 音量 </param>
    public void MasterVolChange(float masterValue)
    {
        m_masterVolume = masterValue;
        masterVolumeChange = true;
    }

    /// <summary>
    /// BGM音量を変更する
    /// </summary>
    /// <param name="bgmValue"> 音量 </param>
    public void BgmVolChange(float bgmValue)
    {
        m_bgmVolume = bgmValue;
        bgmVolumeChange = true;
    }

    /// <summary>
    /// SE音量を変更する
    /// </summary>
    /// <param name="seValue"> 音量 </param>
    public void SeVolChange(float seValue)
    {
        m_seVolume = seValue;
        seVolumeChange = true;
    }

    /// <summary>
    /// ボイス音量を変更する
    /// </summary>
    /// <param name="voiceValue"> 音量 </param>
    public void VoiceVolChange(float voiceValue)
    {
        m_voiceVolume = voiceValue;
        voiceVolumeChange = true;
    }

    void PlayBgm(int index)
    {
        if (Instance != null)
        {
            index = Mathf.Clamp(index, 0, m_bgms.Length);

            m_bgmAudioSource.clip = m_bgms[index];
            m_bgmAudioSource.loop = true;
            m_bgmAudioSource.volume = m_bgmVolume * m_masterVolume;
            m_bgmAudioSource.Play();
        }
    }

    void PlaySe(int index)
    {
        index = Mathf.Clamp(index, 0, m_ses.Length);

        m_seAudioSource.PlayOneShot(m_ses[index], m_seVolume * m_masterVolume);
    }

    void PlaySeAtPoint(int index, Vector3 point)
    {
        index = Mathf.Clamp(index, 0, m_ses.Length);

        AudioSource.PlayClipAtPoint(m_ses[index], point, m_seVolume * m_masterVolume);
    }

    void PlayVoice(int index)
    {
        index = Mathf.Clamp(index, 0, m_voices.Length);

        m_voiceAudioSource.PlayOneShot(m_voices[index], m_voiceVolume * m_masterVolume);
    }

    void PlayVoiceAtPoint(int index, Vector3 point)
    {
        index = Mathf.Clamp(index, 0, m_voices.Length);

        AudioSource.PlayClipAtPoint(m_voices[index], point, m_voiceVolume * m_masterVolume);
    }

    int GetBgmIndex(string name)
    {
        if (bgmIndex.ContainsKey(name))
        {
            return bgmIndex[name];
        }
        else
        {
            Debug.Log("指定したBGMがありませんでした");
            return 0;
        }
    }

    int GetSeIndex(string name)
    {
        if (seIndex.ContainsKey(name))
        {
            return seIndex[name];
        }
        else
        {
            Debug.Log("指定したSEがありませんでした");
            return 0;
        }
    }

    int GetVoiceIndex(string name)
    {
        if (voiceIndex.ContainsKey(name))
        {
            return voiceIndex[name];
        }
        else
        {
            Debug.Log("指定したボイスがありませんでした");
            return 0;
        }
    }
}