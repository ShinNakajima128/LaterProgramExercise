using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogData))]
public class DialogDataCustomInspector : Editor
{
    DialogData m_dialogData;

    private void OnEnable()
    {
        m_dialogData = target as DialogData;
    }

    public override void OnInspectorGUI()
    {
        if (!m_dialogData) return;

        if (GUILayout.Button("CharaUpdate"))
        {
            DialogManager.Instance.LoadCharaDataFromSpreadsheet(m_dialogData.ScenarioSheetName);
        }

        if (GUILayout.Button("ChoiceUpdate"))
        {
            DialogManager.Instance.LoadChoicesDataFromSpreadsheet(m_dialogData.ChoicesSheetName);
        }
        base.OnInspectorGUI();
        EditorUtility.SetDirty(m_dialogData);
    }
}
