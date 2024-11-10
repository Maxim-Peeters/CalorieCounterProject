using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalorieCounter.Services
{
    public interface INavigationService
    {
        Task NavigateToSummaryPageAsync();
        Task NavigateBackAsync();
    }
}
