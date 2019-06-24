﻿using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG
{
	class Battle : Screen
	{
		private Enemy flasher;
		private double flashTimer;
		private float timerMult;
		private byte flashCounter;

		private Enemy knight;
		private Battler travis;

		private Texture2D blackRect;
		private Texture2D background;
		private Texture2D background2;
		private Texture2D youWon;
		private bool victory;
		private double victoryTimer;
		private Color victoryColor;
		
		private Effect effect;
		private Effect flash;
		private double bgTimer;
		private double waiterTimer;
		
		private RenderTarget2D firstEffect;
		private RenderTarget2D secondEffect;
		private RenderTarget2D comboEffect;
		private RenderTarget2D final;
		private GraphicsDevice graphicsDevice;
		private Hud text;
		private Hud commandName;
		private KeyboardState prevState;
		private ContentManager content;
		private int MultiSampleCount;
		private Icons options;
		private Selector selector;

		private World world;
		private Combatant waiter;

		private int offsetHeightTop;
		private int offsetHeightBottom;

		private float secondsPerBeat;
		private double combatTimer;
		private double threshHold;
		private Texture2D combatIndicator;

		private int playerMove;
		private int enemyMove;

		private enum Phase {IntroPhase, PlayerPhase, SelectTarget, EnemyPhase, AttackPhase, AnimPhase, BlinkPhase, PlayerDeathPhase, EnemyDeathPhase, YouWin};
		private Phase curPhase;
		private double turnWaiter;

		private Animation magicAnim;
		private Texture2D magic;
		private double animTimer;
		private double darkenTimer;
		private bool enemyDraw;
		private byte flashCount;

		private bool deathMessageDisplayed;

		public Battle(ContentManager contentManager, RenderTarget2D final, GraphicsDevice graphicsDevice, PresentationParameters pp)
		{
			curPhase = Phase.IntroPhase;
			effect = contentManager.Load<Effect>("Battle/BattleBG");
			effect.CurrentTechnique = effect.Techniques[1];
			flash = contentManager.Load<Effect>("Battle/SpriteFlash");
			combatTimer = 0;
			threshHold = 0.15;
			combatIndicator = contentManager.Load<Texture2D>("Battle/Icons/Attack");
			youWon = contentManager.Load<Texture2D>("Battle/Icons/YouWon");
			victoryColor = Color.White;
			secondsPerBeat = 0.5f;
			world = new World(ConvertUnits.ToSimUnits(0,Game1.width));
			waiter = null;
			options = new Icons(contentManager);
			blackRect = new Texture2D(graphicsDevice, 1, 1);
			blackRect.SetData(new Color[] { Color.Black });

			travis = new Battler(contentManager, world);
			knight = new Enemy(contentManager, world, secondsPerBeat, threshHold);
			enemyDraw = true;

			MultiSampleCount = pp.MultiSampleCount;
			Texture2D palette = contentManager.Load<Texture2D>("Battle/003Palette");
			effect.Parameters["palette"].SetValue(palette);
			effect.Parameters["paletteWidth"].SetValue((float)palette.Width);
			effect.Parameters["time"].SetValue((float)bgTimer);
			flash.Parameters["time"].SetValue((float)flashTimer);
			firstEffect = new RenderTarget2D(graphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, MultiSampleCount, RenderTargetUsage.DiscardContents);
			secondEffect = new RenderTarget2D(graphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, MultiSampleCount, RenderTargetUsage.DiscardContents);
			comboEffect = new RenderTarget2D(graphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, MultiSampleCount, RenderTargetUsage.DiscardContents);
			content = contentManager;
			prevState = Keyboard.GetState();
			selector = new Selector(4, names: new string[] {"Attack", "Bag", "PSI", "Run"});
			background = contentManager.Load<Texture2D>("Battle/005");
			background2 = content.Load<Texture2D>("Battle/Yellow");
			magic = contentManager.Load<Texture2D>("Battle/Effects/PkFireA");
			magicAnim = new Animation(0, 24);
			
			bgTimer = 0;
			//graphicsDevice.Textures[2] = palette;
			this.final = final;//required for scaling
			this.graphicsDevice = graphicsDevice;
			text = new Hud(new string[] { "@Knight draws near!" }, content, 30, 2, posY: 3, canClose: true);
			//text.finishText();
			commandName = new Hud(new string[] { selector.GetName() }, content, 6, 1, Game1.width - (8 * 9), 4, canClose: false);
			offsetHeightBottom = text.getHeight();
			offsetHeightTop = 32;
			flashCounter = 1;
			timerMult = 1;

			darkenTimer = 1;
		}

		public void DrawBackground(SpriteBatch sb)
		{
			graphicsDevice.SetRenderTarget(firstEffect);
			sb.Begin(sortMode: SpriteSortMode.Immediate);
			effect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			sb.Draw(background, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);//Draw to texture
			sb.End();

			//////////////////////////////////Second Background///////////////////////////////////
			/*
			graphicsDevice.SetRenderTarget(secondEffect);
			sb.Begin(sortMode: SpriteSortMode.Immediate);
			effect.CurrentTechnique.Passes[1].Apply();
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			sb.Draw(background2, new Rectangle(0, 0, 400, 240), Color.White);//Draw to texture
			sb.End();
			*/
			graphicsDevice.SetRenderTarget(final);
			sb.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Immediate);
			//Console.WriteLine("Count: " + flash.Techniques.Count);

			sb.Draw(firstEffect, new Rectangle(0, 0, Game1.width, Game1.height), Color.White * (float)darkenTimer);
			//sb.Draw(secondEffect, new Rectangle(0, 0, 400, 240), Color.White * 0f);

			sb.End();
		}

		private void DrawHud(SpriteBatch sb)
		{
			sb.Begin(samplerState: SamplerState.PointClamp);
			sb.Draw(blackRect, new Rectangle(0, Game1.height - 35, Game1.width, 35), Color.Black);
			sb.Draw(blackRect, new Rectangle(0, 0, Game1.width, 32), Color.Black);
			if (curPhase == Phase.PlayerPhase)
			{
				
			
				if (waiter == null)
				{
					
					commandName.Draw(sb);
				}

				
				//sb.End();

				if (waiter == null)
					options.Draw(sb, selector.GetIndex());
			}

			if (combatTimer > secondsPerBeat - threshHold)// && combatTimer < 0.6)
			{
				//sb.Draw(combatIndicator, new Rectangle(200 - 12, 4, combatIndicator.Width * 2, combatIndicator.Height * 2), Color.White);
				if (combatTimer > secondsPerBeat + threshHold)
					combatTimer = threshHold;
			}
			sb.End();

			if (selector.IndexChanged())
				commandName = new Hud(new string[] { selector.GetName() }, content, 6, 1, Game1.width-(8*9), 4, canClose: false);
		}

		public void Draw(SpriteBatch sb)
		{
			//graphicsDevice.Clear(Color.White);
			DrawBackground(sb);
			DrawHud(sb);
			if(flasher == null)
				sb.Begin(samplerState: SamplerState.PointClamp);
			else
			{
				sb.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Immediate);
				flash.Techniques[0].Passes[0].Apply();
			}

			if(enemyDraw)
				knight.Draw(sb, bgTimer, offsetHeightTop, offsetHeightBottom);
			sb.End();
			sb.Begin(samplerState: SamplerState.PointWrap);

			travis.Draw(sb);
			//if(waiter == null)// || messageBuffered)
			if(!victory || turnWaiter > 0.4)
				text.Draw(sb);

			if(victory && turnWaiter > 0.4)
			{
				sb.Draw(youWon, new Rectangle((Game1.width - 102)/2, 18, 102, 10), new Rectangle(0, 0, 102, 10), victoryColor);
			}

			if(curPhase == Phase.AnimPhase && !text.visible)
			{
				int frame = magicAnim.getFrame();
				sb.Draw(magic, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle((frame % 4) * Game1.width, (frame / 4)*Game1.height, Game1.width, Game1.height), Color.White);
			}
			sb.End();
		}

		public void Update(GameTime gameTime)
		{
			if (flasher != null)
			{
				if (flashCounter % 2 == 0)
					flashTimer -= gameTime.ElapsedGameTime.TotalSeconds * timerMult;
				else
					flashTimer += gameTime.ElapsedGameTime.TotalSeconds * timerMult;
				if(flasher.health > 0)//Attack Flash
					flash.Parameters["time"].SetValue((float)(flashTimer + 0.1));
				else//Dying flash
					flash.Parameters["time"].SetValue((float)(flashTimer/1.1 + 0.01));//the added value controls the offset, multiplication controls transition speed

				if (flashTimer > 0.2)// || flashTimer < 0)
				{
					flashTimer = 0.2;
					flashCounter++;
				}
				else if(flashTimer < 0)
				{
					flashTimer = 0;
					flashCounter++;
				}
				if (flashCounter > 4)
				{
					flasher = null;
					flashTimer = 0;
					flashCounter = 1;
				}
			}

			knight.Update(gameTime);

			travis.Update(gameTime, Keyboard.GetState());

			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
			combatTimer += gameTime.ElapsedGameTime.TotalSeconds;
			waiterTimer += gameTime.ElapsedGameTime.TotalSeconds;
			turnWaiter += gameTime.ElapsedGameTime.TotalSeconds;

			commandName.finishMessage();
			commandName.Update(gameTime, prevState);
			UpdateBackground(gameTime);

			if (curPhase == Phase.EnemyDeathPhase)
			{
				if (flasher == null)//Executed after flashing finishes
				{
					knight.Kill();
					if (text.messageComplete() && !deathMessageDisplayed)
					{
						text = new Hud(new string[] { knight.deathMessage }, content, 30, 2, posY: 3, canClose: true);
						deathMessageDisplayed = true;
					}
				}
				Console.WriteLine("flash: " + flashCounter);

				if(text.messageComplete())
					if(flashCounter == 1)
					{
						text = new Hud(new string[] { "" }, content, 30, 2, posY: 3, canClose: false);
						victory = true;
						turnWaiter = 0;
					}
			}


			if (turnWaiter > 0.2)
			{
				if (text.messageComplete())
				{
					if (waiter != null)
					{
						if (waiter.attacked)
						{
							if (waiter is Enemy)
							{
								if (waiter.IsDone(gameTime, combatTimer, prevState))
								{
									waiter.ForceFinish();
									waiter = null;
									turnWaiter = 0;
								}
							}
							else
							{
								if (waiter.IsDone(gameTime, waiterTimer, prevState))
								{
									waiterTimer = 0;
									waiter.ForceFinish();
									waiter = null;
									turnWaiter = 0;
								}
							}
						}
						else
						{
							waiter.TakeDamage(10, combatTimer);
							waiter.attacked = true;
						}
					}
					else
						advanceBattle(gameTime);
				}
				//else
					//text.Update(gameTime, prevState);


				if (victory && turnWaiter > 0.4)
				{
					victoryTimer += gameTime.ElapsedGameTime.TotalSeconds;

					if (victoryTimer < 0.2)
						victoryColor = new Color(77, 48, 129);//purple
					else if (victoryTimer < 0.3)
						victoryColor = new Color(14, 134, 247);//blue
					else if (victoryTimer < 0.6)
						victoryColor = new Color(12, 251, 255);//light blue
					else if (victoryTimer < 0.7)
						victoryColor = new Color(14, 134, 247);//blue
					else if (victoryTimer < 0.8)
						victoryColor = new Color(12, 251, 255);//light blue
					else if (victoryTimer < 1.9)
						victoryColor = Color.White;
					else
						victoryColor = new Color(216, 254, 177);//yellow
				}
			}
			text.Update(gameTime, prevState);

			prevState = Keyboard.GetState();
		}

		private void advanceBattle(GameTime gameTime)
		{
			//Console.WriteLine("Calling advance!");
			switch (curPhase)
			{
				case Phase.IntroPhase:
					if(text.messageComplete())
						curPhase = Phase.PlayerPhase;
					break;
				case Phase.PlayerPhase:
					if (waiter == null)
						selector.Update(prevState);

					if (Keyboard.GetState().IsKeyDown(Keys.Space) && prevState.IsKeyUp(Keys.Space))
					{
						if (selector.GetIndex() == 0)
						{
							playerMove = 0;
							curPhase = Phase.EnemyPhase;
						}
						else if (selector.GetIndex() == 1)
						{
							playerMove = 1;
							magic = content.Load<Texture2D>("Battle/Effects/PkFireA");
							curPhase = Phase.EnemyPhase;
						}
					}
					break;
				case Phase.EnemyPhase:
					enemyMove = 0;
					curPhase = Phase.AttackPhase;
					break;
				case Phase.AttackPhase:
					if(knight.health <= 0)
					{
						deathMessageDisplayed = false;
						curPhase = Phase.EnemyDeathPhase;
						flasher = knight;
						flashTimer = -0.5;
						flashCounter = 2;
						timerMult = 0.5f;
						//text = new Hud(new string[] { "@The knight dissipates into hollow armor." }, content, 30, 2, posY: 3, canClose: true);
					}
					else
					if(playerMove != -1)
					{
						if (playerMove == 0)
						{
							knight.attacked = false;
							text = new Hud(new string[] { "@Travis's attack!" }, content, 30, 2, posY: 3, canClose: true);
							//knight.TakeDamage(1, combatTimer);
							waiter = knight;
							//playerMove = -1;
						}
						else if(playerMove == 1)
						{
							text = new Hud(new string[] { "@Travis tried PK Fire [!" }, content, 30, 2, posY: 3, canClose: true);
							Console.WriteLine("GetFrame: " + magicAnim.getFrame());
							magicAnim.resetStart();
							darkenTimer = 1;
							curPhase = Phase.AnimPhase;
							//if (magicAnim.getFrame() == 25)
								//playerMove = -1;
							//playerMove = -1;
							//TODO: Something
						}
						playerMove = -1;
					}
					else if(enemyMove == 0)
					{
						flasher = knight;
						flashTimer = 0;

						travis.attacked = false;
						text = new Hud(new string[] { "@Knight's attack!" }, content, 30, 2, posY: 3, canClose: true);
						//travis.TakeDamage(1, combatTimer);
						waiter = travis;
						enemyMove = -1;
						timerMult = 1;
						//flashCounter = 1;
						//flashTimer = 0;
					}
					else
					{
						curPhase = Phase.PlayerPhase;
					}
					break;
				case Phase.EnemyDeathPhase:
					//Refer to the if statement in the Update() function

					break;
				case Phase.AnimPhase:
					Console.WriteLine("anim");

					if (magicAnim.getFrame() == magicAnim.frameCount)
					{
						Console.WriteLine("Skadoosh");
						if (darkenTimer < 1)
							darkenTimer += gameTime.ElapsedGameTime.TotalSeconds * 4;
						else//end phase
						{
							//curPhase = Phase.AttackPhase;
							curPhase = Phase.BlinkPhase;
							enemyDraw = false;
							magicAnim.resetStart();
							animTimer = 0;
						}
					}
					else
					{
						if (darkenTimer > 0.5)
							darkenTimer -= gameTime.ElapsedGameTime.TotalSeconds * 5;

						animTimer += gameTime.ElapsedGameTime.TotalSeconds;

						if (animTimer > 0.05)
						{
							animTimer -= 0.05;
							magicAnim.advanceFrame();
						}
					}
					break;
				case Phase.BlinkPhase:
					animTimer += gameTime.ElapsedGameTime.TotalSeconds;

					if(animTimer > 0.07)
					{
						flashCount++;
						animTimer -= 0.07;
						if(flashCount < 11)
							enemyDraw = !enemyDraw;
					}

					if (flashCount == 11)
					{
						//flashCount = 0;
						enemyDraw = true;
						//curPhase = Phase.AttackPhase;
					}
					else if(flashCount == 17)
					{
						flashCount = 0;
						curPhase = Phase.AttackPhase;
					}
					break;
				default:
					break;
			}
		}

		private void UpdateBackground(GameTime gameTime)
		{
			bgTimer += gameTime.ElapsedGameTime.TotalSeconds;
			if (bgTimer > Math.PI * 2)
			{
				//bgTimer -= Math.PI*2;
				//Console.WriteLine("Timer reset");
			}
			effect.Parameters["time"].SetValue((float)bgTimer);
		}
	}
}
