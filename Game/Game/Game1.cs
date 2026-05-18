using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LookingForLight;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SpriteFont font;

    private Dictionary<Vector2, int> abbys;
    private Dictionary<Vector2, int> floor;
    private Dictionary<Vector2, int> middle;
    private Dictionary<Vector2, int> collisions;
    private Texture2D mapLevel1;
    private Texture2D hitBoxTexture;
    private FollowCamera followCamera;
    private CollisionSystem collisionSystem;

    private int TILESIZE = 128;
    Player player;
    List<Enemy> enemies;
    private List<Rectangle> intersections;
    private Texture2D rectangleTexture;

    private SoundEffect[] footsteps;
    private int footstepIndex = 0;
    private float footstepTimer = 0f;
    private const float FootstepInterval = 0.45f;

    KeyboardState prevKBState;
    MouseState prevMouseState;

    private Rectangle lastAttackHitbox;
    private bool showAttackHitbox;
    private float attackDebugTimer;
    private const float AttackDebugDuration = 0.08f;
    private const float AttackCooldown = 0.75f;
    private float attackCooldownTimer;

    private int currentLevel = 1;
    private Rectangle doorRect;

    private Texture2D keyTexture;
    private Key levelKey = null;
    private bool keySpawned = false;
    private bool hasKey = false;
    private Rectangle lastDeadEnemyRect;

    private Texture2D skel1Idle, skel1Walk, skel1Attack, skel1Hurt, skel1Death;

    private Texture2D skel2Idle, skel2Walk, skel2Attack, skel2Hurt, skel2Death;

    private Texture2D vampIdle, vampWalk, vampAttack, vampHurt, vampDeath;

    private Texture2D playerIdleTex;
    private Texture2D playerWalkTex;
    private Texture2D playerHurtTex;
    private Texture2D playerDeathTex;
    private Texture2D playerAtk1Tex;
    private Texture2D playerAtk2Tex;


    private Texture2D lightCircleTexture;
    private RenderTarget2D lightTarget;
    private RenderTarget2D sceneTarget;
    private const int LightRadius = 700;

    private static readonly BlendState EraseAlphaBlend = new BlendState
    {
        ColorBlendFunction    = BlendFunction.Add,
        ColorSourceBlend      = Blend.Zero,
        ColorDestinationBlend = Blend.One,
        AlphaBlendFunction    = BlendFunction.Add,
        AlphaSourceBlend      = Blend.Zero,
        AlphaDestinationBlend = Blend.InverseSourceAlpha
    };

    private bool isGameOver = false;
    private bool isGameWon = false;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        _graphics.PreferredBackBufferWidth = displayMode.Width;
        _graphics.PreferredBackBufferHeight = displayMode.Height;
        _graphics.HardwareModeSwitch = false;
        _graphics.IsFullScreen = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        followCamera = new FollowCamera(Vector2.Zero);
        intersections = new();
    }

    private Dictionary<Vector2, int> LoadMap(string filePath)
    {
        var result = new Dictionary<Vector2, int>();
        using var reader = new StreamReader(filePath);
        var y = 0;
        var line = "";
        while ((line = reader.ReadLine()) != null)
        {
            var items = line.Split(',');
            for (var x = 0; x < items.Length; x++)
            {
                if (int.TryParse(items[x], out int value) && value > -1)
                    result[new Vector2(x, y)] = value;
            }
            y++;
        }
        return result;
    }

    private void LoadLevel(int level)
    {
        currentLevel = level;
        hasKey = false;
        levelKey = null;
        keySpawned = false;
        lastDeadEnemyRect = Rectangle.Empty;
        intersections = new();

        var root = $"../../../Content/Maps/Map{level}/map{level}_";
        var middleFile = level == 2 ? root + "midlle.csv" : root + "middle.csv";

        abbys = LoadMap(root + "abbys.csv");
        floor = LoadMap(root + "floor.csv");
        middle = LoadMap(middleFile);
        collisions = LoadMap(root + "collisions.csv");

        collisionSystem = new CollisionSystem(collisions, TILESIZE);
        doorRect = FindDoorRect(middle);

        var spawnTile = FindSpawnTile(floor, collisions);

        if (player == null)
        {
            player = new Player(
                playerIdleTex, playerWalkTex, playerHurtTex,
                playerDeathTex, playerAtk1Tex, playerAtk2Tex,
                new Rectangle((int)spawnTile.X * TILESIZE, (int)spawnTile.Y * TILESIZE, TILESIZE, TILESIZE)
            );
        }
        else
        {
            player.rect = new Rectangle(
                (int)spawnTile.X * TILESIZE,
                (int)spawnTile.Y * TILESIZE,
                TILESIZE, TILESIZE
            );
        }

        enemies = CreateEnemies(level, spawnTile);
    }


    private Vector2 FindSpawnTile(Dictionary<Vector2, int> floorLayer, Dictionary<Vector2, int> collisionLayer)
    {
        if (floorLayer.Count == 0) return Vector2.One;

        var minY = int.MaxValue;
        foreach (var key in floorLayer.Keys)
            if ((int)key.Y < minY) minY = (int)key.Y;

        var minX = int.MaxValue;
        foreach (var key in floorLayer.Keys)
            if ((int)key.Y == minY && (int)key.X < minX) minX = (int)key.X;

        var tile = new Vector2(minX + 1, minY + 1);
        var attempts = 0;
        while (collisionLayer.ContainsKey(tile) && attempts++ < 200)
            tile.X++;
        return tile;
    }

    private Rectangle FindDoorRect(Dictionary<Vector2, int> middleLayer)
    {
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        var found = false;
        foreach (var kv in middleLayer)
        {
            if (kv.Value != 36 && kv.Value != 37) continue;
            int x = (int)kv.Key.X, y = (int)kv.Key.Y;
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            found = true;
        }
        if (!found) return Rectangle.Empty;
        return new Rectangle(
            minX * TILESIZE, minY * TILESIZE,
            (maxX - minX + 1) * TILESIZE, (maxY - minY + 1) * TILESIZE
        );
    }

    private List<Vector2> FindEnemySpawnTiles(
        Dictionary<Vector2, int> floorLayer,
        Dictionary<Vector2, int> collisionLayer,
        Vector2 playerTile, int count, int minDist = 8)
    {
        var candidates = new List<Vector2>();
        foreach (var key in floorLayer.Keys)
        {
            if (collisionLayer.ContainsKey(key)) continue;
            var dist = Math.Abs(key.X - playerTile.X) + Math.Abs(key.Y - playerTile.Y);
            if (dist >= minDist) candidates.Add(key);
        }

        var result = new List<Vector2>();
        if (candidates.Count == 0) return result;

        var step = Math.Max(1, candidates.Count / count);
        for (var i = 0; i < count && i * step < candidates.Count; i++)
            result.Add(candidates[i * step]);
        return result;
    }

    private List<Enemy> CreateEnemies(int level, Vector2 spawnTile)
    {
        var list = new List<Enemy>();
        var tiles = FindEnemySpawnTiles(floor, collisions, spawnTile, level == 1 ? 3 : 4);

        for (var i = 0; i < tiles.Count; i++)
        {
            var pos = new Rectangle((int)tiles[i].X * TILESIZE, (int)tiles[i].Y * TILESIZE, TILESIZE, TILESIZE);
            Enemy enemy;
            if (i % 3 == 2)
                enemy = new Enemy(vampIdle, 6, vampWalk, 8, vampAttack, 16, vampHurt, 5, vampDeath, 14, pos);
            else if (i % 2 == 0)
                enemy = new Enemy(skel1Idle, 6, skel1Walk, 10, skel1Attack, 9, skel1Hurt, 5, skel1Death, 17, pos);
            else
                enemy = new Enemy(skel2Idle, 6, skel2Walk, 10, skel2Attack, 15, skel2Hurt, 5, skel2Death, 15, pos);
            list.Add(enemy);
        }
        return list;
    }


    protected override void Initialize()
    {
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        mapLevel1 = Content.Load<Texture2D>("TileSets/Dungeon_Tileset");
        hitBoxTexture = Content.Load<Texture2D>("TileSets/collisionTiles");
        keyTexture = Content.Load<Texture2D>("Keys/key");

        skel1Idle   = Content.Load<Texture2D>("Enemies/Skeleton1/enemies-skeleton1_idle");
        skel1Walk   = Content.Load<Texture2D>("Enemies/Skeleton1/enemies-skeleton1_movement");
        skel1Attack = Content.Load<Texture2D>("Enemies/Skeleton1/enemies-skeleton1-attack");
        skel1Hurt   = Content.Load<Texture2D>("Enemies/Skeleton1/enemies-skeleton1_take_damage");
        skel1Death  = Content.Load<Texture2D>("Enemies/Skeleton1/enemies-skeleton1_death");

        skel2Idle   = Content.Load<Texture2D>("Enemies/Skeleton2/enemies-skeleton2_idle");
        skel2Walk   = Content.Load<Texture2D>("Enemies/Skeleton2/enemies-skeleton2_movemen");
        skel2Attack = Content.Load<Texture2D>("Enemies/Skeleton2/enemies-skeleton2_attack");
        skel2Hurt   = Content.Load<Texture2D>("Enemies/Skeleton2/enemies-skeleton2_take_damage");
        skel2Death  = Content.Load<Texture2D>("Enemies/Skeleton2/enemies-skeleton2_death");

        vampIdle   = Content.Load<Texture2D>("Enemies/Vampire/enemies-vampire_idle");
        vampWalk   = Content.Load<Texture2D>("Enemies/Vampire/enemies-vampire_movement");
        vampAttack = Content.Load<Texture2D>("Enemies/Vampire/enemies-vampire_attack");
        vampHurt   = Content.Load<Texture2D>("Enemies/Vampire/enemies-vampire_take_damage");
        vampDeath  = Content.Load<Texture2D>("Enemies/Vampire/enemies-vampire_death");

        playerIdleTex  = Content.Load<Texture2D>("Player/Soldier-Idle");
        playerWalkTex  = Content.Load<Texture2D>("Player/Soldier-Walk");
        playerHurtTex  = Content.Load<Texture2D>("Player/Soldier-Hurt");
        playerDeathTex = Content.Load<Texture2D>("Player/Soldier-Death");
        playerAtk1Tex  = Content.Load<Texture2D>("Player/Soldier-Attack01");
        playerAtk2Tex  = Content.Load<Texture2D>("Player/Soldier-Attack02");

        rectangleTexture = new Texture2D(GraphicsDevice, 1, 1);
        rectangleTexture.SetData(new[] { Color.White });

        font = Content.Load<SpriteFont>("Fonts/PixelFont");

        footsteps = new SoundEffect[21];
        for (var i = 0; i < 21; i++)
            footsteps[i] = Content.Load<SoundEffect>($"Audio/FootstepsFloor/Steps_floor-{i + 1:D3}");

        lightCircleTexture = CreateCircleTexture(LightRadius);
        var vp = GraphicsDevice.Viewport;
        lightTarget = new RenderTarget2D(GraphicsDevice, vp.Width, vp.Height);
        sceneTarget = new RenderTarget2D(GraphicsDevice, vp.Width, vp.Height);

        LoadLevel(1);
    }

    private Texture2D CreateCircleTexture(int radius)
    {
        int d = radius * 2;
        var tex = new Texture2D(GraphicsDevice, d, d);
        var data = new Color[d * d];
        var center = new Vector2(radius, radius);
        for (int y = 0; y < d; y++)
            for (int x = 0; x < d; x++)
            {
                float t = MathHelper.Clamp(1f - Vector2.Distance(new Vector2(x, y), center) / radius, 0f, 1f);
                t = t * t;
                data[y * d + x] = new Color(t, t, t, t);
            }
        tex.SetData(data);
        return tex;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        KeyboardState currentKBState = Keyboard.GetState();
        MouseState currentMouseState = Mouse.GetState();
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (isGameOver || isGameWon)
        {
            if (currentKBState.IsKeyDown(Keys.R) && !prevKBState.IsKeyDown(Keys.R))
            {
                isGameOver = false;
                isGameWon = false;
                player.Reset();
                LoadLevel(1);
            }
            prevKBState = currentKBState;
            base.Update(gameTime);
            return;
        }


        player.Update(currentKBState, deltaTime);
        player.rect = collisionSystem.MoveWithCollisions(player.rect, player.velocity, out intersections);

        if (player.IsMoving)
        {
            footstepTimer -= deltaTime;
            if (footstepTimer <= 0f)
            {
                footsteps[footstepIndex].Play();
                footstepIndex = (footstepIndex + 1) % footsteps.Length;
                footstepTimer = FootstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        foreach (var e in enemies)
            e.Update(player, collisionSystem, deltaTime);

        for (var i = 0; i < enemies.Count; i++)
        {
            if (!enemies[i].IsAlive) continue;
            for (var j = i + 1; j < enemies.Count; j++)
            {
                if (!enemies[j].IsAlive) continue;
                var ri = enemies[i].rect;
                var rj = enemies[j].rect;
                if (!ri.Intersects(rj)) continue;

                var overlapX = Math.Min(ri.Right, rj.Right) - Math.Max(ri.Left, rj.Left);
                var overlapY = Math.Min(ri.Bottom, rj.Bottom) - Math.Max(ri.Top, rj.Top);

                if (overlapX <= overlapY)
                {
                    var sep = overlapX / 2 + 1;
                    var dir = ri.Center.X < rj.Center.X ? -1 : 1;
                    enemies[i].rect = new Rectangle(ri.X + dir * sep, ri.Y, ri.Width, ri.Height);
                    enemies[j].rect = new Rectangle(rj.X - dir * sep, rj.Y, rj.Width, rj.Height);
                }
                else
                {
                    var sep = overlapY / 2 + 1;
                    var dir = ri.Center.Y < rj.Center.Y ? -1 : 1;
                    enemies[i].rect = new Rectangle(ri.X, ri.Y + dir * sep, ri.Width, ri.Height);
                    enemies[j].rect = new Rectangle(rj.X, rj.Y - dir * sep, rj.Width, rj.Height);
                }
            }
        }

        if (!keySpawned && enemies.Count > 0)
        {
            var allDead = true;
            foreach (var e in enemies) if (e.IsAlive) { allDead = false; break; }
            if (allDead)
            {
                keySpawned = true;
                levelKey = new Key(keyTexture, lastDeadEnemyRect);
            }
        }

        levelKey?.Update(player.rect);
        if (levelKey != null && levelKey.IsPickedUp && !hasKey)
            hasKey = true;

        if (hasKey && doorRect != Rectangle.Empty && player.rect.Intersects(doorRect))
        {
            if (currentLevel == 1)
                LoadLevel(2);
            else if (currentLevel == 2)
                isGameWon = true;
        }


        if (!player.IsAlive)
            isGameOver = true;


        attackCooldownTimer = Math.Max(0f, attackCooldownTimer - deltaTime);
        if (showAttackHitbox)
        {
            attackDebugTimer -= deltaTime;
            if (attackDebugTimer <= 0f) showAttackHitbox = false;
        }

        followCamera.Follow(
            player.rect,
            new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
        );

        var clickedLeftMouse = currentMouseState.LeftButton == ButtonState.Pressed &&
                                prevMouseState.LeftButton == ButtonState.Released;

        if (clickedLeftMouse && attackCooldownTimer <= 0f)
        {
            player.TriggerAttack();
            lastAttackHitbox = BuildAttackHitbox(currentMouseState);
            showAttackHitbox = true;
            attackDebugTimer = AttackDebugDuration;
            attackCooldownTimer = AttackCooldown;

            foreach (var e in enemies)
            {
                if (e.IsAlive && lastAttackHitbox.Intersects(e.rect))
                {
                    e.TakeDamage(1);
                    if (!e.IsAlive)
                        lastDeadEnemyRect = e.rect;
                }
            }
        }

        prevKBState = currentKBState;
        prevMouseState = currentMouseState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(sceneTarget);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(
            transformMatrix: Matrix.CreateTranslation(followCamera.position.X, followCamera.position.Y, 0f),
            samplerState: SamplerState.PointClamp
        );

        DrawLayer(TILESIZE, 10, 32, abbys, mapLevel1);
        DrawLayer(TILESIZE, 10, 32, floor, mapLevel1);
        DrawLayer(TILESIZE, 10, 32, middle, mapLevel1);

        levelKey?.Draw(_spriteBatch);
        player.Draw(_spriteBatch);
        foreach (var e in enemies)
            if (e.IsAlive || !e.IsDeathFinished) e.Draw(_spriteBatch);

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(lightTarget);
        GraphicsDevice.Clear(new Color(0, 0, 0, 240));

        var playerScreenX = player.rect.Center.X + (int)followCamera.position.X;
        var playerScreenY = player.rect.Center.Y + (int)followCamera.position.Y;
        _spriteBatch.Begin(blendState: EraseAlphaBlend, samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(lightCircleTexture,
            new Rectangle(playerScreenX - LightRadius, playerScreenY - LightRadius, LightRadius * 2, LightRadius * 2),
            Color.White);
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(sceneTarget, GraphicsDevice.Viewport.Bounds, Color.White);
        _spriteBatch.End();

        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(lightTarget, GraphicsDevice.Viewport.Bounds, Color.White);
        _spriteBatch.End();

        var vp = GraphicsDevice.Viewport;
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _spriteBatch.DrawString(font, $"Level: {currentLevel}", new Vector2(0, 0), Color.White);
        _spriteBatch.DrawString(font, $"Player HP: {player.Health}", new Vector2(0, 20), Color.White);
        _spriteBatch.DrawString(font, $"Enemies: {enemies.Count}", new Vector2(0, 41), Color.White);

        if (hasKey)
        {
            var keyMsg = "[ KEY ]";
            var keySize = font.MeasureString(keyMsg);
            _spriteBatch.DrawString(font, keyMsg,
                new Vector2((vp.Width - keySize.X) / 2f, 10), Color.Gold);
        }

        if (isGameOver)
        {
            _spriteBatch.Draw(rectangleTexture,
                new Rectangle(0, 0, vp.Width, vp.Height),
                new Color(0, 0, 0, 100));

            var died = "NOOO WAAY, YOU ARE DEAD";
            var restart = "FAST PRESS R AND GO FIGHT AGAIN";
            var diedSize = font.MeasureString(died);
            var restartSize = font.MeasureString(restart);

            _spriteBatch.DrawString(font, died,
                new Vector2((vp.Width - diedSize.X) / 2f, vp.Height / 2f - 30),
                Color.Red);
            _spriteBatch.DrawString(font, restart,
                new Vector2((vp.Width - restartSize.X) / 2f, vp.Height / 2f + 10),
                Color.White);
        }

        if (isGameWon)
        {
            _spriteBatch.Draw(rectangleTexture,
                new Rectangle(0, 0, vp.Width, vp.Height),
                new Color(0, 0, 0, 220));

            var won     = "VICTORY!";
            var sub     = "YOU HAVE FOUND THE LIGHT";
            var restart = "PRESS R TO PLAY AGAIN";
            var wonSize     = font.MeasureString(won);
            var subSize     = font.MeasureString(sub);
            var restartSize = font.MeasureString(restart);

            _spriteBatch.DrawString(font, won,
                new Vector2((vp.Width - wonSize.X) / 2f, vp.Height / 2f - 60),
                Color.Gold);
            _spriteBatch.DrawString(font, sub,
                new Vector2((vp.Width - subSize.X) / 2f, vp.Height / 2f),
                Color.White);
            _spriteBatch.DrawString(font, restart,
                new Vector2((vp.Width - restartSize.X) / 2f, vp.Height / 2f + 60),
                Color.White);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private Rectangle BuildAttackHitbox(MouseState mouseState)
    {
        var attackSize = TILESIZE * 2;
        var playerCenter = new Vector2(player.rect.Center.X, player.rect.Center.Y);
        var mouseWorld = new Vector2(
            mouseState.X - followCamera.position.X,
            mouseState.Y - followCamera.position.Y
        );

        var direction = mouseWorld - playerCenter;
        if (direction.LengthSquared() < 0.0001f) direction = Vector2.UnitX;
        direction.Normalize();

        var distanceFromPlayer = player.rect.Width / 2f + attackSize / 2f;
        var hitboxCenter = playerCenter + direction * distanceFromPlayer;

        return new Rectangle(
            (int)(hitboxCenter.X - attackSize / 2f),
            (int)(hitboxCenter.Y - attackSize / 2f),
            attackSize, attackSize
        );
    }

    private void DrawLayer(int display_tilesize, int num_tiles_per_row, int pixel_tilesize,
        Dictionary<Vector2, int> layer, Texture2D texture)
    {
        foreach (var item in layer)
        {
            var drect = new Rectangle(
                (int)item.Key.X * display_tilesize,
                (int)item.Key.Y * display_tilesize,
                display_tilesize, display_tilesize);

            var tx = item.Value % num_tiles_per_row;
            var ty = item.Value / num_tiles_per_row;
            var src = new Rectangle(tx * pixel_tilesize, ty * pixel_tilesize, pixel_tilesize, pixel_tilesize);

            _spriteBatch.Draw(texture, drect, src, Color.White);
        }
    }

    public void DrawRectHollow(SpriteBatch spriteBatch, Rectangle rect, int thickness)
    {
        spriteBatch.Draw(rectangleTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), Color.White);
        spriteBatch.Draw(rectangleTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), Color.White);
        spriteBatch.Draw(rectangleTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), Color.White);
        spriteBatch.Draw(rectangleTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), Color.White);
    }
}
