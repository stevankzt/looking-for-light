using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LookingForLight;

public class CollisionSystem
{
    private readonly Dictionary<Vector2, int> collisionMap;
    private readonly int tileSize;

    public CollisionSystem(Dictionary<Vector2, int> collisionMap, int tileSize)
    {
        this.collisionMap = collisionMap;
        this.tileSize = tileSize;
    }

    public Rectangle MoveWithCollisions(Rectangle target, Vector2 velocity, out List<Rectangle> checkedTiles)
    {
        target.X += (int)velocity.X;
        checkedTiles = GetIntersectingTilesHorizontal(target);

        foreach (var tile in checkedTiles)
        {
            if (!collisionMap.TryGetValue(new Vector2(tile.X, tile.Y), out _))
            {
                continue;
            }

            Rectangle collision = new(
                tile.X * tileSize,
                tile.Y * tileSize,
                tileSize,
                tileSize
            );

            if (!target.Intersects(collision))
            {
                continue;
            }

            if (velocity.X > 0.0f)
            {
                target.X = collision.Left - target.Width;
            }
            else if (velocity.X < 0.0f)
            {
                target.X = collision.Right;
            }
        }

        target.Y += (int)velocity.Y;
        checkedTiles = GetIntersectingTilesVertical(target);

        foreach (var tile in checkedTiles)
        {
            if (!collisionMap.TryGetValue(new Vector2(tile.X, tile.Y), out _))
            {
                continue;
            }

            Rectangle collision = new(
                tile.X * tileSize,
                tile.Y * tileSize,
                tileSize,
                tileSize
            );

            if (!target.Intersects(collision))
            {
                continue;
            }

            if (velocity.Y > 0.0f)
            {
                target.Y = collision.Top - target.Height;
            }
            else if (velocity.Y < 0.0f)
            {
                target.Y = collision.Bottom;
            }
        }

        return target;
    }

    private List<Rectangle> GetIntersectingTilesHorizontal(Rectangle target)
    {
        List<Rectangle> intersections = new();

        int widthInTiles = (target.Width - (target.Width % tileSize)) / tileSize;
        int heightInTiles = (target.Height - (target.Height % tileSize)) / tileSize;

        for (int x = 0; x <= widthInTiles; x++)
        {
            for (int y = 0; y <= heightInTiles; y++)
            {
                intersections.Add(new Rectangle(
                    (target.X + x * tileSize) / tileSize,
                    (target.Y + y * (tileSize - 1)) / tileSize,
                    tileSize,
                    tileSize
                ));
            }
        }

        return intersections;
    }

    private List<Rectangle> GetIntersectingTilesVertical(Rectangle target)
    {
        List<Rectangle> intersections = new();

        int widthInTiles = (target.Width - (target.Width % tileSize)) / tileSize;
        int heightInTiles = (target.Height - (target.Height % tileSize)) / tileSize;

        for (int x = 0; x <= widthInTiles; x++)
        {
            for (int y = 0; y <= heightInTiles; y++)
            {
                intersections.Add(new Rectangle(
                    (target.X + x * (tileSize - 1)) / tileSize,
                    (target.Y + y * tileSize) / tileSize,
                    tileSize,
                    tileSize
                ));
            }
        }

        return intersections;
    }
}
