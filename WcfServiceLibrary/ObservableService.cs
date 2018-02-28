using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Timers;
using WcfServiceLibrary.PeerServiceReference;

namespace WcfServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
    public class ObservableService : IUserService, ISubscription
    {
        private static Dictionary<string, INotifications> _Peers = new Dictionary<string, INotifications>();
        private int ClientAge;
        private string ServiceName { get; }
        private Timer _timer = new Timer() { Interval = 5000 };
        private IWriter _writer;
        private static bool FirstTime = true;
        private static InstanceContext _callback;
        private static List<string> FailedPeers = new List<string>();

        public ObservableService() : this(new ConsoleWriter())
        {

        }
        public ObservableService(IWriter writer)
        {
            _writer = writer;
            _callback = new InstanceContext(this);
            _writer.WriteLine("Calling construction...", State.Yellow);
            ServiceName = ConfigurationManager.AppSettings["ServiceName"];
            _timer.Elapsed += Timer_Elapsed;
            _timer.Elapsed += Registration;
            _timer.Start();
        }

        private void Registration(object sender, ElapsedEventArgs e)
        {
            if (FirstTime)
            {
                FirstTime = false;
                var PeerServersList = ConfigurationManager.AppSettings["PeerServers"].Split(';');
                foreach (var peer in PeerServersList)
                {
                    try
                    {
                        Subscribe(_callback, peer);
                    }
                    catch (Exception ex)
                    {
                        if (!FailedPeers.Contains(peer))
                        {
                            FailedPeers.Add(peer);
                        }
                        _writer.WriteLine($"Message: {ex.Message}", State.Red);
                        //TODO handle the exception
                    }
                }
            }
        }

        private void Subscribe(InstanceContext callback, string peer)
        {
            var dualBinding = new WSDualHttpBinding();
            var address = new EndpointAddress(peer);
            var PeerServiceFactory = new DuplexChannelFactory<ISubscription>(callback, dualBinding);
            _writer.WriteLine($"Registration started for {peer}...", State.Yellow);
            var PeerService = PeerServiceFactory.CreateChannel(address);
            PeerService.Subscribe(ServiceName);
            _writer.WriteLine($"Registration completed successfully for {peer}...", State.Green);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //_writer.WriteLine("Pinging the data");
            //this.Ping();
            this.RetrySubscription();
        }

        private void RetrySubscription()
        {
            for (int i = FailedPeers.Count - 1; i >= 0; i++)
            {
                try
                {
                    Subscribe(_callback, FailedPeers[i]);
                    FailedPeers.Remove(FailedPeers[i]);
                }
                catch (Exception ex)
                {
                    _writer.WriteLine($"Retry registration failed for {FailedPeers[i]}", State.Red);
                }
            }
        }

        public void MethodThatWillChangeData(int newValue)
        {
            _writer.WriteLine($"MethodThatWillChangeData with {newValue}");
            var PreviousClientAge = ClientAge;
            ClientAge = newValue;
            foreach (var peer in _Peers)
            {
                _writer.WriteLine($"Sending the update to client ==> {peer.Key}");
                peer.Value.UpdateData(newValue);
            }
        }

        public void Ping()
        {
            var test = _Peers.Keys.ToList();
            for (int i = _Peers.Count - 1; i >= 0; i--)
            {
                try
                {
                    _writer.WriteLine($"Checking the pulse of {test[i]}", State.Yellow);
                    (_Peers[test[i]] as IHeartbeat).Pulse();
                }
                catch (Exception ex)
                {
                    _writer.WriteLine($"Pulse failed due to {ex.Message}", State.Red);
                    _Peers.Remove(test[i]);
                }
            }
        }

        public bool Pulse()
        {
            _writer.WriteLine($"Pulse requested...");
            return true;
        }

        public void UpdateData(int newValue)
        {
            _writer.WriteLine($"Recieved an update ==> {newValue} from {ClientAge}");
            ClientAge = newValue;
        }

        public void Unsubscribe(string ServiceName)
        {
            if (_Peers.Keys.Contains(ServiceName))
            {
                _Peers.Remove(ServiceName);
            }
        }

        public void Subscribe(string ServiceName)
        {
            try
            {
                if (!_Peers.Keys.Contains(ServiceName))
                {
                    _writer.WriteLine($"Adding subscriber {ServiceName}...", State.Yellow);
                    _Peers.Add(ServiceName, OperationContext.Current.GetCallbackChannel<INotifications>());
                    _writer.WriteLine($"Adding subscriber {ServiceName} Success...", State.Green);
                }
            }
            catch (Exception ex)
            {
                _writer.WriteLine($"Registration failed due to {ex.Message}", State.Red);
            }
        }
    }
}
