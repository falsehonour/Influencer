using UnityEngine;

namespace CharacterCreation
{
    public class CharacterCreationReferencer : MonoBehaviour
    {
        private static CharacterCreationReferencer instance;
        [SerializeField] private CharacterMeshNameRequirements nameRequirements;
        public static CharacterMeshNameRequirements NameRequirements => instance.nameRequirements;
        [SerializeField] private CharacterReferences references;
        public static CharacterReferences References => instance.references;

        private void Awake()
        {
            instance = this;
        }
    }
}