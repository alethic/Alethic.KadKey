using System;
using System.Threading;
using System.Threading.Tasks;

using Cogito.IO;

using Microsoft.AspNetCore.Mvc;

namespace Alethic.KeyShift.Console.Controllers
{

    [Route("keys")]
    [ApiController]
    public class KeyController : ControllerBase
    {

        readonly IKsHost<string> store;

        public KeyController(IKsHost<string> store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        [HttpGet("{key}")]
        public async Task<ActionResult> Select(string key, Guid? token, CancellationToken cancellationToken = default)
        {
            return await store.Select(key, token, cancellationToken) is byte[] b ? (ActionResult)File(b, "application/octet-stream") : NotFound();
        }

        [HttpPut("{key}")]
        public async Task Update(string key, CancellationToken cancellationToken)
        {
            await store.Update(key, await Request.Body.ReadAllBytesAsync(), cancellationToken);
        }

        [HttpDelete("{key}")]
        public Task Remove(string key, Guid token, Uri forward, CancellationToken cancellationToken)
        {
            return store.Remove(key, token, forward, cancellationToken);
        }

        [HttpGet("{key}/freeze")]
        public Task<Guid> Freeze(string key, Guid? token, CancellationToken cancellationToken)
        {
            return store.Freeze(key, token, cancellationToken);
        }

    }

}
