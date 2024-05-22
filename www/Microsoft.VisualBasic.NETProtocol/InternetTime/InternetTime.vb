#Region "Microsoft.VisualBasic::c7ac3ee66e9224e94dc7144c0005d17f, www\Microsoft.VisualBasic.NETProtocol\InternetTime\InternetTime.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 650
    '    Code Lines: 298 (45.85%)
    ' Comment Lines: 308 (47.38%)
    '    - Xml Docs: 56.82%
    ' 
    '   Blank Lines: 44 (6.77%)
    '     File Size: 29.10 KB


    '     Class SNTPClient
    ' 
    '         Properties: LeapIndicator, LocalClockOffset, Mode, OriginateTimestamp, PollInterval
    '                     Precision, ReceiveTimestamp, ReferenceID, ReferenceTimestamp, RootDelay
    '                     RootDispersion, RoundTripDelay, Stratum, TransmitTimestamp, VersionNumber
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ComputeDate, GetMilliSeconds, IsResponseValid, ToString
    ' 
    '         Sub: Connect, Initialize, SetDate, SetTime
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' http://dotnet-snippets.com/snippet/simple-network-time-ntp-protocol-client/572

'*
'* A C# SNTP Client
'* 
'* Copyright (C)2001-2003 Valer BOCAN <vbocan@dataman.ro>
'* All Rights Reserved
'* 
'* VB.NET port by Ray Frankulin <random0000@cox.net>
'*
'* You may download the latest version from http://www.dataman.ro/sntp
'* If you find this class useful and would like to support my existence, please have a
'* look at my Amazon wish list at
'* http://www.amazon.com/exec/obidos/wishlist/ref=pd_wt_3/103-6370142-9973408
'* or make a donation to my Delta Forth .NET project, at
'* http://shareit1.element5.com/product.html?productid=159082&languageid=1&stylefrom=159082&backlink=http%3A%2F%2Fwww.dataman.ro&currencies=USD
'* 
'* Last modified: September 20, 2003
'*  
'* Permission is hereby granted, free of charge, to any person obtaining a
'* copy of this software and associated documentation files (the
'* "Software"), to deal in the Software without restriction, including
'* without limitation the rights to use, copy, modify, merge, publish,
'* distribute, and/or sell copies of the Software, and to permit persons
'* to whom the Software is furnished to do so, provided that the above
'* copyright notice(s) and this permission notice appear in all copies of
'* the Software and that both the above copyright notice(s) and this
'* permission notice appear in supporting documentation.
'*
'* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
'* OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
'* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT
'* OF THIRD PARTY RIGHTS. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
'* HOLDERS INCLUDED IN THIS NOTICE BE LIABLE FOR ANY CLAIM, OR ANY SPECIAL
'* INDIRECT OR CONSEQUENTIAL DAMAGES, OR ANY DAMAGES WHATSOEVER RESULTING
'* FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT,
'* NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION
'* WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
'*
'* Disclaimer
'* ----------
'* Although reasonable care has been taken to ensure the correctness of this
'* implementation, this code should never be used in any application without
'* proper verification and testing. I disclaim all liability and responsibility
'* to any person or entity with respect to any loss or damage caused, or alleged
'* to be caused, directly or indirectly, by the use of this SNTPClient class.
'*
'* Comments, bugs and suggestions are welcome.
'*
'* Update history:
'* September 20, 2003
'* - Renamed the class from NTPClient to SNTPClient.
'* - Fixed the RoundTripDelay and LocalClockOffset properties.
'*   Thanks go to DNH <dnharris@csrlink.net>.
'* - Fixed the PollInterval property.
'*   Thanks go to Jim Hollenhorst <hollenho@attbi.com>.
'* - Changed the ReceptionTimestamp variable to DestinationTimestamp to follow the standard
'*   more closely.
'* - Precision property is now shown is seconds rather than milliseconds in the
'*   ToString method.
'* 
'* May 28, 2002
'* - Fixed a bug in the Precision property and the SetTime function.
'*   Thanks go to Jim Hollenhorst <hollenho@attbi.com>.
'* 
'* March 14, 2001
'* - First public release.
'*/

Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports stdNum = System.Math

'VB -Simple Network Time (NTP) Protocol Client
'By Valer BOCAN , 6/25/2007

'This Is a free VB.NET implementation Of the SNTP, as documented in the RFC 2030. Feel free to download it And enjoy. 
'There are no restrictions On how you use the code provided herein, except a Short notice To the author.

'The Simple Network Time Protocol (SNTP) Is a protocol For synchronizing the clocks Of computer systems over packet-switched, 
'variable-latency data networks. SNTP uses UDP port 123 As its transport layer. It Is designed particularly To
'resist the effects Of variable latency. SNTP uses Marzullo's algorithm with the UTC time scale, including support for features 
'such as leap seconds. SNTPv4 can usually maintain time to within 10 milliseconds
'(1/100 s) over the Internet, And can achieve accuracies of 200 microseconds (1/5000 s) Or better in local area networks 
'under ideal conditions.

'SNTP Is one of the oldest internet protocols still in use (since before 1985). SNTP was originally designed by
'Dave Mills Of the University Of Delaware, who still maintains it, along With a team Of volunteers.

'SNTP uses a hierarchical system Of “clock strata”, where stratum 1 systems are synchronized To an accurate external 
'clock such As a GPS clock Or other radio clock. SNTP stratum 2 systems derive their time from one Or more stratum 1
'systems, And so on. (Note that this Is different from the notion of clock strata used in telecom systems)

'The 64 - bit timestamps used by SNTP consist Of a 32-bit seconds part And a 32-bit fractional second part, giving SNTP 
'a time scale Of 232 seconds (136 years), With a theoretical resolution Of 2−32 seconds (0.233 nanoseconds). 
'Although the SNTP timescale wraps round every 232 seconds, implementations should disambiguate SNTP time Using a knowledge 
'Of the approximate time from other sources. Since this only requires time accurate To a few decades, 
'this Is Not a problem In general use.

Namespace InternetTime

    ''' <summary>
    ''' SNTPClient is a VB.NET# class designed to connect to time servers on the Internet and
    ''' fetch the current date and time. Optionally, it may update the time of the local system.
    ''' The implementation of the protocol is based on the RFC 2030.
    ''' 
    ''' Public class members:
    '''
    ''' LeapIndicator - Warns of an impending leap second to be inserted/deleted in the last
    ''' minute of the current day. (See the _LeapIndicator enum)
    ''' 
    ''' VersionNumber - Version number of the protocol (3 or 4).
    ''' 
    ''' Mode - Returns mode. (See the _Mode enum)
    ''' 
    ''' Stratum - Stratum of the clock. (See the _Stratum enum)
    ''' 
    ''' PollInterval - Maximum interval between successive messages
    ''' 
    ''' Precision - Precision of the clock
    ''' 
    ''' RootDelay - Round trip time to the primary reference source.
    ''' 
    ''' RootDispersion - Nominal error relative to the primary reference source.
    ''' 
    ''' ReferenceID - Reference identifier (either a 4 character string or an IP address).
    ''' 
    ''' ReferenceTimestamp - The time at which the clock was last set or corrected.
    ''' 
    ''' OriginateTimestamp - The time at which the request departed the client for the server.
    ''' 
    ''' ReceiveTimestamp - The time at which the request arrived at the server.
    ''' 
    ''' Transmit Timestamp - The time at which the reply departed the server for client.
    ''' 
    ''' RoundTripDelay - The time between the departure of request and arrival of reply.
    ''' 
    ''' LocalClockOffset - The offset of the local clock relative to the primary reference
    ''' source.
    ''' 
    ''' Initialize - Sets up data structure and prepares for connection.
    ''' 
    ''' Connect - Connects to the time server and populates the data structure.
    '''	It can also update the system time.
    ''' 
    ''' IsResponseValid - Returns true if received data is valid and if comes from
    ''' a NTP-compliant time server.
    ''' 
    ''' ToString - Returns a string representation of the object.
    ''' 
    ''' -----------------------------------------------------------------------------
    ''' Structure of the standard NTP header (as described in RFC 2030)
    '''                       1                   2                   3
    '''   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    '''  |LI | VN  |Mode |    Stratum    |     Poll      |   Precision   |
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    '''  |                          Root Delay                           |
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    '''  |                       Root Dispersion                         |
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    '''  |                     Reference Identifier                      |
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    '''  |                                                               |
    '''  |                   Reference Timestamp (64)                    |
    '''  |                                                               |
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    '''  |                                                               |
    '''  |                   Originate Timestamp (64)                    |
    '''  |                                                               |
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    '''  |                                                               |
    '''  |                    Receive Timestamp (64)                     |
    '''  |                                                               |
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    '''  |                                                               |
    '''  |                    Transmit Timestamp (64)                    |
    '''  |                                                               |
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    '''  |                 Key Identifier (optional) (32)                |
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    '''  |                                                               |
    '''  |                                                               |
    '''  |                 Message Digest (optional) (128)               |
    '''  |                                                               |
    '''  |                                                               |
    '''  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' 
    ''' -----------------------------------------------------------------------------
    ''' 
    ''' SNTP Timestamp Format (as described in RFC 2030)
    '''                         1                   2                   3
    '''     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |                           Seconds                             |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |                  Seconds Fraction (0-padded)                  |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' 
    ''' </summary>
    Public Class SNTPClient

        ''' <summary>
        ''' NTP Data Structure Length
        ''' </summary>
        Const SNTPDataLength As Byte = 47

        ''' <summary>
        ''' NTP Data Structure (as described in RFC 2030)
        ''' </summary>
        Dim SNTPData(SNTPDataLength) As Byte

        '// Offset constants for timestamps in the data structure
        Private Const offReferenceID As Byte = 12
        Private Const offReferenceTimestamp As Byte = 16
        Private Const offOriginateTimestamp As Byte = 24
        Private Const offReceiveTimestamp As Byte = 32
        Private Const offTransmitTimestamp As Byte = 40

        ''' <summary>
        ''' Leap Indicator
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LeapIndicator() As LeapIndicator
            Get
                'Isolate the two most significant bits
                Dim bVal As Byte = (SNTPData(0) >> 6)
                Select Case bVal
                    Case 0 : Return LeapIndicator.NoWarning
                    Case 1 : Return LeapIndicator.LastMinute61
                    Case 2 : Return LeapIndicator.LastMinute59
                    Case 3 : Return LeapIndicator.Alarm
                    Case Else : Return LeapIndicator.Alarm
                End Select
            End Get
        End Property

        ''' <summary>
        ''' Version Number
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property VersionNumber() As Byte
            Get
                'Isolate bits 3 - 5
                Dim bVal As Byte = (SNTPData(0) And &H38) >> 3
                Return bVal
            End Get
        End Property

        Public ReadOnly Property Mode() As Mode
            Get
                'Isolate bits 0 - 3
                Dim bVal As Byte = (SNTPData(0) And &H7)
                Select Case bVal
                    Case 0, 6, 7
                        Return Mode.Unknown
                    Case 1
                        Return Mode.SymmetricActive
                    Case 2
                        Return Mode.SymmetricPassive
                    Case 3
                        Return Mode.Client
                    Case 4
                        Return Mode.Server
                    Case 5
                        Return Mode.Broadcast
                    Case Else
                        Return Mode.Unknown
                End Select
            End Get
        End Property

        ''' <summary>
        ''' Stratum
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Stratum() As Stratum
            Get
                Dim bVal As Byte = SNTPData(1)
                If (bVal = 0) Then
                    Return Stratum.Unspecified
                ElseIf (bVal = 1) Then
                    Return Stratum.PrimaryReference
                ElseIf (bVal <= 15) Then
                    Return Stratum.SecondaryReference
                Else
                    Return Stratum.Reserved
                End If
            End Get
        End Property

        ''' <summary>
        ''' Poll Interval
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PollInterval() As Int32
            Get
                '// Thanks to Jim Hollenhorst <hollenho@attbi.com>
                Return stdNum.Pow(2, SNTPData(2))
                'Return Math.Round(Math.Pow(2, SNTPData(2)))
            End Get
        End Property

        ''' <summary>
        ''' Precision (in milliseconds)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Precision() As Double
            Get
                '// Thanks to Jim Hollenhorst <hollenho@attbi.com>
                Return stdNum.Pow(2, SNTPData(3))
                'Return (1000 * Math.Pow(2, SNTPData(3) - 256))
            End Get
        End Property

        ''' <summary>
        ''' Root Delay (in milliseconds)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RootDelay() As Double
            Get
                Dim temp As Int64 = 0
                temp = 256 * (256 * (256 * SNTPData(4) + SNTPData(5)) + SNTPData(6)) + SNTPData(7)
                Return 1000 * ((temp) / &H10000)
            End Get
        End Property

        ''' <summary>
        ''' Root Dispersion (in milliseconds)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RootDispersion() As Double
            Get
                Dim temp As Int64 = 0
                temp = 256 * (256 * (256 * SNTPData(8) + SNTPData(9)) + SNTPData(10)) + SNTPData(11)
                Return 1000 * ((temp) / &H10000)
            End Get
        End Property

        ''' <summary>
        ''' Reference Identifier
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ReferenceID() As String
            Get
                Dim val As String = ""
                Select Case Stratum
                    Case Stratum.PrimaryReference Or Stratum.Unspecified
                        If SNTPData(offReferenceID + 0) <> 0 Then val += Chr(SNTPData(offReferenceID + 0))
                        If SNTPData(offReferenceID + 1) <> 0 Then val += Chr(SNTPData(offReferenceID + 1))
                        If SNTPData(offReferenceID + 2) <> 0 Then val += Chr(SNTPData(offReferenceID + 2))
                        If SNTPData(offReferenceID + 3) <> 0 Then val += Chr(SNTPData(offReferenceID + 3))
                    Case Stratum.SecondaryReference
                        Select Case VersionNumber
                            Case 3 '// Version 3, Reference ID is an IPv4 address
                                Dim Address As String = SNTPData(offReferenceID + 0).ToString() + "." + SNTPData(offReferenceID + 1).ToString() + "." + SNTPData(offReferenceID + 2).ToString() + "." + SNTPData(offReferenceID + 3).ToString()
                                Try
                                    Dim Host As IPHostEntry = Dns.GetHostEntry(Address)  ' Dns.GetHostByAddress(Address)
                                    val = Host.HostName + " (" + Address + ")"
                                Catch e As Exception
                                    val = "N/A"
                                End Try
                            Case 4 '// Version 4, Reference ID is the timestamp of last update
                                Dim time As DateTime = ComputeDate(GetMilliSeconds(offReferenceID))
                                '// Take care of the time zone
                                Dim offspan As TimeSpan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)
                                val = (time.Add(offspan)).ToString()
                            Case Else
                                val = "N/A"
                        End Select
                End Select
                Return val
            End Get
        End Property

        ''' <summary>
        ''' Reference Timestamp
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ReferenceTimestamp() As DateTime
            Get
                Dim time As DateTime = ComputeDate(GetMilliSeconds(offReferenceTimestamp))
                '// Take care of the time zone
                Dim offspan As TimeSpan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)
                Return time.Add(offspan)
            End Get
        End Property

        ''' <summary>
        ''' Originate Timestamp
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property OriginateTimestamp() As DateTime
            Get
                Return ComputeDate(GetMilliSeconds(offOriginateTimestamp))
            End Get
        End Property

        ''' <summary>
        ''' Receive Timestamp
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ReceiveTimestamp() As DateTime
            Get
                Dim time As DateTime = ComputeDate(GetMilliSeconds(offReceiveTimestamp))
                'Take care of the time zone
                Dim offspan As TimeSpan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)
                Return time.Add(offspan)
            End Get
        End Property

        ''' <summary>
        ''' Transmit Timestamp
        ''' </summary>
        ''' <returns></returns>
        Public Property TransmitTimestamp() As DateTime
            Get
                Dim time As DateTime = ComputeDate(GetMilliSeconds(offTransmitTimestamp))
                'Take care of the time zone
                Dim offspan As TimeSpan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)
                Return time.Add(offspan)
            End Get
            Set(Value As DateTime)
                SetDate(offTransmitTimestamp, Value)
            End Set
        End Property

        ''' <summary>
        ''' Destination Timestamp
        ''' </summary>
        Public DestinationTimestamp As DateTime

        ''' <summary>
        ''' Round trip delay (in milliseconds)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RoundTripDelay() As Int64
            Get
                '// Thanks to DNH <dnharris@csrlink.net>
                Dim span As TimeSpan = DestinationTimestamp.Subtract(OriginateTimestamp).Subtract(ReceiveTimestamp.Subtract(TransmitTimestamp))
                Return span.TotalMilliseconds
            End Get
        End Property

        ''' <summary>
        ''' Local clock offset (in milliseconds)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LocalClockOffset() As Int64
            Get
                '// Thanks to DNH <dnharris@csrlink.net>
                Dim span As TimeSpan = ReceiveTimestamp.Subtract(OriginateTimestamp).Add((TransmitTimestamp.Subtract(DestinationTimestamp)))
                Return span.TotalMilliseconds / 2
            End Get
        End Property

        ''' <summary>
        ''' Compute date, given the number of milliseconds since January 1, 1900
        ''' </summary>
        ''' <param name="milliseconds"></param>
        ''' <returns></returns>
        Private Function ComputeDate(milliseconds As Decimal) As DateTime
            Dim span As TimeSpan = TimeSpan.FromMilliseconds(milliseconds)
            Dim time As DateTime = New DateTime(1900, 1, 1)
            time = time.Add(span)
            Return time
        End Function

        ''' <summary>
        ''' Compute the number of milliseconds, given the offset of a 8-byte array
        ''' </summary>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Private Function GetMilliSeconds(offset As Byte) As Decimal
            Dim intPart As Decimal = 0, fractPart As Decimal = 0
            Dim i As Int32
            For i = 0 To 3
                intPart = Int(256 * intPart + SNTPData(offset + i))
            Next
            For i = 4 To 7
                fractPart = Int(256 * fractPart + SNTPData(offset + i))
            Next
            Dim milliseconds As Decimal = Int(intPart * 1000 + (fractPart * 1000) / &H100000000L)
            Return milliseconds
        End Function

        ''' <summary>
        ''' Compute the 8-byte array, given the date
        ''' </summary>
        ''' <param name="offset"></param>
        ''' <param name="dateval"></param>
        Private Sub SetDate(offset As Byte, dateval As DateTime)
            Dim intPart As Decimal = 0, fractPart As Decimal = 0
            Dim StartOfCentury As DateTime = New DateTime(1900, 1, 1, 0, 0, 0)
            Dim milliseconds As Decimal = Int(dateval.Subtract(StartOfCentury).TotalMilliseconds)
            intPart = Int(milliseconds / 1000)
            fractPart = Int(((milliseconds Mod 1000) * &H100000000L) / 1000)
            Dim temp As Decimal = intPart
            Dim i As Decimal
            For i = 3 To 0 Step -1
                SNTPData(offset + i) = Int(temp Mod 256)
                temp = Int(temp / 256)
            Next
            temp = Int(fractPart)
            For i = 7 To 4 Step -1
                SNTPData(offset + i) = Int(temp Mod 256)
                temp = Int(temp / 256)
            Next
        End Sub

        ''' <summary>
        ''' Initialize the NTPClient data
        ''' </summary>
        Private Sub Initialize()
            'Set version number to 4 and Mode to 3 (client)
            SNTPData(0) = &H1B
            'Initialize all other fields with 0
            Dim i As Int32
            For i = 1 To 47
                SNTPData(i) = 0
            Next
            'Initialize the transmit timestamp
            TransmitTimestamp = DateTime.Now
        End Sub

        Public Sub New(host As String)
            TimeServer = host
        End Sub

        ''' <summary>
        ''' Connect to the time server and update system time
        ''' </summary>
        ''' <param name="UpdateSystemTime"></param>
        Public Sub Connect(UpdateSystemTime As Boolean)
            'Resolve server address
            Dim hostadd As IPHostEntry = Dns.GetHostEntry(TimeServer) ' Dns.Resolve(TimeServer)
            Dim EPhost As System.Net.IPEndPoint = New System.Net.IPEndPoint(hostadd.AddressList(0), 123)

            'Connect the time server
            Dim TimeSocket As UdpClient = New UdpClient
            TimeSocket.Connect(EPhost)

            'Initialize data structure
            Initialize()
            TimeSocket.Send(SNTPData, SNTPData.Length)
            SNTPData = TimeSocket.Receive(EPhost)
            If IsResponseValid() = False Then
                Throw New Exception("Invalid response from " + TimeServer)
            End If
            DestinationTimestamp = DateTime.Now

            '// Update system time
            If (UpdateSystemTime) Then
                SetTime()
            End If
        End Sub

        ''' <summary>
        ''' Check if the response from server is valid
        ''' </summary>
        ''' <returns></returns>
        Public Function IsResponseValid() As Boolean
            If (SNTPData.Length < SNTPDataLength Or Mode <> Mode.Server) Then
                Return False
            Else
                Return True
            End If
        End Function

        ''' <summary>
        ''' Converts the object to string
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder("")

            sb.Append("Leap Indicator: ")
            Select Case LeapIndicator
                Case LeapIndicator.NoWarning
                    sb.Append("No warning")
                Case LeapIndicator.LastMinute61
                    sb.Append("Last minute has 61 seconds")
                Case LeapIndicator.LastMinute59
                    sb.Append("Last minute has 59 seconds")
                Case LeapIndicator.Alarm
                    sb.Append("Alarm Condition (clock not synchronized)")
            End Select
            sb.Append(vbCrLf & "Version number: " + VersionNumber.ToString())
            sb.Append(vbCrLf & "Mode: ")
            Select Case Mode
                Case Mode.Unknown
                    sb.Append("Unknown")
                Case Mode.SymmetricActive
                    sb.Append("Symmetric Active")
                Case Mode.SymmetricPassive
                    sb.Append("Symmetric Pasive")
                Case Mode.Client
                    sb.Append("Client")
                Case Mode.Server
                    sb.Append("Server")
                Case Mode.Broadcast
                    sb.Append("Broadcast")
            End Select
            sb.Append(vbCrLf & "Stratum: ")
            Select Case Stratum
                Case Stratum.Unspecified
                Case Stratum.Reserved
                    sb.Append("Unspecified")
                Case Stratum.PrimaryReference
                    sb.Append("Primary Reference")
                Case Stratum.SecondaryReference
                    sb.Append("Secondary Reference")
            End Select
            sb.Append(vbCrLf & "Local time: " + TransmitTimestamp.ToString())
            sb.Append(vbCrLf & "Precision: " + Precision.ToString() + " ms")
            sb.Append(vbCrLf & "Poll Interval: " + PollInterval.ToString() + " s")
            sb.Append(vbCrLf & "Reference ID: " + ReferenceID.ToString())
            sb.Append(vbCrLf & "Root Delay: " + RootDelay.ToString() + " ms")
            sb.Append(vbCrLf & "Root Dispersion: " + RootDispersion.ToString() + " ms")
            sb.Append(vbCrLf & "Round Trip Delay: " + RoundTripDelay.ToString() + " ms")
            sb.Append(vbCrLf & "Local Clock Offset: " + LocalClockOffset.ToString() + " ms")
            sb.Append(vbCrLf)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' Set system time according to transmit timestamp
        ''' </summary>
        Private Sub SetTime()
            Dim st As SYSTEMTIME
            Dim trts As DateTime = DateTime.Now.AddMilliseconds(LocalClockOffset)
            st.Year = trts.Year
            st.Month = trts.Month
            st.DayOfWeek = trts.DayOfWeek
            st.Day = trts.Day
            st.Hour = trts.Hour
            st.Minute = trts.Minute
            st.Second = trts.Second
            st.Miliseconds = trts.Millisecond
            SetLocalTime(st)
        End Sub

        ''' <summary>
        ''' The URL of the time server we're connecting to
        ''' </summary>
        Private TimeServer As String
    End Class
End Namespace
