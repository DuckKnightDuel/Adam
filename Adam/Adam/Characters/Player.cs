﻿using Adam;
using Adam.Characters.Enemies;
using Adam.Interactables;
using Adam.Misc;
using Adam.Misc.Interfaces;
using Adam.Obstacles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adam
{
    public partial class Player : Entity, ICollidable, INewtonian
    {
        public delegate void PlayerRespawnHandler();
        public event PlayerRespawnHandler PlayerRespawned;

        #region Variables
        Main game1;
        public Rectangle attackBox;

        Jetpack jetpack = new Jetpack();
        ComplexAnimation complexAnim = new ComplexAnimation();
        SoundEffect jumpSound, takeDamageSound, attackSound, gameOverSound, tadaSound, fallSound;
        SoundEffect chronoActivateSound;
        public SoundFx chronoDeactivateSound;
        SoundEffect[] walkSounds, runSounds;
        SoundEffect[] sounds;
        SoundEffect[] goreSounds;
        SoundFx levelFail;
        SoundFx climb1;
        SoundFx climb2;
        SoundFx jumpedOnEnemySound;

        float blackScreenOpacity;
        float deltaTime;

        public const int MAX_LEVEL = 30;

        public Vector2 respawnPos;

        public List<int> keySecrets = new List<int>();

        //Timers
        GameTime gameTime;
        double zzzTimer, frameTimer, fallingTimer, invincibleTimer, offGroundTimer, controlTimer, sleepTimer;
        double respawnTimer;
        double chronoSoundTimer;
        double tileParticleTimer;
        double movementSoundTimer;

        //Booleans
        public bool isJumping;
        public bool isInvincible;
        public bool isSpaceBarPressed;
        public bool automatic_hasControl = true;
        public bool hasFiredWeapon;
        public bool isOnVines;
        public bool grabbedVine;
        public bool manual_hasControl = true;

        public bool isFlying;
        public bool returnToMainMenu;
        public bool isChronoshifting;
        public bool hasChronoshifted;
        public bool hasJetpack;
        public bool canLift;
        public bool hasFireResistance;
        public bool canActivateShield;
        public bool canBecomeInvisible;
        public bool canSeeInDark;
        public bool isInvisible;
        public bool isRunningFast;
        public bool isWaitingForRespawn;
        public bool hasStoppedTime;

        double timeStopTimer;

        bool gameOverSoundPlayed;
        bool deathAnimationDone;
        bool hasDeactiveSoundPlayed;
        bool hasStomped;
        bool fallSoundPlayed;
        bool goreSoundPlayed;

        //Debug
        public bool canFly;
        public bool isInvulnerable;
        public bool isGhost;

        //Player stats
        private int score;
        public int Score
        {
            get
            {
                try
                {
                    return game1.GameData.CurrentSave.PlayerStats.Score;
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                game1.GameData.CurrentSave.PlayerStats.Score = value;
            }
        }

        public override int MaxHealth
        {
            get
            {
                return 100;
            }
        }

        public int armorPoints = 100;

        //Animation Variables
        int switchFrame;
        int currentFrame;
        Vector2 frameCount;
        #endregion

        public Player(Main game1)
        {
            this.game1 = game1;

            Texture2D edenTexture = ContentHelper.LoadTexture("adam_eden");

            complexAnim.AddAnimationData("idle", new ComplexAnimData(0, edenTexture, new Rectangle(0, 0, 0, 0), 0, 0, 0, 0, 0, true));




            Initialize(0, 0);
            Load();
        }

        /// <summary>
        /// This method will set the player's positions to those specified. It should be used when the map is changed.
        /// </summary>
        /// <param name="setX"> The x-Coordinate</param>
        /// <param name="setY"> The y-Coordinate</param>
        public void Initialize(int setX, int setY)
        {
            //Set the player position according to where in the map he is supposed to be
            collRectangle.X = setX;
            collRectangle.Y = setY;
            respawnPos = new Vector2(setX, setY);

            //Animation information
            frameCount = new Vector2(4, 0);
            collRectangle.Width = 32;
            collRectangle.Height = 64;
            sourceRectangle = new Rectangle(0, 0, 24, 40);
        }

        /// <summary>
        /// Loads all of the player assets.
        /// </summary> 
        public void Load()
        {
            //Use this string to save the path to a file if multiple files begin with the same location.
            string path;

            tadaSound = ContentHelper.LoadSound("Sounds/levelup");

            //Load sound effects
            chronoActivateSound = ContentHelper.LoadSound("Sounds/chronoshift_activate");
            chronoDeactivateSound = new SoundFx("Sounds/chronoshift_deactivate");
            jumpSound = ContentHelper.LoadSound("Sounds/JumpSound");
            takeDamageSound = ContentHelper.LoadSound("Sounds/PlayerHit");
            attackSound = ContentHelper.LoadSound("Sounds/laserunNew");
            gameOverSound = ContentHelper.LoadSound("Sounds/DeathSound");
            fallSound = ContentHelper.LoadSound("Sounds/adam_fall");

            //Movement sounds
            path = "Sounds/Movement/";
            walkSounds = new SoundEffect[]
            {
                ContentHelper.LoadSound(path+"walk1"),
                ContentHelper.LoadSound(path+"walk2"),
                ContentHelper.LoadSound(path+"walk3"),
            };

            //Eventually all sounds will be in one array.
            sounds = new SoundEffect[]
            {
                ContentHelper.LoadSound("Sounds/Chronoshift/startSound"),
                ContentHelper.LoadSound("Sounds/Chronoshift/stopSound"),
                ContentHelper.LoadSound("Sounds/Movement/walk2"),
            };

            path = "Sounds/Gore/";
            goreSounds = new SoundEffect[]
            {
                ContentHelper.LoadSound(path+"gore1"),
                ContentHelper.LoadSound(path+"gore2"),
                ContentHelper.LoadSound(path+"gore3"),
            };

            levelFail = new SoundFx("Sounds/Menu/level_fail");
            climb1 = new SoundFx("Sounds/Player/climbing1");
            climb2 = new SoundFx("Sounds/Player/climbing2");
        }

        /// <summary>
        /// Update player information, checks for collision and input, and many other things.
        /// </summary>
        /// <param name="gameTime"></param>
        /// 
        public void Update(GameTime gameTime)
        {
            if (GameWorld.Instance.CurrentGameMode == GameMode.Edit)
            {
                ContainInGameWorld();
                return;

            }

            this.gameTime = gameTime;

            deltaTime = (float)(60 * gameTime.ElapsedGameTime.TotalSeconds);

            //Update Method is spread out!
            //Check the following things

            CheckDead();


            if (IsDead())
            {
                UpdateTimers();
                return;
            }

            UpdateStats();
            UpdateInput();
            UpdateTimers();
            UpdatePlayerPosition();
            if (!isGhost)
                base.Update();
            CreateWalkingParticles();
            Animate();
            SetEvolutionAttributes();

            jetpack.Update(this, gameTime);

            //If the player is falling really fast, he is not jumping anymore and is falling.
            if (velocity.Y > 7)
            {
                fallingTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (velocity.Y >= 8 && fallingTimer > 500 && !isOnVines)
                {
                    CurrentAnimation = AnimationState.Falling;
                    fallingTimer = 0;
                    sourceRectangle.X = 0;
                    currentFrame = 0;
                    sleepTimer = 0;
                }
            }

            //If the player stays idle for too long, then he will start sleeping.
            sleepTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (sleepTimer > 3 && !isOnVines)
                CurrentAnimation = AnimationState.Sleeping;

            //For debugging chronoshift.
            if (InputHelper.IsKeyDown(Keys.Q) && !isChronoshifting)
            {
                //Chronoshift(Evolution.Modern);
                //isChronoshifting = true;
            }

            if (InputHelper.IsKeyDown(Keys.K))
            {
                velocity.X = 20;
                sourceRectangle.Width = 48;
                frameCount.X = 0;
                currentFrame = 0;
                CurrentAnimation = AnimationState.Dashing;
                isDashing = true;
            }
        }

        /// <summary>
        /// This method will check for input and update the player's position accordingly.
        /// </summary>
        private void UpdateInput()
        {
            if (Main.IsLoadingContent)
                return;

            //Check if player is currently on top of vines
            if (TileIndex >= 0 && TileIndex < GameWorld.Instance.tileArray.Length)
                if (GameWorld.Instance.tileArray[TileIndex].isClimbable)
                    isOnVines = true;
                else isOnVines = false;


            //These variables define how fast the player will accelerate based on whether he is walking or runnning.
            //There is no need to put a max limit because after a certain speed, the friction is neough to maintain a const. vel.
            float walkingAcc = .25f;
            float runningAcc = .4f;
            float acceleration = .5f;
            float deceleration = .95f;
            float jumpSpeed = 8f;

            //Check to see if player is running fast
            if (InputHelper.IsKeyDown(Keys.LeftShift))
            {
                acceleration = runningAcc;
            }
            else acceleration = walkingAcc;


            //Check if the player is moving Left
            if (Keyboard.GetState().IsKeyDown(Keys.A) && automatic_hasControl == true && manual_hasControl)
            {
                velocity.X -= acceleration;
                IsFacingRight = false;
                sleepTimer = 0;
                if (!isJumping)
                    CurrentAnimation = AnimationState.Walking;
                if (canFly)
                    velocity.X = -10f;
            }

            //Check if the player is moving right
            if (Keyboard.GetState().IsKeyDown(Keys.D) && automatic_hasControl == true && manual_hasControl)
            {

                velocity.X += acceleration;
                IsFacingRight = true;
                sleepTimer = 0;
                if (!isJumping)
                    CurrentAnimation = AnimationState.Walking;
                if (canFly)
                    velocity.X = 10f;
            }

            //For when he is on OP mode the player can fly up.
            if (Keyboard.GetState().IsKeyDown(Keys.W) && automatic_hasControl == true && manual_hasControl && canFly)
            {
                velocity.Y = -jumpSpeed;
                sleepTimer = 0;
                CurrentAnimation = AnimationState.Jumping;
            }

            //For when he is on op mode the player can fly down.
            if (Keyboard.GetState().IsKeyDown(Keys.S) && automatic_hasControl == true && manual_hasControl && canFly)
            {
                velocity.Y = jumpSpeed;
                sleepTimer = 0;
            }

            //Slows down y velocity if he is on op mode
            if (canFly)
            {
                velocity.Y = velocity.Y * 0.9f * (float)(60 * gameTime.ElapsedGameTime.TotalSeconds);
            }

            //Check if the player is climbing a vine
            if (isOnVines)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.W) && manual_hasControl)
                {
                    velocity.Y = -5f;
                    grabbedVine = true;
                    CurrentAnimation = AnimationState.Climbing;
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.S) && manual_hasControl)
                {
                    velocity.Y = 5f;
                    grabbedVine = true;
                    CurrentAnimation = AnimationState.Climbing;
                }
                else if (grabbedVine)
                {
                    velocity.Y = 0;
                    CurrentAnimation = AnimationState.Climbing;
                }
            }
            else grabbedVine = false;

            //Check if player is jumping
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && isSpaceBarPressed == false && automatic_hasControl == true && isJumping == false && manual_hasControl)
            {
                Jump();
            }

            //If player stops moving, reduce his speed gradually
            //velocity.X -= velocity.X * .1f * deltaTime;
            velocity.X *= deceleration;

            //If his speed goes below a certain point, just make it zero
            if (Math.Abs(velocity.X) < .1f)
            {
                velocity.X = 0;
                PlayMovementSounds();
            }

            //If the player is not doing anything, change his Animation State
            if (velocity.X == 0 && isJumping == false && CurrentAnimation != AnimationState.Sleeping && CurrentAnimation != AnimationState.Climbing)
                CurrentAnimation = AnimationState.Still;

            //Check if the spacebar is pressed so that high jump mechanics can be activated
            if (Keyboard.GetState().IsKeyUp(Keys.Space))
                isSpaceBarPressed = false;

            //if the player is flying, start timing the time his is off ground for the high jump mechanic. If he is not, reset the timer.
            if (isJumping)
                offGroundTimer += gameTime.ElapsedGameTime.TotalSeconds;
            else
            {
                offGroundTimer = 0;
                if (CurrentAnimation == AnimationState.Jumping || CurrentAnimation == AnimationState.Falling)
                    CurrentAnimation = AnimationState.Still;
            }



            //if the player falls off a ledge without jumping, do not allow him to jump, but give him some room to jump if he is fast enough.
            if (velocity.Y > 2f && !grabbedVine)
                isJumping = true;

            //If the player is falling from a ledge, start the jump animation
            if (velocity.Y > 2 && (CurrentAnimation != AnimationState.Jumping && CurrentAnimation != AnimationState.Falling) && !grabbedVine)
                CurrentAnimation = AnimationState.Jumping;

        }

        /// <summary>
        /// This method updates all of the rectangles and applies velocity.
        /// </summary>
        private void UpdatePlayerPosition()
        {
            ContainInGameWorld();

            //Attack box for killing enemies
            attackBox = new Rectangle(collRectangle.X - 8, collRectangle.Y + collRectangle.Height - 10, collRectangle.Width + 16, 20);
        }

        private void ContainInGameWorld()
        {
            GameWorld gameWorld = GameWorld.Instance;
            if (collRectangle.X < 0)
                collRectangle.X = 0;
            if (collRectangle.X > (gameWorld.worldData.LevelWidth * Main.Tilesize - collRectangle.Width))
                collRectangle.X = (gameWorld.worldData.LevelWidth * Main.Tilesize - collRectangle.Width);
            if (collRectangle.Y < 0)
                collRectangle.Y = 0;
            if (collRectangle.Y > (gameWorld.worldData.LevelHeight * Main.Tilesize - collRectangle.Width) + 100)
            {
                if (gameWorld.CurrentGameMode == GameMode.Edit)
                    collRectangle.Y = gameWorld.worldData.LevelHeight * Main.Tilesize - collRectangle.Height;
                else
                {
                    KillAndRespawn();
                    if (!fallSoundPlayed)
                    {
                        fallSound.Play();
                        fallSoundPlayed = true;
                    }
                }
            }
        }
        

        public void UpdateStats()
        {
        }

        //private void Animate()
        //{
        //    currentFrame = sourceRectangle.X / sourceRectangle.Width;

        //    if (currentFrame >= frameCount.X)
        //    {
        //        currentFrame = 0;
        //        sourceRectangle.X = 0;
        //    }

        //    switch (CurrentAnimation)
        //    {
        //        #region Still Animation
        //        case AnimationState.Still:
        //            //define where in the spritesheet the still sequence is
        //            sourceRectangle.Y = 0;
        //            //defines the speed of the animation
        //            switchFrame = 500;
        //            //starts timer
        //            frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

        //            //if the time is up, moves on to the next frame
        //            if (frameTimer >= switchFrame)
        //            {
        //                if (frameCount.X != 0)
        //                {
        //                    frameTimer = 0;
        //                    sourceRectangle.X += sourceRectangle.Width;
        //                    currentFrame++;
        //                }
        //            }

        //            if (currentFrame >= frameCount.X)
        //            {
        //                currentFrame = 0;
        //                sourceRectangle.X = 0;
        //            }
        //            break;
        //        #endregion
        //        #region Walking Animation
        //        case AnimationState.Walking:
        //            //define where in the spritesheet the still sequence is
        //            sourceRectangle.Y = sourceRectangle.Height;
        //            //defines the speed of the animation
        //            if (velocity.X == 0)
        //                switchFrame = 0;
        //            else switchFrame = (int)Math.Abs(400 / velocity.X);
        //            //starts timer
        //            frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

        //            //if the time is up, moves on to the next frame
        //            if (frameTimer >= switchFrame)
        //            {
        //                if (frameCount.X != 0)
        //                {
        //                    frameTimer = 0;
        //                    sourceRectangle.X += sourceRectangle.Width;
        //                    currentFrame++;
        //                }
        //            }

        //            if (currentFrame == 0 || currentFrame == 2)
        //            {
        //                PlayMovementSounds();
        //            }

        //            if (currentFrame >= frameCount.X)
        //            {
        //                currentFrame = 0;
        //                sourceRectangle.X = 0;
        //            }

        //            break;
        //        #endregion
        //        #region Jump Animation
        //        case AnimationState.Jumping:

        //            //define where in the spritesheet the still sequence is
        //            sourceRectangle.Y = sourceRectangle.Height * 2;
        //            sourceRectangle.X = 0;
        //            currentFrame = 0;
        //            if (velocity.Y > -6)
        //            {
        //                sourceRectangle.X = sourceRectangle.Width;
        //                currentFrame = 1;

        //                if (velocity.Y > -2)
        //                {
        //                    sourceRectangle.X = sourceRectangle.Width * 2;
        //                    currentFrame = 2;

        //                    if (velocity.Y > 2)
        //                        sourceRectangle.X = sourceRectangle.Width * 3;
        //                }
        //            }

        //            break;
        //        #endregion
        //        #region Falling Animation
        //        case AnimationState.Falling:
        //            //define where in the spritesheet the still sequence is
        //            sourceRectangle.Y = sourceRectangle.Height * 3;
        //            //defines the speed of the animation
        //            switchFrame = 100;
        //            //starts timer
        //            frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

        //            //if the time is up, moves on to the next frame
        //            if (frameTimer >= switchFrame)
        //            {
        //                if (frameCount.X != 0)
        //                {
        //                    frameTimer = 0;
        //                    sourceRectangle.X += sourceRectangle.Width;
        //                    currentFrame++;
        //                }
        //            }

        //            if (currentFrame >= frameCount.X)
        //            {
        //                currentFrame = 0;
        //                sourceRectangle.X = 0;
        //            }
        //            break;
        //        #endregion
        //        #region Jump and Walking Animation
        //        case AnimationState.JumpWalking:
        //            if (velocity.X == 0)
        //            {
        //                CurrentAnimation = AnimationState.Jumping;
        //                break;
        //            }
        //            //define where in the spritesheet the still sequence is
        //            sourceRectangle.Y = sourceRectangle.Height * 4;
        //            sourceRectangle.X = 0;
        //            currentFrame = 0;
        //            if (velocity.Y > -4)
        //            {
        //                sourceRectangle.X = sourceRectangle.Width;
        //                currentFrame = 1;

        //                if (velocity.Y > 7)
        //                {
        //                    sourceRectangle.X = sourceRectangle.Width * 2;
        //                    currentFrame = 2;
        //                    //fallingTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

        //                    //if (velocity.Y >= 8 && fallingTimer > 500)
        //                    //{
        //                    //    CurrentAnimation = AnimationState.falling;
        //                    //    fallingTimer = 0;
        //                    //    sourceRectangle.X = 0;
        //                    //    currentFrame = 0;
        //                    //}
        //                }
        //            }
        //            break;
        //        #endregion
        //        #region Sleeping Animation
        //        case AnimationState.Sleeping:
        //            //define where in the spritesheet the still sequence is
        //            sourceRectangle.Y = sourceRectangle.Height * 5;
        //            //defines the speed of the animation
        //            switchFrame = 600;
        //            //starts timer
        //            frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

        //            //if the time is up, moves on to the next frame
        //            if (frameTimer >= switchFrame)
        //            {
        //                if (frameCount.X != 0)
        //                {
        //                    frameTimer = 0;
        //                    sourceRectangle.X += sourceRectangle.Width;
        //                    currentFrame++;
        //                }
        //            }

        //            if (currentFrame >= frameCount.X)
        //            {
        //                currentFrame = 0;
        //                sourceRectangle.X = 0;
        //            }

        //            zzzTimer += gameTime.ElapsedGameTime.TotalSeconds;
        //            if (zzzTimer > 1)
        //            {
        //                GameWorld.Instance.particles.Add(new Particle(this));
        //                zzzTimer = 0;
        //            }

        //            break;
        //        #endregion
        //        #region Climbing Animation
        //        case AnimationState.Climbing:
        //            //define where in the spritesheet the still sequence is
        //            sourceRectangle.Y = sourceRectangle.Height * 4;
        //            //defines the speed of the animation
        //            switchFrame = 250;
        //            //starts timer
        //            frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

        //            //if the time is up, moves on to the next frame
        //            if (frameTimer >= switchFrame && Math.Abs((double)(velocity.Y)) > 2)
        //            {
        //                if (frameCount.X != 0)
        //                {
        //                    frameTimer = 0;
        //                    sourceRectangle.X += sourceRectangle.Width;
        //                    currentFrame++;
        //                }
        //            }

        //            if (currentFrame >= frameCount.X)
        //            {
        //                currentFrame = 0;
        //                sourceRectangle.X = 0;
        //            }

        //            if (currentFrame == 0 || currentFrame == 2)
        //            {
        //                climb1.PlayNewInstanceOnce();
        //                climb2.Reset();
        //            }
        //            if (currentFrame == 1 || currentFrame == 3)
        //            {
        //                climb2.PlayNewInstanceOnce();
        //                climb1.Reset();
        //            }

        //            break;
        //        #endregion

        //        case AnimationState.Dashing:
        //            currentTexture = dashingTexture;
        //            sourceRectangle.Y = 0;
        //            switchFrame = 250;
        //            //starts timer
        //            frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

        //            if (frameTimer >= switchFrame)
        //            {
        //                if (frameCount.X != 0 && currentFrame == 0)
        //                {
        //                    sourceRectangle.X += sourceRectangle.Width;
        //                    currentFrame++;
        //                }
        //            }

        //            if (velocity.X < 1)
        //            {
        //                currentFrame = 2;
        //                sourceRectangle.X += sourceRectangle.Width ;
        //            }


        //            break;
        //    }


        //    if (currentFrame >= frameCount.X)
        //    {
        //        currentFrame = 0;
        //        sourceRectangle.X = 0;
        //    }
        //}

        private void Animate()
        {

        }

        private void CheckDead()
        {
            //If his health falls below 0, kill him.
            if (IsDead() && !isWaitingForRespawn)
            {
                KillAndRespawn();
            }
        }

        private void UpdateTimers()
        {
            //If player became invincible for some reason, make him vincible again after the timer runs out.
            if (isInvincible)
            {
                invincibleTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (invincibleTimer > 2)
                {
                    isInvincible = false;
                    invincibleTimer = 0;
                }
            }

            //If player stopped taking damage and is back on the ground give him control of his character back
            if (!IsDead())
            {
                controlTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (controlTimer > .5)
                {
                    automatic_hasControl = true;
                    controlTimer = 0;
                }
            }

            //Check to see if the player is waiting to respawn
            if (isWaitingForRespawn)
            {
                respawnTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (respawnTimer > 1250)
                {
                    respawnTimer = 0;
                    Revive();
                }
            }

            if (InputHelper.IsKeyDown(Keys.I))
            {
                // hasStoppedTime = true;
            }

            if (hasStoppedTime)
            {
                TrailParticle particle = new TrailParticle(this, Color.White);
                GameWorld.Instance.particles.Add(particle);


                timeStopTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (timeStopTimer > 5)
                {
                    timeStopTimer = 0;
                    hasStoppedTime = false;
                }
            }

            // Checks to see if player is on fire and deals damage accordingly.
            if (IsOnFire)
            {
                onFireTimer.Increment();
                if (onFireTimer.TimeElapsedInSeconds < 4)
                {
                    fireTickTimer.Increment();
                    fireSpawnTimer.Increment();
                    if (fireTickTimer.TimeElapsedInMilliSeconds > 500)
                    {
                        TakeDPS(EnemyDB.FlameSpitter_DPS);
                        fireTickTimer.Reset();
                    }
                    if (fireSpawnTimer.TimeElapsedInMilliSeconds > 100)
                    {
                        EntityFlameParticle flame = new EntityFlameParticle(this, Color.Yellow);
                        EntityFlameParticle flame2 = new EntityFlameParticle(this, Color.Red);
                        GameWorld.Instance.particles.Add(flame);
                        GameWorld.Instance.particles.Add(flame2);
                        fireSpawnTimer.Reset();
                    }
                }
                else
                {
                    IsOnFire = false;
                    onFireTimer.Reset();
                    fireTickTimer.Reset();
                }

            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (GameWorld.Instance.CurrentGameMode == GameMode.Edit) return;
            //DrawSurroundIndexes(spriteBatch);

            if (IsDead()) return;

            jetpack.Draw(spriteBatch);

            if (isChronoshifting) return;

            if (IsFacingRight == true)
                spriteBatch.Draw(currentTexture, DrawRectangle, sourceRectangle, Color.White);
            else spriteBatch.Draw(currentTexture, DrawRectangle, sourceRectangle, Color.White, 0, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0);
        }

        public void TakeDamage(int damage)
        {
            if (isInvulnerable)
                return;
            if (isInvincible)
                return;

            for (int i = 0; i < damage; i++)
            {
                Particle par = new Particle();
                par.CreateTookDamage(this);
                GameWorld.Instance.particles.Add(par);
            }

            Health -= damage;
            //make him invincible for a while and start the being hit animation
            isInvincible = true;
            automatic_hasControl = false;
            takeDamageSound.Play();
            SpillBlood(GameWorld.RandGen.Next(3, 5));
        }

        /// <summary>
        /// Player takes damage without becoming invincible and without spilling blood.
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDPS(int damage)
        {
            Health -= damage;
            takeDamageSound.Play();
        }

        public void TakeDamageAndKnockBack(int damage)
        {
            if (isInvulnerable)
                return;
            if (isInvincible)
                return;

            TakeDamage(damage);

            if (velocity.X > 0)
            {
                velocity.X = -10f;
                velocity.Y = -5;
                isJumping = true;
            }
            else
            {
                velocity.X = 10f;
                velocity.Y = -5;
                isJumping = true;
            }
        }

        public override void Revive()
        {
            if (PlayerRespawned != null)
                PlayerRespawned();

            Console.WriteLine("Player began to revive.");

            Overlay.Instance.FadeIn();

            //reset player velocity
            velocity = new Vector2(0, 0);
            //Take him back to the spawn point
            collRectangle.X = (int)respawnPos.X;
            collRectangle.Y = (int)respawnPos.Y;
            //Reset the death animation sequence so that the player can die again
            deathAnimationDone = false;
            gameOverSoundPlayed = false;
            fallSoundPlayed = false;
            goreSoundPlayed = false;
            manual_hasControl = true;
            isInvisible = false;
            isWaitingForRespawn = false;

            Console.WriteLine("All variables were reset.");
            Console.WriteLine("Reseting world.");
            GameWorld.Instance.ResetWorld();
            Console.WriteLine("World reset.");
            base.Revive();
            Console.WriteLine("Player health set back to max health.");
        }

        public void PlayAttackSound()
        {
            attackSound.Play();
        }

        public void PlayGameOverSound()
        {
            if (gameOverSoundPlayed == false)
            {
                gameOverSound.Play();
                gameOverSoundPlayed = true;
            }
        }

        private void CreateWalkingParticles()
        {
            //Creates little particles that have a texture according to the block below the player
            tileParticleTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (velocity.X == 0)
                return;
            //Particles move faster if player is moving faster, and there is a higher chance of spawning
            //particles if the player is moving faster.
            if (tileParticleTimer < Math.Abs(350 / velocity.X))
                return;
            Tile tile = new Tile();
            int tileIndexBelow = GetTileIndex(new Vector2(collRectangle.X, collRectangle.Y)) + (GameWorld.Instance.worldData.LevelWidth * 2);
            if (tileIndexBelow < GameWorld.Instance.tileArray.Length)
                tile = GameWorld.Instance.tileArray[tileIndexBelow];
            //If the player is above air skip.
            if (tile.ID == 0)
                return;
            Particle eff = new Particle();
            eff.CreateTileParticleEffect(tile, this);
            GameWorld.Instance.particles.Add(eff);
            tileParticleTimer = 0;
        }

        public void PlayMovementSounds()
        {
            if (velocity.X == 0)
                return;
            if (isJumping)
                return;

            movementSoundTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

            Tile tile = new Tile();
            tile = GameWorld.Instance.tileArray[TileIndex + (GameWorld.Instance.worldData.LevelWidth * 2)];

            if (tile.ID != 0 && movementSoundTimer > Math.Abs(500 / velocity.X))
            {
                //if (isRunningFast)
                //    runSounds[Map.randGen.Next(0,runSounds.Length)].Play();
                //else 
                SoundEffectInstance s = walkSounds[0].CreateInstance();
                s.Pitch = 1;
                s.Play();

                movementSoundTimer = 0;
            }
        }

        private void CreateJumpParticles()
        {
            for (int i = 0; i < 20; i++)
            {
                GameWorld.Instance.particles.Add(new JumpSmokeParticle(this));
            }
        }

        private void CreateStompParticles()
        {
            for (int i = 0; i < 20; i++)
            {
                GameWorld.Instance.particles.Add(new StompSmokeParticle(this));
            }
        }

        public void Jump()
        {
            //Make his velocity increase once
            velocity.Y = -8f;
            //Move him away from the tiles so collision does not stop the jump
            collRectangle.Y -= 1;
            //Make him unable to jump again
            isJumping = true;
            //Make this code not be repeated
            isSpaceBarPressed = true;
            //Play the sound
            jumpSound.Play();
            //Stop him from sleeping
            sleepTimer = 0;
            //Change the animation
            CurrentAnimation = AnimationState.Jumping;
            CreateJumpParticles();
            hasStomped = false;
        }

        public void KillAndRespawn()
        {
            TakeDamage(Health);

            if (isWaitingForRespawn)
                return;

            for (int i = 0; i < 10; i++)
            {
                Particle par = new Particle();
                par.CreateDeathSmoke(this);
                GameWorld.Instance.particles.Add(par);
            }

            int rand = GameWorld.RandGen.Next(20, 30);
            SpillBlood(rand);
            TakeDamageAndKnockBack(Health);
            manual_hasControl = false;
            isWaitingForRespawn = true;
            levelFail.PlayIfStopped();
        }

        public void Stomp()
        {
            if (!hasStomped)
            {
                sounds[2].Play();
                CreateStompParticles();
                hasStomped = true;
            }
        }

        public void PlayGoreSound()
        {
            if (!goreSoundPlayed)
            {
                int rand = GameWorld.RandGen.Next(0, 2);
                goreSounds[rand].Play();
                goreSoundPlayed = true;
            }
        }

        public void PlayTakeDamageSound()
        {
            takeDamageSound.Play();
        }

        public void SpillBlood(int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                Particle par = new Particle();
                par.CreateBloodEffect(this, GameWorld.Instance);
                GameWorld.Instance.particles.Add(par);
            }
        }

        public void Heal(int amount)
        {
            //TODO add sounds.
            Health += amount;
            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        public void DealDamage(Enemy enemy)
        {
            Jump();
            enemy.TakeDamage(20);
            jumpedOnEnemySound.PlayNewInstanceOnce();
            jumpedOnEnemySound.Reset();
        }


        void ICollidable.OnCollisionWithTerrainAbove(TerrainCollisionEventArgs e)
        {
            velocity.Y = 0f;
        }

        void ICollidable.OnCollisionWithTerrainBelow(TerrainCollisionEventArgs e)
        {
            velocity.Y = 0f;
            isJumping = false;
            isFlying = false;
            Stomp();
        }

        void ICollidable.OnCollisionWithTerrainRight(TerrainCollisionEventArgs e)
        {
            if (Math.Abs(velocity.Y) < 1)
                CurrentAnimation = AnimationState.Still;
            velocity.X = 0;
        }

        void ICollidable.OnCollisionWithTerrainLeft(TerrainCollisionEventArgs e)
        {
            if (Math.Abs(velocity.Y) < 1)
                CurrentAnimation = AnimationState.Still;
            velocity.X = 0;
        }

        public void OnCollisionWithTerrainAnywhere(TerrainCollisionEventArgs e)
        {
        }

        public float GravityStrength
        {
            get
            {
                float gravity = 0;
                if (!canFly)
                {
                    gravity = .2f;
                    if (InputHelper.IsKeyDown(Keys.Space) && offGroundTimer < .3f)
                    { }
                    else gravity = .5f;
                }
                return gravity;
            }

            set
            {
                GravityStrength = value;
            }
        }


        public bool IsFlying
        {
            get
            {
                return isFlying;
            }
            set
            {
                isFlying = value;
            }
        }

        public bool IsJumping
        {
            get
            {
                return isJumping;
            }
            set
            {
                isJumping = value;
            }
        }

        public bool IsAboveTile { get; set; }

        protected override Rectangle DrawRectangle
        {
            get
            {
                int width;
                if (!isDashing) width = 48; else width = 96;
                return new Rectangle(collRectangle.X - 8, collRectangle.Y - 16, width, 80);
            }
        }
    }

}
