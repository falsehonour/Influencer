using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HashtagChampion
{
    namespace CharacterCreation
    {
        public class CharacterCreationManager : MonoBehaviour
        {
            private static Character characterPreFab;
            private static Character character;
            [SerializeField] private ButtonBehaviour characterSelectionButtonBehaviour;
            [SerializeField] private ButtonBehaviour backButtonBehaviour;
            [SerializeField] private CharacterCreationPanel leftPanel;
            [SerializeField] private CharacterCreationPanel rightPanel;
            private static CharacterCreationManager instance;

            void Start()
            {
                instance = this;
                CharacterCreationButton.InitialiseBackButton(backButtonBehaviour);
                // LocalPlayerData.Initialise();

                PlayerSkinDataHolder playerSkinData =
                    SaveAndLoadManager.Load<PlayerSkinDataHolder>(new PlayerSkinDataHolder());

                InitialiseCharacter(playerSkinData);
            }

            private void InitialiseCharacter(PlayerSkinDataHolder skinDataHolder = null)
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

                character = Instantiate(characterPreFab);
                character.transform.position = Vector3.zero;
                character.transform.rotation = Quaternion.identity;
                character.Initialise();

                if (skinDataHolder != null)
                {
                    PlayerSkinDataHolder.Data skinData = skinDataHolder.data;
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

            public void SaveCharacter()
            {
                PlayerSkinDataHolder playerSkinDataHolder = PlayerSkinDataHolder.CreatePlayerSkinData
                    (characterPreFab, character.equippedMeshesByMeshCategory, character.equippedMeshModifiersByMeshModifierCategory);

                SaveAndLoadManager.Save<PlayerSkinDataHolder>(playerSkinDataHolder);
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
