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
    class OldHud
    {
        private Texture2D chars;
        private Texture2D borders;
        private string[] messages;
        private Point[][] locations;
        private int width;
        private int height;
        private int offsetX;
        private int offsetY;
        private int posX;
        private int posY;
        private double timer;
        private int charCount;
        private int curMessage;
        private bool visible;
		private bool canClose;

        public OldHud(string[] message, ContentManager content, int width = 18, int height = 3, int posX = -1, int posY = -1, bool canClose = true)
        {
			this.canClose = canClose;
            this.posX = posX;
            this.posY = posY;
            visible = true;
            curMessage = 0;
            charCount = 0;
            timer = 0;
            this.width = width;
            this.height = height;
            if (posX == -1)
            {
                offsetX = (400 - width * 8) / 2;
                offsetY = 240 - (height + 2) * 8;
            }
            else
            {
                offsetX = posX;
                offsetY = posY;
            }
            this.chars = content.Load<Texture2D>("Textbox/Chars");
            this.borders = content.Load<Texture2D>("Textbox/Textbox");
            this.messages = message;
            //message = message.ToUpper();
            locations = new Point[messages.Length][];//[message.Length];
            for (int i = 0; i < locations.Length; i++)
                locations[i] = new Point[messages[i].Length];
            for(int i = 0; i < messages.Length; i++)
                for(int j = 0; j < messages[i].Length; j++)
                {
                    char letter = message[i][j];
                    if (letter >= 'A' && letter <= 'J')
                        locations[i][j] = new Point(8 * (letter - 'A'), 0);
                    else if (letter >= 'K' && letter <= 'U')
                        locations[i][j] = new Point(8 * (letter - 'K'), 8);
                    else if (letter >= 'V' && letter <= 'Z')
                        locations[i][j] = new Point(8 * (letter - 'V'), 16);
                    else if (letter >= 'a' && letter <= 'e')
                        locations[i][j] = new Point(40 + 8 * (letter - 'a'), 16);
                    else if (letter >= 'f' && letter <= 'o')
                        locations[i][j] = new Point(8 * (letter - 'f'), 24);
                    else if (letter >= 'p' && letter <= 'z')
                        locations[i][j] = new Point(8 * (letter - 'p'), 32);
                    else if (letter == '-')
                        locations[i][j] = new Point(0, 40);
                    else if (letter == '"')
                        locations[i][j] = new Point(8, 40);
                    else if (letter == '!')
                        locations[i][j] = new Point(16, 40);
                    else if (letter == '?')
                        locations[i][j] = new Point(24, 40);
                    else if (letter == '\'')
                        locations[i][j] = new Point(32, 40);
                    else if (letter == ',')
                        locations[i][j] = new Point(40, 40);
                    else if (letter == '.')
                        locations[i][j] = new Point(48, 40);
                    else if (letter == '/')
                        locations[i][j] = new Point(56, 40);
                    else if (letter == '<')
                        locations[i][j] = new Point(64, 40);
                    else if (letter == '>')
                        locations[i][j] = new Point(72, 40);
                    else if (letter >= '0' && letter <= '9')
                        locations[i][j] = new Point(8 * ((int)letter - (int)'0'), 48);
                    else
                        locations[i][j] = new Point(80, 0);
                }
        }

		public void finishMessage()
		{
			//curMessage = messages.Length - 1;
			charCount = messages[curMessage].Length;
			//visible = !canClose;
		}

		public void finishText()
		{
			curMessage = messages.Length - 1;
			charCount = messages[curMessage].Length;
			visible = !canClose;
		}

		public void Update(GameTime gameTime, KeyboardState prevState)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !prevState.IsKeyDown(Keys.Space))
            {
                if (true)
                {
                    if (isFinished())// && curMessage < messages.Length - 1)
                    {
                        if (curMessage < messages.Length - 1)
                        {
							//advance message
                            curMessage++;
                            charCount = 0;
                        }
                        else
                        {
                            //close textbox
                            visible = !canClose;
                        }
                    }
                    else
                    {
                        if (charCount < messages[curMessage].Length)
                        {
                            //skip text
                            charCount = messages[curMessage].Length;
                        }
                        
                    }
                }
                //spacePressedLastFrame = true;
            }
            else
            {
                //spacePressedLastFrame = false;
                timer += gameTime.ElapsedGameTime.TotalSeconds;
                if (timer >= 0.08)
                {
                    timer = 0;
                    if (charCount < messages[curMessage].Length)
                        charCount++;
                }
            }
        }
        private void DrawBlank(SpriteBatch sb)
        {
            //UL corner
            sb.Draw(borders, new Rectangle(offsetX, offsetY, 8, 8), new Rectangle(7 * 8, 0, 8, 8), Color.White);
            //BL corner
            sb.Draw(borders, new Rectangle(offsetX, offsetY + (height + 1) * 8, 8, 8), new Rectangle(2 * 8, 0, 8, 8), Color.White);
            //UR corner
            sb.Draw(borders, new Rectangle(offsetX + (width+1)*8, offsetY, 8, 8), new Rectangle(8 * 8, 0, 8, 8), Color.White);
            //BR corner
            sb.Draw(borders, new Rectangle(offsetX + (width+1)*8, offsetY + (height + 1) * 8, 8, 8), new Rectangle(3 * 8, 0, 8, 8), Color.White);
            //left&right
            for (int i = 0; i < height; i++)
            {
                sb.Draw(borders, new Rectangle(offsetX, offsetY + (i+1)*8, 8, 8), new Rectangle(4 * 8, 0, 8, 8), Color.White);
                sb.Draw(borders, new Rectangle(offsetX + (width+1)*8, offsetY + (i+1)*8, 8, 8), new Rectangle(5 * 8, 0, 8, 8), Color.White);
            }
            
            //top&bottom
            for (int i = 0; i < width; i++)
            {
                sb.Draw(borders, new Rectangle(offsetX + (i+1) * 8, offsetY, 8, 8), new Rectangle(6 * 8, 0, 8, 8), Color.White);
                sb.Draw(borders, new Rectangle(offsetX + (i+1) * 8, offsetY + (height+1)*8, 8, 8), new Rectangle(1 * 8, 0, 8, 8), Color.White);
            }
            //fill inside with blanks
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    sb.Draw(chars, new Rectangle(offsetX + (j + 1) * 8, offsetY + (i + 1) * 8, 8, 8), new Rectangle(80, 0, 8, 8), Color.White);
                }
            }
            
        }
        public bool isFinished()
        {
            return charCount == messages[curMessage].Length;
        }
 
        public void Draw(SpriteBatch sb)
        {
            if (visible)
            {
                DrawBlank(sb);
                int c = 0;
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        if (c < charCount)
                        {
                            if (messages[curMessage].ElementAt<char>(c) == '\n')
                            {
                                i++;
                                j = -1;
                            }
                            else
                                sb.Draw(chars, new Rectangle(offsetX + (j + 1) * 8, offsetY + (i + 1) * 8, 8, 8), new Rectangle(locations[curMessage][c].X, locations[curMessage][c].Y, 8, 8), Color.White);
                            c++;
                        }
                //sb.Draw(chars, new Rectangle(0, 0, 8, 8), new Rectangle(0, 0, 8, 8), Color.White);
                //sb.Draw(chars, new Rectangle(8, 0, 8, 8), new Rectangle(32, 0, 8, 8), Color.White);
            }
        }

        public bool messageComplete()
        {
            return !visible;
        }

		public int getHeight()
		{
			return height * 8;
		}
    }
}
