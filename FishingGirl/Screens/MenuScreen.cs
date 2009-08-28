using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Extensions;
using Library.Screen;
using Library.Sprite;
using Library.Sprite.Pipeline;

using FishingGirl.Properties;

namespace FishingGirl.Screens
{
    /// <summary>
    /// A menu screen.
    /// </summary>
    public class MenuScreen : Screen
    {
        /// <summary>
        /// If this screen is the root menu of a tree.
        /// </summary>
        public bool IsRoot { get; set; }

        /// <summary>
        /// The number of entries to display on each screen before scrolling.
        /// </summary>
        protected int NumVisibleEntries { get; set; }

        /// <summary>
        /// If the back button should be displayed if this is a root menu screen.
        /// </summary>
        protected bool ShowBackOnRoot { get; set; }

        /// <summary>
        /// The vertical padding, in pixels, between menu entries.
        /// </summary>
        protected float Spacing { get; set; }

        /// <summary>
        /// Creates a new menu screen.
        /// </summary>
        public MenuScreen(FishingGameContext context)
        {
            _context = context;

            ShowBeneath = true;
            TransitionOnTime = 0.4f;
            TransitionOffTime = 0.2f;
            NumVisibleEntries = 5;
            Spacing = 10f;
        }

        /// <summary>
        /// Loads the content for this screen.
        /// </summary>
        public virtual void LoadContent(ContentManager content)
        {
            _screenDescriptor = content.Load<SpriteDescriptorTemplate>("Sprites/MenuScreen").Create();
            _screenDescriptor.GetSprite<TextSprite>("TextSelect").Text = Resources.MenuSelect;
            _screenDescriptor.GetSprite<TextSprite>("TextBack").Text = Resources.MenuBack;
            _entriesSprite = _screenDescriptor.GetSprite<CompositeSprite>("Entries");
            _soundMove = content.Load<SoundEffect>("Sounds/MenuMove");
        }

        /// <summary>
        /// Adds a menu entry to this menu.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        public void AddEntry(MenuEntry entry)
        {
            _entries.Add(entry);
            if (_visibleEntries.Count < NumVisibleEntries)
            {
                ShowEntry(entry, _visibleEntries.Count);
            }
        }

        /// <summary>
        /// Removes a menu entry from this menu.
        /// </summary>
        /// <param name="entry">The entry to remove.</param>
        /// <returns>True if the entry is successfully removed; otherwise, false.</returns>
        public bool RemoveEntry(MenuEntry entry)
        {
            MenuEntry selectedEntry = _entries[_selectedEntryAbs];
            bool removed = _entries.Remove(entry);
            if (removed)
            {
                bool visRemoved = _visibleEntries.Remove(entry);
                if (visRemoved)
                {
                    _entriesSprite.Remove(entry.Sprite);
                }
                if (entry == selectedEntry)
                {
                    SetSelected(0); // move the focus off the now-defunct entry
                }
                else
                {
                    _selectedEntryAbs = MathHelperExtensions.Clamp(_selectedEntryAbs - 1, 0, _entries.Count); // fix the index
                    if (visRemoved)
                    {
                        _selectedEntryRel = MathHelperExtensions.Clamp(_selectedEntryRel - 1, 0, _visibleEntries.Count);
                    }
                }
            }
            return removed;
        }

        /// <summary>
        /// Removes all the entries (including decoration) from this menu.
        /// </summary>
        public void ClearEntries()
        {
            _entries.ForEach(entry => _entriesSprite.Remove(entry.Sprite));
            _visibleEntries.Clear();
            _entries.Clear();
            _selectedEntryAbs = 0;
            _selectedEntryRel = 0;
            _listWindowBaseIndex = 0;
        }

        /// <summary>
        /// Centers the menu entry sprites vertically and horizontally.
        /// </summary>
        public void LayoutEntries()
        {
            Vector2 menuSize = _screenDescriptor.GetSprite("Frame").Size;

            float height = 0f;
            foreach (MenuEntry entry in _visibleEntries)
            {
                height += entry.Sprite.Size.Y + Spacing;
            }
            height -= Spacing; // remove the extra padding at the bottom

            float y = (menuSize.Y - height) / 2f;
            foreach (MenuEntry entry in _visibleEntries)
            {
                float x = (menuSize.X - entry.Sprite.Size.X) / 2f;
                entry.Sprite.Position = new Vector2((int)x, (int)y);
                y += entry.Sprite.Size.Y + Spacing;
            }
        }

        /// <summary>
        /// Adds the given sprite as a decorative entry.
        /// </summary>
        /// <param name="sprite">The sprite to add.</param>
        public void AddDecoration(Sprite sprite)
        {
            _entriesSprite.Add(sprite);
        }

        /// <summary>
        /// Draws this screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            _screenDescriptor.Sprite.Draw(spriteBatch);
            spriteBatch.End();
        }

        /// <summary>
        /// Focuses the first entry by default.
        /// </summary>
        protected override void Show(bool pushed)
        {
            base.Show(pushed);
            _screenDescriptor.GetSprite("Back").Color = IsRoot && !ShowBackOnRoot ? Color.TransparentWhite : Color.White;
            _screenDescriptor.GetSprite("Select").Color = Color.White;
            _screenDescriptor.GetSprite("ArrowUp").Color = Color.TransparentWhite;
            _screenDescriptor.GetSprite("ArrowDown").Color = Color.TransparentWhite;
            if (pushed)
            {
                // reset the list
                _entries[_selectedEntryAbs].OnFocusChanged(false);
                _visibleEntries.ToArray().ForEach(e => HideEntry(e));
                _visibleEntries.Clear();
                for (int i = 0; i < Math.Min(_entries.Count, NumVisibleEntries); i++)
                {
                    ShowEntry(_entries[i], _visibleEntries.Count);
                }
                LayoutEntries();
                _selectedEntryRel = _selectedEntryAbs = _listWindowBaseIndex = 0;
                SetSelected(0);
            }
        }

        /// <summary>
        /// Unfocuses the selected entry.
        /// </summary>
        protected override void Hide(bool popped)
        {
            base.Hide(popped);
            if (popped)
            {
                _entries[_selectedEntryRel].OnFocusChanged(false);
            }
            _screenDescriptor.GetSprite("Select").Color = Color.TransparentWhite;
            _screenDescriptor.GetSprite("Back").Color = Color.TransparentWhite;
        }

        /// <summary>
        /// Updates this screen.
        /// </summary>
        protected override void UpdateActive(float time)
        {
            if (_context.Input.Cancel.Pressed)
            {
                Stack.Pop();
            }

            int delta = 0;
            if (_context.Input.Up.PressedRepeat)
            {
                delta = -1;
            }
            else if (_context.Input.Down.PressedRepeat)
            {
                delta = 1;
            }
            if (delta != 0)
            {
                SetSelected(delta);
            }

            if (_context.Input.Action.Pressed && _entries[_selectedEntryAbs].IsSelectable)
            {
                _entries[_selectedEntryAbs].OnSelected();
            }

            _entries[_selectedEntryAbs].Update(time);
        }

        /// <summary>
        /// Fades this menu in.
        /// </summary>
        protected override void UpdateTransitionOn(float time, float progress, bool pushed)
        {
            // let the old menu fade out first
            if (progress < 0.5)
            {
                return;
            }
            progress = (progress - 0.5f) * 2f;
            // then fade in this one
            _entriesSprite.Color = new Color(Color.White, progress);
            if (IsRoot && pushed)
            {
                _screenDescriptor.GetSprite("Background").Color = new Color(Color.White, progress);
            }
        }

        /// <summary>
        /// Fades this menu out.
        /// </summary>
        protected override void UpdateTransitionOff(float time, float progress, bool popped)
        {
            _entriesSprite.Color = new Color(Color.White, 1 - progress);
            if (IsRoot && popped)
            {
                // fade out the background when the last menu screen is popped off
                _screenDescriptor.GetSprite("Background").Color = new Color(Color.White, 1 - progress);
            }
        }

        /// <summary>
        /// Sets the selected menu item.
        /// </summary>
        /// <param name="deltaIdx">The change in selected index.</param>
        protected virtual void SetSelected(int deltaIdx)
        {
            int selected = _selectedEntryAbs;

            _entries[_selectedEntryAbs].OnFocusChanged(false);

            int nextRelEntry = _selectedEntryRel + deltaIdx;
            int nextAbsEntry = _selectedEntryAbs + deltaIdx;

            if (nextRelEntry >= _visibleEntries.Count && nextAbsEntry < _entries.Count)
            {
                HideEntry(_visibleEntries[0]);
                ShowEntry(_entries[nextAbsEntry], _visibleEntries.Count);
                LayoutEntries();
                _listWindowBaseIndex += 1;
            }
            else if (nextRelEntry < 0 && nextAbsEntry >= 0)
            {
                HideEntry(_visibleEntries[_visibleEntries.Count - 1]);
                ShowEntry(_entries[nextAbsEntry], 0);
                LayoutEntries();
                _listWindowBaseIndex -= 1;
            }

            _selectedEntryRel = MathHelperExtensions.Clamp(nextRelEntry, 0, _visibleEntries.Count - 1);
            _selectedEntryAbs = MathHelperExtensions.Clamp(nextAbsEntry, 0, _entries.Count - 1);

            if (_entries.Count > NumVisibleEntries)
            {
                _screenDescriptor.GetSprite("ArrowUp").Color =
                    (_listWindowBaseIndex == 0) ? Color.TransparentWhite : Color.White;
                _screenDescriptor.GetSprite("ArrowDown").Color =
                    (_listWindowBaseIndex == _entries.Count - NumVisibleEntries) ? Color.TransparentWhite : Color.White;
            }
            _screenDescriptor.GetSprite("Select").Color =
                _entries[_selectedEntryAbs].IsSelectable ? Color.White : Color.TransparentWhite;
            _screenDescriptor.GetSprite<TextSprite>("TextSelect").Text =
                _entries[_selectedEntryAbs].SelectText;

            _entries[_selectedEntryAbs].OnFocusChanged(true);

            if (selected != _selectedEntryAbs)
            {
                _soundMove.Play();
            }
        }

        /// <summary>
        /// Shows the specified menu entry at a specific position.
        /// </summary>
        private void ShowEntry(MenuEntry entry, int position)
        {
            _visibleEntries.Insert(position, entry);
            _entriesSprite.Add(entry.Sprite);
        }

        /// <summary>
        /// Hides the specified menu entry.
        /// </summary>
        private void HideEntry(MenuEntry entry)
        {
            _visibleEntries.Remove(entry);
            _entriesSprite.Remove(entry.Sprite);
        }

        protected SpriteDescriptor _screenDescriptor;
        private CompositeSprite _entriesSprite;
        private SoundEffect _soundMove;

        protected List<MenuEntry> _entries = new List<MenuEntry>();
        private List<MenuEntry> _visibleEntries = new List<MenuEntry>();
        private int _selectedEntryRel; /// relative index inside visible entries
        private int _selectedEntryAbs; /// absolute index inside all entries
        private int _listWindowBaseIndex; /// index of top entry

        protected FishingGameContext _context;
    }

    /// <summary>
    /// An entry in the menu.
    /// </summary>
    public class MenuEntry
    {
        /// <summary>
        /// The sprite used to display this menu entry.
        /// </summary>
        public Sprite Sprite { get; protected set; }

        /// <summary>
        /// If this entry can be selected.
        /// </summary>
        public bool IsSelectable { get; set; }

        /// <summary>
        /// A verb to describe the action of this menu entry.
        /// </summary>
        public string SelectText { get; set; }

        /// <summary>
        /// Invoked when this menu entry is selected.
        /// </summary>
        public event EventHandler<EventArgs> Selected;

        /// <summary>
        /// Creates a new menu entry.
        /// </summary>
        /// <param name="sprite">The sprite to show.</param>
        public MenuEntry(Sprite sprite)
        {
            Sprite = sprite;
            IsSelectable = true;
            SelectText = Resources.MenuSelect;
        }

        /// <summary>
        /// Updates this menu entry when it is focused.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public virtual void Update(float time)
        {
        }

        /// <summary>
        /// Notifies this menu entry that is has been selected.
        /// </summary>
        public virtual void OnSelected()
        {
            if (IsSelectable && Selected != null)
            {
                Selected(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Notifies this menu entry that its focused state changed.
        /// </summary>
        /// <param name="focused">True if the entry is focused; otherwise, false.</param>
        public virtual void OnFocusChanged(bool focused)
        {
        }
    }
}
