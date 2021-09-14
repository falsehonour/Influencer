using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HashtagChampion
{
    namespace CharacterCreation
    {
        public class CharacterCreationManager : MenuManager
        {
            private static Character characterPreFab;
            private static Character character;
            [SerializeField] private ButtonBehaviour characterSelectionButtonBehaviour;
            [SerializeField] private ButtonBehaviour backButtonBehaviour;
            [SerializeField] private CharacterCreationPanel leftPanel;
            [SerializeField] private CharacterCreationPanel rightPanel;
            [SerializeField] private MenusCameraController cameraController;

            private string modifiedPlayerName;
            [SerializeField] private TMPro.TMP_InputField playerNameInputField;
            [SerializeField] private Transform characterParent;

            void Start()
            {
                CharacterCreationButton.InitialiseBackButton(backButtonBehaviour);
                // LocalPlayerData.Initialise();

                SkinDataHolder playerSkinData = SaveAndLoadManager.TryLoad<SkinDataHolder>();

                InitialiseCharacter(playerSkinData);

                playerNameInputField.characterLimit = PlayerName.MAX_LETTER_COUNT;
            }

            public override void Activate()
            {
                base.Activate();
                //InitialisePanels();
                modifiedPlayerName = StaticData.playerName.name;
                playerNameInputField.text = modifiedPlayerName;

                cameraController.SetIsControllable(true);

            }

            public override void Deactivate()
            {
                base.Deactivate();
                cameraController.SetIsControllable(false);
            }

            public void ModifyPlayerName()
            {
                modifiedPlayerName = PlayerName.LegaliseName(playerNameInputField.text);
                playerNameInputField.text = modifiedPlayerName;
            }

            private void InitialiseCharacter(SkinDataHolder skinDataHolder = null)
            {
                //TODO: This function is kinda gross, should be split.
                if (character != null)
                {
                    Destroy(character.gameObject);
                }
                if (skinDataHolder != null)
                {
                    characterPreFab = CharacterCreationReferencer.References.GetCharacterPreFab(skinDataHolder.data.characterPrefabIndex);
                }

                if (characterPreFab == null)
                {
                    characterPreFab = CharacterCreationReferencer.References.GetCharacterPreFab(0);
                }

                character = Instantiate(characterPreFab,characterParent);
                character.transform.position = Vector3.zero;
                character.transform.rotation = Quaternion.identity;
                character.Initialise();

                if (skinDataHolder != null)
                {
                    SkinDataHolder.Data skinData = skinDataHolder.data;
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

                InitialisePanels();
            }

            public void SaveChanges()
            {
                SkinDataHolder playerSkinDataHolder = SkinDataHolder.CreatePlayerSkinData
                    (characterPreFab, character.equippedMeshesByMeshCategory, character.equippedMeshModifiersByMeshModifierCategory);

                SaveAndLoadManager.Save<SkinDataHolder>(playerSkinDataHolder);

                StaticData.playerName.name = modifiedPlayerName;
                SaveAndLoadManager.Save<PlayerName>(StaticData.playerName);

            }

            public void RevertChanges()
            {
                //TODO: This is identical to what happens on Start();
                SkinDataHolder playerSkinData = SaveAndLoadManager.TryLoad<SkinDataHolder>();

                InitialiseCharacter(playerSkinData);
            }

            #region GUI:

            private void InitialisePanels()
            {
                ButtonBehaviour[] characterInitialButtonBehaviours = character.BaseProperties.initialButtonBehaviours;
                ButtonBehaviour[] leftPanelBehaviours = new ButtonBehaviour[characterInitialButtonBehaviours.Length + 1];
                leftPanelBehaviours[0] = characterSelectionButtonBehaviour;
                for (int i = 1; i < leftPanelBehaviours.Length; i++)
                {
                    leftPanelBehaviours[i] = characterInitialButtonBehaviours[i - 1];
                }
                leftPanel.Initialise(leftPanelBehaviours, this, rightPanel);

                rightPanel.Initialise(new ButtonBehaviour[0], this, rightPanel);
            }

            public void OnButtonClicked(ButtonBehaviour buttonBehaviour)
            {
                if (buttonBehaviour.CharacterPreFab != null)
                {
                    CharacterCreationManager.characterPreFab = buttonBehaviour.CharacterPreFab;
                    InitialiseCharacter();
                }
                else
                {
                    CharacterPiece[] characterPieces = buttonBehaviour.CharacterPieces;

                    if (characterPieces != null)
                    {
                        //Debug.Log("characterPieces.Length " + characterPieces.Length);
                        for (int i = 0; i < characterPieces.Length; i++)
                        {
                            character.EquipCharacterPiece(characterPieces[i]);
                        }
                        character.TryEquipFallbackPieces();
                    }
                }

            }
            #endregion

        }
    }
}
