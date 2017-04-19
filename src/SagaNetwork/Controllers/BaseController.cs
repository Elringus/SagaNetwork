using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;

namespace SagaNetwork.Controllers
{
    /// <summary>
    /// Base class for all the controllers. Serves both REST (sync) and WebSocket (async) pipeline.
    /// Handles OCC retrying and performs basic request data validation.
    /// </summary>
    public abstract class BaseController
    {
        /// <summary>
        /// Number of retries before giving up and returning JStatus.OccFail
        /// </summary>
        protected const int OCC_RETRY_COUNT = 3;

        /// <summary>
        /// Setting this true will initiate OCC retrying.
        /// </summary>
        protected bool OccFailFlag { get; set; } = false;

        private JToken responseToken;
        private int curOccRetry = 0;

        /// <summary>
        /// Default handler for all the HTTP requests.
        /// </summary>
        /// <param name="data">JSON body of the request.</param>
        /// <returns>JSON response.</returns>
        [HttpPost]
        public async Task<JToken> HandleHttpRequestAsync ([FromBody]JToken data)
        {
            return await HandleRequestAsync(data);
        }

        /// <summary>
        /// Default handler to serve WebSocket pipeline.
        /// </summary>
        /// <param name="httpContext">HTTP context of the request.</param>
        /// <param name="next"></param>
        public static async Task HandleWebSoсketRequestAsync (HttpContext httpContext, Func<Task> next)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                var isAuthed = false;
                var buffer = new ArraySegment<byte>(new byte[4096]);
                var requestString = string.Empty;

                if (webSocket != null && webSocket.State == WebSocketState.Open)
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        var received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                        requestString += Encoding.UTF8.GetString(buffer.Array, buffer.Offset, received.Count);
                        if (!received.EndOfMessage) continue;

                        var requestToken = JParser.TryParse(requestString);
                        requestString = string.Empty;
                        if (requestToken == null) continue;

                        JToken responseToken;

                        // Check auth only once per socket connection.
                        if (!isAuthed) isAuthed = Authorization.CheckAuth(requestToken);

                        if (isAuthed)
                        {
                            var controllerName = requestToken.ParseField<string>("Controller");
                            if (controllerName == null) responseToken = JStatus.RequestArgumentNotFound("Controller"); 
                            else
                            {
                                var controllerType = Type.GetType($"SagaNetwork.Controllers.{controllerName}Controller");
                                if (controllerType == null) responseToken = JStatus.ControllerNotFound;
                                else
                                {
                                    var controller = Activator.CreateInstance(controllerType) as BaseController;
                                    if (controller != null) responseToken = await controller.HandleRequestAsync(requestToken, skipClientAuthCheck: true);
                                    else responseToken = JStatus.ControllerNotFound;
                                }
                            }
                        }
                        else responseToken = JStatus.AuthFail;

                        // Inject request ID.
                        var requestId = requestToken.ParseField<string>("RequestId");
                        if (requestId == null) responseToken = JStatus.RequestArgumentNotFound("RequestId");
                        else responseToken["RequestId"] = requestId;

                        var responseData = Encoding.UTF8.GetBytes(responseToken.ToString());
                        buffer = new ArraySegment<byte>(responseData);
                        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
            else await next(); // not a websocket request — pass downstream
        }

        /// <summary>
        /// Custom logic of the derived controller.
        /// </summary>
        /// <param name="data">JSON data of the request.</param>
        /// <returns>JSON data of the response.</returns>
        protected abstract Task<JToken> ExecuteController (JToken data);

        /// <summary>
        /// Validates and parses request/response data, executes derived controller and handles OCC.
        /// </summary>
        private async Task<JToken> HandleRequestAsync (JToken data, bool skipClientAuthCheck = false)
        {
            if (data == null || !data.HasValues)
                return JStatus.WrongQuery;

            var derivedType = this.GetType();

            // Handle server auth.
            if (derivedType.GetCustomAttributes<RequireServerAuthAttribute>(true).Any() && 
                !Authorization.CheckAuth(data, requireServerAuth: true))
                return JStatus.ServerAuthFail;

            // Handle client auth.
            if (!skipClientAuthCheck && derivedType.GetCustomAttributes<RequireAuthAttribute>(true).Any() && 
                !Authorization.CheckAuth(data))
                return JStatus.AuthFail;

            // Inject request arguments.
            var fields = derivedType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields.Where(p => p.GetCustomAttributes<ApiRequestDataAttribute>(true).Any()))
            {
                var parsedField = data.ParseField(field.Name, field.FieldType);
                if (parsedField == null) return JStatus.RequestArgumentNotFound(field.Name);
                field.SetValue(this, Convert.ChangeType(parsedField, field.FieldType));
            }

            // Execute the derived controller logic.
            responseToken = await ExecuteController(data);

            // Handle OCC violations.
            if (OccFailFlag)
            {
                OccFailFlag = false;
                curOccRetry++;
                if (curOccRetry > OCC_RETRY_COUNT)
                    return JStatus.OccFail;
                await HandleRequestAsync(data);
            }

            if (responseToken == null)
                responseToken = JStatus.ControllerFail;

            // Inject response data.
            foreach (var field in fields.Where(p => p.GetCustomAttributes<ApiResponseDataAttribute>(true).Any()))
                if (field.GetValue(this) != null) responseToken[field.Name] = JToken.FromObject(field.GetValue(this));

            return responseToken;
        }
    }
}
