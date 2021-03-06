﻿using Adam;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adam.Levels;

namespace Adam.Obstacles
{
    public class Liquid : Obstacle
    {
        double _particleTimer;
        double _restartTime;
        bool _isOnTop;

        public enum Type
        {
            Water, Poison, Lava
        }
        Type _currentType;

        protected override Rectangle DrawRectangle
        {
            get
            {
                return new Rectangle(CollRectangle.X + 8, CollRectangle.Y + 8, 32, 32);
            }
        }

        public Liquid(Tile sourceTile, Type type)
        {
            CollRectangle = new Rectangle(sourceTile.DrawRectangle.X + 8, sourceTile.DrawRectangle.Y + 8, Main.Tilesize / 2, Main.Tilesize / 2);
            _particleTimer = GameWorld.RandGen.Next(0, 8) * GameWorld.RandGen.NextDouble();
            _restartTime = GameWorld.RandGen.Next(5, 8) * GameWorld.RandGen.NextDouble();
            if (_restartTime < 1)
                _restartTime = 1;
            _currentType = type;

            sourceTile.OnTileUpdate += Update;
            sourceTile.OnTileDestroyed += SourceTile_OnTileDestroyed;
        }

        private void SourceTile_OnTileDestroyed(Tile t)
        {
            t.OnTileUpdate -= Update;
            t.OnTileDestroyed -= SourceTile_OnTileDestroyed;
        }

        public void Update(Tile t)
        {
            GameWorld gameWorld = GameWorld.Instance;
            this.Player = GameWorld.Instance.Player;

            if (CollRectangle.Intersects(GameWorld.Instance.Player.GetCollRectangle()))
            {
                Player.TakeDamage(this, Player.MaxHealth);
            }

            //if (!isOnTop)
            //    return;

            if (_currentType == Type.Lava)
            {
                if (GameWorld.Instance.TileArray[GetTileIndex() - GameWorld.Instance.WorldData.LevelWidth].Id == 0)
                {
                    _particleTimer += GameWorld.Instance.GetGameTime().ElapsedGameTime.TotalSeconds;
                    if (_particleTimer > _restartTime)
                    {
                        Particle par = new Particle();
                        par.CreateLavaParticle(this, gameWorld);
                        gameWorld.Particles.Add(par);
                        _particleTimer = 0;
                    }
                }
            }
            

        }

        public void CheckOnTop(Tile[] array, GameWorld gameWorld)
        {
            //this.gameWorld = gameWorld;
            //int indexAbove = TileIndex - gameWorld.worldData.LevelWidth;
            //if (array[indexAbove].ID == 0)
            //{
            //    this.isOnTop = true;
            //}
        }
    }
}
