using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DialogMasterData;

/// <summary>
/// ダイアログを管理するクラス
/// </summary>
public class DialogManager : MonoBehaviour
{
    [SerializeField]
    bool m_isVersionUpFlag = false;

    [Header("ダイアログリスト")]
    [SerializeField]
    DialogData[] m_data = default;

    [Header("テキストのスピード")]
    [SerializeField]
    float m_textSpeed = 1;

    [Header("パネルの各オブジェクト")]
    [SerializeField]
    GameObject m_display = default;

    [SerializeField]
    Image m_character = default;

    [SerializeField]
    Text m_characterName = default;

    [SerializeField]
    Text m_messageText = default;

    [SerializeField]
    GameObject m_clickIcon = default;

    [SerializeField]
    CharacterImageData[] m_imageDatas = default;

    bool m_endMessage = false;
    bool isSkip = false;
    Coroutine m_currentCoroutine = default;
    DialogMasterDataClass<CharacterData> m_dialogMaster;
    delegate void DialogDataCallback<T>(T data);
    public CharacterData[] CharacterDataMaster => m_dialogMaster.Data;

    void Start()
    {
        m_display.SetActive(false);
        LoadDialogMasterData("Chara", (DialogMasterDataClass<CharacterData> m_data) => m_dialogMaster = m_data);
    }

    void SetUp()
    {
        for (int i = 0; i < m_data.Length; i++)
        {
            for (int n = 0; n < m_data[i].CharacterData.Length; n++)
            {
                m_data[i].CharacterData[n] = m_dialogMaster.Data[n];
                m_data[i].CharacterData[n].MessagesToArray();
            }
        }      
    }
    /// <summary>
    /// メッセージを表示する
    /// </summary>
    /// <returns></returns>
    IEnumerator StartMessage()
    {
        m_display.SetActive(true);

        for (int i = 0; i < m_data.Length; i++)
        {
            m_currentCoroutine = StartCoroutine(DisplayMessage(m_data[i]));
            Debug.Log("開始");
            yield return m_currentCoroutine;
        }
        m_display.SetActive(false);
        Debug.Log("終了");
    }

    /// <summary>
    /// ダイアログを表示する
    /// </summary>
    /// <param name="data"> ダイアログデータ </param>
    /// <returns></returns>
    IEnumerator DisplayMessage(DialogData data)
    {
        for (int n = 0; n < data.CharacterData.Length; n++)
        {
            //ダイアログをリセット
            m_endMessage = false;
            isSkip = false;
            m_clickIcon.SetActive(false);
            m_character.sprite = SetCharaImage(data.CharacterData[n].Talker);
            m_characterName.text = data.CharacterData[n].Talker;

            for (int i = 0; i < data.CharacterData[n].AllMessages.Length; i++)
            {
                m_messageText.text = "";
                int _messageCount = 0;


                while (data.CharacterData[n].AllMessages[i].Length > _messageCount)
                {
                    m_messageText.text += data.CharacterData[n].AllMessages[i][_messageCount];  //一文字ずつ表示
                    _messageCount++;
                    yield return StartCoroutine(WaitTimer(m_textSpeed));  //次の文字を表示するのを設定した時間待つ

                    if (isSkip) //スキップされたら
                    {
                        m_messageText.text = data.CharacterData[n].AllMessages[i];
                        break;
                    }
                    yield return null;
                }
                yield return null;
                m_endMessage = true;
                m_clickIcon.SetActive(true);

                while (true)
                {
                    if (m_endMessage && Input.GetMouseButtonDown(0))    //テキストを全て表示した状態でクリックされたら
                    {
                        if (i < data.CharacterData[i].AllMessages.Length)
                        {
                            m_endMessage = false;
                            break;
                        }
                    }
                    yield return null;
                }
                yield return null;
            }
        }
        
    }

    Sprite SetCharaImage(string charaName)
    {
        Sprite chara = default;

        for (int i = 0; i < m_imageDatas.Length; i++)
        {
            if (charaName == m_imageDatas[i].CharacterName)
            {
                chara =  m_imageDatas[i].CharacterImages[0];
                break;
            }
        }
        return chara;
    }
    /// <summary>
    /// 指定した時間待機する
    /// </summary>
    /// <param name="time"> 待つ時間 </param>
    /// <returns></returns>
    IEnumerator WaitTimer(float time)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= time)
            {
                yield break;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                isSkip = true;
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// クイズデータを読み込む
    /// </summary>
    /// <typeparam name="T"> クイズデータのクラス </typeparam>
    /// <param name="file"> クイズの時代名 </param>
    /// <param name="callback"></param>
    void LoadDialogMasterData<T>(string file, DialogDataCallback<T> callback)
    {
        var data = LocalData.Load<T>(file);
        if (m_isVersionUpFlag)
        {
            Network.WebRequest.Request<Network.WebRequest.GetString>("https://script.google.com/macros/s/AKfycbxkXM9so9l2drzNtbaSPIcMBJTV0_fScdRw-bVXREQdkJ8Vn1Tv/exec?Sheet=" + file, Network.WebRequest.ResultType.String, (string json) =>
            {
                var dldata = JsonUtility.FromJson<T>(json);
                LocalData.Save<T>(file, dldata);
                callback(dldata);
                Debug.Log("Network download. : " + file + " / " + json + "/" + dldata);
                SetUp();
                StartCoroutine(StartMessage());
            });
        }
        else
        {
            Debug.Log("Local load. : " + file + " / " + data);
            callback(data);
            StartCoroutine(StartMessage());
        }
    }
}
