using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OCPP.Core.API;
using OCPP.Core.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCPP.Core.Server.Pages
{
    public class DebugModel : PageModel
    {
        private readonly ConnectorStatusViewController connectorStatusViewController;

        public DebugModel(ConnectorStatusViewController connectorStatusViewController)
        {
            this.connectorStatusViewController = connectorStatusViewController;
        }

        public List<ConnectorStatusView> connectorStatus { get; private set; }

        public async Task OnGet()
        {
            connectorStatus = await connectorStatusViewController.GetAll();
        }
    }
}
