﻿
using System.Windows.Forms;
using Adam.Levels;
using Adam.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adam.UI.Elements
{
    /// <summary>
    /// Used to contain UI elements.
    /// </summary>
    class InGameWindow
    {
        private static Rectangle[] _sourceRectangles =
        {
            new Rectangle(320, 198, 4, 4), // top left
            new Rectangle(325, 198, 1, 4), // top
            new Rectangle(327, 198, 4, 4), // top right
            new Rectangle(320, 203, 11, 4), // middle fill
            new Rectangle(320, 208, 4, 4), // bot left
            new Rectangle(325, 208, 1, 4), // bot
            new Rectangle(327, 208, 4, 4), // bot right
        };

        private Vector2 _hiddenPos;
        private Vector2 _shownPos;
        private Vector2 _startPos;
        private Misc.Timer _animationTimer = new Misc.Timer();
        public Vector2 Position { get; private set; }
        public Vector2 Size { get; private set; }
        public bool IsHidden { get; private set; } = true;
        private Texture2D _texture = GameWorld.UiSpriteSheet;
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Makes a window with these game dimensions at the center of the screen.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public InGameWindow(int width, int height)
        {
            width = CalcHelper.ApplyUiRatio(width);
            height = CalcHelper.ApplyUiRatio(height);

            Size = new Vector2(width, height);
            float x = (Main.UserResWidth/2) - width/2;
            float y = (Main.UserResHeight/2) - height/2;
            Position = new Vector2(x,y);

             _shownPos = Position;
            Position = new Vector2(Position.X, Main.UserResHeight);
            _hiddenPos = Position;
        }


        /// <summary>
        /// Makes a window in game dimensions at the game coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public InGameWindow(int x, int y, int width, int height, bool convertCoords)
        {
            if (convertCoords)
            {
                width = CalcHelper.ApplyUiRatio(width);
                height = CalcHelper.ApplyUiRatio(height);
                x = CalcHelper.ApplyUiRatio(x);
                y = CalcHelper.ApplyUiRatio(y);
            }

            Size = new Vector2(width, height);
            Position = new Vector2(x,y);

            _shownPos = Position;
            Position = new Vector2(Position.X, Main.UserResHeight);
            _hiddenPos = Position;
        }

        public void DisableAnimation()
        {
            Position = _shownPos;
            Show();
        }

        public void Show()
        {
            if (IsHidden)
            {
                IsHidden = false;
                _animationTimer.Reset();
                _startPos = Position;
            }
        }

        public void Hide()
        {
            if (!IsHidden)
            {
                IsHidden = true;
                _animationTimer.Reset();
                _startPos = Position;
            }
        }

        public void Update()
        {
            if (IsHidden)
            {
                Position = new Vector2(Position.X,
                    CalcHelper.EaseInAndOut((float)_animationTimer.TimeElapsedInMilliSeconds, _startPos.Y,
                        _hiddenPos.Y - _startPos.Y, 100));
            }
            else
            {
                Position = new Vector2(Position.X,
                    CalcHelper.EaseInAndOut((float)_animationTimer.TimeElapsedInMilliSeconds, _startPos.Y,
                        _shownPos.Y - _startPos.Y, 100));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int cornerSize = CalcHelper.ApplyUiRatio(_sourceRectangles[0].Width);
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X, (int)Position.Y, cornerSize, cornerSize), _sourceRectangles[0], Color);
            int topBlankWidth = (int) (Size.X - cornerSize*2);
            spriteBatch.Draw(_texture, new Rectangle((int)(Position.X + cornerSize), (int)Position.Y, topBlankWidth, cornerSize), _sourceRectangles[1], Color);
            spriteBatch.Draw(_texture, new Rectangle((int)(Position.X + cornerSize + topBlankWidth), (int)Position.Y, cornerSize, cornerSize), _sourceRectangles[2], Color);

            int midBlankHeight = (int) (Size.Y - cornerSize*2);
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X,(int)(Position.Y + cornerSize), (int)Size.X, midBlankHeight),_sourceRectangles[3], Color );

            int yBot = (int) (Position.Y + midBlankHeight + cornerSize);
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X, yBot, cornerSize, cornerSize), _sourceRectangles[4], Color);
            spriteBatch.Draw(_texture, new Rectangle((int)(Position.X + cornerSize), yBot, topBlankWidth, cornerSize), _sourceRectangles[5], Color);
            spriteBatch.Draw(_texture, new Rectangle((int)(Position.X + cornerSize + topBlankWidth), yBot, cornerSize, cornerSize), _sourceRectangles[6], Color);
        }

    }
}
