#Region "Microsoft.VisualBasic::7864fa3284719eb34d0f47c9967b508e, Data\DataFrame\Extensions\Extensions.vb"

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

    '   Total Lines: 659
    '    Code Lines: 375 (56.90%)
    ' Comment Lines: 210 (31.87%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 74 (11.23%)
    '     File Size: 28.41 KB


    ' Module Extensions
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: (+4 Overloads) AsDataSource, AsLinq, (+5 Overloads) DataFrame, GetLocusMapName, IsEmptyTable
    '               (+3 Overloads) LoadCsv, LoadDataFrame, LoadDblVector, LoadStream, LoadTsv
    '               SaveDataSet, (+2 Overloads) SaveTable, (+6 Overloads) SaveTo, TabExport, ToCsvDoc
    ' 
    '     Sub: Cable, ForEach
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.IO.Linq
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Text
Imports any = Microsoft.VisualBasic.Scripting
Imports File_csv = Microsoft.VisualBasic.Data.csv.IO.File

''' <summary>
''' The shortcuts operation for the common csv document operations.
''' </summary>
''' <remarks></remarks>
'''
<HideModuleName>
Public Module Extensions

    Sub New()
        Call initStreamIOHandlers()
    End Sub

    <Extension>
    Public Function LoadDataFrame(path As String, Optional encoding As Encoding = Nothing) As DATA.DataFrame
        Return New DATA.DataFrame(EntityObject.LoadDataSet(path, encoding:=encoding))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function LoadCsv(Of T As Class)(path As DefaultString) As List(Of T)
        Return path.DefaultValue.LoadCsv(Of T)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function LoadTsv(Of T As Class)(path As DefaultString, Optional encoding As Encoding = Nothing) As IEnumerable(Of T)
        Return path.DefaultValue.LoadTsv(Of T)(encoding)
    End Function

    ''' <summary>
    ''' 判断这个表格文件是否是空的？
    ''' 
    ''' > 只包含有标题行，文件不存在，文件为空等都会被判断为空
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    <Extension>
    Public Function IsEmptyTable(path As String) As Boolean
        If path.FileLength <= 0 Then
            Return True
        Else
            ' This table file only contains header line
            ' is also an empty table file.
            Return path.IterateAllLines.Take(2).Count <= 1
        End If
    End Function

    ''' <summary>
    ''' Anonymous type data reader helper.(System.MissingMethodException occurred
    '''  HResult=0x80131513
    '''  Message=No parameterless constructor defined for this object.
    '''  Source=mscorlib
    '''  StackTrace:
    '''   at System.RuntimeTypeHandle.CreateInstance(RuntimeType type, Boolean publicOnly, Boolean noCheck, Boolean&amp; canBeCached, RuntimeMethodHandleInternal&amp; ctor, Boolean&amp; bNeedSecurityCheck)
    '''   at System.RuntimeType.CreateInstanceSlow(Boolean publicOnly, Boolean skipCheckThis, Boolean fillCache, StackCrawlMark&amp; stackMark)
    '''   at System.Activator.CreateInstance(Type type, Boolean nonPublic)
    '''   at System.Activator.CreateInstance(Type type)
    '''   at Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Reflector._Closure$__1-0._Lambda$__0(SeqValue`1 line) In G:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\StorageProvider\Reflection\StorageProviders\Reflection.vb:line 96
    '''   at System.Linq.Parallel.SelectQueryOperator`2.SelectQueryOperatorEnumerator`1.MoveNext(TOutput&amp; currentElement, TKey&amp; currentKey)
    '''   at System.Linq.Parallel.PipelineSpoolingTask`2.SpoolingWork()
    '''   at System.Linq.Parallel.SpoolingTaskBase.Work()
    ''')对于匿名类型，这个方法还无法正常工作
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <param name="template"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadCsv(Of T As Class)(path$, template As T, Optional encoding As Encodings = Encodings.UTF8) As T()
        Return path.LoadCsv(Of T)(encoding:=encoding.CodePage).ToArray
    End Function

    ''' <summary>
    ''' Save all of the <see cref="Dictionary(Of T).Values"/> into a csv file.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="table"></param>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SaveTo(Of T As INamedValue)(table As Dictionary(Of T), path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Return table.Values.SaveTo(path, encoding)
    End Function

    ''' <summary>
    ''' Save variable value vector as data frame
    ''' </summary>
    ''' <param name="samples"></param>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <param name="xlabels#"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SaveTo(samples As IEnumerable(Of NamedValue(Of Double())),
                           path$,
                           Optional encoding As Encodings = Encodings.ASCII,
                           Optional xlabels#() = Nothing) As Boolean
        Dim out As New IO.File
        Dim data As NamedValue(Of Double())() = samples.ToArray

        If xlabels.IsNullOrEmpty Then
            out += New RowObject(data.Select(Function(s) s.Name))

            For i As Integer = 0 To data(Scan0).Value.Length - 1
                Dim row As New RowObject

                For Each sample In data
                    row.Add(CStr(sample.Value(i)))
                Next

                out += row
            Next
        Else
            out += New RowObject("X".Join(data.Select(Function(s) s.Name)))

            For i As Integer = 0 To data(Scan0).Value.Length - 1
                Dim row As New RowObject From {
                    CStr(xlabels(i))
                }
                For Each sample As NamedValue(Of Double()) In data
                    Call row.Add(CStr(sample.Value(i)))
                Next

                out += row
            Next
        End If

        Return out.Save(path, encoding)
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="path">Csv file path</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetLocusMapName(path As String) As String
        Return File.ReadHeaderRow(path).FirstOrDefault
    End Function

    <Extension>
    Public Function TabExport(Of T As Class)(source As IEnumerable(Of T), saveTo$,
                                             Optional noTitle As Boolean = False,
                                             Optional encoding As Encodings = Encodings.UTF8) As Boolean

        Dim doc As File = Reflector.Save(source, False)
        Dim lines As RowObject() = If(noTitle, doc.Skip(1).ToArray, doc.ToArray)
        Dim slines As String() = lines.Select(Function(x) x.AsLine(ASCII.TAB)).ToArray
        Dim sdoc As String = String.Join(vbCrLf, slines)
        Return sdoc.SaveTo(saveTo, encoding.CodePage)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub ForEach(Of T As Class)(path As String, invoke As Action(Of T))
        Call DataStream.OpenHandle(path).ForEach(Of T)(invoke)
    End Sub

    ''' <summary>
    ''' As query source for the LINQ or PLINQ, this function is much save time for the large data set query!
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsLinq(Of T As Class)(path$, Optional parallel As Boolean = False) As IEnumerable(Of T)
        Return DataLinqStream.OpenHandle(path).AsLinq(Of T)(parallel)
    End Function

    ''' <summary>
    ''' Convert a database table into a dynamics dataframe in VisualBasic.(将数据库之中所读取出来的数据表转换为表格对象)
    ''' </summary>
    ''' <param name="reader"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <Extension>
    Public Function DataFrame(reader As DbDataReader) As DataFrame
        Dim csv As New IO.File
        Dim fields As Integer() = reader.FieldCount.Sequence.ToArray

        csv += From i As Integer
               In fields
               Select reader.GetName(i)

        Do While reader.Read
            csv += From i As Integer
                   In fields
                   Let val As Object = reader.GetValue(i)
                   Select s = Scripting.ToString(val)
        Loop

        Return DataFrame.CreateObject(csv)
    End Function

    <Extension>
    Public Function DataFrame(table As DataTable) As DataFrame
        Dim headers As New List(Of String)
        Dim rows As New List(Of RowObject)
        Dim rowObj As DataRow
        Dim rowData As New List(Of String)

        For i As Integer = 0 To table.Columns.Count - 1
            headers.Add(table.Columns(i).ColumnName)
        Next

        For i As Integer = 0 To table.Rows.Count - 1
            rowObj = table.Rows.Item(i)
            rowData = New List(Of String)

            For Each c As Object In rowObj.ItemArray
                rowData.Add(any.ToString(c))
            Next

            rows.Add(New RowObject(rowData))
        Next

        Return DataFrame.CreateObject(headers, rows)
    End Function

    ''' <summary>
    ''' Convert the dictionary table collection as the <see cref="EntityObject"/> collection.
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function DataFrame(source As IEnumerable(Of NamedValue(Of Dictionary(Of String, String)))) As EntityObject()
        Return source _
            .Select(Function(o)
                        Return New EntityObject With {
                            .ID = o.Name,
                            .Properties = o.Value
                        }
                    End Function) _
            .ToArray
    End Function

    <Extension>
    Public Function DataFrame(Of T)(source As IEnumerable(Of T)) As EntityObject()
        Return IO.DataFrame.CreateObject(source.ToCsvDoc).AsDataSource(Of EntityObject)(False).ToArray
    End Function

    <Extension>
    Public Function SaveTable(table As IEnumerable(Of KeyValuePair(Of String, Double)), path$, Optional encoding As Encoding = Nothing) As Boolean
        Dim csv As New File_csv

        csv += {"ID", "value"}
        csv += table _
            .Select(Function(map As KeyValuePair(Of String, Double))
                        Return New RowObject(New String() {map.Key, map.Value.ToString})
                    End Function)

        Return csv.Save(path, encoding)
    End Function

    ''' <summary>
    ''' This extension is using for .NET scripting API.
    ''' (这个函数不会被申明为拓展函数了，因为这个object序列类型的函数如果为拓展函数的话，会与T泛型函数产生冲突)
    ''' </summary>
    ''' <param name="data">A generic .NET collection, using for scripting API.</param>
    ''' <param name="path$">The file path of the csv file for saved.</param>
    ''' <param name="encoding">Default is utf-8 without BOM</param>
    ''' <param name="type">
    ''' If this <see cref="Type"/> information provider is nothing, then the function will peeks of the first sevral element for the type information.
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function SaveTable(data As IEnumerable, path$,
                              Optional encoding As Encoding = Nothing,
                              Optional type As Type = Nothing,
                              Optional silent As Boolean = False) As Boolean

        If type Is Nothing Then
            For Each x As Object In data
                If Not x Is Nothing Then
                    type = x.GetType
                    Exit For
                End If
            Next
        End If

        Return Reflector _
            .doSave(objSource:=data,
                    typeDef:=type,
                    strict:=False,
                    schemaOut:=Nothing) _
            .SaveDataFrame(
                path:=path,
                encoding:=encoding,
                silent:=silent
             )
    End Function

    ''' <summary>
    ''' Export the given data set as csv file
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SaveTo(data As IEnumerable(Of DynamicObjectLoader), path$, Optional encoding As Encoding = Nothing) As Boolean
        Dim headers As Dictionary(Of String, Integer) = data.First.Schema
        Dim LQuery = LinqAPI.Exec(Of RowObject) _
                                                _
            () <= From x As DynamicObjectLoader
                  In data
                  Let content = (From p In headers Select x.GetValue(p.Value)) '
                  Select New RowObject(content)

        With New IO.File
            Call .AppendLine(From p In headers Select p.Key)
            Call .AppendRange(LQuery)

            Return .Save(path, encoding)
        End With
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SaveTo(dat As IEnumerable(Of RowObject), path$, Optional encoding As Encoding = Nothing) As Boolean
        Return CType(dat, IO.File).Save(path, Encoding:=encoding)
    End Function

    ''' <summary>
    ''' Create a dynamics data frame object from a csv document object.
    ''' (从Csv文件之中创建一个数据框容器)
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function DataFrame(data As File) As DataFrame
        Return DataFrame.CreateObject(data)
    End Function

    ''' <summary>
    ''' Convert the csv data file to a type specific collection.(将目标Csv文件转换为特定类型的集合数据)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="dataSet"></param>
    ''' <param name="strict">
    ''' If this parameter is true, which means if the property in target class <typeparamref name="T"/> is not marked
    ''' <see cref="Column"/> attribute, then this property will be ignored.
    ''' </param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function AsDataSource(Of T As Class)(dataSet As File_csv,
                                                Optional strict As Boolean = False,
                                                Optional skipEmpty As Boolean = True,
                                                Optional maps As Dictionary(Of String, String) = Nothing,
                                                Optional silent As Boolean = False) As IEnumerable(Of T)
        Dim sheet As File_csv

        If skipEmpty Then
            sheet = dataSet _
                .Where(Function(row)
                           Return Not row Is Nothing AndAlso row.NumbersOfColumn > 0
                       End Function) _
                .ToArray
        Else
            sheet = dataSet
        End If

        Return IO.DataFrame _
            .CreateObject(file:=sheet) _
            .AsDataSource(Of T)(strict, maps, silent:=silent)
    End Function

    ''' <summary>
    ''' Convert the csv data file to a type specific collection.(将目标Csv文件转换为特定类型的集合数据)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="dataframe"></param>
    ''' <param name="explicit"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function AsDataSource(Of T As Class)(dataframe As DataFrame,
                                                Optional explicit As Boolean = False,
                                                Optional maps As Dictionary(Of String, String) = Nothing,
                                                Optional silent As Boolean = False) As IEnumerable(Of T)
        With dataframe
            If Not maps Is Nothing Then
                Call .ChangeMapping(maps)
            End If

            Return .DoCall(Function(table) Reflector.Convert(Of T)(table, explicit, silent:=silent))
        End With
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="importsFile">The file path of the text doucment which will be imports as a csv document.</param>
    ''' <param name="Delimiter">The delimiter to parsing a row in the csv document.</param>
    ''' <param name="explicit"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function AsDataSource(Of T As Class)(importsFile$,
                                                Optional delimiter$ = ",",
                                                Optional explicit As Boolean = True,
                                                Optional silent As Boolean = False) As T()

        Dim df As DataFrame = IO.DataFrame.CreateObject([Imports](importsFile, delimiter))
        Dim data As T() = Reflector.Convert(Of T)(df, explicit, silent:=silent).ToArray

        Return data
    End Function

    ''' <summary>
    ''' Convert the string collection as the type specific collection, please make sure the first element
    ''' in this collection is stands for the title row.
    ''' (将字符串数组转换为数据源对象，注意：请确保第一行为标题行)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="lines"></param>
    ''' <param name="Delimiter"></param>
    ''' <param name="strict"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function AsDataSource(Of T As Class)(lines As IEnumerable(Of String),
                                                Optional delimiter$ = ",",
                                                Optional strict As Boolean = True) As IEnumerable(Of T)

        Dim splitter As String = String.Format(DataImports.SplitRegxExpression, delimiter)
        Dim rows As IEnumerable(Of RowObject) = From line As String
                                                In lines
                                                Select RowParsing(line, splitter)
        ' 解析完文本数据之后进行对象的反射加载操作
        Return New File_csv(rows).AsDataSource(Of T)(strict)
    End Function

    ''' <summary>
    ''' Load a csv data file document using a specific object type.
    ''' </summary>
    ''' <typeparam name="T">The type parameter of the element in the returns collection data.</typeparam>
    ''' <param name="path">The csv document file path.(目标Csv数据文件的文件路径)</param>
    ''' <param name="explicit"></param>
    ''' <param name="encoding"></param>
    ''' <param name="maps">``Csv.Field -> <see cref="PropertyInfo.Name"/>``</param>
    ''' <param name="skipWhile">
    ''' [fieldName => test] 如果目标csv文件数据集非常大的话，可以使用这个函数来过滤掉一些数据行，以减少数据量
    ''' </param>
    ''' <returns>a collection of the <typeparamref name="T"/>.</returns>
    ''' <remarks>
    ''' (将某一个Csv数据文件加载仅一个特定类型的对象集合中，空文件的话会返回一个空集合，这是一个安全的函数，不会返回空值)
    ''' </remarks>
    <Extension>
    Public Function LoadCsv(Of T As Class)(path$,
                                           Optional explicit As Boolean = False,
                                           Optional encoding As Encoding = Nothing,
                                           Optional fast As Boolean = False,
                                           Optional maps As NameMapping = Nothing,
                                           Optional mute As Boolean = False,
                                           Optional metaBlank As String = "",
                                           Optional simpleRowIterators As Boolean = True,
                                           Optional skipWhile As NamedValue(Of Func(Of String, Boolean)) = Nothing) As List(Of T)
        Dim buffer As List(Of T)
        Dim fs$, ms&

        Call "Start to load csv data....".__DEBUG_ECHO(mute:=mute)

        With Stopwatch.StartNew
            buffer = Reflector.Load(Of T)(
                path, explicit, encoding,
                fast:=fast,
                maps:=maps,
                mute:=mute,
                metaBlank:=metaBlank,
                skipWhile:=skipWhile,
                simpleRowIterators:=simpleRowIterators
            ).AsList
            ms = .ElapsedMilliseconds
            fs = If(ms > 1000, (ms / 1000) & "sec", ms & "ms")
        End With

        Dim type$ = GetType(T).FullName
        Dim n% = buffer.Count

        Call $"[CSV.Reflector::{type}]{vbLf}Load {n} lines of data from ""{path.ToFileURL}""! ...................{fs}".__DEBUG_ECHO(mute:=mute)

        Return buffer
    End Function

    ''' <summary>
    ''' Load object data set from the text lines stream.(从文本行之中加载数据集)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="explicit">
    ''' 列名称隐式解析，即不强制要求属性上面有<see cref="ColumnAttribute"/>标记，默认是，
    ''' 否则只解析出带有<see cref="ColumnAttribute"/>自定义属性标记的属性作为csv的列的
    ''' 数据源
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadStream(Of T As Class)(source As IEnumerable(Of String),
                                              Optional explicit As Boolean = True,
                                              Optional trimBlanks As Boolean = False,
                                              Optional isTsv As Boolean = False) As IEnumerable(Of T)

        Return FileLoader.Load(source.ToArray, trimBlanks, isTsv:=isTsv) _
            .DoCall(Function(rs) New File(rs)) _
            .AsDataSource(Of T)(Not explicit)
    End Function

    ''' <summary>
    ''' 如果直接使用<see cref="SaveTo"/>函数来保存数据集的话，可能列的顺序是被打乱的，
    ''' 则下次加载的时候<see cref="EntityObject.ID"/>列可能就不是第一列了，会出错，
    ''' 故而需要使用这个专门的函数来进行数据集的保存操作
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <param name="KeyMap">将<see cref="EntityObject.ID"/>重命名为这个参数的值，假若这个参数值不是空字符串的话</param>
    ''' <param name="blank"></param>
    ''' <param name="reorderKeys"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 对于<see cref="DataSet"/>类型的数据集，可以先使用拓展函数转化为<see cref="EntityObject"/>之后再调用本函数进行保存操作
    ''' </remarks>
    <Extension>
    Public Function SaveDataSet(Of T As EntityObject)(source As IEnumerable(Of T),
                                                      path$,
                                                      Optional encoding As Encodings = Encodings.UTF8,
                                                      Optional KeyMap$ = Nothing,
                                                      Optional blank$ = "",
                                                      Optional reorderKeys As Integer = 0,
                                                      Optional transpose As Boolean = False,
                                                      Optional silent As Boolean = True) As Boolean

        Dim modify As Dictionary(Of String, String) = Nothing
        Dim layout As New Dictionary(Of String, Integer) From {
            {NameOf(EntityObject.ID), -10000}
        }

        If Not KeyMap.StringEmpty Then
            modify = New Dictionary(Of String, String) From {
                {NameOf(EntityObject.ID), KeyMap}
            }
            layout.Clear()
            layout.Add(KeyMap, -10000)
        End If

        Return source.SaveTo(path, , encoding.CodePage, blank,, modify, reorderKeys, layout, transpose:=transpose, silent:=silent)
    End Function

    <Extension>
    Public Function SaveTo(Of T)(source As IEnumerable(Of T),
                                 path As String,
                                 encoding As Encodings,
                                 Optional explicit As Boolean = False) As Boolean

        Return source.SaveTo(path, explicit, encoding.CodePage)
    End Function

    ''' <summary>
    ''' Generate a csv document from a object collection.
    ''' (从一个特定类型的数据集合之中生成一个Csv文件，非并行化的以保持数据原有的顺序)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="strict">默认导出所有的可用属性</param>
    ''' <param name="metaBlank">对于字典对象之中，空缺下来的域键名的值使用什么来替代？默认使用空白字符串</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function ToCsvDoc(Of T)(source As IEnumerable(Of T),
                                   Optional strict As Boolean = False,
                                   Optional maps As Dictionary(Of String, String) = Nothing,
                                   Optional metaBlank$ = "",
                                   Optional reorderKeys% = 0,
                                   Optional numFormat$ = Nothing) As File
        Return Reflector.Save(
            source, strict,
            maps:=maps,
            metaBlank:=metaBlank,
            reorderKeys:=reorderKeys,
            numFormat:=numFormat
        )
    End Function

    ''' <summary>
    ''' Save the data collection vector as a csv document.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <Extension>
    Public Function SaveTo(data As IEnumerable(Of Double), path$, Optional encoding As Encodings = Encodings.ASCII) As Boolean
        Dim row As IEnumerable(Of String) = From n As Double
                                            In data
                                            Select s = n.ToString

        Dim buf As New IO.File({New RowObject(row)})
        Return buf.Save(path, encoding.CodePage)
    End Function

    ''' <summary>
    ''' Load the data from the csv document as a double data type vector.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <Extension>
    Public Function LoadDblVector(path As String) As Double()
        Dim buf As IO.File = IO.File.Load(path)
        Dim FirstRow As RowObject = buf.First
        Dim data As Double() = FirstRow.Select(AddressOf Val).ToArray
        Return data
    End Function

    <Extension>
    Public Sub Cable(Of T)(method As LoadObject(Of T))
        Dim type As Type = GetType(T)
        Dim name As String = type.FullName
        Dim helper As LoadObject = AddressOf method.Invoke

        Call CapabilityPromise(name, type, helper)
    End Sub
End Module
