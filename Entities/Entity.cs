using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoxxiWaiting.Components;

namespace RoxxiWaiting.Entities
{
    public abstract class Entity
    {
        public Sprite Sprite;
        public Vector2 Position;
        public float MovementSpeed;
        public Vector2 Movement;

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}