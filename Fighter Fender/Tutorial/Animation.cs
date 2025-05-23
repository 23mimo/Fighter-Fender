using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace Tutorial
{
    public class Animation
    {
        private List<Texture2D> frames;
        private float frameDelay;
        private float timer;
        private int currentFrame;

        public Animation(List<Texture2D> frames, float frameDelay)
        {
            this.frames = frames;
            this.frameDelay = frameDelay;
            this.timer = 0f;
            this.currentFrame = 0;
        }

        public List<Texture2D> Frames => frames;
        public float FrameDelay => frameDelay;
        public int FrameCount => frames.Count;

        public Texture2D CurrentTexture
        {
            get
            {
                if (frames == null || frames.Count == 0)
                    return null;
                return frames[currentFrame];
            }
        }

        public void Update(GameTime gameTime)
        {
            if (frames == null || frames.Count == 0)
                return;

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= frameDelay)
            {
                timer -= frameDelay;
                currentFrame = (currentFrame + 1) % frames.Count;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle destination, Color color)
        {
            if (CurrentTexture != null)
                spriteBatch.Draw(CurrentTexture, destination, color);
        }

        public static List<Texture2D> LoadFrames(ContentManager content, string basePath, int count)
        {
            var frames = new List<Texture2D>();
            for (int i = 0; i < count; i++)
            {
                string path = $"{basePath}{i}.png";
                if (File.Exists(content.RootDirectory + "/" + path))
                {
                    frames.Add(content.Load<Texture2D>(path));
                }
            }
            return frames;
        }
    }
}
