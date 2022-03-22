using Prinubes.vCenterSDK;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Prinubes.vCenterSDK;

namespace PlatformWorker
{


    public class PropertyFilterUpdateEventArgs : EventArgs
    {
        private PropertyFilterUpdate _filterUpdate;
        public PropertyFilterUpdate FilterUpdate
        {
            get { return _filterUpdate; }
        }
        public PropertyFilterUpdateEventArgs(PropertyFilterUpdate filterUpdate)
        {
            _filterUpdate = filterUpdate;
        }
    }
    public class UpdateSetEventArgs : EventArgs
    {
        private UpdateSet _updateSet;
        public UpdateSet UpdateSet
        {
            get { return _updateSet; }
        }
        public UpdateSetEventArgs(UpdateSet updateSet)
        {
            _updateSet = updateSet;
        }
    }
    public class ListenerExceptionEventArgs : EventArgs
    {
        private Exception _exception;
        public Exception Exception
        {
            get { return _exception; }
        }
        public ListenerExceptionEventArgs(Exception exception)
        {
            _exception = exception;
        }
    }
    public delegate void ListenerExceptionHandler(object sender, ListenerExceptionEventArgs eventArgs);
    public delegate void UpdateSetHandler(object sender, UpdateSetEventArgs eventArgs);
    public delegate void PropertyFilterUpdateHandler(object sender, PropertyFilterUpdateEventArgs eventArgs);
    public class PropertyManager
    {
        private object _thisLock = new();
        private event AsyncCallback WaitForUpdatesDone;
        private UpdateSet _updateSetResult;
        private string _version;
        private event UpdateSetHandler UpdateSet;
        private Dictionary<string, PropertyFilterUpdateHandler> _filterUpdates;
        private bool _isListening;
        private ServiceContent _context;
        public ServiceContent Context
        {
            get { return _context; }
        }
        private SvcConnection _connection;
        public SvcConnection Connection
        {
            get { return _connection; }
        }
        public event ListenerExceptionHandler ListenerException;
        public PropertyManager(SvcConnection connection, ServiceContent context)
        {
            _context = context;
            _connection = connection;
            WaitForUpdatesDone += new AsyncCallback(PropertyManager_WaitForUpdatesDone);
            _filterUpdates = new Dictionary<string, PropertyFilterUpdateHandler>();
            _isListening = false;
        }
        public void CheckForUpdatesAsync()
        {
            lock (_thisLock)
            {
                try
                {
                    UpdateSet update = Connection.Service.CheckForUpdatesAsync(Context.propertyCollector, _version).Result;
                    Dispatch(update);
                }
                catch (Exception x)
                {
                    ReportException(x);
                }
            }
        }

        public SvcConnection GetConnection()
        {
            return Connection;
        }

        public async Task StartListeningAsync(SvcConnection connection)
        {
            lock (_thisLock)
            {
                _isListening = true;
                _updateSetResult = connection.Service.WaitForUpdatesAsync(Context.propertyCollector, _version).Result;
            }
        }
        public ManagedObjectReference Register(PropertyFilterSpec spec, bool partialUpdates, PropertyFilterUpdateHandler handler)
        {
            lock (_thisLock)
            {
                ManagedObjectReference ret = Connection.Service.CreateFilterAsync(Context.propertyCollector, spec, partialUpdates).Result;
                Register(ret, handler);
                return ret;
            }
        }
        public void Register(ManagedObjectReference filter, PropertyFilterUpdateHandler handler)
        {
            lock (_thisLock)
            {
                if (_filterUpdates.ContainsKey(filter.Value))
                {
                    _filterUpdates[filter.Value] += handler;
                }
                else
                {
                    _filterUpdates[filter.Value] = handler;
                }
            }
        }
        public void Register(UpdateSetHandler handler)
        {
            lock (_thisLock)
            {
                UpdateSet += handler;
            }
        }
        public void Unregister(ManagedObjectReference filter, PropertyFilterUpdateHandler handler)
        {
            lock (_thisLock)
            {
                if (_filterUpdates.ContainsKey(filter.Value))
                {
                    _filterUpdates[filter.Value] -= handler;
                }
            }
        }
        public void Unregister(UpdateSetHandler handler)
        {
            lock (_thisLock)
            {
                UpdateSet -= handler;
            }
        }
        public void UnregisterAll(ManagedObjectReference filter)
        {
            lock (_thisLock)
            {
                if (filter != null)
                {
                    if (_filterUpdates.ContainsKey(filter.Value))
                    {
                        PropertyFilterUpdateHandler handler = _filterUpdates[filter.Value];
                        handler = null;
                        _filterUpdates[filter.Value] = null;
                    }
                }
                else
                {
                    UpdateSet = null;
                }
            }
        }
        private void Dispatch(UpdateSet update)
        {
            lock (_thisLock)
            {
                if (update != null)
                {
                    // ok. next version please
                    _version = update.version;
                    if (UpdateSet != null)
                    {
                        UpdateSet(this, new UpdateSetEventArgs(update));
                    }
                    foreach (PropertyFilterUpdate pfu in update.filterSet)
                    {
                        if (_filterUpdates.ContainsKey(pfu.filter.Value))
                        {
                            _filterUpdates[pfu.filter.Value](this, new PropertyFilterUpdateEventArgs(pfu));
                        }
                    }
                }
            }
        }
        private void PropertyManager_WaitForUpdatesDone(IAsyncResult ar)
        {
            lock (_thisLock)
            {
                _updateSetResult = null;
                try
                {
                    UpdateSet update = Connection.Service.WaitForUpdatesAsync(Context.propertyCollector, _version).Result;
                    Dispatch(update);
                }
                catch (Exception x)
                {
                    ReportException(x);
                }
                try
                {
                    // Restart the listening loop
                    StartListeningAsync(GetConnection());
                }
                catch (Exception x)
                {
                    ReportException(x);
                }
            }
        }
        private void ReportException(Exception x)
        {
            WebException wx = x as WebException;
            if (wx == null || wx.Status != WebExceptionStatus.RequestCanceled)
            {
                if (ListenerException != null)
                {
                    ListenerException(this, new ListenerExceptionEventArgs(x));
                }
            }
        }
    }
    public class TaskWaiter
    {
        private PropertyManager _pm;
        public PropertyManager PropertyManager
        {
            get { return _pm; }
        }
        private ManagedObjectReference _task;
        public ManagedObjectReference Task
        {
            get { return _task; }
        }
        private PropertyFilterSpec _pfSpec;
        public PropertyFilterSpec PropertyFilterSpec
        {
            get { return _pfSpec; }
        }
        private ManualResetEvent _done;
        public TaskWaiter(PropertyManager pm, ManagedObjectReference task)
        {
            _task = task;
            _pm = pm;
            ObjectSpec oSpec = new ObjectSpec();
            oSpec.obj = task;
            oSpec.skip = false; oSpec.skipSpecified = true;
            PropertySpec pSpec = new PropertySpec();
            pSpec.all = false; pSpec.allSpecified = true;
            pSpec.pathSet = new String[] { "info.error", "info.state" };
            pSpec.type = task.type;
            _pfSpec = new PropertyFilterSpec();
            _pfSpec.objectSet = new ObjectSpec[] { oSpec };
            _pfSpec.propSet = new PropertySpec[] { pSpec };
        }
        public void WaitForTask()
        {
            WaitForTaskAsync(-1);
        }
        public async Task WaitForTaskAsync(int millisecondsTimeout)
        {
            _done = new ManualResetEvent(false);
            ManagedObjectReference filter = _pm.Register(_pfSpec, false, new PropertyFilterUpdateHandler(WaitForTaskHandler));
            _done.WaitOne(millisecondsTimeout, true);
            _pm.UnregisterAll(filter);
            await _pm.Connection.Service.DestroyPropertyFilterAsync(filter);
        }
        private void WaitForTaskHandler(object sender, PropertyFilterUpdateEventArgs args)
        {
            foreach (PropertyChange change in args.FilterUpdate.objectSet[0].changeSet)
            {
                System.Console.WriteLine("Change: name->{0}, op->{1}, val->{2}", change.name, change.op, change.val);
                if (change.name == "info.error")
                {
                    if ((change.op == PropertyChangeOp.assign ||
                         change.op == PropertyChangeOp.add) &&
                         change.val != null)
                    {
                        // Signal we are done...
                        _done.Set();
                    }
                }
                else if (change.name == "info.state")
                {
                    if (change.op == PropertyChangeOp.assign ||
                       change.op == PropertyChangeOp.add)
                    {
                        TaskInfoState tis = (TaskInfoState)change.val;
                        if (tis != TaskInfoState.queued && tis != TaskInfoState.running)
                        {
                            // Signal we are done...
                            _done.Set();
                        }
                    }
                }
            }
        }
    }
}
