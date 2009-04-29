using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Extensions;
using Library.Sprite;
using Library.Sprite.Pipeline;

namespace FishingGirl.Screens
{
    public class BadgesScreen : MenuScreen
    {
        public BadgesScreen(FishingGameContext context) : base(context)
        {
            _badgeEntries = new List<BadgeMenuEntry>();
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            _upArrow = content.Load<ImageSpriteTemplate>("ArrowUp").Create();
            _upArrow.Position = UpArrowPosition;
            _screenDescriptor.GetSprite<CompositeSprite>("Entries").Add(_upArrow);
            _downArrow = content.Load<ImageSpriteTemplate>("ArrowDown").Create();
            _downArrow.Position = DownArrowPosition;
            _screenDescriptor.GetSprite<CompositeSprite>("Entries").Add(_downArrow);

            SpriteFont font = content.Load<SpriteFont>(@"Fonts/Text");
            AddBadge(new BadgeMenuEntry(new TextSprite(font, "A")));
            AddBadge(new BadgeMenuEntry(new TextSprite(font, "B")));
            AddBadge(new BadgeMenuEntry(new TextSprite(font, "C")));
            AddBadge(new BadgeMenuEntry(new TextSprite(font, "D")));
            AddBadge(new BadgeMenuEntry(new TextSprite(font, "E")));
            AddBadge(new BadgeMenuEntry(new TextSprite(font, "F")));
            AddBadge(new BadgeMenuEntry(new TextSprite(font, "G")));
            AddBadge(new BadgeMenuEntry(new TextSprite(font, "H")));
            LayoutEntries();
        }
        
        /// <summary>
        /// Adds a badge to this screen.
        /// </summary>
        /// <param name="entry">The badge to add.</param>
        public void AddBadge(BadgeMenuEntry entry)
        {
            _badgeEntries.Add(entry);
            if (_entries.Count < NumberDisplayed)
            {
                AddEntry(entry);
            }
        }

        protected override void Show(bool pushed)
        {
            base.Show(pushed);
            _screenDescriptor.GetSprite("Select").Color = Color.TransparentWhite; // nothing is selectable
            if (pushed)
            {
                // reset the list
                _entries[_selectedEntry].OnFocusChanged(false);
                _entries.ForEach(e => _screenDescriptor.GetSprite<CompositeSprite>("Entries").Remove(e.Sprite));
                _entries.Clear();
                for (int i = 0; i < NumberDisplayed; i++)
                {
                    AddEntry(_badgeEntries[i]);
                }
                LayoutEntries();
                _selectedEntry = _selectedBadge = _listWindowBaseIndex = 0;
                SetSelected(0);
            }
        }

        protected override void SetSelected(int deltaIdx)
        {
            _entries[_selectedEntry].OnFocusChanged(false);

            int nextEntry = _selectedEntry + deltaIdx;
            int nextBadge = _selectedBadge + deltaIdx;

            if (nextEntry >= _entries.Count && nextBadge < _badgeEntries.Count)
            {
                RemoveEntry(_entries[0]);
                AddEntry(_badgeEntries[nextBadge]);
                LayoutEntries();
                _listWindowBaseIndex += 1;
            }
            else if (nextEntry < 0 && nextBadge >= 0)
            {
                RemoveEntry(_entries[_entries.Count - 1]);
                _entries.Insert(0, _badgeEntries[nextBadge]);
                _screenDescriptor.GetSprite<CompositeSprite>("Entries").Add(_badgeEntries[nextBadge].Sprite);
                LayoutEntries();
                _listWindowBaseIndex -= 1;
            }

            _selectedEntry = MathHelperExtensions.Clamp(nextEntry, 0, _entries.Count - 1);
            _selectedBadge = MathHelperExtensions.Clamp(nextBadge, 0, _badgeEntries.Count - 1);

            _upArrow.Color = (_listWindowBaseIndex == 0) ? Color.TransparentWhite : Color.White;
            _downArrow.Color = (_listWindowBaseIndex == _badgeEntries.Count - NumberDisplayed) ? Color.TransparentWhite : Color.White;

            _entries[_selectedEntry].OnFocusChanged(true);
        }

        private List<BadgeMenuEntry> _badgeEntries;
        private int _selectedBadge;
        private int _listWindowBaseIndex;

        private ImageSprite _upArrow;
        private ImageSprite _downArrow;

        private const int NumberDisplayed = 5;
        private readonly Vector2 UpArrowPosition = new Vector2(420, 10);
        private readonly Vector2 DownArrowPosition = new Vector2(420, 199);
    }

    public class BadgeMenuEntry : MenuEntry
    {
        public BadgeMenuEntry(Sprite s) : base(s)
        {
        }

        public override void OnFocusChanged(bool focused)
        {
            ((TextSprite)Sprite).Color = focused ? Color.Red : Color.White;
        }
    }
}
