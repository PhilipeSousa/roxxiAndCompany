using System;
using System.Collections.Generic;
using System.Linq;
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
    private const string FONT_FREE_MONO = "FreeMono";
    private Texture2D _graphicsAltas;
    public const int WIDTH_WINDOW = 1565 - 192;
    public const int HEIGHT_WINDOW = 324;
    private Sprite _background;
    private Vector2 _backgroundPostion;

    private Sprite _cat;
    private Vector2 _catPosition;

    private Roxxi _roxxi;
    private Vector2 _roxxiPosition;

    private Sprite _scythe;
    private  bool _isAttacking;
    private float _attackDuration;
    private float _attackTimer;
    private float _rotationCooldown;

    private Vector2 _offsetNoneFlipScythe;
    private Vector2 _offsetHoriFlipScythe;

    //private Enemy enemy1;
    //private Enemy enemy2;

    private Texture2D _collisionTexture;

    private Rectangle _scytheHitbox;

    private bool _hit;

    private float _durationHitboxScythe;
    private float _attackScytheTimerHitbox;
    private float _deathTimer;
    private float _deathTimerDuration;
    private bool _playAnimDeathEnemy;

    private static Random _random;

    private List<Enemy> _enemies;

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

        _isAttacking = false;
        _attackDuration = 0.5f;
        _attackTimer = 0f;
        _rotationCooldown = 0f;

        _offsetNoneFlipScythe = new Vector2(-5, 80);
        _offsetHoriFlipScythe = new Vector2(90, 80); 

        //enemy1 = new Enemy(_graphicsAltas, new Vector2(100, 274), 0.4f, SpriteEffects.None);
        //enemy2 = new Enemy(_graphicsAltas, new Vector2(1200, 274), 0.6f, SpriteEffects.FlipHorizontally);

        _collisionTexture = new Texture2D(GraphicsDevice, 1, 1);
        //_collisionTexture.SetData(new[] { Color.Red });

        _hit = false;
        _durationHitboxScythe = 0.02f; // menos que isso vai dar merda
        _attackScytheTimerHitbox = 0f;
        _playAnimDeathEnemy = false;

        _deathTimer = 0f;
        _deathTimerDuration = 3f;

         _random = new Random();

         _enemies = new List<Enemy>();

         _enemies.Add(new Enemy(_graphicsAltas, new Vector2(100, 274), 0.4f, SpriteEffects.None));
         _enemies.Add(new Enemy(_graphicsAltas, new Vector2(1200, 274), 0.6f, SpriteEffects.FlipHorizontally));



    }
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        KeyboardManager.Update();

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (KeyboardManager.HasBeenPressed(Keys.Space) && !_isAttacking) 
        {
            _isAttacking = true;
            _hit = true;
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
            _rotationCooldown -= deltaTime; 
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

    
            if (_attackTimer <= 0f)
                _isAttacking = false;
            if (_attackScytheTimerHitbox <= 0f)
                _hit = false;
        }

        _roxxi.Update(gameTime);


        if (_hit){
            foreach (var enemy in _enemies)
            {
                if (_scytheHitbox.Intersects(enemy.Sprite.HitBox))
                {
                    Console.WriteLine("Hitou o inimigo");
                    enemy.IsDeath = true;
                    _playAnimDeathEnemy = true;
                    enemy.Sprite.color = Color.Red;
                    enemy.MyDeathEffect = 2;
                    enemy.DeathEffect(gameTime);
                    _deathTimer = _deathTimerDuration;
                }
            }
            
        }
        if(_playAnimDeathEnemy) {
            _deathTimer -= deltaTime;}

        foreach (var enemy in _enemies.ToList())
        {
            if (enemy.IsDeath)
            {
                if (_deathTimer <= 0f)
                {
                    _enemies.Remove(enemy); 
                }
                else
                {
                    enemy.DeathEffect(gameTime); 
                }
            }
            else
            {
                enemy.Update(gameTime); // Atualizar inimigo vivo
            }
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

        _spriteBatch.Draw(
                _collisionTexture, 
                _scytheHitbox, 
                null,                 
                Color.Black, 
                0f,                   
                Vector2.Zero,         
                SpriteEffects.None,   
                1f                    
            );
        
        foreach (var item in _enemies)
        {
            item.Draw(_spriteBatch);
        }
  
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}