using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Cenet.ActionsAudit
{

    public class AuditInspector : IParameterInspector, IErrorHandler
    {

        ServiceDescription serviceDescription;
        ServiceHostBase serviceHostBase;
        public AuditInspector(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            this.serviceDescription= serviceDescription;
            this.serviceHostBase = serviceHostBase;
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            return Stopwatch.StartNew();
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            var watch = (Stopwatch)correlationState;
            watch.Stop();
            var time = watch.ElapsedMilliseconds;
            // Do something with the result
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            
        }

        public bool HandleError(Exception error)
        {
            return false;
        }
    }
    public class AuditBehaviorExtension : BehaviorExtensionElement, IServiceBehavior
    {

        public override Type BehaviorType
        {
            get { return typeof(AuditBehaviorExtension); }
        }

        protected override object CreateBehavior()
        {
            return this;
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (var endpoint in channelDispatcher.Endpoints)
                {
                    foreach (var operation in endpoint.DispatchRuntime.Operations)
                    {
                        var inspector = new AuditInspector(serviceDescription,serviceHostBase);
                        operation.ParameterInspectors.Add(inspector);
                    }
                }
            }
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
