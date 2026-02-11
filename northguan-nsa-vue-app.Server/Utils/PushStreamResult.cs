
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace northguan_nsa_vue_app.Server.Utils
{
    public class PushStreamResult : IActionResult
    {
        private readonly Func<Stream, CancellationToken, Task> _onStreamAvailable;
        private readonly string _contentType;
        private readonly CancellationToken _requestAborted;

        public PushStreamResult(Func<Stream, CancellationToken, Task> onStreamAvailable, string contentType)
        {
            _onStreamAvailable = onStreamAvailable ?? throw new ArgumentNullException(nameof(onStreamAvailable));
            _contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.HttpContext.Response;
            response.ContentType = _contentType;

            return _onStreamAvailable(response.Body, context.HttpContext.RequestAborted);
        }
    }
}
