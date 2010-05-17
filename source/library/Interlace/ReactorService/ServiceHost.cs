#region Using Directives and Copyright Notice

// Copyright (c) 2007-2010, Computer Consultancy Pty Ltd
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Computer Consultancy Pty Ltd nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL COMPUTER CONSULTANCY PTY LTD BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Interlace.Collections;
using Interlace.ReactorCore;

#endregion

namespace Interlace.ReactorService
{
    public class ServiceHost : IDisposable
    {
        Thread _thread = null;

        List<IService> _services = new List<IService>();
        List<IService> _permanentServices = new List<IService>();

        ServiceState _state = ServiceState.Down;
        ServiceStateRequest _stateRequest = ServiceStateRequest.None;
        ManualResetEvent _stateRequestEvent;
        ServiceStateRequest _pendingRequest = ServiceStateRequest.None;

        TimeSpan _startingExceptionWaitTimeout = new TimeSpan(0, 1, 0);
        TimeSpan _upExceptionWaitTimeout = new TimeSpan(0, 1, 0);

        TimeSpan _serviceHostStopTimeout = new TimeSpan(0, 0, 30);

        DateTime _startingExceptionWaitFinishAt;
        DateTime _upExceptionWaitFinishAt;

        object _stateRequestLock = new object();

        Dictionary<string, object> _environment = null;

        Dictionary<string, object> _defaultEnvironment = null;

        public event EventHandler<ServiceExceptionEventArgs> UnhandledException;
        public event EventHandler<ServiceHostStateChangedEventArgs> StateChanged;

        Reactor _reactor;

        string _description;

        public ServiceHost()
        {
            _defaultEnvironment = new Dictionary<string, object>();

            _stateRequestEvent = new ManualResetEvent(false);

            _reactor = new Reactor();
            _reactor.ReactorException += new EventHandler<ServiceExceptionEventArgs>(_reactor_ReactorException);

            _reactor.AddPermanentHandle(_stateRequestEvent, StateRequestEventFired);
            _reactor.AddRepeatingTimer(new TimeSpan(0, 0, 10), ExceptionWaitTimer, null);
        }

        void _reactor_ReactorException(object sender, ServiceExceptionEventArgs e)
        {
            if (UnhandledException != null) UnhandledException(this, 
                new ServiceExceptionEventArgs(e.Kind, e.Exception));

            if (_state == ServiceState.Up)
            {
                CloseAll();
            }

            SetState(ServiceState.UpExceptionWait);

            _upExceptionWaitFinishAt = DateTime.Now + _upExceptionWaitTimeout;

            e.Handled = true;
        }

        public Dictionary<string, object> DefaultEnvironment
        {
            get { return _defaultEnvironment; }
        }

        public void Dispose()
        {
            if (_stateRequestEvent != null) _stateRequestEvent.Close();
            _stateRequestEvent = null;
        }

        public void AddService(IService service)
        {
            if (_thread != null) 
            {
                throw new InvalidOperationException(
                    "A service can not be added to the service host after it has been started.");
            }

            _services.Add(service);
        }

        public void AddPermanentService(IService service)
        {
            if (_thread != null) 
            {
                throw new InvalidOperationException(
                    "A permanent service can not be added to the service host after it has been started.");
            }

            _permanentServices.Add(service);
        }

        public void StartServiceHost()
        {
            // Do an initial sorting; as long as no permanent services add services,
            // any sorting problems should be thrown here:
            SortServices(_services);
            SortServices(_permanentServices);

            _thread = new Thread(ThreadMain);
            _thread.Name = _description;

            _thread.Start();
        }

        void StateRequestEventFired(IAsyncResult result, object state)
        {
            lock (_stateRequestLock)
            {
                _pendingRequest = _stateRequest;
                _stateRequest = ServiceStateRequest.None;
                _stateRequestEvent.Reset();
            }
        }

        void ExceptionWaitTimer(DateTime fireAt, object state)
        {
            if (_pendingRequest == ServiceStateRequest.None)
            {
                if (_state == ServiceState.StartingExceptionWait)
                {
                    if (DateTime.Now > _startingExceptionWaitFinishAt) OpenServices();
                }
                else if (_state == ServiceState.UpExceptionWait)
                {
                    if (DateTime.Now > _upExceptionWaitFinishAt) OpenServices();
                }
            }
        }

        void ThreadMain()
        {
            ServiceHostImplementation host = new ServiceHostImplementation(_reactor, _defaultEnvironment, _services);

            bool permanentServicesOpened = OpenListOfServices(_permanentServices, host, 
                ServiceExceptionKind.DuringPermanentOpen, ServiceExceptionKind.DuringPermanentOpenAbort);

            try
            {
                SortServices(_services);
            }
            catch (Exception ex)
            {
                if (UnhandledException != null) UnhandledException(this, 
                    new ServiceExceptionEventArgs(ServiceExceptionKind.DuringPermanentOpen, ex));

                permanentServicesOpened = false;
            }

            bool running = true;

            while (running)
            {
                _reactor.RunLoopIteration();

                ServiceStateRequest request = _pendingRequest;
                _pendingRequest = ServiceStateRequest.None;

                switch (request)
                {
                    case ServiceStateRequest.Open:
                        if (permanentServicesOpened) OpenWasRequested();
                        break;

                    case ServiceStateRequest.Close:
                        if (permanentServicesOpened) CloseWasRequested();
                        break;

                    case ServiceStateRequest.Shutdown:
                        running = false;
                        break;
                }
            }

            if (_state == ServiceState.Up)
            {
                CloseAll();
                _state = ServiceState.Down;
            }

            if (permanentServicesOpened) CloseListOfServices(_permanentServices, host, ServiceExceptionKind.DuringPermanentClose);
        }

        public void StopServiceHost()
        {
            lock (_stateRequestLock)
            {
                _stateRequest = ServiceStateRequest.Shutdown;
                _stateRequestEvent.Set();
            }

            bool joined = _thread.Join(_serviceHostStopTimeout);

            if (!joined) 
            {
                _thread.Abort();

                throw new InvalidOperationException(
                    "The service host thread failed to shut down in the specified time.");
            }
        }

        /// <summary>
        /// Attempts to bring the service up, or to restart it if it is already 
        /// started.
        /// </summary>
        public void OpenServices()
        {
            lock (_stateRequestLock)
            {
                if (_stateRequest != ServiceStateRequest.Shutdown)
                {
                    _stateRequest = ServiceStateRequest.Open;
                    _stateRequestEvent.Set();
                }
            }
        }

        /// <summary>
        /// Attempts to shutdown the services.
        /// </summary>
        public void CloseServices()
        {
            lock (_stateRequestLock)
            {
                if (_stateRequest != ServiceStateRequest.Shutdown)
                {
                    _stateRequest = ServiceStateRequest.Close;
                    _stateRequestEvent.Set();
                }
            }
        }

        void SetState(ServiceState newState)
        {
            ServiceState oldState = _state;

            _state = newState;

            try
            {
                if (StateChanged != null) StateChanged(this, new ServiceHostStateChangedEventArgs(oldState, newState));
            }
            catch (Exception)
            {
                // Ignore exceptions; doing anything else is liable to cause problems in one
                // of the many places the state is changed.
            }
        }

        void OpenWasRequested()
        {
            bool successful = false;

            switch (_state)
            {
                case ServiceState.Down:
                    SetState(ServiceState.Starting);
                    successful = OpenAll();
                    break;

                case ServiceState.StartingExceptionWait:
                    SetState(ServiceState.Starting);
                    successful = OpenAll();
                    break;

                case ServiceState.Up:
                    SetState(ServiceState.Restarting);
                    CloseAll();
                    successful = OpenAll();
                    break;

                case ServiceState.UpExceptionWait:
                    SetState(ServiceState.Restarting);
                    successful = OpenAll();
                    break;
            }

            if (successful)
            {
                SetState(ServiceState.Up);
            }
            else
            {
                SetState(ServiceState.StartingExceptionWait);

                _startingExceptionWaitFinishAt = DateTime.Now + _startingExceptionWaitTimeout;
            }
        }

        void CloseWasRequested()
        {
            switch (_state)
            {
                case ServiceState.Down:
                    break;

                case ServiceState.StartingExceptionWait:
                    // Do nothing.
                    SetState(ServiceState.Down);
                    break;

                case ServiceState.Up:
                    SetState(ServiceState.Stopping);
                    CloseAll();
                    SetState(ServiceState.Down);
                    break;

                case ServiceState.UpExceptionWait:
                    // Do nothing.
                    SetState(ServiceState.Down);
                    break;
            }
        }

        static void SortServices(List<IService> services)
        {
            // Build a list of the services that also implement IServiceDependencies:
            List<IServiceDependencies> dependenciesList = new List<IServiceDependencies>();

            foreach (IService service in services)
            {
                if (service is IServiceDependencies)
                {
                    IServiceDependencies dependencies = service as IServiceDependencies;

                    dependenciesList.Add(dependencies);
                }
            }

            // Build a dictionary of service tags to services:
            Dictionary<string, IService> serviceTags = new Dictionary<string, IService>();

            foreach (IServiceDependencies dependencies in dependenciesList)
            {
                foreach (string serviceTag in dependencies.ProvidesServiceTags)
                {
                    if (serviceTags.ContainsKey(serviceTag))
                    {
                        throw new ServiceHostException(string.Format(
                            "Multiple services are advertising that they provide the \"{0}\" service.",
                            serviceTag));
                    }

                    serviceTags[serviceTag] = dependencies as IService;
                }
            }

            // Build a map from required services to requiring services:
            Dictionary<IService, Set<IService>> dependsOn = new Dictionary<IService, Set<IService>>();

            foreach (IServiceDependencies dependencies in dependenciesList)
            {
                foreach (string serviceTag in dependencies.RequiresServiceTags)
                {
                    if (!serviceTags.ContainsKey(serviceTag))
                    {
                        throw new ServiceHostException(string.Format(
                            "No service is providing the \"{0}\" service tag.", serviceTag));
                    }

                    IService requiredService = serviceTags[serviceTag];

                    if (!dependsOn.ContainsKey(requiredService))
                    {
                        dependsOn[requiredService] = new Set<IService>();
                    }

                    dependsOn[requiredService].UnionUpdate(dependencies as IService);
                }
            }

            // Sort the services:
            ICollection<IService> sortedServices = TopologicalSort.Sort(services, 
                delegate(IService service) 
                { 
                    Set<IService> dependsOnSet;

                    if (dependsOn.TryGetValue(service, out dependsOnSet))
                    {
                        return (IEnumerable<IService>)dependsOnSet;
                    }
                    else
                    {
                        return (IEnumerable<IService>)new IService[] { };
                    }
                });

            services.Clear();
            services.AddRange(sortedServices);
        }

        bool OpenListOfServices(List<IService> services, IServiceHost host, 
            ServiceExceptionKind openExceptionKind, ServiceExceptionKind openAbortExceptionKind)
        {
            Stack<IService> startedServices = new Stack<IService>();

            try
            {
                foreach (IService service in services)
                {
                    service.Open(host);

                    startedServices.Push(service);
                }

                return true;
            }
            catch (Exception ex)
            {
                while (startedServices.Count > 0)
                {
                    IService serviceToShutdown = startedServices.Pop();

                    try
                    {
                        serviceToShutdown.Close(host);
                    }
                    catch (Exception nestedEx)
                    {
                        if (UnhandledException != null) UnhandledException(this, 
                            new ServiceExceptionEventArgs(openAbortExceptionKind, nestedEx));
                    }
                }

                if (UnhandledException != null) UnhandledException(this, 
                    new ServiceExceptionEventArgs(openExceptionKind, ex));

                return false;
            }
        }

        void CloseListOfServices(List<IService> services, IServiceHost host, 
            ServiceExceptionKind closeExceptionKind)
        {
            List<IService> reverseList = new List<IService>(services);
            reverseList.Reverse();

            foreach (IService service in reverseList)
            {
                try
                {
                    service.Close(host);
                }
                catch (Exception ex)
                {
                    if (UnhandledException != null) UnhandledException(this, 
                        new ServiceExceptionEventArgs(closeExceptionKind, ex));
                }
            }
        }

        bool OpenAll()
        {
            _environment = new Dictionary<string, object>();

            IServiceHost host = new ServiceHostImplementation(_reactor, _environment, null);

            foreach (KeyValuePair<string, object> pair in _defaultEnvironment)
            {
                _environment.Add(pair.Key, pair.Value);
            }

            return OpenListOfServices(_services, host, ServiceExceptionKind.DuringOpen, ServiceExceptionKind.DuringOpenAbort);
        }

        void CloseAll()
        {
            IServiceHost host = new ServiceHostImplementation(_reactor, _environment, null);

            CloseListOfServices(_services, host, ServiceExceptionKind.DuringClose);

            _environment = null;
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
    }
}
