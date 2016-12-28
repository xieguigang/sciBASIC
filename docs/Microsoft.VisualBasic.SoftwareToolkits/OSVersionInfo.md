# OSVersionInfo
_namespace: [Microsoft.VisualBasic.SoftwareToolkits](./index.md)_

Provides detailed information about the host operating system.(用于判断操作系统的具体版本信息的工具)



### Methods

#### GetVersionEx
```csharp
Microsoft.VisualBasic.SoftwareToolkits.OSVersionInfo.GetVersionEx(Microsoft.VisualBasic.SoftwareToolkits.OSVersionInfo.OSVERSIONINFOEX@)
```
##### GetVersionEx Function
 
 _GetVersionEx may be altered Or unavailable for releases after Windows 8.1. Instead, use the Version Helper functions_
 
 With the release Of Windows 8.1, the behavior Of the GetVersionEx API has changed In the value it will Return For the 
 operating system version. The value returned by the GetVersionEx Function now depends On how the application Is 
 manifested.
 Applications Not manifested for Windows 8.1 Or Windows 10 will return the Windows 8 OS version value (6.2). Once an 
 application Is manifested for a given operating system version, GetVersionEx will always return the version that the 
 application Is manifested for in future releases. To manifest your applications for Windows 8.1 Or Windows 10, refer 
 to Targeting your application for Windows.
 
 **Syntax**
 
 ```C
 BOOL WINAPI GetVersionEx(
 _Inout_ LPOSVERSIONINFO lpVersionInfo
 );
 ```

|Parameter Name|Remarks|
|--------------|-------|
|osVersionInfo|An OSVersionInfo Or OSVERSIONINFOEX structure that receives the operating system information.
 Before calling the GetVersionEx Function, set the dwOSVersionInfoSize member of the structure as appropriate to indicate 
 which data structure Is being passed to this function.
 |


_returns: If the Then Function succeeds, the Return value Is a nonzero value.
 If the Then Function fails, the Return value Is zero. To Get extended Error information, Call GetLastError. 
 The Function fails If you specify an invalid value For the dwOSVersionInfoSize member Of the OSVERSIONINFO 
 Or OSVERSIONINFOEX Structure.
 _
> 
>  Identifying the current operating system Is usually Not the best way To determine whether a particular operating system 
>  feature Is present. This Is because the operating system may have had New features added In a redistributable DLL. Rather 
>  than Using GetVersionEx To determine the operating system platform Or version number, test For the presence Of the feature 
>  itself. For more information, see Operating System Version.
>  The GetSystemMetrics function provides additional information about the current operating system.
>  
>  |Product|Setting|
>  |-------|-------|
>  |Windows XP Media Center Edition|SM_MEDIACENTER|
>  |Windows XP Starter Edition|SM_STARTER|
>  |Windows XP Tablet PC Edition|SM_TABLETPC|
>  |Windows Server 2003 R2|SM_SERVERR2|
> 
>  To check for specific operating systems Or operating system features, use the IsOS function. The GetProductInfo function retrieves the product type.
>  To retrieve information for the operating system on a remote computer, use the NetWkstaGetInfo function, the Win32_OperatingSystem WMI class, Or the OperatingSystem property of the IADsComputer interface.
>  To compare the current system version to a required version, use the VerifyVersionInfo function instead of using GetVersionEx to perform the comparison yourself.
>  
>  If compatibility Then mode Is In effect, the GetVersionEx Function reports the operating system As it identifies itself, which may Not 
>  be the operating system that Is installed. For example, If compatibility mode Is In effect, GetVersionEx reports the operating system 
>  that Is selected For application compatibility.
>  
>  **Examples**
> 
>  When using the GetVersionEx function to determine whether your application Is running on a particular version of the operating system, 
>  check for version numbers that are greater than Or equal to the desired version numbers. This ensures that the test succeeds for later 
>  versions of the operating system. For example, if your application requires Windows XP Or later, use the following test.
>  
>  ```C
>  #include <windows.h>
>  #include <stdio.h>
> 
>  Void main()
>  {
>      OSVersionInfo osvi;
>      BOOL bIsWindowsXPorLater;
> 
>      ZeroMemory(&osvi, SizeOf(OSVersionInfo));
>      osvi.dwOSVersionInfoSize = SizeOf(OSVersionInfo);
> 
>      GetVersionEx(&osvi);
> 
>      bIsWindowsXPorLater =
>         ((osvi.dwMajorVersion > 5) ||
>         ((osvi.dwMajorVersion == 5) && (osvi.dwMinorVersion >= 1) ));
>  
>      if(bIsWindowsXPorLater) 
>          printf("The system meets the requirements.\n");
>      else printf("The system does not meet the requirements.\n");
>  }
>  ```
> 
>  For an example that identifies the current operating system, see Getting the System Version.
>  
>  **Requirements**
> 
>  | | |
>  |-|-|
>  |Minimum supported client|Windows 2000 Professional [desktop apps only]|
>  |Minimum supported server|Windows 2000 Server [desktop apps only]|
>  |Header|Winbase.h(include Windows.h)|
>  |Library|Kernel32.lib|
>  |DLL|Kernel32.dll|
>  |Unicode And ANSI names|GetVersionExW(Unicode) And GetVersionExA (ANSI)|
>  


### Properties

#### BuildVersion
Gets the build version number of the operating system running on this computer.
#### Edition
Gets the edition of the operating system running on this computer.
#### MajorVersion
Gets the major version number of the operating system running on this computer.
#### MinorVersion
Gets the minor version number of the operating system running on this computer.
#### ProcessorBits
Determines if the current processor is 32 or 64-bit.
#### ProgramBits
Determines if the current application is 32 or 64-bit.
#### RevisionVersion
Gets the revision version number of the operating system running on this computer.
#### ServicePack
Gets the service pack information of the operating system running on this computer.
#### Version
Gets the full version of the operating system running on this computer.
#### VersionString
Gets the full version string of the operating system running on this computer.
#### WindowsName
Gets the name of the operating system running on this computer.
