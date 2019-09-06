#Region "Microsoft.VisualBasic::84ebe917805a662f87a5ed3016710b8b, Microsoft.VisualBasic.Core\Scripting\MetaData\Type.vb"

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

    '     Class TypeInfo
    ' 
    '         Properties: assm, fullIdentity, isSystemKnownType
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [GetType], LoadAssembly, ToString
    ' 
    '         Sub: doInfoParser
    ' 
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language.Default

Namespace Scripting.MetaData

    ''' <summary>
    ''' The type reference information.(类型信息)
    ''' </summary>
    Public Class TypeInfo

        ''' <summary>
        ''' The assembly file which contains this type definition.(模块文件)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property assm As String
        ''' <summary>
        ''' <see cref="Type.FullName"/>.(类型源)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property fullIdentity As String

        ''' <summary>
        ''' Is this type object is a known system type?(是否是已知的类型？)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isSystemKnownType As Boolean
            Get
                Return Not Scripting.GetType(fullIdentity) Is Nothing
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' Creates type reference from the definition.
        ''' </summary>
        ''' <param name="info"></param>
        Sub New(info As Type)
            Call doInfoParser(info, assm, fullIdentity)
        End Sub

        Private Shared Sub doInfoParser(info As Type, ByRef assm As String, ByRef id As String)
            assm = info.Assembly.Location.FileName
            id = info.FullName
        End Sub

        Public Overrides Function ToString() As String
            Return $"{assm}!{fullIdentity}"
        End Function

        ''' <summary>
        ''' Loads the assembly file which contains this type. If the <param name="directory"></param> is not a valid directory location, 
        ''' then using the location <see cref="App.HOME"/> as default.
        ''' </summary>
        ''' <returns></returns>
        Public Function LoadAssembly(Optional directory As DefaultString = Nothing) As Assembly
            Dim path As String = $"{directory Or App.HOME}/{Me.assm}"
            Dim assm As Assembly = Assembly.LoadFile(path)

            Return assm
        End Function

        ''' <summary>
        ''' Get mapping type information.
        ''' </summary>
        ''' <param name="knownFirst">
        ''' 如果这个参数为真的话, 则会尝试直接从当前的应用程序域中查找类信息, 反之则会加载目标程序集进行类型信息查找
        ''' </param>
        ''' <returns></returns>
        Public Overloads Function [GetType](Optional knownFirst As Boolean = False) As Type
            Dim type As Type
            Dim assm As Assembly

            If knownFirst Then
                type = Scripting.GetType(fullIdentity)

                If Not type Is Nothing Then
                    Return type
                End If
            End If

            assm = LoadAssembly()
            type = assm.GetType(Me.fullIdentity)

            Return type
        End Function

        ''' <summary>
        ''' 检查a是否是指向b的类型引用的
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =(a As TypeInfo, b As Type) As Boolean
            Dim assm As String = Nothing, type As String = Nothing
            Call doInfoParser(b, assm, type)
            Return String.Equals(a.assm, assm, StringComparison.OrdinalIgnoreCase) AndAlso
                String.Equals(a.fullIdentity, type, StringComparison.Ordinal)
        End Operator

        Public Overloads Shared Operator <>(a As TypeInfo, b As Type) As Boolean
            Return Not a = b
        End Operator
    End Class
End Namespace
