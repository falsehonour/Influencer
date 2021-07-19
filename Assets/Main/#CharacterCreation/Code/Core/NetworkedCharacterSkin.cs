using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace HashtagChampion
{
    namespace CharacterCreation
    {
        public class NetworkedCharacterSkin : NetworkBehaviour
        {

            [SerializeField] public Character character;
            private PlayerSkinDataHolder.Data serverCachedSkinData;

            [Command]
            private void Cmd_DownloadSkin(PlayerSkinDataHolder.Data skinData)
            {
                serverCachedSkinData = skinData;
                if (serverCachedSkinData.meshIndexes == null || serverCachedSkinData.meshModifierIndexes == null)
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
                characterTransform.localRotation = Quaternion.identity; //This used to be characterTransform.Rotation... Do not repeat this mistake...
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
                {
                    Animator animator = character.GetAnimator();
                    if (animator != null)
                    {
                        NetworkAnimator networkAnimator = GetComponent<NetworkAnimator>();
                        if (networkAnimator != null)
                        {
                            //NOTE: This move right here might be causing us bugs. This is explained in PlayerControllers's HandleMovement.
                            Animator placeHolderAnimator = networkAnimator.animator;
#if UNITY_EDITOR
                            //Animation Identity check:
                            {
                                string mismatchEntry = "Animator Paramenet mismatch: ";
                                AnimatorControllerParameter[] oldParams = placeHolderAnimator.parameters;
                                AnimatorControllerParameter[] newParams = animator.parameters;
                                if (oldParams.Length == newParams.Length)
                                {
                                    for (int i = 0; i < oldParams.Length; i++)
                                    {
                                        AnimatorControllerParameter oldParam = oldParams[i];
                                        AnimatorControllerParameter newParam = newParams[i];

                                        if (oldParam.name != newParam.name)
                                        {
                                            Debug.LogError(mismatchEntry + 
                                                oldParam.name + "!=" + newParam.name);
                                        }
                                        if (oldParam.type != newParam.type)
                                        {
                                            Debug.LogError(mismatchEntry + 
                                                oldParam.type.ToString() + "!=" + newParam.type.ToString());
                                        }
                                    }
                           
                                }
                           
                            }
#endif

                            networkAnimator.animator = animator;
                            Destroy(placeHolderAnimator);

                        }

                        Player player = GetComponent<Player>();
                        if (player != null)
                        {
                            player.SetAnimator(character.GetAnimator(), networkAnimator);
                        }
                        else
                        {
                            Debug.LogWarning("playerController == null");
                        }


                        //TODO: Mirror sets its animator vars on awake so this approach is not working and I don't wanna mess with Mirror's Animator at the moment

                        //if (hasAuthority)
                        /* {
                             NetworkAnimator networkAnimator = character.GetComponent<NetworkAnimator>();
                             if (networkAnimator != null)
                             {
                                 networkAnimator.animator = animator;
                                 networkAnimator.TryInitialise();
                             }
                             else
                             {
                                 Debug.LogWarning("networkAnimator == null");
                             }
                         }*/


                    }
                    else
                    {
                        Debug.LogWarning("animator == null");
                    }
                }
            }


            /*[Command]
            public void AssignNetworkAnimator(NetworkIdentity id)
            {
                NetworkAnimator networkAnimator = character.GetComponent<NetworkAnimator>();
                if (networkAnimator != null)
                {
                    networkAnimator.netIdentity.AssignClientAuthority(id.connectionToClient);
                }
                else
                {
                    Debug.LogWarning("networkAnimator == null");
                }
            }*/

            [Server]
            private bool ServerCachedSkinDataInitialised()
            {
                return (serverCachedSkinData.meshIndexes != null && serverCachedSkinData.meshModifierIndexes != null);
            }

            [Command(requiresAuthority = false)]
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
                    PlayerSkinDataHolder localSkinDataHolder =
                         SaveAndLoadManager.Load<PlayerSkinDataHolder>(new PlayerSkinDataHolder());
                    if (localSkinDataHolder == null)
                    {
                        //TODO: Getting a default skin should not be here
                        localSkinDataHolder = new PlayerSkinDataHolder(0, new byte[0], new byte[0]);
                        // PlayerSkinDataHolder.CreatePlayerSkinData(characterPreFab, character.equippedMeshesByMeshCategory, character.equippedMeshModifiersByMeshModifierCategory);
                    }

                    /*if (characterPreFab == null)
                    {
                        characterPreFab = CharacterCreationReferencer.References.GetCharacterPreFab(0);
                    }*/

                    PlayerSkinDataHolder.Data localSkinData = localSkinDataHolder.data;

                    Cmd_DownloadSkin(localSkinData);
                    EquipSkin(ref localSkinData);
                    //AssignNetworkAnimator(this.netIdentity);
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
}

