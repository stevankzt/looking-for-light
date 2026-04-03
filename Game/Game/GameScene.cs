using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LookingForLight;

public class GameScene : Iscene
{
    private ContentManager contentManager;
    private Texture2D texture;
    private SceneManager sceneManager;
    public GameScene( ContentManager content, SceneManager sceneManager )
    {
        contentManager = content;
        this.sceneManager = sceneManager;
    }
    
    public void Load()
    {
        texture = contentManager.Load<Texture2D>("Images/map");
    }

    public void Update(GameTime gameTime)
    {
        if(Keyboard.GetState().IsKeyDown(Keys.J))
            sceneManager.AddScene(new ExitScene(contentManager));
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 cameraOffset)
    {
        spriteBatch.Draw(texture, new Rectangle((int)cameraOffset.X, (int)cameraOffset.Y, 1920, 1080), Color.White);
    }
}