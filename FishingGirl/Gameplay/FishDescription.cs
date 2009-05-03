using System;

using FishingGirl.Properties;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// The size of a fish.
    /// </summary>
    public enum FishSize
    {
        Small,
        Medium,
        Large,
        VeryLarge
    }

    /// <summary>
    /// The rarity of a fish.
    /// </summary>
    public enum FishRarity
    {
        Common,
        Uncommon,
        Rare,
        VeryRare
    }

    /// <summary>
    /// The modifier applied to the description.
    /// </summary>
    public enum FishModifier
    {
        Typical,
        Fat,
        Thin,
        Ancient,
        Young,
        Beautiful,
        Ugly
    }

    /// <summary>
    /// Describes a fish.
    /// </summary>
    public class FishDescription
    {
        /// <summary>
        /// The size of the fish.
        /// </summary>
        public readonly FishSize Size;

        /// <summary>
        /// The rarity of the fish.
        /// </summary>
        public readonly FishRarity Rarity;

        /// <summary>
        /// The description modifier applied to the fish.
        /// </summary>
        public readonly FishModifier Modifier;

        /// <summary>
        /// The localized name of the fish.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The monetary value of the fish.
        /// </summary>
        public readonly int Value;

        /// <summary>
        /// The name of the sprite descriptor for the fish.
        /// </summary>
        public readonly string DescriptorName;

        /// <summary>
        /// Creates a new fish description.
        /// </summary>
        public FishDescription(FishSize size, FishRarity rarity, FishModifier modifier)
        {
            Size = size;
            Rarity = rarity;
            Modifier = modifier;
            Value = GetValue(size, rarity, modifier);
            Name = GetName(size, rarity, modifier);
            DescriptorName = GetDescriptorName(size, rarity);
        }

        /// <summary>
        /// Returns the localized name of the fish for the given type.
        /// </summary>
        private string GetName(FishSize size, FishRarity rarity, FishModifier modifier)
        {
            return String.Format(Resources.FishName,
                Resources.ResourceManager.GetString(modifier.ToString(), Resources.Culture),
                Resources.ResourceManager.GetString(size.ToString() + rarity.ToString(), Resources.Culture),
                Resources.ResourceManager.GetString(rarity.ToString(), Resources.Culture));
        }

        /// <summary>
        /// Returns the value of the fish for the given type.
        /// </summary>
        private int GetValue(FishSize size, FishRarity rarity, FishModifier modifier)
        {
            float sizeModifier = 1f;
            switch (size)
            {
                case FishSize.Small: sizeModifier = 1f; break;
                case FishSize.Medium: sizeModifier = 3f; break;
                case FishSize.Large: sizeModifier = 5f; break;
                case FishSize.VeryLarge: sizeModifier = 7f; break;
            }

            float rarityModifier = 1f;
            switch (rarity)
            {
                case FishRarity.Common: rarityModifier = 1f; break;
                case FishRarity.Uncommon: rarityModifier = 2f; break;
                case FishRarity.Rare: rarityModifier = 3f; break;
                case FishRarity.VeryRare: rarityModifier = 4f; break;
            }

            float modModifier = 1f;
            switch (modifier)
            {
                case FishModifier.Typical: modModifier = 1f; break;
                case FishModifier.Young: modModifier = 0.75f; break;
                case FishModifier.Ancient: modModifier = 1.5f; break;
                case FishModifier.Ugly: modModifier = 0.75f; break;
                case FishModifier.Beautiful: modModifier = 1.5f; break;
                case FishModifier.Thin: modModifier = 0.75f; break;
                case FishModifier.Fat: modModifier = 1.5f; break;
            }

            return (int)Math.Round(sizeModifier * rarityModifier * modModifier * 10);
        }

        /// <summary>
        /// Returns the name of the descriptor for the given fish type.
        /// </summary>
        private string GetDescriptorName(FishSize size, FishRarity rarity)
        {
            string sizePart = "";
            switch (size)
            {
                case FishSize.Small: sizePart = "Small"; break;
                case FishSize.Medium: sizePart = "Medium"; break;
                case FishSize.Large: sizePart = "Large"; break;
                case FishSize.VeryLarge: sizePart = "VeryLarge"; break;
            }

            string rarityPart = "";
            switch (rarity)
            {
                case FishRarity.Common: rarityPart = "1"; break;
                case FishRarity.Uncommon: rarityPart = "2"; break;
                case FishRarity.Rare: rarityPart = "3"; break;
                case FishRarity.VeryRare: rarityPart = "4"; break;
            }

            return "Sprites/Fish/" + sizePart + rarityPart;
        }
    }
}
