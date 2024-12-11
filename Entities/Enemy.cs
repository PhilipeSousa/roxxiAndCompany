using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoxxiWaiting.Components;


namespace RoxxiWaiting.Entities
{
    public class Enemy : Entity

    {
        private const double CHANCE_TO_FLIP = 0.0025; 
        private const double CHANCE_TO_STOP = 0.0015;     
        private bool _isPaused = false;      
        private float _pauseTimer = 0f;   
        private float _MAXpauseDuration = 3f;
        private float _moveDelta;
        public float RightlimitTotheEnemy = -100f;
        public float LeftlimitTotheEnemy = 1800f;
        private Vector2 _auxPos;
        private Random _random = new Random();
        

        public Enemy(Texture2D mainAtlas, Vector2 position, float layer, SpriteEffects spriteEffectsflip)
        {
            Position = position;
            Sprite = new Sprite(mainAtlas, 123, 180, 65, 52, Position);
            Sprite.Layer = layer;
            MovementSpeed = 100f;
            Sprite.SpriteEffects_Flip = spriteEffectsflip;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Sprite.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
          float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_isPaused)
            {
                _pauseTimer -= delta;
                if (_pauseTimer <= 0) _isPaused = false; 
                return;
            }

            _moveDelta = delta * MovementSpeed;
            _auxPos = Vector2.Zero;

            if (Sprite.SpriteEffects_Flip == SpriteEffects.None)
            {
                _auxPos.X += _moveDelta;
                if (Sprite.Position.X + _auxPos.X > LeftlimitTotheEnemy || RandomFlip())
                    FlipDirection();

            }
            else
            {
                _auxPos.X -= _moveDelta;
                if (Sprite.Position.X + _auxPos.X < RightlimitTotheEnemy || RandomFlip())
                    FlipDirection();
                
            }

            Sprite.Position += _auxPos;
            Sprite.Rotation = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10) * 
                            (_auxPos.X > 0 ? 0.1f : -0.1f);

            if (RareRandomPause()) 
                    StartPause();
            
        }

        private void StartPause()
        {
            _isPaused = true;
            _pauseTimer = (float)_random.NextDouble() * _MAXpauseDuration; 
        }

        private void FlipDirection()
        {
            Sprite.SpriteEffects_Flip = Sprite.SpriteEffects_Flip == SpriteEffects.None 
                ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        private bool RareRandomPause()
        {
            return _random.NextDouble() < CHANCE_TO_STOP; 
        }


        private bool RandomFlip()
        {
            return _random.NextDouble() < CHANCE_TO_FLIP; 
        }

    }
}