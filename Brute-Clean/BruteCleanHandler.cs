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
using System.IO;
using System.Threading.Tasks;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;

namespace BruteClean
{
    /// <summary>
    /// Brute clean handler.
    /// </summary>
    public class BruteCleanHandler : CommandHandler
    {
        #region constructor
        /// <summary>
        /// Constructor performs initialization
        /// </summary>
        public BruteCleanHandler()
        {
            Init();
        }
        #endregion

        /// <summary>
        /// Run the command
        /// </summary>
        protected override void Run()
        {
            // check if shift is pressed
            bool shiftPressed = Xwt.Keyboard.CurrentModifiers.HasFlag(Xwt.ModifierKeys.Shift);
            // soln folder
            var solFolder = IdeApp.Workspace.Items[0].BaseDirectory;
            // package cache folder
            var userNuGetFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget");
            // if shift is not pressed, ask for confirmation
            if(!shiftPressed)
            {
                bool confirmed = ConfirmAndDelete(solFolder);
                if (confirmed)
                {
                    ConfirmAndDelete(userNuGetFolder);
                }
            }
            else
            {
                Task.Run(async () =>
                {
                    await BruteCleanFolderAsync(solFolder);
                    await BruteCleanFolderAsync(userNuGetFolder);
                });

            }
        }

        /// <summary>
        /// Enable only if a solution is open
        /// </summary>
        /// <param name="info">Info.</param>
        protected override void Update(CommandInfo info)
        {
            info.Enabled = IdeApp.Workspace.IsOpen;
        }

        #region private methods
        /// <summary>
        /// Initialize the instance
        /// </summary>
        private void Init()
        { 
            _optputProgressMonitor = IdeApp.Workbench.ProgressMonitors.GetOutputProgressMonitor("Brute Clean", null, true, true);
        }

        /// <summary>
        /// Get confirmation from the user prior to deletion
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        private bool ConfirmAndDelete(string dirName)
        {
            string message = $"This will delete all the bin, obj and package folders from {dirName}.\r\n\r\nTip: Holding down Shift Key when clicking menu will bypass this confirmation message.\r\n\r\nDo you want to proceed?";
            ConfirmationMessage msg = new ConfirmationMessage
            {
                ConfirmButton = AlertButton.Delete,
                Text = $"This will delete all the bin, obj and package folders from {dirName}.\r\n\r\nDo you want to proceed?",
                SecondaryText = "Tip: Holding down Shift Key when clicking menu will bypass this confirmation message.",
            };

            if (MessageService.Confirm(msg))
            {
                // do the actual delete
                Task.Run(async () =>
                {
                    await BruteCleanFolderAsync(dirName);
                })
                .Wait();

                return true;
            }

            return false;
        }

        /// <summary>
        /// The real method that calls the lib
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        private async Task BruteCleanFolderAsync(string dirName)
        {
            _optputProgressMonitor.Console.Out.WriteLine($"Brute Cleaning Folder {dirName}\r\n");
            var cleanUtil = new BruteCleanLib.BruteCleanUtil(dirName);
            cleanUtil.FolderRemoved += CleanUtil_FolderRemoved;
            cleanUtil.FailedToRemoveFolder += CleanUtil_ErrorRemovingFolder;
            await cleanUtil.Cleanup().ContinueWith((res) =>
            {
                cleanUtil.FolderRemoved -= CleanUtil_FolderRemoved;
                cleanUtil.FailedToRemoveFolder -= CleanUtil_ErrorRemovingFolder;

                _optputProgressMonitor.Console.Out.WriteLine($"Brute Cleaned {dirName}\r\n");
            });

        }

        /// <summary>
        /// Show the removed folder in the output window
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="folderName">Folder name.</param>
        private void CleanUtil_FolderRemoved(object sender, string folderName)
        {
            _optputProgressMonitor.Console.Out.WriteLine($"Removed Folder {folderName}\r\n");
        }

        /// <summary>
        /// Show the error while removing a folder in the output window
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="error">Folder name and the exception.</param>
        private void CleanUtil_ErrorRemovingFolder(object sender, Tuple<string, string> error)
        {
            _optputProgressMonitor.Console.Out.WriteLine($"!!! Failed to remove Folder {error.Item1}\r\n");
            _optputProgressMonitor.Console.Out.WriteLine($"---!!! Exception {error.Item2}\r\n");
        }
        #endregion

        #region fields
        // output window
        private OutputProgressMonitor _optputProgressMonitor;
        #endregion
    }
}
