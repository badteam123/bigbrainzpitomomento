using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using static System.Reflection.Metadata.BlobBuilder;

namespace hell {

    public static class ResourceManager {
        public static GraphicsDevice GraphicsDevice { get; set; }
    }

    public class Game1: Game {

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;

        //private zpitoHandler zpitoHandler = new zpitoHandler();
        //List<Block> blocks = new List<Block>();
        //Dictionary<string, Texture2D> texDict = new Dictionary<string, Texture2D>();
        //Dictionary<string, BasicEffect> effDict = new Dictionary<string, BasicEffect>();
        //private Player player;

        private World world = new World();

        private Random rand = new Random();

        FastNoiseLite noise = new FastNoiseLite();
        private float noiseScale = 0.7f; //0.3f
        private float vertScale = 15f;
        private float threshold = 0.3f;

        private int renderDistance = 500; // 16 - 18 fps on 500 render dist before greedy meshing

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            AllocConsole();

            world.player.movementSpeed = 1000f;
            world.player.gravity = 15f;

            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            //target framerate
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 144.0);

            //consistent framerate
            IsFixedTimeStep = true;

            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.ApplyChanges();

            //player = new Player(new Vector3(0, 5, 0), 50f);

            //player.mouseLock = true;
            IsMouseVisible = true;



            string[] bruh = new string[11];
            bruh[0] = "stone";
            bruh[1] = "gray";

            noise.SetSeed(rand.Next());

            for (int x = -renderDistance; x < renderDistance + 1; x++) {
                for (int z = -renderDistance; z < renderDistance + 1; z++) {
                    world.addBlock(x, (int)Math.Round(noise.GetNoise(x * noiseScale, z * noiseScale) * vertScale) - 10, z, bruh[rand.Next(0, 2)]);
                }
            }

            /*
            for (int x = -renderDistance; x < renderDistance + 1; x++) {
                for (int y = -renderDistance; y < renderDistance + 1; y++) {
                    for (int z = -renderDistance; z < renderDistance + 1; z++) {
                        if (noise.GetNoise(x * noiseScale, y * noiseScale, z * noiseScale) > threshold) {
                            if (
                                noise.GetNoise((x + 1) * noiseScale, y * noiseScale, z * noiseScale) < threshold ||
                                noise.GetNoise((x - 1) * noiseScale, y * noiseScale, z * noiseScale) < threshold ||
                                noise.GetNoise(x * noiseScale, (y + 1) * noiseScale, z * noiseScale) < threshold ||
                                noise.GetNoise(x * noiseScale, (y - 1) * noiseScale, z * noiseScale) < threshold ||
                                noise.GetNoise(x * noiseScale, y * noiseScale, (z + 1) * noiseScale) < threshold ||
                                noise.GetNoise(x * noiseScale, y * noiseScale, (z - 1) * noiseScale) < threshold
                                ) {
                                world.addBlock(x, y-150, z, bruh[rand.Next(0,2)]);
                            }
                        }
                    }
                }
            }
            */

            Console.WriteLine(world.blocks["stone"].Count * 2);

        }

        protected override void Initialize() {
            base.Initialize();
            ResourceManager.GraphicsDevice = GraphicsDevice;
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("ArialFont");

            foreach (string key in world.blocks.Keys) {
            if (!world.textures.ContainsKey(key)) {
                world.textures[key] = Content.Load<Texture2D>(key);
                world.textures[key].GraphicsDevice.SamplerStates[0] = new SamplerState { Filter = TextureFilter.Point, AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap };
            }
                world.effects[key] = new BasicEffect(GraphicsDevice) {
                    TextureEnabled = true,
                    Texture = world.textures[key],
                    Projection = Matrix.CreatePerspectiveFieldOfView(world.player.fieldOfView, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f)
                };
            };

            world.regenerate();

        }

        protected override void Update(GameTime gameTime) {

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            IsMouseVisible = !world.player.mouseLock;

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            world.updatePlayer(deltaTime);

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                world.player.yVel += 0.50f;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            world.render();

            spriteBatch.Begin();
            spriteBatch.DrawString(font, (1/(float)gameTime.ElapsedGameTime.TotalSeconds).ToString(), new Microsoft.Xna.Framework.Vector2(10, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}