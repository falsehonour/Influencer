using UnityEngine;
using UnityEditor;

namespace HashtagChampion
{
namespace CharacterCreation
{
    [CustomEditor(typeof(CharacterReferences))]
    public class CharacterPieceRefferencesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CharacterReferences script = (CharacterReferences)target;

            if (GUILayout.Button("Update Refferences", GUILayout.Height(40)))
            {
                script.UpdateRefferences();
            }

        }
    }
}
}


