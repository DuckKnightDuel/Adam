﻿using Adam;
using Adam.Interactables;
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
    enum Evolution
    {
        Eden,
        Prehistoric,
        Ancient,
        Medieval,
        Renaissance,
        Modern,
        Future,
        God,
        InProgress,
    }

    class Player : Entity
    {
        //temp
        int count;

        public delegate void PlayerRespawnHandler();
        public event PlayerRespawnHandler PlayerRespawned;

        #region Variables
        public Texture2D currentTexture;
        public Texture2D previousSingleTexture;
        public Texture2D newSingleTexture;
        Texture2D blackScreen;
        Texture2D maxTexture;
        Texture2D[] textureArray;
        Texture2D[] singleTextureArray;

        public Weapon weapon;

        public Vector2 previousPosition;

        public Rectangle attackBox;
        public Rectangle topMidBound, botMidBound;

        Jetpack jetpack = new Jetpack();

        SoundEffect jumpSound, takeDamageSound, attackSound, gameOverSound, tadaSound, fallSound;
        SoundEffect chronoActivateSound, chronoDeactivateSound;
        SoundEffect[] walkSounds, runSounds;
        SoundEffect[] sounds;
        SoundEffect[] goreSounds;

        float blackScreenOpacity;
        float deltaTime;

        public const int MAX_LEVEL = 30;

        public Vector2 respawnPos;

        public List<Particle> particles = new List<Particle>();
        public List<int> keySecrets = new List<int>();
        List<Particle> chronoshiftParticles = new List<Particle>();

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
        public bool isReceivingInput = true;
        public bool isPlayerDead;
        public bool hasFiredWeapon;
        public bool isFacingRight = true;
        public bool isOnVines;
        public bool grabbedVine;
        public bool hasControl = true;

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

        bool gameOverSoundPlayed;
        bool deathAnimationDone;
        bool hasDeactiveSoundPlayed;
        bool stompSoundPlayed;
        bool fallSoundPlayed;
        bool goreSoundPlayed;

        //Debug
        public bool canFly;
        public bool isInvulnerable;
        public bool isGhost;

        //Player stats
        public int score = 0;
        public int health = 100;
        public int maxHealth = 100;
        public int armorPoints = 100;

        //Animation Variables
        int switchFrame;
        int currentFrame;
        Vector2 frameCount;
        #endregion

        /// <summary>
        /// Describes the state of the player's animation and determines which sprites to loop.
        /// </summary>
        public enum AnimationState
        {
            Still,
            Walking,
            Jumping,
            Falling,
            JumpWalking,
            Sleeping
        }

        public AnimationState CurrentAnimation = AnimationState.Still;
        public Evolution CurrentEvolution = Evolution.Eden;

        public Player()
        {
            //CollidedWithTerrainAbove += OnCollisionWithTerrainAbove;
            //CollidedWithTerrainBelow += OnCollisionWithTerrainBelow;
            //CollidedWithTerrainRight += OnCollisionWithTerrainRight;
            //CollidedWithTerrainLeft += OnCollisionWithTerrainLeft;
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
            position = new Vector2(collRectangle.X, collRectangle.Y);
            respawnPos = new Vector2(setX, setY);
            weapon = new Weapon();

            //Animation information
            frameCount = new Vector2(4, 6);
            collRectangle.Width = 32;
            collRectangle.Height = 64;
            sourceRectangle = new Rectangle(0, 0, 24, 40);
            drawRectangle = new Rectangle(0, 0, 48, 80);
        }

        /// <summary>
        /// Loads all of the player assets.
        /// </summary> 
        public void Load()
        {
            //Use this string to save the path to a file if multiple files begin with the same location.
            string path;

            //Use this array to get each evolution texture.
            path = "Characters/adam_";
            textureArray = new Texture2D[] 
            {
                ContentHelper.LoadTexture(path+"eden_new"),
                ContentHelper.LoadTexture(path+"prehistoric_s5"),
                ContentHelper.LoadTexture(path+"eden"),
                ContentHelper.LoadTexture(path+"eden"),
                ContentHelper.LoadTexture(path+"eden"),
                ContentHelper.LoadTexture(path+"modern_s5"),
                ContentHelper.LoadTexture(path+"future_s5"),
            };

            //Use this array to get any evolution single texture.
            //Single textures are used for the desintegrated rectangles effect.
            path = "Characters/Adam Singles/adam_";
            singleTextureArray = new Texture2D[]
            {
                ContentHelper.LoadTexture(path+"eden_single"),
                ContentHelper.LoadTexture(path+"prehistoric_single"),
                ContentHelper.LoadTexture(path+"eden_single"),
                ContentHelper.LoadTexture(path+"eden_single"),
                ContentHelper.LoadTexture(path+"eden_single"),
                ContentHelper.LoadTexture(path+"modern_single"),
                ContentHelper.LoadTexture(path+"future_single"),
            };

            //Load sound effects
            chronoActivateSound = ContentHelper.LoadSound("Sounds/chronoshift_activate");
            chronoDeactivateSound = ContentHelper.LoadSound("Sounds/chronoshift_deactivate");
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

            //Returns textures based on the current evolution. At the beginning they are all the same.
            newSingleTexture = GetSingleTexture();
            previousSingleTexture = GetSingleTexture();
            currentTexture = textureArray[0];

            //Weapon
            if (weapon != null)
                weapon.Load();
        }

        /// <summary>
        /// Update player information, checks for collision and input, and many other things.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="map"></param>
        public void Update(GameTime gameTime, Map map)
        {
            //If the player is currently chronoshifting, most of the update method is skipped.
            if (hasChronoshifted)
                goto UpdateChrono;

            this.gameTime = gameTime;
            this.map = map;

            deltaTime = (float)(60 * gameTime.ElapsedGameTime.TotalSeconds);

            //Update Method is spread out!
            //Check the following things
            DefineStats();
            CheckDead();
            CheckTimers();
            UpdateInput();
            UpdatePlayerPosition();
            DetectCollisionWithTerrain();
            //CheckTerrainCollision(map);
            DetectCollisions();
            CreateWalkingParticles();
            Animate();
            SetEvolutionAttributes();

            weapon.Update(this, map, gameTime);
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

            //Update particles
            foreach (Particle eff in particles)
            {
                eff.Update(gameTime);
                if (eff.ToDelete())
                {
                    particles.Remove(eff);
                    break;
                }
            }

            //Play death animation when player dies.
            if (isPlayerDead && !deathAnimationDone)
            {
                velocity.Y = -10f;
                velocity.X = 0f;
                deathAnimationDone = true;
            }

            //Wait a bit then respawn when the player dies.
            if (isPlayerDead)
            {
                respawnTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (respawnTimer > 3)
                {
                    returnToMainMenu = true;
                }
            }

            //For debugging chronoshift.
            if (InputHelper.IsKeyDown(Keys.Q) && !hasChronoshifted)
            {
                Chronoshift(Evolution.Modern);
            }

            previousPosition = position;

            //If player is chronoshifting the update method skips to here.

            UpdateChrono:
            if (hasChronoshifted)
            {
                if (!hasDeactiveSoundPlayed)
                    chronoSoundTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (chronoSoundTimer > 1000 && !hasDeactiveSoundPlayed)
                {
                    chronoDeactivateSound.Play();
                    sounds[1].Play();
                    hasDeactiveSoundPlayed = true;
                    chronoSoundTimer = 0;
                }
                foreach (var eff in chronoshiftParticles)
                {
                    eff.Update(gameTime);
                }
                bool processing = false;
                foreach (var eff in chronoshiftParticles)
                {
                    if (!eff.isComplete)
                        processing = true;
                }
                if (!processing)
                {
                    chronoshiftParticles = new List<Particle>();
                    currentTexture = textureArray[GetTextureNumber(CurrentEvolution)];
                    hasChronoshifted = false;
                    hasDeactiveSoundPlayed = false;
                }
            }
        }

        /// <summary>
        /// This method will check for input and update the player's position accordingly.
        /// </summary>
        private void UpdateInput()
        {
            //These variables define how fast the player will accelerate based on whether he is walking or runnning.
            //There is no need to put a max limit because after a certain speed, the friction is neough to maintain a const. vel.
            float walkingAcc = .5f;
            float runningAcc = .8f;
            float acceleration = .5f;

            //Check to see if player is running fast
            if (InputHelper.IsKeyDown(Keys.LeftShift))
            {
                acceleration = runningAcc;
            }
            else acceleration = walkingAcc;


            //Check if the player is moving Left
            if (Keyboard.GetState().IsKeyDown(Keys.A) && isReceivingInput == true && hasControl)
            {
                velocity.X -= acceleration;
                isFacingRight = false;
                sleepTimer = 0;
                if (!isJumping)
                    CurrentAnimation = AnimationState.Walking;
                if (canFly)
                    velocity.X = -10f;
            }

            //Check if the player is moving right
            if (Keyboard.GetState().IsKeyDown(Keys.D) && isReceivingInput == true && hasControl)
            {

                velocity.X += acceleration;
                isFacingRight = true;
                sleepTimer = 0;
                if (!isJumping)
                    CurrentAnimation = AnimationState.Walking;
                if (canFly)
                    velocity.X = 10f;
            }

            //For when he is on OP mode the player can fly up.
            if (Keyboard.GetState().IsKeyDown(Keys.W) && isReceivingInput == true && hasControl && canFly)
            {
                velocity.Y = -10f;
                sleepTimer = 0;
                CurrentAnimation = AnimationState.Jumping;
            }

            //For when he is on op mode the player can fly down.
            if (Keyboard.GetState().IsKeyDown(Keys.S) && isReceivingInput == true && hasControl && canFly)
            {
                velocity.Y = 10f;
                sleepTimer = 0;
                CurrentAnimation = AnimationState.Falling;
            }

            //Slows down y velocity if he is on op mode
            if (canFly)
            {
                velocity.Y = velocity.Y * 0.9f * (float)(60 * gameTime.ElapsedGameTime.TotalSeconds);
            }

            //Check if the player is climbing a vine
            if (isOnVines)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.W) && hasControl)
                {
                    velocity.Y = -5f;
                    grabbedVine = true;
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.S) && hasControl)
                {
                    velocity.Y = 5f;
                    grabbedVine = true;
                }
                else if (grabbedVine) //&& Keyboard.GetState().IsKeyUp(Keys.W)&& Keyboard.GetState().IsKeyUp(Keys.S))
                {
                    velocity.Y = 0;
                }
            }
            else grabbedVine = false;

            //Check if player is jumping
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && isSpaceBarPressed == false && isReceivingInput == true && isJumping == false && hasControl)
            {
                Jump();
            }

            //If player stops moving, reduce his speed gradually
            velocity.X -= velocity.X * .1f * deltaTime;

            //If his speed goes below a certain point, just make it zero
            if (Math.Abs(velocity.X) < .1f)
            {
                velocity.X = 0;
                PlayMovementSounds();
            }

            //If the player is not doing anything, change his Animation State
            if (velocity.X == 0 && isJumping == false && CurrentAnimation != AnimationState.Sleeping)
                CurrentAnimation = AnimationState.Still;

            //Check if the spacebar is pressed so that high jump mechanics can be activated
            if (Keyboard.GetState().IsKeyUp(Keys.Space))
                isSpaceBarPressed = false;

            //if the player is flying, start timing the time his is off ground for the high jump mechanic. If he is not, reset the timer.
            if (isJumping)
                offGroundTimer += gameTime.ElapsedGameTime.TotalSeconds;
            else offGroundTimer = 0;

            if (!canFly)
            {
                //IF there is a tile below the player, he will start falling.
                if (TileIndex + (map.mapTexture.Width * 2) < map.tileArray.Length)
                    if (!map.tileArray[TileIndex + (map.mapTexture.Width * 2)].isSolid)
                        velocity.Y += .2f * deltaTime;
                    else
                    {
                        isFlying = false;
                        if (CurrentAnimation == AnimationState.Jumping && !isJumping)
                            CurrentAnimation = AnimationState.Still;
                    }
                //This is for the high jump mechanic, where the player jumps higher if he holds the spacebar.
                if (isJumping)
                {
                    velocity.Y += .2f * deltaTime;
                    if (InputHelper.IsKeyDown(Keys.Space) && offGroundTimer < .3f)
                    { }
                    else velocity.Y += .07f * deltaTime;
                }
            }

            //if the player falls off a ledge without jumping, do not allow him to jump, but give him some room to jump if he is fast enough.
            if (velocity.Y > 2f)
                isJumping = true;

            //If the player is falling from a ledge, start the jump animation
            if (velocity.Y > 2 && (CurrentAnimation != AnimationState.Jumping && CurrentAnimation != AnimationState.Falling))
                CurrentAnimation = AnimationState.Jumping;

            //If player is trying to fly using his jetpack
            if (CurrentEvolution == Evolution.Future && InputHelper.IsLeftMousePressed())
            {
                if (jetpack.HasFuel)
                {
                    isFlying = true;
                    isJumping = true;
                    velocity.Y -= 1f;
                    CurrentAnimation = AnimationState.Jumping;

                    if (velocity.Y < Jetpack.MaxSpeed)
                        velocity.Y = Jetpack.MaxSpeed;
                }
            }
        }

        private void OnCollisionWithTerrainAbove(TerrainCollisionEventArgs e)
        {
            Tile tile = e.Tile;
            position.Y = tile.rectangle.Y + tile.rectangle.Height + 1;
            velocity.Y = 0f;
        }

        private void OnCollisionWithTerrainBelow(TerrainCollisionEventArgs e)
        {
            Tile tile = e.Tile;
            position.Y = tile.rectangle.Y - collRectangle.Height;
            velocity.Y = 0f;
            isJumping = false;
            isFlying = false;
            PlayStompSound();
        }

        private void OnCollisionWithTerrainRight(TerrainCollisionEventArgs e)
        {
            Tile tile = e.Tile;
            position.X = tile.rectangle.X - collRectangle.Width - 1;
            velocity.X = 0f;
        }

        private void OnCollisionWithTerrainLeft(TerrainCollisionEventArgs e)
        {
            Tile tile = e.Tile;
            position.X = tile.rectangle.X + tile.rectangle.Width + 1;
            velocity.X = 0f;
        }


        /// <summary>
        /// This method will check if the player is colliding with the terrain and prevent it.
        /// </summary>
        private void DetectCollisionWithTerrain()
        {
            Texture2D mapTexture = this.map.mapTexture;
            //This defines the player position in the map. 
            if (isGhost)
                return;

            int[] q = new int[12];
            q[0] = TileIndex - mapTexture.Width - 1;
            q[1] = TileIndex - mapTexture.Width;
            q[2] = TileIndex - mapTexture.Width + 1;
            q[3] = TileIndex - 1;
            q[4] = TileIndex;
            q[5] = TileIndex + 1;
            q[6] = TileIndex + mapTexture.Width - 1;
            q[7] = TileIndex + mapTexture.Width;
            q[8] = TileIndex + mapTexture.Width + 1;
            q[9] = TileIndex + mapTexture.Width + mapTexture.Width - 1;
            q[10] = TileIndex + mapTexture.Width + mapTexture.Width;
            q[11] = TileIndex + mapTexture.Width + mapTexture.Width + 1;

            //check the tiles around the player for collision
            foreach (int quadrant in q)
            {
                if (quadrant >= 0 && quadrant <= map.tileArray.Length - 1 && map.tileArray[quadrant].isSolid == true && isPlayerDead == false)
                {
                    if (yRect.Intersects(map.tileArray[quadrant].rectangle))
                    {
                        if (position.Y < map.tileArray[quadrant].rectangle.Y) //hits bot
                        {
                            //if (velocity.Y >= .5f)
                            position.Y = map.tileArray[quadrant].rectangle.Y - collRectangle.Height;
                            //else position.Y = previousPosition.Y;
                            velocity.Y = 0f;
                            isJumping = false;
                            isFlying = false;
                            PlayStompSound();
                        }
                        if (position.Y > map.tileArray[quadrant].rectangle.Y) //hits top
                        {
                            //if (velocity.Y <= -.5f)
                            position.Y = map.tileArray[quadrant].rectangle.Y + map.tileArray[quadrant].rectangle.Height + 1;
                            //else position.Y = previousPosition.Y;
                            velocity.Y = 0f;

                        }
                    }
                    else if (xRect.Intersects(map.tileArray[quadrant].rectangle))
                    {
                        if (position.X < map.tileArray[quadrant].rectangle.X) //hits right
                        {
                            //if (velocity.X >= .5f)
                            position.X = map.tileArray[quadrant].rectangle.X - collRectangle.Width - 1;
                            //else position.X = previousPosition.X;
                            velocity.X = 0f;
                        }
                        if (position.X > map.tileArray[quadrant].rectangle.X) //hits left
                        {
                            //if (velocity.X <= -.5f)
                            position.X = map.tileArray[quadrant].rectangle.X + map.tileArray[quadrant].rectangle.Width + 1;
                            //else position.X = previousPosition.X;
                            velocity.X = 0f;


                        }
                    }
                }

            }
        }

        /// <summary>
        /// This method updates all of the rectangles and applies velocity.
        /// </summary>
        private void UpdatePlayerPosition()
        {
            //Check if player is out of bounds
            if (position.X < 0)
                position.X = 0;
            if (position.X > (int)(map.mapTexture.Width * Game1.Tilesize - collRectangle.Width))
                position.X = (int)(map.mapTexture.Width * Game1.Tilesize - collRectangle.Width);
            if (position.Y < 0)
                position.Y = 0;
            if (position.Y > (int)(map.mapTexture.Height * Game1.Tilesize - collRectangle.Width) + 100)
            {
                KillAndRespawn();
                if (!fallSoundPlayed)
                {
                    fallSound.Play();
                    fallSoundPlayed = true;
                }
            }
            //Update the main collision rectangle;
            position += velocity * (float)(gameTime.ElapsedGameTime.TotalSeconds) * 60f;

            collRectangle.X = (int)(position.X);
            collRectangle.Y = (int)(position.Y);

            //Update the drawRectangle
            drawRectangle.X = collRectangle.X - 8;
            drawRectangle.Y = collRectangle.Y - 16;

            //Update the rectangles that define the player's position
            topMidBound = new Rectangle(collRectangle.X + 16, collRectangle.Y + 8, 1, 1);
            botMidBound = new Rectangle(collRectangle.X + 16, collRectangle.Y + (3 * 64 / 4), 1, 1);

            //Update the rectangles that define the player collision
            xRect = new Rectangle(collRectangle.X, collRectangle.Y + 15, 32, 64 - 20);
            yRect = new Rectangle(collRectangle.X + 10, collRectangle.Y, 32 - 20, 64);

            //Attack box for killing enemies
            attackBox = new Rectangle(collRectangle.X - 8, collRectangle.Y + collRectangle.Height - 10, collRectangle.Width + 16, 20);
        }

        //This changes the abilities that Adam has available depending on his evolution
        private void SetEvolutionAttributes()
        {
            switch (CurrentEvolution)
            {
                case Evolution.Eden:
                    canLift = false;
                    canBecomeInvisible = false;
                    canActivateShield = false;
                    canSeeInDark = false;
                    hasFireResistance = false;
                    hasJetpack = false;
                    weapon.SwitchWeapon(WeaponType.None);
                    break;
                case Evolution.Prehistoric:
                    canLift = true;
                    canBecomeInvisible = false;
                    canActivateShield = false;
                    canSeeInDark = false;
                    hasFireResistance = false;
                    hasJetpack = false;
                    weapon.SwitchWeapon(WeaponType.Stick);
                    break;
                case Evolution.Ancient:
                    canLift = false;
                    canBecomeInvisible = true;
                    canActivateShield = false;
                    canSeeInDark = false;
                    hasFireResistance = false;
                    hasJetpack = false;
                    weapon.SwitchWeapon(WeaponType.Bow);
                    break;
                case Evolution.Medieval:
                    canLift = false;
                    canBecomeInvisible = false;
                    canActivateShield = true;
                    canSeeInDark = false;
                    hasFireResistance = false;
                    hasJetpack = false;
                    weapon.SwitchWeapon(WeaponType.Sword);
                    break;
                case Evolution.Renaissance:
                    canLift = false;
                    canBecomeInvisible = false;
                    canActivateShield = false;
                    canSeeInDark = true;
                    hasFireResistance = false;
                    hasJetpack = false;
                    weapon.SwitchWeapon(WeaponType.Shotgun);
                    break;
                case Evolution.Modern:
                    canLift = false;
                    canBecomeInvisible = false;
                    canActivateShield = false;
                    canSeeInDark = false;
                    hasFireResistance = true;
                    hasJetpack = false;
                    weapon.SwitchWeapon(WeaponType.None);
                    break;
                case Evolution.Future:
                    canLift = false;
                    canBecomeInvisible = false;
                    canActivateShield = false;
                    canSeeInDark = false;
                    hasFireResistance = false;
                    hasJetpack = true;
                    weapon.SwitchWeapon(WeaponType.LaserGun);
                    break;
                case Evolution.God:
                    break;
            }
        }

        public void DefineStats()
        {
        }

        private void Animate()
        {
            switch (CurrentAnimation)
            {
                #region Still Animation
                case AnimationState.Still:
                    //define where in the spritesheet the still sequence is
                    sourceRectangle.Y = 0;
                    //defines the speed of the animation
                    switchFrame = 500;
                    //starts timer
                    frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

                    //if the time is up, moves on to the next frame
                    if (frameTimer >= switchFrame)
                    {
                        if (frameCount.X != 0)
                        {
                            frameTimer = 0;
                            sourceRectangle.X += sourceRectangle.Width;
                            currentFrame++;
                        }
                    }

                    if (currentFrame >= frameCount.X)
                    {
                        currentFrame = 0;
                        sourceRectangle.X = 0;
                    }
                    break;
                #endregion
                #region Walking Animation
                case AnimationState.Walking:
                    //define where in the spritesheet the still sequence is
                    sourceRectangle.Y = sourceRectangle.Height;
                    //defines the speed of the animation
                    if (velocity.X == 0)
                        switchFrame = 0;
                    else switchFrame = (int)Math.Abs(400 / velocity.X);
                    //starts timer
                    frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

                    //if the time is up, moves on to the next frame
                    if (frameTimer >= switchFrame)
                    {
                        if (frameCount.X != 0)
                        {
                            frameTimer = 0;
                            sourceRectangle.X += sourceRectangle.Width;
                            currentFrame++;
                        }
                    }

                    if (currentFrame == 0 || currentFrame == 2)
                    {
                        PlayMovementSounds();
                    }

                    if (currentFrame >= frameCount.X)
                    {
                        currentFrame = 0;
                        sourceRectangle.X = 0;
                    }

                    break;
                #endregion
                #region Jump Animation
                case AnimationState.Jumping:

                    //define where in the spritesheet the still sequence is
                    sourceRectangle.Y = sourceRectangle.Height * 2;
                    sourceRectangle.X = 0;
                    currentFrame = 0;
                    if (velocity.Y > -6)
                    {
                        sourceRectangle.X = sourceRectangle.Width;
                        currentFrame = 1;

                        if (velocity.Y > -2)
                        {
                            sourceRectangle.X = sourceRectangle.Width * 2;
                            currentFrame = 2;

                            if (velocity.Y > 2)
                                sourceRectangle.X = sourceRectangle.Width * 3;
                        }
                    }

                    break;
                #endregion
                #region Falling Animation
                case AnimationState.Falling:
                    //define where in the spritesheet the still sequence is
                    sourceRectangle.Y = sourceRectangle.Height * 3;
                    //defines the speed of the animation
                    switchFrame = 100;
                    //starts timer
                    frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

                    //if the time is up, moves on to the next frame
                    if (frameTimer >= switchFrame)
                    {
                        if (frameCount.X != 0)
                        {
                            frameTimer = 0;
                            sourceRectangle.X += sourceRectangle.Width;
                            currentFrame++;
                        }
                    }

                    if (currentFrame >= frameCount.X)
                    {
                        currentFrame = 0;
                        sourceRectangle.X = 0;
                    }
                    break;
                #endregion
                #region Jump and Walking Animation
                case AnimationState.JumpWalking:
                    if (velocity.X == 0)
                    {
                        CurrentAnimation = AnimationState.Jumping;
                        break;
                    }
                    //define where in the spritesheet the still sequence is
                    sourceRectangle.Y = sourceRectangle.Height * 4;
                    sourceRectangle.X = 0;
                    currentFrame = 0;
                    if (velocity.Y > -4)
                    {
                        sourceRectangle.X = sourceRectangle.Width;
                        currentFrame = 1;

                        if (velocity.Y > 7)
                        {
                            sourceRectangle.X = sourceRectangle.Width * 2;
                            currentFrame = 2;
                            //fallingTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

                            //if (velocity.Y >= 8 && fallingTimer > 500)
                            //{
                            //    CurrentAnimation = AnimationState.falling;
                            //    fallingTimer = 0;
                            //    sourceRectangle.X = 0;
                            //    currentFrame = 0;
                            //}
                        }
                    }
                    break;
                #endregion
                #region Sleeping Animation
                case AnimationState.Sleeping:
                    //define where in the spritesheet the still sequence is
                    sourceRectangle.Y = sourceRectangle.Height * 5;
                    //defines the speed of the animation
                    switchFrame = 600;
                    //starts timer
                    frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

                    //if the time is up, moves on to the next frame
                    if (frameTimer >= switchFrame)
                    {
                        if (frameCount.X != 0)
                        {
                            frameTimer = 0;
                            sourceRectangle.X += sourceRectangle.Width;
                            currentFrame++;
                        }
                    }

                    if (currentFrame >= frameCount.X)
                    {
                        currentFrame = 0;
                        sourceRectangle.X = 0;
                    }

                    zzzTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    if (zzzTimer > 1)
                    {
                        particles.Add(new Particle(this));
                        zzzTimer = 0;
                    }

                    break;
                #endregion
            }

        }

        private void CheckDead()
        {
            //If somewhere it is game over, do not let the player control
            if (isPlayerDead)
                isReceivingInput = false;

            //If his health falls below 0, kill him.
            if (health <= 0)
            {
                KillAndRespawn();
            }
        }

        private void CheckTimers()
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
            if (!isPlayerDead)
            {
                controlTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (controlTimer > .5)
                {
                    isReceivingInput = true;
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
                    Respawn();
                }
            }
        }

        private void DetectCollisions()
        {
            count++;
            Console.WriteLine(count);
            foreach (Obstacle ob in map.entities.OfType<Obstacle>())
            {
                if (ob.IsCollidable)
                {
                    CollisionLocation co = CheckCollisionWithOtherEntity(ob);
                    switch (co)
                    {
                        case CollisionLocation.Bottom:
                            position.Y = ob.collRectangle.Y - collRectangle.Height;
                            velocity.Y = 0f;
                            isJumping = false;
                            isFlying = false;
                            PlayMovementSounds();
                            PlayStompSound();
                            break;
                        case CollisionLocation.Right:
                            position.X = ob.collRectangle.X - collRectangle.Width - 1;
                            velocity.X = 0f;
                            break;
                        case CollisionLocation.Left:
                            position.X = ob.collRectangle.X + ob.collRectangle.Width + 1;
                            velocity.X = 0f;
                            break;
                        case CollisionLocation.Top:
                            break;
                        case CollisionLocation.Null:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            jetpack.Draw(spriteBatch);

            //Draw the player facing the direction he is supposed to be facing
            if (hasChronoshifted || isInvisible || isWaitingForRespawn)
                goto DrawOtherThings;

            if (isFacingRight == true)
                spriteBatch.Draw(currentTexture, drawRectangle, sourceRectangle, Color.White);
            else spriteBatch.Draw(currentTexture, drawRectangle, sourceRectangle, Color.White, 0, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0);

            //spriteBatch.Draw(ContentHelper.LoadTexture("Tiles/temp"), attackBox, Color.Red);

        DrawOtherThings:
            foreach (Particle z in particles)
                z.Draw(spriteBatch);
            foreach (var eff in chronoshiftParticles)
                eff.Draw(spriteBatch);


            if (weapon != null)
                weapon.Draw(spriteBatch);

            DrawSurroundIndexes(spriteBatch);
        }

        public void TakeDamage(int damage)
        {
            if (isInvulnerable)
                return;
            if (isInvincible)
                return;

            health -= damage;
            //make him invincible for a while and start the being hit animation
            isInvincible = true;
            isReceivingInput = false;
            takeDamageSound.Play();
            SpillBlood(Map.randGen.Next(3, 5));
        }

        public void TakeDamageAndKnockBack(int damage)
        {

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

        //TODO
        public void GetKnockedBack(Projectile projectile)
        {

        }

        private void Respawn()
        {
            if (PlayerRespawned != null)
                PlayerRespawned();

            health = maxHealth;
            //reset player velocity
            velocity = new Vector2(0, 0);
            //Take him back to the spawn point
            position.X = (int)respawnPos.X;
            position.Y = (int)respawnPos.Y;
            //Reset the death animation sequence so that the player can die again
            deathAnimationDone = false;
            gameOverSoundPlayed = false;
            fallSoundPlayed = false;
            isPlayerDead = false;
            goreSoundPlayed = false;

            hasControl = true;
            isInvisible = false;
            isWaitingForRespawn = false;

            map.RespawnEnemies();
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

        //Getters and Setters
        public bool IsGameOver
        {
            get { return isPlayerDead; }
        }

        public int ZZZCount
        {
            get { return particles.Count; }
        }

        public void GetDisintegratedRectangles(out Rectangle[] rectangles)
        {
            Vector2 size = new Vector2(previousSingleTexture.Width / Game1.Tilesize, previousSingleTexture.Height / Game1.Tilesize);
            int xSize = 4 * (int)size.X;
            int ySize = 4 * (int)size.Y;
            int width = previousSingleTexture.Width / xSize;
            int height = previousSingleTexture.Height / ySize;
            rectangles = new Rectangle[xSize * ySize];

            int i = 0;
            for (int h = 0; h < ySize; h++)
            {
                for (int w = 0; w < xSize; w++)
                {
                    rectangles[i] = new Rectangle(w * width, h * height, width, height);
                    i++;
                }
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
            if (TileIndex + (map.mapTexture.Width * 2) < map.tileArray.Length)
                tile = map.tileArray[TileIndex + (map.mapTexture.Width * 2)];
            //If the player is above air skip.
            if (tile.ID == 0)
                return;
            Particle eff = new Particle();
            eff.CreateTileParticleEffect(tile, this);
            particles.Add(eff);
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
            tile = map.tileArray[TileIndex + (map.mapTexture.Width * 2)];

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

        public void Jump()
        {
            //Make his velocity increase once
            velocity.Y = -10f;
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

            stompSoundPlayed = false;
        }

        public void Chronoshift(Evolution newEvolution)
        {
            previousSingleTexture = singleTextureArray[GetTextureNumber(CurrentEvolution)];
            CurrentEvolution = newEvolution;
            newSingleTexture = singleTextureArray[GetTextureNumber(CurrentEvolution)];

            if (!hasChronoshifted)
            {
                hasChronoshifted = true;
                isChronoshifting = true;
                Rectangle[] rectangles;
                GetDisintegratedRectangles(out rectangles);
                chronoActivateSound.Play();


                sounds[0].Play();

                foreach (var rec in rectangles)
                {
                    Particle eff = new Particle();
                    eff.CreatePlayerChronoshiftEffect(this, rec);
                    chronoshiftParticles.Add(eff);
                }
            }
        }

        public void KillAndRespawn()
        {
            if (isWaitingForRespawn)
                return;

            Rectangle[] rectangles;
            GetDisintegratedRectangles(out rectangles);

            foreach (var rec in rectangles)
            {
                Particle eff = new Particle();
                eff.CreatePlayerDesintegrationEffect(this, rec);
                particles.Add(eff);
            }

            int rand = Map.randGen.Next(20, 30);
            SpillBlood(rand);


            health = 0;
            hasControl = false;
            isWaitingForRespawn = true;
        }

        public void GameOver()
        {

        }

        private int GetTextureNumber(Evolution ev)
        {
            switch (ev)
            {
                case Evolution.Eden:
                    return 0;
                case Evolution.Prehistoric:
                    return 1;
                case Evolution.Ancient:
                    return 2;
                case Evolution.Medieval:
                    return 3;
                case Evolution.Renaissance:
                    return 4;
                case Evolution.Modern:
                    return 5;
                case Evolution.Future:
                    return 6;
                case Evolution.God:
                    return 7;
                case Evolution.InProgress:
                    return 0;
                default:
                    return 0;
            }
        }

        public Texture2D GetSingleTexture()
        {
            return singleTextureArray[GetTextureNumber(CurrentEvolution)];
        }

        public void PlayStompSound()
        {
            if (!stompSoundPlayed)
            {
                sounds[2].Play();
                stompSoundPlayed = true;
            }
        }

        public void PlayGoreSound()
        {
            if (!goreSoundPlayed)
            {
                int rand = Map.randGen.Next(0, 2);
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
                par.CreateBloodEffect(this, map);
                particles.Add(par);
            }
        }

        public void Heal(int amount)
        {
            //TODO add sounds.
            health += amount;
            if (health > maxHealth)
                health = maxHealth;
        }

    }

}
