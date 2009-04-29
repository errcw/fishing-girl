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
            Components.Add(_storage = new StorageDeviceManager(this));
            Components.Add(_trial = new TrialModeObserverComponent(this));
            //Components.Add(new FPSOverlay(this){FontName=@"Fonts\Text"});
            //Components.Add(new UnsafeAreaOverlayComponent(this));

            Content.RootDirectory = "Content";

            Guide.SimulateTrialMode = true;
        }

        /// <summary>
        /// Loads the content for all the game components.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            _input = new Input();
            FishingGameContext context = new FishingGameContext(this, _input, _storage, _trial);

            GameplayScreen gameplayScreen = new GameplayScreen(_context);
            gameplayScreen.LoadContent(Content);
            TitleScreen titleScreen = new TitleScreen(_context);
            titleScreen.LoadContent(Content);

            _screens = new ScreenStack();
            _screens.Push(gameplayScreen);
            _screens.Push(titleScreen);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// Updates the game state.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float time = gameTime.GetElapsedSeconds();
            if (time <= 0 || !IsActive)
            {
                return; // discard "empty" updates
            }

            _input.Update(time);
            _screens.Update(time);

            if (_screens.ActiveScreen == null)
            {
                Exit();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game to the screen.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>     
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _screens.Draw(_spriteBatch);

            base.Draw(gameTime);
        }

        private SpriteBatch _spriteBatch;

        private ScreenStack _screens;

        private StorageDeviceManager _storage;
        private TrialModeObserverComponent _trial;
        private Input _input;
    }
}
