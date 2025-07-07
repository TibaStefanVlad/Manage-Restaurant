using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Restaurant.Views
{
    public partial class OrderView : UserControl
    {
        public OrderView()
        {
            InitializeComponent();

            // Add this handler after the UI is loaded
            this.Loaded += OrderView_Loaded;
        }

        private void OrderView_Loaded(object sender, RoutedEventArgs e)
        {
            // After the view is loaded, find and set all quantity textboxes
            SetDefaultQuantityValues();
        }

        private void SetDefaultQuantityValues()
        {
            // Find all quantity textboxes and ensure they show "1" by default
            var textBoxes = FindVisualChildren<TextBox>(this);

            foreach (var textBox in textBoxes)
            {
                // Check if this is likely a quantity textbox based on width or name
                if (textBox.Width == 40 && string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = "1";
                }
            }
        }

        // Helper method to find all children of a specific type
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}