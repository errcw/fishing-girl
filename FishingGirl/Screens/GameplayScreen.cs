﻿using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

using Library.Screen;
using Library.Sprite;
using Library.Sprite.Pipeline;
using Library.Storage;

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
            _context.Game.Exiting += (s, a) => ExitGame();
            _context.Trial.TrialModeEnded += (s, a) => ConfigureStorage();

            _camera = new CameraSprite(_context.Game.GraphicsDevice);
            _state = GameState.Story;
        }

        /// <summary>
        /// Loads the content for the game.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _badges = new Badges();
            _badgeView = new BadgeView(_badges);
            _badgeView.LoadContent(content);

            _options = new Options();

            _storeScreen = new StoreScreen(_context);
            _storeScreen.LoadContent(content);

            _transitionScreen = new TransitionScreen();
            _transitionScreen.LoadContent(content);

            _endScreen = new GameOverScreen(_context);
            _endScreen.LoadContent(content);

            _pauseScreen = new PauseMenuScreenFactory().Create(_options, _badges, _context, content);

            _oceanSong = content.Load<Song>("Sounds/Ocean");
            MediaPlayer.IsRepeating = true;

            StartGame(content);
        }

        /// <summary>
        /// Draws this game.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_state)
            {
                case GameState.Transition: goto case GameState.Story;
                case GameState.Story:
                    DrawStory(spriteBatch);
                    break;

                default:
                    DrawGame(spriteBatch);
                    break;
            }
        }

        /// <summary>
        /// Draws all the items in the story sequence.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        private void DrawStory(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None, _camera.Transform);
            _sceneView.Draw(spriteBatch);
            _oceanView.Draw(spriteBatch);
            spriteBatch.End();
        }

        /// <summary>
        /// Draws the all items in the game and interface.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        private void DrawGame(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None, _camera.Transform);
            _sceneView.Draw(spriteBatch);
            _fishingView.Draw(spriteBatch);
            _fishCaughtView.Draw(spriteBatch);
            _fishEatenView.Draw(spriteBatch);
            _distanceView.Draw(spriteBatch);
            _oceanView.Draw(spriteBatch);
            _storeView.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            _guideView.Draw(spriteBatch);
            _moneyView.Draw(spriteBatch);
            _lureView.Draw(spriteBatch);
            _badgeView.Draw(spriteBatch);
            if (_options.TimerToggle)
            {
                _timerView.Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        /// <summary>
        /// Triggers the appropriate actions when this screen is displayed.
        /// </summary>
        protected override void Show(bool pushed)
        {
            base.Show(pushed);
            if (_state == GameState.Ended)
            {
                // start a new game when this screen is shown again
                _state = GameState.Game;
                StartGame(_context.Game.Content);
            }
            else
            {
                if (pushed)
                {
                    MediaPlayer.Play(_oceanSong);
                }
                else
                {
                    MediaPlayer.Resume();
                }
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
                    ConfigureStorage();
                    break;

                case GameState.Game:
                    UpdateGame(time);
                    break;

                case GameState.EndStory:
                    UpdateGameOver(time);
                    break;
            }
        }

        /// <summary>
        /// Updates the game in the background.
        /// </summary>
        protected override void UpdateInactive(float time)
        {
            if (_state != GameState.Story && _state != GameState.Transition)
            {
                return;
            }
            _scene.UpdateStory(time);
            _ocean.Update(time);
            _oceanView.Update(time);
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
            _lureView.Update(time);

            _moneyView.Update(time);

            _store.Update(time);
            _storeView.Update(time);

            _badges.Update(time);
            _badgeView.Update(time);

            _guide.Update(time);
            _guideView.Update(time);

            if (_options.TimerToggle)
            {
                if (_fishing.Action != FishingAction.Island)
                {
                    // stop the timer once the lure hits the island
                    _timer.Update(time);
                }
                _timerView.Update(time);

                if (_timer.Time <= 0f)
                {
                    EndGame(false);
                    return;
                }
            }

            if (_scene.FarShore.X - _scene.NearShore.X <= EndingStoryCloseThreshold)
            {
                _state = GameState.EndStory;
                _scene.StartEnding();
            }
        }

        /// <summary>
        /// Updates the win animation for this frame.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        private void UpdateGameOver(float time)
        {
            _cameraController.Update(time);

            if (!_scene.UpdateEnding(time))
            {
                EndGame(true);
                return;
            }
            _sceneView.Update(time);

            //??? _badges.Update(time);
            //??? _badgeView.Update(time);

            _ocean.Update(time);
            _oceanView.Update(time);

            _fishing.Update(time, _context.Input);
            _fishingView.UpdateHide(time);
        }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        /// <param name="content">The content manager to load from.</param>
        private void StartGame(ContentManager content)
        {
            // create the game objects
            _scene = new Scene();
            _scene.LoadContent(content);

            _fishing = new FishingState(this, _scene);
            _fishing.LoadContent(content);

            _ocean = new Ocean(_fishing);
            _ocean.LoadContent(content);

            _money = new Money(_fishing);

            _timer = new Timer(_fishing);

            _store = new Store(_money, _fishing);
            _store.LoadContent(content);
            _store.Hit += OnStoreHit;

            BadgeContext badgeContext = new BadgeContext();
            badgeContext.Fishing = _fishing;
            badgeContext.Money = _money;
            badgeContext.Store = _store;
            badgeContext.Timer = _timer;
            _badges.Context = badgeContext;

            // create the views
            _sceneView = new SceneView(_scene, _camera);
            _sceneView.LoadContent(content);

            _oceanView = new OceanView(_ocean);

            _fishCaughtView = new FishCaughtView(_fishing);
            _fishEatenView = new FishEatenView(_fishing);
            _fishingView = new FishingView(_fishing, _context);
            _fishingView.LoadContent(content);

            _distanceView = new DistanceView(_scene, _fishing);
            _distanceView.LoadContent(content);

            _moneyView = new MoneyView(_money);
            _moneyView.LoadContent(content);

            _timerView = new TimerView(_timer);
            _timerView.LoadContent(content);

            _lureView = new LureView(_fishing, _store);
            _lureView.LoadContent(content);

            _storeView = new StoreView(_store);
            _storeView.LoadContent(content);

            _guideView = new GameGuideView();
            _guideView.LoadContent(content);
            _guide = new GameGuide(_guideView, _fishing, _store, _money);

            _cameraController = new CameraController(_camera, _scene, _fishing);
        }

        /// <summary>
        /// Pauses the game and displays the pause menu.
        /// </summary>
        private void PauseGame()
        {
            Stack.Push(_pauseScreen);
            MediaPlayer.Pause();
        }

        /// <summary>
        /// Starts the end-of-game screen sequence.
        /// </summary>
        /// <param name="won">True if the current game was won; otherwise, false.</param>
        private void EndGame(bool won)
        {
            _state = GameState.Ended;
            _endScreen.IsWon = won;
            Stack.Push(_endScreen);
        }

        /// <summary>
        /// Saves the current state when the game is exiting.
        /// </summary>
        private void ExitGame()
        {
            // save the state if possible
            if (_context.Storage != null && _context.Storage.IsValid)
            {
                _options.Save(_context.Storage);
                if (!Guide.IsTrialMode)
                {
                    _badges.Save(_context.Storage);
                }
            }
        }

        /// <summary>
        /// Sets up the storage.
        /// </summary>
        private void ConfigureStorage()
        {
            if (_context.Storage == null && _context.Input.Controller.HasValue)
            {
                _context.Storage = new PlayerStorage(_context.Game, "FishingGirl", _context.Input.Controller.Value);
                _context.Storage.DeviceSelected += (o, a) =>
                {
                    _options.Load(_context.Storage);
                    _badges.Load(_context.Storage);
                };
                _context.Storage.PromptForDevice();

                _context.Game.Components.Add(_context.Storage);
            }
        }

        /// <summary>
        /// Watches for the store screen.
        /// </summary>
        private void OnStoreHit(object store, EventArgs args)
        {
            _storeScreen.Store = (Store)store;
            Stack.Push(_storeScreen);
        }

        /// <summary>
        /// The state/stage of the game.
        /// </summary>
        private enum GameState
        {
            Story,
            Transition,
            Game,
            EndStory,
            Ended,
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
        private LureView _lureView;

        private Store _store;
        private StoreView _storeView;

        private Money _money;
        private MoneyView _moneyView;

        private Timer _timer;
        private TimerView _timerView;

        private GameGuide _guide;
        private GameGuideView _guideView;

        private Badges _badges;
        private BadgeView _badgeView;

        private StoreScreen _storeScreen;
        private TransitionScreen _transitionScreen;
        private MenuScreen _pauseScreen;
        private GameOverScreen _endScreen;

        private Song _oceanSong;

        private Options _options;
        private FishingGameContext _context;

        private const float EndingStoryCloseThreshold = 150f;
    }
}
