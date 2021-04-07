using UnityEngine;
using UnityEditor;

namespace CharacterCreation
{
    [CustomEditor(typeof(CharacterPieceRefferences))]
    public class CharacterPieceRefferencesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CharacterPieceRefferences script = (CharacterPieceRefferences)target;

            if (GUILayout.Button("Update Refferences", GUILayout.Height(40)))
            {
                script.UpdateRefferences();
            }

        }
    }
}

