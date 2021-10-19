using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogMasterData;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "MyScriptable/Create DialogData")]
public class DialogData : ScriptableObject
{
    [SerializeField]
    CharacterData[] m_characterData = default;

    public CharacterData[] CharacterData => m_characterData;
}
