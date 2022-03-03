using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCPP.Core.Database;

namespace OCPP.Core.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectorStatusViewController : ControllerBase
    {
        private readonly OCPPCoreContext dbContext;

        public ConnectorStatusViewController(OCPPCoreContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<List<ConnectorStatusView>> GetAll()
        {
            return await dbContext.ConnectorStatusViews.ToListAsync();
        }
    }
}
