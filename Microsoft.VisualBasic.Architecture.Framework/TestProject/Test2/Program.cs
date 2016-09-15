using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ComponentModel.DataSourceModel;
using Microsoft.VisualBasic.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.VisualBasic.Extensions;

namespace Test2
{
    public static class Program
    {

        public static PropertyValue<long> myId<T>(this T x) where T : ClassObject
        {
            // Just copy this statement without any big modification. just modify the generics type constraint.
            return PropertyValue<long>.Read<T>(x, MethodBase.GetCurrentMethod());  
        }

        static void Main(string[] args)
        {
            var x = new ClassObject();
            long n = x.myId().value; // The init value is ZERO

            x.myId().value = 55;     // Extension property set value

            n = -100;
            n = x.myId().value;      // Extension property get value, value should be 55 not -100
            n.__DEBUG_ECHO();        // display the value

            Pause();
        }
    }
}
