using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LookingForLight;

public class Player : Sprite
{
	private readonly float movementSpeed;

	public Player(Texture2D texture, Rectangle rect, Rectangle srect, float movementSpeed = 5f)
		: base(texture, rect, srect)
	{
		this.movementSpeed = movementSpeed;
	}

	public void Update(KeyboardState keyState)
	{
		velocity = Vector2.Zero;

		if (keyState.IsKeyDown(Keys.D))
		{
			velocity.X = movementSpeed;
		}
		if (keyState.IsKeyDown(Keys.A))
		{
			velocity.X = -movementSpeed;
		}
		if (keyState.IsKeyDown(Keys.W))
		{
			velocity.Y = -movementSpeed;
		}
		if (keyState.IsKeyDown(Keys.S))
		{
			velocity.Y = movementSpeed;
		}
	}
}
