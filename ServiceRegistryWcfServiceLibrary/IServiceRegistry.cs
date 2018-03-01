using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ServiceRegistryWcfServiceLibrary
{
    [ServiceContract(CallbackContract = typeof(IServiceRegistryCallback))]
    public interface IServiceRegistry : IServiceRegistryEssentialFeatures
    {
        [OperationContract]
        bool Online(string Url);
        [OperationContract]
        List<string> GetServersList();
    }
}
