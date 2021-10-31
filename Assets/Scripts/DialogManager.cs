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
    #region serialize field
    [Header("ダイアログリスト")]
    [SerializeField]
    DialogData[] m_data = default;

    [Header("テキストのスピード")]
    [SerializeField]
    float m_textSpeed = 1;

    [Header("自動再生モードで次に進むまでの時間")]
    [SerializeField]
    float m_autoflowTime = 1.5f;

    [SerializeField]
    string m_playerName = default;

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

    [SerializeField]
    GameObject m_logPanel = default;

    [SerializeField, Header("キャラクターリスト")]
    CharacterImageData[] m_imageDatas = default;
    #endregion

    #region field
    int m_nextMessageId = 0;
    int m_AfterReactionMessageId = 0;
    bool m_endMessage = false;
    bool isSkip = false;
    bool isAnimPlaying = false;
    bool isChoiced = false;
    bool isReactioned = false;
    IEnumerator m_currentCoroutine = default;
    Image[] m_characterImage;
    Animator[] m_anim;
    #endregion

    #region property
    public static DialogManager Instance { get; private set; }
    public bool IsAutoflow { get; set; }
    public int AfterReactionMessageId { get => m_AfterReactionMessageId; set => m_AfterReactionMessageId = value; }
    #endregion

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
            m_characterImage[i] = m_character[i].GetComponent<Image>();
            m_anim[i] = m_character[i].GetComponent<Animator>();
        }
        m_display.SetActive(false);
        StartCoroutine(StartMessage());
    }

    #region Coroutine
    /// <summary>
    /// メッセージを表示する
    /// </summary>
    /// <returns></returns>
    IEnumerator StartMessage()
    {
        m_display.SetActive(true);

        for (int i = 0; i < m_data.Length; i++)
        {
            if (i == 0)
            {
                m_bgCtrl.Setup(m_data[i].BackgroundType); //最初の背景をセットする
            }
            else
            {
                m_bgCtrl.Crossfade(m_data[i].BackgroundType); //次のダイアログの背景にクロスフェードする
                isAnimPlaying = true;
                BackGroundController.BackgroundAnim += FinishReceive;

                //クロスフェードが終わるまで待つ
                while (isAnimPlaying)
                {
                    yield return null;
                }
                BackGroundController.BackgroundAnim -= FinishReceive;
            }

            m_currentCoroutine = DisplayMessage(m_data[i]);
            yield return m_currentCoroutine;
        }
        //全てのダイアログが終了したらこの下の処理が行われる
        m_display.SetActive(false);
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
        int currentDialogIndex = 0;

        while (currentDialogIndex < data.CharacterData.Length)
        {
            //ダイアログをリセット
            m_endMessage = false;
            isSkip = false;

            //キャラクターのアニメーションが終わるまで待つ
            yield return WaitForCharaAnimation(data.CharacterData[currentDialogIndex].Talker,
                                               data.CharacterData[currentDialogIndex].Position,
                                               data.CharacterData[currentDialogIndex].AnimationType);

            m_display.SetActive(true);
            m_characterName.text = data.CharacterData[currentDialogIndex].Talker.Replace("プレイヤー", m_playerName);
            EmphasisCharacter(data.CharacterData[currentDialogIndex].Position); //アクティブなキャラ以外を暗転する

            for (int i = 0; i < data.CharacterData[currentDialogIndex].AllMessages.Length; i++)
            {
                if (m_characterImage[data.CharacterData[currentDialogIndex].Position].enabled)
                {
                    m_characterImage[data.CharacterData[currentDialogIndex].Position].sprite = SetCharaImage(data.CharacterData[currentDialogIndex].Talker, data.CharacterData[currentDialogIndex].FaceTypes[i]);
                }
                m_clickIcon.SetActive(false);
                m_messageText.text = "";
                int _messageCount = 0;
                string message = data.CharacterData[currentDialogIndex].AllMessages[i].Replace("プレイヤー", m_playerName);

                //各メッセージを一文字ずつ表示する
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

                //自動再生モードがOFFならクリックアイコンを表示
                if (!IsAutoflow)
                {
                    m_clickIcon.SetActive(true);
                }

                yield return null;

                if (data.CharacterData[currentDialogIndex].ChoicesId != 0) //選択肢がある場合
                {
                    m_choicesPanel.SetActive(true);

                    for (int k = 0; k < data.ChoicesDatas.Length; k++)
                    {
                        if (data.ChoicesDatas[k].ChoicesId == data.CharacterData[currentDialogIndex].ChoicesId) //IDが一致したら
                        {
                            CreateChoices(data.CharacterData, data.ChoicesDatas[k], data.ChoicesDatas[k].NextId); //選択肢を生成
                            break;
                        }
                    }
                    yield return new WaitUntil(() => isChoiced); //ボタンが押されるまで待つ

                    m_choicesPanel.SetActive(false); //選択肢画面を非表示にする

                    //選択肢を選択した直後だったら
                    if (isChoiced && !isReactioned)
                    {
                        currentDialogIndex = m_nextMessageId; //選択した項目に対応したメッセージに次に表示する
                        isChoiced = false;
                        isReactioned = true;
                    }
                }
                else
                {
                    float timer = default;

                    while (true)
                    {
                        //自動再生モードがONだったら
                        if (IsAutoflow)
                        {
                            if (m_clickIcon.activeSelf)
                            {
                                m_clickIcon.SetActive(false);
                            }
                            timer += Time.deltaTime;

                            if (timer >= m_autoflowTime)
                            {
                                m_endMessage = false;
                                break;
                            }
                        }
                        if (m_endMessage && IsInputed())    //テキストを全て表示した状態でクリックされたら
                        {
                            m_endMessage = false;
                            break;
                        }
                        yield return null;
                    }
                }
                yield return null;
            }
            //選択肢に対応したメッセージが表示済みだったら
            if (isReactioned)
            {
                currentDialogIndex = m_AfterReactionMessageId;
                isReactioned = false;
            }
            else
            {
                currentDialogIndex = data.CharacterData[currentDialogIndex].NextId;
            }
            yield return null;
        }
        //ダイアログの内容が全て終了したら表示中のキャラクターをフェードアウトさせ、フェードが終了するまで待つ。
        yield return WaitForFinishDialogFadeOut();
    }

    IEnumerator WaitForCharaAnimation(string charaName, int positionIndex, string animation)
    {
        if (!m_characterImage[positionIndex].enabled)
        {
            m_characterImage[positionIndex].enabled = true;
        }

        m_characterImage[positionIndex].sprite = SetCharaImage(charaName);

        if (animation != null && animation != "なし") //アニメーションの指定があれば
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
            if (IsInputed())
            {
                m_anim[positionIndex].Play("Idle");
                isAnimPlaying = false;
            }
            yield return null;
        }
        CharacterPanel.CharacterAnim -= FinishReceive;
    }

    IEnumerator WaitForFinishDialogFadeOut()
    {
        for (int i = 0; i < m_characterImage.Length; i++)
        {
            if (m_characterImage[i].enabled)
            {
                m_characterImage[i].color = new Color(1, 1, 1);
                m_anim[i].Play("FadeOut");
                isAnimPlaying = true;
                CharacterPanel.CharacterAnim += FinishReceive;
            }
        }
        m_display.SetActive(false);

        while (isAnimPlaying) //アニメーションが終わるまで待つ
        {
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
            else if (IsInputed())
            {
                isSkip = true;
                yield break;
            }
            yield return null;
        }
    }
    #endregion

    /// <summary>
    /// 会話ログを表示する
    /// </summary>
    public void OpenLog()
    {
        m_logPanel.SetActive(true);

        //クリックアイコンが点滅していたら目立つので非表示にする
        if (m_clickIcon.activeSelf)
        {
            m_clickIcon.SetActive(false);
        }
        StopCoroutine(m_currentCoroutine); //再生中のコルーチンを一時停止
    }

    /// <summary>
    /// 会話ログを閉じる
    /// </summary>
    public void CloseLog()
    {
        m_logPanel.SetActive(false);

        if (m_endMessage)
        {
            m_clickIcon.SetActive(true);
        }
        StartCoroutine(m_currentCoroutine); //コルーチン再開
    }

    public void SwitchIndex(int nextId)
    {
        m_nextMessageId = nextId;
    }

    Sprite SetCharaImage(string charaName, int faceType = 0)
    {
        Sprite chara = default;

        for (int i = 0; i < m_imageDatas.Length; i++)
        {
            if (charaName == m_imageDatas[i].CharacterName)
            {
                chara = m_imageDatas[i].CharacterImages[faceType];
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

    /// <summary>
    /// 選択肢を生成する
    /// </summary>
    /// <param name="characterDatas"> キャラクターのデータ </param>
    /// <param name="data"> 選択肢のデータ </param>
    /// <param name="id"> 選択肢を押した後に表示するメッセージのID </param>
    void CreateChoices(CharacterData[] characterDatas, ChoicesData data, int[] id)
    {
        for (int i = 0; i < data.AllChoices.Length; i++)
        {
            var c = Instantiate(m_choicesPrefab, m_choicesPanel.transform);
            var choiceButton = c.GetComponent<ChoicesButton>();
            choiceButton.NextMessageId = id[i];

            for (int n = 0; n < characterDatas.Length; n++)
            {
                if (characterDatas[n].MessageId == id[i])
                {
                    choiceButton.AfterReactionMessageId = id[i];
                }
            }

            var b = c.GetComponent<Button>();
            b.onClick.AddListener(() =>
            {
                isChoiced = true;
                foreach (Transform child in m_choicesPanel.transform)
                {
                    Destroy(child.gameObject);
                }
            });
            var t = c.GetComponentInChildren<Text>();
            t.text = data.AllChoices[i];
        }
    }

    /// <summary>
    /// 入力判定を行う
    /// </summary>
    /// <returns> 入力判定 </returns>
    bool IsInputed()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void FinishReceive()
    {
        isAnimPlaying = false;
    }
}
