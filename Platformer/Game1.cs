using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Collections.Generic;

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

        Song gameMusic;

        List<Enemy> enemies = new List<Enemy>();
       // Sprite goal = null;
        Sprite coin = null;

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

        Texture2D backgroundTexture;
        Texture2D kermitTexture;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }



        protected override void Initialize()
        {
            player = new Player(this);
            player.Position = new Vector2(0, 0); //(360, 1200) wanted spawn location
            base.Initialize();
        }


       
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            backgroundTexture = Content.Load<Texture2D>("background1");
            kermitTexture = Content.Load<Texture2D>("kermitslurp");

            player.Load(Content);

            berlinSans = Content.Load<SpriteFont>("berlinsansfb");
            heart = Content.Load<Texture2D>("heart");

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

                /*if (layer.Name == "Goal")
                {
                    TiledMapObject obj = layer.Objects[0];
                    if (obj != null)
                    {
                        AnimatedTexture anim = new AnimatedTexture(Vector2.Zero, 0, 1, 1);
                        anim.Load(Content, "key", 1, 1);
                        goal = new Sprite();
                        goal.Add(anim, 0, 5);
                        goal.position = new Vector2(obj.Position.X, obj.Position.Y);
                    }
                } */

                if (layer.Name == "Pickups")
                {
                    TiledMapObject obj = layer.Objects[0];

                    if (obj != null)
                    {
                        AnimatedTexture anim = new AnimatedTexture(Vector2.Zero, 0, 1, 1);
                        anim.Load(Content, "coin", 1, 1);

                        coin = new Sprite();
                        coin.Add(anim, 0, 5);
                        coin.position = new Vector2(obj.Position.X, obj.Position.Y);
                    }
                }

            }

            gameMusic = Content.Load<Song>("Music/Superhero_violin");
            MediaPlayer.Play(gameMusic);

        }   

       

        protected override void UnloadContent()
        {
            
        }


      
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            player.Update(deltaTime);

            foreach (Enemy e in enemies)
            {
                e.Update(deltaTime);
            }

            camera.Position = player.Position - new Vector2(ScreenWidth / 2, ScreenHeight / 2);

            camera.Zoom = 1f;

            base.Update(gameTime);

            MediaPlayer.Volume = 0.01f;

        }


       
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

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

            foreach (Enemy e in enemies)
            {
                e.Draw(spriteBatch);
            }
            coin.Draw(spriteBatch); // YES

            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(berlinSans, "Score: " + score.ToString(), new Vector2(20, 20), Color.DarkOrange);

            for (int i = 0; i < lives; i++)
            {
                spriteBatch.Draw(heart, new Vector2(ScreenWidth - 80 - i * 55, 20), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
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
