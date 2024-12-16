using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoxxiWaiting.Components;


namespace RoxxiWaiting.Entities
{
    public class Roxxi : Entity
    {   
        public Roxxi(Texture2D mainAltas, Vector2 initalPosition)

        {
            Position = initalPosition; 
            Sprite = new Sprite(mainAltas, 105, 90, 85, 90, Position);
            Sprite.Layer = 0.4f;
            MovementSpeed = 200f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Sprite.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds * MovementSpeed;

            Movement = Vector2.Zero;

            if (state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.Left))
            {
                if( Sprite.Position.X + Movement.X <= -10)
                {
                    Movement.X = Movement.X + 0f;
                }else
                {
                    Movement.X -= delta;
                }
            } 
            
            if (state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.Right))
            {
                if (Sprite.Position.X + Movement.X >= 1330)
                {
                    Movement.X = Movement.X + 0f;
                }else
                {
                    Movement.X += delta;
                }
        
            } 
   

            // Flip e rotação
            if (Movement.X != 0)
            {
                Sprite.Rotation = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10) * (Movement.X > 0 ? 0.1f : -0.1f);
                Sprite.SpriteEffects_Flip = Movement.X > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
  
            }
            else
            {
                Sprite.Rotation = 0f;
            }

            Sprite.Position += Movement;
        }
    }
}