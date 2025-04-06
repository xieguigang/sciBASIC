#Region "Microsoft.VisualBasic::29d14793a578c9a9778ce5e3ed9432ce, Microsoft.VisualBasic.Core\src\Extensions\Extensions.vb"

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

    '   Total Lines: 1550
    '    Code Lines: 855 (55.16%)
    ' Comment Lines: 553 (35.68%)
    '    - Xml Docs: 91.86%
    ' 
    '   Blank Lines: 142 (9.16%)
    '     File Size: 57.48 KB


    ' Module Extensions
    ' 
    ' 
    ' Module Extensions
    ' 
    '     Function: [Set], Add, (+5 Overloads) AddRange, AsRange, (+2 Overloads) Average
    '               CheckDuplicated, Constrain, DateToString, DescriptionValue, DriverRun
    '               FuzzyMatching, IndexOf, (+2 Overloads) InlineCopy, InsertOrUpdate, Invoke
    '               InvokeSet, is_empty, Is_NA_UHandle, (+2 Overloads) IsNaNImaginary, (+2 Overloads) JoinBy
    '               (+2 Overloads) LongSeq, MatrixToUltraLargeVector, MatrixTranspose, MatrixTransposeIgnoredDimensionAgreement, MD5
    '               ModifyValue, (+2 Overloads) Offset, Range, Remove, RemoveDuplicates
    '               RemoveFirst, (+2 Overloads) RemoveLast, Second, SeqRandom, (+3 Overloads) Sequence
    '               (+11 Overloads) ShadowCopy, Shell, Shuffles, Slice, (+2 Overloads) SplitMV
    '               Sum, (+2 Overloads) ToArray, ToBoolean, ToDictionary, ToNormalizedPathString
    '               ToString, ToStringArray, ToVector, (+3 Overloads) TrimNull, TryCount
    '               Unlist, WriteAddress
    ' 
    '     Sub: Add, Removes, (+2 Overloads) Swap, SwapItem
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.[Default]
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.SecurityString
Imports Microsoft.VisualBasic.Text.Similarity
Imports any = Microsoft.VisualBasic.Scripting

#If DEBUG Then
Imports Microsoft.VisualBasic.Serialization.JSON
#End If

#Const FRAMEWORD_CORE = 1
#Const Yes = 1

#If FRAMEWORD_CORE = Yes Then

''' <summary>
''' Common extension methods library for convenient the programming job.
''' </summary>
''' <remarks></remarks>
<Package("Framework.Extensions",
                    Description:="The common extension methods module in this Microsoft.VisualBasic program assembly." &
                                 "Common extension methods library for convenient the programming job.",
                    Publisher:="xie.guigang@gmail.com",
                    Revision:=8655,
                    Url:="http://github.com/xieguigang/sciBASIC#")>
<HideModuleName>
<Extension> Public Module Extensions
#Else

''' <summary>
''' Common extension methods library for convenient the programming job.
''' </summary>
''' <remarks></remarks>
Public Module Extensions
#End If

    ''' <summary>
    ''' check of the given object is nothing or value is empty?
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    <Extension>
    Public Function is_empty(obj As IsEmpty) As Boolean
        If obj Is Nothing Then
            Return True
        Else
            Return obj.IsEmpty
        End If
    End Function

    ''' <summary>
    ''' get description text value from <see cref="DescriptionAttribute"/>.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this function is used for get the description text value from the GetType value <see cref="Type"/> of 
    ''' the given <paramref name="obj"/>. this function may not working for the reflection result value, 
    ''' example as when <paramref name="obj"/> is <see cref="PropertyInfo"/>, <see cref="MethodInfo"/> or 
    ''' something, due to the reason of gettype of these reflection object contains no description data in 
    ''' .net framework.
    ''' 
    ''' 20240414 change the function name from ``Description`` to ``DescriptionValue`` due to the reason of
    ''' the function name of Description will always overloads other extension method which is named Description.
    ''' </remarks>
    <Extension>
    Public Function DescriptionValue(Of T As Class)(obj As T) As String
        If Not obj Is Nothing Then
            Dim desc As DescriptionAttribute = obj.GetType.GetCustomAttribute(GetType(DescriptionAttribute))

            If desc Is Nothing Then
                Return Nothing
            Else
                Return desc.Description
            End If
        End If

        Return Nothing
    End Function

    <Extension>
    Public Function Sum(Of T)(v As IEnumerable(Of T), aggregate As Func(Of T, Integer, Double)) As Double
        Dim total As Double = 0
        Dim i As Integer = 0

        For Each xi As T In v
            total += aggregate(xi, i)
            i += 1
        Next

        Return total
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ToString(ms As MemoryStream, encoding As Encoding) As String
        Return encoding.GetString(ms.ToArray, Scan0, ms.Length)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Average(range As DoubleRange) As Double
        Return (range.Min + range.Max) / 2
    End Function

    ''' <summary>
    ''' Create the numeric range from a numeric value collection
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Range(data As IEnumerable(Of Double), Optional scale# = 1) As DoubleRange
        Return New DoubleRange(data) * scale
    End Function

    ''' <summary>
    ''' 将目标值域切割为等长递增的<paramref name="n"/>个值域
    ''' </summary>
    ''' <param name="range"></param>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Slice(range As DoubleRange, n%) As IEnumerable(Of DoubleRange)
        Dim l = range.Length
        Dim d = l / n
        Dim parts = Math.seq(range.Min, range.Max, by:=d) _
                        .SlideWindows(winSize:=2) _
                        .Select(Function(w)
                                    Return New DoubleRange(w)
                                End Function) _
                        .ToArray
        Return parts
    End Function

    <Extension>
    Public Function Average(data As IEnumerable(Of TimeSpan)) As TimeSpan
        Dim avg# = data.Select(Function(x) x.TotalMilliseconds).Average
        Return TimeSpan.FromMilliseconds(avg)
    End Function

    ''' <summary>
    ''' Get target string's md5 hash code
    ''' </summary>
    ''' <param name="s">
    ''' any text data
    ''' </param>
    ''' <returns>
    ''' a 32 bit md5 string in lower case
    ''' </returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function MD5(s$) As String
        Return s.GetMd5Hash.ToLower
    End Function

    ''' <summary>
    ''' Returns the second element in the source collection, if the collection 
    ''' is nothing or elements count not enough, then will returns nothing if 
    ''' the <paramref name="suppressError"/> option was opend, otherwise this 
    ''' function will throw exception.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <Extension> Public Function Second(Of T)(source As IEnumerable(Of T), Optional suppressError As Boolean = False, Optional [default] As T = Nothing) As T
        For Each x As SeqValue(Of T) In source.SeqIterator
            If x.i = 1 Then
                Return x.value
            End If
        Next

        If Not suppressError Then
            Throw New IndexOutOfRangeException
        Else
            Return [default]
        End If
    End Function

    <Extension>
    Public Function Add(Of T As INamedValue)(ByRef table As Dictionary(Of String, T), obj As T) As Dictionary(Of String, T)
        If table Is Nothing Then
            table = New Dictionary(Of String, T)
        End If
        If table.ContainsKey(obj.Key) Then
            Throw New Exception($"[{obj.Key}] was duplicated in the dictionary!")
        Else
            Call table.Add(obj.Key, obj)
        End If

        Return table
    End Function

    <Extension>
    Public Function IndexOf(Of T)(source As Queue(Of T), x As T) As Integer
        If source.IsNullOrEmpty Then
            Return -1
        Else
            Return source.AsList.IndexOf(x)
        End If
    End Function

    ''' <summary>
    ''' Adds the elements of the specified collection to the end of the List`1.
    ''' (会自动跳过空集合，这个方法是安全的)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <param name="value">The collection whose elements should be added to the end of the List`1.</param>
    <Extension> Public Sub Add(Of T)(ByRef list As List(Of T), ParamArray value As T())
        If value.IsNullOrEmpty Then
            Return
        Else
            Call list.AddRange(value)
        End If
    End Sub

    <Extension> Public Function [Set](Of T)(ByRef array As T(), index As Integer, value As T) As T()
        If index < 0 Then
            Return array
        End If

        If array.Length - 1 >= index Then
            array(index) = value
        Else
            Dim copy As T() = New T(index) {}
            Call System.Array.ConstrainedCopy(array, Scan0, copy, Scan0, array.Length)
            copy(index) = value
            array = copy
        End If

        Return array
    End Function

    ''' <summary>
    ''' Constrain the inherits class type into the base type.
    ''' (基类集合与继承类的集合约束)
    ''' </summary>
    ''' <typeparam name="T">继承类向基类进行约束</typeparam>
    ''' <typeparam name="Tbase">基类</typeparam>
    ''' <returns></returns>
    Public Function Constrain(Of Tbase As Class, T As Tbase)(source As IEnumerable(Of T)) As Tbase()
        If source Is Nothing Then
            Return New Tbase() {}
        End If

        Dim array As T() = source.ToArray
        Dim out As Tbase() = New Tbase(array.Length - 1) {}

        For i As Integer = 0 To out.Length - 1
            out(i) = source(i)
        Next

        Return out
    End Function

    ''' <summary>
    ''' 0 -> False
    ''' 1 -> True
    ''' </summary>
    ''' <param name="b"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ToBoolean(b As Long) As Boolean
        If b = 0 Then
            Return False
        Else
            Return True
        End If
    End Function

    <Extension>
    Public Function AddRange(Of T)(ByRef table As Dictionary(Of String, T),
                                   data As IEnumerable(Of T),
                                   key As Func(Of T, String),
                                   Optional replaceDuplicated As Boolean = False) As Dictionary(Of String, T)
        If data Is Nothing Then
            Return table
        ElseIf replaceDuplicated Then
            For Each obj In data
                table(key(obj)) = obj
            Next
        Else
            For Each obj In data
                table.Add(key(obj), obj)
            Next
        End If

        Return table
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="table"></param>
    ''' <param name="data"></param>
    ''' <param name="replaceDuplicated">
    ''' 这个函数参数决定了在遇到重复的键名称的时候的处理方法：
    ''' 
    ''' + 如果为真，则默认会用新的值来替换旧的值
    ''' + 如果为False，则遇到重复的键名的时候会报错
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function AddRange(Of T)(ByRef table As Dictionary(Of String, T),
                                   data As IEnumerable(Of NamedValue(Of T)),
                                   Optional replaceDuplicated As Boolean = False) As Dictionary(Of String, T)
        If data Is Nothing Then
            Return table
        ElseIf replaceDuplicated Then
            For Each obj In data
                table(obj.Name) = obj.Value
            Next
        Else
            For Each obj In data
                table.Add(obj.Name, obj.Value)
            Next
        End If

        Return table
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="table"></param>
    ''' <param name="data"></param>
    ''' <param name="replaceDuplicated">
    ''' 这个函数参数决定了在遇到重复的键名称的时候的处理方法：
    ''' 
    ''' + 如果为真，则默认会用新的值来替换旧的值
    ''' + 如果为False，则遇到重复的键名的时候会报错
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function AddRange(Of TKey, TValue)(ByRef table As Dictionary(Of TKey, TValue),
                                              data As IEnumerable(Of KeyValuePair(Of TKey, TValue)),
                                              Optional replaceDuplicated As Boolean = False) As Dictionary(Of TKey, TValue)
        If data Is Nothing Then
            Return table
        ElseIf replaceDuplicated Then
            For Each obj In data
                table(obj.Key) = obj.Value
            Next
        Else
            For Each obj In data
                table.Add(obj.Key, obj.Value)
            Next
        End If

        Return table
    End Function

    ''' <summary>
    ''' Format the datetime value in the format of yy/mm/dd hh:min
    ''' </summary>
    ''' <param name="dat"></param>
    ''' <returns></returns>
    <Extension>
    Public Function DateToString(dat As Date) As String
        Dim yy = dat.Year
        Dim mm As String = dat.Month.FormatZero
        Dim dd As String = dat.Day.FormatZero
        Dim hh As String = dat.Hour.FormatZero
        Dim mmin As String = dat.Minute.FormatZero

        Return $"{yy}/{mm}/{dd} {hh}:{mmin}"
    End Function

    <ExportAPI("Date.ToNormalizedPathString")>
    <Extension>
    Public Function ToNormalizedPathString(dat As Date) As String
        Dim yy = dat.Year
        Dim mm As String = dat.Month.FormatZero
        Dim dd As String = dat.Day.FormatZero
        Dim hh As String = dat.Hour.FormatZero
        Dim mmin As String = dat.Minute.FormatZero
        Dim sec As String = dat.Second.FormatZero

        Return String.Format("{0}-{1}-{2} {3}.{4}.{5}", yy, mm, dd, hh, mmin, sec)
    End Function

    ''' <summary>
    ''' Join the string tokens with a given delimiter text.
    ''' </summary>
    ''' <param name="tokens">parts of the string tokens for make join</param>
    ''' <param name="delimiter">
    ''' the delimiter for make string join.
    ''' </param>
    ''' <returns>
    ''' This is a safe function: if the source string collection is nothing, 
    ''' then whistle function will returns a empty string instead of throw 
    ''' exception.
    ''' 
    ''' (a safe wrapper of <see cref="System.String.Join"/>)
    ''' </returns>
    ''' <remarks>
    ''' 这是一个安全的函数，当数组为空的时候回返回空字符串
    ''' </remarks>
    <DebuggerStepThrough>
    <Extension>
    Public Function JoinBy(tokens As IEnumerable(Of String), delimiter$) As String
        If tokens Is Nothing Then
            Return ""
        End If
        Return String.Join(If(delimiter, ""), tokens.ToArray)
    End Function

    ''' <summary>
    ''' <see cref="String.Join"/>，这是一个安全的函数，当数组为空的时候回返回空字符串
    ''' </summary>
    ''' <param name="values"></param>
    ''' <param name="delimiter"></param>
    ''' <returns></returns>
    <Extension> Public Function JoinBy(values As IEnumerable(Of Integer), delimiter$) As String
        If values Is Nothing Then
            Return ""
        End If
        Return String.Join(delimiter, values.Select(Function(n) CStr(n)).ToArray)
    End Function

    '#If FRAMEWORD_CORE Then
    '    ''' <summary>
    '    ''' Show open file dialog and return the selected file path.
    '    ''' </summary>
    '    ''' <param name="ext$"></param>
    '    ''' <returns></returns>
    '    Public Function SelectFile(Optional ext$ = "*.*", Optional title$ = Nothing) As String
    '        Dim mime$ = ext.GetMIMEDescrib.Details

    '        Using Open As New OpenFileDialog With {
    '            .Filter = $"{ext}|{ext}",
    '            .Title = If(title.StringEmpty, $"Open {mime}", title)
    '        }
    '            If Open.ShowDialog = DialogResult.OK Then
    '                Return Open.FileName
    '            Else
    '                Return Nothing
    '            End If
    '        End Using
    '    End Function
    '#End If

    ''' <summary>
    ''' Invoke a folked system process object to execute a parallel task.
    ''' (本方法会执行外部命令并等待其执行完毕，函数返回状态值)
    ''' </summary>
    ''' <param name="process"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <Extension>
    Public Function Invoke(process As Process) As Integer
        Call process.Start()
        Call process.WaitForExit()
        Return process.ExitCode
    End Function

    ''' <summary>
    ''' Run the driver in a new thread, NOTE: from this extension function calls, then run thread is already be started, 
    ''' so that no needs of calling the method <see cref="Threading.Thread.Start()"/> again.
    ''' (使用线程的方式启动，在函数调用之后，线程是已经启动了的，所以不需要再次调用<see cref="Threading.Thread.Start()"/>方法了)
    ''' </summary>
    ''' <param name="driver">The object which is implements the interface <see cref="ITaskDriver"/></param>
    ''' <remarks>
    ''' if the parameter <paramref name="sync"/> value is set to TRUE, then the given 
    ''' task function <paramref name="driver"/> will be run in the caller thread, a
    ''' caller thread will be blocked at this function in sync mode, this function will
    ''' returns nothing
    ''' 
    ''' otherwise if the parameter <paramref name="sync"/> value is set to FASLE by default,
    ''' then the given task function <paramref name="driver"/> will be running in a new
    ''' thread, the new allocated thread object will be returned from this function.
    ''' </remarks>
    <Extension>
    Public Function DriverRun(driver As ITaskDriver, Optional sync As Boolean = False) As Threading.Thread
        If sync Then
            Call driver.Run()
            Return Nothing
        Else
            Return Parallel.RunTask(
                start:=AddressOf driver.Run,
                taskName:=$"{driver.GetType.Name}_DriverRun_{driver.GetHashCode}"
            )
        End If
    End Function

    ''' <summary>
    ''' Gets the element counts in the target data collection, if the collection object is nothing or empty
    ''' then this function will returns ZERO, others returns Collection.Count.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="collection"></param>
    ''' <returns>this is a safe function: this function returns zero 
    ''' if the given input <paramref name="collection"/> object is nothing
    ''' </returns>
    ''' <remarks>(返回一个数据集合之中的元素的数目，
    ''' 假若这个集合是空值或者空的，则返回0，其他情况则返回Count拓展函数的结果)</remarks>
    <Extension>
    Public Function TryCount(Of T)(collection As IEnumerable(Of T)) As Integer
        If collection Is Nothing Then
            Return 0
        ElseIf TypeOf collection Is T() Then
            Return DirectCast(collection, T()).Length
        ElseIf collection.GetType.IsInheritsFrom(GetType(System.Collections.Generic.List(Of T))) Then
            Return DirectCast(collection, System.Collections.Generic.List(Of T)).Count
        ElseIf collection.GetType.ImplementInterface(GetType(IList)) Then
            Return DirectCast(collection, IList).Count
        ElseIf collection.GetType.ImplementInterface(GetType(ICollection)) Then
            Return DirectCast(collection, ICollection).Count
        Else
            Return Enumerable.Count(collection)
        End If
    End Function

    ''' <summary>
    ''' All of the number value in the target array offset a integer value.
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="intOffset"></param>
    ''' <returns></returns>
    <ExportAPI("OffSet")>
    <Extension> Public Function Offset(ByRef array As Integer(), intOffset As Integer) As Integer()
        For i As Integer = 0 To array.Length - 1
            array(i) = array(i) + intOffset
        Next
        Return array
    End Function

    ''' <summary>
    ''' All of the number value in the target array offset a integer value.
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="intOffset"></param>
    ''' <returns></returns>
    <ExportAPI("OffSet")>
    <Extension> Public Function Offset(ByRef array As Long(), intOffset As Integer) As Long()
        For i As Integer = 0 To array.Length - 1
            array(i) = array(i) + intOffset
        Next
        Return array
    End Function

#Region ""

    <Extension> Public Function InvokeSet(Of T As Class, Tvalue)(obj As T, [Property] As PropertyInfo, value As Tvalue) As T
        Call [Property].SetValue(obj, value, Nothing)
        Return obj
    End Function

    ''' <summary>
    ''' Value assignment to the target variable.(将<paramref name="value"/>参数里面的值赋值给<paramref name="var"/>参数然后返回<paramref name="value"/>)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="var"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function InlineCopy(Of T)(ByRef var As T, value As T) As T
        var = value
        Return value
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function InlineCopy(Of T)(ByRef var As T, value As Func(Of T, T)) As T
        var = value(arg:=var)
        Return var
    End Function

    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T) As T
        arg1 = source
        arg2 = source
        Return source
    End Function

    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T, ByRef arg8 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        arg8 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T, ByRef arg8 As T, ByRef arg9 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        arg8 = source
        arg9 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T, ByRef arg8 As T, ByRef arg9 As T, ByRef arg10 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        arg8 = source
        arg9 = source
        arg10 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T, ByRef arg8 As T, ByRef arg9 As T, ByRef arg10 As T, ByRef arg11 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        arg8 = source
        arg9 = source
        arg10 = source
        arg11 = source
        Return source
    End Function
    ''' <summary>
    ''' Copy the source value directly to the target variable and then return the source value.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    <Extension> Public Function ShadowCopy(Of T)(source As T, ByRef arg1 As T, ByRef arg2 As T, ByRef arg3 As T, ByRef arg4 As T, ByRef arg5 As T, ByRef arg6 As T, ByRef arg7 As T, ByRef arg8 As T, ByRef arg9 As T, ByRef arg10 As T, ByRef arg11 As T, ByRef arg12 As T) As T
        arg1 = source
        arg2 = source
        arg3 = source
        arg4 = source
        arg5 = source
        arg6 = source
        arg7 = source
        arg8 = source
        arg9 = source
        arg10 = source
        arg11 = source
        arg12 = source
        Return source
    End Function
#End Region

#If NET_40 = 0 Then

    ''' <summary>
    ''' Modify target object property value using a <paramref name="valueModifier">specific value provider</paramref> and then return original instance object.
    ''' (修改目标对象的属性之后返回目标对象)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ModifyValue(Of T As Class)([property] As PropertyInfo, obj As T, valueModifier As Func(Of Object, Object)) As T
        Dim Value As Object = [property].GetValue(obj)
        Value = valueModifier(Value)
        Call [property].SetValue(obj, Value)

        Return obj
    End Function
#End If

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Insert data or update the exists data in the dictionary, if the target object with <see cref="INamedValue.Key"/> 
    ''' is not exists in the dictionary, then will be insert, else the old value will be replaced with the parameter 
    ''' value <paramref name="item"/>.
    ''' (向字典对象之中更新或者插入新的数据，假若目标字典对象之中已经存在了一个数据的话，则会将原有的数据覆盖，并返回原来的数据)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="dict"></param>
    ''' <param name="item"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function InsertOrUpdate(Of T As INamedValue)(ByRef dict As Dictionary(Of String, T), item As T) As T
        Dim pre As T

        If dict.ContainsKey(item.Key) Then
            pre = dict(item.Key)

            Call dict.Remove(item.Key)
            Call $"data was updated: {Scripting.ToString(pre)} -> {item.Key}".__DEBUG_ECHO
        Else
            pre = item
        End If

        Call dict.Add(item.Key, item)

        Return pre
    End Function

    ''' <summary>
    ''' Remove target object from dictionary.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="dict"></param>
    ''' <param name="item"></param>
    ''' <returns></returns>
    <Extension> Public Function Remove(Of T As INamedValue)(ByRef dict As Dictionary(Of String, T), item As T) As T
        Call dict.Remove(item.Key)
        Return item
    End Function

    <Extension> Public Function AddRange(Of T As INamedValue)(ByRef dict As Dictionary(Of String, T), data As IEnumerable(Of T)) As Dictionary(Of String, T)
        For Each x As T In data
            Call InsertOrUpdate(dict, x)
        Next

        Return dict
    End Function
#End If

    ''' <summary>
    ''' Merge the target array collection into one collection.(将目标数组的集合合并为一个数组)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function ToVector(Of T)(source As IEnumerable(Of IEnumerable(Of T))) As T()
        Return Unlist(source).ToArray
    End Function

    ''' <summary>
    ''' Empty list will be skip and ignored.
    ''' (这是一个安全的方法，空集合会被自动跳过，并且这个函数总是返回一个集合不会返回空值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <Extension> Public Function Unlist(Of T)(source As IEnumerable(Of IEnumerable(Of T))) As List(Of T)
        Dim list As New List(Of T)

        For Each line As IEnumerable(Of T) In source
            If Not line Is Nothing Then
                Call list.AddRange(collection:=line)
            End If
        Next

        Return list
    End Function

    ''' <summary>
    ''' Merge the target array collection into one collection.
    ''' (将目标数组的集合合并为一个数组，这个方法是提供给超大的集合的，即元素的数目非常的多的，即超过了<see cref="Integer"></see>的上限值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MatrixToUltraLargeVector(Of T)(source As IEnumerable(Of T())) As LinkedList(Of T)
        Dim lnkList As LinkedList(Of T) = New LinkedList(Of T)

        For Each Line As T() In source
            For Each item As T In Line
                Call lnkList.AddLast(item)
            Next
        Next

        Return lnkList
    End Function

    ''' <summary>
    ''' Add a linked list of a collection of specific type of data.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension> Public Function AddRange(Of T)(list As LinkedList(Of T), data As IEnumerable(Of T)) As LinkedList(Of T)
        For Each item As T In data
            Call list.AddLast(item)
        Next

        Return list
    End Function

    ''' <summary>
    ''' 矩阵转置： 将矩阵之中的元素进行行列位置的互换
    ''' </summary>
    ''' <typeparam name="T">矩阵之中的元素类型</typeparam>
    ''' <param name="MAT">为了方便理解和使用，矩阵使用数组的数组来表示的</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Iterator Function MatrixTranspose(Of T)(MAT As IEnumerable(Of T()), Optional safecheck_dimension As Boolean = False) As IEnumerable(Of T())
        Dim data As T()() = MAT.ToArray

        If safecheck_dimension Then
            Dim index As Integer() = data _
                .OrderByDescending(Function(a) a.Length) _
                .First _
                .Sequence _
                .ToArray

            ' maybe slower
            For Each i As Integer In index
                Yield (From line As T() In data Select line.ElementAtOrDefault(i)).ToArray
            Next
        Else
            Dim index = data(Scan0).Sequence.ToArray

            ' it is faster when no check of the matrix dimension
            ' and also it could be index out of range error
            ' when the first row is longer than any other rows
            For Each i As Integer In index
                Yield (From line As T() In data Select line(i)).ToArray
            Next
        End If
    End Function

    ''' <summary>
    ''' 将矩阵之中的元素进行行列位置的互换，请注意，假若长度不一致的话，会按照最短的元素来转置，故而使用本函数可能会造成一些信息的丢失
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="MAT"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Iterator Function MatrixTransposeIgnoredDimensionAgreement(Of T)(MAT As IEnumerable(Of T()), Optional sizeByMax As Boolean = False) As IEnumerable(Of T())
        Dim data = MAT.ToArray
        Dim index As Integer

        If sizeByMax Then
            index = Aggregate row In data Into Max(row.Length)
        Else
            index = Aggregate row In data Into Min(row.Length)
        End If

        For Each i As Integer In index.Sequence
            If sizeByMax Then
                Yield (From line As T() In data Select line.ElementAtOrNull(i)).ToArray
            Else
                Yield (From line As T() In data Select line(i)).ToArray
            End If
        Next
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="DIR">The source directory.</param>
    ''' <param name="moveTo"></param>
    ''' <param name="Split"></param>
    ''' <returns></returns>
#If FRAMEWORD_CORE Then
    <ExportAPI("Mv.Split")>
    Public Function SplitMV(DIR As String, <Parameter("DIR.MoveTo")> moveTo As String, Split As Integer) As Integer
#Else
    Public Function SplitMV(dir As String, moveto As String, split As Integer) As Integer
#End If
        Dim Files As String() = FileIO.FileSystem.GetFiles(DIR, FileIO.SearchOption.SearchTopLevelOnly).ToArray
        Dim n As Integer
        Dim m As Integer = 1

        For i As Integer = 0 To Files.Length - 1
            If n < Split Then
                Call FileIO.FileSystem.MoveFile(Files(i), String.Format("{0}_{1}/{2}", moveTo, m, FileIO.FileSystem.GetFileInfo(Files(i)).Name))
                n += 1
            Else
                n = 0
                m += 1
            End If
        Next

        Return 0
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' The target parameter <paramref name="n"/> value is NaN or not a real number or not?
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' (判断目标实数是否为一个无穷数或者非计算的数字，产生的原因主要来自于除0运算结果或者达到了
    ''' <see cref="Double"></see>的上限或者下限)
    ''' </remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function IsNaNImaginary(n As Double) As Boolean
#Else
    <Extension> Public Function Is_NA_UHandle(n As Double) As Boolean
#End If
        Return Double.IsNaN(n) OrElse
            Double.IsInfinity(n) OrElse
            Double.IsNegativeInfinity(n) OrElse
            Double.IsPositiveInfinity(n)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function IsNaNImaginary(n As Single) As Boolean
        Return Single.IsNaN(n) OrElse
            Single.IsInfinity(n) OrElse
            Single.IsNegativeInfinity(n) OrElse
            Single.IsPositiveInfinity(n)
    End Function
#If FRAMEWORD_CORE Then

    ''' <summary>
    ''' Fuzzy match two string, this is useful for the text query or searching.
    ''' (请注意，这个函数是不会自动转换大小写的，如果是需要字符大小写不敏感，
    ''' 请先将query以及subject都转换为小写)
    ''' </summary>
    ''' <param name="query"></param>
    ''' <param name="Subject"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function FuzzyMatching(query$, subject$, Optional tokenbased As Boolean = True, Optional cutoff# = 0.8) As Boolean
        If tokenbased Then
            Dim similarity# = Evaluate(query, subject,,, )
            Return similarity >= cutoff
        Else
            Dim dist = LevenshteinDistance.ComputeDistance(query, subject)
            If dist Is Nothing Then
                Return False
            Else
                Return dist.MatchSimilarity >= cutoff
            End If
        End If
    End Function
#End If

    ''' <summary>
    ''' 函数只返回有重复的数据
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="TTag"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="getKey"></param>
    ''' <returns></returns>
    <Extension> Public Function CheckDuplicated(Of T, TTag)(source As IEnumerable(Of T),
                                                            getKey As Func(Of T, TTag)) _
                                                                   As GroupResult(Of T, TTag)()
        Dim Groups = From x As T
                     In source
                     Select x
                     Group x By tag = getKey(x) Into Group '
        Dim duplicates As GroupResult(Of T, TTag)() =
            LinqAPI.Exec(Of GroupResult(Of T, TTag)) <=
                                                       _
                From g
                In Groups.AsParallel
                Where g.Group.Count > 1
                Select New GroupResult(Of T, TTag) With {
                    .Tag = g.tag,
                    .Group = g.Group.ToArray
                }

        Return duplicates
    End Function

    ''' <summary>
    ''' 移除重复的对象，这个函数是根据对象所生成的标签来完成的
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="Tag"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="getKey">得到对象的标签</param>
    ''' <returns></returns>
    <Extension> Public Function RemoveDuplicates(Of T, Tag)(source As IEnumerable(Of T), getKey As Func(Of T, Tag)) As T()
        Dim Groups = From obj As T
                     In source
                     Select obj
                     Group obj By objTag = getKey(obj) Into Group '
        Dim LQuery = (From obj In Groups Select obj.Group.First).ToArray
        Return LQuery
    End Function

#If FRAMEWORD_CORE Then

    ''' <summary>
    ''' Remove all of the null object in the target object collection.
    ''' (这个是一个安全的方法，假若目标集合是空值，则函数会返回一个空的集合)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <Extension>
    Public Function TrimNull(Of T As Class)(source As IEnumerable(Of T)) As T()
#Else
    ''' <summary>
    ''' Remove all of the null object in the target object collection
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function TrimNull(Of T As Class)(source As IEnumerable(Of T)) As T()
#End If
        If source Is Nothing Then
            Return New T() {}
        Else
            Return (From x In source Where Not x Is Nothing Select x).ToArray
        End If
    End Function

    ''' <summary>
    ''' Remove all of the null object in the target object collection
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function TrimNull(source As IEnumerable(Of String)) As String()
        If source Is Nothing Then
            Return New String() {}
        Else
            Return (From x In source Where Not x.StringEmpty Select x).ToArray
        End If
    End Function

    ''' <summary>
    ''' Return a collection with randomize element position in 
    ''' <paramref name="source">the original collection</paramref>.
    ''' (从原有序序列中获取一个随机元素的序列)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this method can be affected by the <see cref="Math.RandomExtensions.SetSeed"/> method.
    ''' </remarks>
    <ExportAPI("Shuffles")>
    <Extension>
    Public Function Shuffles(Of T)(source As IEnumerable(Of T)) As T()
        Dim list = source.SafeQuery.ToList
        Call Math.Shuffle(list)
        Return list.ToArray
    End Function

    ''' <summary>
    ''' Generates the shuffle index result
    ''' 
    ''' (返回n长度的序列数值，这些序列数值是打乱顺序的，但是升序排序之后会得到1:<paramref name="n"/>的序列
    ''' 请注意，这个序列并不是随机数，而是将n长度的序列之中的元素打乱顺序的结果)
    ''' </summary>
    ''' <param name="n">the size of the index vector</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 1. generates a index vector in range 0:<paramref name="n"/>-1
    ''' 2. make index shuffles
    ''' 
    ''' returns the shuffles result
    ''' </remarks>
    <Extension>
    Public Function SeqRandom(n As Integer) As Integer()
        Dim source As Integer() = n.Sequence.ToArray
        Dim random As Integer() = source.Shuffles
        Return random
    End Function

    ''' <summary>
    ''' Convert target object type collection into a string array using
    ''' the <see cref="any.ToString(Object, String, Boolean)"/> interface
    ''' function.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns>A string array</returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function ToStringArray(Of T)(source As IEnumerable(Of T), Optional null As String = "") As String()
        If source Is Nothing Then
            Return {}
        End If

        Dim LQuery$() = LinqAPI.Exec(Of String) _
                                                _
            () <= From item As T
                  In source
                  Let strItem As String = any.ToString(item, null:=null)
                  Select strItem

        Return LQuery
    End Function

    ''' <summary>
    ''' swap two element in the array
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="array"></param>
    ''' <param name="a%"></param>
    ''' <param name="b%"></param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub Swap(Of T)(ByRef array As T(), a%, b%)
        Dim tmp As T = array(a)
        array(a) = array(b)
        array(b) = tmp
    End Sub

    ''' <summary>
    ''' Swap the value in the two variables.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj1"></param>
    ''' <param name="obj2"></param>
    ''' <remarks></remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub Swap(Of T)(ByRef obj1 As T, ByRef obj2 As T)
        Dim temp As T = obj1
        obj1 = obj2
        obj2 = temp
    End Sub

    ''' <summary>
    ''' Swap the two item position in the target <paramref name="list">list</paramref>.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <param name="obj_1"></param>
    ''' <param name="obj_2"></param>
    <Extension>
    Public Sub SwapItem(Of T)(ByRef list As List(Of T), obj_1 As T, obj_2 As T)
        Dim idx_1 As Integer = list.IndexOf(obj_1)
        Dim idx_2 As Integer = list.IndexOf(obj_2)

        If idx_1 = -1 OrElse idx_2 = -1 Then
            Return
        End If

        Call list.RemoveAt(idx_1)
        Call list.Insert(idx_1, obj_2)
        Call list.RemoveAt(idx_2)
        Call list.Insert(idx_2, obj_2)
    End Sub

#If FRAMEWORD_CORE Then

    ''' <summary>
    ''' Add array location index value for the <see cref="IAddressOf"/> elements in the sequence.
    ''' (为列表中的对象添加对象句柄值)
    ''' </summary>
    ''' <param name="source"></param>
    ''' <remarks></remarks>
    <Extension>
    Public Function WriteAddress(Of T As IAddressOf)(ByRef source As IEnumerable(Of T), Optional offset As Integer = 0) As T()
        Dim list As New List(Of T)
        Dim i As Integer = offset

        For Each x As T In source
            Call x.Assign(address:=i)

            i += 1
            list += x
        Next

        Return list
    End Function
#End If

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Gets the subscript index of a generic collection.(获取某一个集合的下标的集合)
    ''' </summary>
    ''' <typeparam name="T">集合中的元素为任意类型的</typeparam>
    ''' <param name="source">目标集合对象</param>
    ''' <returns>A integer array of subscript index of the target generic collection.</returns>
    ''' <remarks></remarks>
    '''
    <Extension>
    Public Iterator Function Sequence(Of T)(
                                        <Parameter("source", "")> source As IEnumerable(Of T),
                                        <Parameter("index.OffSet", "")> Optional offSet% = 0) _
                                     As <FunctionReturns("A integer array of subscript index of the target generic collection.")> IEnumerable(Of Integer)
#Else
    ''' <summary>
    ''' 获取某一个集合的下标的集合
    ''' </summary>
    ''' <typeparam name="T">集合中的元素为任意类型的</typeparam>
    ''' <param name="Collection">目标集合对象</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <Extension> Public Iterator Function Sequence(Of T)(source As IEnumerable(Of T), Optional offset As Integer = 0) As IEnumerable(Of Integer)
#End If
        If source Is Nothing Then
            Return
        Else
            Dim i As Integer = offSet

            For Each x As T In source
                Yield i
                i += 1
            Next
        End If
    End Function

    ''' <summary>
    ''' Alias of the linq function <see cref="Enumerable.Range"/>
    ''' </summary>
    ''' <param name="range"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Iterator Function Sequence(range As IntRange, Optional stepOffset% = 1) As IEnumerable(Of Integer)
        If stepOffset = 0 Then
            stepOffset = 1
#If DEBUG Then
            Call $"step_offset is ZERO! This will caused a infinity loop, using default step `1`!".Warning
#End If
        End If

        For i As Integer = range.Min To range.Max Step stepOffset
            Yield i
        Next
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsRange(ints As IEnumerable(Of Integer)) As IntRange
        Return New IntRange(ints)
    End Function

    <Extension>
    Public Iterator Function LongSeq(Of T)(source As IEnumerable(Of T), Optional offset% = 0) As IEnumerable(Of Long)
        If source Is Nothing Then
            Return
        Else
            Dim i As Long = offset

            For Each x As T In source
                Yield i
                i += 1
            Next
        End If
    End Function

    <Extension>
    Public Function LongSeq(n&) As Long()
        Dim array&() = New Long(n - 1) {}

        For i As Long = 0 To array.Length - 1
            array(i) = i
        Next

        Return array
    End Function

    ''' <summary>
    ''' 将目标键值对对象的集合转换为一个字典对象
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="remoteDuplicates">当这个参数为False的时候，出现重复的键名会抛出错误，当为True的时候，有重复的键名存在的话，可能会丢失一部分的数据</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function ToDictionary(Of TKey, TValue)(
                                source As IEnumerable(Of KeyValuePair(Of TKey, TValue)),
                       Optional remoteDuplicates As Boolean = False) As Dictionary(Of TKey, TValue)

        If remoteDuplicates Then
            Dim table As New Dictionary(Of TKey, TValue)

            For Each x In source
                If table.ContainsKey(x.Key) Then
                    Call $"[Duplicated] {x.Key.ToString}".PrintException
                Else
                    Call table.Add(x.Key, x.Value)
                End If
            Next

            Return table
        Else
            Dim dictionary As Dictionary(Of TKey, TValue) =
                source.ToDictionary(Function(x) x.Key,
                                    Function(x) x.Value)
            Return dictionary
        End If
    End Function

    ''' <summary>
    ''' [height, width] or [rows, columns]
    ''' </summary>
    ''' <param name="size"></param>
    ''' <param name="reverse">
    ''' [width, height] or [columns, rows]
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function ToArray(size As Size, Optional reverse As Boolean = False) As Integer()
        If reverse Then
            Return {size.Width, size.Height}
        Else
            Return {size.Height, size.Width}
        End If
    End Function

    ''' <summary>
    ''' [height, width] or [rows, columns]
    ''' </summary>
    ''' <param name="size"></param>
    ''' <param name="reverse">
    ''' [width, height] or [columns, rows]
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function ToArray(size As SizeF, Optional reverse As Boolean = False) As Double()
        If reverse Then
            Return New Double() {size.Width, size.Height}
        Else
            Return New Double() {size.Height, size.Width}
        End If
    End Function

#If FRAMEWORD_CORE Then

    ''' <summary>
    ''' 执行一个命令行语句，并返回一个IO重定向对象，以获取被执行的目标命令的标准输出
    ''' </summary>
    ''' <param name="CLI"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("Shell")>
    <Extension>
    Public Function Shell(CLI As String) As IIORedirectAbstract
        Return CType(CLI, IORedirect)
    End Function
#End If

    ''' <summary>
    ''' 获取一个实数集合中所有元素的积
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    '''
    <ExportAPI("PI")>
    <Extension>
    Public Function π(source As IEnumerable(Of Double)) As Double
        If source Is Nothing Then
            Return 0
        End If

        Dim result# = 1
        Dim stepInto As Boolean = False

        For Each x As Double In source
            stepInto = True
            result *= x
        Next

        If Not stepInto Then
            Return 0
        Else
            Return result
        End If
    End Function

    ''' <summary>
    ''' Remove all of the element in the <paramref name="collection"></paramref> from target <paramref name="List">list</paramref>
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="List"></param>
    ''' <param name="collection"></param>
    ''' <remarks></remarks>
    <Extension>
    Public Sub Removes(Of T)(ByRef List As List(Of T), collection As IEnumerable(Of T))
        For Each obj In collection
            Call List.Remove(obj)
        Next
    End Sub

#Region "Removes Last Element"

    ''' <summary>
    ''' Removes the last element in the List object.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="clrList"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' (这个拓展函数同时兼容.NET框架的list类型以及sciBASIC之中的list类型)
    ''' </remarks>
    <Extension>
    Public Function RemoveLast(Of T)(ByRef clrList As System.Collections.Generic.List(Of T)) As System.Collections.Generic.List(Of T)
        If clrList.IsNullOrEmpty Then
            clrList = New List(Of T)

            ' 2018-1-25
            ' 需要将0和1分开来看，否则会造成最后一个元素永远都移除不了的bug
        ElseIf clrList.Count = 1 Then
            clrList.Clear()
        Else
            Dim i As Integer = clrList.Count - 1
            Call clrList.RemoveAt(i)
        End If

        Return clrList
    End Function

    ''' <summary>
    ''' Removes the last element in the List object.
    ''' (这个拓展函数同时兼容.NET框架的list类型以及sciBASIC之中的<see cref="List(Of T)"/>类型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <returns></returns>
    <Extension>
    Public Function RemoveLast(Of T)(ByRef list As List(Of T)) As List(Of T)
        Return DirectCast(RemoveLast(clrList:=list), List(Of T))
    End Function

#End Region

    ''' <summary>
    ''' Removes the first element and then returns the list
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <returns></returns>
    <Extension>
    Public Function RemoveFirst(Of T)(ByRef list As List(Of T)) As List(Of T)
        If list.IsNullOrEmpty OrElse list.Count = 1 Then
            list = New List(Of T)
        Else
            Call list.RemoveAt(Scan0)
        End If

        Return list
    End Function
End Module
