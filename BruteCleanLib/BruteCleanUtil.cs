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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BruteCleanLib
{
    /// <summary>
    /// Utility for the cleanup
    /// </summary>
    public class BruteCleanUtil
    {
        /// <summary>
        /// Event => a folder has been removed
        /// </summary>
        public event EventHandler<string> FolderRemoved;

        /// <summary>
        /// Event failed to remove a folder
        /// Item1 -> folder name
        /// Item2 -> ex.Message
        /// </summary>
        public event EventHandler<Tuple<string, string>> FailedToRemoveFolder;

        /// <summary>
        /// Constructor accepting the root folder
        /// </summary>
        /// <param name="rootFolder"></param>
        public BruteCleanUtil(string rootFolder)
        {
            if (string.IsNullOrWhiteSpace(rootFolder))
            {
                throw new ArgumentNullException(nameof(rootFolder));
            }

            if (!Directory.Exists(rootFolder))
            {
                throw new ArgumentException($"Invalid folder {rootFolder}");
            }

            _rootFolder = rootFolder;
        }

        /// <summary>
        /// Do the clean up by calling iterative method
        /// </summary>
        /// <returns></returns>
        public Task<bool> Cleanup()
        {
            //bool ret = CleanFolder(_rootFolder);

            //return await Task.FromResult(ret);

            return Task.Run<bool>(() =>
            {
                return CleanFolder(_rootFolder);
            });

        }

        /// <summary>
        /// Clean the folder in recursive way
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private bool CleanFolder(string folder)
        {
            try
            {
                // walk thorugh each folder
                foreach (var dir in Directory.EnumerateDirectories(folder))
                {
                    // if it is to be removed, do it and invoke the event
                    if (ShouldDelete(dir))
                    {
                        int maxTries = 3;
                        for (int i = 0; i < maxTries; i++)
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                                FolderRemoved?.Invoke(this, dir);
                                break;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                FailedToRemoveFolder?.Invoke(this, new Tuple<string, string>(dir, $"{i+1} of {maxTries}: {ex.Message}"));
                            }
                        }
                    }
                    else
                    {   
                        /// else check its subfolders
                        CleanFolder(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks, if it should be deleted
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private bool ShouldDelete(string dir)
        {
            // get the last index of the folder
            int lastInd = dir.LastIndexOf(Path.DirectorySeparatorChar);
            if (lastInd >= 0)
            {
                var subfolderName = dir.Substring(lastInd + 1);

                foreach (var deleteCandidate in _foldersToDelete)
                {
                    if (string.Equals(subfolderName, deleteCandidate, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // root folder
        private readonly string _rootFolder;

        // folders to delete
        private readonly string[] _foldersToDelete = { "bin", "obj", "packages" };
    }
}
