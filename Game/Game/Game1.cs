using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LookingForLight;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    AnimationManager am;
    
    private SpriteFont font;

    private Dictionary<Vector2, int> floor;
    private Dictionary<Vector2, int> walls;
    private Dictionary<Vector2, int> collisions;
    private Texture2D mapLevel1;
    private Texture2D hitBoxTexture;
    private FollowCamera followCamera;
    private CollisionSystem collisionSystem;
    
    private int TILESIZE = 32;
    Player player;
    private List<Rectangle> intersections;
    private Texture2D rectangleTexture;
    
    private List<Rectangle> textureStore;
    
    private Song song;
    SoundEffect effect;
    SoundEffectInstance effectInstance;
    
    KeyboardState prevKBState;
    
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
        floor = LoadMap("../../../Data/level1_floor.csv");
        walls = LoadMap("../../../Data/level1_walls.csv");
        collisions = LoadMap("../../../Data/level1_collisions.csv");
        followCamera = new FollowCamera(Vector2.Zero);
        collisionSystem = new CollisionSystem(collisions, TILESIZE);
        intersections = new();
    }

    private Dictionary<Vector2, int> LoadMap(string filePath)
    {
        Dictionary<Vector2, int> result = new();
        StreamReader reader = new(filePath);

        var line = "";
        var y = 0;
        while ((line = reader.ReadLine()) != null)
        {
            string[] items = line.Split(',');
            
            for(var x = 0; x < items.Length; x++)
            {
                if (int.TryParse((items[x]), out int value))
                {
                    if (value > -  1)
                    {
                        result[new Vector2(x, y)] = value;
                    }
                }
            }

            y++;

        }
        
        return result;
    }

    protected override void Initialize()
    {
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        mapLevel1 = Content.Load<Texture2D>("Dungeon_Tileset");
        hitBoxTexture = Content.Load<Texture2D>("collisionTiles");
        
        Texture2D playerTexture = Content.Load<Texture2D>("priest1_v1_1");
        Texture2D enemyTexture =  Content.Load<Texture2D>("skeleton_v1_1");
        Texture2D enemyTexture2 = Content.Load<Texture2D>("vampire_v1_1");
        
        rectangleTexture = new Texture2D(GraphicsDevice, 1, 1);
        
        player = new Player(playerTexture, new(TILESIZE,TILESIZE, TILESIZE, TILESIZE), new(0,0,16,16));
        
        font = Content.Load<SpriteFont>("Fonts/PixelFont");
        song = Content.Load<Song>("Audio/DropsSound");
        effect = Content.Load<SoundEffect>("Audio/FootStep1");
        effectInstance = effect.CreateInstance();
        
        
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        KeyboardState currentKBState = Keyboard.GetState();
        
        if (currentKBState.IsKeyDown(Keys.A))
        {
            effectInstance.Play();
        }
        if (currentKBState.IsKeyDown(Keys.D))
        {
            effectInstance.Play();
        }
        if (currentKBState.IsKeyDown(Keys.S))
        {
            effectInstance.Play();
        }
        if (currentKBState.IsKeyDown(Keys.W))
        {
            effectInstance.Play();
        }

        player.Update(currentKBState);
        player.rect = collisionSystem.MoveWithCollisions(player.rect, player.velocity, out intersections);

        followCamera.Follow(
            player.rect,
            new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
        );

        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin(
            transformMatrix: Matrix.CreateTranslation(followCamera.position.X, followCamera.position.Y, 0f),
            samplerState: SamplerState.PointClamp
        ); //начало рисования
        
        var display_tilesize = 32;
        var num_tiles_per_row = 10;
        var pixel_tilesize = 16;
        
        DrawLayer(display_tilesize, num_tiles_per_row, pixel_tilesize, floor, mapLevel1);
        DrawLayer(display_tilesize, num_tiles_per_row, pixel_tilesize, walls, mapLevel1);
        DrawLayer(display_tilesize, num_tiles_per_row, pixel_tilesize, collisions, hitBoxTexture);
        
        foreach (var rect in intersections) {

            DrawRectHollow(
                _spriteBatch,
                new Rectangle(
                    rect.X * TILESIZE,
                    rect.Y * TILESIZE,
                    TILESIZE,
                    TILESIZE
                ),
                4
            );
        }
        
        player.Draw(_spriteBatch);
        DrawRectHollow(_spriteBatch, player.rect, 4);
        
        _spriteBatch.End(); //конец рисования

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.DrawString(font, "LOL 0_0", Vector2.Zero, Color.White);
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    private void DrawLayer(int display_tilesize, int num_tiles_per_row, int pixel_tilesize, Dictionary<Vector2, int> layer,  Texture2D texture)
    {
        foreach (var item in layer)
        {
            Rectangle drect = new(
                (int)item.Key.X * display_tilesize,
                (int)item.Key.Y * display_tilesize,
                display_tilesize,
                display_tilesize
            );
            
            var x = item.Value % num_tiles_per_row;
            var y = item.Value / num_tiles_per_row;
            
            Rectangle src = new(
                x * pixel_tilesize,
                y * pixel_tilesize,
                pixel_tilesize,
                pixel_tilesize
            );
            
            _spriteBatch.Draw(texture, drect, src, Color.White);
        }
    }
    
    public void DrawRectHollow(SpriteBatch spriteBatch, Rectangle rect, int thickness) {
        spriteBatch.Draw(
            rectangleTexture,
            new Rectangle(
                rect.X,
                rect.Y,
                rect.Width,
                thickness
            ),
            Color.White
        );
        spriteBatch.Draw(
            rectangleTexture,
            new Rectangle(
                rect.X,
                rect.Bottom - thickness,
                rect.Width,
                thickness
            ),
            Color.White
        );
        spriteBatch.Draw(
            rectangleTexture,
            new Rectangle(
                rect.X,
                rect.Y,
                thickness,
                rect.Height
            ),
            Color.White
        );
        spriteBatch.Draw(
            rectangleTexture,
            new Rectangle(
                rect.Right - thickness,
                rect.Y,
                thickness,
                rect.Height
            ),
            Color.White
        );
    }
}