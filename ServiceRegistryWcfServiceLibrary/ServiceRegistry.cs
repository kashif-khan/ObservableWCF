using ServiceRegistryWcfServiceLibrary.PeerDiagnosticServiceReference;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Timers;
using Utilities;

namespace ServiceRegistryWcfServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceRegistry : IServiceRegistry
    {
        private static Timer _timer = new Timer();
        private static List<string> Servers = new List<string>();
        private static IWriter _writer;
        private static Stopwatch sw = new Stopwatch();

        public ServiceRegistry(IWriter writer)
        {
            _timer = new Timer();
            _timer.Interval = 5000; //TODO Make this value configurable
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            _writer = writer;
        }

        public ServiceRegistry() : this(new ConsoleWriter())
        {

        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PingAllServers();
        }

        public bool Online(string Url)
        {
            if (!Servers.Contains(Url))
            {
                Servers.Add(Url);
                _writer.WriteLine($"{Url} is online...");
                return true;
            }
            return false;
        }

        public void PingAllServers()
        {
            for (int i = Servers.Count - 1; i >= 0; i--)
            {
                var currentServer = Servers[i];
                try
                {
                    sw.Restart();
                    _writer.WriteLine($"Pinging {currentServer} in progress...", State.Yellow);
                    var basicBinding = new BasicHttpBinding();
                    var address = new EndpointAddress(currentServer.Replace("peer", "diagnostic"));
                    var serverFactory = new ChannelFactory<IDiagnostic>(basicBinding);
                    var serverReference = serverFactory.CreateChannel(address);
                    serverReference.Ping();
                    _writer.WriteLine($"Pinged {currentServer} and it took {sw.Elapsed} ms");
                }
                catch (Exception ex)
                {
                    _writer.WriteLine($"{currentServer} is not reachable. Hence it is being removed from the service registry.");
                    Servers.Remove(currentServer);
                    _writer.WriteLine($"{currentServer} is removed from the service registry.");
                }
            }
        }

        public List<string> GetServersList()
        {
            return Servers;
        }
    }
}
