# LANTools
_namespace: [Microsoft.VisualBasic.Net](./index.md)_

http://www.codeproject.com/Tips/358946/Retrieving-IP-and-MAC-addresses-for-a-LAN



### Methods

#### GetAllDevicesOnLAN
```csharp
Microsoft.VisualBasic.Net.LANTools.GetAllDevicesOnLAN
```
Get the IP and MAC addresses of all known devices on the LAN
> 
>  1) This table is not updated often - it can take some human-scale time 
>     to notice that a device has dropped off the network, or a new device
>     has connected.
>  2) This discards non-local devices if they are found - these are multicast
>     and can be discarded by IP address range.
>  

#### GetIPAddress
```csharp
Microsoft.VisualBasic.Net.LANTools.GetIPAddress
```
Gets the IP address of the current PC

#### GetIpNetTable
```csharp
Microsoft.VisualBasic.Net.LANTools.GetIpNetTable(System.IntPtr,System.Int32@,System.Boolean)
```
GetIpNetTable external method

|Parameter Name|Remarks|
|--------------|-------|
|pIpNetTable|-|
|pdwSize|-|
|bOrder|-|


#### GetMacAddress
```csharp
Microsoft.VisualBasic.Net.LANTools.GetMacAddress
```
Gets the MAC address of the current PC.

#### IsMulticast
```csharp
Microsoft.VisualBasic.Net.LANTools.IsMulticast(System.Net.IPAddress)
```
Returns true if the specified IP address is a multicast address

|Parameter Name|Remarks|
|--------------|-------|
|ip|-|



### Properties

#### ERROR_INSUFFICIENT_BUFFER
Error codes GetIpNetTable returns that we recognise
