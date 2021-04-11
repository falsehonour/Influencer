using UnityEngine;
using UnityEditor;

namespace CharacterCreation
{
    [CustomEditor(typeof(CharacterPieceReferences))]
    public class CharacterPieceRefferencesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CharacterPieceReferences script = (CharacterPieceReferences)target;

            if (GUILayout.Button("Update Refferences", GUILayout.Height(40)))
            {
                script.UpdateRefferences();
            }

        }
    }
}

