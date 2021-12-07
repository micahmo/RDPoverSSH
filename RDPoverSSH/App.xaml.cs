using System;
using System.Linq;
using System.Windows;
using CommandLine;
using RDPoverSSH.Arguments;
using RDPoverSSH.Common;

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
                ParsedArgs = Parser.Default.ParseArguments<ShowMessageArgument, SaveKeyArgument, DeleteUserArgument>(e.Args);

                // Handle some args here, without ever going to the UI
                ParsedArgs.WithParsed<DeleteUserArgument>(arg =>
                {
                    UserUtils.DeleteWindowsUser(arg.Username);
                    Environment.Exit(0);
                });
            }
        }

        public static bool HasArgs { get; private set; }

        public static ParserResult<object> ParsedArgs { get; private set; }
    }
}
