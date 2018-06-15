using System.Windows;
using UniqueNames.ViewModels;

namespace UniqueNames.Views
{
    public partial class UsingCodeBehind
    {
        public UsingCodeBehind()
        {
            InitializeComponent();
        }

        private void OnMenuBtnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                if (element.DataContext is PanelViewModel item)
                {
                    element.Name = $"MenuBtn{item.Id}";
                }
            }
        }
    }
}
