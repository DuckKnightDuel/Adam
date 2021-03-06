﻿using System;
using Adam.Characters;
using Adam.Interactables;
using Adam.Levels;
using Adam.Misc;
using Adam.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adam.Player
{
    public partial class Player : Character
    {
        public delegate void DamageHandler(Rectangle damageArea, int damage);

        public delegate void Eventhandler();

        public delegate void PlayerHandler(Player player);

        private readonly Main _game1;
        private readonly Jetpack _jetpack = new Jetpack();
        private readonly PlayerScript script = new PlayerScript();
        public Rectangle AttackBox;
        public SoundFx AttackSound;
        //Debug
        public bool CanFly;
        public bool IsGhost;
        public bool IsInvulnerable;
        public bool IsClimbing { get; set; }
        private string _spawnPointNextLevel;

        public Player(Main game1)
        {
            _game1 = game1;

            script.Initialize(this);

            var edenTexture = ContentHelper.LoadTexture("Characters/adam_eden_darker");
            var idlePoop = ContentHelper.LoadTexture("Characters/adam_poop");
            var ninjaDash = ContentHelper.LoadTexture("Characters/adam_ninja");
            var fallStandTexture = ContentHelper.LoadTexture("Characters/adam_fall");
            var fightTexture = ContentHelper.LoadTexture("Characters/adam_punch");

            AttackSound = new SoundFx("Player/attackSound");

            ComplexAnim.AnimationEnded += ComplexAnim_AnimationEnded;
            ComplexAnim.AnimationStateChanged += ComplexAnim_AnimationStateChanged;
            ComplexAnim.FrameChanged += ComplexAnim_FrameChanged;

            // Animation textures.
            ComplexAnim.AddAnimationData("idle",
                new ComplexAnimData(0, edenTexture, new Rectangle(6, 7, 12, 66), 0, 24, 40, 400, 4, true));
            ComplexAnim.AddAnimationData("smellPoop",
                new ComplexAnimData(1, idlePoop, new Rectangle(6, 7, 12, 66), 0, 24, 40, 125, 21, false));
            ComplexAnim.AddAnimationData("sleep",
                new ComplexAnimData(1, edenTexture, new Rectangle(6, 7, 12, 66), 200, 24, 40, 125, 4, true));
            ComplexAnim.AddAnimationData("fightIdle",
                new ComplexAnimData(50, fightTexture, new Rectangle(6, 7, 12, 66), 40, 24, 40, 125, 4, true));
            ComplexAnim.AddAnimationData("walk",
                new ComplexAnimData(100, edenTexture, new Rectangle(6, 7, 12, 66), 40, 24, 40, 125, 4, true));
            ComplexAnim.AddAnimationData("run",
                new ComplexAnimData(150, edenTexture, new Rectangle(6, 7, 12, 66), 240, 24, 40, 125, 4, true));
            ComplexAnim.AddAnimationData("slide",
               new ComplexAnimData(153, edenTexture, new Rectangle(6, 7, 12, 66), 280, 24, 40, 125, 4, true));
            ComplexAnim.AddAnimationData("standup",
                new ComplexAnimData(155, fallStandTexture, new Rectangle(15, 7, 12, 66), 0, 45, 40, 125, 3, false));
            ComplexAnim.AddAnimationData("duck",
                new ComplexAnimData(156, fallStandTexture, new Rectangle(15, 7, 12, 66), 40, 45, 40, 125, 3, false));
            ComplexAnim.AddAnimationData("jump",
                new ComplexAnimData(200, edenTexture, new Rectangle(6, 7, 12, 66), 80, 24, 40, 125, 4, false));
            ComplexAnim.AddAnimationData("climb",
                new ComplexAnimData(900, edenTexture, new Rectangle(6, 7, 12, 66), 160, 24, 40, 125, 4, true));
            ComplexAnim.AddAnimationData("fall",
                new ComplexAnimData(1000, edenTexture, new Rectangle(6, 7, 12, 66), 120, 24, 40, 125, 4, true));
            ComplexAnim.AddAnimationData("ninjaDash",
                new ComplexAnimData(1100, ninjaDash, new Rectangle(19, 8, 12, 66), 0, 48, 40, 200, 1, false));
            ComplexAnim.AddAnimationData("punch",
                new ComplexAnimData(1110, fightTexture, new Rectangle(6, 7, 12, 66), 0, 24, 40, 75, 4, false));
            ComplexAnim.AddAnimationData("punch2",
                new ComplexAnimData(1111, fightTexture, new Rectangle(6, 7, 12, 66), 80, 24, 40, 75, 4, false));
            ComplexAnim.AddAnimationData("death",
                new ComplexAnimData(int.MaxValue, edenTexture, new Rectangle(6, 7, 12, 66), 280, 24, 40, 125, 4, true));

            // Sounds
            Sounds.AddSoundRef("hurt", "Player/hurtSound");
            Sounds.AddSoundRef("jump", "Player/jumpSound");
            Sounds.AddSoundRef("stomp", "Player/jumpSound");
            Sounds.AddSoundRef("punch", "Sounds/punch");
            Sounds.AddSoundRef("fail", "Sounds/Menu/level_fail");

            ComplexAnim.AddToQueue("idle");

            InitializeInput();
            Initialize(0, 0);

            PlayerAttacked += OnPlayerAttack;
            HasFinishedDying += OnPlayerDeath;
            HasTakenDamage += OnDamageTaken;
            HasRevived += OnPlayerRevive;
        }

        private void OnPlayerRevive()
        {
            Overlay.Instance.FadeIn();
        }

        protected override Rectangle DrawRectangle => new Rectangle(CollRectangle.X - 8, CollRectangle.Y - 16, 48, 80);
        //Player stats
        public int Score
        {
            get
            {
                try
                {
                    return _game1.GameData.CurrentSave.PlayerStats.Score;
                }
                catch
                {
                    return 0;
                }
            }
            set { _game1.GameData.CurrentSave.PlayerStats.Score = value; }
        }

        public override int MaxHealth => 100;
        //Animation Variables
        public int CurrentAnimationFrame { get; private set; }
        public event EventHandler PlayerRespawned;
        public event PlayerHandler AnimationEnded;
        public event PlayerHandler AnimationFrameChanged;
        public event DamageHandler PlayerAttacked;
        public event DamageHandler PlayerDamaged;

        private void OnPlayerAttack(Rectangle damageArea, int damage)
        {
            Sounds.Get("punch").Play();
        }

        private void ComplexAnim_FrameChanged(FrameArgs e)
        {
            CurrentAnimationFrame = e.CurrentFrame;
            if (AnimationFrameChanged != null)
                AnimationFrameChanged(this);
        }

        private void ComplexAnim_AnimationStateChanged()
        {
        }

        private void ComplexAnim_AnimationEnded()
        {
            AnimationEnded?.Invoke(this);
        }

        /// <summary>
        ///     This method will set the player's positions to those specified. It should be used when the map is changed.
        /// </summary>
        /// <param name="setX"> The x-Coordinate</param>
        /// <param name="setY"> The y-Coordinate</param>
        public void Initialize(int setX, int setY)
        {
            if (_spawnPointNextLevel != null)
            {
                int spawnIndex;
                if (int.TryParse(_spawnPointNextLevel, out spawnIndex))
                {
                    int x = (spawnIndex % GameWorld.Instance.WorldData.LevelWidth)*Main.Tilesize;
                    int y = (spawnIndex / GameWorld.Instance.WorldData.LevelWidth)*Main.Tilesize;
                    CollRectangle.X = x;
                    CollRectangle.Y = y;
                    RespawnPos = new Vector2(x, y);
                    _spawnPointNextLevel = null;
                    goto NoError;
                }

            }
            //Set the player position according to where in the map his default spawn point is.
            CollRectangle.X = setX;
            CollRectangle.Y = setY;
            RespawnPos = new Vector2(setX, setY);

            NoError:

            //Animation information
            CollRectangle.Width = 32;
            CollRectangle.Height = 64;
            SourceRectangle = new Rectangle(0, 0, 24, 40);

            InitializeInput();
        }

        /// <summary>
        ///     Update player information, checks for collision and input, and many other things.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            script.Run();

            if (GameWorld.Instance.CurrentGameMode == GameMode.Edit)
            {
                ContainInGameWorld();
                return;
            }

            CheckInput();
            Burn();
            UpdatePlayerPosition();
            base.Update();

            _jetpack.Update(this, gameTime);
        }

        /// <summary>
        ///     This method updates all of the rectangles and applies velocity.
        /// </summary>
        private void UpdatePlayerPosition()
        {
            ContainInGameWorld();
        }

        private void ContainInGameWorld()
        {
            var gameWorld = GameWorld.Instance;
            if (CollRectangle.X < 0)
                CollRectangle.X = 0;
            if (CollRectangle.X > (gameWorld.WorldData.LevelWidth * Main.Tilesize - CollRectangle.Width))
                CollRectangle.X = (gameWorld.WorldData.LevelWidth * Main.Tilesize - CollRectangle.Width);
            if (CollRectangle.Y < 0)
                CollRectangle.Y = 0;
            if (CollRectangle.Y > (gameWorld.WorldData.LevelHeight * Main.Tilesize - CollRectangle.Width) + 100)
            {
                // Player dies when he falls out of the world in play mode.
                if (gameWorld.CurrentGameMode == GameMode.Edit)
                    CollRectangle.Y = gameWorld.WorldData.LevelHeight * Main.Tilesize - CollRectangle.Height;
                else
                {
                    TakeDamage(null, MaxHealth);
                }
            }
        }

        public void CreateMovingParticles()
        {
            if (!IsJumping)
            {
                if (Math.Abs(Velocity.X) < .1f)
                    return;
                if (_movementParticlesTimer.TimeElapsedInMilliSeconds > 500 / Math.Abs(Velocity.X))
                {
                    _movementParticlesTimer.Reset();
                    var par = new SmokeParticle(CollRectangle.Center.X, CollRectangle.Bottom,
                        new Vector2(0, (float)(GameWorld.RandGen.Next(-5, 5) / 10f)));
                    GameWorld.ParticleSystem.Add(par);
                }
            }
        }

        private void Burn()
        {
            // Checks to see if player is on fire and deals damage accordingly.
            if (IsOnFire)
            {
                if (_onFireTimer.TimeElapsedInSeconds < 4)
                {
                    if (_fireTickTimer.TimeElapsedInMilliSeconds > 500)
                    {
                        TakeDps(EnemyDb.FlameSpitterDps);
                        _fireTickTimer.Reset();
                    }
                    if (_fireSpawnTimer.TimeElapsedInMilliSeconds > 100)
                    {
                        var flame = new EntityFlameParticle(this, Color.Yellow);
                        var flame2 = new EntityFlameParticle(this, Color.Red);
                        GameWorld.Instance.Particles.Add(flame);
                        GameWorld.Instance.Particles.Add(flame2);
                        _fireSpawnTimer.Reset();
                    }
                }
                else
                {
                    IsOnFire = false;
                    _onFireTimer.Reset();
                    _fireTickTimer.Reset();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (GameWorld.Instance.CurrentGameMode == GameMode.Edit)
                return;
            ComplexAnim.Draw(spriteBatch, IsFacingRight, Color);
            base.Draw(spriteBatch);
        }

        /// <summary>
        ///     Player takes damage without becoming invincible and without spilling blood.
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDps(int damage)
        {
            Health -= damage;
        }

        private void CreateJumpParticles()
        {
            for (var i = 0; i < 20; i++)
            {
                GameWorld.Instance.Particles.Add(new JumpSmokeParticle(this));
            }
        }

        private void CreateStompParticles()
        {
            for (var i = 0; i < 20; i++)
            {
                GameWorld.Instance.Particles.Add(new StompSmokeParticle(this));
            }
        }

        public void Heal(int amount)
        {
            //TODO add sounds.
            Health += amount;
            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        /// <summary>
        ///     Fires an event to all subscribers saying the player is dealing damage to that area.
        /// </summary>
        /// <param name="damageArea"></param>
        /// <param name="damage"></param>
        public void DealDamage(Rectangle damageArea, int damage)
        {
            PlayerAttacked?.Invoke(damageArea, damage);
        }

        public void MoveTo(Vector2 position)
        {
            CollRectangle.X = (int)position.X;
            CollRectangle.Y = (int)position.Y;
            Overlay.Instance.FadeIn();
        }

        private void OnPlayerDeath(Entity entity)
        {
            _respawnTimer.ResetAndWaitFor(4000);
            _respawnTimer.SetTimeReached += Revive;
            Sounds.Get("fail").Play();
            Overlay.Instance.FadeOut();
        }

        private void OnDamageTaken()
        {
            Sounds.Get("hurt").Play();
        }

        public void SetRespawnPoint(int x, int y)
        {
            RespawnPos = new Vector2(x, y);
        }

        /// <summary>
        /// Sets the spawn point for the player when the next level loads.
        /// </summary>
        /// <param name="spawnPoint"></param>
        public void SetSpawnPointForNextLevel(string spawnPoint)
        {
            _spawnPointNextLevel = spawnPoint;
        }
    }
}