using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LookingForLight;

public class Player : Sprite
{
    public int Health { get; private set; } = 5;
    public bool IsAlive => Health > 0;
    public bool IsInvincible => invincibilityTimer > 0;

    private readonly float movementSpeed;
    private float invincibilityTimer = 0f;
    private const float InvincibilityDuration = 1.5f;

    public Player(Texture2D texture, Rectangle rect, Rectangle srect, float movementSpeed = 5f)
        : base(texture, rect, srect)
    {
        this.movementSpeed = movementSpeed;
    }

    public void Update(KeyboardState keyState, float deltaTime)
    {
        velocity = Vector2.Zero;

        if (keyState.IsKeyDown(Keys.D)) velocity.X = movementSpeed;
        if (keyState.IsKeyDown(Keys.A)) velocity.X = -movementSpeed;
        if (keyState.IsKeyDown(Keys.W)) velocity.Y = -movementSpeed;
        if (keyState.IsKeyDown(Keys.S)) velocity.Y = movementSpeed;

        if (invincibilityTimer > 0f) invincibilityTimer -= deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive || IsInvincible) return;
        Health = Math.Max(0, Health - damage);
        invincibilityTimer = InvincibilityDuration;
    }
}
