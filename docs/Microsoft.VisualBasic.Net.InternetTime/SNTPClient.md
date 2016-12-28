# SNTPClient
_namespace: [Microsoft.VisualBasic.Net.InternetTime](./index.md)_

SNTPClient is a VB.NET# class designed to connect to time servers on the Internet and
 fetch the current date and time. Optionally, it may update the time of the local system.
 The implementation of the protocol is based on the RFC 2030.
 
 Public class members:

 LeapIndicator - Warns of an impending leap second to be inserted/deleted in the last
 minute of the current day. (See the _LeapIndicator enum)
 
 VersionNumber - Version number of the protocol (3 or 4).
 
 Mode - Returns mode. (See the _Mode enum)
 
 Stratum - Stratum of the clock. (See the _Stratum enum)
 
 PollInterval - Maximum interval between successive messages
 
 Precision - Precision of the clock
 
 RootDelay - Round trip time to the primary reference source.
 
 RootDispersion - Nominal error relative to the primary reference source.
 
 ReferenceID - Reference identifier (either a 4 character string or an IP address).
 
 ReferenceTimestamp - The time at which the clock was last set or corrected.
 
 OriginateTimestamp - The time at which the request departed the client for the server.
 
 ReceiveTimestamp - The time at which the request arrived at the server.
 
 Transmit Timestamp - The time at which the reply departed the server for client.
 
 RoundTripDelay - The time between the departure of request and arrival of reply.
 
 LocalClockOffset - The offset of the local clock relative to the primary reference
 source.
 
 Initialize - Sets up data structure and prepares for connection.
 
 Connect - Connects to the time server and populates the data structure.
It can also update the system time.
 
 IsResponseValid - Returns true if received data is valid and if comes from
 a NTP-compliant time server.
 
 ToString - Returns a string representation of the object.
 
 -----------------------------------------------------------------------------
 Structure of the standard NTP header (as described in RFC 2030)
 1 2 3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 |LI | VN |Mode | Stratum | Poll | Precision |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | Root Delay |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | Root Dispersion |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | Reference Identifier |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | |
 | Reference Timestamp (64) |
 | |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | |
 | Originate Timestamp (64) |
 | |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | |
 | Receive Timestamp (64) |
 | |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | |
 | Transmit Timestamp (64) |
 | |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | Key Identifier (optional) (32) |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | |
 | |
 | Message Digest (optional) (128) |
 | |
 | |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 
 -----------------------------------------------------------------------------
 
 SNTP Timestamp Format (as described in RFC 2030)
 1 2 3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | Seconds |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 | Seconds Fraction (0-padded) |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+



### Methods

#### ComputeDate
```csharp
Microsoft.VisualBasic.Net.InternetTime.SNTPClient.ComputeDate(System.Decimal)
```
Compute date, given the number of milliseconds since January 1, 1900

|Parameter Name|Remarks|
|--------------|-------|
|milliseconds|-|


#### Connect
```csharp
Microsoft.VisualBasic.Net.InternetTime.SNTPClient.Connect(System.Boolean)
```
Connect to the time server and update system time

|Parameter Name|Remarks|
|--------------|-------|
|UpdateSystemTime|-|


#### GetMilliSeconds
```csharp
Microsoft.VisualBasic.Net.InternetTime.SNTPClient.GetMilliSeconds(System.Byte)
```
Compute the number of milliseconds, given the offset of a 8-byte array

|Parameter Name|Remarks|
|--------------|-------|
|offset|-|


#### Initialize
```csharp
Microsoft.VisualBasic.Net.InternetTime.SNTPClient.Initialize
```
Initialize the NTPClient data

#### IsResponseValid
```csharp
Microsoft.VisualBasic.Net.InternetTime.SNTPClient.IsResponseValid
```
Check if the response from server is valid

#### SetDate
```csharp
Microsoft.VisualBasic.Net.InternetTime.SNTPClient.SetDate(System.Byte,System.DateTime)
```
Compute the 8-byte array, given the date

|Parameter Name|Remarks|
|--------------|-------|
|offset|-|
|dateval|-|


#### SetTime
```csharp
Microsoft.VisualBasic.Net.InternetTime.SNTPClient.SetTime
```
Set system time according to transmit timestamp

#### ToString
```csharp
Microsoft.VisualBasic.Net.InternetTime.SNTPClient.ToString
```
Converts the object to string


### Properties

#### DestinationTimestamp
Destination Timestamp
#### LeapIndicator
Leap Indicator
#### LocalClockOffset
Local clock offset (in milliseconds)
#### OriginateTimestamp
Originate Timestamp
#### PollInterval
Poll Interval
#### Precision
Precision (in milliseconds)
#### ReceiveTimestamp
Receive Timestamp
#### ReferenceID
Reference Identifier
#### ReferenceTimestamp
Reference Timestamp
#### RootDelay
Root Delay (in milliseconds)
#### RootDispersion
Root Dispersion (in milliseconds)
#### RoundTripDelay
Round trip delay (in milliseconds)
#### SNTPData
NTP Data Structure (as described in RFC 2030)
#### SNTPDataLength
NTP Data Structure Length
#### Stratum
Stratum
#### TimeServer
The URL of the time server we're connecting to
#### TransmitTimestamp
Transmit Timestamp
#### VersionNumber
Version Number
