﻿using Adam.Network;

namespace Adam.Characters
{
    public abstract class Character : Entity
    {
        protected Script Script;

        public override void Update()
        {
            if (Session.IsActive && Session.IsHost && Main.CurrentGameMode == GameMode.Play)
                Script?.Run();
            base.Update();
        }

    }
}