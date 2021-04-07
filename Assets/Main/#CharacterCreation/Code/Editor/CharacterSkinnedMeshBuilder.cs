using UnityEngine;
using UnityEditor;

namespace CharacterCreation
{
    public class CharacterSkinnedMeshBuilder : EditorWindow
    {
        [MenuItem("Window/Character Creation/Character Skinned Mesh Builder")]
        public static void OpenWindow()
        {
            var window = GetWindow<CharacterSkinnedMeshBuilder>();
            window.titleContent = new GUIContent("Character Skinned Mesh Builder");
        }

        private CharacterSkinnedMesh characterSkinnedMesh;
        private SkinnedMeshRenderer skinnedMesh;

        private void OnGUI()
        {
            characterSkinnedMesh = EditorGUILayout.ObjectField
                ("Target CharacterSkinnedMesh", characterSkinnedMesh, typeof(CharacterSkinnedMesh), true) as CharacterSkinnedMesh;
            skinnedMesh = EditorGUILayout.ObjectField
                ("Skinned Mesh", skinnedMesh, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;

            if (GUILayout.Button("Set Target CharacterSkinnedMesh Fields"))
            {
                SetCharacterSkinnedMeshFields();
            }
        }

        private void SetCharacterSkinnedMeshFields()
        {
            Transform[] bones = skinnedMesh.bones;
            string[] bonesNames = new string[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                bonesNames[i] = bones[i].name;
            }

            characterSkinnedMesh.SetSkinnedMesh(skinnedMesh, skinnedMesh.rootBone.name, bonesNames);
            EditorUtility.SetDirty(characterSkinnedMesh);
        }
    }
}
