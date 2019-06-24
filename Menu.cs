﻿using Microsoft.Xna.Framework;
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
	class Menu
	{
		private Text[][] lines;
		private Texture2D textbox;
		private Texture2D background;
		private int offsetX, offsetY;
		private int width, height;
		private int spacingX, spacingY;
		private int rows, columns;
		private int deathOffX;
		private int prevIndexX, prevIndexY;
		private int highlightWidth;
		private int dividerHeight;

		private int cursorBob;
		private bool cursorRight;
		private double cursorTimer;

		private Selector selectorX, selectorY;
		private string[][] items;
		//width 104
		public Menu(ContentManager contentManager, int rows = 7, int highlightWidth = 86, int width = 25, int height = 12)
		{
			textbox = contentManager.Load<Texture2D>("Textbox/Text");
			background = contentManager.Load<Texture2D>("HighlightColor");
			cursorRight = true;

			string[] tempItems = new string[] {  "Old Bat", null, "Garden Band", "Feathery Charm", "Cookie", "Slingshot", "Local Soda",
									"Cheeseburger", "Belring Map" };

			
			//items = new string[] {  "Offense", "Recovery", "Support" };

			this.highlightWidth = highlightWidth;
			this.width = width;//10
			this.height = height;
			dividerHeight = (height+2) * 8 - 20;
			spacingX = 103;
			spacingY = 14;
			if (tempItems.Length < rows)
				this.rows = items.Length;
			else
				this.rows = rows;

			this.columns = (int)Math.Ceiling((double)tempItems.Length/rows);
			selectorX = new Selector(columns, true);
			selectorY = new Selector(rows, false);
			//automatically figure out height based on textHeight (coincidentally also 8) and spacing

			items = new string[columns][];
			lines = new Text[columns][];
			for (int i = 0; i < columns; i++)
			{
				items[i] = new string[rows];
				lines[i] = new Text[rows];
			}

			for(int i = 0; i < tempItems.Length; i++)
			{
				items[i / rows][i % rows] = tempItems[i];
			}

			for(int i = 0; i < columns; i++)
			for(int j = 0; j < rows; j++)
				if(items[i][j] != null)
					lines[i][j] = new Text(textbox, items[i][j], 51);//the 51 is deprecated

			Console.WriteLine("~~~~~~~~~~~~~~Items~~~~~~~~~~~");

			for(int i = 0; i < items.Length; i++)
			{
				for (int j = 0; j < items[i].Length; j++)
					Console.Write(items[i][j] + ',');
				Console.WriteLine();
			}

			Console.WriteLine("~~~~~~~~~~~~~~Lines~~~~~~~~~~~");
			for (int i = 0; i < items.Length; i++)
			{
				for (int j = 0; j < items[i].Length; j++)
					if(lines[i][j] != null)
					Console.Write(items[i][j] + ',');
				Console.WriteLine();
			}

			lines[0][0].SetColor(Color.Black);
		}

		public void Update(GameTime gameTime, KeyboardState prevState)
		{
			cursorTimer += gameTime.ElapsedGameTime.TotalSeconds;
			bool x = selectorX.Update(prevState);
			bool y = selectorY.Update(prevState);

			if(x || y)
			{
				int posX = selectorX.GetIndex();//0 - 1
				int posY = selectorY.GetIndex();//0 - 6

				if (lines[posX][posY] != null)
				{
					Console.WriteLine("PrevX: " + prevIndexX + "PrevY: " + prevIndexY);
					//lines[prevIndexX][prevIndexY].SetColor(Color.White);
					//lines[posX][posY].SetColor(Color.Black);
				}
				else //Try to skip over horizontal blanks, reset to beginning of row if too low
				{
					//If moving down, if encounter a gap, move to beginning of next available line
					if (posY > prevIndexY)
					{
						for (int i = posY; i < lines[posX].Length; i++)
							if (lines[posX][i] != null)
							{
								posY = i;
								selectorY.SetIndex(i);
								i = lines[posX].Length;//exit
							}
							else
							{
								//iterate through x values of y. first one to be found is where the switch occurs
								for (int j = 0; j < lines.Length; j++)
								{
									if (lines[j][i] != null)
									{
										Console.WriteLine(items[j][i]);
										selectorX.SetIndex(j);
										selectorY.SetIndex(i);
										posX = j;
										posY = i;
										j = lines.Length;//exit
										i = lines[posX].Length;
									}
								}
							}
					}
					else if (posY < prevIndexY)
					{
						for (int i = posY; i >= 0; i--)
							if (lines[posX][i] != null)
							{
								posY = i;
								selectorY.SetIndex(i);
								i = -1;
							}
							else
							{
								//iterate through x values of y. first one to be found is where the switch occurs
								for (int j = 0; j < lines.Length; j++)
									if (lines[j][i] != null)
									{
										Console.WriteLine("Thing found!: " + items[j][i]);
										posY = i;
										posX = j;
										selectorX.SetIndex(j);
										selectorY.SetIndex(i);
										i = -1;
										j = lines[j].Length;//exit
									}
							}
					}
					//if moving right, if NOTHING is found all the way to the right, go to the furthest down element on the next available column
					//or dont
					else if(posX > prevIndexX)
					{
						bool found = false;
						for(int i = posX; i < lines.Length; i++)
							if(lines[i][posY] != null)
							{
								selectorX.SetIndex(i);
								i = lines.Length;//exit
								found = true;
							}
						if(!found)
						{
							for(int i = posX; i < lines.Length; i++)
								for(int j = 0; j < lines[i].Length; j++)
								{
									if(lines[i][j] != null)
									{
										posX = i;
										posY = j;
										selectorX.SetIndex(i);
										selectorY.SetIndex(j);
										//j = lines[i].Length;
										//i = lines.Length - 1;
										Console.WriteLine("Found: :" + items[posX][posY] );
									}
								}
						}
					}
					//if moving left, if NOTHING is found all the way to the left, go to the furthest down element on the next available column
					//or dont
					else if (posX < prevIndexX)
					{
						for (int i = posX; i >= 0; i--)
							if (lines[i][posY] != null)
							{
								selectorX.SetIndex(i);
								i = -1;//exit
							}
					}
				}
				//lines[prevIndexX][prevIndexY].SetColor(Color.White);
				//lines[posX][posY].SetColor(Color.Black);
			}

			if (cursorTimer > 0.15)
			{
				cursorTimer -= 0.15;

				if (cursorRight)
				{
					cursorBob++;
					if (cursorBob > 2)
						cursorRight = false;
				}
				else
				{
					cursorBob--;
					if (cursorBob < 1)
						cursorRight = true;
				}
			}

			prevIndexX = selectorX.GetIndex();
			prevIndexY = selectorY.GetIndex();
		}

		public void Draw(SpriteBatch sb)
		{
			DrawBlank(sb);
			Vector2 pos = new Vector2(offsetX + 16, offsetY + 7);

			sb.Draw(background, new Rectangle((int)pos.X - 3 + spacingX*selectorX.GetIndex(), (int)pos.Y + spacingY*selectorY.GetIndex() + 1, highlightWidth, 14), new Rectangle(0, 0, 1, 1), Color.White);
			//for loop it, also move it down with an offset and change 96 into something calculated
			sb.Draw(background, new Rectangle((int)(pos.X - 3 + (spacingX + highlightWidth)/2 + 1.51), (int)pos.Y + 3, 1, dividerHeight), new Rectangle(1, 0, 1, 1), Color.White);


			sb.Draw(textbox, new Rectangle((int)pos.X - 10 + cursorBob + spacingX*selectorX.GetIndex(), (int)pos.Y + spacingY * selectorY.GetIndex() + 3, 6, 9), new Rectangle(48, 96, 6, 9), Color.White);

			for(int i = 0; i < lines.Length; i++)
			{
				for(int j = 0; j < lines[i].Length; j++)
				{
					if(lines[i][j] != null)
						lines[i][j].Draw(sb, pos);

					pos.Y += spacingY;
				}
				pos.X += spacingX;
				pos.Y -= spacingY * lines[0].Length;
			}

		}
		//48, 96
		private void DrawBlank(SpriteBatch sb)
		{
			//UL corner
			sb.Draw(textbox, new Rectangle(offsetX, offsetY, 8, 8), new Rectangle(0 + deathOffX, 112, 8, 8), Color.White);
			//BL corner
			sb.Draw(textbox, new Rectangle(offsetX, offsetY + (height + 1) * 8, 8, 8), new Rectangle(0 + deathOffX, 120, 8, 8), Color.White);
			//UR corner
			sb.Draw(textbox, new Rectangle(offsetX + (width + 1) * 8, offsetY, 8, 8), new Rectangle(8 + deathOffX, 112, 8, 8), Color.White);
			//BR corner
			sb.Draw(textbox, new Rectangle(offsetX + (width + 1) * 8, offsetY + (height + 1) * 8, 8, 8), new Rectangle(8 + deathOffX, 120, 8, 8), Color.White);

			//top&bottom
			//sb.Draw(textbox, new Rectangle(offsetX + (1) * 8, offsetY, width*8, 8), new Rectangle(16, 112, 1, 8), Color.White);
			//sb.Draw(textbox, new Rectangle(offsetX + (1) * 8, offsetY + (height + 1) * 8, width*8, 8), new Rectangle(17, 112, 1, 8), Color.White);
			for (int i = 0; i < width; i++)
			{
				sb.Draw(textbox, new Rectangle(offsetX + (i + 1) * 8, offsetY, 8, 8), new Rectangle(16 + deathOffX, 112, 8, 8), Color.White);//top
				sb.Draw(textbox, new Rectangle(offsetX + (i + 1) * 8, offsetY + (height + 1) * 8, 8, 8), new Rectangle(16 + deathOffX, 120, 8, 8), Color.White);//bottom
			}

			//left&right
			//sb.Draw(textbox, new Rectangle(offsetX, offsetY + (1) * 8, 8, 8*height), new Rectangle(20, 112, 8, 1), Color.White);//left
			//sb.Draw(textbox, new Rectangle(offsetX + (width+1)*8, offsetY + (1) * 8, 8, 8*height), new Rectangle(26, 112, 8, 1), Color.White);//right
			for (int i = 0; i < height; i++)
			{
				sb.Draw(textbox, new Rectangle(offsetX, offsetY + (i + 1) * 8, 8, 8), new Rectangle(24 + deathOffX, 112, 8, 8), Color.White);
				sb.Draw(textbox, new Rectangle(offsetX + (width + 1) * 8, offsetY + (i + 1) * 8, 8, 8), new Rectangle(24 + deathOffX, 120, 8, 8), Color.White);
			}


			//fill inside with black
			sb.Draw(textbox, new Rectangle(offsetX + 8, offsetY + 8, 8 * width, 8 * height), new Rectangle(7 + deathOffX, 119, 1, 1), Color.White);
			//215, 40
			//26 * 5
		}

		public void DeathMode(bool enabled)
		{
			if (enabled)
			{
				deathOffX = 32;
				//textColor = new Color(245, 139, 148);
			}
			else
			{
				deathOffX = 0;
				//textColor = Color.White;
			}
		}
	}
}
