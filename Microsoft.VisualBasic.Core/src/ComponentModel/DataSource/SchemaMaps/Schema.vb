#Region "Microsoft.VisualBasic::0aa04d236ac37ed5119f951b5f7be5c5, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\SchemaMaps\Schema.vb"

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

    '   Total Lines: 57
    '    Code Lines: 40 (70.18%)
    ' Comment Lines: 4 (7.02%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (22.81%)
    '     File Size: 2.09 KB


    '     Class Schema
    ' 
    '         Properties: [Namespace], Fields, SchemaName
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: FindField, GetSchema, ToString, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' Schema for two dimension table.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Schema(Of T As Attribute)

        Public Property [Namespace] As String
        Public Property SchemaName As String
        Public Property Fields As BindProperty(Of T)()

        Sub New()
        End Sub

        Sub New(type As Type, Optional getName As Func(Of T, String) = Nothing, Optional explict As Boolean = False)
            Fields = type.GetFields(getName Or Scripting.GetString(Of T), explict).ToArray
            SchemaName = type.Name
            [Namespace] = type.Namespace
        End Sub

        Public Function FindField(name As String) As BindProperty(Of T)
            Return Fields.Where(Function(p) p.Identity = name).FirstOrDefault
        End Function

        Public Function Write(name$, target As Object, value As Object) As Boolean
            Dim p As BindProperty(Of T) = FindField(name)

            If p Is Nothing OrElse p.member Is Nothing Then
                Return False
            End If

            Try
                Call p.SetValue(target, value)
            Catch ex As Exception
                Call App.LogException(ex)
                Return False
            End Try

            Return True
        End Function

        Public Shared Function GetSchema(type As Type, Optional getName As Func(Of T, String) = Nothing, Optional explict As Boolean = False) As Schema(Of T)
            Dim key As String = $"{type.FullName}+{explict}"

            Static cache As New Dictionary(Of String, Schema(Of T))
            Return cache.ComputeIfAbsent(key, Function() New Schema(Of T)(type, getName, explict))
        End Function

        Public Overrides Function ToString() As String
            Return $"[{SchemaName}: {Fields.Keys.GetJson}]"
        End Function
    End Class
End Namespace
