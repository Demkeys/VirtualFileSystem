# VirtualFileSystem
#### A virtual file system made to work with Udon in VRChat. The file system data can be imported/exported as a string. This means you can carry the string with you to another instance, or save it somewhere offline (eg. Notepad) for later use.
 
_NOTE: This repo only contains the csharp code files from the VFS package. This is meant to be used as a scripting reference. This is not the whole VFS package. There are other files from the VFS package (*.unity, *.mat, *.prefab, etc.) that are not present in this repo, but will be present in the VFS package. To download the whole VFS package visit the Releases page._

There will be a Wiki for the documentation but for now, the documentation can be found on the [Manual](https://github.com/Demkeys/VirtualFileSystem/blob/main/Manual.md) page.

### TODO:
* Add more comments to FileManager and FileSystem. FileSystem's DecompressFileSystemData method might need more comments.
* Create Ex03 example scene to demonstrate a very basic data integrity check.
* Create Wiki containing all the VFS documentation.
* Maybe add a file containing helper methods to convert certain data types to bytes and vice versa. That file is mainly gonna be part of the repo, not the package. People can copy the code for functions that they need.
