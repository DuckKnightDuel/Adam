﻿using Adam;
using Adam.Misc.Helpers;
using Adam.UI;
using Adam.UI.Overlay_Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adam
{
    class Overlay
    {
        public static SpriteFont Font;
        List<SplashDamage> splashDamages = new List<SplashDamage>();

        Heart heart;
        //Time is currently disabled.
        Hourglass hourglass;
        Coin coin;

        public Overlay()
        {
            Font = ContentHelper.LoadFont("Fonts/overlay");

            heart = new Heart(new Vector2(40,40));
            coin = new Coin(new Vector2(40, 120));
        }

        public void Update(GameTime gameTime, Player player, GameWorld map)
        {
            heart.Update(gameTime, player, map, splashDamages);
            coin.Update(player, gameTime);

            foreach (var spl in splashDamages)
            {
                spl.Update(gameTime);
            }
            foreach (var spl in splashDamages)
            {
                if (spl.toDelete)
                {
                    splashDamages.Remove(spl);
                    break;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            heart.Draw(spriteBatch);
            coin.Draw(spriteBatch);

            foreach (var spl in splashDamages)
            {
                spl.Draw(spriteBatch);
            }
        }
    }



}
