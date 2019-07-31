using EConfig.Commands;
using Jil;
using Mono.Options;
using NLog;


namespace EConfig
{
    class Program
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            JSON.SetDefaultOptions(Options.PrettyPrintCamelCase);

            var cmds = new CommandSet("EncryptedConfig")
            {
                new KeyCommand(),
                new EncryptCommand()
            };
            
            cmds.Run(args);
        }
    }
}
