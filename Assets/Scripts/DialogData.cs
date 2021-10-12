using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Create DialogData")]
public class DialogData : ScriptableObject
{
    [SerializeField]
    Sprite m_characterImage = default;

    [SerializeField]
    string m_characterName = default;

    [SerializeField, TextArea(0, 10)]
    string m_message = default;

    public Sprite CharacterImage => m_characterImage;

    public string CharacterName => m_characterName;
    public string Message => m_message;
}
