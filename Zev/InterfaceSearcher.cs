using System;
using System.Linq;
using System.Reflection;

namespace Example.Zev
{
    public class InterfaceSearcher
    {
        public static void SearchForDisposeable(Object obj)
        {
            var fields = obj.GetType().GetFields(
                                     BindingFlags.NonPublic | BindingFlags.Public |
                                     BindingFlags.Instance);

            var props = obj.GetType().GetProperties(
                         BindingFlags.NonPublic | BindingFlags.Public |
                         BindingFlags.Instance);

            Console.WriteLine("Fields");
            var t = typeof(IDisposable);
            foreach (var f in fields)
            {
                if (f.FieldType.GetInterfaces().Contains(t))
                {
                    Console.Write("DISPOSEABLE : ");
                }
                Console.WriteLine(f);
            }
            Console.WriteLine("Props");
            foreach (var f in props)
            {

                if (f.PropertyType.GetInterfaces().Contains(t))
                {
                    Console.Write("DISPOSEABLE : ");
                }
                Console.WriteLine(f);
            }
        }
    }
}
