//using Cauldron.Interception;
using Newtonsoft.Json;
using PostSharp.Aspects;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cenet.ActionsAudit
{
    //[InterceptorOptions(AlwaysCreateNewInstance = true)]
    [PSerializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AuditAttribute :  MethodInterceptionAspect //IMethodInterceptor
    {
               
        private string findReference1 = "", findReference2 = "" ;


        


        public AuditAttribute()
        {
            
        }

        public AuditAttribute(string findReference1="", string findReference2=""):base()
        {
            this.findReference1 = findReference1;
            this.findReference2 = findReference2;
        }

        

        public override void OnInvoke(MethodInterceptionArgs args)
        {                        
            DateTime initialTime = DateTime.Now;
            bool exception = false;
            try
            {                
                args.Proceed();
            }catch(Exception ex)
            {
                exception = true;
                throw ex;
            }
            finally
            {        
                 Audit(args,(DateTime.Now-initialTime).TotalMilliseconds,exception);              
            }
            
        }

        /*  public void OnEnter(Type declaringType, object instance, MethodBase methodbase, object[] values)
          {        
              this.declaringType = declaringType;
              this.instance = instance;
              this.methodbase = methodbase;
              this.values = values;            
              initialTime = DateTime.Now;


              idHiloEntradaGlobalBorrar = System.Threading.Thread.CurrentThread.ManagedThreadId;
          }

          public bool OnException(Exception e)
          {

              exception = e;
              return true;
          }

          public void OnExit()
          {            
              Audit();
          }*/

        private void Audit(MethodInterceptionArgs args,double totalMilliseconds,bool exception)
        {

            string NombreMetodo = args.Method.Name;

            Dictionary<string, object> dicParametros = new Dictionary<string, object>();

            Dictionary<string, string> dicFindReference = new Dictionary<string, string>();

            for (int i = 0; i <= args.Arguments.Count - 1; i++)
            {
                var param = args.Method.GetParameters()[i];

               dicParametros.Add($"{param.Name}  ({param.ParameterType.FullName})", args.Arguments[i]);

                if (!string.IsNullOrWhiteSpace(this.findReference1))
                {
                    string valor = this.GetPropertyString(this.findReference1, args.Arguments[i],param);
                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        dicFindReference.Add(this.findReference1,valor);
                    }
                }

                if (!string.IsNullOrWhiteSpace(this.findReference2))
                {
                    string valor = this.GetPropertyString(this.findReference2, args.Arguments[i],param);
                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        dicFindReference.Add(this.findReference2, valor);
                    }
                }

            }

            AuditEntity objAudit = new AuditEntity()
            {
                NombreMetodo = NombreMetodo,
                Parametros = dicParametros,
                TiempoEjecucion = totalMilliseconds,
                ReferenciasBusqueda = dicFindReference,
                Excepcion = exception
            };

            Action<object> action = (obj) => SaveAudit(obj);
            Task.Factory.StartNew(action, objAudit);
                       
            //SaveAudit(objAudit);           
        }

        private string GetPropertyString (string propertyName,object arg,ParameterInfo param )
        {
            string[] propiedadDetalle = propertyName.Split('.');
            string retorno = "";

            if(propiedadDetalle.Length == 1)
            {

                Type type = arg.GetType();

                if (!IsSimple(type))
                {
                    PropertyInfo prop = arg.GetType().GetProperty(propiedadDetalle[0]);
                    if (prop != null)
                    {
                        if (!IsSimple(prop.PropertyType))
                        {
                            retorno = arg.GetType().GetProperty(propiedadDetalle[0]).GetValue(arg).ToString();

                        }
                        else
                        {
                            retorno = prop.GetValue(arg).ToString();
                        }
                    }
                }
                else if(propertyName == param.Name)
                {
                    retorno = arg.ToString();
                }
            }
            else
            {                
                string property = string.Join(".", propiedadDetalle, 1, propiedadDetalle.Length-1);

                PropertyInfo prop = arg.GetType().GetProperty(propiedadDetalle[1]);
                if (prop != null)
                {                   
                    if (!IsSimple(prop.PropertyType))
                    {
                        retorno = GetPropertyString(property, prop.GetValue(arg),param);
                    }
                    else
                    {
                        retorno = prop.GetValue(arg).ToString();
                    }
                }
            }

            return retorno;
        }

        private static bool IsSimple(Type type)
        {
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal))
              || type.Equals(typeof(DateTime));
        }

        private void SaveAudit(object objAudit)
        {
            try
            {
                AuditEntity audit = (AuditEntity)objAudit;

                if (Convert.ToInt32(audit.Parametros.ToArray()[0].Value.GetType().GetProperty("IdHiloHijo").GetValue(audit.Parametros.ToArray()[0].Value)) != audit.idHiloParamParaBorrar)
                {

                }

                Console.WriteLine(audit.NombreMetodo);
                Console.WriteLine($"{JsonConvert.SerializeObject(audit)} ************** idHiloParamParaBorrar: {audit.idHiloParamParaBorrar} ");
                if (audit.Excepcion != null)
                {
                    Console.WriteLine("Ocurrio excepcion");
                    Console.WriteLine(JsonConvert.SerializeObject(audit.Excepcion));
                }
                else
                {
                  Console.WriteLine("No ocurrio excepcion");
                }

            }
            catch (Exception ex)
            {
                // Se debe agregar a log de errores
                Console.WriteLine("Error");
            }
        }




    }
}
