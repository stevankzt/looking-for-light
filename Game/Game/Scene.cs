using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LookingForLight;

public interface Iscene
{
    public void Load();
    public void Update(GameTime gameTime);
    public void Draw(SpriteBatch spriteBatch, Vector2 cameraOffset);
}