#Region "Microsoft.VisualBasic::1f5a7be2da2d23af411f895ece968967, www\Microsoft.VisualBasic.NETProtocol\Protocol\Reflection\ProtocolAttribute.vb"

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

    '   Total Lines: 88
    '    Code Lines: 38 (43.18%)
    ' Comment Lines: 40 (45.45%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 10 (11.36%)
    '     File Size: 3.95 KB


    '     Class ProtocolAttribute
    ' 
    '         Properties: DeclaringType, EntryPoint
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetEntryPoint, GetProtocolCategory, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Parallel

Namespace Protocols.Reflection

    ''' <summary>
    ''' This attribute indicates the entry point of the protocol processor definition location 
    ''' and the details of the protocol processor. 
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class ProtocolAttribute : Inherits Attribute

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
        ''' <param name="entryPoint"></param>
        Sub New(entryPoint As Long)
            Me.EntryPoint = entryPoint
        End Sub

        ''' <summary>
        ''' Generates the <see cref="ProtocolHandler"/> on the server side, 
        ''' this is using for initialize a protocol API entry point.
        ''' (客户端上面的类型)
        ''' </summary>
        ''' <param name="type">客户端上面的类型</param>
        Sub New(type As Type)
            ' 20201011
            ' 原先是使用type.guid来产生唯一值
            ' 但是后来发现.NET和mono上面得到的guid值似乎会不一样
            ' 所以为了提高兼容性，在这里使用类型的全称的md5值来计算引用
            EntryPoint = SecurityString.MD5Hash.ToLong(type.FullName.MD5)
            DeclaringType = type
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
        Public Shared Function GetProtocolCategory(Type As Type) As ProtocolAttribute
            Dim attrs As Object() = Type.GetCustomAttributes(attributeType:=GetType(ProtocolAttribute), inherit:=True)
            If attrs.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim attr As ProtocolAttribute = DirectCast(attrs(Scan0), ProtocolAttribute)
            Return attr
        End Function

        ''' <summary>
        ''' This method is usually using for generates a details protocol processor, example 
        ''' is calling the method interface: <see cref="DataRequestHandler"/>
        ''' Correspondent to the protocol entry property <see cref="RequestStream.Protocol"/>
        ''' </summary>
        ''' <param name="Method"></param>
        ''' <returns></returns>
        Public Shared Function GetEntryPoint(Method As System.Reflection.MethodInfo) As ProtocolAttribute
            Dim attrs As Object() = Method.GetCustomAttributes(attributeType:=GetType(ProtocolAttribute), inherit:=True)
            If attrs.IsNullOrEmpty Then
                Return Nothing
            End If

            Dim attr As ProtocolAttribute = DirectCast(attrs(Scan0), ProtocolAttribute)
            Return attr
        End Function
    End Class
End Namespace
