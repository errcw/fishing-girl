using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
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
        /// Creates a new menu screen.
        /// </summary>
        public MenuScreen(FishingGameContext context)
        {
            _context = context;
            ShowBeneath = true;
            TransitionOnTime = 0.4f;
            TransitionOffTime = 0.2f;
        }

        /// <summary>
        /// Loads the content for this screen.
        /// </summary>
        public virtual void LoadContent(ContentManager content)
        {
            _screenDescriptor = content.Load<SpriteDescriptorTemplate>("Sprites/MenuScreen").Create(content);
            _screenDescriptor.GetSprite<TextSprite>("TextSelect").Text = Resources.MenuSelect;
            _screenDescriptor.GetSprite<TextSprite>("TextBack").Text = Resources.MenuBack;
        }

        /// <summary>
        /// Adds a menu entry to this menu.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        public void AddEntry(MenuEntry entry)
        {
            _entries.Add(entry);
            _screenDescriptor.GetSprite<CompositeSprite>("Entries").Add(entry.Sprite);
        }

        /// <summary>
        /// Removes a menu entry from this menu.
        /// </summary>
        /// <param name="entry">The entry to remove.</param>
        /// <returns>True if the entry is successfully removed; otherwise, false.</returns>
        public bool RemoveEntry(MenuEntry entry)
        {
            MenuEntry selectedEntry = _entries[_selectedEntry];
            bool removed = _entries.Remove(entry);
            if (removed)
            {
                _screenDescriptor.GetSprite<CompositeSprite>("Entries").Remove(entry.Sprite);
                if (entry == selectedEntry)
                {
                    SetSelected(0); // move the focus off the now-defunct entry
                }
                else
                {
                    _selectedEntry -= 1; // fix the index
                }
            }
            return removed;
        }

        /// <summary>
        /// Centers the menu entry sprites vertically and horizontally.
        /// </summary>
        public void LayoutEntries()
        {
            Vector2 menuSize = _screenDescriptor.GetSprite("Frame").Size;

            float height = 0f;
            foreach (MenuEntry entry in _entries)
            {
                height += entry.Sprite.Size.Y + Spacing;
            }
            height -= Spacing; // remove the extra padding at the bottom

            float y = (menuSize.Y - height) / 2f;
            foreach (MenuEntry entry in _entries)
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
            _screenDescriptor.GetSprite<CompositeSprite>("Entries").Add(sprite);
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
            _screenDescriptor.GetSprite("Back").Color = IsRoot ? Color.TransparentWhite : Color.White;
            _screenDescriptor.GetSprite("Select").Color = Color.White;
            if (pushed)
            {
                _selectedEntry = 0;
                _entries[_selectedEntry].OnFocusChanged(true);
                _screenDescriptor.GetSprite("Select").Color = _entries[_selectedEntry].IsSelectable ? Color.White : Color.TransparentWhite;
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
                _entries[_selectedEntry].OnFocusChanged(false);
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

            if (_context.Input.Action.Pressed && _entries[_selectedEntry].IsSelectable)
            {
                _entries[_selectedEntry].OnSelected();
            }

            _entries[_selectedEntry].Update(time);
        }

        /// <summary>
        /// Fades the menu in.
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
            _screenDescriptor.GetSprite("Entries").Color = new Color(Color.White, progress);
            if (IsRoot && pushed)
            {
                _screenDescriptor.GetSprite("Background").Color = new Color(Color.White, progress);
            }
        }

        /// <summary>
        /// Fades the menu out.
        /// </summary>
        protected override void UpdateTransitionOff(float time, float progress, bool popped)
        {
            _screenDescriptor.GetSprite("Entries").Color = new Color(Color.White, 1 - progress);
            if (IsRoot && popped)
            {
                _screenDescriptor.GetSprite("Background").Color = new Color(Color.White, 1 - progress);
            }
        }

        /// <summary>
        /// Sets the selected menu item.
        /// </summary>
        /// <param name="deltaIdx">The change in selected index.</param>
        protected virtual void SetSelected(int deltaIdx)
        {
            _entries[_selectedEntry].OnFocusChanged(false);
            _selectedEntry = MathHelperExtensions.Clamp(_selectedEntry + deltaIdx, 0, _entries.Count - 1);
            _entries[_selectedEntry].OnFocusChanged(true);
            _screenDescriptor.GetSprite("Select").Color = _entries[_selectedEntry].IsSelectable ? Color.White : Color.TransparentWhite;
        }

        protected SpriteDescriptor _screenDescriptor;

        protected List<MenuEntry> _entries = new List<MenuEntry>();
        protected int _selectedEntry;

        protected FishingGameContext _context;

        private const float Spacing = 10f;
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
