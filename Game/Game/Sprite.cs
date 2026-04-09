// A Sprite class for keeping track of image and position data for our player.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

    public void Update(KeyboardState keystate) {
        velocity = Vector2.Zero;

        if (keystate.IsKeyDown(Keys.D)) {
            velocity.X = 5;
        }
        if (keystate.IsKeyDown(Keys.A)) {
            velocity.X = -5;
        }
        if (keystate.IsKeyDown(Keys.W)) {
            velocity.Y = -5;
        }
        if (keystate.IsKeyDown(Keys.S)) {
            velocity.Y = 5;
        }
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