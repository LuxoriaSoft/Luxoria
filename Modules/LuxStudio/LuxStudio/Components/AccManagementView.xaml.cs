using LuxStudio.COM.Auth;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace LuxStudio.Components
{
    public sealed partial class AccManagementView : Page
    {
        private AuthManager? _authMgr;

        public AccManagementView(ref AuthManager? authMgr)
        {
            this.InitializeComponent();
            _authMgr = authMgr;
            Loaded += AccManagementView_Loaded;
        }

        private async void AccManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_authMgr == null)
            {
                LoadingPanel.Visibility = Visibility.Visible;
                InputPanel.Visibility = Visibility.Collapsed;

                // Simulate initialization delay
                await Task.Delay(2000);

                LoadingPanel.Visibility = Visibility.Collapsed;
                InputPanel.Visibility = Visibility.Visible;
            }
            else
            {
                // Proceed with authenticated state, or skip directly
                InputPanel.Visibility = Visibility.Visible;
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
