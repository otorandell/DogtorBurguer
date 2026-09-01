using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>One line of the Credits panel: a role heading ("ART BY") in its accent color
    /// over its authored checkered band (UI art name) carrying the name.</summary>
    public readonly struct CreditsEntry
    {
        public string Role { get; }
        public string Name { get; }
        public Color RoleColor { get; }
        public string BandArt { get; }

        public CreditsEntry(string role, string name, Color roleColor, string bandArt)
        {
            Role = role;
            Name = name;
            RoleColor = roleColor;
            BandArt = bandArt;
        }
    }
}
