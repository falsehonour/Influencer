using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CharacterCreation
{
    public class NetworkedCharacterSkin : NetworkBehaviour
    {

        [SerializeField] private Character character;
        private PlayerSkinDataHolder.Data serverCachedSkinData;

        [Command]
        private void Cmd_DownloadSkin(PlayerSkinDataHolder.Data skinData)
        {
            serverCachedSkinData = skinData;
            if(serverCachedSkinData.meshIndexes == null || serverCachedSkinData.meshModifierIndexes == null)
            {
                Debug.LogError("skinData.meshIndexes == null || skinData.meshModifierIndexes == null");
            }
        }

        private void EquipSkin(ref PlayerSkinDataHolder.Data skinData)
        {
            if (character != null)
            {
                Destroy(character.gameObject);
            }
            Character preFab = CharacterCreationReferencer.References.GetCharacterPreFab(skinData.characterPrefabIndex);
            character = Instantiate(preFab);
            Transform characterTransform = character.transform;
            characterTransform.SetParent(this.transform);
            characterTransform.localPosition = Vector3.zero;
            characterTransform.rotation = Quaternion.identity;
            character.Initialise();
            
            for (int i = 0; i < skinData.meshIndexes.Length; i++)
            {
                character.EquipCharacterPiece(CharacterCreationReferencer.References.GetCharacterMesh(skinData.meshIndexes[i]));
            }
            for (int i = 0; i < skinData.meshModifierIndexes.Length; i++)
            {
                character.EquipCharacterPiece(CharacterCreationReferencer.References.GetCharacterMeshModifier(skinData.meshModifierIndexes[i]));
            }
            character.TryEquipFallbackPieces();

            //Animator references:
            if(false)
            {
                Animator animator = character.GetAnimator();
                if(animator != null)
                {
                    PlayerController playerController = GetComponent<PlayerController>();
                    if(playerController != null)
                    {
                        playerController.SetAnimator(character.GetAnimator());
                    }
                    NetworkAnimator networkAnimator = GetComponent<NetworkAnimator>();
                    if (networkAnimator != null)
                    {
                        networkAnimator.animator = animator;
                    }
                }
            }
        }

        [Server]
        private bool ServerCachedSkinDataInitialised()
        {
            return (serverCachedSkinData.meshIndexes != null && serverCachedSkinData.meshModifierIndexes != null);
        }

        [Command(ignoreAuthority = true)]
        private void Cmd_SendSkin(NetworkConnectionToClient conn = null)
        {
            StartCoroutine(WaitForSkin(conn));    
        }

        [Server]
        private IEnumerator WaitForSkin(NetworkConnectionToClient conn)
        {
            while (!ServerCachedSkinDataInitialised())
            {
                Debug.Log("Waiting for skin to be uploaded...");
                yield return new WaitForSeconds(0.25f);
            }
            TargetRpc_EquipSkin(conn, serverCachedSkinData/*, new byte[16]*/);
            
        }

        [TargetRpc]
        private void TargetRpc_EquipSkin(NetworkConnection target, PlayerSkinDataHolder.Data skinData)
        {
           // Debug.LogError("testArray length: " + testArray.Length);

            if (skinData.meshIndexes == null || skinData.meshModifierIndexes == null)
            {
                Debug.LogError("skinData.meshIndexes == null || skinData.meshModifierIndexes == null");
            }
            EquipSkin(ref skinData);
        }

        [Client]
        public void Initialise()
        {
            if (isLocalPlayer)
            {
                PlayerSkinDataHolder.Data localSkinData = 
                    SaveAndLoadManager.Load<PlayerSkinDataHolder>(new PlayerSkinDataHolder()).data;
                Cmd_DownloadSkin(localSkinData);
                EquipSkin(ref localSkinData);
            }
            else
            {
                /*Debug.Log("Initialise !isLocalPlayer");
                // Cmd_SendSkin(this.netIdentity);*/
                Cmd_SendSkin();

            }
        }

    }
}
