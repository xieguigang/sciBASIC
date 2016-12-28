# HardwareInfo
_namespace: [Microsoft.VisualBasic.Win32](./index.md)_

Get mother board serial numbers and CPU IDs in Visual Basic .NET



### Methods

#### CPU_Id
```csharp
Microsoft.VisualBasic.Win32.HardwareInfo.CPU_Id
```
The following code gets a WMI object and selects Win32_Processor objects. It loops through them getting their processor IDs.

#### HarddriveInfo
```csharp
Microsoft.VisualBasic.Win32.HardwareInfo.HarddriveInfo
```
How to Retrieve the REAL Hard Drive Serial Number.
 
 Shows you how to obtain the hardware serial number set by the manufacturer and 
 not the Volume Serial Number that changes after you format a hard drive.
> 
>  http://www.codeproject.com/Articles/6077/How-to-Retrieve-the-REAL-Hard-Drive-Serial-Number
>  

#### SystemSerialNumber
```csharp
Microsoft.VisualBasic.Win32.HardwareInfo.SystemSerialNumber
```
The following function gets a WMI object and then gets a collection of WMI_BaseBoard objects 
 representing the system's mother boards. It loops through them getting their serial numbers.


