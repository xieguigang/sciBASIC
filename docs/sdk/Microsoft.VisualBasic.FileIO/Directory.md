# Directory
_namespace: [Microsoft.VisualBasic.FileIO](./index.md)_

A wrapper object for the processing of relative file path.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.FileIO.Directory.#ctor(System.String)
```
Construct a directory object from the specific Dir path value.

|Parameter Name|Remarks|
|--------------|-------|
|DIR|Target directory path|


#### CreateDirectory
```csharp
Microsoft.VisualBasic.FileIO.Directory.CreateDirectory(System.String)
```
Creates a directory.

|Parameter Name|Remarks|
|--------------|-------|
|junctionPoint|Name and location of the directory.|

> 
>  Exceptions:
>    T:System.ArgumentException:
>      The directory name is malformed. For example, it contains illegal characters
>      or is only white space.
> 
>    T:System.ArgumentNullException:
>      directory is Nothing or an empty string.
> 
>    T:System.IO.PathTooLongException:
>      The directory name is too long.
> 
>    T:System.NotSupportedException:
>      The directory name is only a colon (:).
> 
>    T:System.IO.IOException:
>      The parent directory of the directory to be created is read-only
> 
>    T:System.UnauthorizedAccessException:
>      The user does not have permission to create the directory.
>  

#### GetFullPath
```csharp
Microsoft.VisualBasic.FileIO.Directory.GetFullPath(System.String)
```
Gets the full path of the target file based on the path relative to this directory object.

|Parameter Name|Remarks|
|--------------|-------|
|file|
 The relative path of the target file, and this parameter is also compatible with absolute file path.
 (相对路径)|


#### IsAbsolutePath
```csharp
Microsoft.VisualBasic.FileIO.Directory.IsAbsolutePath(System.String)
```
Determined that the input file path is a absolute path or not?

|Parameter Name|Remarks|
|--------------|-------|
|file|-|



