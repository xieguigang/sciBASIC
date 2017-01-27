# PathMapper
_namespace: [Microsoft.VisualBasic.Language.UnixBash](./index.md)_

Maps the linux path to the Windows path.(这个模块是将Linux路径映射为Windows路径的)



### Methods

#### GetMapPath
```csharp
Microsoft.VisualBasic.Language.UnixBash.PathMapper.GetMapPath(System.String)
```
Map linux path on Windows:
 
 + [~ -> C:\User\<user_name>]
 + [# -> @``P:Microsoft.VisualBasic.App.HOME``]
 + [/ -> C:\]
 + [/usr/bin -> C:\Program Files\]
 + [/usr -> C:\User\]
 + [- -> @``P:Microsoft.VisualBasic.App.PreviousDirectory``]

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### HOME
```csharp
Microsoft.VisualBasic.Language.UnixBash.PathMapper.HOME
```
Get user home folder


### Properties

#### platform
Gets a @``T:System.PlatformID`` enumeration value that identifies the operating system
 platform.
