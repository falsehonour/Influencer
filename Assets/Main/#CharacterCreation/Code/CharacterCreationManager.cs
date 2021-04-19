using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCreation
{
    public class CharacterCreationManager : MonoBehaviour
    {

        [SerializeField] private Character[] characterPreFabs;
        private static int currentCharacterBaseIndex = 0;
        private static Character character;

        private static ButtonBehaviour lastClickedButtonBehaviour;

        [SerializeField] private CharacterCreationPanel leftPanel;
        [SerializeField] private CharacterCreationPanel rightPanel;
        private static CharacterCreationManager instance;

        void Start()
        {
            instance = this;

            // LocalPlayerData.Initialise();

            PlayerSkinData playerSkinData = SaveAndLoadManager.Load<PlayerSkinData>(new PlayerSkinData());

            InitialiseCharacter(playerSkinData);
        }

        private void InitialiseCharacter(PlayerSkinData skinData)
        {

            if (character != null)
            {
                Destroy(character.gameObject);
            }
            if (skinData != null)
            {
                Character preFab = CharacterCreationReferencer.References.GetCharacterPreFab(skinData.characterPrefabIndex);
                for (byte i = 0; i < characterPreFabs.Length; i++)
                {
                    if(preFab == characterPreFabs[i])
                    {
                        currentCharacterBaseIndex = i;
                    }
                }
            }
            character = Instantiate(characterPreFabs[currentCharacterBaseIndex]);
            character.transform.position = Vector3.zero;
            character.transform.rotation = Quaternion.identity;
            character.Initialise();

            if (skinData != null)
            {
                for (int i = 0; i < skinData.meshIndexes.Length; i++)
                {
                    character.EquipCharacterPiece(CharacterCreationReferencer.References.GetCharacterMesh(skinData.meshIndexes[i]));
                }
                for (int i = 0; i < skinData.meshModifierIndexes.Length; i++)
                {
                    character.EquipCharacterPiece(CharacterCreationReferencer.References.GetCharacterMeshModifier(skinData.meshModifierIndexes[i]));
                }
            }
            character.TryEquipFallbackPieces();
            leftPanel.Initialise(character.BaseProperties.initialButtonBehaviours, false);
            rightPanel.Initialise(new ButtonBehaviour[0], false);
        }

        public void SwitchCharacter()
        {
            currentCharacterBaseIndex++;
            if (currentCharacterBaseIndex >= characterPreFabs.Length)
            {
                currentCharacterBaseIndex = 0;
            }

            InitialiseCharacter(null);
        }

        public void SaveCharacter()
        {
            PlayerSkinData playerSkinData = PlayerSkinData.CreatePlayerSkinData
                (characterPreFabs[currentCharacterBaseIndex], character.equippedMeshesByMeshCategory, character.equippedMeshModifiersByMeshModifierCategory);

            SaveAndLoadManager.Save<PlayerSkinData>(playerSkinData);
        }

        #region GUI:

        public static void OnButtonClicked(ButtonBehaviour buttonBehaviour, CharacterCreationPanel buttonPanel)
        {
            if(buttonBehaviour == lastClickedButtonBehaviour)
            {
                Debug.LogWarning("Tried to click the same object more than once in a row. Aborting");
                return;
            }

            lastClickedButtonBehaviour = buttonBehaviour;

            CharacterPiece[] characterPieces = buttonBehaviour.CharacterPieces;

            if(characterPieces != null )
            {
                //Debug.Log("characterPieces.Length " + characterPieces.Length);
                for (int i = 0; i < characterPieces.Length; i++)
                {
                   character.EquipCharacterPiece(characterPieces[i]);
                }
                character.TryEquipFallbackPieces();
            }

            ButtonBehaviour[] linkedButtonBehaviours = buttonBehaviour.LinkedButtonBehaviours;
            if (linkedButtonBehaviours != null && linkedButtonBehaviours.Length > 0)
            {
                CharacterCreationPanel rightPanel = instance.rightPanel;
                rightPanel.Initialise(linkedButtonBehaviours, (rightPanel == buttonPanel));
            }
        }

        
        #endregion

    }
}