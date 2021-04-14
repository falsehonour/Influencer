using UnityEngine;
using UnityEditor;

namespace CharacterCreation
{
    public class CharacterStaticMeshBuilder : EditorWindow
    {
        [MenuItem("Window/Character Creation/Character Static Mesh Builder")]
        public static void OpenWindow()
        {
            var window = GetWindow<CharacterStaticMeshBuilder>();
            window.titleContent = new GUIContent("Character Static Mesh Builder");
        }

        private CharacterStaticMesh characterStaticMesh;
        private Renderer renderer;

        private void OnGUI()
        {
            characterStaticMesh = EditorGUILayout.ObjectField
                ("Target CharacterStaticMesh", characterStaticMesh, typeof(CharacterStaticMesh), true) as CharacterStaticMesh;
            renderer = EditorGUILayout.ObjectField
                ("Renderer", renderer, typeof(Renderer), true) as Renderer;

            if (GUILayout.Button("Set Target CharacterStaticMesh Fields"))
            {
                SetCharacterStaticMeshFields();
            }
        }

        private void SetCharacterStaticMeshFields()
        {
           // GameObject prefab = EditorUtility.FindPrefabRoot(meshRenderer.gameObject); // PrefabUtility.FindPrefabRoot(meshRenderer.gameObject);
            
            Transform meshRendererTransform = renderer.transform;
            TransformProperties transformOffset = new TransformProperties
            {
                position = meshRendererTransform.localPosition,
                rotation = meshRendererTransform.localRotation,
                scale = meshRendererTransform.localScale
            };

            characterStaticMesh.SetFields(meshRendererTransform.parent.name, transformOffset);
            EditorUtility.SetDirty(characterStaticMesh);
        }
    }
}
