using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCreation
{
    [CreateAssetMenu(fileName = "CharacterBaseProperties", menuName = "Character Creation/CharacterBaseProperties")]

    public class CharacterBaseProperties : ScriptableObject
    {
        public CharacterMesh[] fallbackMeshes;
        public CharacterMeshModifier[] fallbackMeshModifiers;
        public ButtonBehaviour[] initialButtonBehaviours;
    }
}

