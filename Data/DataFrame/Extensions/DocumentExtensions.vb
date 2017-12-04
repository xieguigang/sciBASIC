#Region "Microsoft.VisualBasic::a07867e88b814c49c0c30f739aed0adc, ..\sciBASIC#\Data\DataFrame\Extensions\DocumentExtensions.vb"

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

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

''' <summary>
''' The csv document extensions API
''' </summary>
Public Module DocumentExtensions

    <Extension>
    Public Iterator Function InvalidsAsRLangNA(source As IEnumerable(Of DataSet), Optional replaceAs$ = "NA") As IEnumerable(Of EntityObject)
        Dim NaN As Index(Of String) = {
            "正无穷大", "负无穷大", "非数字",
            "Infinity", "-Infinity",
            "NaN",
            "∞", "-∞"
        }

        For Each data As DataSet In source
            Dim values = data _
                .Properties _
                .ToDictionary(Function(map) map.Key,
                              Function(map)
                                  Dim s = map.Value.ToString

                                  If NaN.IndexOf(s) > -1 Then
                                      Return replaceAs
                                  Else
                                      Return s
                                  End If
                              End Function)

            Yield New EntityObject With {
                .ID = data.ID,
                .Properties = values
            }
        Next
    End Function

    ''' <summary>
    ''' 对于一些数学计算的数值结果，无穷大，无穷小或者非实数会被转换为中文，导致R程序无法识别
    ''' 则需要使用这个函数来将这些数值替换为目标字符串<paramref name="replaceAs"/>
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="replaceAs$">默认为R之中可以识别的``NA``常数值</param>
    ''' <returns></returns>
    Public Function StripNaN(path$, Optional replaceAs$ = "NA") As Boolean
        Dim csv As IO.File = IO.File.Load(path)
        Dim invalids As Index(Of String) = {"正无穷大", "负无穷大", "非数字"}.Indexing
        Dim file As New List(Of RowObject)

        ' 因为第一行一般都是标题行，所以在这里直接跳过了
        For Each row In csv.Skip(1)
            Dim buffer = row.ToArray

            For i As Integer = 0 To buffer.Length - 1
                If invalids.IndexOf(buffer(i)) > -1 Then
                    buffer(i) = replaceAs
                End If
            Next

            file += New RowObject(buffer)
        Next

        csv = csv.First + file

        Return csv.Save(path, )
    End Function

    ''' <summary>
    ''' 将列数据合并为一个csv文件对象
    ''' </summary>
    ''' <param name="cols"><see cref="io.File.Columns"/> filtering results.</param>
    ''' <returns></returns>
    <Extension>
    Public Function JoinColumns(cols As IEnumerable(Of String())) As IO.File
        Dim array$()() = cols.ToArray
        Dim out As New IO.File

        For i As Integer = 0 To array.First.Length - 1
            Dim ind As Integer = i
            out += New RowObject(array.Select(Function(x) x(ind)))
        Next

        Return out
    End Function

    <Extension>
    Public Function Apply(ByRef row As RowObject, action As Func(Of String, String), Optional skip As Integer = 0) As RowObject
        For i As Integer = skip To row.buffer.Count - 1
            row.buffer(i) = action(row.buffer(i))
        Next

        Return row
    End Function

    Private Class GenericTable
        Public Property Data As Dictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return Data.GetJson
        End Function
    End Class

    <Extension>
    Public Function SaveAsDataFrame(d As IEnumerable(Of Dictionary(Of String, String)), path$) As Boolean
        Dim table As GenericTable() = d _
            .Select(Function(x) New GenericTable With {.Data = x}) _
            .ToArray
        Return table.SaveTo(path)
    End Function

    <Extension>
    Public Function SaveAsDataFrame(d As IEnumerable(Of Dictionary(Of String, Double)), path$) As Boolean
        Return d _
            .Select(
            Function(x) x.ToDictionary(
            Function(k) k.Key,
            Function(v) v.Value.ToString)) _
            .SaveAsDataFrame(path)
    End Function

    ''' <summary>
    ''' Directly merges the csv table files from the target source <paramref name="files"/>, 
    ''' and save the result to a specific file location <paramref name="EXPORT"/>
    ''' </summary>
    ''' <param name="files"></param>
    ''' <param name="EXPORT$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function DirectAppends(files As IEnumerable(Of String), EXPORT$) As Boolean
        Dim data As New List(Of GenericTable)

        For Each path$ In files
            ' List(Of T) 对象的 + 语法有冲突，所以在这里需要先进行转换
            data += DirectCast(path.LoadCsv(Of GenericTable), IEnumerable(Of GenericTable))
        Next

        Return data.SaveTo(EXPORT)
    End Function

    <Extension>
    Public Function SaveTsv(csv As IO.File, path$, Optional encoding As Encodings = Encodings.ASCII) As Boolean
        Using file As StreamWriter = path.OpenWriter(encoding)
            For Each line In csv
                Call file.WriteLine(line.TsvLine)
            Next

            Return True
        End Using
    End Function

    <Extension>
    Public Function TsvLine(row As RowObject) As String
        Dim ls As New List(Of String)

        For Each c As String In row
            If c.Contains(ASCII.TAB) Then
                c = $"""{c}"""
            End If

            ls.Add(c)
        Next

        Return ls.JoinBy(ASCII.TAB)
    End Function

    ''' <summary>
    ''' 文件之中的每一列都是数据
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="skipFirstColumn">假若第一列是固定的时间序列的话，是否需要跳过这第一列？？</param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadData(path$, Optional skipFirstColumn As Boolean = False) As NamedValue(Of Double())()
        Dim data As IO.File = IO.File.Load(path)
        Dim source As IEnumerable(Of String())

        If skipFirstColumn Then
            source = data.Columns.Skip(1)
        Else
            source = data.Columns
        End If

        Dim out = LinqAPI.Exec(Of NamedValue(Of Double())) _
 _
            () <= From column As String()
                  In source
                  Let name As String = column(Scan0)
                  Let values As Double() = column _
                      .Skip(1) _
                      .Select(AddressOf Val) _
                      .ToArray
                  Select New NamedValue(Of Double()) With {
                      .Name = name,
                      .Value = values
                  }

        Return out
    End Function

    ''' <summary>
    ''' Load a .NET collection from a tsv file which is specific by <paramref name="path"/> value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <param name="nameMaps"></param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadTsv(Of T As Class)(path$,
                                           Optional encoding As Encodings = Encodings.Default,
                                           Optional nameMaps As Dictionary(Of String, String) = Nothing) As T()
        Return [Imports](Of T)(path,
                               delimiter:=ASCII.TAB,
                               encoding:=encoding.CodePage,
                               nameMaps:=nameMaps)
    End Function

    <Extension>
    Public Iterator Function LoadMappings(path$, key$, mapTo$) As IEnumerable(Of Map(Of String, String))
        Dim header As RowObject = RowObject.TryParse(path.ReadFirstLine)
        Dim keyIndex% = header.IndexOf(key)
        Dim mapIndex% = header.IndexOf(mapTo)

        If keyIndex = -1 OrElse mapIndex = -1 Then
            Dim msg$ =
                $"Mapping: {key} --> {mapTo} is missing in the target csv file! ({header.ToArray.GetJson})"
            Throw New KeyNotFoundException(msg)
        End If

        Dim skip1 As Boolean = True

        For Each line$ In path.IterateAllLines
            If Not skip1 Then
                Dim row As RowObject = RowObject.TryParse(line)

                Yield New Map(Of String, String) With {
                    .Key = row(keyIndex),
                    .Maps = row(mapIndex)
                }
            Else
                skip1 = False
            End If
        Next
    End Function

    <Extension>
    Public Function GetColumnValues(csv As IO.File, column$) As IEnumerable(Of String)
        Dim index As Integer = csv.Headers.IndexOf(column)
        Dim out As New List(Of String)

        For Each r As RowObject In csv.Skip(1)
            Call out.Add(r(index))
        Next

        Return out
    End Function

    <Extension>
    Public Iterator Function GetColumnObjects(Of T)(csv As IO.File, column$, [ctype] As Func(Of String, T)) As IEnumerable(Of T)
        For Each row As String In csv.GetColumnValues(column)
            Yield [ctype](row)
        Next
    End Function

    <Extension>
    Public Function LoadCsv(path$, Optional encoding As Encodings = Encodings.ASCII) As IO.File
        Return IO.File.Load(path, encoding.CodePage)
    End Function

    ''' <summary>
    ''' 这个函数会自动判断对象的格式为tsv还是csv文件格式
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 采样前几行的数据，假若是csv文件的话，则逗号出现的频率要高于tab分隔符，反之亦然
    ''' </remarks>
    <Extension>
    Public Function LoadTable(path$, Optional encoding As Encodings = Encodings.UTF8, Optional sampling% = 20) As IO.File
        Dim csv%() = New Integer(sampling) {}
        Dim tsv%() = New Integer(sampling) {}
        Dim i%

        For Each line As String In path.IterateAllLines(encoding)
            If i < sampling Then
                csv(i) = line.Count(","c)
                tsv(i) = line.Count(ASCII.TAB)

                i += 1
            Else
                Exit For
            End If
        Next

        If tsv.Average <= csv.Average Then
            Return IO.File.Load(path, encoding.CodePage)
        Else
            Return IO.File.LoadTsv(path, encoding)
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ParseDoc(csv$, Optional removesBlank As Boolean = False) As IO.File
        Return IO.File.Load(csv.lTokens, trimBlanks:=removesBlank)
    End Function
End Module
