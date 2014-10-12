using System;

namespace Fabmunc
{
    public class Tracer
    {
        public void Write(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
