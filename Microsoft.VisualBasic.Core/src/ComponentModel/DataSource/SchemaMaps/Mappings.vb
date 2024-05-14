#Region "Microsoft.VisualBasic::0ffe7734572ed00c0c1613b759265e8b, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\Mappings.vb"

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

    '   Total Lines: 243
    '    Code Lines: 142
    ' Comment Lines: 68
    '   Blank Lines: 33
    '     File Size: 9.86 KB


    '     Module Mappings
    ' 
    '         Function: (+2 Overloads) FieldNameMappings, GetAliasName, GetAliasNames, GetColumnName, (+2 Overloads) GetFields
    '                   GetSchema, GetSchemaName
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace ComponentModel.DataSourceModel.SchemaMaps

    Public Module Mappings

        ''' <summary>
        ''' get alias name of a specific type class
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="type"></param>
        ''' <param name="getName">get name from the attribute <typeparamref name="T"/> object</param>
        ''' <param name="explict"></param>
        ''' <returns></returns>
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
        Public Iterator Function GetFields(Of T As {Attribute})(type As Type,
                                                                getName As Func(Of T, String),
                                                                Optional explict As Boolean = False) As IEnumerable(Of BindProperty(Of T))
            For Each field As FieldInfo In type.GetFields
                Dim attr As T = field.GetCustomAttribute(Of T)

                If attr Is Nothing Then
                    If explict Then
                        Yield New BindProperty(Of T)(field)
                    End If
                Else
                    Yield New BindProperty(Of T)(attr, field) With {
                        .Identity = getName(attr)
                    }
                End If
            Next

            For Each [property] As PropertyInfo In type.GetProperties
                Dim attr As T = [property].GetCustomAttribute(Of T)

                If attr Is Nothing Then
                    If explict Then
                        Yield New BindProperty(Of T)([property])
                    End If
                Else
                    Yield New BindProperty(Of T)(attr, [property]) With {
                        .Identity = getName(attr)
                    }
                End If
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="explict">
        ''' construct the default masked <see cref="ColumnAttribute"/> if this parameter value is set to True
        ''' </param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFields(Of T)(Optional explict As Boolean = True) As BindProperty(Of ColumnAttribute)()
            Return GetType(T).GetFields(Of ColumnAttribute)(Function(o) o.Name, explict:=explict).ToArray
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
                                                Optional reversed As Boolean = False,
                                                Optional includesAliasNames As Boolean = False) As Dictionary(Of String, String)

            Return GetFields(Of T)(explict).FieldNameMappings(Of T)(reversed, includesAliasNames)
        End Function

        <Extension>
        Public Iterator Function GetAliasNames(map As BindProperty(Of ColumnAttribute)) As IEnumerable(Of String)
            Yield map.memberName

            If Not map.field Is Nothing Then
                Yield map.GetColumnName

                If Not map.field.alias.IsNullOrEmpty Then
                    For Each name As String In map.field.alias
                        Yield name
                    Next
                End If
            End If
        End Function

        ''' <summary>
        ''' try to get the alias name for the specific property object
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns>
        ''' this function will returns nothing if no alias name mapping attribute could be found
        ''' </returns>
        ''' <remarks>
        ''' the alias name could be tagged with attributes:
        ''' 
        ''' 1. <see cref="ColumnAttribute"/>
        ''' 2. <see cref="Field"/>
        ''' 3. <see cref="DataFrameColumnAttribute"/>
        ''' </remarks>
        <Extension>
        Public Function GetAliasName(p As PropertyInfo) As String
            Dim a1 As ColumnAttribute = p.GetCustomAttribute(Of ColumnAttribute)

            If Not a1 Is Nothing Then
                Return a1.Name
            End If

            Dim a2 As Field = p.GetCustomAttribute(Of Field)

            If Not a2 Is Nothing Then
                Return a2.Name
            End If

            Dim a3 As DataFrameColumnAttribute = p.GetCustomAttribute(Of DataFrameColumnAttribute)

            If Not a3 Is Nothing Then
                Return a3.Name
            End If

#If NETCOREAPP Then
            Dim a4 As System.ComponentModel.DataAnnotations.Schema.ColumnAttribute = p.GetCustomAttribute(Of System.ComponentModel.DataAnnotations.Schema.ColumnAttribute)

            If Not a4 Is Nothing Then
                Return a4.Name
            End If
#End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="fields">raw mapping</param>
        ''' <param name="reversed"></param>
        ''' <param name="includesAliasNames"></param>
        ''' <returns></returns>
        <Extension>
        Public Function FieldNameMappings(Of T)(fields As IEnumerable(Of BindProperty(Of ColumnAttribute)),
                                                Optional reversed As Boolean = False,
                                                Optional includesAliasNames As Boolean = False) As Dictionary(Of String, String)

            Dim table As New Dictionary(Of String, String)
            Dim memberName As String

            For Each map As BindProperty(Of ColumnAttribute) In fields
                ' 获取类型定义之中的成员的属性的反射名称
                memberName = map.memberName

                If Not reversed Then
                    ' 获取该成员上面的自定义属性之中所记录的名称
                    table(memberName) = map.GetColumnName
                Else
                    If includesAliasNames Then
                        table(map.GetColumnName) = memberName

                        If Not map.field Is Nothing Then
                            If Not map.field.alias.IsNullOrEmpty Then
                                For Each name As String In map.field.alias
                                    table(name) = memberName
                                Next
                            End If
                        End If
                    Else
                        table(map.GetColumnName) = memberName
                    End If
                End If
            Next

            Return table
        End Function

        ''' <summary>
        ''' get name value from <see cref="ColumnAttribute.Name"/>
        ''' </summary>
        ''' <param name="[property]"></param>
        ''' <returns></returns>
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
                .Fields = type.GetFields(Of TField)(getField, explict).ToArray
            }
        End Function
    End Module
End Namespace
