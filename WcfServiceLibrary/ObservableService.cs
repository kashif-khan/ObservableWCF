using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Text;
using System.Timers;
using Utilities;
using WcfServiceLibrary.PeerServiceReference;
using WcfServiceLibrary.ServiceRegistryReference;

namespace WcfServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ObservableService : IUserService, ISubscription, IServiceRegistryCallback, IDiagnostic
    {
        private static Dictionary<string, INotifications> _PeersCallbackList = new Dictionary<string, INotifications>();
        private static List<string> _PeersAddressList = new List<string>();
        private int ClientAge;
        private string ServiceName { get; }
        //TODO make timer configurable
        private Timer _timer = new Timer() { Interval = 5000 };
        private static IWriter _writer;
        private static bool FirstTime = true;
        private static InstanceContext _callback;
        private static List<string> FailedPeers = new List<string>();
        private static ServiceRegistryClient _ServiceRegistry;

        public ObservableService() : this(new ConsoleWriter())
        {

        }

        public ObservableService(IWriter writer)
        {
            _writer = writer;
            _callback = new InstanceContext(this);
            _writer.WriteLine("Calling construction...", State.Yellow);
            ServiceName = ReadServiceName();
            _timer.Elapsed += Timer_Elapsed;
            _timer.Elapsed += Registration;
            _timer.Start();
        }

        private void RegisterWithServiceRegistry()
        {
            if (_ServiceRegistry == null)
            {
                try
                {
                    string PeerEndpointAddress = string.Empty;
                    _ServiceRegistry = new ServiceRegistryClient(new InstanceContext(this));
                    PeerEndpointAddress = ReadEndpointAddress();
                    _ServiceRegistry.Online(PeerEndpointAddress);
                }
                catch (Exception ex)
                {
                    _ServiceRegistry = null;
                    _writer.WriteLine($"Could not register with service registry. Exception shown below:{Environment.NewLine}{ex.Message}");
                }
            }
        }

        private static string ReadEndpointAddress()
        {
            string PeerEndpointAddress = string.Empty;
            var serviceModel = ServiceModelSectionGroup.GetSectionGroup(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None));
            var endpoints = serviceModel.Services.Services;
            foreach (ServiceElement e in endpoints)
            {
                if (e.Name == "WcfServiceLibrary.ObservableService")
                {
                    foreach (ServiceEndpointElement item in e.Endpoints)
                    {
                        if (item.Name == "PeerEndpoint")
                        {
                            PeerEndpointAddress = $"{e.Host.BaseAddresses[0].BaseAddress.ToString()}/{item.Address.ToString()}";
                            break;
                        }
                    }
                    break;
                }
            }
            return PeerEndpointAddress;
        }

        private static string ReadServiceName()
        {
            string _ServiceName = string.Empty;
            var serviceModel = ServiceModelSectionGroup.GetSectionGroup(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None));
            var endpoints = serviceModel.Services.Services;
            foreach (ServiceElement e in endpoints)
            {
                if (e.Name == "WcfServiceLibrary.ObservableService")
                {
                    foreach (ServiceEndpointElement item in e.Endpoints)
                    {
                        if (item.Name == "PeerEndpoint")
                        {
                            _ServiceName = $"{e.Host.BaseAddresses[0].BaseAddress.ToString()}";
                            var test = new Uri(_ServiceName);
                            _ServiceName = $"{test.Host}:{test.Port}";

                            break;
                        }
                    }
                    break;
                }
            }
            return _ServiceName;
        }

        private void Registration(object sender, ElapsedEventArgs e)
        {
            RegisterWithServiceRegistry();
            if (FirstTime)
            {
                FirstTime = false;
                var PeerServersList = _ServiceRegistry?.GetServersList();
                if (PeerServersList != null)
                {
                    foreach (var peer in PeerServersList)
                    {
                        try
                        {
                            if (ReadEndpointAddress() != peer)
                            {
                                Subscribe(_callback, peer);
                            }
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
            foreach (var peer in _PeersCallbackList)
            {
                _writer.WriteLine($"Sending the update to client ==> {peer.Key}");
                peer.Value.UpdateData(newValue);
            }
        }

        public void UpdateData(int newValue)
        {
            _writer.WriteLine($"Recieved an update ==> {newValue} from {ClientAge}");
            ClientAge = newValue;
        }

        public void Unsubscribe(string ServiceName)
        {
            if (_PeersCallbackList.Keys.Contains(ServiceName))
            {
                _PeersCallbackList.Remove(ServiceName);
            }
        }

        public void Subscribe(string ServiceName)
        {
            try
            {
                if (!_PeersCallbackList.Keys.Contains(ServiceName))
                {
                    _writer.WriteLine($"Adding subscriber {ServiceName}...", State.Yellow);
                    _PeersCallbackList.Add(ServiceName, OperationContext.Current.GetCallbackChannel<INotifications>());
                    _writer.WriteLine($"Adding subscriber {ServiceName} Success...", State.Green);
                }
            }
            catch (Exception ex)
            {
                _writer.WriteLine($"Registration failed due to {ex.Message}", State.Red);
            }
        }

        public void NewServiceAdded(string Url)
        {
            if (!_PeersAddressList.Contains(Url))
            {
                _PeersAddressList.Add(Url);
                _writer.WriteLine($"{Url} added as a peer", State.Yellow);
            }
        }

        public void ServiceRemoved(string Url)
        {
            if (_PeersAddressList.Contains(Url))
            {
                _PeersAddressList.Remove(Url);
                _writer.WriteLine($"{Url} removed from the list of peer", State.Yellow);
            }
        }

        bool IDiagnostic.Ping()
        {
            return true;
        }

        bool IServiceRegistryCallback.Ping()
        {
            return true;
        }
    }
}
