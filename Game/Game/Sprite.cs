using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LookingForLight;

public class Sprite {

    public Texture2D texture;
    public Rectangle rect;
    public Rectangle srect;
    public Vector2 velocity;

    public Sprite(
        Texture2D texture,
        Rectangle rect,
        Rectangle srect
    ) {
        this.texture = texture;
        this.rect = rect;
        this.srect = srect;
        velocity = new();
    }

    public void Draw(SpriteBatch spriteBatch) {
        spriteBatch.Draw(
            texture,
            rect,
            srect,
            Color.White
        );
    }
}