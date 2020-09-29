using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Cenet.ActionsAudit
{
    public class AuditBehavior : BehaviorExtensionElement, IEndpointBehavior
    {

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            AuditMessageInspector inspector = new AuditMessageInspector();
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
        }

        public class AuditMessageInspector : IDispatchMessageInspector
        {
            public void BeforeSendReply(ref Message reply, object correlationState)
            {

                try
                {
                   

                    if (AuditContext.Current != null && AuditContext.Current.CanAudit)
                    {
                        AuditContext.Current.ExecutionTime.Stop();

                        AuditEntity audit = AuditContext.Current.AuditObject;

                        audit.Excepcion = reply.IsFault;
                        audit.TiempoEjecucion = AuditContext.Current.ExecutionTime.ElapsedMilliseconds;

                        Action<object> action = (obj) => SaveAudit(obj);
                        Task.Factory.StartNew(action, audit);
                    }
                }
                catch(Exception ex)
                {
                    //auditar errores
                }
            }

            public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
            {

                string actionName = request.Headers.Action.Substring(request.Headers.Action.LastIndexOf('/') + 1);
                if (!string.IsNullOrEmpty(actionName))
                {
                    var methodInfo = instanceContext.Host.Description.ServiceType.GetMethod(actionName);
                    if (methodInfo != null)
                    {
                        var customAttributes = methodInfo.GetCustomAttributes(false);
                        var auditAttribute = customAttributes.Where(ca => ca.GetType().Equals(typeof(Audit2Attribute))).FirstOrDefault();
                        if (auditAttribute != null)
                        {

                            string ref1 = (auditAttribute as Audit2Attribute).FindReference1;
                            string ref2 = (auditAttribute as Audit2Attribute).FindReference2;


                            Dictionary<string, string> dicFindReference = new Dictionary<string, string>();
                            Dictionary<string, object> dicParametros = new Dictionary<string, object>();

                            MemoryStream ms = new MemoryStream();

                            XmlDictionaryWriter w = XmlDictionaryWriter.CreateTextWriter(ms);

                            request.WriteMessage(w);
                            w.Flush();

                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(Encoding.UTF8.GetString(ms.ToArray()));

                            var node = FindNode(doc.ChildNodes, actionName);

                            if (!string.IsNullOrWhiteSpace(ref1))
                            {
                                var valor = GetPropertyString(ref1, node);
                                if (!string.IsNullOrWhiteSpace(valor))
                                {
                                    dicFindReference.Add(ref1, valor);
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(ref2))
                            {
                                var valor = GetPropertyString(ref2, node);
                                if (!string.IsNullOrWhiteSpace(valor))
                                {
                                    dicFindReference.Add(ref2, valor);
                                }
                            }


                        
                            var parameters = methodInfo.GetParameters();
                            for (int i = 0; i <= node.ChildNodes.Count - 1; i++)
                            {
                                XmlDocument docxml = new XmlDocument();
                                docxml.LoadXml(RemoveAllNamespaces(node.ChildNodes[i].OuterXml));
                                string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(docxml);


                                dicParametros.Add($"{node.ChildNodes[i].Name} ({parameters[i].ParameterType.FullName}) ", json);

                               /* if (Type.GetType(parameters[i].ParameterType.FullName) != null)
                                {
                                    dicParametros.Add($"{node.ChildNodes[i].Name} ({parameters[i].ParameterType.FullName}) ", JsonDeserialize(Type.GetType(parameters[i].ParameterType.FullName), json));
                                }
                                else
                                {
                                    dicParametros.Add($"{node.ChildNodes[i].Name} ({parameters[i].ParameterType.FullName}) ", json);
                                }*/
                            }


                            AuditEntity objAudit = new AuditEntity()
                            {
                                NombreMetodo = actionName,
                                Parametros = dicParametros,
                                ReferenciasBusqueda = dicFindReference,
                            };

                            OperationContext.Current.Extensions.Add(new AuditContext()
                            {                                
                                CanAudit = true,
                                AuditObject = objAudit,
                                ExecutionTime = Stopwatch.StartNew()
                            });

                            // need to recreate the message, since it has already been read
                            ms.Position = 0;
                            request = Message.CreateMessage(XmlReader.Create(ms), int.MaxValue, request.Version);

                        }
                    }
                }

                return null;
            }

            //private object JsonDeserialize(Type type, string json)
            //{
            //    if (!IsSimple(type))
            //    {
            //        object obj = Activator.CreateInstance(type);
            //        MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            //        DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            //        obj = serializer.ReadObject(ms);
            //        ms.Close();
            //        return obj;
            //    }
            //    else
            //    {

            //        dynamic stuff = JsonConvert.DeserializeObject(json);
            //        return ((Newtonsoft.Json.Linq.JValue)((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)stuff).First).Value).Value;
            //    }
            //}

            //private bool IsSimple(Type type)
            //{
            //    return type.IsPrimitive
            //      || type.IsEnum
            //      || type.Equals(typeof(string))
            //      || type.Equals(typeof(decimal))
            //      || type.Equals(typeof(DateTime));
            //}

            private string GetPropertyString(string propertyName, XmlNode param)
            {
                string[] propiedadDetalle = propertyName.Split('.');
                string retorno = "";


                if (propiedadDetalle.Length == 1)
                {
                    var nodo = FindNode(param.ChildNodes,propiedadDetalle[0]);

                    if (nodo != null && nodo.LastChild != null)
                    {

                        retorno = nodo.LastChild.Value;

                    }
                    
                }
                else
                {
                    string property = string.Join(".", propiedadDetalle, 1, propiedadDetalle.Length - 1);

                    var nodo = FindNode(param.ChildNodes, propiedadDetalle[0]);

                    if (nodo != null)
                    {
                        if (nodo.HasChildNodes)
                        {
                            retorno = GetPropertyString(property, nodo);
                        }
                    }
                }


                return retorno;
            }

        
            private XmlNode FindNode(XmlNodeList list, string nodeName)
            {
                if (list.Count > 0)
                {
                    foreach (XmlNode node in list)
                    {
                        string name = node.Name;
                        var names = name.Split(':');
                        
                        if (names.Count() > 1)
                            name = names[1];                        


                        if (name.ToLower().Equals(nodeName.ToLower())) return node;
                        if (node.HasChildNodes)
                        {
                            XmlNode nodeFound = FindNode(node.ChildNodes, nodeName);
                            if (nodeFound != null)
                                return nodeFound;
                        }
                    }
                }
                return null;
            }

            private void SaveAudit(object objAudit)
            {
                try
                {
                    AuditEntity audit = (AuditEntity)objAudit;

                    Debug.WriteLine(audit.NombreMetodo);
                    Debug.WriteLine($"{JsonConvert.SerializeObject(audit)} ************** idHiloParamParaBorrar: {audit.idHiloParamParaBorrar} ");
                    if (audit.Excepcion)
                    {
                        Debug.WriteLine("Ocurrio excepcion");
                        Debug.WriteLine(JsonConvert.SerializeObject(audit.Excepcion));
                    }
                    else
                    {
                        Debug.WriteLine("No ocurrio excepcion");
                    }

                }
                catch (Exception ex)
                {
                    // Se debe agregar a log de errores
                    Debug.WriteLine("Error");
                }
            }


            public static string RemoveAllNamespaces(string xmlDocument)
            {
                XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

                return xmlDocumentWithoutNs.ToString();
            }

            //Core recursion function
            private static XElement RemoveAllNamespaces(XElement xmlDocument)
            {
                if (!xmlDocument.HasElements)
                {
                    XElement xElement = new XElement(xmlDocument.Name.LocalName);
                    xElement.Value = xmlDocument.Value;

                   /* foreach (XAttribute attribute in xmlDocument.Attributes())
                        xElement.Add(attribute);*/

                    return xElement;
                }
                return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
            }

        }

       


        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            
        }

     

        public void Validate(ServiceEndpoint endpoint)
        {
         
        }

        public override System.Type BehaviorType
        {
            get { return typeof(AuditBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new AuditBehavior();
        }


    }

    public class AuditContext : IExtension<OperationContext>
    {
        //The "current" custom context
        public static AuditContext Current
        {
            get
            {
                return OperationContext.Current == null ? null : OperationContext.Current.Extensions.Find<AuditContext>();
            }
        }

        #region IExtension<OperationContext> Members

        public void Attach(OperationContext owner)
        {
            //no-op
        }

        public void Detach(OperationContext owner)
        {
            //no-op
        }
        #endregion IExtension<OperationContext> Members

        
        Stopwatch executionTime;

        bool canAudit;                       
        
        public bool CanAudit { get => canAudit; set => canAudit = value; }


        private AuditEntity auditObject;
        internal AuditEntity AuditObject { get => auditObject; set => auditObject = value; }
        public Stopwatch ExecutionTime { get => executionTime; set => executionTime = value; }
    }
}
