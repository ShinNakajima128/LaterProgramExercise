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
    [SerializeField, Header("ダイアログリスト")]
    DialogData[] m_data = default;

    [SerializeField, Header("テキストのスピード")]
    float m_textSpeed = 1;

    [SerializeField]
    string m_playerName = default;
    #region
    [Header("パネルの各オブジェクト")]
    [SerializeField]
    GameObject m_display = default;

    [SerializeField]
    BackGroundController m_bgCtrl = default;

    [SerializeField]
    GameObject[] m_character = default;

    [SerializeField]
    Text m_characterName = default;

    [SerializeField]
    Text m_messageText = default;

    [SerializeField]
    GameObject m_clickIcon = default;

    [SerializeField]
    GameObject m_choicesPanel = default;

    [SerializeField]
    GameObject m_choicesPrefab = default;

    [SerializeField, Header("キャラクターリスト")]
    CharacterImageData[] m_imageDatas = default;
    #endregion

    int m_currentBackground = 0;
    bool m_endMessage = false;
    bool isSkip = false;
    bool isAnimPlaying = false;
    bool isChoiced = false;
    Coroutine m_currentCoroutine = default;
    DialogMasterDataClass<CharacterData> m_dialogMaster;
    delegate void DialogDataCallback<T>(T data);
    Image[] m_characterImage;
    Animator[] m_anim;

    public static DialogManager Instance { get; private set; }
    public CharacterData[] CharacterDataMaster => m_dialogMaster.Data;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        m_characterImage = new Image[m_character.Length];
        m_anim = new Animator[m_character.Length];

        for (int i = 0; i < m_character.Length; i++)
        {
            Debug.Log(m_character[i]);
            m_characterImage[i] = m_character[i].GetComponent<Image>();
            m_anim[i] = m_character[i].GetComponent<Animator>();
        }
        m_display.SetActive(false);
        StartCoroutine(StartMessage());
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
        m_choicesPanel.SetActive(false);
        m_display.SetActive(false);
        for (int n = 0; n < data.CharacterData.Length; n++)
        {
            //ダイアログをリセット
            m_endMessage = false;
            isSkip = false;

            //キャラクターのアニメーションが終わるまで待つ
            yield return WaitForCharaAnimation(data.CharacterData[n].Talker, data.CharacterData[n].Position, data.CharacterData[n].AnimationType);

            m_display.SetActive(true);
            m_characterName.text = data.CharacterData[n].Talker.Replace("プレイヤー", m_playerName);
            EmphasisCharacter(data.CharacterData[n].Position);
            //各キャラクターの全てのメッセージが表示し終わるまで
            for (int i = 0; i < data.CharacterData[n].AllMessages.Length; i++)
            {
                m_clickIcon.SetActive(false);
                m_messageText.text = "";
                int _messageCount = 0;
                string message = data.CharacterData[n].AllMessages[i].Replace("プレイヤー", m_playerName);

                //各メッセージの
                while (message.Length > _messageCount)
                {
                    m_messageText.text += message[_messageCount];  //一文字ずつ表示
                    _messageCount++;
                    yield return WaitTimer(m_textSpeed);  //次の文字を表示するのを設定した時間待つ

                    if (isSkip) //スキップされたら
                    {
                        m_messageText.text = message;
                        break;
                    }
                    yield return null;
                }
                m_endMessage = true;
                m_clickIcon.SetActive(true);

                yield return null;

                if (data.CharacterData[n].ChoicesId != 0)
                {
                    m_choicesPanel.SetActive(true);

                    for (int k = 0; k < data.ChoicesDatas.Length; k++)
                    {
                        if (data.ChoicesDatas[k].ChoicesId == data.CharacterData[n].ChoicesId) //IDが一致したら
                        {
                            CreateChoices(data.ChoicesDatas[k]);
                            break;
                        }
                    }
                    yield return new WaitUntil(() => isChoiced);

                    m_choicesPanel.SetActive(false);
                    isChoiced = false;
                }
                else
                {
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
                }
                yield return null;
            }
        }
    }

    IEnumerator WaitForCharaAnimation(string charaName, int positionIndex, string animation)
    {
        if (!m_characterImage[positionIndex].enabled)
        {
            m_characterImage[positionIndex].enabled = true;
        }

        m_characterImage[positionIndex].sprite = SetCharaImage(charaName);

        if (animation != null && animation != "") //アニメーションの指定があれば
        {
            m_anim[positionIndex].Play(animation);
            isAnimPlaying = true;
            CharacterPanel.CharacterAnim += FinishReceive;
        }
        else
        {
            yield break;
        }

        while (isAnimPlaying) //アニメーションが終わるまで待つ
        {
            if (Input.GetMouseButtonDown(0))
            {
                m_anim[positionIndex].Play("Idle");
                isAnimPlaying = false;
            }
            yield return null;
        }
        CharacterPanel.CharacterAnim -= FinishReceive;
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

    public void LoadCharaDataFromSpreadsheet(string sheetName)
    {
        for (int i = 0; i < m_data.Length; i++)
        {
            if (m_data[i].ScenarioSheetName == sheetName)  //プロパティとシート名が一致したら
            {
                LoadDialogMasterData(sheetName, (DialogMasterDataClass<CharacterData> data) =>
                {
                    m_data[i].CharacterData = data.Data;  //データ更新

                    for (int n = 0; n < m_data[i].CharacterData.Length; n++)
                    {
                        m_data[i].CharacterData[n].MessagesToArray();
                    }
                });
                return;
            }
        }
        //データがロードできなかった場合
        Debug.LogError("データをロードできませんでした");
    }

    public void LoadChoicesDataFromSpreadsheet(string sheetName)
    {
        for (int i = 0; i < m_data.Length; i++)
        {
            if (m_data[i].ChoicesSheetName == sheetName)  //プロパティとシート名が一致したら
            {
                LoadDialogMasterData(sheetName, (DialogMasterDataClass<ChoicesData> data) =>
                {
                    m_data[i].ChoicesDatas = data.Data;  //データ更新

                    for (int n = 0; n < m_data[i].ChoicesDatas.Length; n++)
                    {
                        m_data[i].ChoicesDatas[n].MessagesAndNextIdToArray();
                    }
                });
                return;
            }
        }
        //データがロードできなかった場合
        Debug.LogError("データをロードできませんでした");
    }
    
    Sprite SetCharaImage(string charaName)
    {
        Sprite chara = default;

        for (int i = 0; i < m_imageDatas.Length; i++)
        {
            if (charaName == m_imageDatas[i].CharacterName)
            {
                chara = m_imageDatas[i].CharacterImages[0];
                break;
            }
        }
        return chara;
    }

    /// <summary>
    /// アクティブなキャラクター以外を暗転させる
    /// </summary>
    /// <param name="currentIndex"></param>
    void EmphasisCharacter(int currentIndex)
    {
        for (int i = 0; i < m_characterImage.Length; i++)
        {
            if (m_characterImage[i].enabled)
            {
                if (i == currentIndex)
                {
                    m_characterImage[i].color = new Color(1, 1, 1); //アクティブにする
                }
                else
                {
                    m_characterImage[i].color = new Color(0.5f, 0.5f, 0.5f); //非アクティブにする
                }
            }
        }
    }

    //選択肢を生成する
    void CreateChoices(ChoicesData data)
    {
        for (int i = 0; i < data.AllChoices.Length; i++)
        {
            var c = Instantiate(m_choicesPrefab, m_choicesPanel.transform);
            var b = c.GetComponentInChildren<Button>();
            b.onClick.AddListener(() =>
            {
                isChoiced = true;
            });
            var t = c.GetComponentInChildren<Text>();
            t.text = data.AllChoices[i];
        }
    }

    //次に表示するメッセージを用意する
    void NextMessage()
    {

    }
    void FinishReceive()
    {
        isAnimPlaying = false;
    }
    /// <summary>
    /// ダイアログデータを読み込む
    /// </summary>
    /// <typeparam name="T"> ダイアログデータのクラス </typeparam>
    /// <param name="file"> ダイアログ名 </param>
    /// <param name="callback"></param>
    void LoadDialogMasterData<T>(string file, DialogDataCallback<T> callback)
    {
        Network.WebRequest.Request<Network.WebRequest.GetString>("https://script.google.com/macros/s/AKfycbxkXM9so9l2drzNtbaSPIcMBJTV0_fScdRw-bVXREQdkJ8Vn1Tv/exec?sheet=" + file, Network.WebRequest.ResultType.String, (string json) =>
        {
            var dldata = JsonUtility.FromJson<T>(json);
            callback(dldata);
            Debug.Log("Network download. : " + file + " / " + json + "/" + dldata);
        });
    }
}
