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
    public interface ISubscription : IHeartbeat
    {
        void Subscribe(string ServiceName, IObservableService Client);
        void Unsubscribe(string ServiceName);

    }
    [ServiceContract(CallbackContract = typeof(IObservableService))]
    public interface IObservableService : ISubscription
    {
        [OperationContract(IsOneWay = true)]
        void MethodThatWillChangeData(int value);
        [OperationContract(IsOneWay = true)]
        void SendUpdatedDataToClients(int newValue);
    }
}
