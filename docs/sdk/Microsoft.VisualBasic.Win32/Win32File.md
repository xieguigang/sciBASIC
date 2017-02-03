# Win32File
_namespace: [Microsoft.VisualBasic.Win32](./index.md)_

.NET 2.0 Workaround for PathTooLongException

> 
>  http://www.codeproject.com/Articles/22013/NET-Workaround-for-PathTooLongException
>  


### Methods

#### GetAccess
```csharp
Microsoft.VisualBasic.Win32.Win32File.GetAccess(System.IO.FileAccess)
```
Converts the FileAccess constant to win32 constant

|Parameter Name|Remarks|
|--------------|-------|
|access|-|


#### GetMode
```csharp
Microsoft.VisualBasic.Win32.Win32File.GetMode(System.IO.FileMode)
```
Converts the filemode constant to win32 constant

|Parameter Name|Remarks|
|--------------|-------|
|mode|-|


#### GetShare
```csharp
Microsoft.VisualBasic.Win32.Win32File.GetShare(System.IO.FileShare)
```
Converts the FileShare constant to win32 constant

|Parameter Name|Remarks|
|--------------|-------|
|share|-|


#### OpenRead
```csharp
Microsoft.VisualBasic.Win32.Win32File.OpenRead(System.String)
```
Open readonly file mode open(String, FileMode.Open, FileAccess.Read, FileShare.Read)

|Parameter Name|Remarks|
|--------------|-------|
|filepath|-|


#### OpenWrite
```csharp
Microsoft.VisualBasic.Win32.Win32File.OpenWrite(System.String)
```
open writable open(String, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None).

|Parameter Name|Remarks|
|--------------|-------|
|filepath|-|



### Properties

#### ERROR_ALREADY_EXISTS
Error
