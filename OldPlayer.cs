using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG
{
	class OldPlayer
	{
		private bool flipped;
		private Animation walkLeft;
		private Animation walkUp;
		private Animation walkDown;
		private Animation curAnim;
		public enum State { Left, Right, Up, Down };
		private State prevState;
		private State curState;
		private double timer;
		private float animSpeed;
		private Texture2D tex;
		public Body body;
		private int animIndex;
		private int[][] colArray;
		private int x;
		private int y;

		private bool canMove;
		private bool isMoving;
		private Vector2 finalPos;

		public OldPlayer(World world, ContentManager content, int[][] colArray, int posX, int posY)
		{
			animSpeed = 0.25f;
			isMoving = false;
			canMove = true;
			this.colArray = colArray;
			flipped = false;
			body = new Body(world, new Vector2(0, 0));
			//body = BodyFactory.CreateRectangle(world, 10, 10, 1, new Vector2(0,0));
			//body.Position = new Vector2(8 * 31, 23*16);
			body.BodyType = BodyType.Kinematic;
			body.Position = new Vector2(posX * 16, posY * 16);
			x = posX;
			y = posY;
			colArray[y][x] = 2;
			//body.Position = new Vector2(16 * 16, 14 * 16);
			tex = content.Load<Texture2D>("overworld_jobs");
			timer = 0;
			prevState = State.Up;
			curState = State.Up;
			walkLeft = new Animation(0, 1);
			walkUp = new Animation(2, 3);
			walkDown = new Animation(4, 5);
			curAnim = walkUp;
		}

		public void HandleInput()
		{

		}

		public void Update(GameTime gameTime, bool paused)
		{
			Vector2 tempPos = new Vector2((int)Math.Round(body.Position.X), (int)Math.Round(body.Position.Y));
			body.Position = tempPos;

			if (!isMoving && !paused)
			{
				if (Keyboard.GetState().IsKeyDown(Keys.Right) && x + 1 < colArray[y].Length)
				{
					SetState(State.Right);
					//myMap.Camera.Move(new Vector2(2, 0));

					if (colArray[y][x + 1] == 1)// || colArray[y][x + 1] == 3)
						animSpeed = 0.5f;
					else
					{
						animSpeed = 0.25f;
						body.LinearVelocity = new Vector2(64, 0);
						finalPos = new Vector2(body.Position.X + 16, body.Position.Y);
						colArray[y][x++] = 0;
						colArray[y][x] = 2;
						isMoving = true;
					}
				}
				else if (Keyboard.GetState().IsKeyDown(Keys.Left) && x > 0)
				{
					SetState(State.Left);
					//myMap.Camera.Move(new Vector2(2, 0));
					if (colArray[y][x - 1] == 1 || colArray[y][x - 1] == 3)
						animSpeed = 0.5f;
					else
					{
						animSpeed = 0.25f;
						body.LinearVelocity = new Vector2(-64, 0);
						finalPos = new Vector2(body.Position.X - 16, body.Position.Y);
						colArray[y][x--] = 0;
						colArray[y][x] = 2;
						isMoving = true;
					}
				}
				else if (Keyboard.GetState().IsKeyDown(Keys.Up) && y > 0)
				{
					SetState(State.Up);
					//myMap.Camera.Move(new Vector2(2, 0));
					if (colArray[y - 1][x] == 1 || colArray[y - 1][x] == 3)
						animSpeed = 0.5f;
					else
					{
						animSpeed = 0.25f;
						body.LinearVelocity = new Vector2(0, -64);
						finalPos = new Vector2(body.Position.X, body.Position.Y - 16);
						colArray[y--][x] = 0;
						colArray[y][x] = 2;
						isMoving = true;
					}
				}
				else if (Keyboard.GetState().IsKeyDown(Keys.Down) && y + 1 < colArray.Length)
				{
					SetState(State.Down);
					//myMap.Camera.Move(new Vector2(2, 0));
					if (colArray[y + 1][x] == 1 || colArray[y + 1][x] == 3)
						animSpeed = 0.5f;
					else
					{
						animSpeed = 0.25f;
						body.LinearVelocity = new Vector2(0, 64);
						finalPos = new Vector2(body.Position.X, body.Position.Y + 16);
						colArray[y++][x] = 0;
						colArray[y][x] = 2;
						isMoving = true;
					}
				}
				else
					body.LinearVelocity = new Vector2(0, 0);
			}
			else
			{
				switch (curState)
				{
					case State.Left:
						if (body.Position.X <= finalPos.X)
						{
							isMoving = false;
							body.LinearVelocity = new Vector2(0, 0);
							body.SetTransform(finalPos, 0);
						}
						break;
					case State.Right:
						if (body.Position.X >= finalPos.X)
						{
							isMoving = false;
							body.LinearVelocity = new Vector2(0, 0);
							body.SetTransform(finalPos, 0);
						}
						break;
					case State.Up:
						if (body.Position.Y <= finalPos.Y)
						{
							isMoving = false;
							body.LinearVelocity = new Vector2(0, 0);
							body.SetTransform(finalPos, 0);
						}
						break;
					default:
						if (body.Position.Y >= finalPos.Y)
						{
							isMoving = false;
							body.LinearVelocity = new Vector2(0, 0);
							body.SetTransform(finalPos, 0);
						}
						break;
				}
			}
			tempPos = new Vector2((int)Math.Round(body.Position.X), (int)Math.Round(body.Position.Y));
			body.Position = tempPos;
			if (!paused)
				Move(gameTime);
		}

		public void Move(GameTime gameTime)
		{
			//Console.WriteLine(timer);
			timer += gameTime.ElapsedGameTime.TotalSeconds;

			if (curState == State.Right && (isMoving || Keyboard.GetState().IsKeyDown(Keys.Right)))
			{
				flipped = true;
				SetState(State.Right);
				curAnim = walkLeft;
				//animIndex = (int)State.Left;
				if (timer > animSpeed)
				{
					walkLeft.advanceFrame();
					timer = 0;
				}
				animIndex = walkLeft.getFrame();
			}
			else if (curState == State.Left && (isMoving || Keyboard.GetState().IsKeyDown(Keys.Left)))
			{
				flipped = false;
				SetState(State.Left);
				curAnim = walkLeft;
				//animIndex = (int)State.Left;
				if (timer > animSpeed)
				{
					walkLeft.advanceFrame();
					timer = 0;
				}
				animIndex = walkLeft.getFrame();
			}
			else if (curState == State.Up && (isMoving || Keyboard.GetState().IsKeyDown(Keys.Up)))
			{
				flipped = false;
				SetState(State.Up);
				curAnim = walkUp;
				//animIndex = (int)State.Left;
				if (timer > animSpeed)
				{
					walkUp.advanceFrame();
					timer = 0;
				}
				animIndex = walkUp.getFrame();
			}
			else if (curState == State.Down && (isMoving || Keyboard.GetState().IsKeyDown(Keys.Down)))
			{
				flipped = false;
				SetState(State.Down);
				curAnim = walkDown;
				//animIndex = (int)State.Left;
				if (timer > animSpeed)
				{
					walkDown.advanceFrame();
					timer = 0;
				}
				animIndex = walkDown.getFrame();
			}
			else
			{
				timer = 0;
				curAnim.resetStart();
				animIndex = curAnim.getFrame();//sets current frame to standing animation
				curAnim.resetEnd();
			}
		}
		private void SetState(State s)
		{
			if (prevState != curState)
				curAnim.resetEnd();
			prevState = curState;
			curState = s;
		}
		public void Draw(SpriteBatch sb)
		{
			//Need to add +1 to animIndex if flipped
			if (curState == State.Right)
				sb.Draw(tex, new Rectangle((int)body.Position.X, (int)body.Position.Y, 16, 16), new Rectangle((animIndex + 1) * 16, 32, -16, 16), Color.White);
			else
				sb.Draw(tex, new Rectangle((int)body.Position.X, (int)body.Position.Y, 16, 16), new Rectangle(animIndex * 16, 32, 16, 16), Color.White);
		}

		public State GetState()
		{
			return curState;
		}

		public bool isStopped()
		{
			return !isMoving;
		}
	}
}
