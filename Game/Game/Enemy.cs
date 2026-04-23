using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LookingForLight;

public class Enemy : Sprite
{
    public int Health { get; private set; }
    public bool IsAlive => Health > 0;

    private float movementSpeed = 11f;
    private int attackDamage = 1;
    private float attackRange = 160f;
    private float attackCooldown = 1.5f;
    private float attackCooldownTimer = 0f;

    private float aggroRange = 640f;
    private bool isAggro = false;

    private AnimationManager walkAnim;

    public Enemy(Texture2D texture, Rectangle rect, Rectangle srect, int animFrames = 4) : base(texture, rect, srect)
    {
        Health = 3;
        walkAnim = new AnimationManager(animFrames, new Vector2(32, 32), 8);
    }

    public void Update(Player player, CollisionSystem collisionSystem, float deltaTime)
    {
        if (!IsAlive) return;

        var distanceToPlayer = Vector2.Distance(rect.Center.ToVector2(), player.rect.Center.ToVector2());

        if (!isAggro && distanceToPlayer <= aggroRange)
            isAggro = true;

        if (isAggro)
        {
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

            walkAnim.Update();
            srect = walkAnim.GetFrame();
        }
        else
        {
            velocity = Vector2.Zero;
        }

        List<Rectangle> checkedTiles;
        rect = collisionSystem.MoveWithCollisions(rect, velocity, out checkedTiles);

        attackCooldownTimer = Math.Max(0f, attackCooldownTimer - deltaTime);

        if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0f)
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
