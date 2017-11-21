using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using SuperNova.Shared.Configs;
using SuperNova.Shared.Tests;
using Microsoft.Extensions.Logging;

namespace SuperNova.StorageTest.FakeStorage
{
    public class FakeServiceCollection : IServiceCollection
    {
        private List<ServiceDescriptor> _coreList = new List<ServiceDescriptor>();

        public ServiceDescriptor this[int index] { get => _coreList[index]; set => _coreList[index] = value; }

        public int Count => _coreList.Count;

        public bool IsReadOnly => false;

        public void Add(ServiceDescriptor item)
        {
            _coreList.Add(item);

            if(item.ImplementationFactory != null )
            {
                // Lets not kick that far yet
                // item.ImplementationFactory(new FakeServiceProvider());
            }
        }

        public void Clear() => _coreList.Clear();

        public bool Contains(ServiceDescriptor item) => _coreList.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<ServiceDescriptor> GetEnumerator() => _coreList.GetEnumerator();

        public int IndexOf(ServiceDescriptor item) => _coreList.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item) => _coreList.Insert(index, item);

        public bool Remove(ServiceDescriptor item) => _coreList.Remove(item);

        public void RemoveAt(int index) => _coreList.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => _coreList.GetEnumerator();
    }

    public class FakeServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            if (serviceType.Equals(typeof(ConfigStore)))
            {
                return new TestConfigStore(new FakeLogFactory());
            }
            else if (serviceType.Equals(typeof(ILoggerFactory)))
            {
                return new FakeLogFactory();
            }
            else return null;
        }
    }
}
