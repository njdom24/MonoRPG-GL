﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPG
{
	class Battler : Combatant
	{
		private Texture2D portrait;
		private Texture2D text;
		private Texture2D scrollingNums;
		private ScrollingVal hp, pp;
		private Vector2 nameOffset;

		private int posX, posY;
		private Body body;
		private float lastForce;
		private double fullTimer;
		private double moveTimer;
		private Text name;
		//private int nameOffset;

		public Battler(ContentManager contentManager, World world)
		{
			maxHealth = 123;
			health = maxHealth;
			maxMagic = 69;
			magic = maxMagic;

			portrait = contentManager.Load<Texture2D>("Battle/Player");
			text = contentManager.Load<Texture2D>("Textbox/Text");
			scrollingNums = contentManager.Load<Texture2D>("Battle/Numbers/ScrollingNums");//5x8
			posX = Game1.width/2 -61/2;
			posY = Game1.height - 55;
			//posY = Game1.height / 2;
			Console.WriteLine(posX + ", " + posY);
			body = new Body(world, ConvertUnits.ToSimUnits(posX, posY));
			body.BodyType = BodyType.Dynamic;
			body.IgnoreGravity = true;
			lastForce = 130f;
			name = new Text(contentManager, "Travis");
			name.SetColor(Color.Black);
			hp = new ScrollingVal(contentManager, maxHealth);
			pp = new ScrollingVal(contentManager, maxMagic);

			nameOffset = new Vector2((61 - name.width) / 2, 7);
			//nameWidth = letterPos[letterPos.Length - 1];
			//Console.WriteLine("NameWidth: " + nameWidth);
		}
		public override void ForceFinish()
		{
			lastForce = 130f;
			moveTimer = 0;
			body.ResetDynamics();
			body.SetTransform(ConvertUnits.ToSimUnits(posX, posY), 0);
		}

		public override bool IsDone(GameTime gameTime, double combatTimer, KeyboardState prevState)
		{
			fullTimer += gameTime.ElapsedGameTime.TotalSeconds;
			if (fullTimer > 1)
			{
				fullTimer = 0;
				ForceFinish();
				return true;
			}
			else
			{
				moveTimer += gameTime.ElapsedGameTime.TotalSeconds;

				if(moveTimer > 0.1f)
				{
					lastForce *= -0.9f;
					body.ResetDynamics();
					body.ApplyForce(new Vector2(0, lastForce));
					
					//body.LinearVelocity = -body.LinearVelocity;
					moveTimer = 0;
				}
				/*
				if (Math.Abs(lastForce) < 0.2)
				{
					ForceFinish();
					lastForce = 0;
				}
				else if (moveTimer > 0.05)
				{
					Jostle(-0.75f);
					//moveDuration = moveDuration / 1.2;
					moveTimer = 0;
				}
				*/
			}
			//throw new NotImplementedException();
			return false;
		}

		public override void TakeDamage(int damage, double combatTimer)
		{
			Console.WriteLine("K Y K Y");
			health -= damage;
			//body.ResetDynamics();
			body.ApplyForce(new Vector2(0, lastForce));
			//body.LinearVelocity = ConvertUnits.ToSimUnits(0, 150);
			//throw new NotImplementedException();
		}

		

		public void Draw(SpriteBatch sb)
		{
			Vector2 pos = ConvertUnits.ToDisplayUnits(body.Position);
			sb.Draw(portrait, new Rectangle((int)pos.X, (int)pos.Y, 61, 55), new Rectangle(0, 0, 61, 55), Color.White);

			name.Draw(sb, pos + nameOffset);

			hp.Draw(sb, pos, health);
			pos.Y += 11;
			pp.Draw(sb, pos, magic);
		}

		public void Update(GameTime gameTime, KeyboardState state)
		{
			if (state.IsKeyDown(Keys.A))
				health = 0;
			else if (state.IsKeyDown(Keys.S))
				health = 101;
			else if (state.IsKeyDown(Keys.D))
				health = 202;

			hp.Update(gameTime, health);
			pp.Update(gameTime, magic);
			//Console.WriteLine("Divided: " + rollingHealth/10);
		}
	}
}
