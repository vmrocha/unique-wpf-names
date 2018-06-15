using System.Collections.Generic;

namespace UniqueNames.ViewModels
{
    public class PanelsViewModel : BaseViewModel
    {
        public IEnumerable<PanelViewModel> Panels { get; } = new[]
        {
            new PanelViewModel {Id = 1, Title = "Panel 1"},
            new PanelViewModel {Id = 2, Title = "Panel 2"},
            new PanelViewModel {Id = 3, Title = "Panel 3"}
        };

        public IEnumerable<string> Values { get; } = new[]
        {
            "Panel 1",
            "Panel 2",
            "Panel 3"
        };
    }
}
