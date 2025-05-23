using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq; // Add this using directive for LINQ methods

namespace Tutorial
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;

        private Texture2D background;
        private Texture2D ship;
        private Vector2 shipPosition;
        private Vector2 shipSpeed = new Vector2(5f, 5f);

        private Texture2D enemyTexture;
        private List<Vector2> enemies = new List<Vector2>();
        private Vector2 enemySpeed = new Vector2(0, 1f); // Default, will be set on game start
        private double enemySpawnTimer = 0;

        private Texture2D bulletTexture;
        private List<Vector2> bullets = new List<Vector2>();
        private Vector2 bulletSpeed = new Vector2(0, -5f);

        private Texture2D coinTexture;
        private List<Vector2> coins = new List<Vector2>();
        private double coinSpawnTimer = 0;

        private Texture2D asteroidTexture;
        private List<Vector2> asteroids = new List<Vector2>();
        private double asteroidSpawnTimer = 0;

        private Texture2D powerupDoublePointsTexture;
        private Texture2D powerupImmortalTexture;
        private Texture2D powerupHeartTexture;
        private List<Tuple<Vector2, string>> powerups = new List<Tuple<Vector2, string>>();
        private double powerupSpawnTimer = 0;
        private bool doublePoints = false;
        private bool immortal = false;
        private double doublePointsTimer = 0;
        private double immortalTimer = 0;

        private SpriteFont font;
        private SpriteFont BoldFont; // Renamed from bigBoldFont
        private SpriteFont BigBoldFont; // Add a field for the big font

        private int score = 0;
        private int health = 3;
        private bool gameStarted = false;
        private bool gameOver = false;
        private bool gameWin = false;

        private Random random = new Random();   

        // Add constants for ship size
        private const int ShipWidth = 64;
        private const int ShipHeight = 64;
        private const int EnemyWidth = 60;   // Add this
        private const int EnemyHeight = 60;  // Add this

        private Menus menus; // Add this field

        // GumService GumService => GumService.Default;

        private Texture2D logoTexture;
        private Texture2D whitePixel;

        private MouseState prevMouseState;

        private enum MenuScreen
        {
            Main,
            Difficulty,
            NameEntry,
            Leaderboard // Add leaderboard screen
        }
        private MenuScreen currentMenuScreen = MenuScreen.Main;
        private int selectedDifficulty = -1;

        private Leaderboard leaderboard = new Leaderboard(); // Add leaderboard field

        private string playerName = "";
        private bool isEnteringName = false;
        private string lastEnteredName = "";

        private double nameEntryInitialDelay = 0.5; // 500 ms before repeat starts
        private double nameEntryRepeatDelay = 0.04; // 40 ms between repeats
        private double nameEntryKeyTimer = 0;
        private Keys lastNameEntryKey = Keys.None;
        private bool nameEntryInitialPress = true;

        private bool lostScreen = false;
        private double lostScreenTimer = 0; // Add a timer for the lost screen

        // Add for leaderboard back button
        private Rectangle leaderboardBackBox = new Rectangle(20, 20, 48, 48);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Use ShipHeight for initial position
            shipPosition = new Vector2(300, Window.ClientBounds.Height - ShipHeight - 36);
            // Gum.Initialize(this);

            // var mainPanel = new StackPanel();
            // mainPanel.AddToRoot();
            //var playButton = new Button("Play");
            // playButton.Text = "Play";
            //mainPanel.AddChild(playButton);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            background = Content.Load<Texture2D>("Sprites/background");
            ship = Content.Load<Texture2D>("Sprites/ship");
            enemyTexture = Content.Load<Texture2D>("Sprites/tripod");
            bulletTexture = Content.Load<Texture2D>("Sprites/bullet");
            coinTexture = Content.Load<Texture2D>("Sprites/coin");
            asteroidTexture = Content.Load<Texture2D>("Sprites/asteroid");
            powerupDoublePointsTexture = Content.Load<Texture2D>("Sprites/doubleup");
            powerupImmortalTexture = Content.Load<Texture2D>("Sprites/immortal");
            powerupHeartTexture = Content.Load<Texture2D>("Sprites/heart");
            font = Content.Load<SpriteFont>("Sprites/BoldFont");

            // Load logo texture from Sprites folder
            logoTexture = Content.Load<Texture2D>("Sprites/fighter fender");

            // Create a 1x1 white pixel texture for drawing rectangles
            whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            whitePixel.SetData(new[] { Color.White });

            // Initialize and load Menus
            menus = new Menus();
            menus.LoadContent(font);

            BoldFont = Content.Load<SpriteFont>("Sprites/BoldFont");
            BigBoldFont = Content.Load<SpriteFont>("Sprites/BigFont"); // Assign the big bold font here
            leaderboard.Load(); // Load leaderboard
        }

        protected override void Update(GameTime gameTime)
        {
            // Gum.Update(gameTime);

            KeyboardState k = Keyboard.GetState();
            // Remove this line so ESC does not exit the game:
            // if (k.IsKeyDown(Keys.Escape)) Exit();

            MouseState mouseState = Mouse.GetState();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (lostScreen)
            {
                lostScreenTimer -= dt;
                if (lostScreenTimer <= 0)
                {
                    lostScreen = false;
                    // Reset all gameplay objects and player position
                    ResetGame();
                    // Do NOT set currentMenuScreen or gameStarted here, let the menu logic handle it
                }
                prevMouseState = Mouse.GetState();
                return;
            }

            if (!gameStarted)
            {
                // Ensure gameplay objects are always reset when entering menu or name entry after lostScreen
                if (enemies.Count > 0 || bullets.Count > 0 || coins.Count > 0 || asteroids.Count > 0 || powerups.Count > 0 || health != 3 || score != 0)
                {
                    ResetGame();
                }
                if (currentMenuScreen == MenuScreen.Main)
                {
                    if (menus.IsPlayBoxClicked(mouseState, prevMouseState))
                    {
                        playerName = "";
                        isEnteringName = true;
                        currentMenuScreen = MenuScreen.NameEntry;
                    }
                    else if (menus.IsDifficultyBoxClicked(mouseState, prevMouseState))
                    {
                        currentMenuScreen = MenuScreen.Difficulty;
                    }
                    // Add: Leaderboard button click
                    else if (menus.IsLeaderboardBoxClicked != null && menus.IsLeaderboardBoxClicked(mouseState, prevMouseState))
                    {
                        currentMenuScreen = MenuScreen.Leaderboard;
                    }
                }
                else if (currentMenuScreen == MenuScreen.Difficulty)
                {
                    int diff = menus.GetDifficultyButtonClicked(mouseState, prevMouseState);
                    if (diff != -1)
                    {
                        selectedDifficulty = diff;
                        currentMenuScreen = MenuScreen.Main;
                    }
                }
                else if (currentMenuScreen == MenuScreen.NameEntry)
                {
                    nameEntryKeyTimer -= dt;
                    Keys[] pressed = k.GetPressedKeys();
                    Keys key = pressed.Length > 0 ? pressed[0] : Keys.None;

                    if (key != lastNameEntryKey)
                    {
                        nameEntryKeyTimer = 0;
                        nameEntryInitialPress = true;
                    }

                    if (key != Keys.None && nameEntryKeyTimer <= 0)
                    {
                        if (key == Keys.Enter && playerName.Length > 0)
                        {
                            isEnteringName = false;
                            ResetGame();
                            SetDifficultyEnemySpeed();
                            gameStarted = true;
                            currentMenuScreen = MenuScreen.Main;
                            // Start the game immediately after submitting the name
                            // (already handled by setting gameStarted = true and currentMenuScreen = MenuScreen.Main)
                        }
                        else if (key == Keys.Back && playerName.Length > 0)
                        {
                            playerName = playerName.Substring(0, playerName.Length - 1);
                        }
                        else if (key >= Keys.A && key <= Keys.Z && playerName.Length < 12)
                        {
                            bool shift = k.IsKeyDown(Keys.LeftShift) || k.IsKeyDown(Keys.RightShift);
                            char c = (char)(key - Keys.A + (shift ? 'A' : 'a'));
                            playerName += c;
                        }
                        else if (key >= Keys.D0 && key <= Keys.D9 && playerName.Length < 12)
                        {
                            playerName += (char)('0' + (key - Keys.D0));
                        }
                        nameEntryKeyTimer = nameEntryInitialPress ? nameEntryInitialDelay : nameEntryRepeatDelay;
                        nameEntryInitialPress = false;
                    }
                    lastNameEntryKey = key;
                    prevMouseState = Mouse.GetState();
                    return;
                }
                else if (currentMenuScreen == MenuScreen.Leaderboard)
                {
                    // Return to main menu on ESC (do NOT exit the game)
                    if (k.IsKeyDown(Keys.Escape))
                    {
                        currentMenuScreen = MenuScreen.Main;
                    }
                    // Handle mouse click on back box
                    MouseState ms = Mouse.GetState();
                    if (ms.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                    {
                        if (leaderboardBackBox.Contains(ms.Position))
                        {
                            currentMenuScreen = MenuScreen.Main;
                        }
                    }
                    prevMouseState = ms;
                    return;
                }
                prevMouseState = mouseState;
                return;
            }

            if (lostScreen)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.R))
                {
                    lostScreen = false;
                    ResetGame();
                    gameStarted = true;
                }
                prevMouseState = Mouse.GetState();
                return;
            }

            if (gameOver || gameWin)
            {
                // --- Only handle name entry and leaderboard here ---
                if (gameOver && isEnteringName)
                {
                    nameEntryKeyTimer -= dt;
                    Keys[] pressed = k.GetPressedKeys();
                    Keys key = pressed.Length > 0 ? pressed[0] : Keys.None;

                    if (key != lastNameEntryKey)
                    {
                        nameEntryKeyTimer = 0;
                        nameEntryInitialPress = true;
                    }

                    if (key != Keys.None && nameEntryKeyTimer <= 0)
                    {
                        if (key == Keys.Enter && playerName.Length > 0)
                        {
                            leaderboard.AddScoreWithName(score, playerName);
                            lastEnteredName = playerName;
                            playerName = "";
                            isEnteringName = false;
                        }
                        else if (key == Keys.Back && playerName.Length > 0)
                        {
                            playerName = playerName.Substring(0, playerName.Length - 1);
                        }
                        else if (key >= Keys.A && key <= Keys.Z && playerName.Length < 12)
                        {
                            bool shift = k.IsKeyDown(Keys.LeftShift) || k.IsKeyDown(Keys.RightShift);
                            char c = (char)(key - Keys.A + (shift ? 'A' : 'a'));
                            playerName += c;
                        }
                        else if (key >= Keys.D0 && key <= Keys.D9 && playerName.Length < 12)
                        {
                            playerName += (char)('0' + (key - Keys.D0));
                        }
                        nameEntryKeyTimer = nameEntryInitialPress ? nameEntryInitialDelay : nameEntryRepeatDelay;
                        nameEntryInitialPress = false;
                    }
                    lastNameEntryKey = key;
                    prevMouseState = Mouse.GetState();
                    return;
                }

                // Only allow restart after name has been entered
                if (!isEnteringName && Keyboard.GetState().IsKeyDown(Keys.R))
                {
                    ResetGame();
                    gameStarted = true;
                }
                return;
            }

            // Movement
            if (k.IsKeyDown(Keys.A) && shipPosition.X > 0) shipPosition.X -= shipSpeed.X;
            if (k.IsKeyDown(Keys.D) && shipPosition.X + ShipWidth < Window.ClientBounds.Width) shipPosition.X += shipSpeed.X;
            if (k.IsKeyDown(Keys.W) && shipPosition.Y > 0) shipPosition.Y -= shipSpeed.Y;
            if (k.IsKeyDown(Keys.S) && shipPosition.Y + ShipHeight < Window.ClientBounds.Height) shipPosition.Y += shipSpeed.Y;

            // Shooting
            if (k.IsKeyDown(Keys.Space) && bullets.Count == 0)
            {
                bullets.Add(new Vector2(shipPosition.X + ShipWidth / 2 - 5, shipPosition.Y));
            }

            // Update bullets
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i] += bulletSpeed;
                if (bullets[i].Y < 0) bullets.RemoveAt(i);
            }

            // Spawn enemies
            enemySpawnTimer += dt;
            if (enemySpawnTimer >= 1f)
            {
                enemySpawnTimer = 0;
                int maxX = Window.ClientBounds.Width - EnemyWidth; // Use EnemyWidth
                int enemyX = maxX > 0 ? random.Next(0, maxX) : 0;
                enemies.Add(new Vector2(enemyX, -EnemyHeight)); // Use EnemyHeight
            }

            // Update enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i] += enemySpeed;

                if (enemies[i].Y > Window.ClientBounds.Height)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                Rectangle enemyRect = new Rectangle((int)enemies[i].X, (int)enemies[i].Y, EnemyWidth, EnemyHeight); // Use EnemyWidth/Height
                Rectangle playerRect = new Rectangle((int)shipPosition.X, (int)shipPosition.Y, ShipWidth, ShipHeight);

                if (!immortal && enemyRect.Intersects(playerRect))
                {
                    enemies.RemoveAt(i);
                    health -= 1; // 1 damage
                    if (health <= 0)
                    {
                        lostScreen = true;
                        lostScreenTimer = 3.0; // Increased from 1.5 to 3.0 seconds
                        gameStarted = false;
                        leaderboard.AddScoreWithName(score, playerName); // Save score and name immediately
                        leaderboard.Load(); // <-- Reload leaderboard to reflect new score
                    }
                }

                for (int j = bullets.Count - 1; j >= 0; j--)
                {
                    Rectangle bulletRect = new Rectangle((int)bullets[j].X, (int)bullets[j].Y, bulletTexture.Width, bulletTexture.Height);
                    if (enemyRect.Intersects(bulletRect))
                    {
                        enemies.RemoveAt(i);
                        bullets.RemoveAt(j);
                        score += doublePoints ? 200 : 100;
                        break;
                    }
                }
            }

            // Coin spawn every 3 seconds
            coinSpawnTimer += dt;
            if (coinSpawnTimer >= 3f)
            {
                coinSpawnTimer = 0;
                int maxX = Window.ClientBounds.Width - coinTexture.Width;
                int coinX = maxX > 0 ? random.Next(0, maxX) : 0;
                coins.Add(new Vector2(coinX, -coinTexture.Height));
            }

            // Coin collection
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                coins[i] += new Vector2(0, 1f);
                Rectangle coinRect = new Rectangle((int)coins[i].X, (int)coins[i].Y, coinTexture.Width, coinTexture.Height);
                Rectangle playerRect = new Rectangle((int)shipPosition.X, (int)shipPosition.Y, ShipWidth, ShipHeight);
                if (coinRect.Intersects(playerRect))
                {
                    coins.RemoveAt(i);
                    score += doublePoints ? 100 : 50;
                }
            }

            // Asteroid spawn every 12 seconds (5 per minute)
            asteroidSpawnTimer += dt;
            if (asteroidSpawnTimer >= 12f)
            {
                asteroidSpawnTimer = 0;
                int maxX = Window.ClientBounds.Width - asteroidTexture.Width;
                int asteroidX = maxX > 0 ? random.Next(0, maxX) : 0;
                asteroids.Add(new Vector2(asteroidX, -asteroidTexture.Height));
            }

            // Update asteroids
            for (int i = asteroids.Count - 1; i >= 0; i--)
            {
                asteroids[i] += new Vector2(0, 2f);
                if (asteroids[i].Y > Window.ClientBounds.Height) 
                {
                    asteroids.RemoveAt(i);
                    continue;
                }
                else
                {
                    Rectangle a = new Rectangle((int)asteroids[i].X, (int)asteroids[i].Y, asteroidTexture.Width, asteroidTexture.Height);
                    Rectangle p = new Rectangle((int)shipPosition.X, (int)shipPosition.Y, ShipWidth, ShipHeight);
                    if (!immortal && a.Intersects(p))
                    {
                        asteroids.RemoveAt(i);
                        health -= 2; // Asteroid does 2 damage
                        if (health <= 0)
                        {
                            lostScreen = true;
                            lostScreenTimer = 3.0; // Increased from 1.5 to 3.0 seconds
                            gameStarted = false;
                            leaderboard.AddScoreWithName(score, playerName); // Save score and name immediately
                            leaderboard.Load(); // <-- Reload leaderboard to reflect new score
                        }
                        continue; // Prevent out-of-range access after removal
                    }
                }
            }

            // Powerup spawn
            powerupSpawnTimer += dt;
            if (powerupSpawnTimer >= 15f)
            {
                powerupSpawnTimer = 0;
                string type;
                int rand = random.Next(0, 3);
                if (rand == 0) type = "doubleup";
                else if (rand == 1) type = "immortal";
                else type = "heart";

                int maxX = Window.ClientBounds.Width - coinTexture.Width;
                int powerupX = maxX > 0 ? random.Next(0, maxX) : 0;
                powerups.Add(new Tuple<Vector2, string>(new Vector2(powerupX, -coinTexture.Height), type));
            }

            // Update powerups
            for (int i = powerups.Count - 1; i >= 0; i--)
            {
                var pu = powerups[i];
                Vector2 pos = pu.Item1 + new Vector2(0, 1.5f);
                string type = pu.Item2;

                if (pos.Y > Window.ClientBounds.Height)
                {
                    powerups.RemoveAt(i);
                    continue;
                }

                Rectangle puRect = new Rectangle((int)pos.X, (int)pos.Y, coinTexture.Width, coinTexture.Height);
                Rectangle playerRect = new Rectangle((int)shipPosition.X, (int)shipPosition.Y, ShipWidth, ShipHeight);

                if (puRect.Intersects(playerRect))
                {
                    powerups.RemoveAt(i);
                    switch (type)
                    {
                        case "doubleup":
                            doublePoints = true;
                            doublePointsTimer = 10.0; // 10 seconds
                            break;
                        case "immortal":
                            immortal = true;
                            immortalTimer = 5.0; // 5 seconds
                            break;
                        case "heart":
                            if (health < 3) health++;
                            break;
                    }
                }
                else
                {
                    powerups[i] = new Tuple<Vector2, string>(pos, type);
                }
            }

            // Timers for powerups
            if (doublePoints)
            {
                doublePointsTimer -= dt;
                if (doublePointsTimer <= 0)
                {
                    doublePoints = false;
                }
            }
            if (immortal)
            {
                immortalTimer -= dt;
                if (immortalTimer <= 0)
                {
                    immortal = false;
                }
            }

            prevMouseState = mouseState;
            base.Update(gameTime);
        }

        private void ResetGame()
        {
            score = 0;
            health = 3;
            enemies.Clear();
            bullets.Clear();
            coins.Clear();
            asteroids.Clear();
            powerups.Clear();
            doublePoints = false;
            immortal = false;
            doublePointsTimer = 0;
            immortalTimer = 0;
            gameOver = false;
            gameWin = false;
            shipPosition = new Vector2(300, Window.ClientBounds.Height - ShipHeight - 36);
            SetDifficultyEnemySpeed(); // Ensure speed is set on reset as well
        }

        private int asteroidDrawSize = 64; // Default size

        private void SetDifficultyEnemySpeed()
        {
            // 0 = Easy, 1 = Medium, 2 = Hard
            switch (selectedDifficulty)
            {
                case 0: // Easy
                    enemySpeed = new Vector2(0, 0.7f);
                    asteroidDrawSize = 64;
                    break;
                case 1: // Medium
                    enemySpeed = new Vector2(0, 1.2f);
                    asteroidDrawSize = 64;
                    break;
                case 2: // Hard
                    enemySpeed = new Vector2(0, 2.0f);
                    asteroidDrawSize = 96; // Increase asteroid size in hard mode
                    break;
                default:
                    enemySpeed = new Vector2(0, 1f);
                    asteroidDrawSize = 64;
                    break;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            if (lostScreen)
            {
                string text = "Lost";
                // Always use BigBoldFont for the "Lost" text if possible
                SpriteFont fontToUse = BigBoldFont ?? BoldFont;
                bool canRender = true;
                foreach (char c in text)
                {
                    if (!fontToUse.Characters.Contains(c))
                    {
                        canRender = false;
                        break;
                    }
                }
                if (!canRender)
                    fontToUse = BoldFont;

                // Increase the size by scaling if the font is still not visually big enough
                Vector2 textSize = fontToUse.MeasureString(text);
                float scale = 1f;
                // If you want the text to take up more of the screen, increase the scale
                if (fontToUse == BigBoldFont)
                {
                    // Try to fill about 60% of the screen width
                    float targetWidth = Window.ClientBounds.Width * 0.6f;
                    scale = Math.Min(targetWidth / textSize.X, 2.5f); // Don't scale too much
                }
                spriteBatch.DrawString(
                    fontToUse,
                    text,
                    new Vector2((Window.ClientBounds.Width - textSize.X * scale) / 2, (Window.ClientBounds.Height - textSize.Y * scale) / 2),
                    Color.Red,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0f
                );

                int y = (int)((Window.ClientBounds.Height + 40) / 2) + 20;

                // Show top 3 scores (not last 3)
                spriteBatch.DrawString(font, "Leaderboard:", new Vector2(10, y), Color.Yellow);
                int rank = 1;
                var topScores = leaderboard.HighScoresWithNames
                    .OrderByDescending(e => e.Score)
                    .Take(3)
                    .ToList();
                foreach (var entry in topScores)
                {
                    y += 24;
                    spriteBatch.DrawString(font, $"{rank}. {entry.Name} - {entry.Score}", new Vector2(10, y), Color.White);
                    rank++;
                }

                spriteBatch.End();
                return;
            }

            if (!gameStarted)
            {
                if (currentMenuScreen == MenuScreen.Main)
                {
                    // Draw custom menu with logo and containers (updates playBoxRect)
                    menus.DrawCustomMainMenu(
                        spriteBatch,
                        Window.ClientBounds.Width,
                        Window.ClientBounds.Height,
                        logoTexture,
                        whitePixel
                    );
                }
                else if (currentMenuScreen == MenuScreen.Difficulty)
                {
                    menus.DrawDifficultyMenu(
                        spriteBatch,
                        Window.ClientBounds.Width,
                        Window.ClientBounds.Height,
                        whitePixel
                    );
                }
                else if (currentMenuScreen == MenuScreen.NameEntry)
                {
                    string prompt = "Enter your name: " + playerName + "_";
                    Vector2 textSize = font.MeasureString(prompt);
                    spriteBatch.DrawString(font, prompt, new Vector2((Window.ClientBounds.Width - textSize.X) / 2, Window.ClientBounds.Height / 2), Color.Yellow);
                }
                else if (currentMenuScreen == MenuScreen.Leaderboard)
                {
                    // Draw leaderboard screen
                    spriteBatch.GraphicsDevice.Clear(Color.Black);
                    string title = "Leaderboard";
                    Vector2 titleSize = font.MeasureString(title);
                    spriteBatch.DrawString(font, title, new Vector2((Window.ClientBounds.Width - titleSize.X) / 2, 60), Color.Yellow);

                    int y = 120;
                    int rank = 1;
                    foreach (var entry in leaderboard.HighScoresWithNames)
                    {
                        string line = $"{rank}. {entry.Name} - {entry.Score}"; // Show both name and score
                        Vector2 lineSize = font.MeasureString(line);
                        spriteBatch.DrawString(font, line, new Vector2((Window.ClientBounds.Width - lineSize.X) / 2, y), Color.White);
                        y += 32;
                        rank++;
                    }

                    string hint = "Press ESC to return";
                    Vector2 hintSize = font.MeasureString(hint);
                    spriteBatch.DrawString(font, hint, new Vector2((Window.ClientBounds.Width - hintSize.X) / 2, y + 32), Color.Gray);

                    // Draw the back button (square)
                    spriteBatch.Draw(whitePixel, leaderboardBackBox, Color.Gray * 0.7f);
                    string backArrow = "<";
                    Vector2 arrowSize = font.MeasureString(backArrow);
                    spriteBatch.DrawString(font, backArrow,
                        new Vector2(
                            leaderboardBackBox.X + (leaderboardBackBox.Width - arrowSize.X) / 2,
                            leaderboardBackBox.Y + (leaderboardBackBox.Height - arrowSize.Y) / 2
                        ),
                        Color.White);

                    spriteBatch.End();
                    return;
                }
            }
            if (gameOver)
            {
                string text = "Game Over!\nPress R to Restart";
                Vector2 textSize = font.MeasureString(text);
                spriteBatch.DrawString(font, text, new Vector2((Window.ClientBounds.Width - textSize.X) / 2, (Window.ClientBounds.Height - textSize.Y) / 2), Color.Red);

                int y = (int)((Window.ClientBounds.Height + textSize.Y) / 2) + 20;

                if (isEnteringName)
                {
                    string prompt = "Enter your name: " + playerName + "_";
                    spriteBatch.DrawString(font, prompt, new Vector2(10, y), Color.Yellow);
                }
                else
                {
                    spriteBatch.DrawString(font, "Leaderboard:", new Vector2(10, y), Color.Yellow);
                    int rank = 1;
                    foreach (var entry in leaderboard.HighScoresWithNames)
                    {
                        y += 24;
                        spriteBatch.DrawString(font, $"{rank}. {entry.Name} - {entry.Score}", new Vector2(10, y), Color.White);
                        rank++;
                    }
                }
                spriteBatch.End();
                return;
            }

            if (gameWin)
            {
                string text = "You Win!\nPress R to Restart";
                Vector2 textSize = font.MeasureString(text);
                spriteBatch.DrawString(font, text, new Vector2((Window.ClientBounds.Width - textSize.X) / 2, (Window.ClientBounds.Height - textSize.Y) / 2), Color.Green);
                spriteBatch.End();
                return;
            }

            // Only draw gameplay objects when game is running
            if (gameStarted && !lostScreen && !gameOver && !gameWin)
            {
                // Draw the ship at 64x64 pixels
                spriteBatch.Draw(
                    ship,
                    new Rectangle((int)shipPosition.X, (int)shipPosition.Y, ShipWidth, ShipHeight),
                    Color.White
                );

                // Draw each enemy at 60x60 pixels
                foreach (var enemy in enemies)
                    spriteBatch.Draw(
                        enemyTexture,
                        new Rectangle((int)enemy.X, (int)enemy.Y, EnemyWidth, EnemyHeight),
                        Color.White
                    );

                foreach (var bullet in bullets)
                    spriteBatch.Draw(bulletTexture, bullet, Color.White);

                foreach (var coin in coins)
                    spriteBatch.Draw(coinTexture, coin, Color.White);

                // Draw asteroids scaled to match their hitbox
                foreach (var asteroid in asteroids)
                    spriteBatch.Draw(
                        asteroidTexture,
                        new Rectangle((int)asteroid.X, (int)asteroid.Y, asteroidDrawSize, asteroidDrawSize),
                        null,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        SpriteEffects.None,
                        0f
                    );

                // Draw powerups
                foreach (var pu in powerups)
                {
                    Texture2D tex = null;
                    if (pu.Item2 == "doubleup") tex = powerupDoublePointsTexture;
                    else if (pu.Item2 == "immortal") tex = powerupImmortalTexture;
                    else if (pu.Item2 == "heart") tex = powerupHeartTexture;

                    if (tex != null)
                    {
                        if (pu.Item2 != "heart")
                        {
                            Rectangle destRect = new Rectangle((int)pu.Item1.X, (int)pu.Item1.Y, coinTexture.Width, coinTexture.Height);
                            spriteBatch.Draw(tex, destRect, Color.White);
                        }
                        else
                        {
                            Vector2 position = pu.Item1;
                            int maxWidth = coinTexture.Width;
                            int maxHeight = coinTexture.Height;

                            float scaleX = (float)maxWidth / tex.Width;
                            float scaleY = (float)maxHeight / tex.Height;
                            float scale = Math.Min(scaleX, scaleY);

                            Vector2 size = new Vector2(tex.Width * scale, tex.Height * scale);
                            Vector2 drawPos = new Vector2(position.X + (maxWidth - size.X) / 2, position.Y + (maxHeight - size.Y) / 2);

                            spriteBatch.Draw(tex, drawPos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                        }
                    }
                }

                // Use BoldFont (small font) for score and health
                spriteBatch.DrawString(BoldFont, $"Score: {score}", new Vector2(10, 10), Color.White);
                spriteBatch.DrawString(BoldFont, $"Health: {health}", new Vector2(10, 40), Color.White);
                if (doublePoints)
                    spriteBatch.DrawString(BoldFont, $"Double Points Active!", new Vector2(10, 70), Color.Yellow);
                if (immortal)
                    spriteBatch.DrawString(BoldFont, $"Immortal Active!", new Vector2(10, 100), Color.LightBlue);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

