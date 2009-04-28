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
        }

        /// <summary>
        /// Initialises this game.
        /// </summary>
        protected override void Initialize()
        {
            GamerServicesComponent gamer = new GamerServicesComponent(this);
            Components.Add(gamer);
            StorageDeviceManager storage = new StorageDeviceManager(this);
            Components.Add(storage);
            TrialModeObserverComponent trial = new TrialModeObserverComponent(this);
            Components.Add(trial);

            _input = new Input();

            FishingGameContext context = new FishingGameContext(this, _input, storage, trial);

            _screens = new ScreenStack();
            _screens.Push(_gameplayScreen = new GameplayScreen(context));
            _screens.Push(_titleScreen = new TitleScreen(context));

            Guide.SimulateTrialMode = true;
            base.Initialize();
        }

        /// <summary>
        /// Loads the content for all the game components.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            Content.RootDirectory = "Content";
            _gameplayScreen.LoadContent(Content);
            _titleScreen.LoadContent(Content);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// Updates the game state.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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
        }

        /// <summary>
        /// Draws the game to the screen.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>     
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.Clear(Color.Black);
            _screens.Draw(_spriteBatch);
        }

        private SpriteBatch _spriteBatch;

        private ScreenStack _screens;
        private TitleScreen _titleScreen;
        private GameplayScreen _gameplayScreen;

        private Input _input;
    }
}
