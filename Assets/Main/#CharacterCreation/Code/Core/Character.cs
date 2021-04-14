using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCreation
{
    public class Character : MonoBehaviour
    {

        [SerializeField] private Transform myTransform;
        [SerializeField] private Transform root;
        [SerializeField] private CharacterMesh[] equippedMeshesByMeshCategory;
        [SerializeField] private CharacterMeshModifier[] equippedMeshModifiersByMeshModifierCategory;
        private Renderer[] characterRenderersByMeshCategory;

        private Transform[] bones;
        [SerializeField] private Animator animator;
        private MaterialPropertyBlock materialPropertyBlock;
        private bool initialised = false;
        private void Start()
        {
            //Initialise();
        }

        public void Initialise()
        {
            if (initialised)
            {
                Debug.LogError("Cannot initialise more than once!");
                return;
            }
            initialised = true;
            materialPropertyBlock = new MaterialPropertyBlock();
            bones = root.GetComponentsInChildren<Transform>();
            InitialiseEquippedPieces();
        }

        private void InitialiseEquippedPieces()
        {
            int length;
            length = (int)MeshCategories.Length;
            equippedMeshesByMeshCategory = new CharacterMesh[length];
            characterRenderersByMeshCategory = new Renderer[length];

            length = (int)MeshModifierCategories.Length;
            equippedMeshModifiersByMeshModifierCategory = new CharacterMeshModifier[length];
        }

        #region Equip Methods:

        public void EquipCharacterPiece(CharacterPiece characterPiece)
        {
            Debug.Log("EquipCharacterPiece");
            if (!initialised)
            {
                Debug.LogError("Character has not initialised yet!");
            }

            if (characterPiece is CharacterMesh)
            {
                EquipMesh((CharacterMesh)characterPiece);
            }
            else if (characterPiece is CharacterMeshModifier)
            {
                EquipMeshModifier((CharacterMeshModifier)characterPiece);
            }
            else
            {
                Debug.LogError("Tried to equip a null CharacterPiece");
            }
        }

        private void EquipMesh(CharacterMesh mesh)
        {
            if (mesh == null)
            {
                Debug.LogError("The mesh being equipped is null!");
                return;
            }
            //int index = (int)mesh.Categories;
            MeshCategories meshCategories = mesh.Categories;
            if (meshCategories == 0)
            {
                Debug.LogError(mesh.name + " has no categories associated with it!");
                return;
            }
            if (bones == null)
            {
                Debug.LogError("bones == null");
            }

            Renderer renderer = null;
            if (mesh.Renderer != null)
            {
                renderer = Instantiate(mesh.Renderer);
                //Making sure the object has its correct name so that the animator recognises it in case it's animated
                string requiredName = CharacterCreationReferencer.NameRequirements.GetRequiredMeshName(mesh.Categories);
                if (requiredName != null)
                {
                    renderer.name = requiredName;
                }

                if (mesh is CharacterStaticMesh)
                {
                    CharacterStaticMesh characterStaticMesh = (CharacterStaticMesh)mesh;
                         
                    Transform parent = null;
                    foreach (Transform bone in bones)
                    {
                        if (bone.name == characterStaticMesh.ParentName)
                        {
                            parent = bone;
                            break;
                        }
                    }
                    if (parent == null)
                    {
                        Debug.LogError($"No object by the name {characterStaticMesh.ParentName} was found inside characterBones");
                        return;
                    }
                   
                    Transform meshRendererTransform = renderer.transform;
                    meshRendererTransform.SetParent(parent);
                    TransformProperties offset = characterStaticMesh.TransformOffset;
                    meshRendererTransform.localPosition = offset.position;
                    meshRendererTransform.localRotation = offset.rotation;
                    meshRendererTransform.localScale = offset.scale;
                         
                }
                else if (mesh is CharacterSkinnedMesh)
                {
                    CharacterSkinnedMesh characterSkinnedMesh = (CharacterSkinnedMesh)mesh;
                    SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)renderer;

                    Transform parent = myTransform;
                    skinnedMeshRenderer.transform.SetParent(parent);
                    Transform[] newBones = skinnedMeshRenderer.bones;

                    //Debug.Log("skinnedMesh bones:" + newBones.Length);
                    for (int i = 0; i < newBones.Length; i++)
                    {
                        string boneName = characterSkinnedMesh.Bones[i];

                        foreach (Transform bone in bones)
                        {
                            if (bone.name == boneName)
                            {
                                newBones[i] = bone;
                                //Debug.Log("Bone assigned");
                                break;
                            }
                        }

                        if (newBones[i] == null)
                        {
                            Debug.LogError("Failed to find a bone!");
                        }
                    }

                    skinnedMeshRenderer.bones = newBones;

                    foreach (Transform bone in bones)
                    {
                        if (bone.name == characterSkinnedMesh.RootName)
                        {
                            skinnedMeshRenderer.rootBone = bone;
                            //Debug.Log("rootBone assigned");
                            break;
                        }
                    }
                    
                }
                else
                {
                    Debug.LogError("The piece part is neither a CharacterSkinnedMesh nor a CharacterStaticMesh!");
                }
            }
            else
            {
                Debug.Log("Mesh is null, no mesh will be instantiated");
            }


            //Making sure the aanimator recognises the changes we make
            animator.Rebind();

            //Replace mesh:
            {
                int length = (int)MeshCategories.Length;

                for (int i = 0; i < length; i++)
                {

                    MeshCategories categoryIndex = (MeshCategories)(0b1 << i);
                    if ((meshCategories & categoryIndex) != 0)
                    {
                        if(equippedMeshesByMeshCategory[i] != null)
                        {
                            MeshCategories overlappingCategories =
                                ((equippedMeshesByMeshCategory[i].Categories ^ meshCategories) & equippedMeshesByMeshCategory[i].Categories);
                            for (int j = 0; j < length; j++)
                            {
                                if(equippedMeshesByMeshCategory[j] != null  && (equippedMeshesByMeshCategory[j].Categories & overlappingCategories) != 0)
                                {
                                    equippedMeshesByMeshCategory[j] = null;
                                }
                            }
                        }
                        equippedMeshesByMeshCategory[i] = mesh;

                        if (characterRenderersByMeshCategory[i] != null)
                        {
                            Destroy(characterRenderersByMeshCategory[i].gameObject);
                        }
                        characterRenderersByMeshCategory[i] = renderer;
                    }
                }
            }

            List<CharacterMeshModifier> compatibleModifiers = GetCompatableModifiers(mesh);
            string report = null;
            if (compatibleModifiers.Count == 0)
            {
                report = "No compatibleModifiers detected for the mesh.";
            }
            else
            {
                if (renderer == null)
                {
                    report = "compatibleModifiers detected but the renderer is null";
                }
                else
                {
                    report = "compatibleModifiers detected, applying to mesh";

                    for (int i = 0; i < compatibleModifiers.Count; i++)
                    {
                        ApplyModifierToRenderer(compatibleModifiers[i], renderer);
                    }
                }

            }
            Debug.Log(report);
        }

        private void EquipMeshModifier(CharacterMeshModifier modifier)
        {
            if (modifier.Categories == 0)
            {
                Debug.LogError("Modifier has no categories associated with it!");
                return;
            }
            List<Renderer> compatibleRenderers = GetCompatableRenderers(modifier);

            //Replace modifier:
            {
                MeshModifierCategories modifierCategories = modifier.Categories;
                int length = (int)MeshModifierCategories.Length;
                for (int i = 0; i < length; i++)
                {
                    MeshModifierCategories categoryIndex = (MeshModifierCategories)(0b1 << i);
                    if ((modifierCategories & categoryIndex) != 0)
                    {
                        equippedMeshModifiersByMeshModifierCategory[i] = modifier;
                    }
                }
            }

            string warning = null;
            if (compatibleRenderers.Count == 0)
            {
                warning = "No compatibleRenderers detected. Modifier was equipped without effecting existing meshes";
            }
            else
            {
                int modifiedMeshes = 0;
                for (int i = 0; i < compatibleRenderers.Count; i++)
                {
                    Renderer renderer = compatibleRenderers[i];
                    if (ApplyModifierToRenderer(modifier, renderer))
                    {
                        modifiedMeshes++;
                    }
                }

                if (modifiedMeshes == 0)
                {
                    warning = "No meshes were despite there being compatible renderers.";
                }
            }
            if (warning != null)
            {
                Debug.LogWarning(warning);
            }
        }

        private bool ApplyModifierToRenderer(CharacterMeshModifier modifier, Renderer renderer)
        {
            bool succeeded = false;
            if (modifier is CharacterTextures)
            {
                CharacterTextures characterTextures = (CharacterTextures)modifier;
                CharacterTextures.MaterialTexture[] materialTextures = characterTextures.MaterialTextures;

                for (int j = 0; j < materialTextures.Length; j++)
                {
                    CharacterTextures.MaterialTexture materialTexture = materialTextures[j];
                    int materialIndex = materialTexture.materialIndex;
                    renderer.GetPropertyBlock(materialPropertyBlock, materialIndex);
                    Texture texture = materialTexture.texture2D != null ? materialTexture.texture2D : Texture2D.whiteTexture;
                    materialPropertyBlock.SetTexture("_MainTex", texture);
                    materialPropertyBlock.SetColor("_Color", materialTexture.colour);
                    renderer.SetPropertyBlock(materialPropertyBlock, materialIndex);
                }
                succeeded = true;

            }
            else if (modifier is CharacterMorph)
            {
                CharacterMorph morph = (CharacterMorph)modifier;
                if (renderer is SkinnedMeshRenderer)
                {
                    SkinnedMeshRenderer morphedSkinnedMesh = (SkinnedMeshRenderer)renderer;
                    Mesh sharedMesh = morphedSkinnedMesh.sharedMesh;
                    for (int j = 0; j < morph.MorphBlendShapes.Length; j++)
                    {
                        CharacterMorph.MorphBlendShape morphBlendShape = morph.MorphBlendShapes[j];
                        int blendShapeIndex = sharedMesh.GetBlendShapeIndex(morphBlendShape.Name);
                        morphedSkinnedMesh.SetBlendShapeWeight(blendShapeIndex, morphBlendShape.Weight);
                    }
                    succeeded = true;
                }
                else
                {
                    Debug.LogError("Tried using a morph on a non-SkinnedMeshRenderer");
                }

            }
            return succeeded;
        }
        #endregion

        #region Compatibility:
        private List<Renderer> GetCompatableRenderers(CharacterMeshModifier modifier)
        {
            List<Renderer> compatibleRenderers = new List<Renderer>();

            CharacterMesh[] modifierCompatibleMeshes = modifier.CompatibleMeshes;
            if (modifierCompatibleMeshes == null || modifierCompatibleMeshes.Length == 0)
            {
                Debug.LogWarning("compatibleRenderers is empty or nonexistent. Resorting to check compatability by categories.");
                MeshCategories compatibleMeshCategories = CategoriesCompatability.GetCompatableMeshCategories(modifier.Categories);
                for (int i = 0; i < equippedMeshesByMeshCategory.Length; i++)
                {
                    if (equippedMeshesByMeshCategory[i] != null)
                    {
                        if ((equippedMeshesByMeshCategory[i].Categories & compatibleMeshCategories) != 0)
                        {
                            compatibleRenderers.Add(characterRenderersByMeshCategory[i]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < modifierCompatibleMeshes.Length; i++)
                {
                    for (int j = 0; j < equippedMeshesByMeshCategory.Length; j++)
                    {
                        if (equippedMeshesByMeshCategory[j] != null)
                        {
                            if (equippedMeshesByMeshCategory[j] == modifierCompatibleMeshes[i])
                            {
                                compatibleRenderers.Add(characterRenderersByMeshCategory[j]);
                            }
                        }
                    }
                }
            }

            return compatibleRenderers;
        }

        private List<CharacterMeshModifier> GetCompatableModifiers(CharacterMesh characterMesh)
        {
            List<CharacterMeshModifier> compatibleModifiers = new List<CharacterMeshModifier>();

            MeshModifierCategories compatibleModifierCategories =
                  CategoriesCompatability.GetCompatableModifierCategories(characterMesh.Categories);
            for (int i = 0; i < equippedMeshModifiersByMeshModifierCategory.Length; i++)
            {
                CharacterMeshModifier modifier = equippedMeshModifiersByMeshModifierCategory[i];
                if (modifier != null)
                {
                    bool modifierIsCompatible = false;
                    CharacterMesh[] modifierCompatibleMeshes = modifier.CompatibleMeshes;
                    if (modifierCompatibleMeshes == null || modifierCompatibleMeshes.Length == 0)
                    {
                        Debug.LogWarning("compatibleRenderers is empty or nonexistent. Resorting to check compatability by categories.");
                        //MeshCategories compatibleMeshCategories = CategoriesCompatability.GetCompatableMeshCategories(modifier.Categories);

                        if ((modifier.Categories & compatibleModifierCategories) != 0)
                        {
                            compatibleModifiers.Add(modifier);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < modifierCompatibleMeshes.Length; j++)
                        {
                            if (modifierCompatibleMeshes[j] == characterMesh)
                            {
                                modifierIsCompatible = true;
                                break;
                            }
                        }
                    }

                    if (modifierIsCompatible)
                    {
                        compatibleModifiers.Add(modifier);
                    }
                }
            }

            return compatibleModifiers;
        }


        /*private static bool IsCompatible(CharacterMeshModifier modifier,CharacterMesh mesh)
        {
            CharacterMesh[] modifierCompatibleMeshes = modifier.CompatibleMeshes;
            if (modifierCompatibleMeshes == null || modifierCompatibleMeshes.Length == 0)
            {
                Debug.LogWarning("compatibleRenderers is empty or nonexistent. Resorting to check compatability by categories.");
                MeshCategories compatibleMeshCategories = CategoriesCompatability.GetCompatableMeshCategories(modifier.Categories);
                for (int i = 0; i < equippedCharacterMeshes.Length; i++)
                {
                    if (equippedCharacterMeshes[i] != null)
                    {
                        if ((equippedCharacterMeshes[i].Categories & compatibleMeshCategories) != 0)
                        {
                            compatibleRenderers.Add(renderersByMeshCategory[i]);
                        }
                    }
                }
            }
            else
            {

                for (int i = 0; i < modifierCompatibleMeshes.Length; i++)
                {
                    for (int j = 0; j < equippedCharacterMeshes.Length; j++)
                    {
                        if (equippedCharacterMeshes[j] != null)
                        {
                            if (equippedCharacterMeshes[j] == modifierCompatibleMeshes[i])
                            {
                                compatibleRenderers.Add(renderersByMeshCategory[j]);
                            }
                        }
                    }
                }
            }
        }*/
        #endregion
    }
}

