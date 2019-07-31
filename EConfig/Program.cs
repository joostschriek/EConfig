using EConfig.Commands;
using Mono.Options;
using NLog;


namespace EConfig
{
    class Program
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            var cmds = new CommandSet("EncryptedConfig")
            {
                new KeyCommand(),
                new EncryptCommand()
            };
            
            cmds.Run(args);
        }
    }
}
