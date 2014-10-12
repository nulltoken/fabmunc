using System;
using System.Threading;

namespace Fabmunc
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: All of the Target properties should be passed through args to ease testing

            using (var sim = new Simulator(
                Target.Url, Target.Login, Target.Password,
                Target.SignatureName, Target.SignatureEmail,
                new Tracer()))
            {
                while (true)
                {
                    sim.SimulateActivity();
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}
