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


        [SerializeField] private CharacterCreationPanel leftPanel;
        [SerializeField] private CharacterCreationPanel rightPanel;
        private static CharacterCreationManager instance;

        void Start()
        {
            instance = this;

            // LocalPlayerData.Initialise();

            PlayerSkinDataHolder playerSkinData = SaveAndLoadManager.Load<PlayerSkinDataHolder>(new PlayerSkinDataHolder());

            InitialiseCharacter(playerSkinData);
        }

        private void InitialiseCharacter(PlayerSkinDataHolder skinDataHolder)
        {

            if (character != null)
            {
                Destroy(character.gameObject);
            }
            if (skinDataHolder != null)
            {
                Character preFab = CharacterCreationReferencer.References.GetCharacterPreFab(skinDataHolder.data.characterPrefabIndex);
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
            leftPanel.Initialise(character.BaseProperties.initialButtonBehaviours,this, rightPanel);
            rightPanel.Initialise(new ButtonBehaviour[0], this, rightPanel);
        }

        public void SwitchCharacter(int indexModifier)
        {

            currentCharacterBaseIndex += indexModifier;
            if (currentCharacterBaseIndex >= characterPreFabs.Length)
            {
                currentCharacterBaseIndex = 0;
            }
            else if(currentCharacterBaseIndex < 0)
            {
                currentCharacterBaseIndex = characterPreFabs.Length -1;
            }
            //TODO: going out of bounds will not give the results one might expect
            InitialiseCharacter(null);
        }

        public void SaveCharacter()
        {
            PlayerSkinDataHolder playerSkinDataHolder = PlayerSkinDataHolder.CreatePlayerSkinData
                (characterPreFabs[currentCharacterBaseIndex], character.equippedMeshesByMeshCategory, character.equippedMeshModifiersByMeshModifierCategory);

            SaveAndLoadManager.Save<PlayerSkinDataHolder>(playerSkinDataHolder);
        }

        #region GUI:

        public void OnButtonClicked(ButtonBehaviour buttonBehaviour)
        {

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
        }     
        #endregion

    }
}