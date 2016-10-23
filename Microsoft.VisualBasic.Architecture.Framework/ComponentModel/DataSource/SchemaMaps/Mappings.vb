#Region "Microsoft.VisualBasic::2df7c1dac40fa6dce1aced53471e08e0, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\SchemaMaps\Mappings.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

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
        ''' <param name="explict"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetFields(Of T As Attribute)(
                                 type As Type,
                                 getFieldName As Func(Of T, String),
                                 Optional explict As Boolean = False) As BindProperty(Of T)()
            Throw New NotImplementedException
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
