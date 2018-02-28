﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

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

    [ServiceContract]
    public interface INotifications
    {
        [OperationContract(IsOneWay = true)]
        void UpdateData(int newValue);
    }

    [ServiceContract]
    public interface IUserService
    {
        [OperationContract(IsOneWay = true)]
        void MethodThatWillChangeData(int value);
    }
}
