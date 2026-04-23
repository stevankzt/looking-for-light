using Microsoft.Xna.Framework;

namespace LookingForLight;

public class AnimationManager
{
    int numFrames;
    Vector2 size;

    int counter;
    int activeFrame;
    int intervall;

    public AnimationManager(int numFrames, Vector2 size, int interval = 8)
    {
        this.numFrames = numFrames;
        this.size = size;
        this.intervall = interval;
        counter = 0;
        activeFrame = 0;
    }

    public void Update()
    {
        counter++;
        if (counter >= intervall)
        {
            counter = 0;
            NextFrame();
        }
    }

    private void NextFrame()
    {
        activeFrame++;
        if (activeFrame >= numFrames)
        {
            activeFrame = 0;
        }
    }

    public Rectangle GetFrame()
    {
        return new Rectangle(
            activeFrame * (int)size.X,
            0,
            (int)size.X,
            (int)size.Y);
    }
}
