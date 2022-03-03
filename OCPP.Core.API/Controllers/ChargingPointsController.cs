using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCPP.Core.Database;

namespace OCPP.Core.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingPointsController : ControllerBase
    {
        private readonly OCPPCoreContext context;

        public ChargingPointsController(OCPPCoreContext context)
        {
            this.context = context;
        }

        [HttpGet]  
        public async Task<List<ChargePoint>> GetAll()
        {
            return await context.ChargePoints.ToListAsync();
        }
    }
}
