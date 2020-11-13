using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Alethic.KeyShift.Console.Controllers
{

    /// <summary>
    /// Implements the backend interface.
    /// </summary>
    [Route("kshost")]
    [ApiController]
    public class KsHostController : ControllerBase
    {

        readonly IKsHost<string> host;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host"></param>
        public KsHostController(IKsHost<string> host)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
        }

        /// <summary>
        /// Begins a shift, returning the existing data and a token.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{key}")]
        public async Task<ActionResult> ShiftLockAsync(string key, [FromHeader(Name = "KeyShift-Token")] string token, CancellationToken cancellationToken = default)
        {
            var r = await host.ShiftLockAsync(key, token, cancellationToken);

            // data has already moved
            if (r.ForwardUri != null)
                return Redirect(r.ForwardUri.ToString());

            // data is present
            if (r.Data != null)
            {
                Response.Headers.Add("KeyShift-Token", r.Token);
                return File(r.Data, "application/octet-stream");
            }

            return NotFound();
        }

        /// <summary>
        /// Finalizes a shift, using the existing token.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="forward"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{key}")]
        public Task ShiftAsync(string key, [FromHeader(Name = "KeyShift-Token")] string token, [FromQuery] Uri forward, CancellationToken cancellationToken)
        {
            return host.ShiftAsync(key, token, forward, cancellationToken);
        }

    }

}
