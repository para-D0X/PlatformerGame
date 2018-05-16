using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    class Coins
    {
        Sprite sprite = new Sprite();

        Game1 game = null;
        Vector2 velocity = Vector2.Zero;

        float pause = 0;

        public Vector2 Position
        {
            get
            {
                return sprite.position;
            }
            set
            {
                sprite.position = value;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return sprite.Bounds;
            }
        }

        public Coins(Game1 game)
        {
            this.game = game;
            velocity = Vector2.Zero;
        }

        public void Load(ContentManager content)
        {
            AnimatedTexture animation = new AnimatedTexture(Vector2.Zero, 0, 1, 1);
            animation.Load(content, "coin", 1, 1);

            sprite.Add(animation, 0, 0); 
        }


        public void Update(float deltaTime)
        {
            sprite.Update(deltaTime);
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch);
        }

    }
}
