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
    public bool IsMoving => velocity != Vector2.Zero;

    private readonly float movementSpeed;
    private float invincibilityTimer = 0f;
    private const float InvincibilityDuration = 1.5f;

    private enum AnimState { Idle, Walk, Hurt, Death, Attack }
    private AnimState animState = AnimState.Idle;
    private int nextAttackIndex = 0;
    private int currentAttackIndex = 0;

    private Texture2D idleTex, walkTex, hurtTex, deathTex, atk1Tex, atk2Tex;

    private AnimationManager idleAnim  = new(6, new Vector2(32, 32), 8,  loop: true);
    private AnimationManager walkAnim  = new(8, new Vector2(32, 32), 6,  loop: true);
    private AnimationManager hurtAnim  = new(4, new Vector2(32, 32), 5,  loop: false);
    private AnimationManager deathAnim = new(4, new Vector2(32, 32), 8,  loop: false);
    private AnimationManager atk1Anim  = new(6, new Vector2(32, 32), 5,  loop: false);
    private AnimationManager atk2Anim  = new(6, new Vector2(32, 32), 5,  loop: false);

    public Player(
        Texture2D idleTex, Texture2D walkTex, Texture2D hurtTex,
        Texture2D deathTex, Texture2D atk1Tex, Texture2D atk2Tex,
        Rectangle rect, float movementSpeed = 10f)
        : base(idleTex, rect, new Rectangle(0, 0, 32, 32))
    {
        this.idleTex  = idleTex;
        this.walkTex  = walkTex;
        this.hurtTex  = hurtTex;
        this.deathTex = deathTex;
        this.atk1Tex  = atk1Tex;
        this.atk2Tex  = atk2Tex;
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

        if (!IsAlive)
        {
            animState = AnimState.Death;
        }
        else if (animState == AnimState.Hurt)
        {
            if (hurtAnim.IsFinished)
                animState = velocity != Vector2.Zero ? AnimState.Walk : AnimState.Idle;
        }
        else if (animState == AnimState.Attack)
        {
            var activeAtkAnim = currentAttackIndex == 0 ? atk1Anim : atk2Anim;
            if (activeAtkAnim.IsFinished)
                animState = velocity != Vector2.Zero ? AnimState.Walk : AnimState.Idle;
        }
        else
        {
            animState = velocity != Vector2.Zero ? AnimState.Walk : AnimState.Idle;
        }

        switch (animState)
        {
            case AnimState.Idle:
                texture = idleTex;
                idleAnim.Update();
                srect = idleAnim.GetFrame();
                break;
            case AnimState.Walk:
                texture = walkTex;
                walkAnim.Update();
                srect = walkAnim.GetFrame();
                break;
            case AnimState.Hurt:
                texture = hurtTex;
                hurtAnim.Update();
                srect = hurtAnim.GetFrame();
                break;
            case AnimState.Death:
                texture = deathTex;
                deathAnim.Update();
                srect = deathAnim.GetFrame();
                break;
            case AnimState.Attack:
                if (currentAttackIndex == 0)
                {
                    texture = atk1Tex;
                    atk1Anim.Update();
                    srect = atk1Anim.GetFrame();
                }
                else
                {
                    texture = atk2Tex;
                    atk2Anim.Update();
                    srect = atk2Anim.GetFrame();
                }
                break;
        }
    }

    public void TriggerAttack()
    {
        if (!IsAlive) return;
        currentAttackIndex = nextAttackIndex;
        nextAttackIndex = 1 - nextAttackIndex;
        if (currentAttackIndex == 0) atk1Anim.Reset(); else atk2Anim.Reset();
        animState = AnimState.Attack;
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive || IsInvincible) return;
        Health = Math.Max(0, Health - damage);
        invincibilityTimer = InvincibilityDuration;
        if (IsAlive)
        {
            hurtAnim.Reset();
            animState = AnimState.Hurt;
        }
    }

    public void Reset()
    {
        Health = 5;
        invincibilityTimer = 0f;
        animState = AnimState.Idle;
        nextAttackIndex = 0;
        currentAttackIndex = 0;
        idleAnim.Reset();
        walkAnim.Reset();
        hurtAnim.Reset();
        deathAnim.Reset();
        atk1Anim.Reset();
        atk2Anim.Reset();
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
