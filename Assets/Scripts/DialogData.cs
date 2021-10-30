using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogMasterData;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "MyScriptable/Create DialogData")]
public class DialogData : ScriptableObject
{
    [SerializeField]
    string m_scenarioSheetName = default;

    [SerializeField]
    string m_choicesSheetName = default;

    [SerializeField]
    CharacterData[] m_characterData = default;

    [SerializeField]
    ChoicesData[] m_choicesData = default;

    [SerializeField]
    int m_backgroundType = default;

    public string ScenarioSheetName => m_scenarioSheetName;
    public string ChoicesSheetName => m_choicesSheetName;
    public CharacterData[] CharacterData { get => m_characterData; set => m_characterData = value; }
    public ChoicesData[] ChoicesDatas { get => m_choicesData; set => m_choicesData = value; }
    public int BackgroundType { get => m_backgroundType; set => m_backgroundType = value; }
}
