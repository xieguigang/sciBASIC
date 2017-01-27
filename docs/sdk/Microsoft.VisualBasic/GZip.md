# GZip
_namespace: [Microsoft.VisualBasic](./index.md)_

Creating Zip Files Easily in .NET 4.5
 Tim Corey, 11 May 2012
 http://www.codeproject.com/Articles/381661/Creating-Zip-Files-Easily-in-NET



### Methods

#### AddToArchive
```csharp
Microsoft.VisualBasic.GZip.AddToArchive(System.String,System.Collections.Generic.IEnumerable{System.String},Microsoft.VisualBasic.GZip.ArchiveAction,Microsoft.VisualBasic.GZip.Overwrite,System.IO.Compression.CompressionLevel)
```
Allows you to add files to an archive, whether the archive
 already exists or not

|Parameter Name|Remarks|
|--------------|-------|
|archiveFullName|
 The name of the archive to you want to add your files to
 |
|files|
 A set of file names that are to be added
 |
|action|
 Specifies how we are going to handle an existing archive
 |
|compression|
 Specifies what type of compression to use - defaults to Optimal
 |


#### ImprovedExtractToDirectory
```csharp
Microsoft.VisualBasic.GZip.ImprovedExtractToDirectory(System.String,System.String,Microsoft.VisualBasic.GZip.Overwrite)
```
Unzips the specified file to the given folder in a safe
 manner. This plans for missing paths and existing files
 and handles them gracefully.

|Parameter Name|Remarks|
|--------------|-------|
|sourceArchiveFileName|
 The name of the zip file to be extracted
 |
|destinationDirectoryName|
 The directory to extract the zip file to
 |
|overwriteMethod|
 Specifies how we are going to handle an existing file.
 The default is IfNewer.
 |


#### ImprovedExtractToFile
```csharp
Microsoft.VisualBasic.GZip.ImprovedExtractToFile(System.IO.Compression.ZipArchiveEntry,System.String,Microsoft.VisualBasic.GZip.Overwrite)
```
Safely extracts a single file from a zip file

|Parameter Name|Remarks|
|--------------|-------|
|file__1|
 The zip entry we are pulling the file from
 |
|destinationPath|
 The root of where the file is going
 |
|overwriteMethod|
 Specifies how we are going to handle an existing file.
 The default is Overwrite.IfNewer.
 |



