using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfServiceLibrary
{
    public class ObservableService : IUserService, ISubscription
    {
        private static Dictionary<string, ISubscription> _CallbackList = new Dictionary<string, ISubscription>();
        private int ClientAge;

        public void MethodThatWillChangeData(int newValue)
        {
            var PreviousClientAge = ClientAge;
            ClientAge = newValue;
            foreach (var item in _CallbackList)
            {
                item.Value.AnnounceClientAgeToClients(newValue);
            }
        }

        public void Ping()
        {
            var test = _CallbackList.Keys.ToList();
            for (int i = _CallbackList.Count - 1; i >= 0; i--)
            {
                try
                {
                    _CallbackList[test[i]].Pulse();
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

        public void AnnounceClientAgeToClients(int newValue)
        {
            ClientAge = newValue;
        }

        public void Subscribe(string ServiceName, ISubscription Client)
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
