using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>One line of the Credits panel: a role heading ("ART BY") in its accent color
    /// over a pastel checkered band carrying the name.</summary>
    public readonly struct CreditsEntry
    {
        public string Role { get; }
        public string Name { get; }
        public Color RoleColor { get; }
        public Color BandColor { get; }

        public CreditsEntry(string role, string name, Color roleColor, Color bandColor)
        {
            Role = role;
            Name = name;
            RoleColor = roleColor;
            BandColor = bandColor;
        }
    }
}
