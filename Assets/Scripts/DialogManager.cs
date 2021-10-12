using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ダイアログを管理するクラス
/// </summary>
public class DialogManager : MonoBehaviour
{
    [SerializeField]
    GameObject m_display = default;

    [Header("ダイアログリスト")]
    [SerializeField]
    DialogData[] m_data = default;

    [SerializeField]
    Image m_character = default;

    [SerializeField]
    Text m_characterName = default;

    [SerializeField]
    Text m_messageText = default;

    [SerializeField]
    float m_textSpeed = 1;

    [SerializeField]
    GameObject m_clickIcon = default;

    bool m_endMessage = false;
    bool isSkip = false;
    Coroutine m_currentCoroutine = default;

    void Start()
    {
        StartCoroutine(StartMessage());
    }

    /// <summary>
    /// ダイアログを表示する
    /// </summary>
    /// <param name="data"> ダイアログデータ </param>
    /// <returns></returns>
    IEnumerator DisplayMessage(DialogData data)
    {
        //プロパティをリセット
        m_endMessage = false;
        isSkip = false;
        m_clickIcon.SetActive(false);
        m_character.sprite = data.CharacterImage;
        m_characterName.text = data.CharacterName;
        m_messageText.text = "";
        int _messageCount = 0;

        yield return null;

        while (data.Message.Length > _messageCount)
        {
            if (!m_endMessage)
            {
                m_messageText.text += data.Message[_messageCount];  //一文字ずつ表示
                _messageCount++;
                yield return StartCoroutine(WaitTimer(m_textSpeed));  //次の文字を表示するのを設定した時間待つ

                if (isSkip) //スキップされたら
                {
                    m_messageText.text = data.Message;
                    break; 
                }
            }
            else
            {
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
                yield break;
            }
            yield return null;
        }
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
    /// メッセージを表示する
    /// </summary>
    /// <returns></returns>
    IEnumerator StartMessage()
    {
        m_display.SetActive(true);

        for (int i = 0; i < m_data.Length; i++)
        {
            if (m_currentCoroutine != null)
            {
                StopCoroutine(m_currentCoroutine);
                m_currentCoroutine = null;
            }

            m_currentCoroutine = StartCoroutine(DisplayMessage(m_data[i]));
            Debug.Log("開始");
            yield return m_currentCoroutine;
        }
        m_display.SetActive(false);
        Debug.Log("終了");
    }
}
