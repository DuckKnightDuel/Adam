﻿using Adam;
using Adam.Characters.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adam.Misc;
using Adam.Misc.Interfaces;

namespace Adam.Enemies
{
    public class Snake : Enemy, IAnimated
    {
        double projCooldownTimer;
        Vector2 frameCount;

        public override byte ID
        {
            get
            {
                return 201;
            }
        }

        protected override int MaxHealth
        {
            get
            {
                return EnemyDB.Snake_MaxHealth;
            }
        }

        SoundFx meanSound;
        protected override SoundFx MeanSound
        {
            get
            {
                if (meanSound == null)
                    meanSound = new SoundFx("Sounds/Snake/mean");
                return meanSound;
            }
        }

        SoundFx attackSound;
        protected override SoundFx AttackSound
        {
            get
            {
                if (attackSound == null)
                    attackSound = new SoundFx("Sounds/Snake/attack");
                return attackSound;
            }
        }

        SoundFx deathSound;
        protected override SoundFx DeathSound
        {
            get
            {
                if (deathSound == null)
                    deathSound = new SoundFx("Sounds/Snake/death");
                return deathSound;
            }
        }

        Animation animation;
        public Animation Animation
        {
            get
            {
                if (animation == null)
                    animation = new Animation(Texture, drawRectangle, sourceRectangle);
                return animation;
            }
        }

        AnimationData[] animationData;
        public AnimationData[] AnimationData
        {
            get
            {
                if (animationData == null)
                    animationData = new Adam.AnimationData[]
                    {
                        new Adam.AnimationData(250,4,0,AnimationType.Loop),
                    };
                return animationData;
            }
        }

        public AnimationState CurrentAnimationState
        {
            get; set;
        }

        public Snake(int x, int y)
        {
            //Sets up specific variables for the snake
            frameCount = new Vector2(8, 0);
            sourceRectangle = new Rectangle(0, 0, 64, 96);
            drawRectangle = new Rectangle(x, y - 64, 64, 96);

            //Textures and sound effects, single is for rectangle pieces explosion
            Texture = Content.Load<Texture2D>("Enemies/Snake");

            //Creates animation
            animation = new Animation(Texture, drawRectangle, 240, 0, AnimationType.Loop);
        }

        public override void Update()
        {
            base.Update();

            collRectangle = new Rectangle(drawRectangle.X + 8, drawRectangle.Y + 12, drawRectangle.Width - 16, drawRectangle.Height - 12);

            if (projCooldownTimer > 3 && !isDead)
            {
                if (GameWorld.RandGen.Next(0, 1000) < 50)
                {
                    GameWorld.Instance.entities.Add(new ParabolicProjectile(this, GameWorld.Instance, Content, ProjectileSource.Snake));
                    PlayAttackSound();
                    projCooldownTimer = 0;
                }
            }
            projCooldownTimer += GameWorld.Instance.GetGameTime().ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (isDead)
                return;

            if (IsPlayerToTheRight())
                animation.isFlipped = true;
            else animation.isFlipped = false;

            animation.Draw(spriteBatch);
        }

        public void Animate()
        {
            throw new NotImplementedException();
        }
    }
}
