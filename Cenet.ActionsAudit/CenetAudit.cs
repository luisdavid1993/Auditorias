using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cenet.ActionsAudit
{
    public class CenetAudit : IDisposable
    {

        static readonly Stopwatch Watch = new Stopwatch();

        TimeSpan Start;
        private bool Excepcion = false;
        private string findReference1 = "", findReference2 = "";       
        private Dictionary<String, Type> _methodParamaters;
        private List<Tuple<String, Type, object>> _providedParametars;



        /// <summary>
        /// Audita la ejecución de un metodo junto con la informacion de los parametros.
        /// </summary>
        /// <param name="findReference1">Nombre de la propiedad para referecia1 de busqueda. Ej: clase compleja NombreParametro.Propiedad.Propiedad. Para parametro simple Ej: Nombre del parametro </param>        
        /// <param name="providedParameters">Todos los parametros de entrada del metodo que se deseen auditar</param>
        public CenetAudit(string findReference1, params Expression<Func<object>>[] providedParameters)
        {
            this.findReference1 = findReference1;
            var currentMethod = new StackTrace().GetFrame(1).GetMethod();
            this.ProcessParams(currentMethod, providedParameters);
        }

        /// <summary>
        /// Audita la ejecución de un metodo junto con la informacion de los parametros.
        /// </summary>
        /// <param name="findReference1">Nombre de la propiedad para referecia1 de busqueda. Ej: clase compleja NombreParametro.Propiedad.Propiedad. Para parametro simple solo el nombre del parametro </param>
        /// <param name="findReference2">Nombre de la propiedad para referecia2 de busqueda. Ej: clase compleja NombreParametro.Propiedad.Propiedad. Para parametro simple solo el nombre del parametro </param>
        /// <param name="providedParameters">Todos los parametros de entrada del metodo que se deseen auditar</param>
        public CenetAudit(string findReference1, string findReference2, params Expression<Func<object>>[] providedParameters)
        {
            this.findReference1 = findReference1;
            this.findReference2 = findReference2;
            var currentMethod = new StackTrace().GetFrame(1).GetMethod();
            this.ProcessParams(currentMethod, providedParameters);
        }


        /// <summary>
        /// Audita la ejecución de un metodo junto con la informacion de los parametros.
        /// </summary>        
        /// <param name="providedParameters">Todos los parametros de entrada del metodo que se deseen auditar</param>
        public CenetAudit(params Expression<Func<object>>[] providedParameters)
        {
            var currentMethod = new StackTrace().GetFrame(1).GetMethod();
            this.ProcessParams(currentMethod, providedParameters);
        }

        /// <summary>
        /// Ejecuta la accion a auditar
        /// </summary>
        /// <param name="action">Accion a ejecutar</param>
        public void Execute(Action action)
        {
            try
            {
                Watch.Start();
                Start = Watch.Elapsed;
                action.Invoke();
                Watch.Stop();
            }
            catch (Exception ex)
            {
                this.Excepcion = true;
                throw ex;
            }
        }

        public void Dispose()
        {
            TimeSpan elapsed = Watch.Elapsed - Start;


            // Get call stack
            StackTrace stackTrace = new StackTrace();

            StackFrame frame = stackTrace.GetFrame(1);

            if (frame != null)
            {

                MethodBase method = frame.GetMethod();

                Dictionary<string, object> dicParametros = new Dictionary<string, object>();
                Dictionary<string, string> dicFindReference = new Dictionary<string, string>();



                foreach (var aMethodParamater in _methodParamaters)
                {
                    var aParameter =
                        _providedParametars.Where(
                            obj => obj.Item1.Equals(aMethodParamater.Key) && obj.Item2 == aMethodParamater.Value).SingleOrDefault();

                    if (aParameter != null)
                    {
                        dicParametros.Add($"{aParameter.Item1}  ({aParameter.Item2})", aParameter.Item3);


                        if (!string.IsNullOrWhiteSpace(this.findReference1))
                        {
                            string valor = this.GetPropertyString(this.findReference1, aParameter.Item3, aParameter.Item1);
                            if (!string.IsNullOrWhiteSpace(valor))
                            {
                                dicFindReference.Add(this.findReference1, valor);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(this.findReference2))
                        {
                            string valor = this.GetPropertyString(this.findReference2, aParameter.Item3, aParameter.Item1);
                            if (!string.IsNullOrWhiteSpace(valor))
                            {
                                dicFindReference.Add(this.findReference2, valor);
                            }
                        }
                    }

                }
                AuditEntity objAudit = new AuditEntity()
                {
                    NombreMetodo = $"Class = {method.DeclaringType.FullName}, Method = {method.Name}",
                    Parametros = dicParametros,
                    TiempoEjecucion = elapsed.TotalMilliseconds,
                    ReferenciasBusqueda = dicFindReference,
                    Excepcion = this.Excepcion
                };

                Action<object> action = (obj) => SaveAudit(obj);
                Task.Factory.StartNew(action, objAudit);
            }

        }

        private void ProcessParams(MethodBase currentMethod, params Expression<Func<object>>[] providedParameters)
        {
            try
            {
                /*obtiene los parametros del metodo*/
                _methodParamaters = new Dictionary<string, Type>();
                (from aParamater in currentMethod.GetParameters()
                 select new { Name = aParamater.Name, DataType = aParamater.ParameterType })
                 .ToList()
                 .ForEach(obj => _methodParamaters.Add(obj.Name, obj.DataType));

                /*obtiene los parametros del provider*/
                _providedParametars = new List<Tuple<string, Type, object>>();
                foreach (var aExpression in providedParameters)
                {
                    Expression bodyType = aExpression.Body;

                    if (bodyType is MemberExpression)
                    {
                        AddProvidedParamaterDetail((MemberExpression)aExpression.Body);
                    }
                    else if (bodyType is UnaryExpression)
                    {
                        UnaryExpression unaryExpression = (UnaryExpression)aExpression.Body;
                        AddProvidedParamaterDetail((MemberExpression)unaryExpression.Operand);
                    }
                    else
                    {
                        throw new Exception("Expression type unknown.");
                    }
                }
            }
            catch (Exception exception)
            {
                //Auditar error
            }
        }

        private void AddProvidedParamaterDetail(MemberExpression memberExpression)
        {
            ConstantExpression constantExpression = (ConstantExpression)memberExpression.Expression;
            var name = memberExpression.Member.Name;
            var value = ((FieldInfo)memberExpression.Member).GetValue(constantExpression.Value);
            var type = value.GetType();
            _providedParametars.Add(new Tuple<string, Type, object>(name, type, value));
        }

        private void SaveAudit(object objAudit)
        {
            try
            {
                AuditEntity audit = (AuditEntity)objAudit;

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


        private string GetPropertyString(string propertyName, object arg, string paramName)
        {
            string[] propiedadDetalle = propertyName.Split('.');
            string retorno = "";

            if (propiedadDetalle.Length == 1)
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
                else if (propertyName == paramName)
                {
                    retorno = arg.ToString();
                }
            }
            else
            {
                string property = string.Join(".", propiedadDetalle, 1, propiedadDetalle.Length - 1);

                PropertyInfo prop = arg.GetType().GetProperty(propiedadDetalle[1]);
                if (prop != null)
                {
                    if (!IsSimple(prop.PropertyType))
                    {
                        retorno = GetPropertyString(property, prop.GetValue(arg), paramName);
                    }
                    else
                    {
                        retorno = prop.GetValue(arg).ToString();
                    }
                }
            }

            return retorno;
        }

        private bool IsSimple(Type type)
        {
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal))
              || type.Equals(typeof(DateTime));
        }
    }
}
