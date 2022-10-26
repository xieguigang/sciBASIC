#Region "Microsoft.VisualBasic::e869aad3ff463b28eda2fe64724352c4, sciBASIC#\Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\Mappings.vb"

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

    '   Total Lines: 144
    '    Code Lines: 99
    ' Comment Lines: 27
    '   Blank Lines: 18
    '     File Size: 6.29 KB


    '     Module Mappings
    ' 
    '         Function: FieldNameMappings, GetColumnName, (+2 Overloads) GetFields, GetSchema, GetSchemaName
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel.DataAnnotations.Schema
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
        ''' <param name="getName"></param>
        ''' <param name="explict">
        ''' 当自定义属性不存在的时候，隐式的使用域名或者属性名作为名称，否则会跳过该对象，默认是跳过该对象
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function GetFields(Of T As {Attribute})(type As Type, getName As Func(Of T, String), Optional explict As Boolean = False) As BindProperty(Of T)()
            Dim out As New List(Of BindProperty(Of T))

            For Each field As FieldInfo In type.GetFields
                Dim attr As T = field.GetCustomAttribute(Of T)

                If attr Is Nothing Then
                    If explict Then
                        out += New BindProperty(Of T)(field)
                    End If
                Else
                    out += New BindProperty(Of T)(attr, field) With {
                        .Identity = getName(attr)
                    }
                End If
            Next

            For Each [property] As PropertyInfo In type.GetProperties
                Dim attr As T = [property].GetCustomAttribute(Of T)

                If attr Is Nothing Then
                    If explict Then
                        out += New BindProperty(Of T)([property])
                    End If
                Else
                    out += New BindProperty(Of T)(attr, [property]) With {
                        .Identity = getName(attr)
                    }
                End If
            Next

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFields(Of T)(Optional explict As Boolean = True) As BindProperty(Of ColumnAttribute)()
            Return GetType(T).GetFields(Of ColumnAttribute)(Function(o) o.Name, explict:=explict)
        End Function

        ''' <summary>
        ''' 获取从目标类型定义之中的从类型成员域名称映射到列名称的哈希表
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="explict"></param>
        ''' <param name="reversed">
        ''' 当这个参数为真的时候，将会：
        ''' 
        ''' ```
        ''' <see cref="ColumnAttribute.Name"/> => <see cref="MemberInfo.Name"/>
        ''' ```
        ''' </param>
        ''' <returns></returns>
        Public Function FieldNameMappings(Of T)(Optional explict As Boolean = False,
                                                Optional reversed As Boolean = False) As Dictionary(Of String, String)

            Dim fields As BindProperty(Of ColumnAttribute)() = GetFields(Of T)(explict)
            Dim table As Dictionary(Of String, String)

            If Not reversed Then
                table = fields.ToDictionary(Function(field)
                                                ' 获取类型定义之中的成员的属性的反射名称
                                                Return field.member.Name
                                            End Function,
                                            Function(map)
                                                ' 获取该成员上面的自定义属性之中所记录的名称
                                                Return map.GetColumnName
                                            End Function)
            Else
                table = fields _
                    .ToDictionary(Function(map)
                                      ' 获取该成员上面的自定义属性之中所记录的名称
                                      Return map.GetColumnName
                                  End Function,
                                  Function(field)
                                      ' 获取类型定义之中的成员的属性的反射名称
                                      Return field.member.Name
                                  End Function)
            End If

            Return table
        End Function

        <Extension>
        Public Function GetColumnName([property] As BindProperty(Of ColumnAttribute)) As String
            If [property].field Is Nothing OrElse [property].field.Name.StringEmpty Then
                Return [property].Identity
            Else
                Return [property].field.Name
            End If
        End Function

        <Extension>
        Public Function GetSchema(Of TField As Attribute, TTable As Attribute)(
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
