﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Adam;
using Microsoft.Xna.Framework.Input;
using Adam.Lights;
using Adam.Misc.Interfaces;
using Adam.Characters.Enemies;
using Adam.Levels;

namespace Adam
{
    public enum ProjectileSource
    {
        Player, Snake,
    }

    public abstract class Projectile : Entity
    {
        public int TileHit;
        protected bool IsInactive;
        public ProjectileSource CurrentProjectileSource;

        protected float Rotation;
        protected bool IsFlipped;
        protected double EffTimer;
        protected GameTime GameTime;
        protected Player.Player Player;
        protected Enemy Enemy;


        public Projectile()
        {

        }

        protected void CreateParticleEffect(GameTime gameTime)
        {
            EffTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (EffTimer > 20 && !IsInactive)
            {
                GameWorld.Instance.Particles.Add(new Particle(this));
                EffTimer = 0;
            }
        }

        public virtual void Update(Player.Player player, GameTime gameTime)
        {
            base.Update();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (CurrentProjectileSource)
            {
                case ProjectileSource.Snake:
                    //animation.Draw(spriteBatch);
                    break;
                case ProjectileSource.Player:
                    spriteBatch.Draw(Texture, CollRectangle, null, Color.White, Rotation, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0);
                    if (Light != null)
                        Light.DrawGlow(spriteBatch);
                    break;
                default:
                    base.Draw(spriteBatch);
                    break;
            }
        }

        public void DrawLights(SpriteBatch spriteBatch)
        {
            switch (CurrentProjectileSource)
            {
                case ProjectileSource.Player:
                    if (Light != null)
                        Light.Draw(spriteBatch);
                    break;
            }

        }

        protected void Destroy()
        {
            if (Light != null)
                GameWorld.Instance.LightEngine.RemoveDynamicLight(Light);
            GameWorld.Instance.Entities.Remove(this);
        }

        public void Animate()
        {
            throw new NotImplementedException();
        }
    }

    public class PlayerWeaponProjectile : Projectile
    {
        public PlayerWeaponProjectile(Player.Player player, ContentManager content)
        { 
        //{
        //    CurrentProjectileSource = ProjectileSource.Player;
        //    this.player = player;
        //    this.Content = Content;
        //    player.weapon.CurrentWeaponType = WeaponType.LaserGun;
        //    switch (player.weapon.CurrentWeaponType)
        //    {
        //        case WeaponType.Stick:
        //            break;
        //        case WeaponType.Bow:
        //            break;
        //        case WeaponType.Sword:
        //            break;
        //        case WeaponType.Shotgun:
        //            break;
        //        case WeaponType.LaserGun:
        //            Texture = ContentHelper.LoadTexture("Projectiles/laser");

        //            //light = new PointLight();
        //            //light.Create(new Vector2(collRectangle.Center.X, collRectangle.Center.Y));
        //            //light.SetColor(Color.Red);

        //            MouseState mouse = Mouse.GetState();
        //            Vector2 center = new Vector2((Main.DefaultResWidth / 2) + (player.GetCollRectangle().Width / 2),
        //                (Main.DefaultResHeight * 3 / 5) + (player.GetCollRectangle().Height / 2));

        //            //Find the unit vector according to where the mouse is
        //            double xDiff = (mouse.X - center.X);
        //            double yDiff = (mouse.Y - center.Y);
        //            double x2 = Math.Pow(xDiff, 2.0);
        //            double y2 = Math.Pow(yDiff, 2.0);
        //            double magnitude = Math.Sqrt(x2 + y2);
        //            double xComp = xDiff / magnitude;
        //            double yComp = yDiff / magnitude;

        //            //arctangent for rotation of proj, also takes into account periodicity
        //            rotation = (float)Math.Atan(yDiff / xDiff);
        //            if (yDiff < 0 && xDiff > 0)
        //                rotation += 3.14f;
        //            if (yDiff > 0 && xDiff > 0)
        //                rotation += 3.14f;

        //            //Multiply unit vectors by max speed
        //            float linearSpeed = 20f;
        //            velocity = new Vector2((float)(linearSpeed * xComp), (float)(linearSpeed * yComp));

        //            player.weapon.CreateBurstEffect(this);
        //            break;
        //        default:
        //            break;
        //    }

        //    if (Texture == null) return;
        //    collRectangle = new Rectangle(player.weapon.rectangle.X + player.weapon.texture.Width, player.weapon.rectangle.Y
        //        + player.weapon.texture.Height / 2, Texture.Width, Texture.Height);

        //    collRectangle = new Rectangle((int)(player.weapon.tipPos.X), (int)(player.weapon.tipPos.Y), Texture.Width, Texture.Height);
        }

        protected override Rectangle DrawRectangle
        {
            get
            {
                return CollRectangle;
            }
        }

        public override void Update(Player.Player player, GameTime gameTime)
        {
            this.GameTime = gameTime;
            CollRectangle.X += (int)Velocity.X;
            CollRectangle.Y += (int)Velocity.Y;

            CreateTrailEffect();
        }


        private void CreateTrailEffect()
        {

        }

    }

    //Only use this with enemies
    public abstract class LinearProjectile : Projectile
    {
        public LinearProjectile()
        {

        }
    }

    public class FlyingWheelProjectile : LinearProjectile
    {
        public FlyingWheelProjectile(int x, int y, int xVel, int yVel)
        {
            Texture = Main.DefaultTexture;
            CollRectangle = new Rectangle(x, y, 16, 16);
            Velocity = new Vector2(xVel, yVel);
            Light = new DynamicPointLight(this, 1, true, Color.MediumPurple, 1);
            GameWorld.Instance.LightEngine.AddDynamicLight(Light);

        }

        protected override Rectangle DrawRectangle
        {
            get
            {
                return CollRectangle;
            }
        }

        public void OnCollisionWithTerrainAnywhere(Entity entity, Tile tile)
        {
            Destroy();
        }

        public override void Update(Player.Player player, GameTime gameTime)
        {
            GameWorld.Instance.Particles.Add(new TrailParticle(this, Color.MediumPurple));
            GameWorld.Instance.Particles.Add(new TrailParticle(this, Color.MediumPurple));

            base.Update(player, gameTime);
        }
    }

    //Only use this with enemies
    public class ParabolicProjectile : Projectile
    {
        public ParabolicProjectile(Enemy enemy, GameWorld map, ProjectileSource currentProjectileSource)
        {
            this.CurrentProjectileSource = currentProjectileSource;
            this.Enemy = enemy;

            switch (currentProjectileSource)
            {
                case ProjectileSource.Snake:
                    Texture = ContentHelper.LoadTexture("Projectiles/venom_dark");
                    CollRectangle = new Rectangle(enemy.GetCollRectangle().X, enemy.GetCollRectangle().Y, 32, 32);
                   // animation = new Animation(Texture, collRectangle, 200, 0, AnimationType.Loop);
                    if (!enemy.IsFacingRight)
                    {
                        Velocity = new Vector2(-10, -15);
                        //animation.isFlipped = true;
                    }
                    else Velocity = new Vector2(10, -15);
                    break;
            }

        }

        protected override Rectangle DrawRectangle
        {
            get
            {
                return CollRectangle;
            }
        }

        public override void Update(Player.Player player, GameTime gameTime)
        {
            this.Player = player;
            this.GameTime = gameTime;

            switch (CurrentProjectileSource)
            {
                case ProjectileSource.Snake:
                    CollRectangle.X += (int)Velocity.X;
                    CollRectangle.Y += (int)Velocity.Y;

                  //  animation.UpdateRectangle(collRectangle);
                   // animation.Update(gameTime);
                    CreateParticleEffect(gameTime);

                    Velocity.Y += .8f;

                    if (Velocity.Y > 10)
                        Velocity.Y = 10f;

                    Velocity.X = Velocity.X * 0.995f;
                    break;
            }

            CheckCollisionWithPlayer();
            CheckIfOutsideBoundaries();

            if (IsInactive)
            {
                ToDelete = true;
            }
        }

        private void CheckCollisionWithPlayer()
        {
            if (Player.GetCollRectangle().Intersects(CollRectangle) && !IsInactive)
            {
                IsInactive = true;
            }
        }

        private void CheckIfOutsideBoundaries()
        {
            if (CollRectangle.Y > GameWorld.Instance.WorldData.LevelHeight * Main.Tilesize)
                IsInactive = true;
        }
    }


}



