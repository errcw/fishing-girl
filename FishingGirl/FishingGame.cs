using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

using Library.Components;
using Library.Screen;
using Library.Storage;
using Library.Extensions;

using FishingGirl.Screens;
using FishingGirl.Properties;

namespace FishingGirl
{
    /// <summary>
    /// The game scaffolding.
    /// </summary>
    public class FishingGame : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// Creates a new game.
        /// </summary>
        public FishingGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Window.Title = Resources.FishingGirl;

            Components.Add(new GamerServicesComponent(this));
            Components.Add(_trial = new TrialModeObserverComponent(this));
            Components.Add(_input = new Input(this));
            Components.Add(_screens = new ScreenStack(this));
            //Components.Add(new FPSOverlay(this){FontName=@"Fonts\Text"});
            //Components.Add(new UnsafeAreaOverlayComponent(this));

            Content.RootDirectory = "Content";

#if WINDOWS && DEBUG
            Guide.SimulateTrialMode = true;
#endif
        }

        /// <summary>
        /// Loads the content for all the game components.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            FishingGameContext context = new FishingGameContext();
            context.Game = this;
            context.Input = _input;
            context.Trial = _trial;

            GameplayScreen gameplayScreen = new GameplayScreen(context);
            gameplayScreen.LoadContent(Content);
            TitleScreen titleScreen = new TitleScreen(context);
            titleScreen.LoadContent(Content);

            _screens.Push(gameplayScreen);
            _screens.Push(titleScreen);
        }

        /// <summary>
        /// Updates the game state.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_screens.ActiveScreen == null)
            {
                Exit();
            }
        }

        /// <summary>
        /// Draws the game to the screen.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>     
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }

        private ScreenStack _screens;
        private Input _input;
        private TrialModeObserverComponent _trial;
    }
}
