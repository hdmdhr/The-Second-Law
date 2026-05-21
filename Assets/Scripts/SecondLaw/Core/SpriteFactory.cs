using UnityEngine;

namespace SecondLaw
{
    public static class SpriteFactory
    {
        public static Sprite MakeSprite(string name, Color main, Color accent, int width = 64, int height = 64)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = name + "Texture";
            texture.filterMode = FilterMode.Point;

            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
            float radius = Mathf.Min(width, height) * 0.42f;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    Color color = Color.clear;
                    if (distance < radius)
                    {
                        float t = Mathf.Clamp01(distance / radius);
                        color = Color.Lerp(accent, main, t);
                        color.a = 1f;
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.18f), 64f);
        }

        public static Sprite MakeRectSprite(string name, Color color, int width = 32, int height = 32)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = name + "Texture";
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
