using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tutorial
{
    public class Menus
    {
        private SpriteFont _font;
        private KeyboardState _previousKeyboardState;

        // Store rectangles for click detection
        private Rectangle playBoxRect;
        private Rectangle difficultyBoxRect;
        private Rectangle[] difficultyButtons = new Rectangle[3];

        public enum MenuScreen
        {
            Main,
            Difficulty
        }

        public MenuScreen CurrentMenuScreen = MenuScreen.Main;
        public int SelectedDifficulty = -1; // 0 = Easy, 1 = Medium, 2 = Hard

        public void LoadContent(SpriteFont font)
        {
            _font = font;
        }

        public MenuAction UpdateMainMenu(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
            {
                _previousKeyboardState = keyboardState;
                return MenuAction.StartGame;
            }
            
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                return MenuAction.ExitGame;
            }
            
            _previousKeyboardState = keyboardState;
            return MenuAction.None;
        }
        
        public MenuAction UpdateGameOver(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.R) && !_previousKeyboardState.IsKeyDown(Keys.R))
            {
                _previousKeyboardState = keyboardState;
                return MenuAction.RestartGame;
            }
            
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                return MenuAction.BackToMenu;
            }
            
            _previousKeyboardState = keyboardState;
            return MenuAction.None;
        }
        
        public MenuAction UpdateGameWin(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.R) && !_previousKeyboardState.IsKeyDown(Keys.R))
            {
                _previousKeyboardState = keyboardState;
                return MenuAction.RestartGame;
            }
            
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                return MenuAction.BackToMenu;
            }
            
            _previousKeyboardState = keyboardState;
            return MenuAction.None;
        }
        
        public void DrawMainMenu(SpriteBatch spriteBatch, int windowWidth, int windowHeight)
        {
            // Updated menu text
            string text = "=== SPACE SHOOTER ===\n\n" +
                          "Defend the galaxy and collect points!\n\n" +
                          "Controls:\n" +
                          "  - Move: W A S D\n" +
                          "  - Shoot: SPACE\n\n" +
                          "Press SPACE to Start\n" +
                          "Press ESC to Exit\n\n" +
                          "Reach 5000 Points to Win!";
            Vector2 textSize = _font.MeasureString(text);
            spriteBatch.DrawString(_font, text, new Vector2((windowWidth - textSize.X) / 2, (windowHeight - textSize.Y) / 2), Color.White);
        }
        
        public void DrawGameOver(SpriteBatch spriteBatch, int windowWidth, int windowHeight, int finalScore)
        {
            string text = $"Game Over!\nFinal Score: {finalScore}\n\nPress R to Restart\nPress ESC for Main Menu";
            Vector2 textSize = _font.MeasureString(text);
            spriteBatch.DrawString(_font, text, new Vector2((windowWidth - textSize.X) / 2, (windowHeight - textSize.Y) / 2), Color.Red);
        }
        
        public void DrawGameWin(SpriteBatch spriteBatch, int windowWidth, int windowHeight, int finalScore)
        {
            string text = $"You Win!\nFinal Score: {finalScore}\n\nPress R to Restart\nPress ESC for Main Menu";
            Vector2 textSize = _font.MeasureString(text);
            spriteBatch.DrawString(_font, text, new Vector2((windowWidth - textSize.X) / 2, (windowHeight - textSize.Y) / 2), Color.Green);
        }
        
        public void DrawCustomMainMenu(SpriteBatch spriteBatch, int windowWidth, int windowHeight, Texture2D logoTexture, Texture2D whitePixel)
        {
            // Draw dark background (fill entire screen)
            spriteBatch.GraphicsDevice.Clear(new Color(16, 16, 16));

            // --- Draw logo ---
            int logoSize = 128; // or any square size you want
            // Use the smaller of the texture's width/height to keep it square
            int logoDrawSize = Math.Min(logoTexture.Width, logoTexture.Height);
            float scale = logoSize / (float)logoDrawSize;
            int logoX = (windowWidth - logoSize) / 2;
            int logoY = windowHeight / 2 - 200;
            spriteBatch.Draw(
                logoTexture,
                new Rectangle(logoX, logoY, logoSize, logoSize),
                new Rectangle(0, 0, logoDrawSize, logoDrawSize),
                Color.White
            );

            // --- Draw containers ---
            int containerWidth = 320;
            int containerHeight = 60;
            int containerSpacing = 20;
            int totalHeight = containerHeight * 3 + containerSpacing * 2;
            int startY = (windowHeight - totalHeight) / 2 + 40;

            for (int i = 0; i < 3; i++)
            {
                int x = (windowWidth - containerWidth) / 2;
                int y = startY + i * (containerHeight + containerSpacing);
                Rectangle boxRect = new Rectangle(x, y, containerWidth, containerHeight);
                spriteBatch.Draw(whitePixel, boxRect, new Color(32, 32, 32, 220));

                if (i == 0)
                {
                    playBoxRect = boxRect;
                    // Draw "Play" centered in the first box
                    string playText = "Play";
                    Vector2 textSize = _font.MeasureString(playText);
                    Vector2 textPos = new Vector2(
                        x + (containerWidth - textSize.X) / 2,
                        y + (containerHeight - textSize.Y) / 2
                    );
                    spriteBatch.DrawString(_font, playText, textPos, Color.White);
                }
                else if (i == 1)
                {
                    difficultyBoxRect = boxRect;
                    // Draw "Difficulty" centered in the second box
                    string diffText = "Difficulty";
                    Vector2 textSize = _font.MeasureString(diffText);
                    Vector2 textPos = new Vector2(
                        x + (containerWidth - textSize.X) / 2,
                        y + (containerHeight - textSize.Y) / 2
                    );
                    spriteBatch.DrawString(_font, diffText, textPos, Color.White);
                }
                // Optionally, draw text for other boxes here
            }
        }

        public void DrawDifficultyMenu(SpriteBatch spriteBatch, int windowWidth, int windowHeight, Texture2D whitePixel)
        {
            spriteBatch.GraphicsDevice.Clear(new Color(16, 16, 16));
            string title = "Select Difficulty";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2((windowWidth - titleSize.X) / 2, 120), Color.White);

            int buttonWidth = 160;
            int buttonHeight = 60;
            int spacing = 40;
            int totalWidth = buttonWidth * 3 + spacing * 2;
            int startX = (windowWidth - totalWidth) / 2;
            int y = windowHeight / 2 - buttonHeight / 2;

            string[] labels = { "Easy", "Medium", "Hard" };
            for (int i = 0; i < 3; i++)
            {
                int x = startX + i * (buttonWidth + spacing);
                Rectangle btnRect = new Rectangle(x, y, buttonWidth, buttonHeight);
                difficultyButtons[i] = btnRect;
                spriteBatch.Draw(whitePixel, btnRect, new Color(32, 32, 32, 220));
                Vector2 textSize = _font.MeasureString(labels[i]);
                Vector2 textPos = new Vector2(
                    x + (buttonWidth - textSize.X) / 2,
                    y + (buttonHeight - textSize.Y) / 2
                );
                spriteBatch.DrawString(_font, labels[i], textPos, Color.White);
            }
        }

        // Call this in your Game1.cs Update when in menu state
        public bool HandleMainMenuClick(MouseState mouseState, MouseState prevMouseState)
        {
            // Only trigger on mouse press (not hold)
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (playBoxRect.Contains(mouseState.Position))
                {
                    return true; // Play box clicked
                }
            }
            return false;
        }

        public bool IsPlayBoxClicked(MouseState mouseState, MouseState prevMouseState)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (playBoxRect.Contains(mouseState.Position))
                    return true;
            }
            return false;
        }

        public bool IsDifficultyBoxClicked(MouseState mouseState, MouseState prevMouseState)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (difficultyBoxRect.Contains(mouseState.Position))
                    return true;
            }
            return false;
        }

        public int GetDifficultyButtonClicked(MouseState mouseState, MouseState prevMouseState)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (difficultyButtons[i].Contains(mouseState.Position))
                        return i; // 0 = Easy, 1 = Medium, 2 = Hard
                }
            }
            return -1;
        }
    }
    
    public enum MenuAction
    {
        None,
        StartGame,
        RestartGame,
        BackToMenu,
        ExitGame
    }
}