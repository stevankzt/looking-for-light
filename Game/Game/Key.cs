using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LookingForLight;

public class Key
{
    private Texture2D texture;
    private AnimationManager anim;
    public Rectangle rect;
    public bool IsPickedUp { get; private set; }

    public Key(Texture2D texture, Rectangle rect, int animFrames = 4)
    {
        this.texture = texture;
        this.rect = rect;
        anim = new AnimationManager(animFrames, new Vector2(32, 32), 8);
    }

    public void Update(Rectangle playerRect)
    {
        if (IsPickedUp) return;
        anim.Update();
        if (rect.Intersects(playerRect))
            IsPickedUp = true;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsPickedUp)
            spriteBatch.Draw(texture, rect, anim.GetFrame(), Color.White);
    }
}
