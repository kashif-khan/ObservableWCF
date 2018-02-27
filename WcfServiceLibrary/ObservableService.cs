using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfServiceLibrary
{
    public class ObservableService : IObservableService
    {
        private static Dictionary<string, IObservableService> _CallbackList = new Dictionary<string, IObservableService>();
        private int MyIntVariable;
        public void MethodThatWillChangeData(int newValue)
        {
            var previousValue = MyIntVariable;
            MyIntVariable = newValue;
            foreach (var item in _CallbackList)
            {
                item.Value.SendUpdatedDataToClients(newValue);
            }
        }

        public void Ping()
        {
            var test = _CallbackList.Keys.ToList();
            for (int i = _CallbackList.Count - 1; i >= 0; i--)
            {
                try
                {
                    _CallbackList[test[i]].Pulse()
                }
                catch (Exception)
                {
                    _CallbackList.Remove(test[i]);
                }
            }
        }

        public bool Pulse()
        {
            return true;
        }

        public void SendUpdatedDataToClients(int newValue)
        {
            MyIntVariable = newValue;
        }

        public void Subscribe(string ServiceName, IObservableService Client)
        {
            if (!_CallbackList.Keys.Contains(ServiceName))
            {
                _CallbackList.Add(ServiceName, Client);
            }
        }

        public void Unsubscribe(string ServiceName)
        {
            if (_CallbackList.Keys.Contains(ServiceName))
            {
                _CallbackList.Remove(ServiceName);
            }
        }
    }
}
