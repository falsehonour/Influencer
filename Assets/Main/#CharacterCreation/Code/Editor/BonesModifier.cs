using UnityEngine;
using UnityEditor;

namespace HashtagChampion
{
namespace CharacterCreation
{
    public class BonesModifier : EditorWindow
    {
        [MenuItem("Window/Character Creation/Bones Modifier")]
        public static void OpenWindow()
        {
            var window = GetWindow<BonesModifier>();
            window.titleContent = new GUIContent("Bones Modifier");
        }

        private SkinnedMeshRenderer skinnedMesh;

        private void OnGUI()
        {

            skinnedMesh = EditorGUILayout.ObjectField
                ("Skinned Mesh", skinnedMesh, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;

            if (GUILayout.Button("Print Names"))
            {
                Debug.Log("Bones Names:");

                Transform[] bones = skinnedMesh.bones;
                for (int i = 0; i < bones.Length; i++)
                {
                    Debug.Log(bones[i].name);
                }
            }

            if (GUILayout.Button("Clear Bones"))
            {
                skinnedMesh.bones = null;
            }
        }
    }
}

}

