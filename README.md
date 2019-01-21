# Brute Clean
## How it works
Several times developers need to delete the obj, bin, package folders for a really clean build. It is a time consuming manual process to go through the folders in all the projects and the packages cache. This extension walks the sub-tree of solution folder and performs the job for you. It will also optionally delete all the packages from the packages cache folder. Effectively it saves time and labor. Note: Visual Studio may recreate bin and obj folders and subfolders after the operation.

This is available in both Visual Studio 2017 and VS for Mac (7.7.2)

https://marketplace.visualstudio.com/items?itemName=Sameerk.BruteClean

https://addins.monodevelop.com/Project/Index/364

## How to use
* Build menu will have a sub-menu Brute Clean. This is enabled when the solution is open and not building.
* Clicking the menu will show you the solution folder that will be cleaned. After confirmation, it will clean the folder.
* Then it will show the location of the packages cache folder. After the confirmation, it will clean the cache folder. The folders that are removed are displayed in the output window.

## Tip
If you hold the shift key while clicking the Brute Clean menu, the confirmation messages will not be shown and all the folders will be cleaned.

## Known Limitations
* If a project is not a subtree of the solutions folder, it will not be cleaned.
* The package cache folder is default user based folder e.g. "C:\Users<yourname>.nuget\packages". If your package cache folder is different, it will not get cleaned.

# License
//////////////////////////////////////////////////////////////////////////////////////////////////////////

Copyright 2018-2019 (c) Sameer Khandekar

License: For your viewing pleasure only.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
 MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
 DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//////////////////////////////////////////////////////////////////////////////////////////////////////////