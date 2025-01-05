using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

    [CreateAssetMenu(fileName = "New Custom Animated Rule Tile", menuName = "Tiles/Custom Animated Rule Tile")]
    public class CustomAnimatedRuleTile : RuleTile<CustomAnimatedRuleTile.Neighbor>
    {
        [System.Serializable]
        public class Neighbor : RuleTile.TilingRule.Neighbor { }

        [Header("Animation Settings")]
        public float defaultMinSpeed = 3f; // Speed of the animation
        public float defaultMaxSpeed = 5f; // Speed of the animation

        public void SetupWithTexture(Texture2D texture)
        {
            if (texture == null) return;

            // Extract sprites from the texture
            List<Sprite> sprites = ExtractSpritesFromSheet(texture);
            if (sprites.Count == 0) return;

            if (m_TilingRules == null)
                m_TilingRules = new List<TilingRule>();

            // Clear existing rules
            m_TilingRules.Clear();

            // Create a new tiling rule
            TilingRule rule = new TilingRule
            {
                m_Output = TilingRule.OutputSprite.Animation,
                m_Sprites = sprites.ToArray(),
                m_MinAnimationSpeed = defaultMinSpeed,
                m_MaxAnimationSpeed = defaultMaxSpeed
            };

            // Add the rule
            m_DefaultSprite = sprites[0];
            m_TilingRules.Add(rule);
        }

        private List<Sprite> ExtractSpritesFromSheet(Texture2D texture)
        {
            List<Sprite> sprites = new List<Sprite>();

            // Load all sprites associated with the texture
            string path = AssetDatabase.GetAssetPath(texture);
            Object[] loadedAssets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (Object asset in loadedAssets)
            {
                if (asset is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }

            return sprites;
        }

        [MenuItem("Assets/Create/Custom Animated Rule Tile From Texture", false, 10)]
        private static void CreateTileFromTexture()
        {
            // Get the selected texture
            Texture2D selectedTexture = Selection.activeObject as Texture2D;
            if (selectedTexture == null)
            {
                Debug.LogError("No texture selected. Please select a texture.");
                return;
            }

            // Generate a new CustomAnimatedRuleTile
            string texturePath = AssetDatabase.GetAssetPath(selectedTexture);
            string folderPath = System.IO.Path.GetDirectoryName(texturePath);
            string tileName = selectedTexture.name;

            // Create the tile asset
            CustomAnimatedRuleTile newTile = ScriptableObject.CreateInstance<CustomAnimatedRuleTile>();
            newTile.SetupWithTexture(selectedTexture);

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{tileName}_RuleTile.asset");
            AssetDatabase.CreateAsset(newTile, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Custom Animated Rule Tile created at: {assetPath}");
            Selection.activeObject = newTile;
        }
    }
