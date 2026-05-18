using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LookingForLight;

public class Enemy : Sprite
{
    public int Health { get; private set; }
    public bool IsAlive => Health > 0;
    public bool IsDeathFinished => !IsAlive && deathAnim.IsFinished;

    private float movementSpeed = 11f;
    private int attackDamage = 1;
    private float attackRange = 160f;
    private float attackCooldown = 1.5f;
    private float attackCooldownTimer = 0f;

    private float aggroRange = 640f;
    private bool isAggro = false;

    private enum EnemyAnimState { Idle, Walk, Attack, Hurt, Death }
    private EnemyAnimState animState = EnemyAnimState.Idle;

    private Texture2D idleTex, walkTex, attackTex, hurtTex, deathTex;

    private AnimationManager idleAnim;
    private AnimationManager walkAnim;
    private AnimationManager attackAnim;
    private AnimationManager hurtAnim;
    private AnimationManager deathAnim;

    public Enemy(
        Texture2D idleTex,   int idleFrames,
        Texture2D walkTex,   int walkFrames,
        Texture2D attackTex, int attackFrames,
        Texture2D hurtTex,   int hurtFrames,
        Texture2D deathTex,  int deathFrames,
        Rectangle rect)
        : base(idleTex, rect, new Rectangle(0, 0, 32, 32))
    {
        Health = 3;
        this.idleTex   = idleTex;
        this.walkTex   = walkTex;
        this.attackTex = attackTex;
        this.hurtTex   = hurtTex;
        this.deathTex  = deathTex;

        idleAnim   = new AnimationManager(idleFrames,   new Vector2(32, 32), 8,  loop: true);
        walkAnim   = new AnimationManager(walkFrames,   new Vector2(32, 32), 6,  loop: true);
        attackAnim = new AnimationManager(attackFrames, new Vector2(32, 32), 5,  loop: false);
        hurtAnim   = new AnimationManager(hurtFrames,   new Vector2(32, 32), 5,  loop: false);
        deathAnim  = new AnimationManager(deathFrames,  new Vector2(32, 32), 6,  loop: false);
    }

    public void Update(Player player, CollisionSystem collisionSystem, float deltaTime)
    {
        if (!IsAlive)
        {
            animState = EnemyAnimState.Death;
            UpdateAnimation();
            return;
        }

        var distanceToPlayer = Vector2.Distance(rect.Center.ToVector2(), player.rect.Center.ToVector2());

        if (!isAggro && distanceToPlayer <= aggroRange)
            isAggro = true;

        // State transitions (priority: Hurt > Attack > Walk/Idle)
        if (animState == EnemyAnimState.Hurt)
        {
            if (hurtAnim.IsFinished)
                animState = isAggro ? EnemyAnimState.Walk : EnemyAnimState.Idle;
        }
        else if (animState == EnemyAnimState.Attack)
        {
            if (attackAnim.IsFinished)
                animState = isAggro ? EnemyAnimState.Walk : EnemyAnimState.Idle;
        }
        else
        {
            animState = isAggro ? EnemyAnimState.Walk : EnemyAnimState.Idle;
        }

        if (isAggro && animState == EnemyAnimState.Walk)
        {
            var direction = new Vector2(
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
            attackAnim.Reset();
            animState = EnemyAnimState.Attack;
        }

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        switch (animState)
        {
            case EnemyAnimState.Idle:
                texture = idleTex;
                idleAnim.Update();
                srect = idleAnim.GetFrame();
                break;
            case EnemyAnimState.Walk:
                texture = walkTex;
                walkAnim.Update();
                srect = walkAnim.GetFrame();
                break;
            case EnemyAnimState.Attack:
                texture = attackTex;
                attackAnim.Update();
                srect = attackAnim.GetFrame();
                break;
            case EnemyAnimState.Hurt:
                texture = hurtTex;
                hurtAnim.Update();
                srect = hurtAnim.GetFrame();
                break;
            case EnemyAnimState.Death:
                texture = deathTex;
                deathAnim.Update();
                srect = deathAnim.GetFrame();
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;
        Health = Math.Max(0, Health - damage);
        if (IsAlive)
        {
            hurtAnim.Reset();
            animState = EnemyAnimState.Hurt;
        }
    }

    public new void Draw(SpriteBatch spriteBatch)
    {
        var drawRect = new Rectangle(
            rect.X - rect.Width / 2,
            rect.Y - rect.Height / 2,
            rect.Width * 2,
            rect.Height * 2
        );
        spriteBatch.Draw(texture, drawRect, srect, Color.White);
    }
}
