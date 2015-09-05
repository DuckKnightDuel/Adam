using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Adam;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using Adam.GameData;
using Adam.Network;
using Adam.UI;
using Adam.UI.Information;

namespace Adam
{
    public enum GameState
    {
        SplashScreen,
        Cutscene,
        MainMenu,
        LoadingScreen,
        GameWorld,
    }
    public enum GameMode
    {
        None,
        Edit,
        Play,
    }

    public class Main : Microsoft.Xna.Framework.Game
    {
        private static Main _instance;
        /// <summary>
        /// The running instance of the game.
        /// </summary>
        public static Main Instance
        {
            get { return _instance; }
        }

        #region Variables

        // Color presets for lighting engine.
        static Color SunnyPreset = new Color(255, 238, 186);
        static Color HellPreset = new Color(255, 129, 116);
        static Color WinterPreset = new Color(200, 243, 255);
        static Color NightPreset = new Color(120, 127, 183);
        static Color SunsetPreset = new Color(255, 155, 13);

        // Rendering variables.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont debugFont;
        private RenderTarget2D mainRenderTarget;
        private RenderTarget2D lightingRenderTarget;

        Camera camera;
        Menu menu;
        Thread reloadThread;
        Overlay overlay;
        Texture2D splashDKD;
        Texture2D blackScreen;
        SoundEffect quack;
        GameDebug debug;
        Cutscene cutscene;
        public static bool IsLoadingContent;
        bool hasQuacked;
        bool isDebug = true;
        Stopwatch updateWatch, drawWatch, renderWatch, lightWatch, loadWatch;
        double splashTimer, updateTime, drawTime, lightTime, renderTime;
        double frameRateTimer;
        int fps, totalFrames;

        public const int Tilesize = 32;
        public const int DefaultResWidth = 960;
        public const int DefaultResHeight = 540;

        public static int UserResWidth;
        public static int UserResHeight;

        public const string Version = "Version 0.7.0.0 Beta";
        public const string Producers = "Duck Knight Duel Games";

        public static Texture2D DefaultTexture;
        public static Dialog Dialog;

        public const float Gravity = .5f;

        public static double WidthRatio;
        public static double HeightRatio;

        public bool wasPressed, debugOn, debugPressed;

        public SamplerState desiredSamplerState;
        private BlendState LightBlendState;

        //Defines the initial GameState ----- Use this variable to change the GameState
        public GameState CurrentGameState;
        GameState desiredGameState;
        public GameMode CurrentGameMode;

        //Game Variables
        GameWorld gameWorld;
        Session session;
        public static ObjectiveTracker ObjectiveTracker;
        public GameDataManager GameData;
        public Player player;
        private LoadingScreen loadingScreen;
        public static ContentManager Content;
        public static GraphicsDevice GraphicsDeviceInstance;
        public static DataFolder DataFolder = new DataFolder();

        /// <summary>
        /// Used to display messages to the user where he needs to press OK to continue.
        /// </summary>
        public static MessageBox MessageBox { get; set; }

        public static TextInputBox TextInputBox { get; set; }
        #endregion

        public Main()
        {
            // Get the current monitor resolution and set it as the game's resolution
            Vector2 monitorRes = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            UserResWidth = (int)monitorRes.X;
            UserResHeight = (int)monitorRes.Y;
            WidthRatio = ((double)Main.DefaultResWidth / (double)Main.UserResWidth);
            HeightRatio = ((double)Main.DefaultResHeight / (double)Main.UserResHeight);

            // Important services that need to be instanstiated before other things.
            graphics = new GraphicsDeviceManager(this);
            Content = new ContentManager(Services, "Content");
            GameData = new GameDataManager();
            
            // Sets the current screen resolution to the user resolution.
            graphics.PreferredBackBufferWidth = UserResWidth;
            graphics.PreferredBackBufferHeight = UserResHeight;

            // Change game settings here.
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferMultiSampling = false;
            IsFixedTimeStep = true;
            graphics.IsFullScreen = GameData.Settings.IsFullscreen;

            // Set window to borderless.
            IntPtr hWnd = this.Window.Handle;
            var control = System.Windows.Forms.Control.FromHandle(hWnd);
            var form = control.FindForm();
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;



            //MediaPlayer Settings
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = .5f;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
        }

        protected override void Initialize()
        {
            camera = new Camera(GraphicsDevice.Viewport);
            menu = new Menu(this);
            gameWorld = new GameWorld(this);
            player = new Player(this);
            overlay = new Overlay();
            cutscene = new Cutscene();
            Dialog = new Dialog();
            ObjectiveTracker = new ObjectiveTracker();
            MessageBox = new MessageBox();
            TextInputBox = new TextInputBox();

            DefaultTexture = ContentHelper.LoadTexture("Tiles/temp tile");
            GraphicsDeviceInstance = graphics.GraphicsDevice;

            //Initialize the game render target
            mainRenderTarget = new RenderTarget2D(GraphicsDevice, DefaultResWidth, DefaultResHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PreserveContents);
            lightingRenderTarget = new RenderTarget2D(GraphicsDevice, DefaultResWidth, DefaultResHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PreserveContents);

            spriteBatch = new SpriteBatch(GraphicsDevice);


            LightBlendState = new BlendState();
            LightBlendState.AlphaSourceBlend = Blend.DestinationColor;
            LightBlendState.ColorSourceBlend = Blend.DestinationColor;
            LightBlendState.ColorDestinationBlend = Blend.Zero;

            updateWatch = new Stopwatch();
            drawWatch = new Stopwatch();
            lightWatch = new Stopwatch();
            renderWatch = new Stopwatch();
            loadWatch = new Stopwatch();

            base.Initialize();

        }

        protected override void LoadContent()
        {
            CurrentGameState = GameState.SplashScreen;
            loadingScreen = new LoadingScreen(new Vector2(UserResWidth,UserResHeight), Content);
            splashDKD = ContentHelper.LoadTexture("Backgrounds/Splash/DKD_new");
            quack = Content.Load<SoundEffect>("Backgrounds/Splash/quack");
            blackScreen = ContentHelper.LoadTexture("Tiles/black");

            debugFont = Content.Load<SpriteFont>("debug");
            menu.Load(Content);
            cutscene.Load(Content);

            debug = new GameDebug(debugFont, new Vector2(UserResWidth,UserResHeight), blackScreen);

            CurrentGameMode = GameMode.None;

        }

        public void ChangeState(GameState desiredGameState, GameMode mode)
        {
            CurrentGameState = GameState.LoadingScreen;
            this.desiredGameState = desiredGameState;
            IsLoadingContent = true;
            loadingScreen.Restart();

            if (desiredGameState == GameState.GameWorld)
            {
                LoadWorldFromFile(mode);
            }
            else
            {
                IsLoadingContent = false;
            }

        }

        public void LoadWorldFromFile(GameMode mode)
        {
            CurrentGameState = GameState.LoadingScreen;
            CurrentGameMode = mode;
            desiredGameState = GameState.GameWorld;
            IsLoadingContent = true;
            loadingScreen.Restart();

            reloadThread = new Thread(new ThreadStart(BackgroundThread_FileLoad));
            reloadThread.IsBackground = true;
            reloadThread.Start();
        }

        private void BackgroundThread_FileLoad()
        {
            IsLoadingContent = true;
            if (gameWorld.TryLoadFromFile(CurrentGameMode) == true)
            {
                ObjectiveTracker = GameData.CurrentSave.ObjTracker;
            }
            else
            {
                CurrentGameMode = GameMode.None;
                CurrentGameState = GameState.MainMenu;
            }

            IsLoadingContent = false;
            wasPressed = false;
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            updateWatch.Start();

            frameRateTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (frameRateTimer > 1000f)
            {
                fps = totalFrames;
                totalFrames = 0;
                frameRateTimer = 0;
            }

            #region Mouse Settings

            switch (CurrentGameState)
            {
                case GameState.MainMenu:
                    this.IsMouseVisible = true;
                    break;
                case GameState.GameWorld:
                    this.IsMouseVisible = true;
                    break;
            }
            #endregion

            if (MessageBox.IsActive)
            {
                MessageBox.Update();
                return;
            }
            if (TextInputBox.IsActive)
            {
                TextInputBox.Update();
                return;
            }

            if (GameData.Settings.HasChanged)
            {
                GameData.SaveSettings();
                if (GameData.Settings.NeedsRestart)
                {
                    graphics.IsFullScreen = GameData.Settings.IsFullscreen;
                    graphics.ApplyChanges();
                    GameData.Settings.NeedsRestart = false;
                }
                GameData.Settings.HasChanged = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && CurrentGameState != GameState.MainMenu && CurrentGameState != GameState.LoadingScreen)
            {
                if (CurrentGameMode == GameMode.Edit)
                {
                    if (GameWorld.Instance.levelEditor.actionBar.AskSaveDialog())
                    {
                        GameWorld.Instance.levelEditor.actionBar.SaveButton_MouseClicked();
                    }
                }
                GameData.SaveGame();
                ChangeState(GameState.MainMenu, GameMode.None);

            }

            //Update the game based on what GameState it is
            switch (CurrentGameState)
            {
                case GameState.SplashScreen:
                    splashTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    if (splashTimer > 2)
                        CurrentGameState = GameState.Cutscene;
                    if (splashTimer > 1 && !hasQuacked)
                    {
                        hasQuacked = true;
                        quack.Play();
                    }
                    break;
                case GameState.Cutscene:
                    if (isDebug)
                    {
                        CurrentGameState = GameState.MainMenu;
                        cutscene.Stop();
                    }
                    if (cutscene.wasPlayed == false)
                    {
                        cutscene.Update(CurrentGameState);
                    }
                    else CurrentGameState = GameState.MainMenu;
                    if (InputHelper.IsAnyInputPressed())
                    {
                        CurrentGameState = GameState.MainMenu;
                        cutscene.Stop();
                    }
                    break;
                case GameState.MainMenu:
                    menu.Update(this, gameTime, GameData.Settings);
                    break;
                case GameState.LoadingScreen:
                    loadingScreen.Update(gameTime);

                    if (!IsLoadingContent && loadingScreen.isReady)
                    {
                        CurrentGameState = desiredGameState;
                        overlay.FadeIn();
                    }
                    break;
                case GameState.GameWorld:
                    if (IsLoadingContent) return;
                    if (gameWorld.isOnDebug)
                        break;

                    gameWorld.Update(gameTime, CurrentGameMode, camera);
                    player.Update(gameTime);
                    overlay.Update(gameTime, player, gameWorld);
                    Dialog.Update(gameTime);
                    ObjectiveTracker.Update(gameTime);

                    if (player.returnToMainMenu)
                        ChangeState(GameState.MainMenu, GameMode.None);
                    break;
            }

            base.Update(gameTime);
            debug.Update(this, player, gameWorld, debugOn);
            updateWatch.Stop();
            updateTime = updateWatch.ElapsedMilliseconds;
            updateWatch.Reset();
        }

        protected void PrepareRenderTargets()
        {
            //Does all rendertarget work
            switch (CurrentGameState)
            {
                case GameState.GameWorld:
                    renderWatch.Start();
                    DrawToMainRenderTarget(mainRenderTarget);
                    renderTime = renderWatch.ElapsedMilliseconds;
                    renderWatch.Reset();

                    lightWatch.Start();
                    DrawLightingRenderTarget(lightingRenderTarget);
                    lightTime = lightWatch.ElapsedMilliseconds;
                    lightWatch.Reset();

                    break;
                case GameState.Cutscene:
                    DrawToMainRenderTarget(mainRenderTarget);
                    break;
                case GameState.MainMenu:
                    DrawToMainRenderTarget(mainRenderTarget);
                    break;
            }

        }

        protected void DrawToMainRenderTarget(RenderTarget2D renderTarget)
        {
            //Change RenderTarget to this from the default
            GraphicsDevice.SetRenderTarget(renderTarget);
            //Set up the background color
            GraphicsDevice.Clear(Color.Black);

            //Draw what is needed based on GameState
            switch (CurrentGameState)
            {
                case GameState.Cutscene:
                    spriteBatch.Begin();
                    cutscene.Draw(spriteBatch);
                    spriteBatch.End();
                    break;
                case GameState.GameWorld:
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                    gameWorld.DrawBackground(spriteBatch);
                    gameWorld.DrawClouds(spriteBatch);
                    spriteBatch.End();

                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camera.Translate);
                    gameWorld.DrawInBack(spriteBatch);
                    gameWorld.Draw(spriteBatch);
                    player.Draw(spriteBatch);
                    spriteBatch.End();

                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, null, null, null, camera.Translate);
                    gameWorld.DrawGlows(spriteBatch);
                    spriteBatch.End();
                    break;
            }

            //Return the current RenderTarget to the default
            GraphicsDevice.SetRenderTarget(null);
        }

        protected void DrawLightingRenderTarget(RenderTarget2D renderTarget)
        {
            //Change RenderTarget to this from the default
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Black);

            switch (CurrentGameState)
            {
                case GameState.GameWorld:
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, null, null, null);
                    spriteBatch.Draw(ContentHelper.LoadTexture("Tiles/max_shadow"), new Rectangle(0, 0, Main.DefaultResWidth, Main.DefaultResHeight), Color.White);
                    spriteBatch.End();

                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, null, null, null, camera.Translate);
                    gameWorld.DrawLights(spriteBatch);
                    spriteBatch.End();
                    break;
            }

            //Return the current RenderTarget to the default
            GraphicsDevice.SetRenderTarget(null);
        }

        protected override void Draw(GameTime gameTime)
        {
            drawWatch.Start();
            totalFrames++;
            PrepareRenderTargets();

            //Set background color
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draw what is needed based on GameState
            switch (CurrentGameState)
            {
                case GameState.SplashScreen:
                    spriteBatch.Begin();
                    spriteBatch.Draw(splashDKD, new Rectangle(0, 0,UserResWidth, UserResHeight), Color.White);
                    spriteBatch.End();
                    break;
                case GameState.Cutscene:
                    spriteBatch.Begin();
                    spriteBatch.Draw(mainRenderTarget, new Rectangle(0, 0, UserResWidth, UserResHeight), Color.White);
                    spriteBatch.End();
                    break;
                case GameState.LoadingScreen:
                    spriteBatch.Begin();
                    loadingScreen.Draw(spriteBatch);
                    spriteBatch.End();
                    break;
                case GameState.MainMenu:
                    RasterizerState rs2 = new RasterizerState() { ScissorTestEnable = true };
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, rs2);
                    menu.Draw(spriteBatch);
                    MessageBox.Draw(spriteBatch);
                    spriteBatch.End();
                    break;
                case GameState.GameWorld:
                    //Draw the rendertarget
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                    spriteBatch.Draw(mainRenderTarget, new Rectangle(0, 0, UserResWidth, UserResHeight), Color.White);
                    spriteBatch.End();

                    if (GameData.Settings.DesiredLight)
                    {
                        spriteBatch.Begin(SpriteSortMode.Immediate, LightBlendState, GameData.Settings.DesiredSamplerState, DepthStencilState.None, RasterizerState.CullNone);
                        spriteBatch.Draw(lightingRenderTarget, new Rectangle(0, 0, UserResWidth, UserResHeight), SunnyPreset);
                        spriteBatch.End();
                    }

                    RasterizerState rs = new RasterizerState() { ScissorTestEnable = true };
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, rs);
                    overlay.Draw(spriteBatch);

                    if (!gameWorld.levelEditor.onInventory)
                        ObjectiveTracker.Draw(spriteBatch);

                    gameWorld.DrawUI(spriteBatch);
                    Dialog.Draw(spriteBatch);
                    TextInputBox.Draw(spriteBatch);
                    MessageBox.Draw(spriteBatch);

                    spriteBatch.End();

                    break;
            }
            base.Draw(gameTime);

            if (CurrentGameState != GameState.SplashScreen)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.F3) && !debugPressed && !debugOn)
                {
                    debugOn = true;
                    debugPressed = true;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.F3) && !debugPressed && debugOn)
                {
                    debugOn = false;
                    debugPressed = true;
                }

                if (Keyboard.GetState().IsKeyUp(Keys.F3))
                {
                    debugPressed = false;
                }

                if (debugOn)
                {
                    spriteBatch.Begin();
                    spriteBatch.Draw(blackScreen, new Rectangle(0, 0, UserResWidth, 320), Color.White * .3f);
                    spriteBatch.DrawString(debugFont, Main.Version + " FPS: " + fps, new Vector2(0, 0), Color.White);
                    spriteBatch.DrawString(debugFont, "Is Player Above Tile: " + player.IsAboveTile, new Vector2(0, 20), Color.White);
                    spriteBatch.DrawString(debugFont, "Camera Position:" + camera.invertedCoords.X + "," + camera.invertedCoords.Y, new Vector2(0, 40), Color.White);
                    spriteBatch.DrawString(debugFont, "Editor Rectangle Position:" + gameWorld.levelEditor.editorRectangle.X + "," + gameWorld.levelEditor.editorRectangle.Y, new Vector2(0, 60), Color.White);
                    spriteBatch.DrawString(debugFont, "Camera Zoom:" + camera.GetZoom(), new Vector2(0, 80), Color.White);
                    spriteBatch.DrawString(debugFont, "Times Updated: " + gameWorld.TimesUpdated, new Vector2(0, 100), Color.White);
                    spriteBatch.DrawString(debugFont, "Player is dead: " + player.IsDead(), new Vector2(0,120),Color.White);
                    spriteBatch.DrawString(debugFont, "AnimationState:" + player.CurrentAnimation, new Vector2(0, 140), Color.White);
                    spriteBatch.DrawString(debugFont, "Level:" + CurrentGameMode, new Vector2(0, 160), Color.White);
                    spriteBatch.DrawString(debugFont, "Player Velocity" + player.GetVelocity(), new Vector2(0, 180), Color.White);
                    spriteBatch.DrawString(debugFont, "Total Chunks: " + gameWorld.chunkManager.GetNumberOfChunks() + " Active Chunk: "+GameWorld.Instance.chunkManager.GetActiveChunkIndex(), new Vector2(0, 200), Color.White);
                    spriteBatch.DrawString(debugFont, "Dynamic Lights Count: " + gameWorld.lightEngine?.dynamicLights.Count, new Vector2(0, 220), Color.White);
                    spriteBatch.DrawString(debugFont, "Particle Count: " + gameWorld.particles?.Count, new Vector2(0, 240), Color.White);
                    spriteBatch.DrawString(debugFont, "Entity Count: " + gameWorld.entities?.Count, new Vector2(0, 260), Color.White);
                    spriteBatch.DrawString(debugFont, "Visible Tiles: " + gameWorld.visibleTileArray?.Length, new Vector2(0, 280), Color.White);
                    spriteBatch.DrawString(debugFont, "Visible Lights: " + gameWorld.visibleLightArray?.Length, new Vector2(0, 300), Color.White);

                    debug.Draw(spriteBatch);
                    spriteBatch.End();
                }

            }

            drawWatch.Stop();
            drawTime = drawWatch.ElapsedMilliseconds;
            drawWatch.Reset();
        }
    }
}