using System.ServiceModel;

namespace ServiceRegistryWcfServiceLibrary
{
    [ServiceContract]
    public interface IServiceRegistryCallback
    {
        [OperationContract]
        bool Ping();
        [OperationContract(IsOneWay = true)]
        void NewServiceAdded(string Url);
        [OperationContract(IsOneWay = true)]
        void ServiceRemoved(string Url);
    }
}