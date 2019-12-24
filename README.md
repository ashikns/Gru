# Gru

Gru is a [glTF](https://github.com/KhronosGroup/glTF) runtime importer for Unity3D. 
It's written with code extensibility and performance in mind, and specifically focussed on runtime importing. 
This means that Gru does not support model exporting, nor is there a guarantee that it will work correctly in Edit mode.

### Installation
Include the repo as a submodule or just download the repo as a zip and place in your Unity project. Gru requires [Json.NET](https://github.com/JamesNK/Newtonsoft.Json) to be installed in your project. You can download the Json.NET nuget package, open it as an archive, then copy netstandard2.0 version dlls to Assets/Plugins/ folder in your Unity project.

### Usage
`var modelObject = await GLTFImporter.ImportAsync(modelFilePath, new FileStreamLoader(modelDirectoryPath));`

The second parameter to load is an interface that can open a stream to read buffer data.

### Credits
Gru has been written using code liberally borrowed from the [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF) library, 
and also uses the same shaders.
