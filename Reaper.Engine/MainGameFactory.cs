﻿namespace Reaper.Engine
{
    public static class MainGameFactory
    {
        public static IGame Create(GameSettings settings) 
        {
            return new MainGame(settings);
        }
    }
}