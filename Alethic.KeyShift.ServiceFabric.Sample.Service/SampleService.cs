using System.Fabric;

using Microsoft.ServiceFabric.Services.Runtime;

namespace Alethic.KeyShift.ServiceFabric.Sample.Service
{

    public class SampleService : StatefulService
    {

        public SampleService(StatefulServiceContext serviceContext) :
            base(serviceContext)
        {

        }

    }

}
