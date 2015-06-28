﻿using Adam.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adam.Interactables
{
    class Sign : Entity
    {
        KeyPopUp key;
        int ID;
        bool playerIsOn;

        public Sign(int xCoor, int yCoor, int ID)
        {
            key = new KeyPopUp();
            collRectangle = new Rectangle(xCoor, yCoor, Game1.Tilesize, Game1.Tilesize);
            this.ID = ID;
        }

        public override void Update()
        {
            key.Update(collRectangle);
            if (GameWorld.Instance.player.collRectangle.Intersects(collRectangle))
            {
                if (InputHelper.IsKeyDown(Keys.W))
                {
                    ShowMessage();
                }
            }
        }

        private void ShowMessage()
        {
            Game1.Dialog.Say(GameWorld.Instance.worldData.GetSignMessage(ID));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            key.Draw(spriteBatch);
        }

    }
}
