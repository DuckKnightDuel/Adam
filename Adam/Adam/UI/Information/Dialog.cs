﻿using Adam.Misc;
using Adam.Misc.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adam.UI
{
    public class Dialog
    {
        Texture2D texture;
        SpriteFont font;
        Rectangle drawRectangle;
        Vector2 origin;

        bool isActive = false;
        string text = "";
        StringBuilder sb;
        SoundFx popSound;

        public delegate void EventHandler();
        public event EventHandler NextDialog;
        public event EventHandler CancelDialog;

        float opacity = 0;
        double skipTimer;

        int originalY;

        public Dialog()
        {
            texture = ContentHelper.LoadTexture("Menu/dialog_box");
            drawRectangle = new Rectangle(Game1.UserResWidth / 2, 40, texture.Width * 2, texture.Height * 2);
            origin = new Vector2(drawRectangle.Width / 2, drawRectangle.Height / 2);
            drawRectangle.X -= (int)origin.X;

            originalY = drawRectangle.Y;
            drawRectangle.Y -= 40;

            font = ContentHelper.LoadFont("Fonts/dialog");
            popSound = new SoundFx("Sounds/message_show");
        }

        public void Say(string text)
        {
            isActive = true;
            this.text = FontHelper.WrapText(font, text, drawRectangle.Width - 60);
            skipTimer = 0;
            opacity = 0;
            drawRectangle.Y -= 40;
            popSound.Reset();
        }

        public void Cancel()
        {
            isActive = false;
        }

        public void Update(GameTime gameTime)
        {
            float deltaOpacity = .03f;
            if (isActive)
            {
                popSound.PlayOnce();
                skipTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (skipTimer > .5)
                {
                    if (InputHelper.IsLeftMousePressed())
                    {
                        isActive = false;
                        NextDialog();
                    }
                    if (InputHelper.IsAnyInputPressed() && InputHelper.IsLeftMouseReleased())
                    {
                        isActive = false;
                        CancelDialog();
                    }
                }
            }

            if (isActive)
            {
                float velocity =(originalY - drawRectangle.Y) / 10;
                drawRectangle.Y += (int)velocity;
                opacity += deltaOpacity;
            }
            else
            {
                float velocity = -3f;
                opacity -= deltaOpacity;
                drawRectangle.Y += (int)velocity;
                skipTimer = 0;
            }

            if (opacity > 1)
                opacity = 1;
            if (opacity < 0)
                opacity = 0;
            if (drawRectangle.Y < -100)
                drawRectangle.Y = -100;
        }

        public void Draw(SpriteBatch spriteBatch)
        {            
            spriteBatch.Draw(texture, drawRectangle, Color.White * opacity);
            spriteBatch.DrawString(font, text, new Vector2(drawRectangle.X + 30, drawRectangle.Y + 30), Color.Black * opacity);
        }

    }
}