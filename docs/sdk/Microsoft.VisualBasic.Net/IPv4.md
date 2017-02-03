# IPv4
_namespace: [Microsoft.VisualBasic.Net](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Net.IPv4.#ctor(System.String,System.String)
```
Specify IP address and netmask like: ``Dim ip As New IPv4("10.1.0.25","255.255.255.16")``

|Parameter Name|Remarks|
|--------------|-------|
|symbolicIP|-|
|netmask|-|


#### __checkNetMask
```csharp
Microsoft.VisualBasic.Net.IPv4.__checkNetMask
```
See if there are zeroes inside netmask, like: ``1111111101111`` 
 this Is illegal, throw exception if encountered. 
 Netmask should always have only ones, then only zeroes, 
 like: ``11111111110000``

#### contains
```csharp
Microsoft.VisualBasic.Net.IPv4.contains(Microsoft.VisualBasic.Net.IPv4)
```
Does this IP range contains the specific child?

|Parameter Name|Remarks|
|--------------|-------|
|child|-|


#### GetAvailableIPs
```csharp
Microsoft.VisualBasic.Net.IPv4.GetAvailableIPs(System.Nullable{System.Int32})
```
Get an arry of all the IP addresses available for the IP and netmask/CIDR
 given at initialization
 
 @return

#### GetCIDR
```csharp
Microsoft.VisualBasic.Net.IPv4.GetCIDR(System.Int32,System.Int32)
```
Get the IP and netmask in CIDR form, i.e. ``xxx.xxx.xxx.xxx/xx``

|Parameter Name|Remarks|
|--------------|-------|
|baseIPnumeric|-|
|netmaskNumeric|-|


#### NumericIpToSymbolic
```csharp
Microsoft.VisualBasic.Net.IPv4.NumericIpToSymbolic(System.Nullable{System.Int32})
```
Get the IP in symbolic form, i.e. ``xxx.xxx.xxx.xxx``

|Parameter Name|Remarks|
|--------------|-------|
|ip|-|


#### NumericNetmaskToSymbolic
```csharp
Microsoft.VisualBasic.Net.IPv4.NumericNetmaskToSymbolic(System.Int32)
```
Get the net mask in symbolic form, i.e. ``xxx.xxx.xxx.xxx``

|Parameter Name|Remarks|
|--------------|-------|
|netMaskNumeric|-|



### Properties

#### CIDR
Get the IP and netmask in CIDR form, i.e. xxx.xxx.xxx.xxx/xx
 
 @return
#### hostAddressRange
Range of hosts
 
 @return
#### InvalidNetmaskInitial
The first byte of netmask can not be less than 255
#### IPAddress
Get the IP in symbolic form, i.e. xxx.xxx.xxx.xxx
 
 @return
#### Netmask
Get the net mask in symbolic form, i.e. ``xxx.xxx.xxx.xxx``
 
 @return
#### numberOfHosts
Returns number of hosts available in given range
#### WildcardMask
The XOR of the netmask
