using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer
{
    class Player
    {
        public Vector2 position = Vector2.Zero;

        Sprite sprite = new Sprite();
        bool hFlipped = false;

        public Player()
        {

        }

        public void Load(ContentManager content)
        {
            sprite.Load(content, "playerIdle");
        }

        public void Update(float deltaTime)
        {
            KeyboardState state = Keyboard.GetState();
            int speed = 250;

            if(state.IsKeyDown(Keys.W) == true)
            {
                position.Y -= speed * deltaTime;
            }
            if (state.IsKeyDown(Keys.A) == true)
            {
                position.X -= speed * deltaTime;
                hFlipped = true;
            }
            if (state.IsKeyDown(Keys.S) == true)
            {
                position.Y += speed * deltaTime;
            }
            if (state.IsKeyDown(Keys.D) == true)
            {
                position.X += speed * deltaTime;
                hFlipped = false;
            }

            sprite.Update(deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (hFlipped == true)
                sprite.Draw(spriteBatch, position, SpriteEffects.FlipHorizontally);
            else
                sprite.Draw(spriteBatch, position);
        }






    }
}
