using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace CharacterCreation
{
    public class NetworkedCharacterSkin : NetworkBehaviour
    {

        [SerializeField] private CharacterMesh[] initialMeshes;
        [SerializeField] private CharacterMeshModifier[] initialMeshModifiers;
        [SerializeField] private Character character;

        private void BroadcastSkin()
        {
            byte[] indexes = new byte[initialMeshes.Length];
            for (int i = 0; i < initialMeshes.Length; i++)
            {
                CharacterMesh mesh = initialMeshes[i];
                // byte? meshIndex = CharacterPieceReferences.GetCharacterMeshIndex(mesh);
                byte? meshIndex = CharacterCreationReferencer.References.GetCharacterMeshIndex(mesh);
                if (meshIndex != null)
                {
                    indexes[i] = (byte)meshIndex;
                }
                // string path = mesh.name;//  AssetDatabase.GetAssetPath(mesh); //; PrefabUtility.path(initialMeshes[0]);
                //paths[i] = path;
            }

            Cmd_BroadcastSkin(indexes);
        }

        [Command]
        private void Cmd_BroadcastSkin(byte[] meshesIndexes)
        {
            Rpc_EquipSkin(meshesIndexes);
        }

        [ClientRpc]
        private void Rpc_EquipSkin(byte[] meshesIndexes)
        {
            Debug.LogError("Not implemented!");
           /* initialMeshes = new CharacterMesh[meshesIndexes.Length];
            for (int i = 0; i < meshesIndexes.Length; i++)
            {
               // CharacterMesh mesh = CharacterPieceReferences.GetCharacterMesh(meshesIndexes[i]);
                CharacterMesh mesh = CharacterCreationReferencer.PieceRefferences.GetCharacterMesh(meshesIndexes[i]);

                initialMeshes[i] = mesh;
            }

            for (int i = 0; i < initialMeshes.Length; i++)
            {
                character.EquipCharacterPiece(initialMeshes[i]);
            }*/
        }

        private void Update()
        {
            //if (isLocalPlayer)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    BroadcastSkin();
                }
            }
        }

        private void Start()
        {
            character.Initialise();
        }
        /*private void RebuildCharacterFromEquippedPieces()
        {
            for (int i = 0; i < equippedMeshesByMeshCategory.Length; i++)
            {
                if (equippedMeshesByMeshCategory[i] != null)
                {
                    EquipCharacterPiece(equippedMeshesByMeshCategory[i]);
                }
            }
            for (int i = 0; i < equippedMeshModifiersByMeshModifierCategory.Length; i++)
            {
                if (equippedMeshModifiersByMeshModifierCategory[i] != null)
                {
                    EquipCharacterPiece(equippedMeshModifiersByMeshModifierCategory[i]);
                }
            }
        }*/

    }
}
