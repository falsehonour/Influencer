using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCreation
{
    public class CharacterCreationManager : MonoBehaviour
    {

        [SerializeField] private Character character;
        [SerializeField] private CharacterMesh[] initialMeshes;
        [SerializeField] private CharacterMeshModifier[] initialMeshModifiers;

        //[SerializeField] private List <CharacterPiece> initialPieceParts;

        /*[SerializeField] private CharacterPiece[] allCharacterPieces;
        private static CharacterPiece[][] characterPiecesByCategory;
        [SerializeField] private CharacterMorph[] allCharacterMorphs;
        private static CharacterMorph[][] characterMorphsByCategory;*/

        private static ButtonBehaviour lastClickedButtonBehaviour;
        [SerializeField] private ButtonBehaviour[] initialButtonBehaviours;
        #region GUI
        [Header("GUI")]
        [SerializeField] private CharacterCreationButton[] leftPanelButtons;
        [SerializeField] private CharacterCreationButton[] rightPanelButtons;

        #endregion
        private static CharacterCreationManager instance;

        void Start()
        {
            instance = this;
            Initialise();
        }

        private void Initialise()
        {
            InitialiseCharacter();
            InitialiseGUI();
        }

        private void InitialiseCharacter()
        {
            character.Initialise();
            for (int i = 0; i < initialMeshes.Length; i++)
            {
                EquipCharacterPiece(initialMeshes[i]);
            }
            for (int i = 0; i < initialMeshModifiers.Length; i++)
            {
                EquipCharacterPiece(initialMeshModifiers[i]);
            }
        }

        private static void EquipCharacterPiece(CharacterPiece characterPiece)
        {
            instance.character.EquipCharacterPiece(characterPiece);
        }

        #region GUI:
        private void InitialiseGUI()
        {
            // Debug.Log("Initialise GUI Panels");
            ShowButtons(leftPanelButtons, initialButtonBehaviours, false);
            ShowButtons(rightPanelButtons, new ButtonBehaviour[0], false);
        }

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
                    EquipCharacterPiece(characterPieces[i]);
                }
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