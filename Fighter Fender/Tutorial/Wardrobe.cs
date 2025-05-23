using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tutorial
{
    public class Wardrobe
    {
        private Skins skins;
        private SpriteFont font;

        public bool IsOpen { get; private set; } = false;

        public Wardrobe(Skins skins, SpriteFont font)
        {
            this.skins = skins;
            this.font = font;
        }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            if (!IsOpen) return;

            // Example: Use Left/Right arrows to change skin, Escape to close
            if (keyboardState.IsKeyDown(Keys.Left))
                skins.SelectPreviousSkin();
            if (keyboardState.IsKeyDown(Keys.Right))
                skins.SelectNextSkin();
            if (keyboardState.IsKeyDown(Keys.Escape))
                Close();
        }

        public void Draw(SpriteBatch spriteBatch, int windowWidth, int windowHeight)
        {
            if (!IsOpen) return;

            // Draw a simple wardrobe UI background
            Rectangle bgRect = new Rectangle(windowWidth / 2 - 200, windowHeight / 2 - 150, 400, 300);
            spriteBatch.Draw(TextureHelper.WhitePixel(spriteBatch.GraphicsDevice), bgRect, Color.Black * 0.8f);

            // Draw current skin
            Texture2D skin = skins.CurrentSkin;
            if (skin != null)
            {
                int skinSize = 128;
                Rectangle skinRect = new Rectangle(windowWidth / 2 - skinSize / 2, windowHeight / 2 - skinSize / 2, skinSize, skinSize);
                spriteBatch.Draw(skin, skinRect, Color.White);
            }

            // Draw instructions
            string instructions = "Left/Right: Change Skin   Esc: Close";
            Vector2 instrSize = font.MeasureString(instructions);
            spriteBatch.DrawString(font, instructions, new Vector2(windowWidth / 2 - instrSize.X / 2, windowHeight / 2 + 80), Color.White);
        }

        // Optional: helper for click detection if you want to use mouse for closing
        public bool IsCloseClicked(MouseState mouseState, MouseState prevMouseState, Rectangle closeBox)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (closeBox.Contains(mouseState.Position))
                    return true;
            }
            return false;
        }

        // New method to draw the wardrobe screen directly
        public void DrawWardrobeScreen(SpriteBatch spriteBatch, int windowWidth, int windowHeight)
        {
            // Draw a simple wardrobe UI background
            Rectangle bgRect = new Rectangle(windowWidth / 2 - 200, windowHeight / 2 - 150, 400, 300);
            spriteBatch.Draw(TextureHelper.WhitePixel(spriteBatch.GraphicsDevice), bgRect, Color.Black * 0.8f);

            // Draw current skin
            Texture2D skin = skins.CurrentSkin;
            if (skin != null)
            {
                int skinSize = 128;
                Rectangle skinRect = new Rectangle(windowWidth / 2 - skinSize / 2, windowHeight / 2 - skinSize / 2, skinSize, skinSize);
                spriteBatch.Draw(skin, skinRect, Color.White);
            }

            // Draw instructions
            string instructions = "Left/Right: Change Skin   Esc: Close";
            Vector2 instrSize = font.MeasureString(instructions);
            spriteBatch.DrawString(font, instructions, new Vector2(windowWidth / 2 - instrSize.X / 2, windowHeight / 2 + 80), Color.White);
        }
    }

    // Helper for a 1x1 white pixel texture
    public static class TextureHelper
    {
        private static Texture2D _whitePixel;
        public static Texture2D WhitePixel(GraphicsDevice device)
        {
            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(device, 1, 1);
                _whitePixel.SetData(new[] { Color.White });
            }
            return _whitePixel;
        }
    }
}
