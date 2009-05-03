using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Screen;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Gameplay;
using FishingGirl.Interface;

namespace FishingGirl.Screens
{
    /// <summary>
    /// A screen that shows the game.
    /// </summary>
    public class GameplayScreen : Screen
    {
        /// <summary>
        /// Creates a new gameplay screen.
        /// </summary>
        public GameplayScreen(FishingGameContext context)
        {
            _context = context;
            _context.Input.ControllerDisconnected += (s, a) => PauseGame();

            _camera = new CameraSprite(_context.Game.GraphicsDevice);
            
            _scene = new Scene(_camera);
            _fishing = new FishingState(this, _scene);
            _ocean = new Ocean(_fishing);

            _guideText = new GuideText(_camera);
            _guide = new Guide(_guideText, this, _fishing);

            _cameraController = new CameraController(_camera, _fishing);

            _state = GameState.Story;
        }

        /// <summary>
        /// Loads the content for this screen.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _transitionScreen = new TransitionScreen();
            _transitionScreen.LoadContent(content);
            _pauseScreen = new PauseMenuScreenFactory().Create(_context, content);

            _guideText.LoadContent(content);
            
            _scene.LoadContent(content);
            _fishing.LoadContent(content);
            _ocean.LoadContent(content);
        }

        /// <summary>
        /// Draws this game.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None, _camera.Transform);
            _scene.Draw(spriteBatch);
            _ocean.Draw(spriteBatch);
            _fishing.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            _guideText.Draw(spriteBatch);
            spriteBatch.End();
        }

        /// <summary>
        /// Updates the game state for this frame.
        /// </summary>
        protected override void UpdateActive(float time)
        {
            switch (_state)
            {
                case GameState.Story:
                    Stack.Push(_transitionScreen);
                    _state = GameState.Transition;
                    break;

                case GameState.Transition:
                    _scene.EndStory();
                    _state = GameState.Game;
                    break;

                case GameState.Game:
                    UpdateGame(time);
                    break;
            }
        }

        /// <summary>
        /// Updates the introduction animation.
        /// </summary>
        protected override void UpdateInactive(float time)
        {
            if (_state == GameState.Story || _state == GameState.Transition)
            {
                _scene.UpdateStory(time);
            }
        }

        /// <summary>
        /// Updates the gameplay for this frame.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        private void UpdateGame(float time)
        {
            if (_context.Input.Start.Pressed)
            {
                PauseGame();
                return;
            }

            _cameraController.Update(time);
            _scene.Update(time);
            _ocean.Update(time);
            _fishing.Update(time, _context.Input);

            _guide.Update(time);
            _guideText.Update(time);
        }

        /// <summary>
        /// Pauses the game and displays the pause menu.
        /// </summary>
        private void PauseGame()
        {
            Stack.Push(_pauseScreen);
        }

        /// <summary>
        /// The state of the game.
        /// </summary>
        private enum GameState
        {
            Story,
            Transition,
            Game
        }

        private GameState _state;

        private CameraSprite _camera;
        private CameraController _cameraController;

        private Scene _scene;
        private Ocean _ocean;
        private FishingState _fishing;

        private Guide _guide;
        private GuideText _guideText;

        private TransitionScreen _transitionScreen;
        private MenuScreen _pauseScreen;

        private FishingGameContext _context;
    }
}
