#Region "Microsoft.VisualBasic::87501519ab89b0ea4ac8f6f922922c80, mime\application%rdf+xml\DataTypes.vb"

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

    '   Total Lines: 52
    '    Code Lines: 34
    ' Comment Lines: 12
    '   Blank Lines: 6
    '     File Size: 1.72 KB


    ' Module DataTypes
    ' 
    '     Function: [GetType], (+2 Overloads) SchemaDataType
    ' 
    ' /********************************************************************************/

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

    ReadOnly __types As New Dictionary(Of Type, String) From {
        {GetType(String), dtString},
        {GetType(Integer), dtInteger},
        {GetType(Double), dtDouble}
    }
End Module
