using System.Linq;
using System.Windows;
using CommandLine;
using RDPoverSSH.Arguments;

namespace RDPoverSSH
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <inheritdoc/>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Any())
            {
                HasArgs = true;
                ParsedArgs = Parser.Default.ParseArguments<ShowMessageArgument, object>(e.Args);
            }
        }

        public static bool HasArgs { get; private set; }

        public static ParserResult<object> ParsedArgs { get; private set; }
    }
}
