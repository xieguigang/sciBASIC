#Region "Microsoft.VisualBasic::45bcb377cac3f893bcfe3bc12b19e0dd, ..\visualbasic_App\UXFramework\Molk+\Molk+\Extensions.vb"

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
Imports System.Text
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Drawing
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

#Const DEBUG = True

Namespace My

    Public Module CommonExtensions

        ''' <summary>
        ''' 调用本模块的程序集的可执行文件的完整的文件路径
        ''' </summary>
        ''' <remarks></remarks>
        Private ReadOnly _Assembly As String = String.Format("{0}/{1}.exe", My.Application.Info.DirectoryPath, My.Application.Info.AssemblyName)

        ''' <summary>
        ''' Get the exe file full path string.(调用本模块的程序集的可执行文件的完整的文件路径)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ExePath As String
            Get
                Return FileIO.FileSystem.GetFileInfo(_Assembly).FullName
            End Get
        End Property
    End Module
End Namespace

#If FRAMEWORD_CORE Then
''' <summary>
''' Common extension methods library for convenient the programming job.
''' </summary>
''' <remarks></remarks>
<[Namespace]("Framework.Extensions")> <System.Runtime.CompilerServices.ExtensionAttribute>
Public Module Extensions
#Else
''' <summary>
''' Common extension methods library for convenient the programming job.
''' </summary>
''' <remarks></remarks>  
Public Module Extensions
#End If

    ''' <summary>
    ''' This method is used to replace most calls to the Java String.split method.
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="regexDelimiter"></param>
    ''' <param name="trimTrailingEmptyStrings"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function StringSplit(Source As String, RegexDelimiter As String, Optional TrimTrailingEmptyStrings As Boolean = False) As String()
        Dim splitArray As String() = System.Text.RegularExpressions.Regex.Split(Source, RegexDelimiter)

        If Not TrimTrailingEmptyStrings OrElse splitArray.Length <= 1 Then Return splitArray

        For i As Integer = splitArray.Length To 1 Step -1

            If splitArray(i - 1).Length > 0 Then
                If i < splitArray.Length Then
                    Call System.Array.Resize(splitArray, i)
                End If

                Exit For
            End If
        Next

        Return splitArray
    End Function

    <Extension> Public Function TrimVBCrLf(s As String) As String
        s = s.Replace(vbCrLf, "")
        s = s.Replace(vbCr, "").Replace(vbLf, "")
        Return s
    End Function

    Public Function MAT(Of T)(m As Integer, n As Integer) As T()()
        Dim MATValue As T()() = New T(m - 1)() {}

        For i As Integer = 0 To m - 1
            MATValue(i) = New T(n - 1) {}
        Next

        Return MATValue
    End Function

    ''' <summary>
    ''' 当函数返回Nothing的时候说明目标对象不是一个函数指针
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetDelegateInvokeEntryPoint(obj As Object) As System.Reflection.MethodInfo
        Dim TypeInfo As System.Type = obj.GetType
        Dim InvokeEntryPoint = (From MethodInfo As System.Reflection.MethodInfo In TypeInfo.GetMethods Where String.Equals(MethodInfo.Name, "Invoke") Select MethodInfo).ToArray.FirstOrDefault
        Return InvokeEntryPoint
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="collection">要求集合之中的每一列之中的数据的元素数目都相等</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToMatrix(Of T)(collection As Generic.IEnumerable(Of Generic.IEnumerable(Of T))) As T(,)
        Dim width As Integer = collection.First.Count
        Dim height As Integer = collection.Count
        Dim MAT As T(,) = New T(height - 1, width - 1) {}

        For i As Integer = 0 To collection.Count - 1
            Dim row = collection(i).ToArray

            For j As Integer = 0 To row.Count - 1
                MAT(i, j) = row(j)
            Next
        Next

        Return MAT
    End Function

    <Extension> Public Function MatrixToVectorList(Of T)(MAT As T(,)) As List(Of T())
        Dim ChunkList As List(Of T()) = New List(Of T())
        Dim width As Integer = MAT.GetLength(1)
        Dim height As Integer = MAT.GetLength(0)

        For i As Integer = 0 To height - 1
            Dim Temp As T() = New T(width - 1) {}

            For j As Integer = 0 To width - 1
                Temp(j) = MAT(i, j)
            Next

            Call ChunkList.Add(Temp)
        Next

        Return ChunkList
    End Function

    Public Structure SlideWindowHandle(Of T)
        Implements Generic.IEnumerable(Of T)

        ''' <summary>
        ''' 在创建的滑窗的队列之中当前的窗口对象的位置
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property p As Integer
        Public Property Elements As T()

        ''' <summary>
        ''' 当前窗口在原始的序列之中的左端起始位点
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Left As Integer

        Public ReadOnly Property Right As Integer
            Get
                Return Left + Length
            End Get
        End Property

        ''' <summary>
        ''' 窗口长度
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Length As Integer
            Get
                If Elements.IsNullOrEmpty Then
                    Return 0
                Else
                    Return Elements.Count
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("{0}  ----> {1}", p, String.Join(", ", Elements))
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each item In Elements
                Yield item
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Structure

    ''' <summary>
    ''' Create a collection of slide Windows data for the target collection object.(创建一个滑窗集合)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="slideWindowSize">窗口的大小</param>
    ''' <param name="offset">在序列之上移动的步长</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CreateSlideWindows(Of T)(data As Generic.IEnumerable(Of T), slideWindowSize As Integer, Optional offset As Integer = 1) As SlideWindowHandle(Of T)()
        If slideWindowSize >= data.Count Then
            Return {New SlideWindowHandle(Of T)() With {.Left = 0, .Elements = data.ToArray}}
        End If

        If offset < 1 Then
            Call Console.WriteLine("[DEBUG] The offset parameter is not correct, set its value to 1 as default!")
            offset = 1
        End If

        Dim TempList As List(Of T) = data.ToList
        Dim List As List(Of SlideWindowHandle(Of T)) = New List(Of SlideWindowHandle(Of T))
        Dim n As Integer = data.Count - slideWindowSize - 1
        Dim p As Integer = 0

        For i As Integer = 0 To n Step offset
            Dim ChunkBuffer As T() = TempList.Take(slideWindowSize).ToArray
            Call List.Add(New SlideWindowHandle(Of T)() With {.Elements = ChunkBuffer, .Left = i, .p = p})
            Call TempList.RemoveRange(0, offset)

            p += 1
        Next

        If Not TempList.IsNullOrEmpty Then
            Call List.Add(New SlideWindowHandle(Of T)() With {.Left = n + 1, .elements = TempList.ToArray, .p = p})
        End If

        Return List.ToArray
    End Function

    ''' <summary>
    ''' Chr(0): NULL char
    ''' </summary>
    ''' <remarks></remarks>
    Public Const NIL As Char = Chr(0)

    <Extension> Public Function ToStringEx(dat As Date) As String
        Dim yy = dat.Year
        Dim mm As String = dat.Month.FormatZero
        Dim dd As String = dat.Day.FormatZero
        Dim hh As String = dat.Hour.FormatZero
        Dim mmin As String = dat.Minute.FormatZero

        Return String.Format("{0}/{1}/{2} {3}:{4}", yy, mm, dd, hh, mmin)
    End Function

    <Extension> Public Function ToNormalizedPathString(dat As Date) As String
        Dim yy = dat.Year
        Dim mm As String = dat.Month.FormatZero
        Dim dd As String = dat.Day.FormatZero
        Dim hh As String = dat.Hour.FormatZero
        Dim mmin As String = dat.Minute.FormatZero

        Return String.Format("{0}-{1}-{2}_{3}${4}", yy, mm, dd, hh, mmin)
    End Function

    <Extension> Public Function FormatZero(n As Integer, Optional fill As String = "00") As String
        Dim s = n.ToString
        Dim d = Len(fill) - Len(s)

        If d < 0 Then
            Return s
        Else
            Return Mid(fill, 1, d) & s
        End If
    End Function

    ''' <summary>
    ''' 将目标集合之中的数据按照<paramref name="splitCount"></paramref>参数分配到子集合之中
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="collection"></param>
    ''' <param name="splitCount">每一个子集合之中的元素的数目</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension> Public Function SplitCollection(Of T)(collection As Generic.IEnumerable(Of T), splitCount As Integer) As T()()
        Dim ChunkList As List(Of T()) = New List(Of T())
        Dim ChunkBuffer As T() = collection.ToArray

        Do While ChunkBuffer.Count > 0
            Dim ChunkTemp As T() = ChunkBuffer.Take(splitCount).ToArray
            Call ChunkList.Add(ChunkTemp)
            ChunkBuffer = ChunkBuffer.Skip(splitCount).ToArray
        Loop

        Return ChunkList.ToArray
    End Function

#If FRAMEWORD_CORE Then
    <Command("select_file")>
    Public Function SelectFile(ext As String) As String
        Using Open = New OpenFileDialog With {.Filter = ext}

            If Open.ShowDialog = DialogResult.OK Then
                Return Open.FileName
            Else
                Return ""
            End If
        End Using
    End Function
#End If

    ''' <summary>
    ''' 本方法会执行外部命令并等待其执行完毕，函数返回状态值
    ''' </summary>
    ''' <param name="Process"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Invoke(Process As Process) As Integer
        Call Process.Start()
        Call Process.WaitForExit()
        Return Process.ExitCode
    End Function

    ''' <summary>
    ''' Gets a random number in the region of [0,1]. (获取一个[0,1]区间之中的随机数)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function RandomDouble() As Double
        Call Randomize()
        Dim n As Double = Rnd() * 100
CHECKS: If n > 1 Then
            n /= 10
            GoTo CHECKS
        End If

        Return n
    End Function

#If FRAMEWORD_CORE Then
    <Command("exists_file")>
    <Extension> Public Function FileExists(path As String) As Boolean
#Else
    <Extension> Public Function FileExists(path As String) As Boolean
#End If
        Return Not String.IsNullOrEmpty(path) AndAlso FileIO.FileSystem.FileExists(path)
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 获取目标文件夹的名称
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Command("dir.base_name")>
    Public Function BaseName(dir As String) As String
#Else
    Public Function BaseName(dir As String) As String
#End If
        Return FileIO.FileSystem.GetDriveInfo(dir).Name
    End Function

#If FRAMEWORD_CORE Then
    <Command("run", info:="Running the object model driver, the target object should implement the driver interface.")>
    Public Function RunDriver(driver As ComponentModel.DataSourceModel.IObjectModel_Driver) As Integer
        Return driver.Run
    End Function
#End If

    <Extension> Public Function _Count(Of T)(Collection As Generic.IEnumerable(Of T)) As Integer
        If Collection.IsNullOrEmpty Then
            Return 0
        Else
            Return System.Linq.Enumerable.Count(Collection)
        End If
    End Function

#If FRAMEWORD_CORE Then

    '''<summary>
    '''  Looks up a localized string similar to                     GNU GENERAL PUBLIC LICENSE
    '''                       Version 3, 29 June 2007
    '''
    ''' Copyright (C) 2007 Free Software Foundation, Inc. &lt;http://fsf.org/&gt;
    ''' Everyone is permitted to copy and distribute verbatim copies
    ''' of this license document, but changing it is not allowed.
    '''
    '''                            Preamble
    '''
    '''  The GNU General Public License is a free, copyleft license for
    '''software and other kinds of works.
    '''
    '''  The licenses for most software and other practical works are designed
    '''to take away yo [rest of string was truncated]&quot;;.
    '''</summary>
    Public ReadOnly Property GPL3 As String
        Get
            Return My.Resources.gpl
        End Get
    End Property
#End If

    ''' <summary>
    ''' 获取""或者其他字符所包围的字符串的值
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="wrapper"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetString(s As String, wrapper As Char) As String
        If String.IsNullOrEmpty(s) OrElse Len(s) = 1 Then
            Return s
        End If
        If s.First = wrapper AndAlso s.Last = wrapper Then
            Return Mid(s, 2, Len(s) - 2)
        Else
            Return s
        End If
    End Function

#If NET_40 = 0 Then

    Private ReadOnly _AllDotNETPrefixColors As Color() = (From Color As Color In (From p As PropertyInfo
                                                                                  In GetType(Color).GetProperties(Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static)
                                                                                  Where p.PropertyType = GetType(Color)
                                                                                  Let ColorValue As Color = DirectCast(p.GetValue(Nothing), Color)
                                                                                  Select ColorValue).ToArray
                                                          Where Color <> Color.White
                                                          Select Color).ToArray

    Public ReadOnly Property AllDotNetPrefixColors As Color()
        Get
            Return _AllDotNETPrefixColors.RandomizeElements
        End Get
    End Property
#End If

    ''' <summary>
    ''' Value assignment to the target variable. 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="var"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function SetValueMethod(Of T)(ByRef var As T, value As T) As T
        var = value
        Return value
    End Function

    ''' <summary>
    ''' Pause the console program.
    ''' </summary>
    ''' <param name="Prompted"></param>
    ''' <remarks></remarks>
    Public Sub Pause(Optional Prompted As String = "Press any key to continute...")
        Call Console.WriteLine(Prompted)
        Call Console.Read()
    End Sub

    Const _DOUBLE As String = "((-?\d\.\d+e[+-]\d+)|(-?\d+\.\d+)|(-?\d+))"

    ''' <summary>
    ''' 使用正则表达式解析目标字符串对象之中的一个实数
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ParseDouble(s As String) As Double
        Return Val(s.Match(_DOUBLE))
    End Function

    ''' <summary>
    ''' 当所被读取的文本文件的大小超过了<see cref="System.Text.StringBuilder"></see>的上限的时候，就需要使用本方法进行读取操作了。<paramref name="Path">目标文件</paramref>必须是已经存在的文件
    ''' </summary>
    ''' <param name="Path">目标文件必须是已经存在的文件</param>
    ''' <param name="Encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ReadUltraLargeTextFile(Path As String, Encoding As System.Text.Encoding) As String
        Using FileStream As FileStream = New FileStream(Path, FileMode.Open)
            Dim ChunkBuffer As Byte() = New Byte(FileStream.Length - 1) {}
            Call FileStream.Read(ChunkBuffer, 0, ChunkBuffer.Count)
            Return Encoding.GetString(ChunkBuffer)
        End Using
    End Function

#If NET_40 = 0 Then

    ''' <summary>
    ''' 尝试将目标集合类型转换为通用的枚举类型
    ''' </summary>
    ''' <param name="Type"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Collection2GenericIEnumerable(Type As Type) As Type
        If Array.IndexOf(Type.GetInterfaces, GetType(IEnumerable)) = -1 Then
            Call Console.WriteLine(String.Format("[WARN] Target type ""{0}"" is not a collection type!", Type.FullName))
            Return Type
        End If

        Dim GenericType As Type = GetType(Generic.IEnumerable(Of )) 'Type.GetType("System.Collections.Generic.IEnumerable")
        Dim ElementType = Type.GetElementType
        If ElementType Is Nothing Then
            ElementType = Type.GenericTypeArguments.First
        End If
        GenericType = GenericType.MakeGenericType({ElementType})
        Return GenericType
    End Function
#End If

    ''' <summary>
    ''' Save the binary data into the filesystem.(保存二进制数据包值文件系统)
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="saveto"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function SavePackage(data As Generic.IEnumerable(Of Byte), saveto As String) As Boolean
        Dim ParentDir As String = If(String.IsNullOrEmpty(saveto), FileIO.FileSystem.CurrentDirectory, FileIO.FileSystem.GetParentPath(saveto))
        Call FileIO.FileSystem.CreateDirectory(ParentDir)
        Call FileIO.FileSystem.WriteAllBytes(saveto, data.ToArray, False)

        Return True
    End Function

    ''' <summary>
    ''' Execute a property modify method and then return the target instance object.(修改目标对象的属性之后返回目标对象)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ModifyValue(Of T As Class)(obj As T, valueModifier As System.Action) As T
        Call valueModifier()
        Return obj
    End Function

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

    Public Declare Function SetProcessWorkingSetSize Lib "kernel32.dll" (ByVal process As IntPtr, ByVal minimumWorkingSetSize As Integer, ByVal maximumWorkingSetSize As Integer) As Integer

    ''' <summary>
    ''' Rabbish collection to free the junk memory.(垃圾回收)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub FlushMemory()
        Call GC.Collect()
        Call GC.WaitForPendingFinalizers()

        If (Environment.OSVersion.Platform = PlatformID.Win32NT) Then
            Call SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1)
        End If
    End Sub

    <Extension> Public Function VectorCollectionToMatrix(Of T)(Vectors As Generic.IEnumerable(Of Generic.IEnumerable(Of T))) As T(,)
        Dim MAT As T(,) = New T(Vectors.Count, Vectors.First.Count) {}
        Dim Dimension As Integer = Vectors.First.Count

        For i As Integer = 0 To MAT.GetLength(Dimension)
            Dim Vector = Vectors(i)

            For j As Integer = 0 To Dimension
                MAT(i, j) = Vector(j)
            Next
        Next

        Return MAT
    End Function

    <Extension> Public Function Get_XmlAttributeValue(strData As String, Name As String) As String
        Dim m As Match = Regex.Match(strData, Name & "=(("".+?"")|[^ ]*)")
        If Not m.Success Then Return ""

        strData = m.Value.Replace(Name & "=", "")
        If strData.First = """"c AndAlso strData.Last = """"c Then
            strData = Mid(strData, 2, Len(strData) - 2)
        End If
        Return strData
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 向字典对象之中更新或者插入新的数据，假若目标字典对象之中已经存在了一个数据的话，则会将原有的数据覆盖，并返回原来的数据
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="dict"></param>
    ''' <param name="item"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function InsertOrUpdate(Of T As Microsoft.VisualBasic.ComponentModel.Collection.Generic.IAccessionIdEnumerable)( _
                            ByRef dict As Dictionary(Of String, T), item As T) _
        As T

        Dim pre As T

        If dict.ContainsKey(item.UniqueId) Then
            pre = dict(item.UniqueId)

            Call dict.Remove(item.UniqueId)
            Call Console.WriteLine("[DEBUG] data was updated: {0} -> {1}", pre.ToString, item.UniqueId)
        Else
            pre = item
        End If

        Call dict.Add(item.UniqueId, item)

        Return pre
    End Function

    <Extension> Public Function Remove(Of T As Microsoft.VisualBasic.ComponentModel.Collection.Generic.IAccessionIdEnumerable)( _
                            ByRef dict As Dictionary(Of String, T), item As T) As T

        Call dict.Remove(item.UniqueId)
        Return item
    End Function

    <Extension> Public Function AddRange(Of T As Microsoft.VisualBasic.ComponentModel.Collection.Generic.IAccessionIdEnumerable)( _
                            ByRef dict As Dictionary(Of String, T), data As Generic.IEnumerable(Of T)) _
        As Dictionary(Of String, T)

        For Each item In data
            Call InsertOrUpdate(dict, item)
        Next

        Return dict
    End Function
#End If

    <Extension> Public Function IsNullOrEmpty(sBuilder As StringBuilder) As Boolean
        Return sBuilder Is Nothing OrElse sBuilder.Length = 0
    End Function

    ''' <summary>
    ''' Merge the target array collection into one collection.(将目标数组的集合合并为一个数组)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MatrixToVector(Of T)(Collection As Generic.IEnumerable(Of Generic.IEnumerable(Of T))) As T()
        Return MatrixToList(Collection).ToArray
    End Function

    <Extension> Public Function MatrixToList(Of T)(Collection As Generic.IEnumerable(Of Generic.IEnumerable(Of T))) As List(Of T)
        Dim ChunkBuffer As List(Of T) = New List(Of T)

        For Each Line As Generic.IEnumerable(Of T) In Collection

            If Not Line.IsNullOrEmpty Then
                Call ChunkBuffer.AddRange(collection:=Line)
            End If
        Next

        Return ChunkBuffer
    End Function

    ''' <summary>
    ''' Merge the target array collection into one collection.(将目标数组的集合合并为一个数组，这个方法是提供给超大的集合的，即元素的数目非常的多的，即超过了<see cref="Integer"></see>的上限值)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MatrixToUltraLargeVector(Of T)(Collection As Generic.IEnumerable(Of T())) As LinkedList(Of T)
        Dim ChunkBuffer As LinkedList(Of T) = New LinkedList(Of T)

        For Each Line As T() In Collection
            For Each item As T In Line
                Call ChunkBuffer.AddLast(item)
            Next
        Next

        Return ChunkBuffer
    End Function

    ''' <summary>
    ''' 将矩阵之中的元素进行行列位置的互换
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="MAT"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MatrixTranspose(Of T)(MAT As Generic.IEnumerable(Of T())) As T()()
        Dim LQuery = (From i As Integer In MAT.First.Count.Sequence Select (From Line In MAT Select Line(i)).ToArray).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' 将矩阵之中的元素进行行列位置的互换，请注意，假若长度不一致的话，会按照最短的元素来转置，故而使用本函数可能会造成一些信息的丢失
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="MAT"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MatrixTransposeIgnoredDimensionAgreement(Of T)(MAT As Generic.IEnumerable(Of T())) As T()()
        Dim LQuery = (From i As Integer In (From n In MAT Select n.Count Order By Count Ascending).ToArray.First.Sequence Select (From Line In MAT Select Line(i)).ToArray).ToArray
        Return LQuery
    End Function

#If FRAMEWORD_CORE Then
    <Command("mv.split")>
    Public Function SplitMV(dir As String, moveto As String, split As Integer) As Integer
#Else
    Public Function SplitMV(dir As String, moveto As String, split As Integer) As Integer
#End If
        Dim Files As String() = FileIO.FileSystem.GetFiles(dir, FileIO.SearchOption.SearchTopLevelOnly).ToArray
        Dim n As Integer
        Dim m As Integer = 1

        For i As Integer = 0 To Files.Count - 1
            If n < split Then
                Call FileIO.FileSystem.MoveFile(Files(i), String.Format("{0}_{1}/{2}", moveto, m, FileIO.FileSystem.GetFileInfo(Files(i)).Name))
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
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection">请务必要确保集合之中的元素的<see cref="Microsoft.VisualBasic.ComponentModel.Collection.Generic.IAccessionIdEnumerable.UniqueId"></see></param>属性是唯一的，否则会出错
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToEntriesDictionary(Of T As Microsoft.VisualBasic.ComponentModel.Collection.Generic.IAccessionIdEnumerable)(Collection As Generic.IEnumerable(Of T)) As Dictionary(Of String, T)
        Dim Dictionary As Dictionary(Of String, T) = New Dictionary(Of String, T)
        For Each Item In Collection
            Call Dictionary.Add(Item.UniqueId, Item)
        Next

        Return Dictionary
    End Function
#End If

    ''' <summary>
    ''' 求交集
    ''' </summary>
    ''' <param name="Chunkbuffer"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Intersection(Chunkbuffer As Generic.IEnumerable(Of Generic.IEnumerable(Of String))) As String()
        Chunkbuffer = (From line In Chunkbuffer Select (From strValue As String In line Select strValue Distinct Order By strValue Ascending).ToArray).ToArray
        Dim Union As List(Of String) = New List(Of String)
        For Each Line As String() In Chunkbuffer
            Call Union.AddRange(Line)
        Next
        Union = (From strValue As String In Union Select strValue Distinct Order By strValue Ascending).ToList  '获取并集，接下来需要从并集之中去除在两个集合之中都不存在的
        For Each Line In Chunkbuffer
            For Each Collection In Chunkbuffer       '遍历每一个集合
                Dim LQuery = (From strvalue As String In Collection Where Array.IndexOf(Line, strvalue) = -1 Select strvalue).ToArray
                For Each value In LQuery
                    Call Union.Remove(value) '假若line之中存在不存在的元素，则从并集之中移除
                Next
            Next
        Next
        Return Union.ToArray
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 判断目标实数是否为一个无穷数或者非计算的数字，产生的原因主要来自于除0运算结果或者达到了<see cref="Double"></see>的上限或者下限
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Command("double.is.na", info:="Is this double type of the number is an NA type infinity number. this is major comes from the devided by ZERO.")>
    <Extension> Public Function Is_NA_UHandle(n As Double) As Boolean
#Else
    <Extension> Public Function Is_NA_UHandle(n As Double) As Boolean
#End If
        Return Double.IsNaN(n) OrElse Double.IsInfinity(n) OrElse Double.IsNegativeInfinity(n) OrElse Double.IsPositiveInfinity(n)
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Fuzzy match two string, this is useful for the text query or searching.
    ''' </summary>
    ''' <param name="Query"></param>
    ''' <param name="Subject"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Command("Fuzzy_Match")>
    <Extension> Public Function FuzzyMatching(Query As String, Subject As String) As Boolean
        Return FuzzyMatchString.Equals(Query, Subject)
    End Function
#End If

    Private ReadOnly BooleanValues As Dictionary(Of String, Boolean) = New Dictionary(Of String, Boolean) From
        {
            {"t", True}, {"true", True}, {"1", True}, {"y", True}, {"yes", True},
            {"f", False}, {"false", False}, {"0", False}, {"n", False}, {"no", False}
        }

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Convert the string value into the boolean value, this is useful to the text format configuration file into data model.
    ''' </summary>
    ''' <param name="strValue"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Command("Get.boolean")>
    <Extension> Public Function get_BooleanValue(strValue As String) As Boolean
#Else
    <Extension> Public Function get_BooleanValue(strValue As String) As Boolean
#End If
        If String.IsNullOrEmpty(strValue) Then
            Return False
        End If

        strValue = strValue.ToLower.Trim
        If BooleanValues.ContainsKey(key:=strValue) Then
            Return BooleanValues(strValue)
        Else
            Return False
        End If
    End Function

#If FRAMEWORD_CORE Then
    <Command("Get.Binary")>
    <Extension> Public Function ToBinary([Date] As Date) As Long
#Else
    <Extension> Public Function ToBinary([Date] As Date) As Long
#End If
        Return [Date].Year * 100000 + [Date].Month * 10000 + [Date].Day * 1000 +
                [Date].Hour * 100 + [Date].Minute * 10 + [Date].Second
    End Function

#If FRAMEWORD_CORE Then
    <Command("get_item")>
    <Extension> Public Function GetItem(Of T)(Collection As Generic.IEnumerable(Of T), index As Integer) As T
#Else
    <Extension> Public Function GetItem(Of T)(Collection As Generic.IEnumerable(Of T), index As Integer) As T
#End If
        If Collection.IsNullOrEmpty OrElse index >= Collection.Count Then
            Return Nothing
        Else
            Return Collection(index)
        End If
    End Function

    ''' <summary>
    ''' 求取该数据集的标准差
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function StdError(data As Generic.IEnumerable(Of Double)) As Double
        Dim Average As Double = data.Average
        Dim Sum = (From n As Double In data Select (n - Average) ^ 2).ToArray.Sum
        Sum /= data.Count
        Return Global.System.Math.Sqrt(Sum)
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Count the string value numbers.(请注意，这个函数是倒序排序的)
    ''' </summary>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Command("string.count_tokens")>
    <Extension> Public Function CountStringTokens(Collection As Generic.IEnumerable(Of String), Optional IgnoreCase As Boolean = False) As KeyValuePair(Of String, Integer)()
#Else
    <Extension> Public Function CountStringTokens(Collection As Generic.IEnumerable(Of String), Optional IgnoreCase As Boolean = False) As KeyValuePair(Of String, Integer)()
#End If

        If Not IgnoreCase Then '大小写敏感
            Dim GroupList = (From s As String In Collection Select s Group s By s Into Group).ToArray
            Dim ChunkBuffer = (From item In GroupList Select data = New KeyValuePair(Of String, Integer)(item.s, item.Group.Count) Order By data.Value Descending).ToArray
            Return ChunkBuffer
        End If

        Dim Uniques = (From s As String
                       In (From strValue As String In Collection Select strValue Distinct).ToArray
                       Let data As String = s
                       Select UNIQUE_KEY = s.ToLower, data
                       Group By UNIQUE_KEY Into Group).ToArray
        Dim ChunkList As List(Of KeyValuePair(Of String, Integer)) = New List(Of KeyValuePair(Of String, Integer))

        Dim LQuery = (From UniqueString In Uniques
                      Let s As String = UniqueString.UNIQUE_KEY
                      Let Count As Integer = (From strValue As String In Collection Where String.Equals(strValue, s, StringComparison.OrdinalIgnoreCase) Select 1).ToArray.Count
                      Let ori = (From nn In UniqueString.Group Select nn.data).ToArray
                      Let DataItem As KeyValuePair(Of String, Integer) = New KeyValuePair(Of String, Integer)(ori((UniqueString.Group.Count - 1) * Rnd()).ToString, Count)
                      Select DataItem
                      Order By DataItem.Value Descending).ToArray
        Return LQuery
    End Function

#If FRAMEWORD_CORE Then
    <Command("get.properties")>
    <Extension> Public Function GetReadWriteProperties(type As System.Type) As System.Reflection.PropertyInfo()
#Else
    <Extension> Public Function GetReadWriteProperties(type As System.Type) As System.Reflection.PropertyInfo()
#End If
        Dim LQuery = (From p In type.GetProperties Where p.CanRead AndAlso p.CanWrite Select p).ToArray
        Return LQuery
    End Function

    Public Const Scan0 As Integer = 0

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Get the description data from a enum type value, if the target have no <see cref="DescriptionAttribute"></see> attribute data 
    ''' then function will return the string value from the ToString() function.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Command("get.description")>
    <Extension> Public Function Description(e As [Enum]) As String
#Else
    ''' <summary>
    ''' Get the description data from a enum type value, if the target have no <see cref="DescriptionAttribute"></see> attribute data 
    ''' then function will return the string value from the ToString() function.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Description(e As [Enum]) As String
#End If
        Dim Type As Type = e.GetType()
        Dim MemInfos As MemberInfo() = Type.GetMembers(e.ToString)

        If MemInfos Is Nothing OrElse MemInfos.Length > 0 Then
            Return e.ToString
        End If

        Dim CustomAttrs As Object() =
            MemInfos(Scan0).GetCustomAttributes(GetType(DescriptionAttribute), inherit:=False)

        If CustomAttrs Is Nothing AndAlso CustomAttrs.Length > 0 Then
            Return CType(CustomAttrs.First, DescriptionAttribute).Description
        Else
            Return e.ToString
        End If
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Remove all of the null object in the target object collection
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Command("trim_nullvalue")>
    <Extension> Public Function TrimNull(Of T As Class)(Collection As Generic.IEnumerable(Of T)) As T()
#Else
    ''' <summary>
    ''' Remove all of the null object in the target object collection
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function TrimNull(Of T As Class)(Collection As Generic.IEnumerable(Of T)) As T()
#End If
        If Collection.IsNullOrEmpty Then
            Return New T() {}
        Else
            Return (From item In Collection Where Not item Is Nothing Select item).ToArray
        End If
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Return a collection with randomize element position in <paramref name="Collection">the original collection</paramref>.(从原有序序列中获取一个随机元素的序列)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Command("elements.randomize")>
    <Extension> Public Function RandomizeElements(Of T)(Collection As Generic.IEnumerable(Of T)) As T()
#Else
    ''' <summary>
    ''' Return a collection with randomize element position in <paramref name="Collection">the original collection</paramref>.(从原有序序列中获取一个随机元素的序列)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function RandomizeElements(Of T)(Collection As Generic.IEnumerable(Of T)) As T()
#End If
        Dim ChunkBuffer As T() = New T(Collection.Count - 1) {}
        Dim TempList = Collection.ToList

        For i As Integer = 0 To ChunkBuffer.Count - 1
            Dim index As Integer = RandomDouble() * (TempList.Count - 1)
            ChunkBuffer(i) = TempList(index)
            Call TempList.RemoveAt(index)
        Next

        Return ChunkBuffer
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Get a specific item value from the target collction data using its UniqueID property，
    ''' (请注意，请尽量不要使用本方法，因为这个方法的效率有些低，对于获取<see cref="Microsoft.VisualBasic.ComponentModel.Collection.Generic.IAccessionIdEnumerable">
    ''' </see>类型的集合之中的某一个对象，请尽量先转换为字典对象，在使用该字典对象进行查找以提高代码效率，使用本方法的优点是可以选择忽略<paramref name="UniqueId">
    ''' </paramref>参数之中的大小写，以及对集合之中的存在相同的Key的这种情况的容忍)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <param name="UniqueId"></param>
    ''' <param name="IgnoreCase"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Command("get_item")>
    <Extension> Public Function GetItem(Of T As Microsoft.VisualBasic.ComponentModel.Collection.Generic.IAccessionIdEnumerable)( _
        Collection As Generic.IEnumerable(Of T), UniqueId As String, Optional IgnoreCase As StringComparison = StringComparison.Ordinal) As T

        Dim LQuery = (From item In Collection Where String.Equals(UniqueId, item.UniqueId, IgnoreCase) Select item).ToArray
        If Not LQuery.IsNullOrEmpty Then
            Return LQuery.First
        Else
            Return Nothing
        End If
    End Function
#End If

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Get the value of the first element, if the collection is null or empty then return nothing as default value
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Command("get.firstvalue")>
    <Extension> Public Function GetFirstValue(Of T)(Collection As Generic.IEnumerable(Of T)) As T
#Else
    ''' <summary>
    ''' Get the value of the first element, if the collection is null or empty then return nothing as default value
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension> Public Function GetFirstValue(Of T)(Collection As Generic.IEnumerable(Of T)) As T
#End If
        If Collection.IsNullOrEmpty Then
            Return Nothing
        Else
            Return Collection.First
        End If
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Copy the value in <paramref name="value"></paramref> into target variable <paramref name="target"></paramref> and then return the target variable.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="value"></param>
    ''' <param name="target"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Command("value.copy")>
    <Extension> Public Function CopyTo(Of T)(value As T, ByRef target As T) As T
#Else
    ''' <summary>
    ''' Copy the value in <paramref name="value"></paramref> into target variable <paramref name="target"></paramref> and then return the target variable.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="value"></param>
    ''' <param name="target"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CopyTo(Of T)(value As T, ByRef target As T) As T
#End If
        target = value
        Return value
    End Function

#If FRAMEWORD_CORE Then
    <Command("move_next")>
    <Extension> Public Function MoveNext(ByRef p As Long) As Long
#Else
    <Extension> Public Function MoveNext(ByRef p As Long) As Long
#End If
        Dim value = p
        p += 1
        Return value
    End Function

    ''' <summary>
    ''' 随机的在目标集合中选取指定数目的子集合
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <param name="Counts">当目标数目大于或者等于目标集合的数目的时候，则返回目标集合</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function TakeRandomly(Of T)(Collection As Generic.IEnumerable(Of T), Counts As Integer) As T()
        If Counts >= Collection.Count Then
            Return Collection
        Else
            Dim chunkBuffer As T() = New T(Counts - 1) {}
            Dim OriginalList = Collection.ToList
            For i As Integer = 0 To Counts - 1
                Dim Index = RandomDouble() * (OriginalList.Count - 1)
                chunkBuffer(i) = OriginalList(Index)
                Call OriginalList.RemoveAt(Index)
            Next

            Return chunkBuffer
        End If
    End Function

    ''' <summary>
    ''' Convert target object type collection into a string array using the Object.ToString() interface function.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToStringArray(Of T)(Collection As Generic.IEnumerable(Of T)) As String()
        Dim LQuery = (From item In Collection Let strItem As String = item.ToString Select strItem).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' Get a sub set of the string data which is contains in both collection <paramref name="strArray1"></paramref> and <paramref name="strArray2"></paramref>
    ''' </summary>
    ''' <param name="strArray1"></param>
    ''' <param name="strArray2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Union(strArray1 As String(), strArray2 As String()) As String()
        Dim LQuery = (From strItem As String In strArray1 Where Array.IndexOf(strArray2, strItem) > -1 Select strItem).ToArray
        Return LQuery
    End Function

#If FRAMEWORD_CORE Then
    <Command("swap")>
    Public Sub Swap(Of T)(ByRef obj1 As T, ByRef obj2 As T)
#Else
    Public Sub Swap(Of T)(ByRef obj1 As T, ByRef obj2 As T)
#End If
        Dim objTemp As T = obj1
        obj1 = obj2
        obj2 = objTemp
    End Sub

    ''' <summary>
    ''' Swap the value in the two variables.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj1"></param>
    ''' <param name="obj2"></param>
    ''' <remarks></remarks>
    <Extension> Public Sub SwapWith(Of T)(ByRef obj1 As T, ByRef obj2 As T)
        Dim objTemp As T = obj1
        obj1 = obj2
        obj2 = objTemp
    End Sub

    ''' <summary>
    ''' Swap the two item position in the target <paramref name="List">list</paramref>.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="List"></param>
    ''' <param name="obj_1"></param>
    ''' <param name="obj_2"></param>
    <Extension> Public Sub SwapItem(Of T As Class)(ByRef List As List(Of T), obj_1 As T, obj_2 As T)
        Dim idx_1 As Integer = List.IndexOf(obj_1)
        Dim idx_2 As Integer = List.IndexOf(obj_2)

        If idx_1 = -1 OrElse idx_2 = -1 Then
            Return
        End If

        Call List.RemoveAt(idx_1)
        Call List.Insert(idx_1, obj_2)
        Call List.RemoveAt(idx_2)
        Call List.Insert(idx_2, obj_2)
    End Sub

#If FRAMEWORD_CORE Then
    <Command("trim")>
    <Extension> Public Function TrimA(strText As String, Optional VbCRLF_Replace As String = " ") As String
#Else
    <Extension> Public Function TrimA(strText As String, Optional VbCRLF_Replace As String = " ") As String
#End If
        strText = strText.Replace(vbCrLf, VbCRLF_Replace).Replace(vbCr, VbCRLF_Replace).Replace(vbLf, VbCRLF_Replace)
        strText = strText.Replace("  ", " ")
        Return Trim(strText)
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 为列表中的对象添加对象句柄值
    ''' </summary>
    ''' <param name="Collection"></param>
    ''' <remarks></remarks>
    <Extension> Public Function [AddHandle](Of THandle As Microsoft.VisualBasic.ComponentModel.IHandler)(Collection As Generic.IEnumerable(Of THandle), Optional offset As Integer = 0) _
                As Generic.IEnumerable(Of THandle)
        For i As Integer = 0 To Collection.Count - 1
            Collection(i).Handle = i + offset
        Next
        Return Collection
    End Function
#End If

    ''' <summary>
    ''' <paramref name="p"></paramref> plus one and then return its previous value.
    ''' </summary>
    ''' <param name="p"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function MoveNext(ByRef p As Integer) As Integer
        Dim i As Integer = p
        p += 1
        Return i
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 获取某一个集合的下标的集合
    ''' </summary>
    ''' <typeparam name="T">集合中的元素为任意类型的</typeparam>
    ''' <param name="Collection">目标集合对象</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Command("get.sequence")>
    <Extension> Public Function Sequence(Of T)(Collection As Generic.IEnumerable(Of T), Optional offset As Integer = 0) As Integer()
#Else
    ''' <summary>
    ''' 获取某一个集合的下标的集合
    ''' </summary>
    ''' <typeparam name="T">集合中的元素为任意类型的</typeparam>
    ''' <param name="Collection">目标集合对象</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension> Public Function Sequence(Of T)(Collection As Generic.IEnumerable(Of T), Optional offset As Integer = 0) As Integer()
#End If
        If Collection Is Nothing OrElse Collection.Count = 0 Then
            Return New Integer() {}
        Else
            Dim List(Collection.Count - 1) As Integer
            For i As Integer = 0 To List.Count - 1
                List(i) = i + offset
            Next
            Return List
        End If
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <param name="IndexCollection">所要获取的目标对象的下表的集合</param>
    ''' <param name="reversedSelect">是否为反向选择</param>
    ''' <param name="OffSet">当进行反选的时候，本参数将不会起作用</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Command("takes")>
    <Extension> Public Function Takes(Of T)(Collection As Generic.IEnumerable(Of T), IndexCollection As Integer(), Optional OffSet As Integer = 0, Optional reversedSelect As Boolean = False) As T()
#Else
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <param name="IndexCollection">所要获取的目标对象的下表的集合</param>
    ''' <param name="reversedSelect">是否为反向选择</param>
    ''' <param name="OffSet">当进行反选的时候，本参数将不会起作用</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension> Public Function Takes(Of T)(Collection As Generic.IEnumerable(Of T), IndexCollection As Integer(), Optional OffSet As Integer = 0, Optional reversedSelect As Boolean = False) As T()
#End If
        If reversedSelect Then
            Return InternalReversedTakeSelected(Collection, IndexCollection)
        End If

        Dim result As T()

        If OffSet = 0 Then
            result = (From idx As Integer In IndexCollection Select Collection(idx)).ToArray
        Else
            result = (From idx As Integer In IndexCollection Select Collection(idx + OffSet)).ToArray
        End If
        Return result
    End Function

    ''' <summary>
    ''' 反选，即将所有不出现在<paramref name="indexs"></paramref>之中的元素都选取出来
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="coll"></param>
    ''' <param name="indexs"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function InternalReversedTakeSelected(Of T)(coll As Generic.IEnumerable(Of T), indexs As Integer()) As T()
        Dim result As T() = (From i As Integer In coll.Sequence Where Array.IndexOf(indexs, i) = -1 Select coll(i)).ToArray
        Return result
    End Function

    ''' <summary>
    ''' 产生指定数目的一个递增序列
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Sequence(n As Integer) As Integer()
        Dim List(n - 1) As Integer
        For i As Integer = 0 To n - 1
            List(i) = i
        Next
        Return List
    End Function

    ''' <summary>
    ''' 产生指定数目的一个递增序列
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Sequence(n As Long) As Long()
        Dim List As Long() = New Long(n - 1) {}
        For i As Integer = 0 To n - 1
            List(i) = i
        Next
        Return List
    End Function

    ''' <summary>
    ''' 产生指定数目的一个递增序列
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Sequence(n As UInteger) As Integer()
        Dim List(n - 1) As Integer
        For i As Integer = 0 To n - 1
            List(i) = i
        Next
        Return List
    End Function

    ''' <summary>
    ''' 向一个列表对象之中批量添加一个对象的集合
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="List"></param>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Append(Of T)(List As List(Of T), Collection As Generic.IEnumerable(Of T)) As Integer
        Call List.AddRange(Collection)
        Return 0
    End Function

    ''' <summary>
    ''' 将目标键值对对象的集合转换为一个字典对象
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function ToDictionary(Of TKey, TValue)(Collection As Generic.IEnumerable(Of KeyValuePair(Of TKey, TValue))) As Dictionary(Of TKey, TValue)
        Dim Dictionary As Dictionary(Of TKey, TValue) = New Dictionary(Of TKey, TValue)
        For i As Integer = 0 To Collection.Count - 1
            Call Dictionary.Add(Collection(i).Key, Collection(i).Value)
        Next
        Return Dictionary
    End Function

    ''' <summary>
    ''' This object collection is a null object or contains zero count items.(判断某一个对象集合是否为空)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Collection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function IsNullOrEmpty(Of T)(Collection As Generic.IEnumerable(Of T)) As Boolean
        Return Collection Is Nothing OrElse Collection.Count = 0
    End Function

    <Extension> Public Function IsNullOrEmpty(stringCollection As Generic.IEnumerable(Of String), strict As Boolean) As Boolean
        If strict Then
            If stringCollection.IsNullOrEmpty Then
                Return True
            Else
                Return (stringCollection.Count = 1 AndAlso String.IsNullOrEmpty(stringCollection.First))
            End If
        Else
            Return stringCollection.IsNullOrEmpty
        End If
    End Function

    ''' <summary>
    ''' Write the text file data into a file which was specific by the <paramref name="Path"></paramref> value, this function not append the new data onto the target file.
    ''' (将目标文本字符串写入到一个指定路径的文件之中，但是不会在文件末尾追加新的数据)
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <param name="Text"></param>
    ''' <param name="Encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function SaveTo(Text As String, Path As String, Optional Encoding As System.Text.Encoding = Nothing) As Boolean
        If String.IsNullOrEmpty(Path) Then Return False
        If Encoding Is Nothing Then Encoding = System.Text.Encoding.Default
        Dim Dir = FileIO.FileSystem.GetParentPath(Path)
        If String.IsNullOrEmpty(Dir) Then
            Dir = My.Computer.FileSystem.CurrentDirectory
        End If
        Call FileIO.FileSystem.CreateDirectory(Dir)
        Call FileIO.FileSystem.WriteAllText(Path, Text, append:=False, encoding:=Encoding)
        Return True
    End Function

    ''' <summary>
    ''' 将目标字符串数据全部写入到文件之中，当所写入的文件位置之上没有父文件夹存在的时候，会自动创建文件夹
    ''' </summary>
    ''' <param name="array"></param>
    ''' <param name="path"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function SaveTo(array As Generic.IEnumerable(Of String), path As String, Optional encoding As System.Text.Encoding = Nothing) As Boolean
        If String.IsNullOrEmpty(path) Then Return False
        If encoding Is Nothing Then encoding = System.Text.Encoding.Default
        Dim Dir = FileIO.FileSystem.GetParentPath(path)
        Call FileIO.FileSystem.CreateDirectory(Dir)
        Call IO.File.WriteAllLines(path, array, encoding)
        Return True
    End Function

    ''' <summary>
    ''' 将一个类对象序列化为XML文档
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="e"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetXml(Of T As Class)(e As T) As String
        Dim sBuilder As StringBuilder = New StringBuilder(1024)
        Using Stream As New System.IO.StringWriter(sb:=sBuilder)
#If DEBUG Then
            Try
                Call (New System.Xml.Serialization.XmlSerializer(GetType(T))).Serialize(Stream, e)
            Catch ex As Exception
                Call Console.WriteLine(ex.ToString)
                FileIO.FileSystem.WriteAllText(My.Computer.FileSystem.SpecialDirectories.Temp & "/error.log", ex.ToString & vbCrLf & vbCrLf, append:=True)
                Throw
            End Try
#Else
            Call (New System.Xml.Serialization.XmlSerializer(GetType(T))).Serialize(Stream, e)
#End If
            Return sBuilder.ToString
        End Using
    End Function

    Private ReadOnly currentPid As Integer = Process.GetCurrentProcess.Id

    <DllImport("kernel32.dll", EntryPoint:="CopyFileW", CharSet:=CharSet.Unicode, ExactSpelling:=False)> _
    Public Function CopyFile(ByVal lpExistingFilename As String, ByVal lpNewFileName As String, ByVal bFailIfExists As Boolean) As Boolean
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="XmlFile">XML文件的文件路径</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function LoadXml(Of T As Class)(XmlFile As String, Optional encoding As System.Text.Encoding = Nothing) As T
        If encoding Is Nothing Then encoding = System.Text.Encoding.Default
        'If Len(Xml) > 256 Then
        '    Call Randomize()
        '    Dim TempFile As String = String.Join("/", My.Computer.FileSystem.SpecialDirectories.Temp, currentPid, Rnd() & ".tmp")
        '    Call FileIO.FileSystem.CreateDirectory(FileIO.FileSystem.GetParentPath(TempFile))
        '    TempFile = TempFile.Replace("\", "\\").Replace("/", "\\")
        '    Call CopyFile(Xml, TempFile, False)
        '    FileContent = FileIO.FileSystem.ReadAllText(file:=TempFile, encoding:=encoding)
        'Else
        '    FileContent = FileIO.FileSystem.ReadAllText(file:=Xml, encoding:=encoding)
        'End If

        If Not FileIO.FileSystem.FileExists(XmlFile) Then
            Return Nothing
        End If

        Dim FileContent As String = FileIO.FileSystem.ReadAllText(XmlFile)

        Using Stream As New System.IO.StringReader(s:=FileContent)
#If DEBUG Then
            Try
                Dim Type = GetType(T)
                Dim Data = New System.Xml.Serialization.XmlSerializer(Type).Deserialize(Stream)
                Return DirectCast(Data, T)
            Catch ex As Exception
                FileIO.FileSystem.WriteAllText(My.Computer.FileSystem.SpecialDirectories.Temp & "/error.log", ex.ToString & vbCrLf & vbCrLf, append:=True)
                Throw
            End Try
#Else
            Return DirectCast(New System.Xml.Serialization.XmlSerializer(GetType(T)).Deserialize(Stream), T)
#End If
        End Using
    End Function

    ''' <summary>
    ''' 使用二进制序列化保存一个对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Obj"></param>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Serialize(Of T As Class)(Obj As T, path As String) As Integer
        Dim IFormatter As IFormatter = New BinaryFormatter()
        Dim Stream As Stream = New IO.FileStream(path, IO.FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)

        Try
            Call IFormatter.Serialize(Stream, Obj)
        Catch ex As Exception
            Return -1
        End Try
        Call Stream.Close()
        Return 0
    End Function

    ''' <summary>
    ''' 使用反二进制序列化从指定的文件之中加载一个对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Load(Of T As Class)(path As String) As T
        If Not FileIO.FileSystem.FileExists(path) Then
            Return DirectCast(Activator.CreateInstance(Of T)(), T)
        End If
        Using Stream As Stream = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim IFormatter As IFormatter = New BinaryFormatter()
            Dim obj As T = DirectCast(IFormatter.Deserialize(Stream), T)
            Return obj
        End Using
    End Function

    ''' <summary>
    ''' 使用一个XML文本内容创建一个XML映射对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Xml">是Xml文件的文件内容而非文件路径</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CreateObjectFromXml(Of T As Class)(Xml As String) As T
        Using Stream As New System.IO.StringReader(s:=Xml)

#If DEBUG Then
            Try
                Dim Type = GetType(T)
                Dim Data = New System.Xml.Serialization.XmlSerializer(Type).Deserialize(Stream)
                Return DirectCast(Data, T)
            Catch ex As Exception
                FileIO.FileSystem.WriteAllText(My.Computer.FileSystem.SpecialDirectories.Temp & "/error.log", ex.ToString & vbCrLf & vbCrLf, append:=True)
                Throw
            End Try
#Else
            Return DirectCast(New System.Xml.Serialization.XmlSerializer(GetType(T)).Deserialize(Stream), T)
#End If
        End Using
    End Function

    ''' <summary>
    ''' 使用一个XML文本内容的一个片段创建一个XML映射对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Xml">是Xml文件的文件内容而非文件路径</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CreateObjectFromXmlSegment(Of T As Class)(Xml As String) As T
        Using Stream As New System.IO.StringReader(s:="<?xml version=""1.0""?>" & vbCrLf & Xml)
            Return DirectCast(New System.Xml.Serialization.XmlSerializer(GetType(T)).Deserialize(Stream), T)
        End Using
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' 执行一个命令行语句，并返回一个IO重定向对象，以获取被执行的目标命令的标准输出
    ''' </summary>
    ''' <param name="CommandLine"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Command("shell")>
    <Extension> Public Function Shell(CommandLine As String) As Microsoft.VisualBasic.CommandLine.IORedirect
        Return CType(CommandLine, Microsoft.VisualBasic.CommandLine.IORedirect)
    End Function
#End If

    ''' <summary>
    ''' 获取一个实数集合中所有元素的积
    ''' </summary>
    ''' <param name="Elements"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function π(Elements As Generic.IEnumerable(Of Double)) As Double
        If Elements.IsNullOrEmpty Then
            Return 0
        End If

        Dim result As Double = 1
        For i As Integer = 0 To Elements.Count - 1
            result *= Elements(i)
        Next

        Return result
    End Function

    <Extension> Public Function Match(input As String, pattern As String, Optional options As System.Text.RegularExpressions.RegexOptions = RegularExpressions.RegexOptions.Multiline) As String
        Return System.Text.RegularExpressions.Regex.Match(input, pattern, options).Value
    End Function

    <Extension> Public Function Match(input As System.Text.RegularExpressions.Match, pattern As String, Optional options As System.Text.RegularExpressions.RegexOptions = RegularExpressions.RegexOptions.Multiline) As String
        Return System.Text.RegularExpressions.Regex.Match(input.Value, pattern, options).Value
    End Function

#If FRAMEWORD_CORE Then

    <Extension> Public Sub ClearParameters(Of InteropService As Microsoft.VisualBasic.CommandLine.CommandLine.InteropService)(Instance As CommandLine.CommandLine.InteropService)
        Call CommandLine.Reflection.[Optional].ClearParameters(Instance)
    End Sub

    ''' <summary>
    ''' Fill the newly created image data with the specific color brush
    ''' </summary>
    ''' <param name="Image"></param>
    ''' <param name="FilledColor"></param>
    ''' <remarks></remarks>
    <Extension> Public Sub FillBlank(ByRef Image As System.Drawing.Image, FilledColor As System.Drawing.Brush)
        If Image Is Nothing Then
            Return
        End If
        Using gr As Graphics = Graphics.FromImage(Image)
            Dim R As System.Drawing.Rectangle = New Rectangle(New Point, Image.Size)
            Call gr.FillRectangle(FilledColor, R)
        End Using
    End Sub
#End If

    ''' <summary>
    ''' 枚举所有非法的路径字符
    ''' </summary>
    ''' <remarks></remarks>
    Public Const ILLEGAL_PATH_CHARACTERS_ENUMERATION As String = "\/:*?""<>|"

    ''' <summary>
    ''' 将目标字符串之中的非法的字符替换为"_"符号以成为正确的文件名字符串
    ''' </summary>
    ''' <param name="str"></param>
    ''' <param name="OnlyASCII">当本参数为真的时候，仅26个字母，0-9数字和下划线_以及小数点可以被保留下来</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function NormalizePathString(str As String, Optional OnlyASCII As Boolean = True) As String
        Dim sBuilder As StringBuilder = New StringBuilder(str)
        For Each ch As Char In ILLEGAL_PATH_CHARACTERS_ENUMERATION
            Call sBuilder.Replace(ch, "_")
        Next

        If OnlyASCII Then
            For Each ch As Char In "()[]+-~!@#$%^&=;',."
                Call sBuilder.Replace(ch, "_")
            Next
        End If

        Return sBuilder.ToString
    End Function

    ''' <summary>
    ''' Remove all of the element in the <paramref name="collection"></paramref> from target <paramref name="List">list</paramref>
    ''' </summary> 
    ''' <typeparam name="T"></typeparam>
    ''' <param name="List"></param>
    ''' <param name="collection"></param>
    ''' <remarks></remarks>
    <Extension> Public Sub Removes(Of T)(ByRef List As List(Of T), collection As Generic.IEnumerable(Of T))
        For Each obj In collection
            Call List.Remove(obj)
        Next
    End Sub

    Const PAGE_CONTENT_TITLE As String = "<title>.+</title>"

    ''' <summary>
    ''' 获取两个尖括号之间的内容
    ''' </summary>
    ''' <param name="strData"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetValue(strData As String) As String
        Dim p = InStr(strData, ">") + 1
        Dim q = InStrRev(strData, "<")
        strData = Mid(strData, p, q - p)
        Return strData
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Get the html page content from a website request or a html file on the local filesystem.
    ''' </summary>
    ''' <param name="url">web http request url or a file path handle</param>
    ''' <param name="RequestTimeOut">发生错误的时候的重试的次数</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Command("webpage.request")>
    <Extension> Public Function Get_PageContent(url As String, Optional RequestTimeOut As UInteger = 20, Optional FileSystemUrl As Boolean = False) As String
#Else
    ''' <summary>
    ''' Get the html page content from a website request or a html file on the local filesystem.
    ''' </summary>
    ''' <param name="url">web http request url or a file path handle</param>
    ''' <param name="RequestTimeOut">发生错误的时候的重试的次数</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <Extension> Public Function Get_PageContent(url As String, Optional RequestTimeOut As UInteger = 20, Optional FileSystemUrl As Boolean = False) As String
#End If
        Dim Timer As Stopwatch = New Stopwatch, RequestTime As Integer = 0
        Call Timer.Start()
        Call Console.WriteLine("Request data from: " & url)

        If FileIO.FileSystem.FileExists(url) Then
            Return FileIO.FileSystem.ReadAllText(url)
        Else
            If FileSystemUrl Then
                Call Console.WriteLine("url {0} can not be solved on your filesystem!", url)
                Return ""
            End If
        End If


#If FRAMEWORD_CORE Then
        Using Process As ConsoleDevice.ConsoleProcessIndicator = New ConsoleDevice.ConsoleProcessIndicator
#End If

        Try
RETRY:      Call Console.WriteLine("Waiting for the server reply..")
#If FRAMEWORD_CORE Then
            Call Process.Start()
#End If
            Dim WebRequest As System.Net.HttpWebRequest = System.Net.HttpWebRequest.Create(url)
            Dim WebResponse As WebResponse = WebRequest.GetResponse

            Using respStream As Stream = WebResponse.GetResponseStream, ioStream As StreamReader = New StreamReader(respStream)
                Dim pageContent As String = ioStream.ReadToEnd
                Dim Title = Regex.Match(pageContent, PAGE_CONTENT_TITLE, RegexOptions.IgnoreCase).Value
                If String.IsNullOrEmpty(Title) Then
                    Title = "NULL_TITLE"
                Else
                    Title = Mid(Title, 8, Len(Title) - 15)
                End If
                Call Console.WriteLine("[WebRequst Handler Get Response Data] --> {0}" & vbCrLf &
                                       "   Package Size  :=  {1} bytes" & vbCrLf &
                                       "   Response time :=  {2} ms", Title, Len(pageContent), Timer.ElapsedMilliseconds)

#If DEBUG Then
                Call FileIO.FileSystem.WriteAllText("./DEBUG.tmp", pageContent, False)
#End If

                Return pageContent
            End Using
        Catch ex As Exception

            If RequestTime < RequestTimeOut Then
                RequestTime += 1
                Call Console.WriteLine("Data downloading error, retry connect to the server!")
                GoTo RETRY
            Else
                Dim exMessage As String = String.Format("Unable to get the http request!" & vbCrLf &
                                                        "  Url:=[{0}]" & vbCrLf &
                                                        "  EXCEPTION =====>" & vbCrLf & ex.ToString, url)

                Call Console.WriteLine(exMessage)

#If FRAMEWORD_CORE Then
                    Using log = New Logging.LogFile("http.request_exception.log")
                        Call log.WriteLine(exMessage, "HTTP_REQUEST_EXCEPTION", Type:=Logging.LogFile.MsgTypes.ERR, WriteToScreen:=False)
                    End Using
#End If
            End If
        End Try

#If FRAMEWORD_CORE Then
        End Using
#End If
        Return ""
    End Function


#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' download the file from <paramref name="strUrl"></paramref> to <paramref name="SavedPath">local file</paramref>.
    ''' </summary>
    ''' <param name="strUrl"></param>
    ''' <param name="SavedPath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Command("wget")>
    <Extension> Public Function DownloadFile(strUrl As String, SavedPath As String) As Boolean
#Else
    ''' <summary>
    ''' download the file from <paramref name="strUrl"></paramref> to <paramref name="SavedPath">local file</paramref>.
    ''' </summary>
    ''' <param name="strUrl"></param>
    ''' <param name="SavedPath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function DownloadFile(strUrl As String, SavedPath As String) As Boolean
#End If
        Try
            Dim dwl As New System.Net.WebClient()
            Call dwl.DownloadFile(strUrl, SavedPath)
            Call dwl.Dispose()
            Return True
        Catch ex As Exception
            Call FileIO.FileSystem.WriteAllText(My.Computer.FileSystem.SpecialDirectories.Temp & "\WebClientHandler_error.log", ex.ToString & vbCrLf & vbCrLf, append:=True)
            Return False
        End Try
    End Function
End Module
