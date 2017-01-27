# JunctionPoint
_namespace: [Microsoft.VisualBasic.FileIO.SymLinker](./index.md)_

Provides access to NTFS junction points in .Net.



### Methods

#### Create
```csharp
Microsoft.VisualBasic.FileIO.SymLinker.JunctionPoint.Create(System.String,System.String,System.Boolean)
```
Creates a junction point from the specified directory to the specified target directory.

|Parameter Name|Remarks|
|--------------|-------|
|junctionPoint|The junction point path|
|targetDir|The target directory|
|overwrite|If true overwrites an existing reparse point or empty directory|

> 
>  Only works on NTFS.
>  

#### Delete
```csharp
Microsoft.VisualBasic.FileIO.SymLinker.JunctionPoint.Delete(System.String)
```
Deletes a junction point at the specified source directory along with the directory itself.
 Does nothing if the junction point does not exist.

|Parameter Name|Remarks|
|--------------|-------|
|junctionPoint|The junction point path|

> 
>  Only works on NTFS.
>  

#### Exists
```csharp
Microsoft.VisualBasic.FileIO.SymLinker.JunctionPoint.Exists(System.String)
```
Determines whether the specified path exists and refers to a junction point.

|Parameter Name|Remarks|
|--------------|-------|
|path|The junction point path|


_returns: True if the specified path represents a junction point_

#### GetTarget
```csharp
Microsoft.VisualBasic.FileIO.SymLinker.JunctionPoint.GetTarget(System.String)
```
Gets the target of the specified junction point.

|Parameter Name|Remarks|
|--------------|-------|
|junctionPoint|The junction point path|


_returns: The target of the junction point_
> 
>  Only works on NTFS.
>  


### Properties

#### ERROR_INVALID_REPARSE_DATA
The data present in the reparse point buffer is invalid.
#### ERROR_NOT_A_REPARSE_POINT
The file or directory is not a reparse point.
#### ERROR_REPARSE_ATTRIBUTE_CONFLICT
The reparse point attribute cannot be set because it conflicts with an existing attribute.
#### ERROR_REPARSE_TAG_INVALID
The tag present in the reparse point buffer is invalid.
#### ERROR_REPARSE_TAG_MISMATCH
There is a mismatch between the tag specified in the request and the tag present in the reparse point.
#### FSCTL_DELETE_REPARSE_POINT
Command to delete the reparse point data base.
#### FSCTL_GET_REPARSE_POINT
Command to get the reparse point data block.
#### FSCTL_SET_REPARSE_POINT
Command to set the reparse point data block.
#### IO_REPARSE_TAG_MOUNT_POINT
Reparse point tag used to identify mount points and junction points.
#### NonInterpretedPathPrefix
This prefix indicates to NTFS that the path is to be treated as a non-interpreted
 path in the virtual file system.
