using Microsoft.Xna.Framework;

namespace LookingForLight;

public class AnimationManager
{
    int numFrames;
    Vector2 size;

    int counter;
    int activeFrame;
    int intervall;
    bool loop;

    public bool IsFinished { get; private set; }

    public AnimationManager(int numFrames, Vector2 size, int interval = 8, bool loop = true)
    {
        this.numFrames = numFrames;
        this.size = size;
        this.intervall = interval;
        this.loop = loop;
        counter = 0;
        activeFrame = 0;
        IsFinished = false;
    }

    public void Update()
    {
        if (IsFinished) return;
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
            if (loop) activeFrame = 0;
            else { activeFrame = numFrames - 1; IsFinished = true; }
        }
    }

    public void Reset()
    {
        counter = 0;
        activeFrame = 0;
        IsFinished = false;
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
