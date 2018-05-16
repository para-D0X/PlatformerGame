using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Collections.Generic;
using ParticleEffects;

namespace Platformer
{

    public class Game1 : Game
    {
        public static int tile = 70;
        public static float meter = tile;
        public static float gravity = meter * 9.8f * 6.0f;
        public static Vector2 maxVelocity = new Vector2(meter * 10, meter * 15);
        public static float acceleration = maxVelocity.X * 2;
        public static float friction = maxVelocity.X * 6;
        public static float jumpImpulse = meter * 1500;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player = null;

        Camera2D camera = null;
        TiledMap map = null;
        TiledMapRenderer mapRenderer = null;
        TiledMapTileLayer collisionLayer;

        SpriteFont berlinSans;
        int score = 0;
        int lives = 3;

        Texture2D heart = null;
        Texture2D splash = null;
        Texture2D pressEnter = null;
        Texture2D controls = null;
        Texture2D greyBackground = null;
        Texture2D youWon = null;
        Texture2D scoreImage = null;

        Song gameMusic;
        SoundEffect splashSound;
        SoundEffect startGameSound;
        SoundEffect killZombieSound;
        SoundEffect coinPickupSound;
        SoundEffect youWinSound;
        SoundEffect loseSound;

        SoundEffectInstance splashSoundInstance;
        SoundEffectInstance startGameSoundInstance;
        SoundEffectInstance killZombieSoundInstance;
        SoundEffectInstance coinPickupSoundInstance;
        SoundEffectInstance youWinSoundInstance;
        SoundEffectInstance loseSoundInstance;

        Rectangle GoalRec = new Rectangle(2670, 70, 110, 300);

        List<Enemy> enemies = new List<Enemy>();
        List<Coins> coins = new List<Coins>();
        Sprite coin = null;

        Texture2D backgroundTexture;
        Texture2D kermitTexture;

        Emitter fartEmitter = null;
        Texture2D fartParticle = null;
        Vector2 emitterOffset = new Vector2(25, 30);

        float Timer = 3;
        float popTimer = 1;
        bool RunOnce = false;
        bool PlayOnce = false;

        // STATES

        const int STATE_SPLASH = 0;
        const int STATE_MENU = 1;
        const int STATE_GAME = 2;
        const int STATE_WIN = 3;
        const int STATE_LOSE = 4;

        int GameState = STATE_SPLASH;

        public int ScreenWidth
        {
            get
            {
                return graphics.GraphicsDevice.Viewport.Width;
            }
        }

        public int ScreenHeight
        {
            get
            {
                return graphics.GraphicsDevice.Viewport.Height;
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            GameState = STATE_SPLASH;

            player = new Player(this);
            player.Position = new Vector2(430, 1200);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            backgroundTexture = Content.Load<Texture2D>("background1");
            kermitTexture = Content.Load<Texture2D>("kermitslurp");

            splashSound = Content.Load<SoundEffect>("startupJingle");
            startGameSound = Content.Load<SoundEffect>("gameStartSound");
            killZombieSound = Content.Load<SoundEffect>("pop");
            coinPickupSound = Content.Load<SoundEffect>("coinpickup");
            youWinSound = Content.Load<SoundEffect>("youwinsound");
            loseSound = Content.Load<SoundEffect>("losesound");

            splashSoundInstance = splashSound.CreateInstance();
            startGameSoundInstance = startGameSound.CreateInstance();
            killZombieSoundInstance = killZombieSound.CreateInstance();
            coinPickupSoundInstance = coinPickupSound.CreateInstance();
            youWinSoundInstance = youWinSound.CreateInstance();
            loseSoundInstance = loseSound.CreateInstance();

            player.Load(Content);

            fartParticle = Content.Load<Texture2D>("fartCloud");
            fartEmitter = new Emitter(fartParticle, new Vector2(-100, -100));

            berlinSans = Content.Load<SpriteFont>("berlinsansfb");
            heart = Content.Load<Texture2D>("heart");
            splash = Content.Load<Texture2D>("splash2");
            pressEnter = Content.Load<Texture2D>("pressentertoplay");
            controls = Content.Load<Texture2D>("wasdtoplay");
            greyBackground = Content.Load<Texture2D>("greybackdrop");
            youWon = Content.Load<Texture2D>("youwin");
            scoreImage = Content.Load<Texture2D>("score");


            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice,
                ScreenWidth, ScreenHeight);

            camera = new Camera2D(viewportAdapter);
            camera.Position = new Vector2(0, ScreenHeight);

            map = Content.Load<TiledMap>("test");
            mapRenderer = new TiledMapRenderer(GraphicsDevice);

            foreach (TiledMapTileLayer layer in map.TileLayers)
            {
                if (layer.Name == "Collisions")
                {
                    collisionLayer = layer;
                }
            }

            foreach (TiledMapObjectLayer layer in map.ObjectLayers)
            {
                if (layer.Name == "Enemies")
                {
                    foreach (TiledMapObject obj in layer.Objects)
                    {
                        Enemy enemy = new Enemy(this);
                        enemy.Load(Content);
                        enemy.Position = new Vector2(obj.Position.X, obj.Position.Y);
                        enemies.Add(enemy);
                    } 
                }

                if (layer.Name == "Pickups")
                {
                    foreach (TiledMapObject obj in layer.Objects)
                    {
                        Coins coin = new Coins(this);
                        coin.Load(Content);
                        coin.Position = new Vector2(obj.Position.X, obj.Position.Y);
                        coins.Add(coin);
                    }
                }
            }

            gameMusic = Content.Load<Song>("Music/Superhero_violin");


        }

        protected override void UnloadContent()
        {

        }

        private void CheckCollisions()
        {
            foreach (Enemy e in enemies)
            {
                if (IsColliding(player.Bounds, e.Bounds) == true)
                {
                    if (player.Velocity.Y > 0)
                    {
                        player.JumpOnCollision();
                        enemies.Remove(e);
                        score += 1;
                        killZombieSoundInstance.Play();
                        popTimer = 1;
                        fartEmitter.position = e.Position + emitterOffset;
                        break;
                    }
                    else if (damage == false)
                    {
                        lives --;
                        damage = true;
                    }
                }
            }
            if (IsColliding(player.Bounds, GoalRec) == true)
            {
                GameState = STATE_WIN;
            }

            foreach (Coins c in coins)
            {
                if (IsColliding(player.Bounds, c.Bounds) == true)
                {                  
                    coins.Remove(c);
                    coinPickupSoundInstance.Play();
                    score += 1;
                    break;
                   
                }
            }
            if (IsColliding(player.Bounds, GoalRec) == true)
            {
                GameState = STATE_WIN;
            }
        }

        private bool IsColliding(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.X + rect1.Width < rect2.X ||
                rect1.X > rect2.X + rect2.Width ||
                rect1.Y + rect1.Height < rect2.Y ||
                rect1.Y > rect2.Y + rect2.Height)
            {
                // these two rectangles are not colliding;
                return false;
            }
            Console.WriteLine("Something is colliding");
            return true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            player.Update(deltaTime);
            resetDamage(deltaTime);
            CheckLives();

            KeyboardState state = Keyboard.GetState();


            if (state.IsKeyDown(Keys.P)  == true)
            {
                GameState = STATE_WIN;
            }

                switch (GameState)
            {
                case STATE_SPLASH:
                    UpdateSplashState(deltaTime);
                    break;
                case STATE_MENU:
                    UpdateMenuState(deltaTime);
                    break;
                case STATE_GAME:
                    UpdateGameState(deltaTime);
                    break;
                case STATE_WIN:
                    UpdateWinState(deltaTime);
                    break;
                case STATE_LOSE:
                    UpdateLoseState(deltaTime);
                    break;
            }

            fartEmitter.Update(deltaTime);

            base.Update(gameTime);
        }

        private void UpdateSplashState(float deltaTime)
        {
            Timer -= deltaTime;

            if (Timer <= 0)
            {
                GameState = STATE_MENU;
            }

            MediaPlayer.Volume = 0.01f;

            splashSoundInstance.Volume = 0.4f;

            if (PlayOnce != true)
            {
                splashSoundInstance.Play();
                PlayOnce = true;
            }

        }

        private void DrawSplashState(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(splash, new Vector2(0, -5), null, Color.White, 0f, Vector2.Zero, 0.44f, SpriteEffects.None, 0f);
        }

        private void UpdateGameState(float deltaTime)
        {

            PlayOnce = false;


            foreach (Enemy e in enemies)
            {
                e.Update(deltaTime);
            }

            foreach (Coins c in coins)
            {
                c.Update(deltaTime);
            }

            Console.WriteLine(player.Position);

            camera.Position = player.Position - new Vector2(ScreenWidth / 2, ScreenHeight / 2);
            camera.Zoom = 1f;

            CheckCollisions();

            killZombieSoundInstance.Volume = 0.2f;
            coinPickupSoundInstance.Volume = 0.2f;
            splashSoundInstance.Volume = 0.5f;
            startGameSoundInstance.Volume = 0.5f;
            youWinSoundInstance.Volume = 0.4f;
            loseSoundInstance.Volume = 0.6f;

            MediaPlayer.Volume = 0.01f;

            popTimer -= deltaTime;

            if (popTimer >= 0)
            {
                fartEmitter.emissionRate = 50;
                fartEmitter.transparency = 0.9f;
                fartEmitter.minSize = 30;
                fartEmitter.maxSize = 40;
                fartEmitter.maxLife = 1.5f;
            }
            else
            {
                fartEmitter.position = new Vector2(-100, -100);
            }
        }

        private void DrawGameState(SpriteBatch spriteBatch)
        {

            int tileWidth = (graphics.GraphicsDevice.Viewport.Width / backgroundTexture.Width) + 1;
            int tileHeight = (graphics.GraphicsDevice.Viewport.Height / backgroundTexture.Height) + 1;

            for (int column = 0; column < tileWidth; column += 1)
            {
                for (int row = 0; row < tileHeight; row += 1)
                {
                    Vector2 position = new Vector2(column * backgroundTexture.Width, row * backgroundTexture.Height);

                    spriteBatch.Draw(backgroundTexture, position, Color.White);
                }
            }

            spriteBatch.End();

            var viewMatrix = camera.GetViewMatrix();
            var projectionMatrix = Matrix.CreateOrthographicOffCenter(0,
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0f, -1f);

            spriteBatch.Begin(transformMatrix: viewMatrix);
            mapRenderer.Draw(map, ref viewMatrix, ref projectionMatrix);
            player.Draw(spriteBatch);

            //spriteBatch.DrawRectangle(GoalRec, Color.Red, 1f);


            foreach (Enemy e in enemies)
            {
                e.Draw(spriteBatch);
            }

            foreach (Coins c in coins)
            {
                c.Draw(spriteBatch);
            }


            fartEmitter.Draw(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.Draw(scoreImage, new Vector2(0, 0), null, Color.White, 0f, Vector2.Zero, 0.15f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(berlinSans, score.ToString(), new Vector2(120, 20), Color.DarkOliveGreen);

            for (int i = 0; i < lives; i++)
            {
                spriteBatch.Draw(heart, new Vector2(ScreenWidth - 80 - i * 55, 20), Color.White);
            }
        }

        private void UpdateMenuState(float deltaTime)
        {
            if (PlayOnce != false)
            {
                PlayOnce = false;
            }

            MediaPlayer.Play(gameMusic);
            MediaPlayer.Volume = 0; // DO NOT CHANGE, MEGA DEAF IF YOU DO

            KeyboardState state = Keyboard.GetState();

            startGameSoundInstance.Volume = 0.5f;

            if (state.IsKeyDown(Keys.Enter) == true)
            {
                GameState = STATE_GAME;
                if (PlayOnce != true)
                {
                    startGameSound.Play();
                    PlayOnce = true;
                }
            }
        }

        private void DrawMenuState(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(greyBackground, new Vector2(0, 0), null, Color.White, 0f, Vector2.Zero, 0.44f, SpriteEffects.None, 0f);

            spriteBatch.Draw(pressEnter, new Vector2(ScreenWidth / 5, ScreenHeight / 3), null, Color.White, 0f, Vector2.Zero, 0.44f, SpriteEffects.None, 0f);

            spriteBatch.Draw(controls, new Vector2(0, 300), null, Color.White, 0f, Vector2.Zero, 0.44f, SpriteEffects.None, 0f);
        }

        private void UpdateWinState(float deltaTime)
        {
            MediaPlayer.Stop();

            if (PlayOnce != true)
            {
                youWinSoundInstance.Play();
                PlayOnce = true;
            }

        }

        private void DrawWinState(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Draw(greyBackground, new Vector2(0, 0), null, Color.White, 0f, Vector2.Zero, 0.44f, SpriteEffects.None, 0f);

            spriteBatch.Draw(youWon, new Vector2(ScreenWidth / 7, ScreenHeight / 4), null, Color.White, 0f, Vector2.Zero, 0.44f, SpriteEffects.None, 0f);

        }

        private void UpdateLoseState(float deltaTime)
        {
            MediaPlayer.Stop();

            if (PlayOnce != true)
            {
                loseSoundInstance.Play();
                PlayOnce = true;
            }

        }

        private void DrawLoseState(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.DrawString(berlinSans, "You died dead, FeelsBadMan", new Vector2(ScreenWidth / 3, ScreenHeight / 2), Color.White);

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            switch (GameState)
            {
                case STATE_SPLASH:
                    DrawSplashState(spriteBatch);
                    break;
                case STATE_MENU:
                    DrawMenuState(spriteBatch);
                    break;
                case STATE_GAME:
                    DrawGameState(spriteBatch);
                    break;
                case STATE_WIN:
                    DrawWinState(spriteBatch);
                    break;
                case STATE_LOSE:
                    DrawLoseState(spriteBatch);
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void CheckLives()
        {
            if (lives <= 0)
            {
                GameState = STATE_LOSE;
                MediaPlayer.Stop();
            }
        }

        private bool damage;

        private float resetTime = 1;

        private void resetDamage(float deltaTime)
        {
            if (damage == true)
            {
                resetTime -= deltaTime;
                if (resetTime <= 0)
                {
                    damage = false;
                    resetTime = 1;
                }
            }
        }





        public int PixelToTile(float pixelCoord)
        {
            return (int)Math.Floor(pixelCoord / tile);
        }

        public int TileToPixel(int tileCoord)
        {
            return tile * tileCoord;
        }

        public int CellAtPixelCoord(Vector2 pixelCoords)
        {
            if (pixelCoords.X < 0 || pixelCoords.X > map.WidthInPixels || pixelCoords.Y < 0)
            {
                return 1;
            }

            if (pixelCoords.Y > map.HeightInPixels)
            {
                return 0;
            }

            return CellAtTileCoord(PixelToTile(pixelCoords.X), PixelToTile(pixelCoords.Y));
        }

        public int CellAtTileCoord(int tx, int ty)
        {
            if (tx < 0 || tx >= map.Width || ty < 0)
            {
                return 1;
            }

            if (ty >= map.Height)
            {
                return 0;
            }

            TiledMapTile? tile;
            collisionLayer.TryGetTile(tx, ty, out tile);
            return tile.Value.GlobalIdentifier;
        }
    }
}