namespace CJR.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Practices.ServiceLocation;
    using StructureMap;

    public class StructureMapServiceLocator : ServiceLocatorImplBase
    {
        private IContainer _container;

        public StructureMapServiceLocator(IContainer container)
        {
            _container = container;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            return string.IsNullOrEmpty(key) ? _container.GetInstance(serviceType) : _container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType).OfType<object>();
        }
    }
}