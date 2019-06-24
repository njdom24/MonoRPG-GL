
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG
{
    class OldMap : Screen
    {
        private Effect effect;
		private DebugViewXNA debugView;
		private KeyboardState prevState;//used by both npcs and textboxes
        private Hud hud;
        private int[][] colArray;
        private OldNPC[] npcs;
        private int tileWidth;
        private int tileHeight;
        private int width;
        private int height;
        private GraphicsDevice g;
        private Texture2D debug;
        //private Texture2D light;
        private ContentManager cont;

        private List<Body> blocks;
        private List<Vector2> blockDims;//x, y, width, height

        private TiledMap tMap;
        private TiledMapRenderer mapRenderer;

        private bool speaking;
        private World world;
        public OldPlayer player;
        //private NPC npc;

        private Camera2D camera;

        public Camera2D Camera
        {
            get
            {
                return camera;
            }
        }

        public OldMap(GraphicsDevice pDevice, ContentManager content, int pTileWidth, int pTileHeight, int pWidth, int pHeight)
        {
            //light = content.Load<Texture2D>("lightmask");
            effect = content.Load<Effect>("File");
            //effect.Parameters["lightMask"].SetValue(light);
            prevState = Keyboard.GetState();
            hud = new Hud(new string[] { "Undertale is bad,\nand so am I." }, content);
            speaking = false;
            blocks = new List<Body>();
            tileWidth = pTileWidth;
            tileHeight = pTileHeight;
            width = pWidth;
            height = pHeight;
            g = pDevice;
            cont = content;

            camera = new Camera2D(pDevice);
            debug = content.Load<Texture2D>("Textbox/Chars");

            tMap = content.Load<TiledMap>("corneria");
            mapRenderer = new TiledMapRenderer(pDevice);

            world = new World(new Vector2(0, 0));
			debugView = new DebugViewXNA(world);
			debugView.LoadContent(pDevice, content);
			debugView.AppendFlags(FarseerPhysics.DebugViewFlags.Shape);
			debugView.AppendFlags(FarseerPhysics.DebugViewFlags.PolygonPoints);
            
            //camera.Position = player.body.Position;
            Console.WriteLine("Scunt: " + tMap.ObjectLayers.Count);

            ParseCollisions();
            player = new OldPlayer(world, content, colArray, 16, 23);
            npcs = new OldNPC[] {
                new OldNPC(world, content, colArray, 2, false, 16, 14, new string[] { "Weebs are worse\nthan fortnite\ngamers.", "Where's the lie?" }),
                new OldNPC(world, content, colArray, 0, true, 18, 18, new string[] {"help"}, 4, 1)
            };
            //npc = new NPC(world, content, colArray, 2, false, 16, 14, new string[] {"Weebs are worse\nthan fortnite\ngamers.", "Where's the lie?" });

            for (int i = 0; i < colArray.Length; i++)
            {
                for (int j = 0; j < colArray[i].Length; j++)
                    Console.Write(colArray[i][j]);
                Console.WriteLine("");
            }

            CheckCollisions();


            //blockDims = new List<Vector2>();
            //MakeCollisionBodies();
        }
        public void HandleInput(GameTime gameTime)
        {
			if (Keyboard.GetState().IsKeyDown(Keys.A) && prevState.IsKeyUp(Keys.A))
				hud.finishMessage();
        }
        private void CheckCollisions()
        {
            int x = (int)player.body.Position.X / 16;
            int y = (int)player.body.Position.Y / 16;
        }
        private void ParseCollisions()
        {
            colArray = new int[tMap.Height][];
            for (int i = 0; i < colArray.Length; i++)
                colArray[i] = new int[tMap.Width];

            for (int i = 0; i < tMap.ObjectLayers[0].Objects.Length; i++)//Collision Layer
            {
                int x = (int)tMap.ObjectLayers[0].Objects[i].Position.X/16;
                int y = (int)tMap.ObjectLayers[0].Objects[i].Position.Y/16;
                int width = (int)tMap.ObjectLayers[0].Objects[i].Size.Width / 16;
                int height = (int)tMap.ObjectLayers[0].Objects[i].Size.Height / 16;
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        colArray[y + j][x + k] = 1;
                    }
                }
            }
        }
        public void DrawDebug(SpriteBatch sb)
        {
            for(int i = 0; i < blocks.Count; i++)
            {
                Body b = blocks[i];
                sb.Draw(debug, new Rectangle((int)b.Position.X - ((int)blockDims[i].X - 1) * 8, (int)b.Position.Y - ((int)blockDims[i].Y - 1) * 8, 16 * (int)blockDims[i].X, 16 * (int)blockDims[i].Y), new Rectangle(0, 0, 16, 16), Color.White);
            }
        }
        public void MakeCollisionBodies()
        {
            for (int i = 0; i < tMap.ObjectLayers[0].Objects.Length; i++)//Collision Layer
            {
                int x = (int)tMap.ObjectLayers[0].Objects[i].Position.X / 16;
                int y = (int)tMap.ObjectLayers[0].Objects[i].Position.Y / 16;
                int width = (int)tMap.ObjectLayers[0].Objects[i].Size.Width / 16;
                int height = (int)tMap.ObjectLayers[0].Objects[i].Size.Height / 16;
                //Body b = (BodyFactory.CreateRectangle(world, 16*3, 16*2, 0.2f, new Vector2(32 + 8*2, 32 + 8)));
                blocks.Add(BodyFactory.CreateRectangle(world, width * 16, height * 16, 0, new Vector2(x * 16 + (width - 1) * 8, y * 16 + (height - 1) * 8)));
                blocks[blocks.Count-1].BodyType = BodyType.Static;
                blockDims.Add(new Vector2(width, height));
                //sb.Draw(debug, new Rectangle((int)b.Position.X - (width-1)*8, (int)b.Position.Y - (height-1)*8, 16*width, 16*height), new Rectangle(0, 0, 16, 16), Color.White);
            }
        }

        void Screen.Draw(SpriteBatch pSb)
        {
            mapRenderer.Draw(tMap, camera.GetViewMatrix());
            pSb.Begin(transformMatrix: camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred);//SpriteSortMode.Immediate required for pixel shader

            //pSb.Begin();
            //pSb.Draw(debug, new Rectangle(0, 0, debug.Width, debug.Height), Color.White);
            //mapRenderer.Draw(tMap, camera.GetViewMatrix());
            player.Draw(pSb);
            foreach (OldNPC n in npcs)
                n.Draw(pSb);
            //g.SamplerStates[0] = SamplerState.PointClamp;

            Vector2 tilePos = Vector2.Zero;
            
            //DrawDebug(pSb);
            pSb.End();

            pSb.Begin(sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
            effect.CurrentTechnique.Passes[1].Apply();
            if (speaking)
                hud.Draw(pSb);
            pSb.End();
			Matrix proj = Matrix.CreateOrthographicOffCenter(0f, g.Viewport.Width, g.Viewport.Height, 0f, 0f, 1f);
			Matrix view = camera.GetViewMatrix();
			debugView.RenderDebugData(ref proj, ref view);
			//debugView.RenderDebugData(camera.GetViewMatrix(), camera.GetViewMatrix());
        }

        void Screen.Update(GameTime gameTime)
        {
            hud.Update(gameTime, prevState);
            HandleInput(gameTime);
            AdjustCamera();
            
            mapRenderer.Update(tMap, gameTime);
            

            if (!speaking)
            {
                world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
                player.Update(gameTime, false);
                foreach (OldNPC n in npcs)
                {
                    if (player.isStopped() && n.isStopped() && n.checkPlayer(player.GetState(), prevState))
                    {
                        speaking = true;
                        hud = new Hud(n.messages, cont, n.textWidth, n.textHeight);
                    }

                    n.Update(gameTime);
                }
            }
            else
            {
                player.Update(gameTime, true);
                if (hud.messageComplete())
                {
                    speaking = false;
                }
            }
            

            //camera.Position = new Vector2(player.body.Position.X - 400 / 2, player.body.Position.Y - 240 / 2 + 16);
            prevState = Keyboard.GetState();
            
        }

        private void AdjustCamera()
        {
            Vector2 tempPos = new Vector2(player.body.Position.X - 400 / 2, player.body.Position.Y - 240 / 2 + 16);

            if (TooFarUp())
                tempPos.Y = 0;
            else if (TooFarDown())
                tempPos.Y = tMap.HeightInPixels - 240;
            if (TooFarLeft())
                tempPos.X = 0;
            else if (TooFarRight())
                tempPos.X = tMap.WidthInPixels - 400;

            camera.Position = tempPos;
        }
        private bool TooFarLeft()
        {
            return (player.body.Position.X - 400 / 2 < 0);
        }

        private bool TooFarRight()
        {
            return (player.body.Position.X - 400 / 2 + 400 > tMap.WidthInPixels);
        }

        private bool TooFarUp()
        {
            return (player.body.Position.Y - 240 / 2 + 16 < 0);
        }

        private bool TooFarDown()
        {
            return (player.body.Position.Y - 240 / 2 + 16 + 240 > tMap.HeightInPixels);
        }
    }
}
