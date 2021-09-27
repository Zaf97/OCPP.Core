using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OCPP.Core.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OCPP.Core.Server.Controllers;
[Route("[controller]")]
[ApiController]
public class ApiController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory logFactory;
    private readonly OCPPCoreContext dbContext;
    private readonly OCPPMiddleware oCPPMiddleware;
    private readonly ILogger _logger;

    // Supported OCPP protocols (in order)
    private const string Protocol_OCPP16 = "ocpp1.6";
    private const string Protocol_OCPP20 = "ocpp2.0";
    private static readonly string[] SupportedProtocols = { Protocol_OCPP20, Protocol_OCPP16 /*, "ocpp1.5" */};

    // Dictionary with status objects for each charge point
    private static Dictionary<string, ChargePointStatus> _chargePointStatusDict = new Dictionary<string, ChargePointStatus>();

    public ApiController(IConfiguration _configuration, ILoggerFactory logFactory, OCPPCoreContext dbContext, OCPPMiddleware oCPPMiddleware)
    {
        this._configuration = _configuration;
        this.logFactory = logFactory;
        this.dbContext = dbContext;
        this.oCPPMiddleware = oCPPMiddleware;
        _logger = logFactory.CreateLogger("OCPPMiddleware");
    }

    [HttpGet("/API/{cmd}")]
    public async Task Get(string cmd)
    {
        var context = HttpContext;
        // Check authentication (X-API-Key)
        string apiKeyConfig = _configuration.GetValue<string>("ApiKey");
        if (!string.IsNullOrWhiteSpace(apiKeyConfig))
        {
            // ApiKey specified => check request
            string apiKeyCaller = context.Request.Headers["X-API-Key"].FirstOrDefault();
            if (apiKeyConfig == apiKeyCaller)
            {
                // API-Key matches
                _logger.LogInformation("OCPPMiddleware => Success: X-API-Key matches");
            }
            else
            {
                // API-Key does NOT matches => authentication failure!!!
                _logger.LogWarning("OCPPMiddleware => Failure: Wrong X-API-Key! Caller='{0}'", apiKeyCaller);
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
        }
        else
        {
            // No API-Key configured => no authenticatiuon
            _logger.LogWarning("OCPPMiddleware => No X-API-Key configured!");
        }

        // format: /API/<command>[/chargepointId]
        string[] urlParts = context.Request.Path.Value.Split('/');

        string urlChargePointId = (urlParts.Length >= 4) ? urlParts[3] : null;
        _logger.LogTrace($"OCPPMiddleware => cmd='{cmd}' / id='{urlChargePointId}' / FullPath='{context.Request.Path.Value}')");

        if (cmd == "Status")
        {
            try
            {
                List<ChargePointStatus> statusList = new List<ChargePointStatus>();
                foreach (ChargePointStatus status in _chargePointStatusDict.Values)
                {
                    statusList.Add(status);
                }
                string jsonStatus = JsonConvert.SerializeObject(statusList);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(jsonStatus);
            }
            catch (Exception exp)
            {
                _logger.LogError(exp, $"OCPPMiddleware => Error: {exp.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

        }
        else if (cmd == "Reset")
        {
            if (!string.IsNullOrEmpty(urlChargePointId))
            {
                try
                {
                    ChargePointStatus status = null;
                    if (_chargePointStatusDict.TryGetValue(urlChargePointId, out status))
                    {
                        // Send message to chargepoint
                        if (status.Protocol == Protocol_OCPP20)
                        {
                            // OCPP V2.0
                            await oCPPMiddleware.Reset20(status, context);
                        }
                        else
                        {
                            // OCPP V1.6
                            await oCPPMiddleware.Reset16(status, context);
                        }
                    }
                    else
                    {
                        // Chargepoint offline
                        _logger.LogError($"OCPPMiddleware SoftReset => Chargepoint offline: {urlChargePointId}");
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                catch (Exception exp)
                {
                    _logger.LogError(exp, $"OCPPMiddleware SoftReset => Error: {exp.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                _logger.LogError("OCPPMiddleware SoftReset => Missing chargepoint ID");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
        else if (cmd == "UnlockConnector")
        {
            if (!string.IsNullOrEmpty(urlChargePointId))
            {
                try
                {
                    ChargePointStatus status = null;
                    if (_chargePointStatusDict.TryGetValue(urlChargePointId, out status))
                    {
                        // Send message to chargepoint
                        if (status.Protocol == Protocol_OCPP20)
                        {
                            // OCPP V2.0
                            await oCPPMiddleware.UnlockConnector20(status, context);
                        }
                        else
                        {
                            // OCPP V1.6
                            await oCPPMiddleware.UnlockConnector16(status, context);
                        }
                    }
                    else
                    {
                        // Chargepoint offline
                        _logger.LogError($"OCPPMiddleware UnlockConnector => Chargepoint offline: {urlChargePointId}");
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                catch (Exception exp)
                {
                    _logger.LogError(exp, $"OCPPMiddleware UnlockConnector => Error: {exp.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                _logger.LogError("OCPPMiddleware UnlockConnector => Missing chargepoint ID");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
        else
        {
            // Unknown action/function
            _logger.LogWarning($"OCPPMiddleware => action/function: {cmd}");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }

    //[HttpGet("/API/Status")]
    //public async Task GetStatus()
    //{
    //    var context = HttpContext;
    //    try
    //    {
    //        List<ChargePointStatus> statusList = new List<ChargePointStatus>();
    //        foreach (ChargePointStatus status in _chargePointStatusDict.Values)
    //        {
    //            statusList.Add(status);
    //        }
    //        string jsonStatus = JsonConvert.SerializeObject(statusList);
    //        context.Response.ContentType = "application/json";
    //        await context.Response.WriteAsync(jsonStatus);
    //    }
    //    catch (Exception exp)
    //    {
    //        _logger.LogError(exp, $"OCPPMiddleware => Error: {exp.Message}");
    //        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    //    }
    //}
}