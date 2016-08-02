﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adam.Levels;
using Adam.Misc.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Adam.UI.Elements
{
    public class YesButton : Button
    {
        MessageBox _sender;

        public YesButton(Rectangle containerRectangle, MessageBox sender)
        {
            Text = "Yes";
            int width = 19 * 5;
            int height = 6 * 5;
            int x = containerRectangle.X + containerRectangle.Width / 2;
            int y = containerRectangle.Y + containerRectangle.Height;
            CollRectangle = new Rectangle(x - width / 2, y - height - 20, width, height);
            this._sender = sender;
            Initialize();
        }

        protected void Initialize()
        {
            MouseHover += OnMouseHover;
            MouseOut += OnMouseOut;
            MouseClicked += YesButton_MouseClicked;
            SourceRectangle = new Rectangle(320, 20, 19, 6);
            Texture = GameWorld.SpriteSheet;
            Font = ContentHelper.LoadFont("Fonts/x32");
        }

        private void YesButton_MouseClicked()
        {
            _sender.IsActive = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, CollRectangle, SourceRectangle, Color);
            spriteBatch.DrawString(Font, Text, new Vector2(CollRectangle.Center.X, CollRectangle.Center.Y),
                Color.White, 0, Font.MeasureString(Text) / 2, (float)(.5 / Main.HeightRatio), SpriteEffects.None, 0);

        }
    }

    public class OkButton : Button
    {
        public OkButton(Rectangle containerRectangle)
        {
            Text = "OK";
            int width = 19 * 5;
            int height = 6 * 5;
            ContainerDiff = new Rectangle(containerRectangle.Width/2, containerRectangle.Y/2,0,0);
            int x = containerRectangle.X + ContainerDiff.X;
            int y = containerRectangle.Y + ContainerDiff.Y;
            CollRectangle = new Rectangle(x - width / 2, y - height - 20, width, height);
            Initialize();
        }

        protected void Initialize()
        {
            MouseHover += OnMouseHover;
            MouseOut += OnMouseOut;
            SourceRectangle = new Rectangle(320, 20, 19, 6);
            Font = ContentHelper.LoadFont("Fonts/x32");
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GameWorld.UiSpriteSheet, CollRectangle, SourceRectangle, Color);
            spriteBatch.DrawString(Font, Text, new Vector2(CollRectangle.Center.X, CollRectangle.Center.Y),
                Color.White, 0, Font.MeasureString(Text) / 2, (float)(.5 / Main.HeightRatio), SpriteEffects.None, 0);
        }
    }
}
