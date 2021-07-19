using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HashtagChampion
{
    namespace CharacterCreation
    {
        public class Character : MonoBehaviour
        {
            [SerializeField] private CharacterBaseProperties baseProperties;
            public CharacterBaseProperties BaseProperties => baseProperties;
            [SerializeField] private Transform myTransform;
            [SerializeField] private Transform root;
            [SerializeField] private Animator animator;

            public CharacterMesh[] equippedMeshesByMeshCategory;
            public CharacterMeshMod[] equippedMeshModifiersByMeshModifierCategory;
            private Renderer[] characterRenderersByMeshCategory;

            private Transform[] bones;
            private MaterialPropertyBlock materialPropertyBlock;
            private bool initialised = false;

            [SerializeField] private GameObject gun;
            [SerializeField] private GameObject[] wings;
            [SerializeField] private GameObject banana;

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

                ShowGun(false);
                ShowWings(false);
                ShowBanana(false);

            }

            private void InitialiseEquippedPieces()
            {
                int length;
                length = (int)MeshCategories.Length;
                equippedMeshesByMeshCategory = new CharacterMesh[length];
                characterRenderersByMeshCategory = new Renderer[length];

                length = (int)MeshModifierCategories.Length;
                equippedMeshModifiersByMeshModifierCategory = new CharacterMeshMod[length];
            }

            #region Equip Methods:

            public void EquipCharacterPiece(CharacterPiece characterPiece)
            {

                if (!initialised)
                {
                    Debug.LogError("Character has not initialised yet!");
                }

                if (characterPiece is CharacterMesh)
                {
                    EquipMesh((CharacterMesh)characterPiece);
                    RemoveUnusedModifiers();
                }
                else if (characterPiece is CharacterMeshMod)
                {
                    TryEquipMeshModifier((CharacterMeshMod)characterPiece);
                }
                else
                {
                    Debug.LogError("Tried to equip a null CharacterPiece");
                }
            }

            public void TryEquipFallbackPieces()
            {
                //Meshes:
                {
                    CharacterMesh[] fallbackMeshes = baseProperties.fallbackMeshes;

                    MeshCategories equippedMeshCategories = 0;

                    for (int i = 0; i < equippedMeshesByMeshCategory.Length; i++)
                    {
                        if (equippedMeshesByMeshCategory[i] != null)
                        {
                            equippedMeshCategories |= (MeshCategories)(0b1 << i);
                        }
                    }
                    for (int i = 0; i < fallbackMeshes.Length; i++)
                    {
                        MeshCategories fallbackMeshCategories = fallbackMeshes[i].Categories;

                        //if (~(equippedMeshCategories ^ fallbackMeshCategories) == 0)
                        if ((equippedMeshCategories & fallbackMeshCategories) == 0)
                        {
                            EquipMesh(fallbackMeshes[i]);
                            equippedMeshCategories |= fallbackMeshCategories;
                        }
                    }

                    //Empty category check
                    {
                        string missingCategories = "";
                        for (int i = 0; i < (int)MeshCategories.Length; i++)
                        {
                            MeshCategories category = ((MeshCategories)(0b1 << i));
                            if ((equippedMeshCategories & category) == 0)
                            {
                                missingCategories += category.ToString() + ", ";
                            }
                        }
                        if (missingCategories != "")
                        {
                            Debug.LogWarning("Missing mesh categories post-TryEquipFallbackPieces: " + missingCategories);
                        }

                    }
                }

                //Modifiers:
                {
                    CharacterMeshMod[] fallbackModifiers = baseProperties.fallbackMeshModifiers;

                    MeshModifierCategories equippedModifierCategories = 0;

                    for (int i = 0; i < equippedMeshModifiersByMeshModifierCategory.Length; i++)
                    {
                        if (equippedMeshModifiersByMeshModifierCategory[i] != null)
                        {
                            equippedModifierCategories |= (MeshModifierCategories)(0b1 << i);
                        }
                    }
                    for (int i = 0; i < fallbackModifiers.Length; i++)
                    {
                        MeshModifierCategories fallbackCategories = fallbackModifiers[i].Categories;

                        if ((equippedModifierCategories & fallbackCategories) == 0)
                        {
                            if (TryEquipMeshModifier(fallbackModifiers[i]))
                            {
                                equippedModifierCategories |= fallbackCategories;
                            }
                        }
                    }


                    //Empty category check
                    {
                        string missingCategories = "";
                        for (int i = 0; i < (int)MeshModifierCategories.Length; i++)
                        {
                            MeshModifierCategories category = ((MeshModifierCategories)(0b1 << i));
                            if ((equippedModifierCategories & category) == 0)
                            {
                                missingCategories += category.ToString() + ", ";
                            }
                        }
                        if (missingCategories != "")
                        {
                            Debug.LogWarning("Missing modifier categories post-TryEquipFallbackPieces: " + missingCategories);
                        }

                    }
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
                            if (equippedMeshesByMeshCategory[i] != null)
                            {
                                //Wouldn't it be simpler to do a for loop on equippedMeshesByMeshCategory 
                                //and nullify any equippedMeshesByMeshCategory[i] we find ?
                                MeshCategories overlappingCategories =
                                    ((equippedMeshesByMeshCategory[i].Categories ^ meshCategories) & equippedMeshesByMeshCategory[i].Categories);
                                for (int j = 0; j < length; j++)
                                {
                                    if (equippedMeshesByMeshCategory[j] != null && (equippedMeshesByMeshCategory[j].Categories & overlappingCategories) != 0)
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

                ApplyCompatibleModifiers(mesh, renderer);
            }

            private void ApplyCompatibleModifiers(CharacterMesh mesh, Renderer renderer)
            {

                List<CharacterMeshMod> compatibleModifiers = GetCompatibleEquippedModifiers(mesh);
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

            private void RemoveUnusedModifiers()
            {
                for (int i = 0; i < equippedMeshModifiersByMeshModifierCategory.Length; i++)
                {
                    CharacterMeshMod modifier = equippedMeshModifiersByMeshModifierCategory[i];
                    if (modifier != null)
                    {
                        if (!HasCompatibleEquippedMeshes(modifier))
                        {
                            Debug.LogWarning("Removing a modifier that has no compatible meshes");
                            equippedMeshModifiersByMeshModifierCategory[i] = null;
                        }
                    }

                }
            }

            private bool TryEquipMeshModifier(CharacterMeshMod modifier)
            {
                if (modifier.Categories == 0)
                {
                    Debug.LogError("Modifier has no categories associated with it!");
                    return false;
                }
                List<Renderer> compatibleRenderers = GetCompatableRenderers(modifier);
                int modifiedMeshes = 0;

                string warning = null;
                if (compatibleRenderers.Count == 0)
                {
                    warning = $"No compatibleRenderers detected for {modifier.name}.";
                }
                else
                {
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
                        warning = $"No meshes were found despite there being compatible renderers for {modifier.name}.";
                    }
                }
                if (warning != null)
                {
                    Debug.LogWarning(warning);
                }

                //Replace modifier:
                if (modifiedMeshes > 0)
                {
                    MeshModifierCategories modifierCategories = modifier.Categories;
                    int length = (int)MeshModifierCategories.Length;
                    for (int i = 0; i < length; i++)
                    {
                        MeshModifierCategories categoryIndex = (MeshModifierCategories)(0b1 << i);
                        if ((modifierCategories & categoryIndex) != 0)
                        {
                            //Wouldn't it be simpler to do a for loop on equippedMeshesByMeshCategory 
                            //and nullify any equippedMeshesByMeshCategory[i] we find ?
                            if (equippedMeshModifiersByMeshModifierCategory[i] != null)
                            {
                                MeshModifierCategories overlappingCategories =
                                   ((equippedMeshModifiersByMeshModifierCategory[i].Categories ^ modifierCategories)
                                     & equippedMeshModifiersByMeshModifierCategory[i].Categories);
                                for (int j = 0; j < length; j++)
                                {
                                    if (equippedMeshModifiersByMeshModifierCategory[j] != null &&
                                        (equippedMeshModifiersByMeshModifierCategory[j].Categories & overlappingCategories) != 0)
                                    {
                                        equippedMeshModifiersByMeshModifierCategory[j] = null;
                                    }
                                }
                            }

                            equippedMeshModifiersByMeshModifierCategory[i] = modifier;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private bool ApplyModifierToRenderer(CharacterMeshMod mod, Renderer renderer)
            {
                bool succeeded = false;

                if (mod is CharacterMatsMod)
                {
                    CharacterMatsMod matsMod = (CharacterMatsMod)mod;
                    {
                        CharacterMatsMod.ColourMod[] colourMods = matsMod.ColourMods;
                        if (colourMods != null && colourMods.Length > 0)
                        {
                            for (int i = 0; i < colourMods.Length; i++)
                            {
                                ref CharacterMatsMod.ColourMod colourMod = ref colourMods[i];
                                int matIndex = colourMod.matIndex;
                                //NOTE: renderer.Materials.Length causes an instance of a modified material to be created!
                                if (matIndex < 0 || matIndex >= renderer.sharedMaterials.Length)
                                {
                                    Debug.LogError("Illegal material index!");
                                    continue;
                                }
                                renderer.GetPropertyBlock(materialPropertyBlock, matIndex);
                                materialPropertyBlock.SetColor(colourMod.colourName.ToString(), colourMod.colour);
                                renderer.SetPropertyBlock(materialPropertyBlock, matIndex);
                            }
                        }
                    }
                    {
                        CharacterMatsMod.TextureMod[] textureMods = matsMod.TextureMods;
                        if (textureMods != null && textureMods.Length > 0)
                        {
                            for (int i = 0; i < textureMods.Length; i++)
                            {
                                ref CharacterMatsMod.TextureMod textureMod = ref textureMods[i];
                                int matIndex = textureMod.matIndex;
                                //NOTE: renderer.Materials.Length causes an instance of a modified material to be created!
                                if (matIndex < 0 || matIndex >= renderer.sharedMaterials.Length)
                                {
                                    Debug.LogError("Illegal material index!");
                                    continue;
                                }
                                Texture texture = textureMod.texture != null ? textureMod.texture : Texture2D.whiteTexture;
                                renderer.GetPropertyBlock(materialPropertyBlock, matIndex);
                                materialPropertyBlock.SetTexture(textureMod.textureName.ToString(), texture);
                                renderer.SetPropertyBlock(materialPropertyBlock, matIndex);
                                // Debug.Log("SET TEXTURE");
                            }
                        }
                    }

                    succeeded = true;

                }
                else if (mod is CharacterMorph)
                {
                    CharacterMorph morph = (CharacterMorph)mod;
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

            private bool HasCompatibleEquippedMeshes(CharacterMeshMod modifier)
            {
                List<Renderer> compatibleRenderers = new List<Renderer>();

                CharacterMesh[] modifierCompatibleMeshes = modifier.CompatibleMeshes;
                if (modifierCompatibleMeshes == null || modifierCompatibleMeshes.Length == 0)
                {
                    MeshCategories compatibleMeshCategories = CategoriesCompatability.GetCompatableMeshCategories(modifier.Categories);
                    for (int i = 0; i < equippedMeshesByMeshCategory.Length; i++)
                    {
                        if (equippedMeshesByMeshCategory[i] != null)
                        {
                            if ((equippedMeshesByMeshCategory[i].Categories & compatibleMeshCategories) != 0)
                            {
                                return true;
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
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }

            private List<Renderer> GetCompatableRenderers(CharacterMeshMod modifier)
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
                            if ((equippedMeshesByMeshCategory[i].Categories & compatibleMeshCategories) != 0 && characterRenderersByMeshCategory[i] != null)
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
                                if (equippedMeshesByMeshCategory[j] == modifierCompatibleMeshes[i] && characterRenderersByMeshCategory[i] != null)
                                {
                                    compatibleRenderers.Add(characterRenderersByMeshCategory[j]);
                                }
                            }
                        }
                    }
                }

                //Cleanup
                {
                    //Debug.Log("Pre-clenup compatibleRenderers.Count:" + compatibleRenderers.Count);
                    for (int i = 0; i < compatibleRenderers.Count - 1; i++)
                    {
                        for (int j = i + 1; j < compatibleRenderers.Count;)
                        {
                            if (compatibleRenderers[i] == compatibleRenderers[j])
                            {
                                compatibleRenderers.RemoveAt(j);
                            }
                            else
                            {
                                j++;
                            }
                        }
                    }
                    // Debug.Log("Post-clenup compatibleRenderers.Count:" + compatibleRenderers.Count);
                }

                return compatibleRenderers;
            }

            private List<CharacterMeshMod> GetCompatibleEquippedModifiers(CharacterMesh characterMesh)
            {
                List<CharacterMeshMod> compatibleModifiers = new List<CharacterMeshMod>();

                MeshModifierCategories compatibleModifierCategories =
                      CategoriesCompatability.GetCompatableModifierCategories(characterMesh.Categories);
                for (int i = 0; i < equippedMeshModifiersByMeshModifierCategory.Length; i++)
                {
                    CharacterMeshMod modifier = equippedMeshModifiersByMeshModifierCategory[i];
                    if (modifier != null)
                    {
                        bool modifierIsCompatible = false;
                        CharacterMesh[] modifierCompatibleMeshes = modifier.CompatibleMeshes;
                        if (modifierCompatibleMeshes == null || modifierCompatibleMeshes.Length == 0)
                        {
                            //Debug.LogWarning("compatibleRenderers is empty or nonexistent. Resorting to check compatability by categories.");
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

                        //TODO: We can check if the modifier exists in the list here instead of cleanin up afterwards
                        if (modifierIsCompatible)
                        {
                            compatibleModifiers.Add(modifier);
                        }
                    }
                }

                //Cleanup
                {
                    //Debug.Log("Pre-clenup compatibleModifiers.Count:" + compatibleModifiers.Count);
                    for (int i = 0; i < compatibleModifiers.Count - 1; i++)
                    {
                        for (int j = i + 1; j < compatibleModifiers.Count;)
                        {
                            if (compatibleModifiers[i] == compatibleModifiers[j])
                            {
                                compatibleModifiers.RemoveAt(j);
                            }
                            else
                            {
                                j++;
                            }
                        }
                    }
                    // Debug.Log("Post-clenup compatibleModifiers.Count:" + compatibleModifiers.Count);
                }

                return compatibleModifiers;
            }

            public Animator GetAnimator()
            {
                return animator;
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

            public void ShowGun(bool value)
            {
                if (gun != null && gun.activeSelf != value)
                {
                    gun.SetActive(value);
                }
            }

            public void ShowBanana(bool value)
            {
                if (banana != null && banana.activeSelf != value)
                {
                    banana.SetActive(value);
                }
            }

            public void ShowWings(bool value)
            {
                if (wings != null)
                {
                    for (int i = 0; i < wings.Length; i++)
                    {
                        wings[i].SetActive(value);
                    }
                }
            }

            //  public void HiseWings()
        }
    }
}
