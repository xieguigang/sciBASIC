#Region "Microsoft.VisualBasic::1d9441fa320a7134b5344f3a3c976995, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Extensions\Collection\Linq\Linq.vb"

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
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Linq

    ''' <summary>
    ''' Linq Helpers.(为了方便编写Linq代码而构建的一个拓展模块)
    ''' </summary>
    <PackageNamespace("LINQ", Category:=APICategories.UtilityTools)>
    <Extension>
    Public Module Extensions

        <Extension>
        Public Function ToArray(Of T)(source As IEnumerable(Of Object)) As T()
            If source Is Nothing Then
                Return New T() {}
            Else
                Return (From x As Object In source Select DirectCast(x, T)).ToArray
            End If
        End Function

        ''' <summary>
        ''' A query proxy function makes your linq not so easily crashed due to the unexpected null reference collection as linq source.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function SafeQuery(Of T)(source As IEnumerable(Of T)) As IEnumerable(Of T)
            If Not source Is Nothing Then
                For Each x As T In source
                    Yield x
                Next
            End If
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

        Public Function __innerTry(Of T)(source As Func(Of T), msg As String, Optional throwEx As Boolean = True) As T
            Try
                Return source()
            Catch ex As Exception
                ex = New Exception(msg)

                Call App.LogException(ex, source.ToString)

                If throwEx Then
                    Throw ex
                Else
                    Return Nothing
                End If
            End Try
        End Function

        ''' <summary>
        ''' Iterates all of the elements in a two dimension collection as the data source for the linq expression or ForEach statement.
        ''' (适用于二维的集合做为linq的数据源，不像<see cref="MatrixToList"/>是进行转换，这个是返回迭代器的，推荐使用这个函数)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension> Public Iterator Function MatrixAsIterator(Of T)(source As IEnumerable(Of IEnumerable(Of T))) As IEnumerable(Of T)
            For Each Line As IEnumerable(Of T) In source
                If Not Line.IsNullOrEmpty Then
                    For Each x As T In Line
                        Yield x
                    Next
                End If
            Next
        End Function

        <Extension>
        Public Iterator Function JoinAsIterator(Of T)(a As IEnumerable(Of T), b As IEnumerable(Of T)) As IEnumerable(Of T)
            If Not a Is Nothing Then
                For Each x As T In a
                    Yield x
                Next
            End If
            If Not b Is Nothing Then
                For Each x As T In b
                    Yield x
                Next
            End If
        End Function

        ''' <summary>
        ''' 删除制定的键之后返回剩下的数据
        ''' </summary>
        ''' <typeparam name="TKey"></typeparam>
        ''' <typeparam name="TValue"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="key"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RemoveLeft(Of TKey, TValue)(ByRef source As Dictionary(Of TKey, TValue), key As TKey) As Dictionary(Of TKey, TValue)
            Call source.Remove(key)
            Return source
        End Function

        ''' <summary>
        ''' Copy <paramref name="source"/> <paramref name="n"/> times to construct a new vector.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="n"></param>
        ''' <returns>An array consist of source with n elements.</returns>
        <Extension> Public Function CopyVector(Of T)(source As T, n As Integer) As T()
            Return n.ToArray(Function(x) source)
        End Function

        <Extension>
        Public Function CopyVector(Of T)(n As Integer, source As Func(Of T)) As T()
            Return n.ToArray(Function(x) source())
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
        ''' 产生指定数目的一个递增序列(所生成序列的数值就是生成的数组的元素的个数)
        ''' </summary>
        ''' <param name="n">大于或者等于0的一个数，当小于0的时候会出错</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Sequence")>
        <Extension> Public Iterator Function Sequence(n As Integer) As IEnumerable(Of Integer)
            If n < 0 Then
                Dim ex As String = $"n:={n} is not a valid index generator value for sequence!"
                Throw New Exception(ex)
            End If

            For i As Integer = 0 To n - 1
                Yield i
            Next
        End Function

        ''' <summary>
        ''' [Sequence Generation] Generate regular sequences. seq is a standard generic with a default method.
        ''' </summary>
        ''' <param name="From">the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.</param>
        ''' <param name="To">the starting and (maximal) end values of the sequence. Of length 1 unless just from is supplied as an unnamed argument.</param>
        ''' <param name="By">number: increment of the sequence</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Iterator Function seq([from] As Value(Of Double), [to] As Double, Optional by As Double = 1) As IEnumerable(Of Double)
            Yield from

            Do While (from = from.value + by) <= [to]
                Yield from
            Loop
        End Function

        <Extension>
        Public Iterator Function SeqIterator(n As Integer, Optional offset As Integer = 0) As IEnumerable(Of Integer)
            If n < 0 Then
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
            Dim array As Integer() = n.Sequence

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
        <Extension> Public Function Sequence(n As Long) As Long()
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
        <Extension> Public Function Sequence(n As UInteger) As Integer()
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
        <Extension> Public Function ToArray(Of T)(len As Integer, elementAt As Func(Of Integer, T)) As T()
            Return len.Sequence.ToArray(elementAt)
        End Function

        ''' <summary>
        ''' Creates an array from a <see cref="IEnumerable(Of T)"/>.
        ''' (默认非并行化的，这个函数是安全的，假若参数为空值则会返回一个空的数组)
        ''' </summary>
        ''' <typeparam name="T">The type of the elements of source.</typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="source">An System.Collections.Generic.IEnumerable`1 to create an array from.</param>
        ''' <returns>An array that contains the elements from the input sequence.</returns>
        <Extension> Public Function ToArray(Of T, TOut)(
                                      source As IEnumerable(Of T),
                                     [ctype] As Func(Of T, TOut),
                           Optional parallel As Boolean = False) As TOut()

            If source Is Nothing Then  ' 假若这里使用IsNullOrEmpty的话，会导致Linq查询会被执行两次，所以在这里直接判断是否为空值就行了
                Return New TOut() {}
            End If

            If parallel Then
                Return LinqAPI.Exec(Of TOut) <= From obj As T
                                                In source.AsParallel
                                                Select [ctype](obj)
            Else
                Return source.Select([ctype]).ToArray
            End If
        End Function

        ''' <summary>
        ''' Creates an array from a <see cref="IEnumerable(Of T)"/>.(默认非并行化的，这个函数是安全的，假若参数为空值则会返回一个空的数组)
        ''' </summary>
        ''' <typeparam name="T">The type of the elements of source.</typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="source">An System.Collections.Generic.IEnumerable`1 to create an array from.</param>
        ''' <returns>An array that contains the elements from the input sequence.</returns>
        <Extension> Public Function ToArray(Of T, TOut)(source As IEnumerable(Of T),
                                                        [CType] As Func(Of T, TOut),
                                                        [where] As Func(Of T, Boolean),
                                                        Optional Parallel As Boolean = False) As TOut()
            If source.IsNullOrEmpty Then
                Return New TOut() {}
            End If

            Dim LQuery As TOut()

            If Parallel Then
                LQuery = (From obj As T
                          In source.AsParallel
                          Where where(obj)
                          Select [CType](obj)).ToArray
            Else
                LQuery = (From obj As T In source
                          Where where(obj)
                          Select [CType](obj)).ToArray
            End If

            Return LQuery
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="__ctype">第二个参数是index</param>
        ''' <param name="Parallel"></param>
        ''' <returns></returns>
        <Extension> Public Function ToArray(Of T, TOut)(
                                      source As IEnumerable(Of T),
                                     __ctype As Func(Of T, Integer, TOut),
                           Optional Parallel As Boolean = False) As TOut()

            If source Is Nothing Then
                Return New TOut() {}
            End If

            Dim LQuery As TOut()

            If Parallel Then
                LQuery = LinqAPI.Exec(Of TOut) <=
                    From i As SeqValue(Of T)
                    In source.SeqIterator.AsParallel
                    Let obj As T = i.obj
                    Select __ctype(obj, i.i)
            Else
                LQuery = LinqAPI.Exec(Of TOut) <=
                    From i As SeqValue(Of T)
                    In source.SeqIterator
                    Let obj As T = i.obj
                    Select __ctype(obj, i.i)
            End If

            Return LQuery
        End Function

        <Extension> Public Function ToArray(Of T)(len&, elementAt As Func(Of Long, T)) As T()
            Return len.Sequence.ToArray(elementAt)
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
        <Extension> Public Function FirstOrDefault(Of TSource)(source As IEnumerable(Of TSource), [default] As TSource) As TSource
            Dim value As TSource = source.FirstOrDefault

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
        ''' <param name="[default]">
        ''' If the sequence is nothing or contains no elements, then this default value will be returned.
        ''' </param>
        ''' <returns>default(TSource) if source is empty; otherwise, the first element in source.</returns>
        <Extension> Public Function DefaultFirst(Of T)(source As IEnumerable(Of T), Optional [default] As T = Nothing) As T
            If source.IsNullOrEmpty Then
                Return [default]
            Else
                Return source.First
            End If
        End Function

        ''' <summary>
        ''' Convert the iterator source <see cref="IEnumerable"/> to an object array.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ToVector(source As IEnumerable) As Object()
            Dim LQuery As Object() = (From x As Object In source Select x).ToArray
            Return LQuery
        End Function

        ''' <summary>
        ''' Convert the iterator source <see cref="IEnumerable"/> to a specific type array.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ToArray(Of T)(source As IEnumerable) As T()
            Return ToVector(source).ToArray(
                Function(x) If(x Is Nothing, Nothing, DirectCast(x, T)))
        End Function

        <Extension>
        Public Function ToArray(Of T, TKey, TValue)(
                          source As IEnumerable(Of KeyValuePair(Of TKey, TValue)),
                         [CType] As Func(Of TKey, TValue, T),
               Optional Parallel As Boolean = False,
               Optional [Where] As Func(Of TKey, TValue, Boolean) = Nothing) As T()

            If Where Is Nothing Then
                Return source.__toArrayNoWhere([CType], Parallel)
            Else
                Return source.ToArray(Of T)(
                    Function(x) [CType](x.Key, x.Value),
                    Function(x) Where(x.Key, x.Value))
            End If
        End Function

        <Extension>
        Private Function __toArrayNoWhere(Of T, TKey, TValue)(source As IEnumerable(Of KeyValuePair(Of TKey, TValue)),
                                                          [CType] As Func(Of TKey, TValue, T),
                                                          Parallel As Boolean) As T()
            Return source.ToArray(Of T)(Function(x) [CType](x.Key, x.Value))
        End Function

        <Extension>
        Public Function ToArray(Of T, TKey, TValue)(source As IEnumerable(Of IKeyValuePairObject(Of TKey, TValue)),
                                                [CType] As Func(Of TKey, TValue, T),
                                                Optional Parallel As Boolean = False,
                                                Optional [Where] As Func(Of TKey, TValue, Boolean) = Nothing) As T()

            If Where Is Nothing Then
                Return source.__toArrayNoWhere([CType], Parallel)
            Else
                Return source.ToArray(Of T)(
                    Function(x As IKeyValuePairObject(Of TKey, TValue)) [CType](x.Identifier, x.Value),
                    where:=Function(x) Where(x.Identifier, x.Value),
                    Parallel:=Parallel)
            End If
        End Function

        <Extension>
        Private Function __toArrayNoWhere(Of T, TKey, TValue)(source As IEnumerable(Of IKeyValuePairObject(Of TKey, TValue)),
                                                         [CType] As Func(Of TKey, TValue, T),
                                                         Parallel As Boolean) As T()
            Return source.ToArray(Of T)(Function(x) [CType](x.Identifier, x.Value), parallel:=Parallel)
        End Function

        <Extension>
        Public Function ToArray(Of T, T1, T2, T3)(source As IEnumerable(Of ITripleKeyValuesPair(Of T1, T2, T3)),
                                              [CType] As Func(Of T1, T2, T3, T),
                                              Optional Parallel As Boolean = False,
                                              Optional [Where] As Func(Of T1, T2, T3, Boolean) = Nothing) As T()
            If Where Is Nothing Then
                Return source.__toArrayNoWhere([CType], Parallel)
            Else
                Return source.ToArray(Of T)(Function(x) [CType](x.Identifier, x.Value2, x.Address), where:=Function(x) Where(x.Identifier, x.Value2, x.Address))
            End If
        End Function

        <Extension>
        Private Function __toArrayNoWhere(Of T, T1, T2, T3)(source As IEnumerable(Of ITripleKeyValuesPair(Of T1, T2, T3)),
                                                        [CType] As Func(Of T1, T2, T3, T),
                                                        Parallel As Boolean) As T()
            Return source.ToArray(Of T)(Function(x) [CType](x.Identifier, x.Value2, x.Address))
        End Function
    End Module
End Namespace
