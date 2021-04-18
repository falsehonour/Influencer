using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCreation
{
    public class CharacterCreationManager : MonoBehaviour
    {

        [SerializeField] private Character[] characterPreFabs;
        private int currentCharacterBaseIndex = 0;
        private static Character character;

        /* [SerializeField] private Character character;
         [SerializeField] private CharacterMesh[] initialMeshes;
         [SerializeField] private CharacterMeshModifier[] initialMeshModifiers;*/

        //[SerializeField] private List <CharacterPiece> initialPieceParts;
        /*[SerializeField] private CharacterPiece[] allCharacterPieces;
        private static CharacterPiece[][] characterPiecesByCategory;
        [SerializeField] private CharacterMorph[] allCharacterMorphs;
        private static CharacterMorph[][] characterMorphsByCategory;*/

        private static ButtonBehaviour lastClickedButtonBehaviour;
        #region GUI
        [Header("GUI")]
        [SerializeField] private CharacterCreationButton[] leftPanelButtons;
        [SerializeField] private CharacterCreationButton[] rightPanelButtons;

        #endregion
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
            ShowButtons(leftPanelButtons, character.BaseProperties.initialButtonBehaviours, false);
            ShowButtons(rightPanelButtons, new ButtonBehaviour[0], false);
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

        public static void OnButtonClicked(ButtonBehaviour buttonBehaviour)
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
                instance.ShowButtons(instance.rightPanelButtons, linkedButtonBehaviours, false);
            }
        }

        private void ShowButtons(CharacterCreationButton[] buttons, ButtonBehaviour[] buttonBehaviours, bool addBackButton)
        {
            for (byte i = 0; i < buttons.Length; i++)
            {
                CharacterCreationButton button = buttons[i];
                
                if (i < buttonBehaviours.Length)
                {                   
                    button.gameObject.SetActive(true);
                    button.Initialise(buttonBehaviours[i]);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
            if (addBackButton)
            {
                return; //TODO: Add functionality
               /* CharacterCreationButton backButton = buttons[buttonBehaviours.Length];
                backButton.gameObject.SetActive(true);
                backButton.Initialise(TextReverser.Reverse("חזור"), piece.Category);*/
            }
        }
        #endregion

    }
}