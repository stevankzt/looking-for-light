using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LookingForLight;

public class Enemy : Sprite
{
    public int Health { get; private set; }
    public bool IsAlive => Health > 0;

    private float movementSpeed = 2f;
    private int attackDamage = 1;
    private float attackRange = 40f;
    private float attackCooldown = 1.5f;
    private float attackCooldownTimer = 0f;

    public Enemy(Texture2D texture, Rectangle rect, Rectangle srect) : base(texture, rect, srect)
    {
        Health = 3;
    }

    public void Update(Player player, CollisionSystem collisionSystem, float deltaTime)
    {
        if (!IsAlive) return;

        Vector2 direction = new Vector2(
            player.rect.Center.X - rect.Center.X,
            player.rect.Center.Y - rect.Center.Y
        );

        if (direction.LengthSquared() > 0.0001f)
        {
            direction.Normalize();
            velocity = direction * movementSpeed;
        }
        else
        {
            velocity = Vector2.Zero;
        }

        List<Rectangle> checkedTiles;
        rect = collisionSystem.MoveWithCollisions(rect, velocity, out checkedTiles);

        attackCooldownTimer = Math.Max(0f, attackCooldownTimer - deltaTime);

        float distance = Vector2.Distance(rect.Center.ToVector2(), player.rect.Center.ToVector2());
        if (distance <= attackRange && attackCooldownTimer <= 0f)
        {
            player.TakeDamage(attackDamage);
            attackCooldownTimer = attackCooldown;
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;
        Health = Math.Max(0, Health - damage);
    }
}
