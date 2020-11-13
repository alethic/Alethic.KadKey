using System;
using System.Threading;
using System.Threading.Tasks;

using Cogito.IO;

using Microsoft.AspNetCore.Mvc;

namespace Alethic.KeyShift.Console.Controllers
{

    /// <summary>
    /// Implements the front-end client interface.
    /// </summary>
    [Route("kskeys")]
    [ApiController]
    public class KsKeysController : ControllerBase
    {

        readonly IKsHost<string> host;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host"></param>
        public KsKeysController(IKsHost<string> host)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
        }

        [HttpGet("{key}")]
        public async Task<ActionResult> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            var r = await host.GetAsync(key, cancellationToken);
            if (r != null)
                return File(await host.GetAsync(key, cancellationToken), "application/octet-stream");

            return NotFound();
        }

        [HttpPut("{key}")]
        public async Task SetAsync(string key, CancellationToken cancellationToken = default)
        {
            await host.SetAsync(key, await Request.Body.ReadAllBytesAsync(), cancellationToken);
        }

    }

}
