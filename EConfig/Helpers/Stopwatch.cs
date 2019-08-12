using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace EConfig.Helpers
{
    public class Stopwatch : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public string Component { get; set; }
        public System.Diagnostics.Stopwatch Timer { get; set; }

        public Stopwatch(string component)
        {
            this.Component = component;
            Timer = new System.Diagnostics.Stopwatch();

            Timer.Start();
        }

        public void Dispose()
        {
            Timer.Stop(); 
            logger.Info($"{Component} took {Timer.Elapsed.ToString(@"s\.fff")} seconds.");
        }
    }
}
