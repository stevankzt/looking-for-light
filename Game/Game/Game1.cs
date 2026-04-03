using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    private SceneManager sceneManager;
    
    private FollowCamera camera;
    
    List<Sprite> sprites;
    Player player;
    AnimationManager am;
    
    private SpriteFont font;

    private Dictionary<Vector2, int> floor;
    private Dictionary<Vector2, int> walls;
    
    private Texture2D mapLevel1;
    
    private List<Rectangle> textureStore;
    
    
    private Song song;
    SoundEffect effect;
    SoundEffectInstance effectInstance;
    
    KeyboardState prevKBState;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        sceneManager = new ();
        camera = new(Vector2.Zero);
        
        floor = LoadMap("../../../Data/level1_floor.csv");
        walls = LoadMap("../../../Data/level1_walls.csv");
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
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        sceneManager.AddScene(new GameScene(Content, sceneManager));
        
        mapLevel1 = Content.Load<Texture2D>("Dungeon_Tileset");
        
        sprites = new();
        Texture2D playerTexture = Content.Load<Texture2D>("priest1_v1_1");
        Texture2D enemyTexture =  Content.Load<Texture2D>("skeleton_v1_1");
        Texture2D enemyTexture2 = Content.Load<Texture2D>("vampire_v1_1");
        
        player = new Player(playerTexture, new(200,200, 50, 50), new(0,0,16,16), sprites);
        sprites.Add(player);
        
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

        sceneManager.GetCurrentScene().Update(gameTime);
        
        KeyboardState currentKBState = Keyboard.GetState();
        
        
        if (currentKBState.IsKeyDown(Keys.Space) && !prevKBState.IsKeyDown(Keys.Space))
        {
            MediaPlayer.Play(song);
        }
        
        if (currentKBState.IsKeyDown(Keys.P) && !prevKBState.IsKeyDown(Keys.P))
        {
            MediaPlayer.Pause();
        }
        
        if (currentKBState.IsKeyDown(Keys.R) && !prevKBState.IsKeyDown(Keys.R))
        {
            MediaPlayer.Resume();
        }

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
        
        prevKBState = currentKBState;
        

        foreach (Sprite sprite in sprites)
        {
            sprite.Update();
        }
        
        camera.Follow(player.drect, new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight));

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp); //начало рисования
        
        var display_tilesize = 32;
        var num_tiles_per_row = 10;
        var pixel_tilesize = 16;
        
        DrawLayer(display_tilesize, num_tiles_per_row, pixel_tilesize, floor);
        DrawLayer(display_tilesize, num_tiles_per_row, pixel_tilesize, walls);

        foreach (Sprite sprite in sprites)
        {
            sprite.Draw(_spriteBatch, camera.position);
        }
        
        _spriteBatch.DrawString(font, "LOL 0_0", Vector2.Zero, Color.White);
        
        _spriteBatch.End(); //конец рисования
        
        base.Draw(gameTime);
    }

    private void DrawLayer(int display_tilesize, int num_tiles_per_row, int pixel_tilesize, Dictionary<Vector2, int> layer)
    {
        foreach (var item in layer)
        {
            Rectangle drect = new(
                (int)item.Key.X * display_tilesize + (int)camera.position.X,
                (int)item.Key.Y * display_tilesize + (int)camera.position.Y,
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
            
            _spriteBatch.Draw(mapLevel1, drect, src, Color.White);
        }
    }
}