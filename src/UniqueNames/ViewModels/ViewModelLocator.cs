namespace UniqueNames.ViewModels
{
    public class ViewModelLocator
    {
        private static readonly PanelsViewModel PanelsInstance;

        static ViewModelLocator()
        {
            PanelsInstance = new PanelsViewModel();
        }

        public PanelsViewModel Panels => PanelsInstance;
    }
}
