___NOTE: This is a temporary page. The manual will eventually be moved over to Wiki.___
----
Importing VFS into your project:
- Make sure that you have already imported VRCSDK3 and UdonSharp. 
- Import the latest VFS unity pakcage into your project. You can download the latest version of VFS from the Releases page. 
----
Updating VFS:
If you have VFS already imported into a project and wish to update it to the lastest VFS version, follow these steps:
- Open project. Create a new scene. Close project. This is just to avoid accidentally breaking references in the scene in the later steps.
- Navigate to the Assets folder of your project and delete the VFS folder and VFS.meta file. 
- Open project in Unity and import the latest VFS package into the project. The project should now have the latest VFS package, and all prefab instances should be updated as well.
----
Implementing VFS in your world (maybe give this section it's own page):

Drag the VirtualFileSystem prefab into your scene. Then hook your own Save and Load methods into VFS so they will be executed along with VFS's Save and Load methods. To do that:
- Select SaveNewFileButton (VirtualFileSystem>FileManagerCanvas>SaveNewFileButton). In the OnClick event in the Inspector, there will be two actions. One of them is empty. This is where you add a call to your own Save method. 
- Select LoadFileButton (VirtualFileSystem>FileManagerCanvas>LoadFileButton). In the OnClick event in the Inspector, there will be two actions. One of them is empty. This is where you add a call to your own Load method. 

It is very important to follow the action order that is provided. For the Save button it's, your Save method first, FileSystem Save method second. For the Load button it's, FileSystem Load method first, your Load method second. 
Any data you wish to write to a file must first be converted to a byte array. Any data you read from a file will a byte array. 
When you wish to write data to a file, your Save method must write the byte array to the FileManager's fileDataBuffer variable. When the SaveNewFileButton OnClick event is triggered the FileManager will handle everything else.
When you wish to read data from a file, first the LoadFileButton OnClick event has to be triggered. After the event has been triggered, your Load method must read the FileManager's fileDataBuffer varaible to get the data.
Due to this, the order of the actions (mentioned earlier) must be followed.

Check the example scenes to see practical examples of how VFS can be implemented in your world.
