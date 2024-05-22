#Region "Microsoft.VisualBasic::686e5010694ce36a2ab7b95cfa3787ec, www\Microsoft.VisualBasic.NETProtocol\IPv4\CIDR.vb"

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

    '   Total Lines: 419
    '    Code Lines: 243 (58.00%)
    ' Comment Lines: 80 (19.09%)
    '    - Xml Docs: 86.25%
    ' 
    '   Blank Lines: 96 (22.91%)
    '     File Size: 13.31 KB


    ' Class IPv4
    ' 
    '     Properties: BroadcastAddress, CIDR, hostAddressRange, IPAddress, Netmask
    '                 netmaskInBinary, numberOfHosts, WildcardMask
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: contains, Contains, GetAvailableIPs, GetBinary, GetBroadcastAddress
    '               GetCIDR, GetHostAddressRange, GetNumberOfHosts, GetWildcardMask, invalidIPAddress
    '               invalidNetMask, NumericIpToSymbolic, NumericNetmaskToSymbolic, ToString
    ' 
    '     Sub: checkNetMask, IPNumeric, NetMaskNumeric
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports stdNum = System.Math

Public Class IPv4

    ReadOnly _baseIPnumeric As Integer
    ReadOnly _netmaskNumeric As Integer

    ''' <summary>
    ''' Specify IP address and netmask like: ``Dim ip As New IPv4("10.1.0.25","255.255.255.16")``
    ''' </summary>
    ''' <param name="symbolicIP"> </param>
    ''' <param name="netmask"> </param>
    Public Sub New(symbolicIP As String, netmask As String)
        Call IPNumeric(symbolicIP, _baseIPnumeric)
        Call NetMaskNumeric(netmask, _netmaskNumeric)
        Call checkNetMask()

        ' 反向计算来检查结果是否正确
        Me.IPAddress = NumericIpToSymbolic(_baseIPnumeric)
        Me.Netmask = NumericNetmaskToSymbolic(_netmaskNumeric)
        Me.CIDR = GetCIDR(_baseIPnumeric, _netmaskNumeric)
        Me.hostAddressRange = GetHostAddressRange(_baseIPnumeric, _netmaskNumeric)
        Me.numberOfHosts = GetNumberOfHosts(_netmaskNumeric)
        Me.WildcardMask = GetWildcardMask(_netmaskNumeric)
        Me.netmaskInBinary = GetBinary(_netmaskNumeric)
        Me.BroadcastAddress = GetBroadcastAddress(_baseIPnumeric, _netmaskNumeric)
    End Sub

    ''' <summary>
    ''' See if there are zeroes inside netmask, like: ``1111111101111`` 
    ''' this Is illegal, throw exception if encountered. 
    ''' Netmask should always have only ones, then only zeroes, 
    ''' like: ``11111111110000``
    ''' </summary>
    Private Sub checkNetMask()
        Dim encounteredOne As Boolean = False
        Dim ourMaskBitPattern As Integer = 1

        For i = 0 To 31
            If (_netmaskNumeric And ourMaskBitPattern) <> 0 Then
                encounteredOne = True  ' the bit is 1
            Else
                If encounteredOne = True Then  ' the bit is 0
                    Throw New Exception($"Invalid netmask: {Netmask} (bit {i + 1})")
                End If
            End If

            ourMaskBitPattern = ourMaskBitPattern << 1
        Next
    End Sub

    ''' <summary>
    ''' The first byte of netmask can not be less than 255
    ''' </summary>
    Const InvalidNetmaskInitial As String = "The first byte of netmask can not be less than 255"

    Public Shared Sub NetMaskNumeric(netmask As String, ByRef netmaskNumeric As Integer)
        Dim tokens As String() = StringSplit(netmask, "\.", True)

        If tokens.Length <> 4 Then
            Throw invalidNetMask(netmask)
        End If

        If Convert.ToInt32(tokens(0)) < 255 Then
            Throw New InvalidExpressionException(InvalidNetmaskInitial)
        End If

        Dim i As Integer = 24

        For n As Integer = 0 To tokens.Length - 1
            Dim value As Integer = Convert.ToInt32(tokens(n))

            If value <> (value And &HFF) Then
                Throw invalidNetMask(netmask)
            End If

            netmaskNumeric += value << i

            i -= 8
        Next
    End Sub

    Public Shared Sub IPNumeric(symbolicIP As String, ByRef baseIPnumeric As Integer)
        Dim tokens As String() = StringSplit(symbolicIP, "\.", True)

        If tokens.Length <> 4 Then
            Throw invalidIPAddress(symbolicIP)
        End If

        Dim i As Integer = 24

        For n As Integer = 0 To tokens.Length - 1
            Dim value As Integer = Convert.ToInt32(tokens(n))

            If value <> (value And &HFF) Then
                Throw invalidIPAddress(symbolicIP)
            End If

            baseIPnumeric += value << i
            i -= 8
        Next
    End Sub

#Region "Throw Exceptions"

    Private Shared Function invalidNetMask(netmask As String) As Exception
        Return New Exception("Invalid netmask address: " & netmask)
    End Function

    Private Shared Function invalidIPAddress(symbolicIP As String) As Exception
        Return New Exception("Invalid IP address: " & symbolicIP)
    End Function
#End Region

    ''' <summary>
    ''' Get the IP in symbolic form, i.e. xxx.xxx.xxx.xxx
    ''' 
    ''' @return
    ''' </summary>
    Public ReadOnly Property IPAddress() As String

    ''' <summary>
    ''' Get the IP in symbolic form, i.e. ``xxx.xxx.xxx.xxx``
    ''' </summary>
    ''' <param name="ip"></param>
    ''' <returns></returns>
    Public Shared Function NumericIpToSymbolic(ip As Integer?) As String
        Dim sb As New StringBuilder(15)

        For shift As Integer = 24 To 1 Step -8
            ' process 3 bytes, from high order byte down.
            sb.Append(Convert.ToString(CInt(CUInt(ip) >> shift) And &HFF))
            sb.Append("."c)
        Next

        sb.Append(Convert.ToString(ip And &HFF))

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Get the net mask in symbolic form, i.e. ``xxx.xxx.xxx.xxx``
    ''' </summary>
    ''' <param name="netMaskNumeric"></param>
    ''' <returns></returns>
    Public Shared Function NumericNetmaskToSymbolic(netMaskNumeric As Integer) As String
        Dim sb As New StringBuilder(15)

        For shift As Integer = 24 To 1 Step -8

            ' process 3 bytes, from high order byte down.
            sb.Append(Convert.ToString(CInt(CUInt(netMaskNumeric) >> shift) And &HFF))
            sb.Append("."c)
        Next
        sb.Append(Convert.ToString(netMaskNumeric And &HFF))

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Get the net mask in symbolic form, i.e. ``xxx.xxx.xxx.xxx``
    ''' 
    ''' @return
    ''' </summary>
    Public ReadOnly Property Netmask() As String

    ''' <summary>
    ''' Get the IP and netmask in CIDR form, i.e. ``xxx.xxx.xxx.xxx/xx``
    ''' </summary>
    ''' <param name="baseIPnumeric"></param>
    ''' <param name="netmaskNumeric"></param>
    ''' <returns></returns>
    Public Shared Function GetCIDR(baseIPnumeric As Integer, netmaskNumeric As Integer) As String
        Dim i As Integer

        For i = 0 To 31
            If (netmaskNumeric << i) = 0 Then
                Exit For
            End If
        Next

        Return NumericIpToSymbolic(baseIPnumeric And netmaskNumeric) & "/" & i
    End Function

    ''' <summary>
    ''' Get the IP and netmask in CIDR form, i.e. xxx.xxx.xxx.xxx/xx
    ''' 
    ''' @return
    ''' </summary>
    Public ReadOnly Property CIDR() As String

    ''' <summary>
    ''' Get an arry of all the IP addresses available for the IP and netmask/CIDR
    ''' given at initialization
    ''' 
    ''' @return
    ''' </summary>
    Public Function GetAvailableIPs(numberofIPs__1 As Integer?) As List(Of String)
        Dim result As New List(Of String)()
        Dim numberOfBits As Integer

        For numberOfBits = 0 To 31
            If (_netmaskNumeric << numberOfBits) = 0 Then
                Exit For
            End If
        Next

        Dim numberOfIPs__2 As Integer? = 0

        For n As Integer = 0 To (32 - numberOfBits) - 1

            numberOfIPs__2 = numberOfIPs__2 << 1
            numberOfIPs__2 = numberOfIPs__2 Or &H1
        Next

        Dim baseIP As Integer? = _baseIPnumeric And _netmaskNumeric
        Dim i As Integer = 1

        While i < (numberOfIPs__2) AndAlso i < numberofIPs__1
            Dim ourIP As Integer? = baseIP + i
            Dim ip As String = NumericIpToSymbolic(ourIP)

            result.Add(ip)
            i += 1
        End While

        Return result
    End Function

    Public Shared Function GetHostAddressRange(baseIPnumeric As Integer, netmaskNumeric As Integer) As String
        Dim numberOfBits As Integer
        For numberOfBits = 0 To 31

            If (netmaskNumeric << numberOfBits) = 0 Then
                Exit For
            End If
        Next

        Dim numberOfIPs As System.Nullable(Of Integer) = 0
        For n As Integer = 0 To (32 - numberOfBits) - 1

            numberOfIPs = numberOfIPs << 1
            numberOfIPs = numberOfIPs Or &H1
        Next

        Dim baseIP As System.Nullable(Of Integer) = baseIPnumeric And netmaskNumeric
        Dim firstIP As String = NumericIpToSymbolic(baseIP + 1)
        Dim lastIP As String = NumericIpToSymbolic(baseIP + numberOfIPs - 1)

        Return firstIP & " - " & lastIP
    End Function

    ''' <summary>
    ''' Range of hosts
    ''' 
    ''' @return
    ''' </summary>
    Public ReadOnly Property hostAddressRange() As String

    Public Shared Function GetNumberOfHosts(NetMaskNumeric As Integer) As Long
        Dim numberOfBits As Integer

        For numberOfBits = 0 To 31
            If (NetMaskNumeric << numberOfBits) = 0 Then
                Exit For
            End If
        Next

        Dim x As Double = stdNum.Pow(2, (32 - numberOfBits))

        If x = -1 Then
            x = 1.0
        End If

        Return CLng(x)
    End Function

    ''' <summary>
    ''' Returns number of hosts available in given range
    ''' </summary>
    ''' <returns> number of hosts </returns>
    Public ReadOnly Property numberOfHosts() As Long

    Public Shared Function GetWildcardMask(netMaskNumeric As Integer) As String
        Dim wildcardMask As Integer = netMaskNumeric Xor &HFFFFFFFFUI

        Dim sb As New StringBuilder(15)
        For shift As Integer = 24 To 1 Step -8

            ' process 3 bytes, from high order byte down.
            sb.Append(Convert.ToString(CInt(CUInt(wildcardMask) >> shift) And &HFF))

            sb.Append("."c)
        Next
        sb.Append(Convert.ToString(wildcardMask And &HFF))

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' The XOR of the netmask
    ''' </summary>
    ''' <returns> wildcard mask in text form, i.e. 0.0.15.255 </returns>
    Public ReadOnly Property WildcardMask() As String

    Public Shared Function GetBroadcastAddress(baseIPnumeric As Integer, netMaskNumeric As Integer) As String
        If netMaskNumeric = &HFFFFFFFFUI Then
            Return "0.0.0.0"
        End If

        Dim numberOfBits As Integer

        For numberOfBits = 0 To 31
            If (netMaskNumeric << numberOfBits) = 0 Then
                Exit For
            End If
        Next

        Dim numberOfIPs As System.Nullable(Of Integer) = 0

        For n As Integer = 0 To (32 - numberOfBits) - 1
            numberOfIPs = numberOfIPs << 1
            numberOfIPs = numberOfIPs Or &H1
        Next

        Dim baseIP As System.Nullable(Of Integer) = baseIPnumeric And netMaskNumeric
        Dim ourIP As System.Nullable(Of Integer) = baseIP + numberOfIPs
        Dim ip As String = NumericIpToSymbolic(ourIP)

        Return ip
    End Function

    Public Overridable ReadOnly Property BroadcastAddress() As String

    Public Shared Function GetBinary(number As Integer) As String
        Dim result As String = ""
        Dim ourMaskBitPattern As Integer = 1

        For i As Integer = 1 To 32
            If (number And ourMaskBitPattern) <> 0 Then

                ' the bit is 1
                result = "1" & result
            Else
                ' the bit is 0

                result = "0" & result
            End If
            If (i Mod 8) = 0 AndAlso i <> 0 AndAlso i <> 32 Then

                result = "." & result
            End If

            ourMaskBitPattern = ourMaskBitPattern << 1
        Next

        Return result
    End Function

    Public ReadOnly Property netmaskInBinary() As String

    ''' <summary>
    ''' Checks if the given IP address contains in subnet
    ''' </summary>
    ''' <param name="IPaddress">
    ''' @return </param>
    Public Function contains(IPaddress As String) As Boolean
        Dim checkingIP As System.Nullable(Of Integer) = 0
        Dim st As String() = StringSplit(IPaddress, "\.", True)

        If st.Length <> 4 Then
            Throw New Exception("Invalid IP address: " & IPaddress)
        End If

        Dim i As Integer = 24

        For n As Integer = 0 To st.Length - 1

            Dim value As Integer = Convert.ToInt32(st(n))

            If value <> (value And &HFF) Then

                Throw New Exception("Invalid IP address: " & IPaddress)
            End If

            checkingIP += value << i
            i -= 8
        Next

        If (_baseIPnumeric And _netmaskNumeric) = (checkingIP And _netmaskNumeric) Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Does this IP range contains the specific child?
    ''' </summary>
    ''' <param name="child"></param>
    ''' <returns></returns>
    Public Function Contains(child As IPv4) As Boolean
        Dim subnetID As Integer = child._baseIPnumeric
        Dim subnetMask As Integer = child._netmaskNumeric

        If (subnetID And _netmaskNumeric) = (_baseIPnumeric And _netmaskNumeric) Then
            If (_netmaskNumeric < subnetMask) AndAlso (_baseIPnumeric <= subnetID) Then
                Return True
            End If
        End If

        Return False
    End Function

    Public Overrides Function ToString() As String
        Return CIDR
    End Function
End Class
