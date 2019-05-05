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


namespace BruteCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            string rootFolder;
            // if there is any argument passed, use it
            // else use the current folder
            if (args.Length > 0)
            {
                rootFolder = args[0];
                if (!Directory.Exists(rootFolder))
                {
                    Console.WriteLine($"{rootFolder} cannot be found.");
                    return;
                }
            }
            else
            {
                rootFolder = Directory.GetCurrentDirectory();
            }

            // display warning
            Console.WriteLine($"This will delete all bin, obj and packages folders from {rootFolder}");
            Console.WriteLine($"You are running this at your own risk and developer is not liable for any damages or losses that are caused by running the software!!!");
            Console.WriteLine($"By continuing you agree to these Licensing terms: https://sameer.blog/brute-clean-a-vs-extension/#lic");
            Console.WriteLine("Press Y to continue. Ctrl C to abort");
            // get user confirmation
            var keyinfo = Console.ReadKey();
            if (keyinfo.Key == ConsoleKey.Y)
            {
                Console.WriteLine($"\r\nDeleting from folder {rootFolder}");
                // brute delete
                var cleanUtil = new BruteCleanLib.BruteCleanUtil(rootFolder);
                cleanUtil.FolderRemoved += FolderRemoved;
                cleanUtil.FailedToRemoveFolder += FailedToRemoveFolder;
                cleanUtil.Cleanup().Wait();

                cleanUtil.FolderRemoved -= FolderRemoved;
                cleanUtil.FailedToRemoveFolder -= FailedToRemoveFolder;
                Console.WriteLine($"Completed!!!");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }

        private static void FolderRemoved(object sender, string folderName)
        {
            Console.WriteLine($"Removed folder {folderName}");
        }

        private static void FailedToRemoveFolder(object sender, Tuple<string, string> message)
        {
            Console.WriteLine($"!!! Failed to removed Folder {message.Item1}\r\n");
            Console.WriteLine($"---!!! Exception {message.Item2}\r\n");
        }
    }
}
