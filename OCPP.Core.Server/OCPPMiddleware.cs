using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OCPP.Core.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace OCPP.Core.Server
{
    public partial class OCPPMiddleware
    {
        // Supported OCPP protocols (in order)
        private const string Protocol_OCPP16 = "ocpp1.6";
        private const string Protocol_OCPP20 = "ocpp2.0";
        private static readonly string[] SupportedProtocols = { Protocol_OCPP20, Protocol_OCPP16 /*, "ocpp1.5" */};

        // RegExp for splitting ocpp message parts
        // ^\[\s*(\d)\s*,\s*\"([^"]*)\"\s*,(?:\s*\"(\w*)\"\s*,)?\s*(.*)\s*\]$
        // Third block is optional, because responses don't have an action
        private static string MessageRegExp = "^\\[\\s*(\\d)\\s*,\\s*\"([^\"]*)\"\\s*,(?:\\s*\"(\\w*)\"\\s*,)?\\s*(.*)\\s*\\]$";

        private readonly ILoggerFactory _logFactory;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        // Dictionary with status objects for each charge point
        private static Dictionary<string, ChargePointStatus> _chargePointStatusDict = new Dictionary<string, ChargePointStatus>();

        // Dictionary for processing asynchronous API calls
        private Dictionary<string, OCPPMessage> _requestQueue = new Dictionary<string, OCPPMessage>();

        public OCPPMiddleware(ILoggerFactory logFactory, IConfiguration configuration)
        {
            _logFactory = logFactory;
            _configuration = configuration;

            _logger = logFactory.CreateLogger("OCPPMiddleware");
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogTrace($"OCPPMiddleware => Websocket request: Path='{context.Request.Path}'");

            ChargePointStatus chargePointStatus = null;

            if (context.Request.Path.StartsWithSegments("/"))
            {
                try
                {
                    bool showIndexInfo = _configuration.GetValue<bool>("ShowIndexInfo");
                    if (showIndexInfo)
                    {
                        _logger.LogTrace("OCPPMiddleware => Index status page");

                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(string.Format($"Running...\r\n\r\n{_chargePointStatusDict.Values.Count} chargepoints connected"));
                    }
                    else
                    {
                        _logger.LogInformation("OCPPMiddleware => Root path with deactivated index page");
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                catch (Exception exp)
                {
                    _logger.LogError(exp, $"OCPPMiddleware => Error: {exp.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                _logger.LogWarning("OCPPMiddleware => Bad path request");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
    }

    public static class OCPPMiddlewareExtensions
    {
        public static IApplicationBuilder UseOCPPMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OCPPMiddleware>();
        }
    }
}
