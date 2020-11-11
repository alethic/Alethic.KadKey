using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Cogito.IO;
using Cogito.Kademlia;

using Microsoft.AspNetCore.Mvc;

namespace Alethic.KeyShift.Console.Controllers
{

    [Route("kademlia")]
    [ApiController]
    public class KademliaController : ControllerBase
    {

        readonly IKRouter<KNodeId256> router;
        readonly IKValueAccessor<KNodeId256> values;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="router"></param>
        /// <param name="publisher"></param>
        public KademliaController(IKRouter<KNodeId256> router, IKPublisher<KNodeId256> publisher, IKValueAccessor<KNodeId256> values)
        {
            this.router = router ?? throw new ArgumentNullException(nameof(router));
            this.values = values ?? throw new ArgumentNullException(nameof(values));
        }

        [HttpGet("peers")]
        public IAsyncEnumerable<object> GetPeersAsync()
        {
            return router.AsAsyncEnumerable().Select(i => new
            {
                NodeId = i.Id.ToString(),
                Endpoints = i.Endpoints.Select(j => j.ToUri())
            });
        }

        [HttpPut("dht/{key}")]
        public async Task Set(string key, CancellationToken cancellationToken)
        {
            var h = KNodeId<KNodeId256>.Read(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(key)));
            var v = await values.GetAsync(h, cancellationToken);
            await values.SetAsync(h, new KValueInfo(await Request.Body.ReadAllBytesAsync(), v.HasValue ? (v.Value.Version + 1) : 1, DateTime.UtcNow.AddMinutes(60)), cancellationToken);
        }

        [HttpGet("dht/{key}")]
        public async Task<ActionResult> Get(string key, CancellationToken cancellationToken)
        {
            var h = KNodeId<KNodeId256>.Read(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(key)));
            var v = await values.GetAsync(h, cancellationToken);
            return v.HasValue ? (ActionResult)File(v.Value.Data, "application/octet-stream") : NotFound();
        }

    }

}
