using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoxxiWaiting.Components;
using RoxxiWaiting.Entities;

namespace RoxxiWaiting;

public class RoxxiWaitingGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private const string ASSETS_GRAPHICS_NAME = "F_Grafics";
    private Texture2D _graphicsAltas;

    public const int WIDTH_WINDOW = 1565 - 192;
    public const int HEIGHT_WINDOW = 324;

    private const float INITIAL_SPAWN_DELAY = 3f; 
    private const float MIN_SPAWN_DELAY = 0.01f;  
    private const float SPAWN_ACCELERATION = 0.2f; 
    private const int MAX_ACTIVE_ENEMIES = 50;    
    private float _currentSpawnDelay = INITIAL_SPAWN_DELAY; 


    private float _elapsedTime = 0f; 
    private const float SPAWN_DELAY = 3f; 

    private float _lastAttackTime = 0f;

   
    private float _rotationCooldown = 0f; 

    private Rectangle _scytheHitbox;

    // BackGround
    private Sprite _background;
    private Vector2 _backgroundPostion;

    //Cat
    private Sprite _cat;
    private Vector2 _catPosition;

    //Roxxi
    private Roxxi _roxxi;
    private Vector2 _roxxiPosition;

    // Weapon for roxxi
    private Sprite _scythe;
    private Vector2 offsetNomeFlipFoice = new Vector2(-5, 80);
    private Vector2 offsetHoriFlipFoice = new Vector2(90, 80);

    private Queue<Enemy> _enemyPool; 
    private List<Enemy> _activeEnemies; 
    private Texture2D _collisionTexture;

    private bool _isAttacking = false;
    private float _attackDuration = 0.5f; 
    private float _durationHitboxScythe = 0.02f;
    private float _attackTimer = 0f;
    private float _attackScytheTimerHitbox = 0f;
    private bool _hitting = false;

    public RoxxiWaitingGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        _graphics.PreferredBackBufferHeight = HEIGHT_WINDOW;
        _graphics.PreferredBackBufferWidth = WIDTH_WINDOW;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _collisionTexture = new Texture2D(GraphicsDevice, 1, 1);
        _collisionTexture.SetData(new[] { Color.Red });

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _graphicsAltas = Content.Load<Texture2D>(ASSETS_GRAPHICS_NAME);

        _backgroundPostion = new Vector2(-192, 0);
        _background = new Sprite(_graphicsAltas, 0, 0, _graphicsAltas.Width, _graphicsAltas.Height, _backgroundPostion);
        _background.Layer = 0f;

        _catPosition = new Vector2(880, 132);
        _cat = new Sprite(_graphicsAltas, 0, 130, 105, 194, _catPosition);
        _cat.Layer = 0.5f;

        _roxxiPosition = new Vector2(800, 234);
        _roxxi = new Roxxi(_graphicsAltas, _roxxiPosition);

        _scythe = new Sprite(_graphicsAltas, 128, 231, 60, 92, Vector2.Zero);
        _scythe.Layer = 0.5f;
        _scythe.CenterOfRotation = new Vector2(_scythe.RectangleSprite.Width / 2, _scythe.RectangleSprite.Height);

        _enemyPool =  new Queue<Enemy>();
        _activeEnemies = new List<Enemy>();

        for (int i = 0; i < 100; i++)
        {
            bool fromLeft = (i % 2 == 0);
            bool layerChangerLogc = (i % 3 == 0);

            float initialX = fromLeft ? 100 : 1200;
            float offsetX = i * 150;

            Vector2 position = new Vector2(initialX, 270);

            position.X += fromLeft ? -offsetX : offsetX;

            SpriteEffects effect = fromLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float enemyLayer = layerChangerLogc ? 0.3f : 0.6f;

            var enemy = new Enemy(_graphicsAltas, position, enemyLayer, effect);
            _enemyPool.Enqueue(enemy); 
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

      
        KeyboardManager.Update();

        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        _roxxi.Update(gameTime);

        if (KeyboardManager.HasBeenPressed(Keys.Space) && !_isAttacking) 
        {
            _isAttacking = true;
            _hitting = true;
            _attackTimer = _attackDuration;
            _attackScytheTimerHitbox = _durationHitboxScythe;

            if (_roxxi.Sprite.SpriteEffects_Flip == SpriteEffects.None)
            {
                _scythe.Rotation = -1.5f; 
            }
            else
            {
                _scythe.Rotation = 1.5f; 
            }

            _lastAttackTime = _elapsedTime; 
            _rotationCooldown = 0.5f; 
        }

        if (_isAttacking)
        {
            _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;


            // time da hitbox sempre será menor do que o da animação de ataque
            _attackScytheTimerHitbox -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(_attackScytheTimerHitbox <= 0f){
                _hitting = false;
            }
            if (_attackTimer <= 0f)
            {
                _isAttacking = false;
            }
        }

        // Transição suave 
        if (_rotationCooldown > 0)
        {
            _scythe.Rotation = MathHelper.Lerp(_scythe.Rotation, 0f, 0.1f); // Lerp suaviza o retorno
            _rotationCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds; // Decrementa o cooldown
        }

 
        _scythe.SpriteEffects_Flip = _roxxi.Sprite.SpriteEffects_Flip;

        if (_scythe.SpriteEffects_Flip == SpriteEffects.FlipHorizontally)
        {
            _scytheHitbox = new Rectangle(
                (int)(_roxxi.Sprite.Position.X + 160), 
                (int)_roxxi.Sprite.Position.Y + 70,
                4,
                30);

            _scythe.Position = _roxxi.Sprite.Position + offsetHoriFlipFoice;
        }
        else
        {
            _scytheHitbox = new Rectangle(
                (int)(_roxxi.Sprite.Position.X - 84), 
                (int)_roxxi.Sprite.Position.Y + 70,
                4,
                30
            );

            _scythe.Position = _roxxi.Sprite.Position + offsetNomeFlipFoice;
        }

      
        List<Enemy> enemiesToRemove = new List<Enemy>();

   
        if (_isAttacking)
        {
            if(_hitting)
            {
                foreach (var enemy in _activeEnemies)
                {
                    
                    if (_scytheHitbox.Intersects(enemy.Sprite.GetCollisionRectangle()))
                    {
                        Console.WriteLine($"Hit! Enemy at position {enemy.Sprite.Position} removed.");
                        _activeEnemies.Remove(enemy); 
                        _enemyPool.Enqueue(enemy);    
                        break; 
                    }
                }
            } 
        }


        // Spawn de inimigos
        if (_elapsedTime > _currentSpawnDelay && _activeEnemies.Count < MAX_ACTIVE_ENEMIES)
        {
            if (_enemyPool.Count > 0)
            {
                var enemy = _enemyPool.Dequeue(); 
                _activeEnemies.Add(enemy);       
                _elapsedTime = 0f;               

                
                _currentSpawnDelay = Math.Max(_currentSpawnDelay - SPAWN_ACCELERATION, MIN_SPAWN_DELAY);
            }
        }

        
        foreach (var item in _activeEnemies)
        {
            item.Update(gameTime);
        }

        base.Update(gameTime);
    }



    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

        _background.Draw(_spriteBatch);
        _roxxi.Draw(_spriteBatch);
        _cat.Draw(_spriteBatch);
        _scythe.Draw(_spriteBatch);

        foreach (var item in _activeEnemies)
        {
            item.Draw(_spriteBatch);

            _spriteBatch.Draw(
                _collisionTexture, 
                item.Sprite.GetCollisionRectangle(), 
                Color.White * 0.5f
            );
        }

        _spriteBatch.Draw(
            _collisionTexture,
            _scytheHitbox,
            Color.Blue
        );

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
