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

    private float _elapsedTime; 

    private Rectangle _scytheHitbox;

    private Sprite _background;
    private Vector2 _backgroundPostion;

    private Sprite _cat;
    private Vector2 _catPosition;

    private Roxxi _roxxi;
    private Vector2 _roxxiPosition;

    private Sprite _scythe;
    private Vector2 _offsetNoneFlipScythe;
    private Vector2 _offsetHoriFlipScythe;

    private Queue<Enemy> _enemyPool; 
    private List<Enemy> _activeEnemies;

    private List<Enemy> enemiesToRemoveNow;

    private Texture2D _collisionTexture;

    private bool _isAttacking = false;
    private float _attackDuration = 0.5f; 
    private float _durationHitboxScythe = 0.02f;
    private float _attackTimer = 0f;
    private float _attackScytheTimerHitbox = 0f;
    private bool _hitting = false;
    private float _rotationCooldown = 0f; 

    private bool _debugMode = false;
    
    private Dictionary<Enemy, float> _enemiesPendingRemoval = new Dictionary<Enemy, float>();

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
        _offsetNoneFlipScythe = new Vector2(-5, 80);
        _offsetHoriFlipScythe = new Vector2(90, 80);
        
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

        _enemyPool = new Queue<Enemy>();
        _activeEnemies = new List<Enemy>();

        enemiesToRemoveNow = new List<Enemy>();

        for (int i = 0; i < 100; i++)
        {
            bool fromLeft = (i % 2 == 0);
            bool layerChangerLogic = (i % 3 == 0);

            float initialX = fromLeft ? 100 : 1200;
            float offsetX = i * 150;

            Vector2 position = new Vector2(initialX, 270);
            position.X += fromLeft ? -offsetX : offsetX;

            SpriteEffects effect = fromLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float enemyLayer = layerChangerLogic ? 0.3f : 0.6f;

            var enemy = new Enemy(_graphicsAltas, position, enemyLayer, effect);
            _enemyPool.Enqueue(enemy); 
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        KeyboardManager.Update();

        if(KeyboardManager.HasBeenPressed(Keys.B)){
            _debugMode = !_debugMode;
        }

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _elapsedTime += deltaTime;

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

            _rotationCooldown = 0.5f; 
        }

         if (_rotationCooldown > 0)
        {
            _scythe.Rotation = MathHelper.Lerp(_scythe.Rotation, 0f, 0.1f); 
            _rotationCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds; 
        }

 
        _scythe.SpriteEffects_Flip = _roxxi.Sprite.SpriteEffects_Flip;

        if (_scythe.SpriteEffects_Flip == SpriteEffects.FlipHorizontally)
        {

            _scytheHitbox.X = (int)(_roxxi.Sprite.Position.X + 160);
            _scytheHitbox.Y = (int)_roxxi.Sprite.Position.Y + 70;

        }
        else
        {
            _scytheHitbox.X = (int)(_roxxi.Sprite.Position.X - 84);
            _scytheHitbox.Y = (int)_roxxi.Sprite.Position.Y + 70;
        }

        _scytheHitbox.Width = 4;
        _scytheHitbox.Height = 30;

        _scythe.Position = _roxxi.Sprite.Position + 
                        (_scythe.SpriteEffects_Flip == SpriteEffects.FlipHorizontally ? _offsetHoriFlipScythe : _offsetNoneFlipScythe);


        if (_isAttacking)
        {
            _attackTimer -= deltaTime;
            _attackScytheTimerHitbox -= deltaTime;

            if (_attackScytheTimerHitbox <= 0f)
                _hitting = false;

            if (_attackTimer <= 0f)
                _isAttacking = false;
        }

        // Detect hits
        if (_hitting)
        {
            foreach (var enemy in _activeEnemies)
            {
                if (_scytheHitbox.Intersects(enemy.Sprite.GetCollisionRectangle()) && !_enemiesPendingRemoval.ContainsKey(enemy))
                {
                     enemy.IsDeath = true;
                     enemy.Sprite.color = Color.Red;
                     enemy.MyDeathEffect = new Random().Next(1, 4);
                    _enemiesPendingRemoval[enemy] = 1f; 
                }
            }
        }

        // Process pending removals
        foreach (var kvp in _enemiesPendingRemoval)
        {
            var enemy = kvp.Key;

            enemy.DeathEffect(gameTime);
            _enemiesPendingRemoval[kvp.Key] -= deltaTime;
            if (_enemiesPendingRemoval[kvp.Key] <= 0)
                enemiesToRemoveNow.Add(kvp.Key);
        }

        foreach (var enemy in enemiesToRemoveNow)
        {
            _activeEnemies.Remove(enemy);
            _enemyPool.Enqueue(enemy);
            _enemiesPendingRemoval.Remove(enemy);
        }

        _roxxi.Update(gameTime);
        foreach (var enemy in _activeEnemies)
        {
            enemy.Update(gameTime);
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



        foreach (var enemy in _activeEnemies)
        {
            enemy.Draw(_spriteBatch);

            if(_debugMode){
                _spriteBatch.Draw(
                _collisionTexture,
                enemy.Sprite.GetCollisionRectangle(),
                Color.White * 0.5f
            );
            }
   
        }

        if (_debugMode)
        {
            _spriteBatch.Draw(
            _collisionTexture,
            _scytheHitbox,
            Color.Blue * 0.5f
        );
        } 


        _spriteBatch.End();
        base.Draw(gameTime);
    }
}