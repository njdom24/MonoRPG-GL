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
	class OldNPC
	{
		private bool flipped;
		private Animation walkLeft;
		private Animation walkUp;
		private Animation walkDown;
		private Animation curAnim;
		private enum State { Left, Right, Up, Down };
		private State prevState;
		private State curState;
		private double timer;
		private float animSpeed;
		private Texture2D tex;
		public Body body;
		private int animIndex;
		private int[][] colArray;
		private int steps;
		private int curStep;
		private bool backwards;
		private bool vertical;
		private double moveTimer;
		private int x;
		private int y;
		private bool paused;
		public string[] messages;
		public int textWidth;
		public int textHeight;

		private bool canMove;
		private bool isMoving;
		private Vector2 finalPos;

		public OldNPC(World world, ContentManager content, int[][] colArray, int steps, bool vertical, int posX, int posY, string[] messages, int textWidth = 20, int textHeight = 3)
		{
			this.textWidth = textWidth;
			this.textHeight = textHeight;
			this.messages = messages;
			paused = false;
			backwards = false;
			curStep = 1;
			this.steps = steps;
			this.vertical = vertical;
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
			colArray[y][x] = 1;
			tex = content.Load<Texture2D>("overworld_jobs");
			timer = 0;
			moveTimer = 0;

			walkLeft = new Animation(0, 1);
			walkUp = new Animation(2, 3);
			walkDown = new Animation(4, 5);

			if (vertical)
			{
				prevState = State.Down;
				curState = State.Down;
				curAnim = walkDown;
			}
			else
			{
				prevState = State.Right;
				curState = State.Right;
				curAnim = walkLeft;
			}
		}

		public void Update(GameTime gameTime)
		{
			if (!paused)
			{
				if (steps > 0)
				{
					Vector2 tempPos = new Vector2((int)Math.Round(body.Position.X), (int)Math.Round(body.Position.Y));
					body.Position = tempPos;

					if (!isMoving)
					{
						moveTimer += gameTime.ElapsedGameTime.TotalSeconds;
						if (moveTimer >= 1)
						{
							moveTimer -= gameTime.ElapsedGameTime.TotalSeconds;
							if (vertical)
							{
								if (backwards)
								{
									if (colArray[y + 1][x] != 2)
									{
										if (--curStep == 1)
											backwards = false;
										moveDown();
									}
								}
								else
								{
									if (colArray[y - 1][x] != 2)
									{
										if (curStep++ == steps)
											backwards = true;
										moveUp();
									}
								}
							}
							else
							{
								if (backwards)
								{
									if (colArray[y][x - 1] != 2)
									{
										if (--curStep == 1)
											backwards = false;
										moveLeft();
									}
								}
								else
								{
									if (colArray[y][x + 1] != 2)
									{
										if (curStep++ == steps)
											backwards = true;
										moveRight();
									}
								}
							}
						}
						else
						{
							body.LinearVelocity = new Vector2(0, 0);
						}
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
								}
								break;
							case State.Right:
								if (body.Position.X >= finalPos.X)
								{
									isMoving = false;
									body.LinearVelocity = new Vector2(0, 0);
								}
								break;
							case State.Up:
								if (body.Position.Y <= finalPos.Y)
								{
									isMoving = false;
									body.LinearVelocity = new Vector2(0, 0);
								}
								break;
							default:
								if (body.Position.Y >= finalPos.Y)
								{
									isMoving = false;
									body.LinearVelocity = new Vector2(0, 0);
								}
								break;
						}
					}

					tempPos = new Vector2((int)Math.Round(body.Position.X), (int)Math.Round(body.Position.Y));
					body.Position = tempPos;
				}
				Move(gameTime);
			}
		}

		public void Move(GameTime gameTime)
		{
			//Console.WriteLine(timer);
			timer += gameTime.ElapsedGameTime.TotalSeconds;

			if (curState == State.Right && (isMoving))
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
			else if (curState == State.Left && (isMoving))
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
			else if (curState == State.Up && (isMoving))
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
			else if (curState == State.Down && (isMoving))
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
				animIndex = curAnim.getFrame();
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
				sb.Draw(tex, new Rectangle((int)body.Position.X, (int)body.Position.Y, 16, 16), new Rectangle((animIndex + 1) * 16, 48, -16, 16), Color.White);
			else
				sb.Draw(tex, new Rectangle((int)body.Position.X, (int)body.Position.Y, 16, 16), new Rectangle(animIndex * 16, 48, 16, 16), Color.White);
		}

		private void moveUp()
		{
			moveTimer = 0;
			SetState(State.Up);
			animSpeed = 0.25f;
			body.LinearVelocity = new Vector2(0, -64);
			finalPos = new Vector2(body.Position.X, body.Position.Y - 16);
			colArray[y--][x] = 0;
			colArray[y][x] = 1;
			isMoving = true;
		}

		private void moveDown()
		{
			moveTimer = 0;
			SetState(State.Down);
			animSpeed = 0.25f;
			body.LinearVelocity = new Vector2(0, 64);
			finalPos = new Vector2(body.Position.X, body.Position.Y + 16);
			colArray[y++][x] = 0;
			colArray[y][x] = 1;
			isMoving = true;
		}

		private void moveLeft()
		{
			moveTimer = 0;
			SetState(State.Left);
			animSpeed = 0.25f;
			body.LinearVelocity = new Vector2(-64, 0);
			finalPos = new Vector2(body.Position.X - 16, body.Position.Y);
			colArray[y][x--] = 0;
			colArray[y][x] = 1;
			isMoving = true;
		}

		private void moveRight()
		{
			moveTimer = 0;
			SetState(State.Right);
			animSpeed = 0.25f;
			body.LinearVelocity = new Vector2(64, 0);
			finalPos = new Vector2(body.Position.X + 16, body.Position.Y);
			colArray[y][x++] = 0;
			colArray[y][x] = 1;
			isMoving = true;
		}

		public bool checkPlayer(OldPlayer.State state, KeyboardState prevState)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Space) && !prevState.IsKeyDown(Keys.Space))//doesnt check for OOB!
			{
				if (colArray[y][x + 1] == 2 && state == OldPlayer.State.Left)
				{
					SetState(State.Right);
					curAnim = walkLeft;
					return true;
				}
				else if (colArray[y][x - 1] == 2 && state == OldPlayer.State.Right)
				{
					SetState(State.Left);
					curAnim = walkLeft;
					return true;
				}
				else if (colArray[y - 1][x] == 2 && state == OldPlayer.State.Down)
				{
					SetState(State.Up);
					curAnim = walkUp;
					return true;
				}
				else if (colArray[y + 1][x] == 2 && state == OldPlayer.State.Up)
				{
					SetState(State.Down);
					curAnim = walkDown;
					return true;
				}
			}

			return false;
		}

		public bool isStopped()
		{
			return !isMoving;
		}
	}
}

