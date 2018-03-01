using System.ServiceModel;

namespace WcfServiceLibrary
{
    [ServiceContract]
    public interface IDiagnostic
    {
        [OperationContract]
        bool Ping();
    }
}
