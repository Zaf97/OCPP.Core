using Microsoft.AspNetCore.Mvc;
using OCPP.Core.Database;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace OCPP.Core.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OCPPController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
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

        public OCPPController(IConfiguration config, ILoggerFactory logFactory, OCPPCoreContext dbContext, OCPPMiddleware oCPPMiddleware)
        {
            this.logFactory = logFactory;
            this.dbContext = dbContext;
            this.oCPPMiddleware = oCPPMiddleware;
            _logger = logFactory.CreateLogger("OCPPMiddleware");
        }

        [HttpGet("{connectorId}")]
        public async Task Get(string connectorId)
        {
            var context = HttpContext;
            ChargePointStatus chargePointStatus = new();
            _logger.LogInformation($"OCPPMiddleware => Connection request with chargepoint identifier = '{connectorId}'");

            ChargePoint chargePoint = await dbContext.ChargePoints.Where(row => row.ChargePointId == connectorId).FirstOrDefaultAsync();

            if (chargePoint == null)
            {
                // unknown chargepoint
                _logger.LogWarning($"OCPPMiddleware => FAILURE: Found no chargepoint with identifier={connectorId}");
                _logger.LogTrace("OCPPMiddleware => no chargepoint: http 412");
                context.Response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
                return;
            }

            _logger.LogInformation($"OCPPMiddleware => SUCCESS: Found chargepoint with identifier={chargePoint.ChargePointId}");

            // Check optional chargepoint authentication
            if (!string.IsNullOrWhiteSpace(chargePoint.Username))
            {
                // Chargepoint MUST send basic authentication header

                bool basicAuthSuccess = false;
                string authHeader = context.Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(authHeader))
                {
                    string[] cred = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(authHeader.Substring(6))).Split(':');
                    if (cred.Length == 2 && chargePoint.Username == cred[0] && chargePoint.Password == cred[1])
                    {
                        // Authentication match => OK
                        _logger.LogInformation("OCPPMiddleware => SUCCESS: Basic authentication for chargepoint '{0}' match", chargePoint.ChargePointId);
                        basicAuthSuccess = true;
                    }
                    else
                    {
                        // Authentication does NOT match => Failure
                        _logger.LogWarning($"OCPPMiddleware => FAILURE: Basic authentication for chargepoint '{chargePoint.ChargePointId}' does NOT match");
                    }
                }
                if (basicAuthSuccess == false)
                {
                    context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"OCPP.Core\"");
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }

            }
            else if (!string.IsNullOrWhiteSpace(chargePoint.ClientCertThumb))
            {
                // Chargepoint MUST send basic authentication header

                bool certAuthSuccess = false;
                X509Certificate2 clientCert = context.Connection.ClientCertificate;
                if (clientCert != null)
                {
                    if (clientCert.Thumbprint.Equals(chargePoint.ClientCertThumb, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Authentication match => OK
                        _logger.LogInformation($"OCPPMiddleware => SUCCESS: Certificate authentication for chargepoint '{chargePoint.ChargePointId}' match");
                        certAuthSuccess = true;
                    }
                    else
                    {
                        // Authentication does NOT match => Failure
                        _logger.LogWarning($"OCPPMiddleware => FAILURE: Certificate authentication for chargepoint '{chargePoint.ChargePointId}' does NOT match");
                    }
                }
                if (certAuthSuccess == false)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }
            }
            else
            {
                _logger.LogInformation($"OCPPMiddleware => No authentication for chargepoint '{chargePoint.ChargePointId}' configured");
            }

            // Store chargepoint data
            chargePointStatus = new ChargePointStatus(chargePoint);

            if (chargePointStatus == null)
            {
                // unknown chargepoint
                _logger.LogTrace("OCPPMiddleware => no chargepoint: http 412");
                context.Response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                // no websocket request => failure
                _logger.LogWarning("OCPPMiddleware => Non-Websocket request");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            string subProtocol = GetSupportedProtocol(context);

            if (string.IsNullOrEmpty(subProtocol))
            {
                // Not matching protocol! => failure
                string protocols = string.Empty;
                foreach (string p in context.WebSockets.WebSocketRequestedProtocols)
                {
                    if (string.IsNullOrEmpty(protocols)) protocols += ",";
                    protocols += p;
                }
                _logger.LogWarning($"OCPPMiddleware => No supported sub-protocol in '{protocols}' from charge station '{connectorId}'");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                chargePointStatus.Protocol = subProtocol;

                bool statusSuccess = false;
                try
                {
                    _logger.LogTrace("OCPPMiddleware => Store/Update status object");
                    statusSuccess = ChargingStatusHandling(chargePointStatus, connectorId);
                }
                catch (Exception exp)
                {
                    _logger.LogError(exp, "OCPPMiddleware => Error storing status object in dictionary => refuse connection");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

                if (statusSuccess)
                {
                    // Handle socket communication
                    _logger.LogTrace("OCPPMiddleware => Waiting for message...");

                    await SocketCommunication(context, chargePointStatus, connectorId, subProtocol);
                }
            }

        }

        private static string GetSupportedProtocol(HttpContext context)
        {
            // Match supported sub protocols
            string subProtocol = null;
            foreach (string supportedProtocol in SupportedProtocols)
            {
                if (context.WebSockets.WebSocketRequestedProtocols.Contains(supportedProtocol))
                {
                    subProtocol = supportedProtocol;
                    break;
                }
            }

            return subProtocol;
        }

        private static bool ChargingStatusHandling(ChargePointStatus chargePointStatus, string chargepointIdentifier)
        {
            bool statusSuccess;
            lock (_chargePointStatusDict)
            {
                // Check if this chargepoint already/still has a status object
                if (_chargePointStatusDict.ContainsKey(chargepointIdentifier))
                {
                    // exists => check status
                    if (_chargePointStatusDict[chargepointIdentifier].WebSocket.State != WebSocketState.Open)
                    {
                        // Closed or aborted => remove
                        _chargePointStatusDict.Remove(chargepointIdentifier);
                    }
                }

                _chargePointStatusDict.Add(chargepointIdentifier, chargePointStatus);
                statusSuccess = true;
            }

            return statusSuccess;
        }

        private async Task SocketCommunication(HttpContext context, ChargePointStatus chargePointStatus, string chargepointIdentifier, string subProtocol)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync(subProtocol);
            _logger.LogTrace($"OCPPMiddleware => WebSocket connection with charge point '{chargepointIdentifier}'");
            chargePointStatus.WebSocket = webSocket;

            if (subProtocol == Protocol_OCPP20)
            {
                // OCPP V2.0
                await oCPPMiddleware.Receive20(chargePointStatus, context);
            }
            else
            {
                // OCPP V1.6
                await oCPPMiddleware.Receive16(chargePointStatus, context);
            }
        }
    }
}

