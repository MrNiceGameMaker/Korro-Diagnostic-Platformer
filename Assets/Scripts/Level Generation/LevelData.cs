using System;
using System.Collections.Generic;
using UnityEngine;

namespace KorroAI.LevelSystem
{
    /// <summary>
    /// Represents a single row in the level data.
    /// Each row defines a platform or gap at a specific Z-position.
    /// </summary>
    [Serializable]
    public class RowData
    {
        /// <summary>
        /// Platform type: 0=Standard, 1=Moving, 2=Ghost, 3=Gap
        /// </summary>
        public int type;
        
        /// <summary>
        /// Height offset relative to the previous row's height
        /// </summary>
        public float heightOffset;
    }

    /// <summary>
    /// Complete level data structure matching the JSON schema.
    /// Contains level metadata and an array of rows defining the level layout.
    /// </summary>
    [Serializable]
    public class LevelData
    {
        /// <summary>
        /// Unique identifier for this level
        /// </summary>
        public int levelId;
        
        /// <summary>
        /// Starting height (Y position) for the first row
        /// </summary>
        public float startHeight;
        
        /// <summary>
        /// Array of rows defining the level layout along the Z-axis
        /// </summary>
        public List<RowData> rows;
    }
}

