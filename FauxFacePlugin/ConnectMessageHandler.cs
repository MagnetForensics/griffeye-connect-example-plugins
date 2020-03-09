using System;
using System.Net.Cache;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Griffeye;
using Microsoft.AspNetCore.Http;

namespace FauxFacePlugin
{
    public class ConnectMessageHandler : DelegatingHandler
    {
        private readonly Client _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly object _authResponse_lock = new object();
        private static Response _authResponse;
        private static DateTime _authTokenExpiration;

        public ConnectMessageHandler(Client client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authToken = GetAuthToken(cancellationToken);
            request.Headers.Add("Authorization", $"Bearer {authToken}");

            var correlationId = GetCorrelationId();
            request.Headers.Add("x-correlation-id", correlationId);

            return await base.SendAsync(request, cancellationToken);
        }

        private string GetAuthToken(CancellationToken cancellationToken)
        {
            bool ValidToken()
            {
                if (_authResponse == null) return false;
                var diff = _authTokenExpiration.Subtract(DateTime.Now);
                return diff > TimeSpan.FromSeconds(20);
            }

            if (ValidToken()) return _authResponse.Access_token;
            lock (_authResponse_lock)
            {
                if (ValidToken()) return _authResponse.Access_token;
                _authResponse = _client.Oauth2TokenPostAsync(
                    string.Empty, Grant_type.Password,
                     "root",
                    "password",
                    "debug_plugin",
                    string.Empty,
                    string.Empty, cancellationToken).Result;
                _authTokenExpiration = DateTime.Now.AddSeconds(_authResponse.Expires_in ?? 60);
            }

            return _authResponse.Access_token;
        }

        private string GetCorrelationId()
        {
            var headers = _httpContextAccessor.HttpContext?.Request?.Headers;
            if (headers != null && headers.TryGetValue("x-correlation-id", out var value))
            {
                return value;
            }

            return Guid.NewGuid().ToString("D");
        }
    }
}