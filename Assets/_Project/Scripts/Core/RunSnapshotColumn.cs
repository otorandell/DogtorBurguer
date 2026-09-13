using System;

namespace DogtorBurguer
{
    /// <summary>
    /// One column's stacked pieces in a <see cref="RunSnapshot"/>, bottom row first.
    ///
    /// A landed piece carries no state beyond its type — row, world position and sorting order
    /// are all recomputed from its index in the column — so a board is just four of these.
    /// The class exists because JsonUtility cannot serialize a jagged array directly; it needs
    /// a named type per level of nesting.
    /// </summary>
    [Serializable]
    public class RunSnapshotColumn
    {
        public IngredientType[] Types;
    }
}
