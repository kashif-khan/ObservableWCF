using System.ServiceModel;

namespace WcfServiceLibrary
{
    [ServiceContract(CallbackContract = typeof(INotifications))]
    public interface ISubscription : INotifications
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(string ServiceName);
        [OperationContract(IsOneWay = true)]
        void Unsubscribe(string ServiceName);
    }
}
