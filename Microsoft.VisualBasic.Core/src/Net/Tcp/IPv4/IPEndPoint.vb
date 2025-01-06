#Region "Microsoft.VisualBasic::c3080c6bf92edf11fceb293b9ff45867, Microsoft.VisualBasic.Core\src\Net\Tcp\IPv4\IPEndPoint.vb"

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

    '   Total Lines: 148
    '    Code Lines: 78 (52.70%)
    ' Comment Lines: 46 (31.08%)
    '    - Xml Docs: 95.65%
    ' 
    '   Blank Lines: 24 (16.22%)
    '     File Size: 5.34 KB


    '     Class IPEndPoint
    ' 
    '         Properties: guid, ipAddress, IsValid, port
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: CreateLocal, GetIPEndPoint, GetValue, IsIpv4Address, ToString
    ' 
    '         Sub: ParseIpPort
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Xml.Serialization

Namespace Net

    ''' <summary>
    ''' The object of <see cref="System.Net.IPEndPoint"/> can not be Xml serialization.
    ''' </summary>
    ''' <remarks>
    ''' (系统自带的<see cref="System.Net.IPEndPoint"></see>不能够进行Xml序列化)
    ''' </remarks>
    Public Class IPEndPoint

        ''' <summary>
        ''' Guid value of this portal information on the server registry.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(True)>
        <Description("Guid value of this portal information on the server registry.")>
        <XmlAttribute> Public Property guid As String

        ''' <summary>
        ''' IPAddress of the services instance.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(True)>
        <Description("IPAddress of the services instance.")>
        <XmlAttribute> Public Property ipAddress As String

        ''' <summary>
        ''' Data port of the services instance.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(True)>
        <Description("Data port of the services instance.")>
        <XmlAttribute> Public Property port As Integer

        ''' <summary>
        ''' 格式是否正确
        ''' </summary>
        ''' <returns></returns>
        <SoapIgnore>
        Public ReadOnly Property IsValid As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return port > 0 AndAlso System.Net.IPAddress.TryParse(ipAddress, Nothing)
            End Get
        End Property

        ''' <summary>
        ''' This parameterless constructor is required for the xml serialization.(XML序列化所需要的)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="IPAddress">
        ''' IPAddress string using for create object using 
        ''' method <see cref="System.Net.IPAddress.Parse(String)"/>
        ''' </param>
        ''' <param name="Port"><see cref="System.Net.IPEndPoint.Port"/></param>
        Sub New(IPAddress As String, Port As Integer)
            If IPAddress.TextEquals("localhost") Then
                Me.ipAddress = "127.0.0.1"
            Else
                Me.ipAddress = IPAddress
            End If

            Me.port = Port
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="str">Required format string: ``IPAddress:Port``</param>
        ''' <remarks></remarks>
        Sub New(str As String)
            Call ParseIpPort(str, ipAddress, port)

            If ipAddress.StringEmpty Then
                Throw New DataException(str & " is not a valid IPEndPoint string value!")
            End If
        End Sub

        Sub New(ipEnd As System.Net.IPEndPoint)
            Call Me.New(ipEnd.ToString)
        End Sub

        Public Shared Sub ParseIpPort(ipPort As String, <Out> ByRef ip As String, <Out> ByRef port As Integer)
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))

            ip = Nothing
            port = -1

            Dim colonIndex = ipPort.LastIndexOf(":"c)
            If colonIndex <> -1 Then
                ip = ipPort.Substring(0, colonIndex)
                port = Convert.ToInt32(ipPort.Substring(colonIndex + 1))
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"{ipAddress}:{port}"
        End Function

        ''' <summary>
        ''' Convert this networking end point DDM into the <see cref="System.Net.IPEndPoint"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function GetIPEndPoint() As System.Net.IPEndPoint
            Return New System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipString:=ipAddress), port)
        End Function

        Public Function GetValue() As String
            Return ipAddress & ":" & port.ToString
        End Function

        Public Shared Function CreateLocal(localPort As Integer, Optional host As String = "localhost") As IPEndPoint
            Return New IPEndPoint(host, localPort)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(ep As IPEndPoint) As System.Net.IPEndPoint
            Return ep.GetIPEndPoint
        End Operator

        Public Const RegexIPAddress As String = "\d{1,3}(\.\d{1,3}){3}"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsIpv4Address(addr As String) As Boolean
            Return addr.IsPattern(RegexIPAddress)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(endpoint As (ip$, port%)) As IPEndPoint
            Return New IPEndPoint(endpoint.ip, endpoint.port)
        End Operator

    End Class
End Namespace
