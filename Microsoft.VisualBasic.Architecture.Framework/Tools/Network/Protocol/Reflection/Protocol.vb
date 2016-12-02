#Region "Microsoft.VisualBasic::61f744038f9a91c79bba8fcfd539bebe, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\Protocol\Reflection\Protocol.vb"

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

Namespace Net.Protocols.Reflection

    ''' <summary>
    ''' This attribute indicates the entry point of the protocol processor definition location and the details of the protocol processor. 
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class Protocol : Inherits Attribute

        ''' <summary>
        ''' Entry point for the data protocols, this property usually correspondent to the request stream's 
        ''' property: <see cref="RequestStream.Protocol"/> and <see cref="RequestStream.ProtocolCategory"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EntryPoint As Long
        ''' <summary>
        ''' 这个属性对于方法而言为空，但是对于类型入口点而言则不为空
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DeclaringType As Type

        ''' <summary>
        ''' Generates the protocol method entrypoint.(应用于服务器上面的协议处理方法)
        ''' </summary>
        ''' <param name="EntryPoint"></param>
        Sub New(EntryPoint As Long)
            Me.EntryPoint = EntryPoint
        End Sub

        ''' <summary>
        ''' Generates the <see cref="ProtocolHandler"/> on the server side, this is using for initialize a protocol API entry point.(客户端上面的类型)
        ''' </summary>
        ''' <param name="Type">客户端上面的类型</param>
        Sub New(Type As Type)
            EntryPoint = SecurityString.MD5Hash.ToLong(Type.GUID.ToByteArray)
            DeclaringType = Type
        End Sub

        Public Overrides Function ToString() As String
            If DeclaringType Is Nothing Then
                Return EntryPoint
            Else
                Return $"*{EntryPoint}  ==> {DeclaringType.FullName}"
            End If
        End Function

        ''' <summary>
        ''' This method is usually using for generates a <see cref="protocolhandler"/> object.
        ''' Correspondent to the protocol class property <see cref="RequestStream.ProtocolCategory"/>
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        Public Shared Function GetProtocolCategory(Type As Type) As Protocol
            Dim attrs As Object() = Type.GetCustomAttributes(attributeType:=GetType(Protocol), inherit:=True)
            If attrs.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim attr As Protocol = DirectCast(attrs(Scan0), Protocol)
            Return attr
        End Function

        ''' <summary>
        ''' This method is usually using for generates a details protocol processor, example is calling the method interface: <see cref="Net.Abstract.DataRequestHandler"/>
        ''' Correspondent to the protocol entry property <see cref="RequestStream.Protocol"/>
        ''' </summary>
        ''' <param name="Method"></param>
        ''' <returns></returns>
        Public Shared Function GetEntryPoint(Method As System.Reflection.MethodInfo) As Protocol
            Dim attrs As Object() = Method.GetCustomAttributes(attributeType:=GetType(Protocol), inherit:=True)
            If attrs.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim attr As Protocol = DirectCast(attrs(Scan0), Protocol)
            Return attr
        End Function
    End Class
End Namespace
