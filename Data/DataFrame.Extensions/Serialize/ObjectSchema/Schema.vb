#Region "Microsoft.VisualBasic::09d2293b3edf323abcce6196ec162a5a, Data\DataFrame.Extensions\Serialize\ObjectSchema\Schema.vb"

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

    '   Total Lines: 109
    '    Code Lines: 73 (66.97%)
    ' Comment Lines: 19 (17.43%)
    '    - Xml Docs: 78.95%
    ' 
    '   Blank Lines: 17 (15.60%)
    '     File Size: 4.32 KB


    '     Class Schema
    ' 
    '         Properties: Members, Tables, Type
    ' 
    '         Function: (+2 Overloads) GetSchema, ToString
    ' 
    '         Sub: createMemberStackInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection.TypeSchemaProvider
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Serialize.ObjectSchema

    ''' <summary>
    ''' The schema project json file.
    ''' </summary>
    Public Class Schema

        ''' <summary>
        ''' 默认的主文件的名称
        ''' </summary>
        Public Const DefaultName As String = NameOf(Schema) & ".json"

        ''' <summary>
        ''' ``{member.Name, fileName}``, ``#`` value means this filed its type is the primitive type. 
        ''' If not, then the value goes a external file name.
        ''' </summary>
        ''' <returns></returns>
        Public Property Members As NamedValue(Of String)()
            Get
                Return Tables _
                    .Select(Function(x)
                                Return New NamedValue(Of String) With {
                                    .Name = x.Key,
                                    .Value = x.Value
                                }
                            End Function) _
                    .ToArray
            End Get
            Set(value As NamedValue(Of String)())
                _Tables = value.ToDictionary(Function(x) x.Name, Function(x) x.Value)
            End Set
        End Property

        Public Property Type As String

        ''' <summary>
        ''' <see cref="Members"/> As <see cref="Dictionary"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Tables As IReadOnlyDictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetSchema(Of T As Class)() As Schema
            Return GetSchema(GetType(T))
        End Function

        Public Shared Function GetSchema(type As Type) As Schema
            Dim members As New Dictionary(Of NamedValue(Of String))

            Call createMemberStackInternal(members, type, "$", "#")

            Return New Schema With {
                .Type = type.FullName,
                .Members = members.Values.ToArray
            }
        End Function

        Private Shared Sub createMemberStackInternal(members As Dictionary(Of NamedValue(Of String)), type As Type, parent As String, path As String)
            Dim props = type.GetProperties(BindingFlags.Public + BindingFlags.Instance)
            Dim pType As Type
            Dim elType As Type

#If DEBUG Then
            Call {type.FullName, parent, path}.GetJson.__DEBUG_ECHO
#End If

            For Each prop As PropertyInfo In props
                pType = prop.PropertyType

                ' 假若是基本类型或者字符串，则会直接添加
                If DataFramework.IsPrimitive(pType) Then
                    members += New NamedValue(Of String) With {
                        .Name = $"{parent}::{prop.Name}",
                        .Value = path
                    }
                Else

                    elType = pType.GetThisElement(False)

                    If elType Is Nothing OrElse elType.Equals(pType) Then
                        ' 不是集合类型
                        Call createMemberStackInternal(members, pType, $"{parent}::{prop.Name}", parent.Replace("::", "/") & $"/{prop.Name}.Csv")
                    ElseIf DataFramework.IsPrimitive(elType) Then
                        ' 基本类型
                        members += New NamedValue(Of String) With {
                            .Name = $"{parent}::{prop.Name}",
                            .Value = path
                        }
                    Else
                        ' 复杂类型
                        Call createMemberStackInternal(members, elType, $"{parent}::{prop.Name}", parent.Replace("::", "/") & $"/{prop.Name}.Csv")
                    End If
                End If
            Next
        End Sub
    End Class
End Namespace
