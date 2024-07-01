#Region "Microsoft.VisualBasic::4a95838b94d8c7e77d415a76354a7134, Microsoft.VisualBasic.Core\src\Net\Tcp\IPv4\IPUtils.vb"

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

    '   Total Lines: 114
    '    Code Lines: 41 (35.96%)
    ' Comment Lines: 57 (50.00%)
    '    - Xml Docs: 71.93%
    ' 
    '   Blank Lines: 16 (14.04%)
    '     File Size: 4.16 KB


    '     Module IPUtils
    ' 
    '         Function: GetGeoAddress, GetSubnetMaskFromPrefixLength, ValidateIPAddress
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions


Namespace Net.Tcp

    Public Module IPUtils

        ''' <summary>
        ''' ``\A(25[0-5]|2[0-4]\d|[0-1]?\d?\d)(\.(25[0-5]|2[0-4]\d|[0-1]?\d?\d)){3}\z``
        ''' </summary>
        Const RegexIPAddress As String = "\A(25[0-5]|2[0-4]\d|[0-1]?\d?\d)(\.(25[0-5]|2[0-4]\d|[0-1]?\d?\d)){3}\z"

        <Extension>
        Public Function ValidateIPAddress(IPAddress As String) As Boolean
            If IPAddress.StartsWith("0") Then
                Return False
            End If

            If IPAddress.Length = 0 Then
                Return False
            End If

            If Regex.Match(IPAddress, RegexIPAddress).Success Then
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' IPv4 address to long
        ''' </summary>
        ''' <param name="IPAddress"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' > http://www.codeproject.com/Articles/28363/How-to-convert-IP-address-to-country-name
        ''' 
        ''' #### How do I convert an IP address to an IP number?
        ''' 
        ''' IP address (IPV4) is divided into four sub-blocks. Each sub-block has a different weight number, 
        ''' each powered by 256. The IP number is being used in the database because it is efficient to search 
        ''' between a range of numbers in the database.
        ''' 
        ''' The beginning IP number and the ending IP number are calculated based on the following formula:
        ''' 
        ''' ```
        ''' IP Number = 16777216*w + 65536*x + 256*y + z (1)
        ''' ```
        ''' 
        ''' where:
        ''' 
        ''' ```
        ''' IP Address = w.x.y.z
        ''' ```
        ''' 
        ''' For example, if the IP address is ``202.186.13.4``, then its IP number is ``3401190660`` based on the above formula.
        ''' 
        ''' ```
        ''' IP Address = 202.186.13.4
        ''' ```
        ''' 
        ''' So, ``w = 202, x = 186, y = 13`` and ``z = 4``
        ''' 
        ''' ```
        ''' IP Number = 16777216*202 + 65536*186 + 256*13 + 4
        '''           = 3388997632 + 12189696 + 3328 + 4
        '''           = 3401190660
        ''' ```
        ''' 
        ''' To reverse the IP number to the IP address:
        ''' 
        ''' ```
        ''' w = int ( IP Number / 16777216 ) % 256
        ''' x = int ( IP Number / 65536 ) % 256
        ''' y = int ( IP Number / 256 ) % 256
        ''' z = int ( IP Number ) % 256
        ''' ```
        ''' 
        ''' where, ``%`` is the mod operator and int returns the integer part of the division.
        ''' </remarks>
        Public Function GetGeoAddress(IPAddress As String) As Integer
            Dim DotTedIPTokens As String() = IPAddress.Split("."c)
            Dim Dot2LongIP As Integer

            For i As Integer = 0 To 4 - 1
                Dim Num As Integer = CInt(Val(DotTedIPTokens(i)))
                Dot2LongIP = ((Num Mod 256) * (256 ^ (4 - i))) + Dot2LongIP
            Next

            Return Dot2LongIP
        End Function

        Public Function GetSubnetMaskFromPrefixLength(prefixLength As Integer) As IPAddress
            ' 确保前缀长度在0到32之间
            If prefixLength < 0 Or prefixLength > 32 Then
                Throw New ArgumentOutOfRangeException("prefixLength", "前缀长度必须在0到32之间。")
            End If

            ' 将前缀长度转换为子网掩码
            Dim mask As UInteger = UInteger.MaxValue << (32 - prefixLength)
            Dim maskBytes() As Byte = BitConverter.GetBytes(mask)

            ' 由于BitConverter可能会返回一个长度大于4的字节数组，我们需要将其截断或反转
            Array.Reverse(maskBytes)
            Dim subnetMaskBytes(3) As Byte
            Array.Copy(maskBytes, subnetMaskBytes, 4)

            Return New IPAddress(subnetMaskBytes)
        End Function

    End Module
End Namespace
