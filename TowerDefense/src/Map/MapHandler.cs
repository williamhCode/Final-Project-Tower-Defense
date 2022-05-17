using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Collections.Generic;

namespace TowerDefense.Map
{
    public enum TileType
    {
        Undefined = -1,
        Grass,
        Path,
    }

    public class MapHandler
    {
        public TileType[][] tileTypeMap { get; private set; }
        public int[][] tileMap { get; private set; }
        public Dictionary<int, Texture2D> textureDict { get; private set; }

        private readonly int WIDTH;
        private readonly int HEIGHT;
        private readonly int TILESIZE;

        public MapHandler(int width, int height, int tileSize)
        {
            WIDTH = width;
            HEIGHT = height;
            TILESIZE = tileSize;

            tileTypeMap = new TileType[HEIGHT][];
            for (int i = 0; i < tileTypeMap.Length; i++)
            {
                tileTypeMap[i] = new TileType[WIDTH];
            }
            for (int i = 0; i < tileTypeMap.Length; i++)
            {
                for (int j = 0; j < tileTypeMap[i].Length; j++)
                {
                    tileTypeMap[i][j] = TileType.Undefined;
                }
            }

            tileMap = new int[HEIGHT][];
            for (int i = 0; i < tileMap.Length; i++)
            {
                tileMap[i] = new int[WIDTH];
            }
            for (int i = 0; i < tileMap.Length; i++)
            {
                for (int j = 0; j < tileMap[i].Length; j++)
                {
                    tileMap[i][j] = -1;
                }
            }

            textureDict = new Dictionary<int, Texture2D>();
        }

        public void LoadTileInfo(Game game, string path)
        {
            dynamic tileInfo = JObject.Parse(File.ReadAllText(path));

            game.Content.RootDirectory = tileInfo.RootDirectory;

            dynamic dataList = tileInfo.DataList;
            for (int i = 0; i < dataList.Count; i++)
            {
                dynamic data = dataList[i];

                Texture2D spriteSheet = game.Content.Load<Texture2D>((string)data.TileName);

                int tileSize = data.TileSize;
                int width = data.Width;
                int height = data.Height;

                dynamic configList = data.ConfigList;
                for (int j = 0; j < configList.Count; j++)
                {
                    string config = configList[j];
                    if (config == "None") continue;
                    config = config.Replace(",", "");
                    config = config.Remove(4, 1);
                    config = new string(config.Reverse().ToArray());

                    int value = Convert.ToInt32(config, 2);
                    value |= i << 8;

                    Texture2D texture = new Texture2D(game.GraphicsDevice, tileSize, tileSize);
                    // set texture data from sprite sheet
                    Color[] textureData = new Color[tileSize * tileSize];
                    int x = (j % width) * tileSize;
                    int y = (j / width) * tileSize;
                    spriteSheet.GetData(0, new Rectangle(x, y, tileSize, tileSize), textureData, 0, tileSize * tileSize);
                    texture.SetData(textureData);

                    textureDict.Add(value, texture);
                }
            }
        }

        readonly (int X, int Y)[] neighborOffsets = new[] { (-1, -1), (0, -1), (1, -1), (-1, 0), (1, 0), (-1, 1), (0, 1), (1, 1) };
        public void UpdateTile(int xPos, int yPos)
        {
            TileType tileType;
            try { tileType = tileTypeMap[xPos][yPos]; }
            catch (IndexOutOfRangeException) { return; }

            if (tileType == TileType.Undefined) return;

            // nB = neighborsBool
            var nB = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                var offset = neighborOffsets[i];
                int x = xPos + offset.X;
                int y = yPos + offset.Y;

                try { nB[i] = tileType == tileTypeMap[x][y]; }
                catch (IndexOutOfRangeException) { nB[i] = false; }
            }

            // corner tiles only valid if its surrounded by the same tile
            nB[0] = nB[0] && nB[1] && nB[3];
            nB[2] = nB[2] && nB[1] && nB[4];
            nB[5] = nB[5] && nB[3] && nB[6];
            nB[7] = nB[7] && nB[4] && nB[6];

            // convert nB to int
            int nBInt = 0;
            for (int i = 0; i < 8; i++)
            {
                if (nB[i])
                    nBInt |= 1 << i;
            }
            nBInt |= (int)tileType << 8;

            if (!textureDict.ContainsKey(nBInt))
            {
                nBInt = (int)tileType << 8;
            }

            tileMap[xPos][yPos] = nBInt;
        }

        public void UpdateTileMap()
        {
            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    UpdateTile(i, j);
                }
            }
        }

        public void DrawMap(SpriteBatch spriteBatch, Camera.Camera2D camera)
        {
            var viewport = camera.GetViewport();
            int xStart = (int)Math.Floor(viewport.X / TILESIZE);
            int yStart = (int)Math.Floor(viewport.Y / TILESIZE);
            int xEnd = (int)Math.Ceiling((viewport.X + viewport.Width) / TILESIZE);
            int yEnd = (int)Math.Ceiling((viewport.Y + viewport.Height) / TILESIZE);

            xStart = Math.Max(xStart, 0);
            yStart = Math.Max(yStart, 0);
            xEnd = Math.Min(xEnd, WIDTH);
            yEnd = Math.Min(yEnd, HEIGHT);

            for (int row = xStart; row < xEnd; row++)
            {
                for (int col = yStart; col < yEnd; col++)
                {
                    if (tileTypeMap[row][col] != TileType.Undefined)
                    {
                        spriteBatch.Draw(textureDict[tileMap[row][col]], new Vector2(TILESIZE * row, TILESIZE * col), Color.White);
                    }
                }
            }
        }

        public void SaveMap()
        {
            dynamic map = new JObject();
            map.Width = WIDTH;
            map.Height = HEIGHT;
            map.Data = new JArray();
            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    map.Data.Add(tileTypeMap[i][j]);
                }
            }

            File.WriteAllText("Content/Map.json", map.ToString());
            Console.WriteLine("Map saved.");
        }

        public void LoadMap()
        {
            dynamic map = JObject.Parse(File.ReadAllText("Content/Map.json"));

            tileTypeMap = new TileType[WIDTH][];
            for (int i = 0; i < WIDTH; i++)
            {
                tileTypeMap[i] = new TileType[HEIGHT];
            }

            for (int i = 0; i < HEIGHT; i++)
            {
                for (int j = 0; j < WIDTH; j++)
                {
                    tileTypeMap[j][i] = (TileType)map.Data[i * WIDTH + j];
                }
            }
        }
    }
}