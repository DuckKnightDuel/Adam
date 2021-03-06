﻿using Adam.Misc.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Adam.Misc;

namespace Adam.Characters.Enemies
{
    class Duck : Enemy, INewtonian
    {

        public Duck(int x, int y)
        {
            Texture = ContentHelper.LoadTexture("Enemies/duck");
            CollRectangle = new Rectangle(x, y, 32, 32);
            SourceRectangle = new Rectangle(0, 0, 16, 16);
            Velocity.X = 1;
        }

        public override void Update()
        {
            if (Velocity.X > 0) IsFacingRight = true;
            else IsFacingRight = false;

            base.Update();
        }

        public float GravityStrength { get; set; } = Main.Gravity;

        public bool IsAboveTile { get; set; }

        public bool IsFlying { get; set; }

        public override byte Id
        {
            get
            {
                return 208;
            }
        }

        public override int MaxHealth
        {
            get
            {
                return EnemyDb.DuckMaxHealth;
            }
        }

        protected override SoundFx MeanSound
        {
            get
            {
                return null;
            }
        }

        protected override SoundFx AttackSound
        {
            get
            {
                return null;
            }
        }

        protected override SoundFx DeathSound
        {
            get
            {
                return null;
            }
        }

        protected override Rectangle DrawRectangle
        {
            get
            {
                return CollRectangle;
            }
        }

        public void OnCollisionWithTerrainLeft(Entity entity, Tile tile)
        {
            Velocity.X = -Velocity.X;
        }

        public void OnCollisionWithTerrainRight(Entity entity, Tile tile)
        {
            Velocity.X = -Velocity.X;  
        }
    }
}
