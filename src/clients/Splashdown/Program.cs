using System;
using Wyam.App;

namespace Splashdown
{
    public class Program
    {
        public static int Main(string[] args)
        {
            IBootstrapper bootstrapper = Bootstrapper.CreateDefault(args);
            return bootstrapper.Run();
        }
    }
}
