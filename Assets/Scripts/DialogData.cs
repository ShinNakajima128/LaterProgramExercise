using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogMasterData;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "MyScriptable/Create DialogData")]
public class DialogData : ScriptableObject
{
    [SerializeField]
    string m_dataName = default;

    [SerializeField]
    CharacterData[] m_characterData = default;

    public string DataName => m_dataName;
    public CharacterData[] CharacterData { get => m_characterData; set => m_characterData = value; }
}
