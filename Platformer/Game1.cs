using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using MonoGame.Extended.ViewportAdapters;
using System;

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
            player.Position = new Vector2(0, 0);
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

            camera.Position = player.Position - new Vector2(ScreenWidth / 2, ScreenHeight / 2);

            camera.Zoom = 1f;

            base.Update(gameTime);
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
