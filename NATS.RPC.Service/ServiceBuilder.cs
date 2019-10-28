﻿using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using System;
using System.Collections.Generic;

namespace NATS.RPC.Service
{
    public class ServiceBuilder : IServiceBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceFactory _serviceFactory;

        private readonly ServiceOptions _serviceOptions;

        private List<ContractHandler> _contractHandlers;
        private EventHandler<MsgHandlerEventArgs> _eventHandler;

        public ServiceBuilder(IServiceProvider serviceProvider, ServiceFactory serviceFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));

            _serviceOptions = ServiceOptions.Default;
            _contractHandlers = new List<ContractHandler>();
        }

        public ServiceBuilder Configure(Action<ServiceOptions> options)
        {
            options.Invoke(_serviceOptions);
            return this;
        }

        public ServiceBuilder AddMsgEventHandler(EventHandler<MsgHandlerEventArgs> eventHandler)
        {
            _eventHandler = eventHandler;
            return this;
        }

        public ServiceBuilder AddContractHandler<TContract, TImplementation>(Func<IServiceProvider, ObjectFactory> factory = null)
            where TContract : class
            where TImplementation : class, TContract
        {
            var contractImplFactory = factory != null ? factory.Invoke(_serviceProvider) : 
                ActivatorUtilities.CreateFactory(typeof(TImplementation), Array.Empty<Type>());

            var handler = new ContractHandler(_serviceProvider, typeof(TContract), _serviceOptions.ServiceUid, contractImplFactory);
            _contractHandlers.Add(handler);

            return this;
        }

        public Service Build()
        {
            return _serviceFactory.Create(_serviceOptions, _contractHandlers, _eventHandler);
        }
    }
}