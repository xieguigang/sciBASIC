#Region "Microsoft.VisualBasic::3cc65340137deaf3d3e70aab7919109c, mime\text%yaml\Serialization.vb"

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

    '   Total Lines: 25
    '    Code Lines: 16 (64.00%)
    ' Comment Lines: 6 (24.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (12.00%)
    '     File Size: 1.13 KB


    ' Module Serialization
    ' 
    '     Function: LoadYAML, LoadYAMLDocument
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.json

Public Module Serialization

    ''' <summary>
    ''' De-serialization of the yaml document file as the required .NET CLR object
    ''' </summary>
    ''' <typeparam name="T">the type information of the required .NET CLR object to be deserialization from the YAML document data</typeparam>
    ''' <param name="path">file path to the target yaml document file</param>
    ''' <returns>Target .NET clr object that de-serialized from the given yaml document file, contains the data that read from the yaml document file.</returns>
    <Extension>
    Public Function LoadYAML(Of T As {New, Class})(path As String) As T
        Try
            Return LoadYAMLDocument(Of T)(path.GET)
        Catch ex As Exception
            Throw New InvalidProgramException(path, ex)
        End Try
    End Function

    <Extension>
    Public Function LoadYAMLDocument(Of T As {New, Class})(yaml As String) As T
        Return New YamlParser().Parse(yaml).CreateObject(Of T)(decodeMetachar:=True)
    End Function
End Module
