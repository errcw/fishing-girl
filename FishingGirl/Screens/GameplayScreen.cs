using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

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
            
            _scene = new Scene();
            _fishing = new FishingState(this, _scene);
            _ocean = new Ocean(_fishing);
            _money = new Money(_fishing);

            _guideView = new GuideView();
            _guide = new Guide(_guideView, this, _fishing);

            _cameraController = new CameraController(_camera, _fishing);

            _state = GameState.Story;
        }

        /// <summary>
        /// Loads the content for this screen.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _guideView.LoadContent(content);

            _scene.LoadContent(content);
            _fishing.LoadContent(content);
            _ocean.LoadContent(content);

            // create the views after loading the content
            _sceneView = new SceneView(_scene, _camera);
            _oceanView = new OceanView(_ocean);
            _fishingView = new FishingView(_fishing, _context);
            _fishCaughtView = new FishCaughtView(_fishing);
            _fishEatenView = new FishEatenView(_fishing);

            _distanceView = new DistanceView(_scene, _fishing);
            _distanceView.LoadContent(content);

            _moneyView = new MoneyView(_money);
            _moneyView.LoadContent(content);

            // load the other necessary support bits
            _transitionScreen = new TransitionScreen();
            _transitionScreen.LoadContent(content);
            _pauseScreen = new PauseMenuScreenFactory().Create(_context, content);

            _oceanSong = content.Load<Song>("Sounds/Ocean");
            MediaPlayer.Volume = 0.1f;
        }

        /// <summary>
        /// Draws this game.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None, _camera.Transform);
            _sceneView.Draw(spriteBatch);
            if (_state == GameState.Game)
            {
                _fishingView.Draw(spriteBatch);
                _fishCaughtView.Draw(spriteBatch);
                _fishEatenView.Draw(spriteBatch);
                _distanceView.Draw(spriteBatch);
            }
            _oceanView.Draw(spriteBatch);
            spriteBatch.End();

            if (_state == GameState.Game)
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
                _guideView.Draw(spriteBatch);
                _moneyView.Draw(spriteBatch);
                spriteBatch.End();
            }
        }

        /// <summary>
        /// Starts/resumes the background noises.
        /// </summary>
        protected override void Show(bool pushed)
        {
            base.Show(pushed);
            if (pushed)
            {
                MediaPlayer.Play(_oceanSong);
            }
            else
            {
                MediaPlayer.Resume();
            }
        }

        /// <summary>
        /// Pauses the background noises.
        /// </summary>
        /// <param name="popped"></param>
        protected override void Hide(bool popped)
        {
            base.Hide(popped);
            if (!popped && _state == GameState.Game)
            {
                MediaPlayer.Pause();
            }
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

                _ocean.Update(time);
                _oceanView.Update(time);
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
            _sceneView.Update(time);
            _ocean.Update(time);
            _oceanView.Update(time);
            _fishing.Update(time, _context.Input);
            _fishingView.Update(time);
            _fishCaughtView.Update(time);
            _fishEatenView.Update(time);
            _distanceView.Update(time);
            _moneyView.Update(time);

            _guide.Update(time);
            _guideView.Update(time);
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
        private SceneView _sceneView;

        private Ocean _ocean;
        private OceanView _oceanView;

        private FishingState _fishing;
        private FishingView _fishingView;
        private FishCaughtView _fishCaughtView;
        private FishEatenView _fishEatenView;
        private DistanceView _distanceView;

        private Money _money;
        private MoneyView _moneyView;

        private Guide _guide;
        private GuideView _guideView;

        private TransitionScreen _transitionScreen;
        private MenuScreen _pauseScreen;

        private Song _oceanSong;

        private FishingGameContext _context;
    }
}
