#Region "Microsoft.VisualBasic::7dae273b49a2990ff938c8d92c1093fa, Data\DataFrame\Extensions\DocumentExtensions.vb"

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

    '   Total Lines: 496
    '    Code Lines: 298 (60.08%)
    ' Comment Lines: 129 (26.01%)
    '    - Xml Docs: 93.02%
    ' 
    '   Blank Lines: 69 (13.91%)
    '     File Size: 18.43 KB


    ' Module DocumentExtensions
    ' 
    '     Function: Apply, CreateTable, DirectAppends, Distinct, GetColumnObjects
    '               (+4 Overloads) GetColumnValues, GetLastRow, JoinColumns, LoadCsv, LoadData
    '               LoadDictionary, LoadMappings, LoadTable, (+2 Overloads) LoadTsv, Normalization
    '               ParseDoc, (+2 Overloads) SaveAsDataFrame, SaveTsv, TsvLine
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Framework.IO.CSVFile
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports Table = Microsoft.VisualBasic.Data.Framework.IO.File

''' <summary>
''' The csv document extensions API
''' </summary>
Public Module DocumentExtensions

    ''' <summary>
    ''' 将一些奇怪的符号去除
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Normalization(path As String, replaceAs As String) As Table
        Dim Text As String = FileIO.FileSystem.ReadAllText(path)
        Dim Data As String() = Strings.Split(Text, vbCrLf)

        Data = (From strLine As String In Data Select strLine.Replace(vbLf, replaceAs)).ToArray

        Return Table.__LINQ_LOAD(Data)
    End Function

    ''' <summary>
    ''' 去除Csv文件之中的重复记录
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="orderBy">当为本参数指定一个非负数值的时候，程序会按照指定的列值进行排序</param>
    ''' <param name="asc">当进行排序操作的时候，是否按照升序进行排序，否则按照降序排序</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Distinct(file As String, Optional orderBy As Integer = -1, Optional asc As Boolean = True) As Table
        Return Table.Distinct(Table.Load(file), orderBy, asc)
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

    ''' <summary>
    ''' Apply a given value processing on each column of current row data.
    ''' </summary>
    ''' <param name="row"></param>
    ''' <param name="action"></param>
    ''' <param name="skip"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Apply(ByRef row As RowObject, action As Func(Of String, String), Optional skip As Integer = 0) As RowObject
        For i As Integer = skip To row.buffer.Count - 1
            row.buffer(i) = action(row.buffer(i))
        Next

        Return row
    End Function

    <Extension>
    Public Function SaveAsDataFrame(d As IEnumerable(Of Dictionary(Of String, String)), path$) As Boolean
        Dim table As IO.Table() = d _
            .Select(Function(x)
                        Return New IO.Table With {
                            .Properties = x
                        }
                    End Function) _
            .ToArray
        Return table.SaveTo(path)
    End Function

    <Extension>
    Public Function SaveAsDataFrame(d As IEnumerable(Of Dictionary(Of String, Double)), path$) As Boolean
        Return d _
            .Select(Function(x)
                        Return x.ToDictionary(Function(k) k.Key,
                                              Function(v) v.Value.ToString)
                    End Function) _
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
    Public Function DirectAppends(files As IEnumerable(Of String),
                                  EXPORT$,
                                  Optional encoding As Encodings = Encodings.UTF8WithoutBOM,
                                  Optional orderBy As Func(Of Dictionary(Of String, String), Double) = Nothing) As Boolean

        Dim data As New List(Of IO.Table)
        Dim table As IEnumerable(Of IO.Table)

        For Each path$ In files
            ' List(Of T) 对象的 + 语法有冲突，所以在这里需要先进行转换
            table = path.LoadCsv(Of IO.Table)
            data += table
        Next

        If Not orderBy Is Nothing Then
            data = data.OrderBy(Function(r) orderBy(r.Properties)).AsList
        End If

        Return data.SaveTo(EXPORT, encoding:=encoding.CodePage)
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
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function LoadTsv(Of T As Class)(path$,
                                           Optional encoding As Encodings = Encodings.Default,
                                           Optional nameMaps As NameMapping = Nothing,
                                           Optional mute As Boolean = False) As IEnumerable(Of T)
        Return [Imports](Of T)(path,
                               delimiter:=ASCII.TAB,
                               encoding:=encoding.CodePage,
                               nameMaps:=nameMaps,
                               mute:=mute)
    End Function

    ''' <summary>
    ''' Load a .NET collection from a tsv file which is specific by <paramref name="path"/> value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <param name="nameMaps"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function LoadTsv(Of T As Class)(path$, encoding As Encoding, Optional nameMaps As NameMapping = Nothing) As IEnumerable(Of T)
        Dim rowsData = TsvFileIO.LoadFile(path, encoding, skipFirstLine:=False) _
            .Select(Function(r) New RowObject(r)) _
            .ToArray
        Dim file As New IO.File(rowsData)

        If file.RowNumbers = 0 Then
            Return New T() {}
        Else
            Return file.AsDataSource(Of T)(False, maps:=nameMaps)
        End If
    End Function

    <Extension>
    Public Iterator Function LoadMappings(path$, key$, mapTo$) As IEnumerable(Of Map(Of String, String))
        Dim header As RowObject = RowObject.TryParse(path.ReadFirstLine)
        Dim keyIndex% = header.IndexOf(key)
        Dim mapIndex% = header.IndexOf(mapTo)

        Const Missing$ = "Mapping: '{0} => {1}' is missing in the target csv file! ({2})"

        If keyIndex = -1 OrElse mapIndex = -1 Then
            Dim msg$ = String.Format(Missing, key, mapTo, header.ToArray.GetJson)
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
    Public Function GetColumnValues(csv As IO.File, synonyms As String()) As IEnumerable(Of String)
        For Each name As String In synonyms.SafeQuery
            Dim values As IEnumerable(Of String) = csv.GetColumnValues(name)

            If Not values Is Nothing Then
                Return values
            End If
        Next

        Return Nothing
    End Function

    <Extension>
    Public Function GetColumnValues(csv As DataFrameResolver, synonyms As String()) As IEnumerable(Of String)
        For Each name As String In synonyms.SafeQuery
            Dim offset As Integer = csv.GetOrdinal(name)

            If offset < 0 Then
                Continue For
            Else
                Return csv.table.GetColumn(offset)
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' get a specific column value by name
    ''' </summary>
    ''' <param name="csv"></param>
    ''' <param name="column">the column name that will be find in the table header row</param>
    ''' <returns>
    ''' this function will returns nothing if the speicifc <paramref name="column"/> 
    ''' is not exists in the table headers
    ''' </returns>
    <Extension>
    Public Function GetColumnValues(csv As IO.File, column$) As IEnumerable(Of String)
        Dim index As Integer = csv.Headers.IndexOf(column)
        Dim out As New List(Of String)
        Dim offset As Integer = 1

        If index = -1 Then
            Return Nothing
        End If

        For Each r As RowObject In csv.Skip(offset)
            Call out.Add(r(index))
        Next

        Return out
    End Function

    ''' <summary>
    ''' get a specific column value by name
    ''' </summary>
    ''' <param name="csv">
    ''' 20221127 the dataframe object is already skip the title row
    ''' </param>
    ''' <param name="column">the column name that will be find in the table header row</param>
    ''' <returns>
    ''' this function will returns nothing if the speicifc <paramref name="column"/> 
    ''' is not exists in the table headers
    ''' </returns>
    <Extension>
    Public Function GetColumnValues(csv As DataFrameResolver, column$) As IEnumerable(Of String)
        Dim index As Integer = csv.GetOrdinal(column)
        Dim out As New List(Of String)

        If index = -1 Then
            Return Nothing
        End If

        For Each r As RowObject In csv.table
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

    ''' <summary>
    ''' Load csv table from a given file path.
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' A wrapper function of <see cref="IO.File.Load"/>.
    ''' </remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function LoadCsv(path$, Optional encoding As Encodings = Encodings.ASCII) As IO.File
        Return IO.File.Load(path, encoding.CodePage)
    End Function

    ''' <summary>
    ''' Default using <see cref="Scripting.CTypeDynamic(Of T)(String, T)"/> 
    ''' scripting type casting helper.
    ''' (从一个csv文件数据之中加载一个字典对象，必须要保证第一列是键名，第二列
    ''' 才是数据，文件不存在或者内容为空的时候，会返回空字典)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path$"></param>
    ''' <param name="parser">值<typeparamref name="T"/>的字符串解析函数</param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadDictionary(Of T)(path$, Optional parser As IStringParser(Of T) = Nothing) As Dictionary(Of String, T)
        If path.FileLength <= 0 Then
            Return New Dictionary(Of String, T)
        End If

        With parser Or Scripting.DefaultTextParser(Of T)
            Dim table As New Dictionary(Of String, T)
            Dim key$
            Dim value$

            ' 需要跳过第一行标题行
            For Each line As String In path.IterateAllLines.Skip(1)
                With Tokenizer.CharsParser(line)
                    ' 2018-4-8
                    ' 因为line里面可能会存在双引号，所以不可以直接使用mid函数来进行取值
                    key$ = .First
                    value$ = .ElementAtOrDefault(1)
                End With

                ' parsing string value to object
                table(key) = .ByRef(value)
            Next

            Return table
        End With
    End Function

    <Extension>
    Public Function CreateTable(Of T)(csv As IO.File, Optional parser As IStringParser(Of T) = Nothing) As Dictionary(Of String, T)
        With parser Or Scripting.DefaultTextParser(Of T)
            Dim table As New Dictionary(Of String, T)

            For Each line As RowObject In csv.Skip(1)
                Dim key$ = line.First
                Dim value = line.Second

                table(key) = .ByRef(value)
            Next

            Return table
        End With
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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="csv">the csv document text</param>
    ''' <param name="removesBlank">
    ''' removes blank row which is all content is looks like ``,,,,,,``?
    ''' </param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ParseDoc(csv$, Optional removesBlank As Boolean = False) As IO.File
        Return FileLoader.Load(csv.LineTokens, trimBlanks:=removesBlank)
    End Function

    ''' <summary>
    ''' 获取最后一行数据
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetLastRow(Of T As Class)(path$,
                                              Optional encoding As Encodings = Encodings.UTF8,
                                              Optional strict As Boolean = False,
                                              Optional silent As Boolean = False) As T

        Dim textEncoding As Encoding = encoding.CodePage
        Dim header As RowObject = RowObject.TryParse(path.ReadFirstLine(textEncoding))
        Dim data As RowObject = RowObject.TryParse(path.GetLastLine(textEncoding))
        Dim subFrame As DataFrameResolver = DataFrameResolver.CreateObject({header, data})
        Dim buffer = Reflector.Convert(Of T)(subFrame, strict, silent:=silent)

        Return buffer.First
    End Function
End Module
