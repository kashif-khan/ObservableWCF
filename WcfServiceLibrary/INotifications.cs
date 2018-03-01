using System.ServiceModel;

namespace WcfServiceLibrary
{
    [ServiceContract]
    public interface INotifications
    {
        [OperationContract(IsOneWay = true)]
        void UpdateData(int newValue);
    }
}
