using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG
{
	interface Screen
	{
		void Update(GameTime dt);
		void Draw(SpriteBatch sb);
	}
}
