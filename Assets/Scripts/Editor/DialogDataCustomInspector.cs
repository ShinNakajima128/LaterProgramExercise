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

        if (GUILayout.Button("Update"))
        {
            DialogManager.Instance.LoadDataFromSpreadsheet(m_dialogData.DataName);
        }
        base.OnInspectorGUI();
        EditorUtility.SetDirty(m_dialogData);
    }
}
