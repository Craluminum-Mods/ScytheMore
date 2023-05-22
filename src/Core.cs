using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

[assembly: ModInfo("Scythe More")]

namespace ScytheMore
{
    class Core : ModSystem
    {
        readonly List<string> DisallowedParts = new() { "bush", "cactus", "clipping", "glowworms", "hay", "hotspringbacteria", "mushroom", "roofing", "sapling", "seedling", "vine", "waterlily", };
        readonly List<string> DisallowedSuffixes = new() { "empty", "flowering", "harvested-free", "harvested", };

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Server;
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            var newPrefixes = new List<string>();

            foreach (var block in api.World.Blocks)
            {
                if (IsAllowed(block))
                {
                    UpdatePrefixes(ref newPrefixes, block);
                }
            }

            foreach (var item in api.World.Items)
            {
                if (item is ItemScythe)
                {
                    ChangeAttributes(newPrefixes, DisallowedSuffixes, item);
                }
            }

            api.Logger.Event("started 'Scythe More' mod");
        }

        private static void ChangeAttributes(List<string> newPrefixes, List<string> newSuffixes, Item item)
        {
            item.Attributes ??= new JsonObject(new JObject());

            var codePrefixes = item.Attributes["codePrefixes"].AsObject<List<string>>();
            var disallowedSuffixes = item.Attributes["disallowedSuffixes"].AsObject<List<string>>();

            if (codePrefixes.Count != 0)
            {
                for (int i = 0; i < newPrefixes.Count; i++)
                {
                    if (!codePrefixes.Contains(newPrefixes[i]))
                    {
                        codePrefixes.Add(newPrefixes[i]);
                    }
                }
                item.Attributes.Token["codePrefixes"] = JToken.FromObject(codePrefixes);
            }

            if (disallowedSuffixes.Count != 0)
            {
                for (int i = 0; i < newSuffixes.Count; i++)
                {
                    if (!disallowedSuffixes.Contains(newSuffixes[i]))
                    {
                        disallowedSuffixes.Add(newSuffixes[i]);
                    }
                }
                item.Attributes.Token["disallowedSuffixes"] = JToken.FromObject(disallowedSuffixes);
            }
        }

        private static void UpdatePrefixes(ref List<string> prefixes, Block block)
        {
            if (!prefixes.Contains(block.Code.FirstCodePart()))
            {
                prefixes.Add(block.Code.FirstCodePart());
            }
        }

        private bool IsAllowed(Block block)
        {
            if (block.BlockMaterial != EnumBlockMaterial.Plant)
            {
                return false;
            }

            for (int i = 0; i < DisallowedParts.Count; i++)
            {
                if (block.Code.ToString().Contains(DisallowedParts[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}