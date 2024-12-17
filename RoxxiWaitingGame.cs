using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public const int WIDTH_WINDOW = 1373;
    public const int HEIGHT_WINDOW = 324;
    public Vector2 SCORE_POSITION = new Vector2(WIDTH_WINDOW/2.2f, 10);
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
    private Texture2D _collisionTexture;
    private Rectangle _scytheHitbox;
    private bool _hit;
    private SpriteFont _freeMonoFont;
    private float _durationHitboxScythe;
    private float _attackScytheTimerHitbox;
    private float _deathTimer;
    private float _deathTimerDuration;
    private bool _playAnimDeathEnemy;
    private int _score;
    private bool fromLeftReset;
    private float baseX;
    private Vector2 newPositionResetEnemy;

    private static Random _random;

    private List<Enemy> _enemies;

    private bool _isLoadingComplete;       // Flag para indicar se o carregamento terminou
    private Sprite _splashScreen;       // Tela inicial do desenvolvedor
    private Thread _loadingThread;         // Thread para carregar recursos em paralelo
    private float _splashTimer;            // Timer para controlar a duração da splash screen
    private float _splashDuration = 3f;    // Duração da splash screen (3 segundos)


    public RoxxiWaitingGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        
        _graphics.PreferredBackBufferWidth = WIDTH_WINDOW;
        _graphics.PreferredBackBufferHeight = HEIGHT_WINDOW;
        
        _graphics.ApplyChanges();
        _graphics.IsFullScreen = false;
        Window.AllowUserResizing = false;
        Window.Title = "Roxxi Waiting";  
       
    }

    protected override void LoadContent()
    {   
        _splashScreen = new Sprite(_graphicsAltas, 0, 0, 70, 70, new Vector2(WIDTH_WINDOW/2, HEIGHT_WINDOW/2));
        _splashScreen.Layer = 0f;

        // Inicialize o flag e o timer
        _isLoadingComplete = false;
        _splashTimer = 0f;

        // Inicie uma thread para carregar os recursos do jogo em segundo plano
        _loadingThread = new Thread(LoadGameContent);
        _loadingThread.Start();
    }

    private void LoadGameContent()
    {
        // Simule um carregamento pesado (coloque um delay fictício para testes)
        Thread.Sleep(2000);
         _spriteBatch = new SpriteBatch(GraphicsDevice);
        _graphicsAltas = Content.Load<Texture2D>(ASSETS_GRAPHICS_NAME);
        _freeMonoFont = Content.Load<SpriteFont>(FONT_FREE_MONO);

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

        _collisionTexture = new Texture2D(GraphicsDevice, 1, 1);

        _hit = false;
        _durationHitboxScythe = 0.02f; // menos que isso vai dar merda
        _attackScytheTimerHitbox = 0f;
        _playAnimDeathEnemy = false;

        _deathTimer = 0f;
        _deathTimerDuration = 3f;

        _random = new Random();
        _enemies = new List<Enemy>();

        SpawnEnemies();

        fromLeftReset = (new Random().Next(0, 2) == 0);
        baseX = fromLeftReset ? -100 : 1550;
        newPositionResetEnemy = new Vector2(baseX, 270);

        // Outras inicializações
        _isLoadingComplete = true; 
}


    private void SpawnEnemies()
    {
        for (int i = 0; i < 111; i++)
        {
            bool fromLeft = (i % 2 == 0);
            bool layerChangerLogic = (i % 3 == 0);

            float initialX = fromLeft ? -50 : 1400;
            float offsetX = i * 150;

            Vector2 position = new Vector2(initialX, 270);
            position.X += fromLeft ? -offsetX : offsetX;

            SpriteEffects effect = fromLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float enemyLayer = layerChangerLogic ? 0.3f : 0.6f;

            var enemy = new Enemy(_graphicsAltas, position, enemyLayer, effect);
            _enemies.Add(enemy);
        }
    }

    private void Score()
    {
        _score++;
    }

    private void ResetEnemy(Enemy enemy)
    {

        enemy.Sprite.Position = newPositionResetEnemy;
        enemy.Sprite.SpriteEffects_Flip = fromLeftReset ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        enemy._isPaused = false;
        enemy._pauseTimer = 0f;
        enemy.IsDeath = false;
        enemy.MyDeathEffect = -1;
        enemy.Sprite.color = Color.White; 

        _enemies.Add(enemy);
    }


    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        KeyboardManager.Update();

        if (!_isLoadingComplete)
        {
            // Enquanto os recursos não foram carregados, atualize o timer da splash screen
            _splashTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_splashTimer >= _splashDuration)
            {
                _splashTimer = _splashDuration;
            }
        }
        else
        {
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

        
                if (_attackTimer <= 0f) _isAttacking = false;
                if (_attackScytheTimerHitbox <= 0f) _hit = false;
            }

            _roxxi.Update(gameTime);


            if (_hit){
                foreach( var enemy in _enemies.ToList()){
                    if (_scytheHitbox.Intersects(enemy.Sprite.HitBox) && !enemy.IsDeath)
                    {

                        enemy.IsDeath = true;
                        Score();
                        _playAnimDeathEnemy = true;
                        enemy.Sprite.color = Color.Red;
                        enemy.MyDeathEffect = _random.Next(1, 4);
                        _deathTimer = _deathTimerDuration;
                    }
                }
            
            }
            if(_playAnimDeathEnemy) _deathTimer -= deltaTime;

            foreach (var enemy in _enemies.ToList())
            {
                if (enemy.IsDeath)
                {
                    if (_deathTimer <= 0f)
                    {
                        _enemies.Remove(enemy);
                        ResetEnemy(enemy);
                    }
                    else
                    {
                        enemy.DeathEffect(gameTime); 
                    }
                }
                else
                {
                    enemy.Update(gameTime); 
                }
            }
            base.Update(gameTime);
        }
        
    }

    protected override void Draw(GameTime gameTime)
    {
         GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

        if (!_isLoadingComplete)
        {
            GraphicsDevice.Clear(Color.White);
            _splashScreen.Draw(_spriteBatch);
            

        }
        else
        {
            _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            _background.Draw(_spriteBatch);
            _roxxi.Draw(_spriteBatch);

            _cat.Draw(_spriteBatch);
            _scythe.Draw(_spriteBatch);

            _spriteBatch.DrawString(_freeMonoFont, $"Score: {_score}", SCORE_POSITION, Color.Aqua, 
                    0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

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
        }      
         
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}