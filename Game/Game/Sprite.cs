using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;

namespace LookingForLight;

public class Sprite
{
    /*private static readonly float SCALE = 5f;*/
    
    public Texture2D texture;

    public Rectangle drect, srect;
    
    public Sprite(Texture2D texture, Rectangle drect, Rectangle srect)
    {
        this.texture = texture;
        this.drect = drect;
        this.srect = srect;
    }

    public virtual void Update()
    {
        
    }
    
    public virtual void Draw(SpriteBatch spriteBatch, Vector2 offset)
    {
        Rectangle dest = new(
            drect.X + (int)offset.X,
            drect.Y + (int)offset.Y,
            drect.Width,
            drect.Height
        );
        
        spriteBatch.Draw(texture, dest, srect ,Color.White);
    }
}