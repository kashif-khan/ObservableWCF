using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfServiceLibrary
{
    public interface IHeartbeat
    {
        bool Pulse();
        void Ping();
    }

    [ServiceContract(CallbackContract = typeof(INotifications))]
    public interface ISubscription : IHeartbeat, INotifications
    {
        void Subscribe(string ServiceName, ISubscription Client);
        void Unsubscribe(string ServiceName);
    }

    [ServiceContract]
    public interface INotifications
    {
        [OperationContract(IsOneWay = true)]
        void AnnounceClientAgeToClients(int newValue);
    }

    [ServiceContract]
    public interface IUserService
    {
        [OperationContract(IsOneWay = true)]
        void MethodThatWillChangeData(int value);
    }
}
