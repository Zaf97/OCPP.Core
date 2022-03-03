using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OCPP.Core.API;
using OCPP.Core.API.Controllers;
using OCPP.Core.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCPP.Core.Server.Pages
{
    public class DebugModel : PageModel
    {
        private readonly ConnectorStatusViewController connectorStatusViewController;
        private readonly ChargingPointsController chargePointsController;

        public DebugModel(ConnectorStatusViewController connectorStatusViewController, ChargingPointsController chargePointsController)
        {
            this.connectorStatusViewController = connectorStatusViewController;
            this.chargePointsController = chargePointsController;
        }

        public List<ConnectorStatusView> connectorStatus { get; private set; }
        public List<ChargePoint> chargingPoints { get; private set; }

        public async Task OnGet()
        {
            connectorStatus = await connectorStatusViewController.GetAll();
            chargingPoints = await chargePointsController.GetAll();
        }
    }
}
