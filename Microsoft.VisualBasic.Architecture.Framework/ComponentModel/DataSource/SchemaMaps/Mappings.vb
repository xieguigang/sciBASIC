#Region "Microsoft.VisualBasic::7c974c40710cbb191ea0da34a3623fd4, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\SchemaMaps\Mappings.vb"

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

Imports System.Data.Linq.Mapping
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.DataSourceModel.SchemaMaps

    Public Module Mappings

        <Extension>
        Public Function GetSchemaName(Of T As Attribute)(
                                     type As Type,
                                     getName As Func(Of T, String),
                                     Optional explict As Boolean = False) As String

            Dim attrs As Object() = type.GetCustomAttributes(GetType(T), inherit:=True)

            If attrs.IsNullOrEmpty Then
                If explict Then
                    Return type.Name
                Else
                    Return Nothing
                End If
            Else
                Dim attr As T = DirectCast(attrs(Scan0), T)
                Dim name As String = getName(attr)
                Return name
            End If
        End Function

        ''' <summary>
        ''' 这个只是得到最上面的一层属性值
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="type"></param>
        ''' <param name="getFieldName"></param>
        ''' <param name="explict">
        ''' 当自定义属性不存在的时候，隐式的使用域名或者属性名作为名称，否则会跳过该对象，默认是跳过该对象
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function GetFields(Of T As {Attribute})(
                                 Type As Type,
                                 getFieldName As Func(Of T, String),
                                 Optional explict As Boolean = False) As BindProperty(Of T)()
            Dim out As New List(Of BindProperty(Of T))

            For Each field As FieldInfo In Type.GetFields
                Dim attr As T = field.GetCustomAttribute(Of T)
                If attr Is Nothing Then
                    If explict Then
                        out += New BindProperty(Of T)(field)
                    End If
                Else
                    out += New BindProperty(Of T)(attr, field)
                End If
            Next

            For Each [property] As PropertyInfo In Type.GetProperties
                Dim attr As T = [property].GetCustomAttribute(Of T)
                If attr Is Nothing Then
                    If explict Then
                        out += New BindProperty(Of T)([property])
                    End If
                Else
                    out += New BindProperty(Of T)(attr, [property])
                End If
            Next

            Return out
        End Function

        Public Function GetFields(Of T)(Optional explict As Boolean = True) As BindProperty(Of ColumnAttribute)()
            Return GetType(T).GetFields(Of ColumnAttribute)(Function(o) o.Name, explict:=explict)
        End Function

        ''' <summary>
        ''' 获取从域名称映射到列名称的哈希表
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="explict"></param>
        ''' <returns></returns>
        Public Function FieldNameMappings(Of T)(Optional explict As Boolean = False) As Dictionary(Of String, String)
            Dim fields As BindProperty(Of ColumnAttribute)() = GetFields(Of T)(explict)
            Dim table As Dictionary(Of String, String) = fields _
                .ToDictionary(Function(field) field.Identity,
                              Function(map) map.GetColumnName)
            Return table
        End Function

        <Extension>
        Public Function GetColumnName([property] As BindProperty(Of ColumnAttribute)) As String
            If [property].Field Is Nothing OrElse [property].Field.Name.StringEmpty Then
                Return [property].Identity
            Else
                Return [property].Field.Name
            End If
        End Function

        <Extension>
        Public Function GetSchema(Of TField As Attribute,
                                 TTable As Attribute)(
                                 type As Type,
                                 getTableName As Func(Of TTable, String),
                                 getField As Func(Of TField, String),
                                 Optional explict As Boolean = False) As Schema(Of TField)

            Return New Schema(Of TField) With {
                .SchemaName = type.GetSchemaName(Of TTable)(getTableName, explict),
                .Fields = type.GetFields(Of TField)(getField, explict)
            }
        End Function
    End Module
End Namespace
