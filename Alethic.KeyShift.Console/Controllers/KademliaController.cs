using System;
using System.Collections.Generic;
using System.Linq;

using Cogito.Kademlia;

using Microsoft.AspNetCore.Mvc;

namespace Alethic.KadKey.Console.Controllers
{

    [Route("kademlia")]
    [ApiController]
    public class KademliaController : ControllerBase
    {

        readonly IKRouter<KNodeId256> router;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="router"></param>
        public KademliaController(IKRouter<KNodeId256> router)
        {
            this.router = router ?? throw new ArgumentNullException(nameof(router));
        }

        [HttpGet]
        public string Foo() => "hui";

        [HttpGet("peers")]
        public IAsyncEnumerable<object> GetPeersAsync()
        {
            return router.AsAsyncEnumerable().Select(i => new
            {
                NodeId = i.Id,
                Endpoints = i.Endpoints.Select(j => j.ToUri())
            });
        }

    }

}
