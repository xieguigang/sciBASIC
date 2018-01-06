#Region "Microsoft.VisualBasic::d876f757606fa5b11e2322fb68f8ac3e, ..\sciBASIC#\Data\DataFrame\StorageProvider\Reflection\Attributes\Column.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

Imports System.Data.Linq.Mapping
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' This is a column(or Field) in the csv document. 
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property,
                    AllowMultiple:=False,
                    Inherited:=False)>
    Public Class ColumnAttribute : Inherits DataAttribute
        Implements IAttributeComponent

        Public Overridable ReadOnly Property ProviderId As ProviderIds Implements IAttributeComponent.ProviderId
            Get
                Return ProviderIds.Column
            End Get
        End Property

        ''' <summary>
        ''' The type should implements the interface <see cref="IParser"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CustomParser As Type

        ''' <summary>
        ''' 构建一个列的别名属性值，也可以在这个构造函数之中指定自定义的解析器用来存储非基本类型
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="customParser">The type should implements the interface <see cref="IParser"/>.
        ''' (对于基本类型，这个参数是可以被忽略掉的，但是对于复杂类型，这个参数是不能够被忽略的，否则会报错)
        ''' </param>
        Sub New(Name$, Optional customParser As Type = Nothing)
            Me.Name = Name
            Me.CustomParser = customParser

            If String.IsNullOrEmpty(Name) Then
                Throw New DataException($"{NameOf(Name)} value can not be null!")
            End If
        End Sub

        ''' <summary>
        ''' Display name
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Function GetParser() As IParser
            If CustomParser Is Nothing Then  ' 没有定义自定义解析器
                Return Nothing
            Else
                If Not CustomParser.GetInterface(GetType(IParser).FullName) Is Nothing Then
                    Return DirectCast(Activator.CreateInstance(CustomParser), IParser)
                Else
                    Dim ex As New Exception("==> " & Name)
                    Throw New InvalidProgramException(__innerMsg, ex)
                End If
            End If
        End Function

        Private Function __innerMsg() As String
            Return _
                $"Custom parser required of the type implements interface {GetType(IParser).FullName}, but {CustomParser.FullName} did not!"
        End Function

        ''' <summary>
        ''' Reflector
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property TypeInfo As Type = GetType(ColumnAttribute)

        Public Shared Narrowing Operator CType(attr As ColumnAttribute) As String
            Return attr.Name
        End Operator

        Public Shared Widening Operator CType(sName As String) As ColumnAttribute
            Return New ColumnAttribute(sName)
        End Operator
    End Class
End Namespace
