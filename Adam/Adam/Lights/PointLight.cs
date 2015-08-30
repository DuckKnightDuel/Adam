﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adam.Lights
{
    /// <summary>
    /// Used to create a simple light that shines equally in all directions and follows an entity.
    /// </summary>
    public class DynamicPointLight : Light
    {

        public DynamicPointLight(Entity source, float? scale, bool isShaky, Color? color, float glowIntensity)
        {
            if (scale.HasValue)
                size = (int)(DefaultSize * scale);

            if (color.HasValue)
                this.color = color.Value;

            this.glowIntensity = glowIntensity;
            this.isShaky = isShaky;

            SetPosition(source.GetCollRectangle());
            glow = new Glow(this);
            this.source = source;

            lightHere = true;

        }

        public override void Update(Entity source)
        {
            SetPosition(source.GetCollRectangle());

            if (isShaky) Shake();

            base.Update();
        }
    }

    /// <summary>
    /// Used to create a simple light that shines equally in all directions and stays fixed.
    /// </summary>
    public class FixedPointLight : Light
    {
        public FixedPointLight(Rectangle tileRectangle, bool isShaky, Color color, float? scale, float glowIntensity)
        {
            this.isShaky = isShaky;
            this.color = color;

            if (scale.HasValue)
                size = (int)(DefaultSize * scale);

            SetPosition(tileRectangle);
            this.glowIntensity = glowIntensity;
            glow = new Glow(this);

            lightHere = true;
        }

        public override void Update()
        {
            if (isShaky) Shake();

            base.Update();
        }
    }
}
