using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LookingForLight;

public class Player : Sprite
{
    
    List<Sprite> collisionGroup = new();

    public Player(Texture2D texture, Rectangle drect, Rectangle srect,List<Sprite> collisionGroup) : base(texture, drect, srect)
    {
        this.collisionGroup = collisionGroup;
    }
    
    
    public override void Update()
    {
        
        /*var changeY = 0;*/
        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            /*changeY -= 5;*/
            drect.Y -= 5;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            /*changeY += 5;*/
            drect.Y += 5;
        }
        /*drect.Y += changeY;*/
        

        /*var changeX = 0;*/
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            /*changeX -= 5;*/
            drect.X -= 5;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            /*changeX += 5;*/
            drect.X += 5;
        }
        /*drect.X += changeX;*/

        /*foreach (Sprite sprite in collisionGroup)
        {
            if (sprite != this && sprite.drect.Intersects(this.drect))
            {
                drect.X -= changeX;
                drect.Y -= changeY;
            }
        }*/
    }
}