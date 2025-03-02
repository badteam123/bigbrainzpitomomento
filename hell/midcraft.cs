﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace hell {
    internal class World {

        public Dictionary<string, Texture2D> textures { get; set; }
        public Dictionary<string, BasicEffect> effects { get; set; }
        public Dictionary<string, VertexPositionTexture[]> vertices { get; set; }
        public Dictionary<string, int[]> indices { get; set; }

        public Dictionary<string, List<Block>> blocks { get; set; }
        public Dictionary<string, List<VisualBlock>> visualBlocks { get; set; }

        public Dictionary<string, Dictionary<int, Dictionary<int, List<int>>>> chunk = new Dictionary<string, Dictionary<int, Dictionary<int, List<int>>>>();

        private float chunkSize = 4.0f;

        private float bruh = 0;

        public Player player { get; set; }


        public World() {
            textures = new Dictionary<string, Texture2D>();
            effects = new Dictionary<string, BasicEffect>();
            vertices = new Dictionary<string, VertexPositionTexture[]>();
            indices = new Dictionary<string, int[]>();
            blocks = new Dictionary<string, List<Block>>();
            visualBlocks = new Dictionary<string, List<VisualBlock>>();
            player = new Player(new Vector3(0, 0, 0), 1000f);
        }

        private void floorCheck(string key, double x, double z) {
            if (chunk[key].TryGetValue((int)x, out var dict1) &&
            dict1.TryGetValue((int)z, out var list)) {
                for (int b = 0; b < list.Count; b++) {
                    blocks[key][list[b]].collideFloor(player);
                }
            }
        }

        private void wallCheck(string key, double x, double z) {
            if (chunk[key].TryGetValue((int)x, out var dict1) &&
            dict1.TryGetValue((int)z, out var list)) {
                for (int b = 0; b < list.Count; b++) {
                    blocks[key][list[b]].collide(player);
                }
            }
        }

        private void stepCheck(string key, double x, double z) {
            if (chunk[key].TryGetValue((int)x, out var dict1) &&
            dict1.TryGetValue((int)z, out var list)) {
                for (int b = 0; b < list.Count; b++) {
                    blocks[key][list[b]].step(player);
                }
            }
        }

        /// <summary>
        /// Takes in Player, Block, and Deltatime data
        /// and updates player position and rotation
        /// </summary>
        /// <param name="player">Player object</param>
        /// <param name="blocks">List of blocks</param>
        /// <param name="deltaTime">Deltatime float</param>
        public void updatePlayer(float deltaTime) {

            Microsoft.Xna.Framework.Point screenCenter = new Microsoft.Xna.Framework.Point(ResourceManager.GraphicsDevice.Viewport.Width / 2, ResourceManager.GraphicsDevice.Viewport.Height / 2);

            player.bruh(deltaTime);

            if (player.canMove) {
                if (player.mouseLock) {
                    // Get the current mouse state
                    var mouseState = Mouse.GetState();

                    // Calculate the difference between the current mouse position and the center of the screen
                    int deltaX = mouseState.X - screenCenter.X;
                    int deltaY = mouseState.Y - screenCenter.Y;

                    // Update yaw and pitch based on mouse movement
                    player.r -= deltaX * player.mouseSensitivity;
                    player.t -= deltaY * player.mouseSensitivity;

                    // Clamp the pitch to prevent flipping
                    player.t = MathHelper.Clamp(player.t, -MathHelper.PiOver2, MathHelper.PiOver2);
                }

                // Get keyboard state
                var kstate = Keyboard.GetState();

                // Move player
                if (kstate.IsKeyDown(Keys.W)) {
                    player.xVel += (float)Math.Sin(player.r) * -player.movementSpeed * deltaTime;
                    player.zVel += (float)Math.Cos(player.r) * -player.movementSpeed * deltaTime;
                }
                if (kstate.IsKeyDown(Keys.A)) {
                    player.xVel += (float)Math.Sin(player.r + MathHelper.PiOver2) * -player.movementSpeed * deltaTime;
                    player.zVel += (float)Math.Cos(player.r + MathHelper.PiOver2) * -player.movementSpeed * deltaTime;
                }
                if (kstate.IsKeyDown(Keys.S)) {
                    player.xVel += (float)Math.Sin(player.r + MathHelper.Pi) * -player.movementSpeed * deltaTime;
                    player.zVel += (float)Math.Cos(player.r + MathHelper.Pi) * -player.movementSpeed * deltaTime;
                }
                if (kstate.IsKeyDown(Keys.D)) {
                    player.xVel += (float)Math.Sin(player.r - MathHelper.PiOver2) * -player.movementSpeed * deltaTime;
                    player.zVel += (float)Math.Cos(player.r - MathHelper.PiOver2) * -player.movementSpeed * deltaTime;
                }
                if (kstate.IsKeyDown(Keys.Space) && player.onGround) {
                    player.yVel = player.jumpHeight;
                }
            }

            player.yVel -= player.gravity * deltaTime;

            player.onGround = false;

            foreach (string key in blocks.Keys) {
                floorCheck(key, gc(player.x)-1, gc(player.z)-1);
                floorCheck(key, gc(player.x)-1, gc(player.z));
                floorCheck(key, gc(player.x)-1, gc(player.z)+1);
                floorCheck(key, gc(player.x), gc(player.z)-1);
                floorCheck(key, gc(player.x), gc(player.z));
                floorCheck(key, gc(player.x), gc(player.z)+1);
                floorCheck(key, gc(player.x)+1, gc(player.z)-1);
                floorCheck(key, gc(player.x)+1, gc(player.z));
                floorCheck(key, gc(player.x)+1, gc(player.z)+1);
            }   
            player.prevVelocity = player.velocity;
            player.nextVelocity = player.velocity;
            player.debugCanStepX = true;
            player.debugCanStepZ = true;
            foreach (string key in blocks.Keys) {
                wallCheck(key, gc(player.x) - 1, gc(player.z) - 1);
                wallCheck(key, gc(player.x) - 1, gc(player.z));
                wallCheck(key, gc(player.x) - 1, gc(player.z) + 1);
                wallCheck(key, gc(player.x), gc(player.z) - 1);
                wallCheck(key, gc(player.x), gc(player.z));
                wallCheck(key, gc(player.x), gc(player.z) + 1);
                wallCheck(key, gc(player.x) + 1, gc(player.z) - 1);
                wallCheck(key, gc(player.x) + 1, gc(player.z));
                wallCheck(key, gc(player.x) + 1, gc(player.z) + 1);
            }
            foreach (string key in blocks.Keys) {
                stepCheck(key, gc(player.x) - 1, gc(player.z) - 1);
                stepCheck(key, gc(player.x) - 1, gc(player.z));
                stepCheck(key, gc(player.x) - 1, gc(player.z) + 1);
                stepCheck(key, gc(player.x), gc(player.z) - 1);
                stepCheck(key, gc(player.x), gc(player.z));
                stepCheck(key, gc(player.x), gc(player.z) + 1);
                stepCheck(key, gc(player.x) + 1, gc(player.z) - 1);
                stepCheck(key, gc(player.x) + 1, gc(player.z));
                stepCheck(key, gc(player.x) + 1, gc(player.z) + 1);
            }

            player.velocity = player.nextVelocity;

            player.position += player.velocity * deltaTime;

            player.cameraPosition = player.position + new Microsoft.Xna.Framework.Vector3(0, player.halfHeight / 2f, 0);

            player.xVel = MathHelper.LerpPrecise(player.xVel, 0, Math.Min(player.damping * deltaTime, 1));
            player.zVel = MathHelper.LerpPrecise(player.zVel, 0, Math.Min(player.damping * deltaTime, 1));

            player.prevVelocity = player.velocity;

            if (player.mouseLock) {
                Mouse.SetPosition(screenCenter.X, screenCenter.Y);
            }

        }

        private void addChunk(ref List<int> checks, string key, int x, int z) {
            if (chunk[key].ContainsKey(x)) {
                if (chunk[key][x].ContainsKey(z)) {
                    checks.AddRange(chunk[key][x][z]);
                }
            }
        }

        private int gc(float pos) {
            return (int)Math.Round(pos / chunkSize);
        }
        private int gc(float pos, int off) {
            return (int)Math.Round(pos / chunkSize) + off;
        }

        /*
         * check block make sure availible
         * get list of blocks in the chunk in next position
         * then go thru that list and see if beside
         * if there was one beside then loop again through same logic
         */

        private bool checkXPos(string key, int b) {
            if (!visualBlocks[key][b].toDelete) {

                List<int> list = new List<int>();

                addChunk(ref list, key, gc(visualBlocks[key][b].hx + 1), gc(visualBlocks[key][b].z));

                for(int c = 0; c < list.Count; c++) {
                    if (Math.Round(visualBlocks[key][list[c]].x) == Math.Round(visualBlocks[key][b].hx + 1) && !visualBlocks[key][list[c]].toDelete) {
                        if (visualBlocks[key][list[c]].y == visualBlocks[key][b].y) {
                            if (visualBlocks[key][list[c]].z == visualBlocks[key][b].z) {
                                if (visualBlocks[key][list[c]].dx == 1 && visualBlocks[key][list[c]].dy == 1 && visualBlocks[key][list[c]].dz == 1) {
                                    visualBlocks[key][b].px = 1;
                                    visualBlocks[key][list[c]].toDelete = true;
                                    return true;
                                }
                            }
                        }
                    }
                }
                
            }
            return false;
        }

        private bool checkXNeg(string key, int b) {
            if (!visualBlocks[key][b].toDelete) {

                List<int> list = new List<int>();

                addChunk(ref list, key, gc(visualBlocks[key][b].lx - 1), gc(visualBlocks[key][b].z));

                for (int c = 0; c < list.Count; c++) {
                    if (Math.Round(visualBlocks[key][list[c]].x) == Math.Round(visualBlocks[key][b].lx - 1) && !visualBlocks[key][list[c]].toDelete) {
                        if (visualBlocks[key][list[c]].y == visualBlocks[key][b].y) {
                            if (visualBlocks[key][list[c]].z == visualBlocks[key][b].z) {
                                if (visualBlocks[key][list[c]].dx == 1 && visualBlocks[key][list[c]].dy == 1 && visualBlocks[key][list[c]].dz == 1) {
                                    visualBlocks[key][b].nx = 1;
                                    visualBlocks[key][list[c]].toDelete = true;
                                    return true;
                                }
                            }
                        }
                    }
                }

            }
            return false;
        }

        private bool checkZPos(string key, int b) {
            if (!visualBlocks[key][b].toDelete) {
                List<int> delete = new List<int>();
                for(float x = visualBlocks[key][b].lx; x <= visualBlocks[key][b].hx; x++) {
                    for (float y = visualBlocks[key][b].ly; y <= visualBlocks[key][b].hy; y++) {

                        bool found = false;

                        List<int> list = new List<int>();

                        addChunk(ref list, key, gc(x), gc(visualBlocks[key][b].hz + 1));

                        for (int c = 0; c < list.Count; c++) {
                            if (Math.Round(visualBlocks[key][list[c]].z) == Math.Round(visualBlocks[key][b].hz + 1) && !visualBlocks[key][list[c]].toDelete) {
                                if (Math.Round(visualBlocks[key][list[c]].y) == Math.Round(y)) {
                                    if (Math.Round(visualBlocks[key][list[c]].x) == Math.Round(x)) {
                                        if (visualBlocks[key][list[c]].dx == 1 && visualBlocks[key][list[c]].dy == 1 && visualBlocks[key][list[c]].dz == 1) {
                                            found = true;
                                            delete.Add(list[c]);
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (!found) {
                            return false;
                        }

                    }
                }
                visualBlocks[key][b].pz = 1;
                for(int d = 0; d < delete.Count; d++) {
                    visualBlocks[key][delete[d]].toDelete = true;
                }
                return true;
            }
            return false;
        }

        private bool checkZNeg(string key, int b) {
            if (!visualBlocks[key][b].toDelete) {
                List<int> delete = new List<int>();
                for (float x = visualBlocks[key][b].lx; x <= visualBlocks[key][b].hx; x++) {
                    for (float y = visualBlocks[key][b].ly; y <= visualBlocks[key][b].hy; y++) {

                        bool found = false;

                        List<int> list = new List<int>();

                        addChunk(ref list, key, gc(x), gc(visualBlocks[key][b].lz - 1));

                        for (int c = 0; c < list.Count; c++) {
                            if (visualBlocks[key][list[c]].z == visualBlocks[key][b].lz - 1 && !visualBlocks[key][list[c]].toDelete) {
                                if (visualBlocks[key][list[c]].y == y) {
                                    if (visualBlocks[key][list[c]].x == x) {
                                        if (visualBlocks[key][list[c]].dx == 1 && visualBlocks[key][list[c]].dy == 1 && visualBlocks[key][list[c]].dz == 1) {
                                            found = true;
                                            delete.Add(list[c]);
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (!found) {
                            delete.Clear();
                            return false;
                        }

                    }
                }
                visualBlocks[key][b].nz = 1;
                for (int d = 0; d < delete.Count; d++) {
                    visualBlocks[key][delete[d]].toDelete = true;
                }
                return true;
            }
            return false;
        }

        private bool checkYPos(string key, int b) {
            if (!visualBlocks[key][b].toDelete) {
                List<int> delete = new List<int>();
                for (float x = visualBlocks[key][b].lx; x <= visualBlocks[key][b].hx; x++) {
                    for (float z = visualBlocks[key][b].lz; z <= visualBlocks[key][b].hz; z++) {

                        bool found = false;

                        List<int> list = new List<int>();

                        addChunk(ref list, key, gc(x), gc(z));

                        for (int c = 0; c < list.Count; c++) {
                            if (Math.Round(visualBlocks[key][list[c]].y) == Math.Round(visualBlocks[key][b].hy + 1) && !visualBlocks[key][list[c]].toDelete) {
                                if (Math.Round(visualBlocks[key][list[c]].z) == Math.Round(z)) {
                                    if (Math.Round(visualBlocks[key][list[c]].x) == Math.Round(x)) {
                                        if (visualBlocks[key][list[c]].dx == 1 && visualBlocks[key][list[c]].dy == 1 && visualBlocks[key][list[c]].dz == 1) {
                                            found = true;
                                            delete.Add(list[c]);
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (!found) {
                            return false;
                        }

                    }
                }
                visualBlocks[key][b].py = 1;
                for (int d = 0; d < delete.Count; d++) {
                    visualBlocks[key][delete[d]].toDelete = true;
                }
                return true;
            }
            return false;
        }

        private bool checkYNeg(string key, int b) {
            if (!visualBlocks[key][b].toDelete) {
                List<int> delete = new List<int>();
                for (float x = visualBlocks[key][b].lx; x <= visualBlocks[key][b].hx; x++) {
                    for (float z = visualBlocks[key][b].lz; z <= visualBlocks[key][b].hz; z++) {

                        bool found = false;

                        List<int> list = new List<int>();

                        addChunk(ref list, key, gc(x), gc(z));

                        for (int c = 0; c < list.Count; c++) {
                            if (Math.Round(visualBlocks[key][list[c]].y) == Math.Round(visualBlocks[key][b].ly - 1) && !visualBlocks[key][list[c]].toDelete) {
                                if (Math.Round(visualBlocks[key][list[c]].z) == Math.Round(z)) {
                                    if (Math.Round(visualBlocks[key][list[c]].x) == Math.Round(x)) {
                                        if (visualBlocks[key][list[c]].dx == 1 && visualBlocks[key][list[c]].dy == 1 && visualBlocks[key][list[c]].dz == 1) {
                                            found = true;
                                            delete.Add(list[c]);
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (!found) {
                            return false;
                        }

                    }
                }
                visualBlocks[key][b].ny = 1;
                for (int d = 0; d < delete.Count; d++) {
                    visualBlocks[key][delete[d]].toDelete = true;
                }
                return true;
            }
            return false;
        }

        public void regenerate() {

            int progress = 0;

            foreach (string key in blocks.Keys) {

                visualBlocks[key] = new List<VisualBlock>();
                for (int i = 0; i < blocks[key].Count; i++) {
                    visualBlocks[key].Add(new VisualBlock(blocks[key][i].x, blocks[key][i].y, blocks[key][i].z));
                }

                Console.Clear();
                Console.WriteLine("Loading...");
                Console.CursorLeft = 0;
                Console.CursorTop = 2;
                Console.Write("Combining " + key + " blocks");

                for (int b = 0; b < visualBlocks[key].Count; b++) {

                    if(b % 10000 == 0) {
                        Console.CursorLeft = 0;
                        Console.CursorTop = 1;
                        Console.Write((((b / (visualBlocks[key].Count + 0.0f))*(50 / blocks.Keys.Count)) + (100/blocks.Keys.Count)*progress).ToString("0.00") + "%");
                    }

                    bool doAgain = false;

                    //x+
                    do {
                        doAgain = false;
                        doAgain = checkXPos(key, b);
                    } while (doAgain);

                    //x-
                    do {
                        doAgain = false;
                        doAgain = checkXNeg(key, b);
                    } while (doAgain);

                    //z+
                    do {
                        doAgain = false;
                        doAgain = checkZPos(key, b);
                    } while (doAgain);

                    //z-
                    do {
                        doAgain = false;
                        doAgain = checkZNeg(key, b);
                    } while (doAgain);

                    //y+
                    do {
                        doAgain = false;
                        doAgain = checkYPos(key, b);
                    } while (doAgain);

                    //y-
                    do {
                        doAgain = false;
                        doAgain = checkYNeg(key, b);
                    } while (doAgain);

                }

                Console.Clear();
                Console.WriteLine("Loading...");
                Console.CursorLeft = 0;
                Console.CursorTop = 2;
                Console.Write("Clearing redundant " + key + " blocks");

                int num = 0;
                for (int b = visualBlocks[key].Count - 1; b >= 0; b--) {
                    if (b % 10000 == 0) {
                        Console.CursorLeft = 0;
                        Console.CursorTop = 1;
                        float bruh = (visualBlocks[key].Count - b) / (visualBlocks[key].Count / (50.0f / blocks.Keys.Count));
                        Console.Write((bruh + (50/blocks.Keys.Count) + ((100/blocks.Keys.Count)*progress)).ToString("0.00") + "%");
                    }
                    if (visualBlocks[key][b].toDelete) {
                        visualBlocks[key].RemoveAt(b);
                        num++;
                    }
                }
                progress++;
            }

            Console.Clear();
            Console.WriteLine("Done!");

            //################################################################################################

            vertices = new Dictionary<string, VertexPositionTexture[]>();
            indices = new Dictionary<string, int[]>();

            foreach (string key in textures.Keys) {
                List<Vector3> positions = new List<Vector3>();
                List<Vector2> UVs = new List<Vector2>();
                List<int> listIndices = new List<int>();

                for (int b = 0; b < visualBlocks[key].Count; b++) {

                    float dx2 = visualBlocks[key][b].dx / 2f;
                    float dy2 = visualBlocks[key][b].dy / 2f;
                    float dz2 = visualBlocks[key][b].dz / 2f;
                    float dx = visualBlocks[key][b].dx;
                    float dy = visualBlocks[key][b].dy;
                    float dz = visualBlocks[key][b].dz;

                    // Back (z-)
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z - dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z - dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z - dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z - dz2));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(0, dy));
                    UVs.Add(new Vector2(dx, dy));
                    UVs.Add(new Vector2(dx, 0));

                    // Front (z+)
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z + dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z + dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z + dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z + dz2));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(0, dy));
                    UVs.Add(new Vector2(dx, dy));
                    UVs.Add(new Vector2(dx, 0));

                    // Left (x-)
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z - dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z + dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z + dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z - dz2));
                    UVs.Add(new Vector2(dz, dy));
                    UVs.Add(new Vector2(0, dy));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(dz, 0));

                    // Right (x+)
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z + dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z - dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z - dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z + dz2));
                    UVs.Add(new Vector2(dz, dy));
                    UVs.Add(new Vector2(0, dy));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(dz, 0));

                    // Bottom (y-)
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z - dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z + dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z + dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y - dy2, visualBlocks[key][b].z - dz2));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(0, dz));
                    UVs.Add(new Vector2(dx, dz));
                    UVs.Add(new Vector2(dx, 0));

                    // Top (y+)
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z + dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x - dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z - dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z - dz2));
                    positions.Add(new Vector3(visualBlocks[key][b].x + dx2, visualBlocks[key][b].y + dy2, visualBlocks[key][b].z + dz2));
                    UVs.Add(new Vector2(0, dz));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(dx, 0));
                    UVs.Add(new Vector2(dx, dz));

                    // Cube indices
                    int[] addedIndices;

                    addedIndices = new int[]{
                        0 + (b*24), 2 + (b*24), 1 + (b*24), 0 + (b*24), 3 + (b*24), 2 + (b*24),       // Front  (z+)
                        4 + (b*24), 5 + (b*24), 6 + (b*24), 4 + (b*24), 6 + (b*24), 7 + (b*24),       // Back   (z-)
                        10 + (b*24), 11 + (b*24), 8 + (b*24), 10 + (b*24), 8 + (b*24), 9 + (b*24),    // Left   (x-)
                        12 + (b*24), 13 + (b*24), 14 + (b*24), 12 + (b*24), 14 + (b*24), 15 + (b*24), // Right  (x+)
                        16 + (b*24), 18 + (b*24), 19 + (b*24), 16 + (b*24), 17 + (b*24), 18 + (b*24), // Top    (y+)
                        21 + (b*24), 22 + (b*24), 23 + (b*24), 21 + (b*24), 23 + (b*24), 20 + (b*24)  // Bottom (y-)
                    };
                    listIndices.AddRange(addedIndices);

                }

                vertices[key] = new VertexPositionTexture[positions.Count];
                indices[key] = new int[listIndices.Count];

                for (int i = 0; i < positions.Count; i++) {
                    vertices[key][i] = new VertexPositionTexture(new Vector3(positions[i].X, positions[i].Y, positions[i].Z), new Vector2(UVs[i].X, UVs[i].Y));
                }

                for (int i = 0; i < listIndices.Count; i++) {
                    indices[key][i] = listIndices[i];
                }

            }

            /*
            foreach (string key in textures.Keys) {
                List<Vector3> positions = new List<Vector3>();
                List<Vector2> UVs = new List<Vector2>();
                List<int> listIndices = new List<int>();

                for (int b = 0; b < blocks[key].Count; b++) {

                    // Back (z-)
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z - 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z - 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z - 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z - 0.5f));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(0, 1));
                    UVs.Add(new Vector2(1, 1));
                    UVs.Add(new Vector2(1, 0));

                    // Front (z+)
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z + 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z + 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z + 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z + 0.5f));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(0, 1));
                    UVs.Add(new Vector2(1, 1));
                    UVs.Add(new Vector2(1, 0));

                    // Left (x-)
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z + 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z + 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z - 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z - 0.5f));
                    UVs.Add(new Vector2(1, 1));
                    UVs.Add(new Vector2(0, 1));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(1, 0));

                    // Right (x+)
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z - 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z - 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z + 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z + 0.5f));
                    UVs.Add(new Vector2(1, 1));
                    UVs.Add(new Vector2(0, 1));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(1, 0));

                    // Bottom (y-)
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z - 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z + 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z + 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y - 0.5f, blocks[key][b].z - 0.5f));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(0, 1));
                    UVs.Add(new Vector2(1, 1));
                    UVs.Add(new Vector2(1, 0));

                    // Top (y+)
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z + 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x - 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z - 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z - 0.5f));
                    positions.Add(new Vector3(blocks[key][b].x + 0.5f, blocks[key][b].y + 0.5f, blocks[key][b].z + 0.5f));
                    UVs.Add(new Vector2(0, 1));
                    UVs.Add(new Vector2(0, 0));
                    UVs.Add(new Vector2(1, 0));
                    UVs.Add(new Vector2(1, 1));

                    // Cube indices
                    int[] addedIndices;

                    addedIndices = new int[]{
                        0 + (b*24), 2 + (b*24), 1 + (b*24), 0 + (b*24), 3 + (b*24), 2 + (b*24),       // Front  (z+)
                        4 + (b*24), 5 + (b*24), 6 + (b*24), 4 + (b*24), 6 + (b*24), 7 + (b*24),       // Back   (z-)
                        10 + (b*24), 11 + (b*24), 8 + (b*24), 10 + (b*24), 8 + (b*24), 9 + (b*24),    // Left   (x-)
                        12 + (b*24), 13 + (b*24), 14 + (b*24), 12 + (b*24), 14 + (b*24), 15 + (b*24), // Right  (x+)
                        16 + (b*24), 18 + (b*24), 19 + (b*24), 16 + (b*24), 17 + (b*24), 18 + (b*24), // Top    (y+)
                        21 + (b*24), 22 + (b*24), 23 + (b*24), 21 + (b*24), 23 + (b*24), 20 + (b*24)  // Bottom (y-)
                    };
                    listIndices.AddRange(addedIndices);

                }
                
                vertices[key] = new VertexPositionTexture[positions.Count];
                indices[key] = new int[listIndices.Count];

                for (int i = 0; i < positions.Count; i++) {
                    vertices[key][i] = new VertexPositionTexture(new Vector3(positions[i].X, positions[i].Y, positions[i].Z), new Vector2(UVs[i].X, UVs[i].Y));
                }

                for (int i = 0; i < listIndices.Count; i++) {
                    indices[key][i] = listIndices[i];
                }

            }
            */

        }

        public void render() {

            ResourceManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ResourceManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ResourceManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            Microsoft.Xna.Framework.Point screenCenter = new Microsoft.Xna.Framework.Point(ResourceManager.GraphicsDevice.Viewport.Width / 2, ResourceManager.GraphicsDevice.Viewport.Height / 2);

            var mouseState = Mouse.GetState();

            int deltaX = mouseState.X - screenCenter.X;
            int deltaY = mouseState.Y - screenCenter.Y;
            bruh += 1f;

            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(player.r, player.t, 0);
            Vector3 lookDirection = Vector3.Transform(Vector3.Forward, rotationMatrix);
            Vector3 upDirection = Vector3.Transform(Vector3.Up, rotationMatrix);
            Matrix viewMatrix = Matrix.CreateLookAt(player.cameraPosition, player.cameraPosition + lookDirection, upDirection);

            foreach (string key in textures.Keys) {
                effects[key].View = viewMatrix;
                ResourceManager.GraphicsDevice.SamplerStates[0] = new SamplerState { Filter = TextureFilter.Point, AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap };
                foreach (var pass in effects[key].CurrentTechnique.Passes) {
                    pass.Apply();
                    ResourceManager.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices[key], 0, vertices[key].Length, indices[key], 0, 12 * (visualBlocks[key].Count));
                }
            }
            
            ResourceManager.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            ResourceManager.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            ResourceManager.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        public void addBlock(int x, int y, int z, string tex) {
            if (!blocks.ContainsKey(tex)) {
                blocks[tex] = new List<Block>();
            }
            blocks[tex].Add(new Block(x, y, z, tex));

            if (!chunk.ContainsKey(tex)) {
                chunk[tex] = new Dictionary<int, Dictionary<int, List<int>>>();
            }

            if (!chunk[tex].ContainsKey((int)Math.Round(x / chunkSize))) {
                chunk[tex][(int)Math.Round(x / chunkSize)] = new Dictionary<int, List<int>>();
            }

            if (!chunk[tex][(int)Math.Round(x / chunkSize)].ContainsKey((int)Math.Round(z / chunkSize))) {
                chunk[tex][(int)Math.Round(x / chunkSize)][(int)Math.Round(z / chunkSize)] = new List<int>();
            }

            chunk[tex][(int)Math.Round(x / chunkSize)][(int)Math.Round(z / chunkSize)].Add(blocks[tex].Count - 1);
        }
    }

    internal class VisualBlock {

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float dx { get; set; }
        public float dy { get; set; }
        public float dz { get; set; }
        public bool toDelete { get; set; }

        public VisualBlock(int x, int y, int z) {
            this.x = x + 0.0f;
            this.y = y + 0.0f;
            this.z = z + 0.0f;
            this.dx = 1;
            this.dy = 1;
            this.dz = 1;
            this.toDelete = false;
        }

        public int px {
            set { x += value / 2.0f; dx += value; }
        }
        public int nx {
            set { x -= value / 2.0f; dx += value; }
        }
        public int py {
            set { y += value / 2.0f; dy += value; }
        }
        public int ny {
            set { y -= value / 2.0f; dy += value; }
        }
        public int pz {
            set { z += value / 2.0f; dz += value; }
        }
        public int nz {
            set { z -= value / 2.0f; dz += value; }
        }

        public float lx {
            get { return this.x - (this.dx / 2.0f) + 0.5f; }
        }
        public float ly {
            get { return this.y - (this.dy / 2.0f) + 0.5f; }
        }
        public float lz {
            get { return this.z - (this.dz / 2.0f) + 0.5f; }
        }
        public float hx {
            get { return this.x + (this.dx / 2.0f) - 0.5f; }
        }
        public float hy {
            get { return this.y + (this.dy / 2.0f) - 0.5f; }
        }
        public float hz {
            get { return this.z + (this.dz / 2.0f) - 0.5f; }
        }
    }

    internal class Block {

        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }

        public Block(int x, int y, int z, string tex) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float xMin {
            get { return this.x - 0.5f; }
        }
        public float xMax {
            get { return this.x + 0.5f; }
        }
        public float yMin {
            get { return this.y - 0.5f; }
        }
        public float yMax {
            get { return this.y + 0.5f; }
        }
        public float zMin {
            get { return this.z - 0.5f; }
        }
        public float zMax {
            get { return this.z + 0.5f; }
        }

        public void collideFloor(Player player) {
            bool insideNext = false;
            if (player.xMaxNext > this.xMin && player.xMinNext < this.xMax && player.zMaxNext > this.zMin && player.zMinNext < this.zMax) {
                insideNext = true;
            }
            if (insideNext && player.yMin + 0.0001f >= this.yMax && player.yMinNext < this.yMax) {
                player.y = this.yMax + player.halfHeight;
                player.yVel = 0;
                player.onGround = true;
            }
            if (insideNext && player.yMax < this.yMin && player.yMaxNext > this.yMin) {
                player.y = this.yMin - player.halfHeight;
                player.yVel = 0;
            }
        }
        public void step(Player player) {
            bool insideYNext = false;
            bool insideX = false;
            bool insideXNext = false;
            bool insideZ = false;
            bool insideZNext = false;

            if (player.yMaxNextPrev > this.yMin && player.yMinNextPrev < this.yMax) {
                insideYNext = true;
            }
            if (player.zMax > this.zMin && player.zMin < this.zMax) {
                insideZ = true;
            }
            if (player.zMaxNextPrev > this.zMin && player.zMinNextPrev < this.zMax) {
                insideZNext = true;
            }
            if (player.xMax > this.xMin && player.xMin < this.xMax) {
                insideX = true;
            }
            if (player.xMaxNextPrev > this.xMin && player.xMinNextPrev < this.xMax) {
                insideXNext = true;
            }

            if (insideXNext && insideYNext && insideZNext && player.yMin > this.yMax - player.stepHeight && player.onGround) {
                if (insideX && player.debugCanStepZ) {
                    player.nextVelocity = player.prevVelocity;
                    player.y = this.yMax + player.halfHeight;
                    player.nextyVel = 0;
                    player.onGround = true;
                }
                if (insideZ && player.debugCanStepX) {
                    player.nextVelocity = player.prevVelocity;
                    player.y = this.yMin + player.halfHeight;
                    player.nextyVel = 0;
                    player.onGround = true;
                }
            }

        }
        public void collide(Player player) {
            bool insideYNext = false;
            bool insideX = false;
            bool insideXNext = false;
            bool insideZ = false;
            bool insideZNext = false;

            if (player.yMaxNextPrev > this.yMin && player.yMinNextPrev < this.yMax) {
                insideYNext = true;
            }
            if (player.zMax > this.zMin && player.zMin < this.zMax) {
                insideZ = true;
            }
            if (player.zMaxNextPrev > this.zMin && player.zMinNextPrev < this.zMax) {
                insideZNext = true;
            }
            if (player.xMax > this.xMin && player.xMin < this.xMax) {
                insideX = true;
            }
            if (player.xMaxNextPrev > this.xMin && player.xMinNextPrev < this.xMax) {
                insideXNext = true;
            }
            if (insideYNext) {
                if (insideZNext && insideZ && player.xMin >= this.xMax && player.xMinNext < this.xMax) {
                    player.x = this.xMax + player.halfWidth;
                    player.nextxVel = 0;
                    if (player.yMin <= this.yMax - player.stepHeight) {
                        player.debugCanStepX = false;
                    }
                }
                if (insideZNext && insideZ && player.xMax <= this.xMin && player.xMaxNext > this.xMin) {
                    player.x = this.xMin - player.halfWidth;
                    player.nextxVel = 0;
                    if (player.yMin <= this.yMax - player.stepHeight) {
                        player.debugCanStepX = false;
                    }
                }
                if (insideXNext && insideX && player.zMin >= this.zMax && player.zMinNext < this.zMax) {
                    player.z = this.zMax + player.halfWidth;
                    player.nextzVel = 0;
                    if (player.yMin <= this.yMax - player.stepHeight) {
                        player.debugCanStepZ = false;
                    }
                }
                if (insideXNext && insideX && player.zMax <= this.zMin && player.zMaxNext > this.zMin) {
                    player.z = this.zMin - player.halfWidth;
                    player.nextzVel = 0;
                    if (player.yMin <= this.yMax - player.stepHeight) {
                        player.debugCanStepZ = false;
                    }
                }
            }
            // corner bug fix oh god my brain
            if (insideXNext && insideYNext && insideZNext) {
                if (!insideX && !insideZ) {
                    if (Math.Abs(player.prevxVel) < Math.Abs(player.prevzVel)) {
                        if (player.x > this.x) {
                            player.x = this.xMax + player.halfWidth;
                            player.nextxVel = 0;
                            if (player.yMin <= this.yMax - player.stepHeight) {
                                player.debugCanStepX = false;
                            }
                        } else {
                            player.x = this.xMin - player.halfWidth;
                            player.nextxVel = 0;
                            if (player.yMin <= this.yMax - player.stepHeight) {
                                player.debugCanStepX = false;
                            }
                        }
                    } else {
                        if (player.z > this.z) {
                            player.z = this.zMax + player.halfWidth;
                            player.nextzVel = 0;
                            if (player.yMin <= this.yMax - player.stepHeight) {
                                player.debugCanStepZ = false;
                            }
                        } else {
                            player.z = this.zMin - player.halfWidth;
                            player.nextzVel = 0;
                            if (player.yMin <= this.yMax - player.stepHeight) {
                                player.debugCanStepZ = false;
                            }
                        }
                    }
                }
            }

        }

    }

    internal class Player {

        private Microsoft.Xna.Framework.Vector3 pos;
        private Microsoft.Xna.Framework.Vector3 vel;
        private Microsoft.Xna.Framework.Vector2 rot;
        private Microsoft.Xna.Framework.Vector3 prevVel;
        private Microsoft.Xna.Framework.Vector3 nextVel;
        public float deltaTime { get; private set; }

        private float fov;

        public float mouseSensitivity { get; set; }
        public float movementSpeed { get; set; }
        public bool onGround { get; set; }
        public bool mouseLock { get; set; }
        public bool canMove { get; set; }
        public float gravity { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public float halfWidth { get; set; }
        public float halfHeight { get; set; }
        public float stepHeight { get; set; }
        public bool debugCanStepX { get; set; }
        public bool debugCanStepZ { get; set; }
        public float jumpHeight { get; set; }
        public float damping { get; set; }
        public Microsoft.Xna.Framework.Vector3 cameraPosition { get; set; }

        public Player(Microsoft.Xna.Framework.Vector3 position, float moveSpeed) {
            this.pos = position;
            this.movementSpeed = moveSpeed;
            this.mouseSensitivity = 0.002f;
            this.mouseLock = true;
            this.canMove = true;
            this.rot = new Microsoft.Xna.Framework.Vector2(0, 0);
            this.fov = 90f;
            this.gravity = 15f;
            this.width = 0.6f;
            this.height = 1.8f;
            this.halfWidth = this.width / 2;
            this.halfHeight = this.height / 2;
            this.stepHeight = 0.6f;
            this.jumpHeight = 6f;
            this.damping = 10f;
            this.onGround = false;
            this.cameraPosition = position + new Microsoft.Xna.Framework.Vector3(0, this.halfHeight / 2f, 0);
            this.debugCanStepX = false;
            this.debugCanStepZ = false;
        }

        public void bruh(float buh) {
            deltaTime = buh;
        }

        public Microsoft.Xna.Framework.Vector3 position {
            get { return pos; }
            set { pos = value; }
        }
        public float x {
            get { return pos.X; }
            set { pos.X = value; }
        }
        public float y {
            get { return pos.Y; }
            set { pos.Y = value; }
        }
        public float z {
            get { return pos.Z; }
            set { pos.Z = value; }
        }

        public Microsoft.Xna.Framework.Vector3 velocity {
            get { return vel; }
            set { vel = value; }
        }
        public float xVel {
            get { return vel.X; }
            set { vel.X = value; }
        }
        public float yVel {
            get { return vel.Y; }
            set { vel.Y = value; }
        }
        public float zVel {
            get { return vel.Z; }
            set { vel.Z = value; }
        }

        public Microsoft.Xna.Framework.Vector3 prevVelocity {
            get { return prevVel; }
            set { prevVel = value; }
        }
        public float prevxVel {
            get { return prevVel.X; }
            set { prevVel.X = value; }
        }
        public float prevyVel {
            get { return prevVel.Y; }
            set { prevVel.Y = value; }
        }
        public float prevzVel {
            get { return prevVel.Z; }
            set { prevVel.Z = value; }
        }

        public Microsoft.Xna.Framework.Vector3 nextVelocity {
            get { return nextVel; }
            set { nextVel = value; }
        }
        public float nextxVel {
            get { return nextVel.X; }
            set { nextVel.X = value; }
        }
        public float nextyVel {
            get { return nextVel.Y; }
            set { nextVel.Y = value; }
        }
        public float nextzVel {
            get { return nextVel.Z; }
            set { nextVel.Z = value; }
        }

        public float xMax {
            get { return pos.X + halfWidth; }
        }
        public float xMin {
            get { return pos.X - halfWidth; }
        }
        public float yMax {
            get { return pos.Y + halfHeight; }
        }
        public float yMin {
            get { return pos.Y - halfHeight; }
        }
        public float zMax {
            get { return pos.Z + halfWidth; }
        }
        public float zMin {
            get { return pos.Z - halfWidth; }
        }

        public float xMaxNext {
            get { return pos.X + halfWidth + (vel.X * deltaTime); }
        }
        public float xMinNext {
            get { return pos.X - halfWidth + (vel.X * deltaTime); }
        }
        public float yMaxNext {
            get { return pos.Y + halfHeight + (vel.Y * deltaTime); }
        }
        public float yMinNext {
            get { return pos.Y - halfHeight + (vel.Y * deltaTime); }
        }
        public float zMaxNext {
            get { return pos.Z + halfWidth + (vel.Z * deltaTime); }
        }
        public float zMinNext {
            get { return pos.Z - halfWidth + (vel.Z * deltaTime); }
        }

        public float xMaxNextPrev {
            get { return pos.X + halfWidth + (prevVel.X * deltaTime); }
        }
        public float xMinNextPrev {
            get { return pos.X - halfWidth + (prevVel.X * deltaTime); }
        }
        public float yMaxNextPrev {
            get { return pos.Y + halfHeight + (prevVel.Y * deltaTime); }
        }
        public float yMinNextPrev {
            get { return pos.Y - halfHeight + (prevVel.Y * deltaTime); }
        }
        public float zMaxNextPrev {
            get { return pos.Z + halfWidth + (prevVel.Z * deltaTime); }
        }
        public float zMinNextPrev {
            get { return pos.Z - halfWidth + (prevVel.Z * deltaTime); }
        }

        public Microsoft.Xna.Framework.Vector2 rotation {
            get { return rot; }
            set { rot = value; }
        }
        public float r {
            get { return rot.Y; }
            set { rot.Y = value; }
        }
        public float t {
            get { return rot.X; }
            set { rot.X = value; }
        }

        public float fieldOfView {
            get { return fov * (MathHelper.Pi / 180f); }
            set { fov = value; }
        }

    }
}
