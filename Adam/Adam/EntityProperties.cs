﻿using Adam.Misc;
using Adam.Misc.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Adam
{
    public partial class Entity
    {
        protected ComplexAnimation ComplexAnim = new ComplexAnimation();

        readonly Timer _hitRecentlyTimer = new Timer();
        readonly Timer _deathAnimationTimer = new Timer();
        protected readonly Timer _respawnTimer = new Timer();

        public SoundFxManager Sounds { get; set; }
        /// <summary>
        /// Determines whether this entity should perform collision checks with other collidable objects.
        /// </summary>
        public bool IsCollidable { get; set; } = true;

        /// <summary>
        /// Determines whether this entity should be affected by gravity.
        /// </summary>
        public bool ObeysGravity { get; set; } = true;

        /// <summary>
        /// Determines whether the entity is in the air.
        /// </summary>
        public bool IsJumping { get; set; }

        /// <summary>
        /// Determines if the entity has recently taken damage.
        /// </summary>
        public bool IsTakingDamage
        {
            get; private set;
        }
        /// <summary>
        /// Used in calculations such as how far back the entity is knocked back and friction against ground.
        /// </summary>
        public int Weight { get; set; } = 10;

        private Vector2 _respawnPos;
        protected Vector2 RespawnPos
        {
            get
            {
                if (_respawnPos == Vector2.Zero)
                    _respawnPos = new Vector2(CollRectangle.X, CollRectangle.Y);
                return _respawnPos;
            }
            set { _respawnPos = value; }
        }

    }
}
