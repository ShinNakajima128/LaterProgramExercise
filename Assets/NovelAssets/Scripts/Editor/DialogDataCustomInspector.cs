using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScenarioData))]
public class DialogDataCustomInspector : Editor
{
    ScenarioData m_ScenarioData;

    private void OnEnable()
    {
        m_ScenarioData = target as ScenarioData;
    }

    public override void OnInspectorGUI()
    {
        if (!m_ScenarioData) return;

        if (GUILayout.Button("CharaUpdate"))
        {
            m_ScenarioData.LoadDialogDataFromSpreadsheet();
        }

        if (GUILayout.Button("ChoiceUpdate"))
        {
            m_ScenarioData.LoadChoicesDataFromSpreadsheet();
        }
        base.OnInspectorGUI();
        EditorUtility.SetDirty(m_ScenarioData);
    }
}
