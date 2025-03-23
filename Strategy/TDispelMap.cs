﻿using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;

namespace Strategy
{
    internal class TDispelMap: TMap
    {
        public static int ChunkSize = 25;
        public int WorldWidth;
        public int WorldHeight;
        public Vector2 GridOffset;
        public Encoding Encoding;
        public byte[] ReadBuffer;

        int Rgb16To32(int byte0, int byte1)
        {
            int color = (byte0 & 0x1F) << 3 | (byte0 & 0xE0) << 5;
            color |= (byte1 & 7) << 13 | (byte1 & 0xF8) << 16;
            if (color > 0) color |= unchecked((int)0xFF000000);
            return color;
        }
        byte[] Rgb32To16(int color)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)((color & 0xF8) >> 3 | (color & 0x1C00) >> 5);
            bytes[1] = (byte)(color >> 16 | (color & 0xE000) >> 13);
            return bytes;
        }
        public List<Image> ReadTileSet(string filename, string ext)
        {
            TGame.TileHeight = 32;
            TGame.TileWidth = 64;
            filename = filename.Substring(0, filename.Length - 4) + ext;
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var pixels = new byte[fStream.Length];
            fStream.Read(pixels, 0, pixels.Length);
            var images = new List<Image>();
            int pos = 0;
            while (pos < pixels.Length)
            {
                var tile = new TPixmap(TGame.TileWidth, TGame.TileHeight);                
                for (int y = 0; y < tile.Height; y++)
                {
                    var n = y < tile.Height / 2 ? y : tile.Height - 1 - y;
                    var r = 1 + 2 * n;
                    for (int x = tile.Width / 2 - r; x < tile.Width / 2 + r; x++)
                        tile[x, y] = Rgb16To32(pixels[pos++], pixels[pos++]);
                }
                images.Add(tile.Image);
            }
            fStream.Close();
            return images;
        }

        public void MapTileSet()
        {
            int mapSize = (int)Math.Sqrt(Game.GroundTilesImages.Count);
            var map = new Bitmap(mapSize, mapSize);
            Game.Cells = new TCell[map.Height, map.Width];
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var cell = new TCell();
                    cell.Game = Game;
                    cell.X = x;
                    cell.Y = y;
                    var tile = new TTile();
                    tile.ImageIndex = y * map.Height + x;
                    cell.GroundTile = tile;
                    Game.Cells[y, x] = cell;
                }
            //Game.Map = map;
            Height = map.Height;
            Width = map.Width;
        }

        public void WriteMap(string filename)
        {
            var fStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            var writer = new BinaryWriter(fStream);
            writer.Write((WorldWidth + 1) / ChunkSize);
            writer.Write((WorldHeight + 1) / ChunkSize);
            int propCount = 2;
            writer.Write(propCount);
            writer.Write(Game.GroundTilesImages.Count);
            var bytes = new byte[(Game.GroundTilesImages.Count - 1) * propCount * sizeof(int)];
            writer.Write(bytes);
            writer.Write(Game.BlockTilesImages.Count);
            bytes = new byte[Game.BlockTilesImages.Count * propCount];
            writer.Write(bytes);
            WriteSprites(writer);
            WriteColumnBlockTiles(writer);
            //writer.Write(ReadBuffer);
            WriteCells(writer);
            writer.Close();
        }

        void WriteCells(BinaryWriter writer)
        {
            var cells = TransformToHexMapping();
            foreach (var cell in cells)
            {
                var eventIdx = cell == null ? 0 : cell.EventIdx;
                writer.Write((short)eventIdx);
                writer.Write((short)0);
            }
            foreach (var cell in cells)
            {
                var idx = 0;
                if (cell != null)
                {
                    idx = cell.GroundTile.ImageIndex << 10;
                    if (cell.Collision) idx |= 1;
                }
                writer.Write(idx);
            }
            var bytes = new byte[4 * WorldHeight * WorldWidth];
            foreach (var roofTile in Game.RoofTiles)
            {
                var cell = roofTile.Cells[0];
                var pos = UnTransformGrid(cell.X, cell.Y);
                var linPos = (int)pos.Y * WorldWidth + (int)pos.X;
                var num = cell.Piece.ImageIndex;
                bytes[4 * linPos + 0] = (byte)num;
                bytes[4 * linPos + 1] = (byte)(num >> 8);
                bytes[4 * linPos + 2] = (byte)(num >> 16);
                bytes[4 * linPos + 3] = (byte)(num >> 24);
            }
            writer.Write(bytes);
        }

        void WriteColumnBlockTiles(BinaryWriter writer)
        {
            writer.Write(Game.ColumnTiles.Count);
            for (int i = 0; i < Game.ColumnTiles.Count; i++)
            {
                var column = Game.ColumnTiles[i];
                column.Images = Game.BlockTilesImages;
                writer.Write(1);
                writer.Write(new byte[260]);
                writer.Write(column.Id);
                writer.Write(1); // viewsCount
                writer.Write(0);
                writer.Write(1); // framesCount
                writer.Write(0);
                writer.Write(column.Bounds.Left);
                writer.Write(column.Bounds.Top);
                writer.Write(column.Bounds.Right);
                writer.Write(column.Bounds.Bottom);
                writer.Write(column.X);
                writer.Write(column.Y);
                writer.Write(column.X_);
                writer.Write(column.Y_);
                writer.Write(1);
                writer.Write(column.Cells.Count);
                writer.Write(column.Cells.Count);
                var props = new List<int>();
                for (int m = 0; m < column.Cells.Count; m++)
                {
                    var imageIdx = (short)column.Cells[m].Piece.ImageIndex;
                    writer.Write(imageIdx);
                    props.Add(BTilesProps[2 * imageIdx]);
                    props.Add(BTilesProps[2 * imageIdx + 1]);
                }
                for (int m = 0; m < props.Count; m++)
                    writer.Write(props[m]);
            }
        }

        public void WriteAnimation(BinaryWriter writer, TAnimation animation)
        {
            writer.Write(animation.Sequences.Count);
            foreach (var sequence in animation.Sequences)
            {
                writer.Write(new byte[264]);
                writer.Write(sequence.Length);
                foreach (var view in sequence)
                {
                    writer.Write(0);
                    writer.Write(view.Length);
                    writer.Write(0);
                    foreach (var frame in view)
                    {
                        writer.Write(frame.Bounds.Left);
                        writer.Write(frame.Bounds.Top);
                        writer.Write(frame.Bounds.Right);
                        writer.Write(frame.Bounds.Bottom);
                        writer.Write(frame.Bounds.Left);
                        writer.Write(frame.Bounds.Top);
                        writer.Write(frame.Offset.X);
                        writer.Write(frame.Offset.Y);
                        writer.Write(frame.Bounds.Width);
                        writer.Write(frame.Bounds.Height);
                        writer.Write(frame.Bounds.Width * frame.Bounds.Height);
                        var tile = new TPixmap(frame.Bounds.Width, frame.Bounds.Height);
                        tile.Image = frame.Image;
                        for (int y = 0; y < tile.Height; y++)
                            for (int x = 0; x < tile.Width; x++)
                                writer.Write(Rgb32To16(tile[x, y]));
                    }
                }
            }
        }

        private void WriteSprites(BinaryWriter writer)
        {
            writer.Write(Game.Animations.Count);
            foreach (var animation in Game.Animations)
                WriteAnimation(writer, animation);
            var spriteList = new List<TSprite>();
            foreach (var sprite in Game.Sprites)
            {
                if (sprite is TElement || sprite is TMonster || sprite is TNpc)
                    continue;
                spriteList.Add(sprite);
            }
            writer.Write(spriteList.Count);
            foreach (var sprite in spriteList)
            {
                writer.Write(sprite.Animation.Index);
                foreach (var frame in sprite.Frames)
                {
                    writer.Write(sprite.Bounds.Left);
                    writer.Write(sprite.Bounds.Top);
                    writer.Write(sprite.Bounds.Right);
                    writer.Write(sprite.Bounds.Bottom);
                    writer.Write(sprite.X);
                    writer.Write(sprite.Y);
                }
            }
        }
        byte[] BTilesProps;
        public override void ReadMap(string filename)
        {
            Cursor.Current = Cursors.WaitCursor;
            Encoding = Encoding.GetEncoding(1250);
            MapName = Path.GetFileNameWithoutExtension(filename);
            GamePath = Path.GetDirectoryName(Path.GetDirectoryName(filename));
            Game.GroundTilesImages = ReadTileSet(filename, ".gtl");
            Game.BlockTilesImages = ReadTileSet(filename, ".btl");
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fStream);
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            WorldWidth = width * ChunkSize - 1;
            WorldHeight = height * ChunkSize - 1;
            Game.Cells = new TCell[WorldHeight, WorldWidth];
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var cell = new TCell();
                    cell.Game = Game;
                    cell.X = x;
                    cell.Y = y;
                    Game.Cells[y, x] = cell;
                }
            int diagonalSize = WorldWidth + WorldHeight;
            var hexMapSize = new Vector2(diagonalSize, diagonalSize);
            var ratio = new Vector2(0.4f, 0.6f);
            var mapSize = ratio * hexMapSize;
            var offset = (hexMapSize - mapSize) / 2;
            GridOffset.X = -(int)(offset.X);
            GridOffset.Y = WorldHeight - (int)(offset.Y);
            Height = (int)mapSize.Y;
            Width = (int)mapSize.X;
            int propCount = reader.ReadInt32();
            int tileCount = reader.ReadInt32();
            reader.ReadBytes((tileCount - 1) * propCount * sizeof(int));
            tileCount = reader.ReadInt32();
            BTilesProps = reader.ReadBytes(tileCount * propCount);
            ReadSprites(reader);
            ReadColumnBlockTiles(reader);
            var pos = reader.BaseStream.Position;
            ReadCells(reader);
            ReadRoofBlockTiles(reader);
            Game.Cells = UntransformFromHexMapping();
            reader.BaseStream.Position = pos;
            ReadBuffer = reader.ReadBytes((int)reader.BaseStream.Length - (int)pos);
            reader.Close();
            LoadExtras(filename);
            LoadMonsters(filename);
            LoadNpc(filename);
            Game.MapNames = LoadInfo($"{GamePath}/AllMap.ini", 1);     
            Dialogs = new TIniReader($"{Game.Map.GamePath}/NpcInGame/Pgp{Game.Map.MapName}.pgp", '|')[""];
            DialogTree = new TIniReader($"{Game.Map.GamePath}/NpcInGame/Dlg{Game.Map.MapName}.dlg")[""];
            RebuildMapView();
            Cursor.Current = Cursors.Default;
        }

        private Dictionary<int, string> LoadInfo(string path, int spriteNameColumnIndex)
        {
            return File.ReadLines(path)
                    .Where(line => !line.StartsWith(";"))
                    .Select(line => line.Split(','))
                    .Where(fields => fields[1] != "null")
                    .ToDictionary(fields => int.Parse(fields[0]), fields => fields[spriteNameColumnIndex]);
        }

        void ReadSpriteModels(List<TAnimation> spriteModels, Dictionary<int, string> names, string path)
        {
            for (var i = 0; i < names.Count; i++)
            {
                var filename = path + names.Values.ElementAt(i);
                if (File.Exists(filename))
                {
                    var file = File.OpenRead(filename);
                    var reader = new BinaryReader(file);
                    var animation = ReadAnimation(reader);
                    //var animation = Game.Animations.Last();
                    animation.Index = names.Keys.ElementAt(i);
                    animation.Name = names[animation.Index];
                    spriteModels[animation.Index] = animation;
                    reader.Close();
                }
            }
        }
        public void LoadExtras(string filename)
        {
            var path = $"{GamePath}/ExtraInGame/";
            var mapRefPath = $"{path}/Ext{MapName}.ref";
            if (!File.Exists(mapRefPath)) return;
            if (Game.ExtraSpriteModels.Count == 0)
            {
                var names = LoadInfo($"{GamePath}/Extra.ini", 1);
                Game.ExtraSpriteModels = new List<TAnimation>(new TAnimation[names.Keys.ElementAt(names.Count - 1) + 1]);
                ReadSpriteModels(Game.ExtraSpriteModels, names, path);
            }
            using (var reader = new BinaryReader(File.OpenRead(mapRefPath)))
            {
                var count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var element = new TElement();
                    //element.Source = Game.ExtraSpriteModels;
                    element.Id = reader.ReadByte();
                    reader.ReadByte();
                    var modelIdx = reader.ReadByte();
                    element.Animation = Game.ExtraSpriteModels[modelIdx];
                    var name = Encoding.GetString(reader.ReadBytes(32));// 0xcd
                    var zidx = name.IndexOf('\0');
                    if (zidx >= 0) name = name.Substring(0, zidx);
                    element.Name = name;
                    element.Type = (TElementType)reader.ReadByte();
                    var x = reader.ReadInt32();
                    var y = reader.ReadInt32();
                    var pos = TransformGrid(x, y);
                    element.X = (int)((pos.X + 1) * TGame.TileWidth / 2);
                    element.Y = (int)(pos.Y * TGame.TileHeight / 2);

                    element.ViewAngle = reader.ReadByte(); // rotation
                    reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();

                    reader.ReadInt32();
                    element.Closed = reader.ReadInt32();// "closed", AsInt32(), "chest 0-open, 1-closed");

                    element.RequiredItem1Id = reader.ReadByte(); // lower bound
                    element.RequiredItem1Type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    element.RequiredItem2Id = reader.ReadByte(); // upper bound
                    element.RequiredItem2Type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    element.Gold = reader.ReadInt32();
                    element.Item1Id = reader.ReadByte(); // lower bound
                    element.Item1Type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    element.ItemCount = reader.ReadInt32();
                    reader.ReadBytes(10 * sizeof(int));

                    element.EventId = reader.ReadInt32();
                    element.MessageId = reader.ReadInt32(); // "id from message.scr for signs");
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadBytes(32);
                    element.Visibility = reader.ReadByte();
                    reader.ReadBytes(3);

                    element.Bounds = new Rectangle(element.X, element.Y, element.Frames[0].Bounds.Width, element.Frames[0].Bounds.Height);
                    element.Bounds.Offset(-element.ActFrame.Offset.X, -element.ActFrame.Offset.Y);
                    Game.Sprites.Add(element);
                }
            }
        }

        public void LoadMonsters(string filename)
        {
            var path = $"{GamePath}/MonsterInGame/";
            var mapRefPath = $"{path}/Mon{MapName}.ref";
            if (!File.Exists(mapRefPath)) return;
            if (Game.MonsterSpriteModels.Count == 0)
            {
                var names = LoadInfo($"{GamePath}/Monster.ini", 2);
                Game.MonsterSpriteModels = new List<TAnimation>(new TAnimation[names.Keys.ElementAt(names.Count - 1) + 1]);
                ReadSpriteModels(Game.MonsterSpriteModels, names, path);
            }
            using (var reader = new BinaryReader(File.OpenRead(mapRefPath)))
            {
                var count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var monster = new TMonster();
                    monster.Order = monster.X;
                    //monster.ModelsSource = Game.MonsterSpriteModels;
                    monster.Id = reader.ReadInt32();
                    var modelIdx = reader.ReadInt32();
                    monster.Animation = Game.MonsterSpriteModels[modelIdx];
                    monster.ViewAngle = monster.Animation.Sequences[0].Length - 1;
                    var x = reader.ReadInt32();
                    var y = reader.ReadInt32();
                    var pos = TransformGrid(x, y);
                    monster.X = (int)((pos.X  + 1) * TGame.TileWidth / 2);
                    monster.Y = (int)(pos.Y * TGame.TileHeight / 2);
                    var unk = reader.ReadBytes(5 * sizeof(int));
                    //reader.ReadInt32();
                    //reader.ReadInt32();
                    //reader.ReadInt32();
                    //reader.ReadInt32();
                    //reader.ReadInt32();
                    monster.LootSlot1Id = reader.ReadByte();
                    monster.LootSlot1Type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    monster.LootSlot2Id = reader.ReadByte();
                    monster.LootSlot2Type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    monster.LootSlot3Id = reader.ReadByte();
                    monster.LootSlot3Type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadInt32();
                    reader.ReadInt32();

                    monster.Bounds = new Rectangle(monster.X, monster.Y, monster.Frames[0].Bounds.Width, monster.Frames[0].Bounds.Height);
                    monster.Bounds.Offset(-monster.ActFrame.Offset.X, -monster.ActFrame.Offset.Y);
                    Game.Sprites.Add(monster);
                    Game.AnimatedSprites.Add(monster);
                }
            }
        }

        public void LoadNpc(string filename)
        {
            var path = $"{GamePath}/NpcInGame/";
            var mapRefPath = $"{path}/Npc{MapName}.ref";
            if (!File.Exists(mapRefPath)) return;
            if (Game.NpcSpriteModels.Count == 0)
            {
                var names = LoadInfo($"{GamePath}/Npc.ini", 1);
                Game.NpcSpriteModels = new List<TAnimation>(new TAnimation[names.Keys.ElementAt(names.Count - 1) + 1]);
                ReadSpriteModels(Game.NpcSpriteModels, names, path);
            }
            byte FILLER = 0xCD;
            int STRING_MAX_LENGTH = 260;
            using (var reader = new BinaryReader(File.OpenRead(mapRefPath)))
            {
                var count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var npc = new TNpc();
                    //npc.ModelsSource = Game.NpcSpriteModels;
                    npc.Id = reader.ReadInt32();
                    var modelIdx = reader.ReadInt32();
                    //sprite.ViewAngle = sprite.ModelsSource[sprite.ModelIdx][0].Length - 1;
                    npc.Animation = Game.NpcSpriteModels[modelIdx];
                    var name = Encoding.GetString(reader.ReadBytes(STRING_MAX_LENGTH));
                    npc.Name = name.Substring(0, name.IndexOf('\0'));
                    name = Encoding.GetString(reader.ReadBytes(STRING_MAX_LENGTH));
                    npc.Description = name.Substring(0, name.IndexOf('\0'));
                    npc.ScriptId = reader.ReadInt32();// party/scriptId
                    npc.OnShowEvent = reader.ReadInt32();
                    reader.ReadInt32();
                    npc.GoTo1Filled = reader.ReadInt32();
                    npc.GoTo2Filled = reader.ReadInt32();
                    npc.GoTo3Filled = reader.ReadInt32();
                    npc.GoTo4Filled = reader.ReadInt32();// when goto4 not filled its 1, idk why
                    npc.GoTo1X = reader.ReadInt32();
                    npc.GoTo2X = reader.ReadInt32();
                    npc.GoTo3X = reader.ReadInt32();
                    npc.GoTo4X = reader.ReadInt32();
                    npc.GoTo1Y = reader.ReadInt32();
                    npc.GoTo2Y = reader.ReadInt32();
                    npc.GoTo3Y = reader.ReadInt32();
                    npc.GoTo4Y = reader.ReadInt32();
                    var pos = TransformGrid(npc.GoTo1X, npc.GoTo1Y);
                    npc.X = (int)((pos.X + 1) * TGame.TileWidth / 2);
                    npc.Y = (int)(pos.Y * TGame.TileHeight / 2);

                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    npc.ViewAngle = reader.ReadInt32();// 0 = up, clockwise
                    //if (npc.ViewAngle > 4)
                    //{
                    //    npc.Flipped = true;
                    //    npc.ViewAngle = 8 - sprite.ViewAngle;
                    //}
                    var viewsCount = npc.Animation.Sequences[0].Length;
                    if (npc.ViewAngle >= viewsCount)
                    {
                        npc.Flipped = true;
                        npc.ViewAngle = 2 * viewsCount - 2 - npc.ViewAngle;
                    }
                    reader.ReadBytes(14 * sizeof(int));

                    npc.DialogId = reader.ReadInt32();// also text for shop
                    reader.ReadInt32();

                    npc.Bounds = new Rectangle(npc.X, npc.Y, npc.Frames[0].Bounds.Width, npc.Frames[0].Bounds.Height);
                    npc.Bounds.Offset(-npc.ActFrame.Offset.X, -npc.ActFrame.Offset.Y);
                    Game.Sprites.Add(npc);
                    Game.AnimatedSprites.Add(npc);
                }
            }
        }

        public Vector2 TransformGrid(float x, float y)
        {
            return new Vector2(x + y, y - x) + GridOffset;
        }
        public Vector2 UnTransformGrid(float x_, float y_)
        {
            x_ -= GridOffset.X;
            y_ -= GridOffset.Y;
            return new Vector2(x_ - y_, y_ + x_) / 2;
        }
        TCell[,] UntransformFromHexMapping()
        {
            var cells = new TCell[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var pos = UnTransformGrid(x, y);
                    cells[y, x] = Game.Cells[(int)pos.Y, (int)pos.X];
                }
            //Game.Cells = cells;
            return cells;
        }

        TCell[,] TransformToHexMapping()
        {
            var cells = new TCell[WorldHeight, WorldWidth];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var pos = UnTransformGrid(x, y);
                    if (pos.Y >= 0 && pos.Y < WorldHeight && pos.X >= 0 && pos.X < WorldWidth)
                        cells[(int)pos.Y, (int)pos.X] = Game.Cells[y, x];
                }
            return cells;
        }

        void ReadCells(BinaryReader reader)
        {
            foreach (var cell in Game.Cells)
            {
                short eventId = reader.ReadInt16();
                short id = reader.ReadInt16();
                cell.EventIdx = eventId;
            }
            foreach (var cell in Game.Cells)
            {
                int idx = reader.ReadInt32();
                var tile = new TTile();
                tile.ImageIndex = idx >> 10;
                cell.GroundTile = tile;
                cell.Collision = (idx & 1) != 0;
            }
        }

        private void ReadRoofBlockTiles(BinaryReader reader)
        {
            Game.RoofTiles.Clear();
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    int idx = reader.ReadInt32();
                    if (idx > 0 && idx < Game.BlockTilesImages.Count)
                    {
                        var roofTile = new TColumnTile();
                        roofTile.Images = Game.BlockTilesImages;
                        var pos = TransformGrid(x, y);
                        var cell = new TCell();
                        cell.Piece = new TTile() { ImageIndex = idx };
                        cell.X = (int)(pos.X * TGame.TileWidth / 2);
                        cell.Y = (int)((pos.Y - 1) * TGame.TileHeight / 2);
                        roofTile.Cells.Add(cell);
                        roofTile.Bounds = new Rectangle(cell.X, cell.Y, TGame.TileWidth, TGame.TileHeight);
                        Game.RoofTiles.Add(roofTile);
                    }
                    else if (idx > 0)
                        ;
                }
        }

        void ReadColumnBlockTiles(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            Game.ColumnTiles.Clear();
            for (int i = 0; i < count; i++)
            {
                int unk = reader.ReadInt32(); // = 1
                var column = new TColumnTile();
                column.Images = Game.BlockTilesImages;
                reader.ReadBytes(260);
                column.Id = reader.ReadInt32();
                var viewsCount = reader.ReadInt32();
                for (int j = 0; j < viewsCount; j++)
                {
                    reader.ReadInt32();
                    var framesCount = reader.ReadInt64();
                    for (int k = 0; k < framesCount; k++)
                    {
                        int left = reader.ReadInt32();
                        int top = reader.ReadInt32();
                        int right = reader.ReadInt32();
                        int bottom = reader.ReadInt32();
                        column.X = reader.ReadInt32();
                        column.Y = reader.ReadInt32();
                        column.X_ = reader.ReadInt32();
                        column.Y_ = reader.ReadInt32();
                        var pos = UnTransformGrid(column.X / TGame.TileWidth * 2, column.Y / TGame.TileHeight * 2);                        reader.ReadInt32();
                        int tilesCount = reader.ReadInt32();
                        reader.ReadInt32();
                        column.Order = Game.ColumnTiles.Count;
                        column.Bounds = Rectangle.FromLTRB(column.X, column.Y, right, bottom);
                        for (int m = 0; m < tilesCount; m++)
                        {
                            var cell = new TCell();
                            cell.X = column.X;
                            cell.Y = column.Y + m * TGame.TileHeight;
                            var tile = new TTile();
                            tile.ImageIndex = reader.ReadInt16();
                            cell.Piece = tile;
                            column.Cells.Add(cell);
                        }
                    }
                }
                for (int m = 0; m < 2 * column.Cells.Count; m++)
                    reader.ReadInt32();
                Game.ColumnTiles.Add(column);
            }
            //Game.ColumnTiles.Sort();
        }

        Bitmap ReadImage(byte[] pixels, int width, int height)
        {
            var pos = 0;
            var tile = new TPixmap(width, height);
            for (int y = 0; y < tile.Height; y++)
                for (int x = 0; x < tile.Width; x++)
                    tile[x, y] = Rgb16To32(pixels[pos++], pixels[pos++]);
            return tile.Image;
        }

        List<TFrame> ReadView(BinaryReader reader)
        {
            var view = new List<TFrame>();
            reader.ReadInt32();
            var framesCount = (int)reader.ReadInt64();
            for (int k = 0; k < framesCount; k++)
            {
                var frame = new TFrame();
                var left = reader.ReadInt32();
                var top = reader.ReadInt32();
                var right = reader.ReadInt32();
                var bottom = reader.ReadInt32();
                var x = reader.ReadInt32();
                var y = reader.ReadInt32();
                frame.Offset.X = reader.ReadInt32();
                frame.Offset.Y = reader.ReadInt32();
                var width = reader.ReadInt32();
                var height = reader.ReadInt32();
                frame.Bounds = new Rectangle(x, y, width, height);
                var pixelCount = reader.ReadInt32();
                if (pixelCount > 0)
                {
                    var pixels = reader.ReadBytes(pixelCount * 2);
                    frame.Image = ReadImage(pixels, width, height);
                    view.Add(frame);
                }
            }
            return view;
        }

        public TAnimation ReadAnimation(BinaryReader reader)
        {
            var animation = new TAnimation();
            animation.Source = Game.Animations;
            animation.Index = Game.Animations.Count;
            int sequencesCount = reader.ReadInt32();
            if (sequencesCount == 6)
                animation.Short6Seq = true;
            for (int j = 0; j < sequencesCount; j++)
            {
                var sequence = new List<TFrame[]>();
                var header = reader.ReadBytes(264);
                var viewsCount = reader.ReadInt32();
                for (int k = 0; k < viewsCount; k++)
                {
                    var view = ReadView(reader);
                    if (view.Count > 0)
                        sequence.Add(view.ToArray());
                }
                if (sequence.Count > 0)
                    animation.Sequences.Add(sequence.ToArray());
            }
            //Game.Animations.Add(animation);
            return animation;
        }

        private void ReadSprites(BinaryReader reader)
        {
            Game.Animations.Clear();
            int animationsCount = reader.ReadInt32();
            for (int i = 0; i < animationsCount; i++)
                Game.Animations.Add(ReadAnimation(reader));
            Game.Sprites.Clear();
            Game.AnimatedSprites.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var sprite = new TSprite();
                var modelIdx = reader.ReadInt32();
                sprite.Animation = Game.Animations[modelIdx];
                for (int j = 0; j < sprite.Frames.Length; j++)
                {
                    var frame = sprite.Frames[j];
                    int left = reader.ReadInt32();
                    int top = reader.ReadInt32();
                    int right = reader.ReadInt32();
                    int bottom = reader.ReadInt32();
                    sprite.X = reader.ReadInt32();
                    sprite.Y = reader.ReadInt32();
                    var frameBounds = Rectangle.FromLTRB(left, top, right, bottom);
                    frame.Offset = Point.Empty;
                    sprite.Bounds = j == 0 ? frameBounds : Rectangle.Union(sprite.Bounds, frameBounds);
                }
                if (sprite.Frames.Length > 1)
                    Game.AnimatedSprites.Add(sprite);
                Game.Sprites.Add(sprite);
            }
        }

        public override void RebuildMapView()
        {
            Game.MapView = new Bitmap(Game.MapView.Width, Game.MapView.Height);
            var sx = TGame.TileWidth / 2;
            var sy = TGame.TileHeight / 2;
            var gc = Graphics.FromImage(Game.MapView);
            gc.ScaleTransform((float)Game.MapView.Width / (Game.Map.Width * sx), (float)Game.MapView.Height / (Game.Map.Height * sy));           
            var vp = new Rectangle(0, 0, Width - 2, Height - 1);
            //Font font = new Font("Arial", 10f / Zoom);
            if (Game.Cells != null)
            for (int y_ = vp.Top; y_ < vp.Bottom; y_++)
                for (int x_ = vp.Left; x_ < vp.Right; x_ += 2)
                {
                    var x = x_ + (y_ & 1);
                    var y = y_;
                    var cell = Game.Cells[y, x];
                    if (cell.GroundTile != null)
                    {
                        var gTile = Game.GroundTilesImages[cell.GroundTile.ImageIndex];
                        gc.DrawImage(gTile, x * sx, y * sy);
                    }
                }
        }


    }
}
