using System;
using System.Reflection;

namespace ADO.Access
{
    public class FrameworkTypeUtility
    {
        public static void SetProperties<TIn, TOut>(TIn input, TOut output) where TIn : class where TOut : class
        {
            if ((input == null) || (output == null)) return;
            Type inType = input.GetType();
            Type outType = output.GetType();

            foreach (PropertyInfo info in inType.GetProperties())
            {
                var valor = info.GetValue(input, null);
                PropertyInfo outfo = ((info != null) && info.CanRead) ? outType.GetProperty(info.Name, info.PropertyType) : null;

                if (outfo != null && outfo.CanWrite && (outfo.PropertyType.Equals(info.PropertyType)))
                {
                    if (valor != null)
                    {
                        outfo.SetValue(output, info.GetValue(input, null), null);
                    }
                }
            }
        }
    }
}