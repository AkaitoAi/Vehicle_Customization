using UnityEngine;

namespace AkaitoAi.Customization
{
    /// <summary>
    /// Applies texture offset and scale (tiling) customization to materials for atlas-based cloth textures (e.g. Shirts, Pants).
    /// Works with both MeshRenderer and SkinnedMeshRenderer on character models.
    /// </summary>
    public class ApplyMaterialOffsetCustomization : Customization
    {
        [Header("Target Renderers & Materials")]
        [Tooltip("The renderers (MeshRenderer or SkinnedMeshRenderer) whose materials will be offset. If empty, will auto-find in children.")]
        [SerializeField] private Renderer[] renderers;

        [Tooltip("If true, automatically searches for Renderers in children if renderers array is empty.")]
        [SerializeField] private bool autoFindRenderers = true;

        [Tooltip("If true, applies the texture offset to all material slots on the renderer.")]
        [SerializeField] private bool applyToAllMaterialSlots = false;

        [Tooltip("Index of the material on the renderer to modify if applyToAllMaterialSlots is false (0 for 1st slot).")]
        [SerializeField] private int materialIndex = 0;

        [Tooltip("Optional: Only apply to materials whose name contains this string (e.g. 'Shirt', 'Pant', 'Cloths'). Leave empty to target by index.")]
        [SerializeField] private string targetMaterialName = "";

        [Tooltip("Shader property name for the main texture (e.g. _MainTex for Standard/Built-in or _BaseMap for URP).")]
        [SerializeField] private string texturePropertyName = "_MainTex";

        [Header("Atlas UV Offsets")]
        [Tooltip("Default texture scale / tiling applied to all offsets (e.g. (0.5, 0.5) for a 2x2 atlas).")]
        [SerializeField] private Vector2 defaultScale = new Vector2(0.5f, 0.5f);

        [Tooltip("List of UV offsets starting from (0, 0) up to (0.5, 0.5) for each itemID (0, 1, 2, ...). itemID maps directly to this array index.")]
        [SerializeField] private Vector2[] offsets = new Vector2[]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(0.5f, 0.0f),
            new Vector2(0.0f, 0.5f),
            new Vector2(0.5f, 0.5f)
        };

        [Header("Grid Auto-Generator (Editor Utility)")]
        [Tooltip("Number of columns in the texture atlas grid.")]
        [SerializeField] private int gridColumns = 2;

        [Tooltip("Number of rows in the texture atlas grid.")]
        [SerializeField] private int gridRows = 2;

        [Tooltip("If false (default), offsets start from (0, 0) at bottom-left up to (0.5, 0.5). If true, starts from top-left.")]
        [SerializeField] private bool topToBottom = false;

        protected override void Customize(int itemID)
        {
            if (!CanCustomize()) return;

            base.Customize(itemID);

            EnsureRenderers();

            if (renderers == null || renderers.Length == 0)
            {
                Debug.LogWarning($"[ApplyMaterialOffsetCustomization] No renderers found on {gameObject.name}.");
                return;
            }

            if (offsets == null || offsets.Length == 0)
            {
                Debug.LogWarning($"[ApplyMaterialOffsetCustomization] Offsets array is empty on {gameObject.name}.");
                return;
            }

            if (itemID < 0 || itemID >= offsets.Length)
            {
                Debug.LogWarning($"[ApplyMaterialOffsetCustomization] itemID {itemID} is out of range for offsets array (Total offsets: {offsets.Length}) on {gameObject.name}.");
                return;
            }

            // Direct mapping: itemID directly maps to offsets array index
            Vector2 targetOffset = offsets[itemID];
            Vector2 targetScale = defaultScale != Vector2.zero ? defaultScale : new Vector2(0.5f, 0.5f);

            // Apply to all assigned renderers
            foreach (Renderer rend in renderers)
            {
                if (rend == null) continue;

                Material[] mats = Application.isPlaying ? rend.materials : rend.sharedMaterials;
                if (mats == null || mats.Length == 0) continue;

                bool modified = false;

                for (int i = 0; i < mats.Length; i++)
                {
                    Material mat = mats[i];
                    if (mat == null) continue;

                    bool shouldApply = false;

                    if (!string.IsNullOrEmpty(targetMaterialName))
                    {
                        if (mat.name.IndexOf(targetMaterialName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            shouldApply = true;
                        }
                    }
                    else if (applyToAllMaterialSlots || i == materialIndex)
                    {
                        shouldApply = true;
                    }

                    if (shouldApply)
                    {
                        ApplyOffsetToMaterial(mat, targetOffset, targetScale);
                        modified = true;
                    }
                }

                if (modified)
                {
                    if (Application.isPlaying)
                    {
                        rend.materials = mats;
                    }
                    else
                    {
                        rend.sharedMaterials = mats;
                    }
                }
            }
        }

        private void ApplyOffsetToMaterial(Material mat, Vector2 offset, Vector2 scale)
        {
            if (mat == null) return;

            if (scale == Vector2.zero)
            {
                scale = defaultScale != Vector2.zero ? defaultScale : new Vector2(0.5f, 0.5f);
            }

            // 1. Custom specified property name
            if (!string.IsNullOrEmpty(texturePropertyName) && mat.HasProperty(texturePropertyName))
            {
                mat.SetTextureOffset(texturePropertyName, offset);
                mat.SetTextureScale(texturePropertyName, scale);
            }

            // 2. Standard main texture property
            if (mat.HasProperty("_MainTex"))
            {
                mat.SetTextureOffset("_MainTex", offset);
                mat.SetTextureScale("_MainTex", scale);
            }

            // 3. URP BaseMap property
            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTextureOffset("_BaseMap", offset);
                mat.SetTextureScale("_BaseMap", scale);
            }

            // 4. Safe fallback for mainTextureOffset
            try
            {
                if (mat.HasProperty("_MainTex"))
                {
                    mat.mainTextureOffset = offset;
                    mat.mainTextureScale = scale;
                }
            }
            catch { }
        }

        private void EnsureRenderers()
        {
            if ((renderers == null || renderers.Length == 0) && autoFindRenderers)
            {
                renderers = GetComponentsInChildren<Renderer>(true);
            }
        }

        protected override void OnEnable()
        {
            EnsureRenderers();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && CustomizationData != null)
            {
                EnsureRenderers();
                Customize(CustomizationData.defaultItemID);
            }
        }
#endif

        /// <summary>
        /// Context menu utility to auto-generate UV offsets starting from (0, 0) up to (0.5, 0.5)
        /// based on grid columns and rows.
        /// Right-click the component header in Inspector and select 'Auto Generate Grid Offsets'.
        /// </summary>
        [ContextMenu("Auto Generate Grid Offsets")]
        public void AutoGenerateGridOffsets()
        {
            if (gridColumns <= 0 || gridRows <= 0)
            {
                Debug.LogWarning("[ApplyMaterialOffsetCustomization] Grid columns and rows must be greater than 0.");
                return;
            }

            int totalItems = gridColumns * gridRows;
            offsets = new Vector2[totalItems];

            float stepX = 1f / gridColumns;
            float stepY = 1f / gridRows;

            defaultScale = new Vector2(stepX, stepY);

            int index = 0;
            if (topToBottom)
            {
                for (int r = gridRows - 1; r >= 0; r--)
                {
                    for (int c = 0; c < gridColumns; c++)
                    {
                        offsets[index++] = new Vector2(c * stepX, r * stepY);
                    }
                }
            }
            else
            {
                // Starts from (0, 0) up to (stepX * (cols-1), stepY * (rows-1)) e.g. (0,0) to (0.5, 0.5)
                for (int r = 0; r < gridRows; r++)
                {
                    for (int c = 0; c < gridColumns; c++)
                    {
                        offsets[index++] = new Vector2(c * stepX, r * stepY);
                    }
                }
            }

            Debug.Log($"[ApplyMaterialOffsetCustomization] Generated {totalItems} UV offsets for {gridColumns}x{gridRows} atlas grid starting at (0, 0). Scale: {defaultScale}");
        }
    }
}
