Imports System.Collections.Generic
Imports System.Text

Public Class IPv4

    Friend baseIPnumeric As Integer = 0
    Friend netmaskNumeric As Integer = 0

    ''' <summary>
    ''' Specify IP address and netmask like: new IPv4("10.1.0.25","255.255.255.16")
    ''' </summary>
    ''' <param name="symbolicIP"> </param>
    ''' <param name="netmask"> </param>
    Public Sub New(symbolicIP As String, netmask As String)
        Dim Tokens As String() = StringSplit(symbolicIP, "\.", True)

        If Tokens.Length <> 4 Then
            Throw New Exception("Invalid IP address: " & symbolicIP)
        End If

        Dim i As Integer = 24

        For n As Integer = 0 To Tokens.Length - 1
            Dim value As Integer = Convert.ToInt32(Tokens(n))

            If value <> (value And &HFF) Then
                Throw New Exception("Invalid IP address: " & symbolicIP)
            End If

            baseIPnumeric += value << i
            i -= 8
        Next

        ' Netmask 
        Tokens = StringSplit(netmask, "\.", True)

        If Tokens.Length <> 4 Then
            Throw New Exception("Invalid netmask address: " & netmask)
        End If

        i = 24

        If Convert.ToInt32(Tokens(0)) < 255 Then
            Throw New Exception("The first byte of netmask can not be less than 255")
        End If

        For n As Integer = 0 To Tokens.Length - 1
            Dim value As Integer = Convert.ToInt32(Tokens(n))

            If value <> (value And &HFF) Then
                Throw New Exception("Invalid netmask address: " & netmask)
            End If

            netmaskNumeric += value << i

            i -= 8
        Next

        '
        '	* see if there are zeroes inside netmask, like: 1111111101111 This is
        '	* illegal, throw exception if encountered. Netmask should always have
        '	* only ones, then only zeroes, like: 11111111110000
        '	

        Dim encounteredOne As Boolean = False
        Dim ourMaskBitPattern As Integer = 1

        For i = 0 To 31
            If (netmaskNumeric And ourMaskBitPattern) <> 0 Then
                encounteredOne = True  ' the bit is 1
            Else
                If encounteredOne = True Then  ' the bit is 0
                    Throw New Exception($"Invalid netmask: {netmask} (bit {i + 1})")
                End If
            End If

            ourMaskBitPattern = ourMaskBitPattern << 1
        Next
    End Sub

    ''' <summary>
    ''' Get the IP in symbolic form, i.e. xxx.xxx.xxx.xxx
    ''' 
    ''' @return
    ''' </summary>
    Public Overridable ReadOnly Property IPAddress() As String
        Get
            Return convertNumericIpToSymbolic(baseIPnumeric)
        End Get
    End Property

    Private Function convertNumericIpToSymbolic(ip As System.Nullable(Of Integer)) As String
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
    ''' Get the net mask in symbolic form, i.e. xxx.xxx.xxx.xxx
    ''' 
    ''' @return
    ''' </summary>

    Public Overridable ReadOnly Property Netmask() As String
        Get
            Dim sb As New StringBuilder(15)

            For shift As Integer = 24 To 1 Step -8

                ' process 3 bytes, from high order byte down.
                sb.Append(Convert.ToString(CInt(CUInt(netmaskNumeric) >> shift) And &HFF))

                sb.Append("."c)
            Next
            sb.Append(Convert.ToString(netmaskNumeric And &HFF))

            Return sb.ToString()
        End Get
    End Property

    ''' <summary>
    ''' Get the IP and netmask in CIDR form, i.e. xxx.xxx.xxx.xxx/xx
    ''' 
    ''' @return
    ''' </summary>

    Public Overridable ReadOnly Property CIDR() As String
        Get
            Try
                Dim i As Integer
                For i = 0 To 31

                    If (netmaskNumeric << i) = 0 Then
                        Exit For

                    End If
                Next
                Return convertNumericIpToSymbolic(baseIPnumeric And netmaskNumeric) & "/" & i
            Catch ex As Exception
                Return ""
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Get an arry of all the IP addresses available for the IP and netmask/CIDR
    ''' given at initialization
    ''' 
    ''' @return
    ''' </summary>
    Public ReadOnly Property AvailableIPs(numberofIPs__1 As System.Nullable(Of Integer)) As IList(Of String)
        Get
            Dim result As New List(Of String)()
            Dim numberOfBits As Integer

            For numberOfBits = 0 To 31

                If (netmaskNumeric << numberOfBits) = 0 Then
                    Exit For
                End If
            Next
            Dim numberOfIPs__2 As System.Nullable(Of Integer) = 0
            For n As Integer = 0 To (32 - numberOfBits) - 1

                numberOfIPs__2 = numberOfIPs__2 << 1
                numberOfIPs__2 = numberOfIPs__2 Or &H1
            Next

            Dim baseIP As System.Nullable(Of Integer) = baseIPnumeric And netmaskNumeric

            Dim i As Integer = 1
            While i < (numberOfIPs__2) AndAlso i < numberofIPs__1

                Dim ourIP As System.Nullable(Of Integer) = baseIP + i

                Dim ip As String = convertNumericIpToSymbolic(ourIP)

                result.Add(ip)
                i += 1
            End While
            Return result
        End Get
    End Property

    ''' <summary>
    ''' Range of hosts
    ''' 
    ''' @return
    ''' </summary>
    Public Overridable ReadOnly Property hostAddressRange() As String
        Get

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
            Dim firstIP As String = convertNumericIpToSymbolic(baseIP + 1)
            Dim lastIP As String = convertNumericIpToSymbolic(baseIP + numberOfIPs - 1)
            Return firstIP & " - " & lastIP
        End Get
    End Property

    ''' <summary>
    ''' Returns number of hosts available in given range
    ''' </summary>
    ''' <returns> number of hosts </returns>
    Public Overridable ReadOnly Property numberOfHosts() As System.Nullable(Of Long)
        Get
            Dim numberOfBits As Integer

            For numberOfBits = 0 To 31

                If (netmaskNumeric << numberOfBits) = 0 Then
                    Exit For
                End If
            Next

            Dim x As System.Nullable(Of Double) = Math.Pow(2, (32 - numberOfBits))

            If x = -1 Then
                x = 1.0
            End If

            Return CLng(x)
        End Get
    End Property

    ''' <summary>
    ''' The XOR of the netmask
    ''' </summary>
    ''' <returns> wildcard mask in text form, i.e. 0.0.15.255 </returns>

    Public Overridable ReadOnly Property WildcardMask() As String
        Get
            Dim _wildcardMask As System.Nullable(Of Integer) = netmaskNumeric Xor &HFFFFFFFFUI

            Dim sb As New StringBuilder(15)
            For shift As Integer = 24 To 1 Step -8

                ' process 3 bytes, from high order byte down.
                sb.Append(Convert.ToString(CInt(CUInt(_wildcardMask) >> shift) And &HFF))

                sb.Append("."c)
            Next
            sb.Append(Convert.ToString(_wildcardMask And &HFF))

            Return sb.ToString()
        End Get
    End Property

    Public Overridable ReadOnly Property BroadcastAddress() As String
        Get

            If netmaskNumeric = &HFFFFFFFFUI Then
                Return "0.0.0.0"
            End If

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
            Dim ourIP As System.Nullable(Of Integer) = baseIP + numberOfIPs

            Dim ip As String = convertNumericIpToSymbolic(ourIP)

            Return ip
        End Get
    End Property

    Private Function getBinary(number As System.Nullable(Of Integer)) As String
        Dim result As String = ""

        Dim ourMaskBitPattern As System.Nullable(Of Integer) = 1
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

    Public Overridable ReadOnly Property netmaskInBinary() As String
        Get

            Return getBinary(netmaskNumeric)
        End Get
    End Property

    ''' <summary>
    ''' Checks if the given IP address contains in subnet
    ''' </summary>
    ''' <param name="IPaddress">
    ''' @return </param>
    Public Overridable Function contains(IPaddress As String) As Boolean

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

        If (baseIPnumeric And netmaskNumeric) = (checkingIP And netmaskNumeric) Then

            Return True
        Else
            Return False
        End If
    End Function

    Public Overridable Function contains(child As IPv4) As Boolean

        Dim subnetID As System.Nullable(Of Integer) = child.baseIPnumeric

        Dim subnetMask As System.Nullable(Of Integer) = child.netmaskNumeric

        If (subnetID And Me.netmaskNumeric) = (Me.baseIPnumeric And Me.netmaskNumeric) Then

            If (Me.netmaskNumeric < subnetMask) = True AndAlso Me.baseIPnumeric <= subnetID Then

                Return True

            End If
        End If
        Return False
    End Function

    Public Overridable Function validateIPAddress() As Boolean
        Dim IPAddress As String = Me.IPAddress

        If IPAddress.StartsWith("0") Then

            Return False
        End If

        If IPAddress.Length = 0 Then

            Return False
        End If

        If IPAddress.Matches("\A(25[0-5]|2[0-4]\d|[0-1]?\d?\d)(\.(25[0-5]|2[0-4]\d|[0-1]?\d?\d)){3}\z") Then

            Return True
        End If
        Return False
    End Function

    Public Overrides Function ToString() As String
        Return CIDR
    End Function
End Class
