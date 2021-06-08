namespace CharacterCreation
{
    public static class CategoriesCompatability
    {
        public static MeshCategories GetCompatableMeshCategories(MeshModifierCategories modifierCategories)
        {
            MeshCategories meshCategories = 0;
            for (int i = 0; i < categoriesCompatabilityBlocks.Length; i++)
            {
                CompatabilityBlock compatabilityBlock = categoriesCompatabilityBlocks[i];
                if ((modifierCategories & compatabilityBlock.ModifierCategories) != 0)
                {
                    meshCategories |= compatabilityBlock.MeshCategories;
                }
            }

            return meshCategories;
        }

        public static MeshModifierCategories GetCompatableModifierCategories(MeshCategories meshCategories)
        {
            MeshModifierCategories modifierCategories = 0;
            for (int i = 0; i < categoriesCompatabilityBlocks.Length; i++)
            {
                CompatabilityBlock compatabilityBlock = categoriesCompatabilityBlocks[i];
                if ((meshCategories & compatabilityBlock.MeshCategories) != 0)
                {
                    modifierCategories |= compatabilityBlock.ModifierCategories;
                }
            }

            return modifierCategories;
        }

        private struct CompatabilityBlock
        {
            private readonly MeshModifierCategories modifierCategories;
            private readonly MeshCategories meshCategories;
            public MeshModifierCategories ModifierCategories => modifierCategories;
            public MeshCategories MeshCategories => meshCategories;

            public CompatabilityBlock(MeshModifierCategories modifierCategories, MeshCategories meshCategories)
            {
                this.modifierCategories = modifierCategories;
                this.meshCategories = meshCategories;
            }
        }

        private static readonly CompatabilityBlock[] categoriesCompatabilityBlocks =
        {
           new CompatabilityBlock(MeshModifierCategories.FaceMorph, MeshCategories.Body),
           new CompatabilityBlock(MeshModifierCategories.NoseMorph, MeshCategories.Body),
           new CompatabilityBlock(MeshModifierCategories.MouthMorph, MeshCategories.Body),
           new CompatabilityBlock(MeshModifierCategories.HairTextures, MeshCategories.Hair),
           new CompatabilityBlock(MeshModifierCategories.SkinColourTextures, MeshCategories.Body),
           new CompatabilityBlock(MeshModifierCategories.TorsoTextures, MeshCategories.Torso),
           new CompatabilityBlock(MeshModifierCategories.FeetTextures, MeshCategories.Feet),
           new CompatabilityBlock(MeshModifierCategories.EyebrowsTextures, MeshCategories.Eyebrows),
           new CompatabilityBlock(MeshModifierCategories.EyebrowsMorph, MeshCategories.Eyebrows),
           new CompatabilityBlock(MeshModifierCategories.LegsTextures, MeshCategories.Legs),
           new CompatabilityBlock(MeshModifierCategories.EyeColourTextures, MeshCategories.Body),

        };
    }
}
