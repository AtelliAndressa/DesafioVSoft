using DeviceManager.Mobile.ViewModels;

namespace DeviceManager.Mobile.Views
{
    public partial class SyncPage : ContentPage
    {
        public SyncPage(SyncViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
} 