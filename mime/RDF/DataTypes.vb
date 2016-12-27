#Region "Microsoft.VisualBasic::326678b7d1ff4ca83b5f33d11c643d42, ..\sciBASIC#\mime\RDF\DataTypes.vb"

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
''' <summary>
''' http://www.w3.org/2001/XMLSchema
''' </summary>
Public Module DataTypes

    Public Const dtString As String = "http://www.w3.org/2001/XMLSchema#string"
    Public Const dtInteger As String = "http://www.w3.org/2001/XMLSchema#int"
    Public Const dtDouble As String = "http://www.w3.org/2001/XMLSchema#float"

    ''' <summary>
    ''' RDF data type
    ''' </summary>
    ''' <returns></returns>
    Public Function [GetType](schema As String) As Type
        If schema.TextEquals(DataTypes.dtDouble) Then
            Return GetType(Double)
        ElseIf schema.TextEquals(DataTypes.dtInteger) Then
            Return GetType(Integer)
        ElseIf schema.TextEquals(DataTypes.dtString) Then
            Return GetType(String)
        Else
            Throw New NotSupportedException(schema)
        End If
    End Function

    ''' <summary>
    ''' Default is string type if property value of <see cref="EntityProperty.dataType"/> is null or empty
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SchemaDataType(x As EntityProperty) As Type
        If String.IsNullOrEmpty(x.dataType) Then
            Return GetType(String)
        Else
            Return DataTypes.GetType(x.dataType)
        End If
    End Function

    <Extension>
    Public Function SchemaDataType(type As Type) As String
        Return __types(type)
    End Function

    ReadOnly __types As Dictionary(Of Type, String) =
        New Dictionary(Of Type, String) From {
            {GetType(String), dtString},
            {GetType(Integer), dtInteger},
            {GetType(Double), dtDouble}
    }
End Module
