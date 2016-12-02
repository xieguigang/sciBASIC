#Region "Microsoft.VisualBasic::684bb3b8e6f149b6f7d04c30c0168b21, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\Tcp\IPEndPoint.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.ComponentModel
Imports System.Xml.Serialization

Namespace Net

    ''' <summary>
    ''' The object of <see cref="System.Net.IPEndPoint"/> can not be Xml serialization.
    ''' (系统自带的<see cref="System.Net.IPEndPoint"></see>不能够进行Xml序列化)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class IPEndPoint

#Region "Public Property"

        <Browsable(True)>
        <Description("Guid value of this portal information on the server registry.")>
        <XmlAttribute> Public Property uid As String
        <Browsable(True)>
        <Description("IPAddress of the services instance.")>
        <XmlAttribute> Public Property IPAddress As String
        <Browsable(True)>
        <Description("Data port of the services instance.")>
        <XmlAttribute> Public Property Port As Integer
#End Region

        ''' <summary>
        ''' This parameterless constructor is required for the xml serialization.(XML序列化所需要的)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="IPAddress">IPAddress string using for create object using method <see cref="System.Net.IPAddress.Parse(String)"/></param>
        ''' <param name="Port"><see cref="System.Net.IPEndPoint.Port"/></param>
        Sub New(IPAddress As String, Port As Integer)
            Me.Port = Port
            Me.IPAddress = IPAddress
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="str">IPAddress:Port</param>
        ''' <remarks></remarks>
        Sub New(str As String)
            Dim Tokens As String() = str.Split(":"c)

            If Tokens.IsNullOrEmpty OrElse Tokens.Length < 2 Then
                Throw New DataException(str & " is not a valid IPEndPoint string value!")
            End If

            IPAddress = Tokens.First
            Port = CInt(Val(Tokens(1)))
        End Sub

        Sub New(ipEnd As System.Net.IPEndPoint)
            Call Me.New(ipEnd.ToString)
        End Sub

        ''' <summary>
        ''' http://IPAddress:&lt;Port>/
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"http://{IPAddress}:{Port}/"
        End Function

        ''' <summary>
        ''' Convert this networking end point DDM into the <see cref="System.Net.IPEndPoint"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function GetIPEndPoint() As System.Net.IPEndPoint
            Return New System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipString:=IPAddress), Port)
        End Function

        Public Function GetValue() As String
            Return IPAddress & ":" & Port.ToString
        End Function

        ''' <summary>
        ''' 格式是否正确
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsValid As Boolean
            Get
                Return Port > 0 AndAlso System.Net.IPAddress.TryParse(IPAddress, Nothing)
            End Get
        End Property

        Public Shared Narrowing Operator CType(ep As IPEndPoint) As System.Net.IPEndPoint
            Return ep.GetIPEndPoint
        End Operator
    End Class
End Namespace
