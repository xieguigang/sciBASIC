#Region "Microsoft.VisualBasic::bb217597026f56d3a5314a520aa34fb7, Data\DataFrame\DataFrame\FastLoader.vb"

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

    '   Total Lines: 87
    '    Code Lines: 59 (67.82%)
    ' Comment Lines: 18 (20.69%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 10 (11.49%)
    '     File Size: 3.38 KB


    ' Module FastLoader
    ' 
    '     Function: ParseFeature, ReadCsv
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.TypeCast
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
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
            Call $"found an empty string line after row #{i}.".Warning
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

Public Module FastLoader

    ''' <summary>
    ''' 在这里仅是针对简单格式的csv文件进行快速文件读取操作，对于包含有复杂格式字符串的csv文件，
    ''' 任然需要通过csv文件模块进行读取，之后再通过相应的API进行对象转换
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 简单格式的含义：
    ''' 
    ''' 1. csv文件之中无注释元数据信息
    ''' 2. 单元格的字符串之中无逗号，制表符等分隔符
    ''' </remarks>
    <Extension>
    Public Function ReadCsv(file As Stream,
                            Optional delimiter As Char = ","c,
                            Optional rowHeader As Boolean = True,
                            Optional encoding As Encodings = Encodings.UTF8,
                            Optional verbose As Boolean = True) As DataFrame

        Dim read As New DataFrameReader(file, rowHeader, delimiter, encoding)

        If file.Length > 1 * ByteSize.GB AndAlso verbose Then
            Call read.ReadStreamLines()
        Else
            Call read.ReadLines()
        End If

        Return New DataFrame With {
            .features = read _
                .GetFeatures _
                .ToDictionary(Function(v)
                                  Return v.name
                              End Function),
            .rownames = read _
                .GetRowHeaders _
                .UniqueNames _
                .ToArray
        }
    End Function

    ''' <summary>
    ''' Parse the feature column
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="name"></param>
    ''' <returns></returns>
    <Extension>
    Friend Function ParseFeature(data As List(Of String), name As String) As FeatureVector
        Dim type As Type = DataImports.SampleForType(data, checkNullFactor:=True)
        Dim parser = type.ParseVector
        Dim array As Array = parser(data)

        Return FeatureVector.FromGeneral(name, array)
    End Function
End Module
