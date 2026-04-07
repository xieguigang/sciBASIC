#Region "Microsoft.VisualBasic::fe8beefe0ce2fe383ba5e291f75354ac, Data\DataFrame\DataFrame\DataFrameReader.vb"

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

    '   Total Lines: 94
    '    Code Lines: 76 (80.85%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (19.15%)
    '     File Size: 3.08 KB


    ' Class DataFrameReader
    ' 
    '     Properties: delimiter, rowHeader
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: GetFeatures, GetRowHeaders, ReadLine
    ' 
    '     Sub: ReadLine, ReadLines, ReadStreamLines
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.TypeCast
Imports Microsoft.VisualBasic.Data.Framework.IO.CSVFile
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Public Class DataFrameReader

    Public ReadOnly Property delimiter As Char = ","c
    Public ReadOnly Property rowHeader As Boolean = True

    ReadOnly file As Stream
    ReadOnly read As StreamReader
    ReadOnly rowHeaders As New List(Of String)
    ReadOnly features As New Dictionary(Of String, List(Of String))
    ReadOnly ordinals As Index(Of String)

    Sub New(file As Stream, rowHeader As Boolean, delimiter As Char, encoding As Encodings)
        Me.file = file
        Me.read = New StreamReader(file, encoding.CodePage)
        Me.delimiter = delimiter
        Me.rowHeader = rowHeader

        ordinals = Tokenizer.CharsParser(read.ReadLine, delimiter).Indexing

        If rowHeader Then
            For Each name As String In ordinals.Objects.Skip(1)
                Call features.Add(name, New List(Of String))
            Next
        Else
            For Each name As String In ordinals.Objects
                Call features.Add(name, New List(Of String))
            Next
        End If
    End Sub

    Public Function GetRowHeaders() As IEnumerable(Of String)
        Return rowHeaders
    End Function

    Public Iterator Function GetFeatures() As IEnumerable(Of FeatureVector)
        For Each feature As KeyValuePair(Of String, List(Of String)) In features
            Yield feature.Value.ParseFeature(feature.Key)
        Next
    End Function

    Private Sub ReadLine(i As i32, line As String)
        Dim tokens As String() = Tokenizer.CharsParser(line, delimiter).ToArray

        If tokens.Length = 0 Then
            Call $"found an empty string line after row #{i}.".warning
            Return
        End If

        If rowHeader Then
            rowHeaders.Add(tokens(Scan0))
        Else
            rowHeaders.Add(++i)
        End If

        For Each name As String In features.Keys
            Call features(name).Add(tokens(ordinals(x:=name)))
        Next
    End Sub

    Public Sub ReadStreamLines()
        Dim i As i32 = 1

        For Each line_str As String In TqdmWrapper.WrapStreamReader(file.Length, AddressOf ReadLine)
            If line_str Is Nothing Then
                Exit For
            End If

            Call ReadLine(i, line_str)
        Next
    End Sub

    Private Function ReadLine(ByRef getOffset As Long, bar As ProgressBar) As String
        Dim str = read.ReadLine
        getOffset = file.Position
        Return str
    End Function

    Public Sub ReadLines()
        Dim line As Value(Of String) = ""
        Dim i As i32 = 1

        Do While Not (line = read.ReadLine) Is Nothing
            Call ReadLine(i, CStr(line))
        Loop
    End Sub
End Class
