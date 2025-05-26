using LuxEditor.EditorUI.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;

namespace LuxEditor.EditorUI.Groups
{
    public class EditorCategory : IEditorControl
    {
        private readonly StackPanel _container;
        private readonly List<IEditorGroupItem> _items = new();
        private readonly string _key;
        private readonly Button _resetButton;

        public event Action<string>? OnResetClicked;

        /// <summary>
        /// Creates a new category for the editor UI.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="title"></param>
        public EditorCategory(string key, string title)
        {
            _key = key;

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 15,
                Margin = new Thickness(0, 5, 0, 5),
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255))
            };

            _resetButton = new Button
            {
                Content = "Reset",
                Visibility = Visibility.Collapsed,
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)),
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            _resetButton.Click += (s, e) => OnResetClicked?.Invoke(_key);

            Grid.SetColumn(titleBlock, 0);
            Grid.SetColumn(_resetButton, 1);
            headerGrid.Children.Add(titleBlock);
            headerGrid.Children.Add(_resetButton);

            _container = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            _container.Children.Add(headerGrid);
        }

        /// <summary>
        /// Adds a category to the editor UI.
        /// </summary>
        /// <param name="item"></param>
        public void AddControl(IEditorGroupItem item)
        {
            _items.Add(item);
            _container.Children.Add(item.GetElement());
        }

        /// <summary>
        /// Get Items in the category.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEditorGroupItem> GetItems() => _items;

        /// <summary>
        /// Sets the visibility of the reset button.
        /// </summary>
        /// <param name="visible"></param>
        public void SetResetVisible(bool visible)
        {
            _resetButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Gets the UI element for this category.
        /// </summary>
        /// <returns></returns>
        public UIElement GetElement() => _container;
    }
}
