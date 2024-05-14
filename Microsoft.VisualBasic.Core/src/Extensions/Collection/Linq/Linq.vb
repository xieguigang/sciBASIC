#Region "Microsoft.VisualBasic::b1c35d49b0e00853172c7d6059b61c65, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\Linq.vb"

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

    '   Total Lines: 472
    '    Code Lines: 265
    ' Comment Lines: 160
    '   Blank Lines: 47
    '     File Size: 18.06 KB


    '     Module Extensions
    ' 
    '         Function: DATA, Populate, (+2 Overloads) SafeQuery, ToArray
    '         Delegate Sub
    ' 
    '             Function: (+2 Overloads) [With], CopyVector, DefaultFirst, FirstOrDefault, LastOrDefault
    '                       MaxInd, (+2 Overloads) Read, RemoveLeft, (+2 Overloads) Removes, Repeats
    '                       (+2 Overloads) SeqIterator, (+4 Overloads) Sequence, (+3 Overloads) ToArray, ToVector, TryCatch
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Linq

    ''' <summary>
    ''' Linq Helpers.(为了方便编写Linq代码而构建的一个拓展模块)
    ''' </summary>
    <Package("LINQ", Category:=APICategories.UtilityTools)>
    <Extension>
    <HideModuleName>
    Public Module Extensions

        ''' <summary>
        ''' Parallel helper
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="parallel"></param>
        ''' <param name="degreeOfParallelism%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Populate(Of T)(source As IEnumerable(Of T), parallel As Boolean, Optional degreeOfParallelism% = -1) As IEnumerable(Of T)
            If parallel Then
                If degreeOfParallelism > 1 Then
                    Return source.AsParallel.WithDegreeOfParallelism(degreeOfParallelism)
                Else
                    Return source.AsParallel
                End If
            Else
                Return source
            End If
        End Function

        Public Function DATA(Of T)(src As IEnumerable(Of T)) As DataValue(Of T)
            Return New DataValue(Of T)(src)
        End Function

        ''' <summary>
        ''' DirectCast of the <paramref name="source"/> sequence into T() array.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ToArray(Of T)(source As IEnumerable(Of Object)) As T()
            If source Is Nothing Then
                Return New T() {}
            Else
                Return (From x As Object In source Select DirectCast(x, T)).ToArray
            End If
        End Function

        ''' <summary>
        ''' A query proxy function makes your linq not so easily crashed due to the 
        ''' unexpected null reference collection as linq source.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        <Extension>
        Public Function SafeQuery(Of T)(source As IEnumerable(Of T), <CallerMemberName> Optional trace$ = Nothing) As IEnumerable(Of T)
            If Not source Is Nothing Then
                Return source
            Else
#If DEBUG Then
                Call $"Target source sequence is nothing...[{trace}]".Warning
#End If
                Return {}
            End If
        End Function

        <Extension>
        Public Function SafeQuery(Of T)(source As Enumeration(Of T)) As IEnumerable(Of T)
            If source Is Nothing Then
                Return {}
            Else
                Return source.AsEnumerable
            End If
        End Function

        Public Delegate Sub DoWith(Of T)(obj As T)

        ''' <summary>
        ''' <paramref name="doWith"/> each element in <paramref name="source"/> and then 
        ''' returns the <paramref name="source"/> sequence after modify.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="doWith"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function [With](Of T As Class)(source As IEnumerable(Of T), doWith As DoWith(Of T)) As IEnumerable(Of T)
            For Each x As T In source
                Call doWith(x)
                Yield x
            Next
        End Function

        ''' <summary>
        ''' <paramref name="doWith"/> target object <paramref name="x"/>, and then reutrns x
        ''' </summary>
        ''' <typeparam name="T">Only works for reference type</typeparam>
        ''' <param name="x"></param>
        ''' <param name="doWith"></param>
        ''' <returns></returns>
        <Extension>
        Public Function [With](Of T As Class)(x As T, doWith As DoWith(Of T)) As T
            Call doWith(x)
            Return x
        End Function

        ''' <summary>
        ''' Gets the max element its index in the collection
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension> Public Function MaxInd(Of T As IComparable)(source As IEnumerable(Of T)) As Integer
            Dim i As Integer = 0
            Dim m As T
            Dim mi As Integer

            For Each x As T In source
                If x.CompareTo(m) > 0 Then
                    m = x
                    mi = i
                End If

                i += 1
            Next

            Return mi
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="match">符合这个条件的所有的元素都将会被移除</param>
        ''' <returns></returns>
        <Extension> Public Function Removes(Of T)(source As IEnumerable(Of T), match As Func(Of T, Boolean), Optional parallel As Boolean = False) As T()
            Dim LQuery As T()
            If parallel Then
                LQuery = (From x In source.AsParallel Where Not match(x) Select x).ToArray
            Else
                LQuery = (From x In source Where Not match(x) Select x).ToArray
            End If
            Return LQuery
        End Function

        <Extension> Public Function Removes(Of T)(lst As List(Of T), match As Func(Of T, Boolean)) As List(Of T)
            If lst.IsNullOrEmpty Then
                Return New List(Of T)
            Else
                For Each x In lst.ToArray
                    If match(x) Then
                        Call lst.Remove(x)
                    End If
                Next

                Return lst
            End If
        End Function

        Public Function TryCatch(Of T)(source As Func(Of T),
                                       msg$,
                                       Optional throwEx As Boolean = True,
                                       Optional ByRef exception As Exception = Nothing) As T
            Try
                Return source()
            Catch ex As Exception
                ex = New Exception(msg, ex)
                exception = ex

                Call App.LogException(ex, source.ToString)

                If throwEx Then
                    Throw ex
                Else
                    Return Nothing
                End If
            End Try
        End Function

        ''' <summary>
        ''' Removes the specific key in the dicitonary and returns the last content.
        ''' (删除指定的键之后返回剩下的数据)
        ''' </summary>
        ''' <typeparam name="TKey"></typeparam>
        ''' <typeparam name="TValue"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="key"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RemoveLeft(Of TKey, TValue)(ByRef source As Dictionary(Of TKey, TValue), key As TKey) As Dictionary(Of TKey, TValue)
            With source
                If .ContainsKey(key) Then
                    Call .Remove(key)
                End If
            End With

            Return source
        End Function

        ''' <summary>
        ''' Copy <paramref name="source"/> <paramref name="times"/> times to construct a new vector.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="times"></param>
        ''' <returns>An array consist of source with n elements.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Repeats(Of T)(source As T, times%) As T()
            Return times.Sequence.Select(Function(x) source).ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="n"></param>
        ''' <param name="source">The object factory</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CopyVector(Of T)(n As Integer, source As Func(Of T)) As T()
            Return n.Sequence.Select(Function(x) source()).ToArray
        End Function

        <Extension> Public Function Read(Of T)(array As T(), ByRef i As Integer, ByRef out As T) As T
            out = array(i)
            i += 1
            Return out
        End Function

        ''' <summary>
        ''' Read source at element position <paramref name="i"/> and returns its value, 
        ''' and then this function makes position <paramref name="i"/> offset +1
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="array"></param>
        ''' <param name="i"></param>
        ''' <returns></returns>
        <Extension> Public Function Read(Of T)(array As T(), ByRef i As Integer) As T
            Dim out As T = array(i)
            i += 1
            Return out
        End Function

        ''' <summary>
        ''' ``[0, n-1]``, 产生指定数目的一个递增序列(用于生成序列的输入参数<paramref name="n"/>数值就是生成的数组的元素的个数)
        ''' </summary>
        ''' <param name="n">大于或者等于0的一个数，当小于0的时候会出错</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Sequence")>
        <Extension>
        Public Iterator Function Sequence(n As Integer) As IEnumerable(Of Integer)
            If n < 0 Then
                Dim ex As String = $"n:={n} is not a valid index generator value for sequence!"
                Throw New Exception(ex)
            End If

            For i As Integer = 0 To n - 1
                Yield i
            Next
        End Function

        ''' <summary>
        ''' ``0,1,2,3,...<paramref name="n"/>``
        ''' </summary>
        ''' <param name="n">the api function is already makes ``n-1`` for populate index sequence.</param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function SeqIterator(n As Integer, Optional offset As Integer = 0) As IEnumerable(Of Integer)
            If (n + offset) < 0 Then
                Dim ex As String = $"n:={n} is not a valid index generator value for sequence!"
                Throw New Exception(ex)
            End If

            For i As Integer = 0 To n - 1
                Yield (i + offset)
            Next
        End Function

        ''' <summary>
        ''' 假若数量已经超过了数组的容量，则需要使用这个函数来产生序列
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function SeqIterator(n As Long, Optional offset As Integer = 0) As IEnumerable(Of Long)
            If n < 0 Then
                Dim ex As String = $"n:={n} is not a valid index generator value for sequence!"
                Throw New Exception(ex)
            End If

            For i As Long = 0 To n - 1
                Yield (i + offset)
            Next
        End Function

        <Extension, ExportAPI("Sequence")>
        Public Function Sequence(n As Integer, offset As Integer) As Integer()
            Dim array As Integer() = n.Sequence.ToArray

            For i As Integer = 0 To array.Length - 1
                array(i) = array(i) + offset
            Next

            Return array
        End Function

        ''' <summary>
        ''' 产生指定数目的一个递增序列(所生成序列的数值就是生成的数组的元素的个数)
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Sequence")>
        <Extension>
        Public Function Sequence(n As Long) As Long()
            Dim List As Long() = New Long(n - 1) {}
            For i As Integer = 0 To n - 1
                List(i) = i
            Next
            Return List
        End Function

        ''' <summary>
        ''' 产生指定数目的一个递增序列(所生成序列的数值就是生成的数组的元素的个数)
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Sequence")>
        <Extension>
        Public Function Sequence(n As UInteger) As Integer()
            Dim List(n - 1) As Integer
            For i As Integer = 0 To n - 1
                List(i) = i
            Next
            Return List
        End Function

        ''' <summary>
        ''' (所生成序列的数值就是生成的数组的元素的个数)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="len"></param>
        ''' <param name="elementAt"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToArray(Of T)(len As Integer, elementAt As Func(Of Integer, T)) As T()
            Return len _
                .Sequence _
                .Select(elementAt) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToArray(Of T)(len&, elementAt As Func(Of Long, T)) As T()
            Return len _
                .Sequence _
                .Select(elementAt) _
                .ToArray
        End Function

        ''' <summary>
        ''' Returns the first element of a sequence, or a default value if the sequence contains no elements.
        ''' </summary>
        ''' <typeparam name="TSource">The type of the elements of source.</typeparam>
        ''' <param name="source">The System.Collections.Generic.IEnumerable`1 to return the first element of.</param>
        ''' <param name="[default]">
        ''' If the sequence is nothing or contains no elements, then this default value will be returned.
        ''' </param>
        ''' <returns>default(TSource) if source is empty; otherwise, the first element in source.</returns>
        <Extension>
        Public Function FirstOrDefault(Of TSource)(source As IEnumerable(Of TSource), [default] As TSource) As TSource
            If source Is Nothing Then
                Return [default]
            End If

            Dim value As TSource = source.FirstOrDefault

            If value Is Nothing Then
                Return [default]
            Else
                Return value
            End If
        End Function

        <Extension>
        Public Function LastOrDefault(Of T)(source As IEnumerable(Of T), [default] As T) As T
            Dim value As T = source.LastOrDefault

            If value Is Nothing Then
                Return [default]
            Else
                Return value
            End If
        End Function

        ''' <summary>
        ''' Returns the first element of a sequence, or a default value if the sequence contains no elements.
        ''' </summary>
        ''' <typeparam name="T">The type of the elements of source.</typeparam>
        ''' <param name="source">The System.Collections.Generic.IEnumerable`1 to return the first element of.</param>
        ''' <param name="default">
        ''' If the sequence is nothing or contains no elements, then this default value will be returned.
        ''' </param>
        ''' <returns>default(<typeparamref name="T"/>) if source is empty; otherwise, the first element in source.</returns>
        <Extension>
        Public Function DefaultFirst(Of T)(source As IEnumerable(Of T), Optional [default] As T = Nothing) As T
            If source Is Nothing OrElse Not source.Any Then
                Return [default]
            Else
                Return source.FirstOrDefault
            End If
        End Function

        ''' <summary>
        ''' Convert the iterator source <see cref="IEnumerable"/> to an object array.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function ensures that the array is not nothing
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToVector(source As IEnumerable) As Object()
            If source Is Nothing Then
                Return {}
            Else
                Return (From x As Object In source Select x).ToArray
            End If
        End Function

        ''' <summary>
        ''' Convert the iterator source <see cref="IEnumerable"/> to a specific type array.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function ensures that the returns array is not nothing
        ''' </remarks>
        <Extension>
        Public Function ToArray(Of T)(source As IEnumerable) As T()
            Return ToVector(source) _
                .Select(Function(x) If(x Is Nothing, Nothing, DirectCast(x, T))) _
                .ToArray
        End Function
    End Module
End Namespace
