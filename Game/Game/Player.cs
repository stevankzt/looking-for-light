using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LookingForLight;

public class Player : Sprite
{
    public int Health { get; private set; } = 1;
    public bool IsAlive => Health > 0;
    public bool IsInvincible => invincibilityTimer > 0;
    public bool IsMoving => velocity != Vector2.Zero;

    private readonly float movementSpeed;
    private float invincibilityTimer = 0f;
    private const float InvincibilityDuration = 1.5f;

    private AnimationManager walkAnim = new(4, new Vector2(32, 32), 8);

    public Player(Texture2D texture, Rectangle rect, Rectangle srect, float movementSpeed = 10f)
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

        if (velocity != Vector2.Zero)
        {
            walkAnim.Update();
            srect = walkAnim.GetFrame();
        }
        else
        {
            srect = new Rectangle(0, 0, 32, 32);
        }

        if (invincibilityTimer > 0f) invincibilityTimer -= deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive || IsInvincible) return;
        Health = Math.Max(0, Health - damage);
        invincibilityTimer = InvincibilityDuration;
    }

    public void Reset()
    {
        Health = 1;
        invincibilityTimer = 0f;
        walkAnim = new(4, new Vector2(32, 32), 8);
    }
}
