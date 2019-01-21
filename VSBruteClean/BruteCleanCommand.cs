//////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2018-2019 (c) Sameer Khandekar
//
// License: For your viewing pleasure only.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace VSBruteClean
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class BruteCleanCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("46f04b9f-f3ca-4c6d-907b-e5e105e0571c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="BruteCleanCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private BruteCleanCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            menuItem.BeforeQueryStatus += new EventHandler(CommandQueryHandler);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static BruteCleanCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in BruteCleanCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new BruteCleanCommand(package, commandService);

            await InitOutputPaneAsync(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            DTE2 dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            if (dte != null)
            {
                bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                string solFileName = dte.Solution.FileName;
                if (string.IsNullOrEmpty(solFileName))
                {
                    return;
                }
                FileInfo solFileInfo = new FileInfo(solFileName);
                string solDirName = solFileInfo.Directory.FullName;
                string nugetCacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget");
                if (!isShiftPressed)
                {
                    bool confirmed = ConfirmAndDelete(solDirName);
                    if (confirmed)
                    {
                        ConfirmAndDelete(nugetCacheDir);
                    }
                }
                else
                {
                    BruteCleanFolderAsync(solDirName);
                    BruteCleanFolderAsync(nugetCacheDir);
                }
            }
        }

        /// <summary>
        /// Query Handler for the command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CommandQueryHandler(object sender, EventArgs e)
        {
            // ref: https://stackoverflow.com/questions/38947351/envdte-project-separate-c-sharp-project-from-solution-project
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var menuCommand = sender as Microsoft.VisualStudio.Shell.OleMenuCommand;
            if (menuCommand != null)
            {
                DTE2 dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
                if (dte != null)
                {
                    // check, if the solution is open
                    if (dte.Solution.IsOpen)
                    {
                        // show the menu
                        menuCommand.Visible = true;
                        // enable only if not building
                        menuCommand.Enabled = (dte.Solution.SolutionBuild.BuildState != vsBuildState.vsBuildStateInProgress);
                    }
                    else
                    {
                        // hide and disable the menu
                        menuCommand.Enabled = menuCommand.Visible = false;
                    }
                }
                else
                {
                    // hide and disable the menu
                    menuCommand.Enabled = menuCommand.Visible = false;
                }
            }
        }

        /// <summary>
        /// Get confirmation from the user prior to deletion
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        private bool ConfirmAndDelete(string dirName)
        {
            string title = "Delete";
            string message = $"This will delete all the bin, obj and package folders from {dirName}.\r\n\r\nTip: Holding down Shift Key when clicking menu will bypass this confirmation message.\r\n\r\nDo you want to proceed?";
            int ret = VsShellUtilities.ShowMessageBox(
                                                        this.package,
                                                        message,
                                                        title,
                                                        OLEMSGICON.OLEMSGICON_CRITICAL,
                                                        OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                                                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            bool confirmed = (ret == 6);
            if (confirmed)
            {
                // do the actual delete
                BruteCleanFolderAsync(dirName);
            }

            return confirmed;
        }

        /// <summary>
        /// Initialize the output pane
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        private static async Task InitOutputPaneAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid generalPaneGuid = VSConstants.BuildOutput; //.GUID_OutWindowGeneralPane; // P.S. There's also the GUID_OutWindowDebugPane available.

            outWindow.GetPane(ref generalPaneGuid, out _generalPane);

            _generalPane?.Activate(); // Brings this pane into view
        }

        /// <summary>
        /// The real method that calls the lib
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        private async Task BruteCleanFolderAsync(string dirName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            _generalPane.OutputString($"Brute Cleaning Folder {dirName}\r\n");
            var cleanUtil = new BruteCleanLib.BruteCleanUtil(dirName);
            cleanUtil.FolderRemoved += CleanUtil_FolderRemovedAsync;
            await cleanUtil.Cleanup().ContinueWith(async (res) =>
            {
                cleanUtil.FolderRemoved -= CleanUtil_FolderRemovedAsync;
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
                _generalPane.OutputString($"Brute Cleaned {dirName}\r\n");
            });

        }

        private async void CleanUtil_FolderRemovedAsync(object sender, string folderName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            _generalPane.OutputString($"Removed Folder {folderName}\r\n");
        }

        /// <summary>
        /// Output pane
        /// </summary>
        static IVsOutputWindowPane _generalPane;
    }
}
