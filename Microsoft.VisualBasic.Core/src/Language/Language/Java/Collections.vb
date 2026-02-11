#Region "Microsoft.VisualBasic::e31a383d5f434320e8d55029ea8e2c24, Microsoft.VisualBasic.Core\src\Language\Language\Java\Collections.vb"

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

    '   Total Lines: 333
    '    Code Lines: 145 (43.54%)
    ' Comment Lines: 154 (46.25%)
    '    - Xml Docs: 61.69%
    ' 
    '   Blank Lines: 34 (10.21%)
    '     File Size: 15.24 KB


    '     Module Collections
    ' 
    '         Function: [get], (+2 Overloads) binarySearch, (+2 Overloads) indexedBinarySearch, (+2 Overloads) iteratorBinarySearch, put
    '                   retainAll
    ' 
    '         Sub: shuffle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

'
' * Copyright (c) 1997, 2014, Oracle and/or its affiliates. All rights reserved.
' * ORACLE PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' 

Namespace Language.Java

    ''' <summary>
    ''' This class consists exclusively of static methods that operate on or return
    ''' collections.  It contains polymorphic algorithms that operate on
    ''' collections, "wrappers", which return a new collection backed by a
    ''' specified collection, and a few other odds and ends.
    ''' 
    ''' &lt;p>The methods of this class all throw a <tt>NullPointerException</tt>
    ''' if the collections or class objects provided to them are null.
    ''' 
    ''' &lt;p>The documentation for the polymorphic algorithms contained in this class
    ''' generally includes a brief description of the <i>implementation</i>.  Such
    ''' descriptions should be regarded as <i>implementation notes</i>, rather than
    ''' parts of the <i>specification</i>.  Implementors should feel free to
    ''' substitute other algorithms, so long as the specification itself is adhered
    ''' to.  (For example, the algorithm used by <tt>sort</tt> does not have to be
    ''' a mergesort, but it does have to be <i>stable</i>.)
    ''' 
    ''' &lt;p>The "destructive" algorithms contained in this [Class], that is, the
    ''' algorithms that modify the collection on which they operate, are specified
    ''' to throw <tt>UnsupportedOperationException</tt> if the collection does not
    ''' support the appropriate mutation primitive(s), such as the <tt>set</tt>
    ''' method.  These algorithms may, but are not required to, throw this
    ''' exception if an invocation would have no effect on the collection.  For
    ''' example, invoking the <tt>sort</tt> method on an unmodifiable list that is
    ''' already sorted may or may not throw <tt>UnsupportedOperationException</tt>.
    ''' 
    ''' &lt;p>This class is a member of the
    ''' <a href="{@docRoot}/../technotes/guides/collections/index.html">
    ''' Java Collections Framework</a>.
    ''' 
    ''' @author  Josh Bloch
    ''' @author  Neal Gafter </summary>
    Public Module Collections

        ' Algorithms

        '    
        '     * Tuning parameters for algorithms - Many of the List algorithms have
        '     * two implementations, one of which is appropriate for RandomAccess
        '     * lists, the other for "sequential."  Often, the random access variant
        '     * yields better performance on small sequential access lists.  The
        '     * tuning parameters below determine the cutoff point for what constitutes
        '     * a "small" sequential access list for each algorithm.  The values below
        '     * were empirically determined to work well for LinkedList. Hopefully
        '     * they should be reasonable for other sequential access List
        '     * implementations.  Those doing performance work on this code would
        '     * do well to validate the values of these parameters from time to time.
        '     * (The first word of each tuning parameter name is the algorithm to which
        '     * it applies.)
        '     
        Private Const BINARYSEARCH_THRESHOLD As Integer = 50000
        Private Const REVERSE_THRESHOLD As Integer = 18
        Private Const SHUFFLE_THRESHOLD As Integer = 5
        Private Const FILL_THRESHOLD As Integer = 25
        Private Const ROTATE_THRESHOLD As Integer = 100
        Private Const COPY_THRESHOLD As Integer = 10
        Private Const REPLACEALL_THRESHOLD As Integer = 11
        Private Const INDEXOFSUBLIST_THRESHOLD As Integer = 35

        <Extension>
        Public Function put(Of K, V)(map As IDictionary(Of K, V), key As K, value As V) As V
            Dim previous As V = If(map.ContainsKey(key), map(key), Nothing)
            map(key) = value
            Return previous
        End Function

        ''' <summary>
        ''' Searches the specified list for the specified object using the binary
        ''' search algorithm.  The list must be sorted into ascending order
        ''' according to the Comparable natural ordering of its
        ''' elements (as by the #sort(List) method) prior to making this
        ''' call.  If it is not sorted, the results are undefined.  If the list
        ''' contains multiple elements equal to the specified object, there is no
        ''' guarantee which one will be found.
        ''' 
        ''' &lt;p>This method runs in log(n) time for a "random access" list (which
        ''' provides near-constant-time positional access).  If the specified list
        ''' does not implement the RandomAccess interface and is large,
        ''' this method will do an iterator-based binary search that performs
        ''' O(n) link traversals and O(log n) element comparisons.
        ''' </summary>
        ''' <typeparam name="T">the class of the objects in the list</typeparam>
        ''' <param name="list"> the list to be searched. </param>
        ''' <param name="key"> the key to be searched for. </param>
        ''' <returns> the index of the search key, if it is contained in the list;
        '''         otherwise, &lt;tt>(-(&lt;i>insertion point&lt;/i>) - 1)&lt;/tt>.  The
        '''         &lt;i>insertion point&lt;/i> is defined as the point at which the
        '''         key would be inserted into the list: the index of the first
        '''         element greater than the key, or &lt;tt>list.size()&lt;/tt> if all
        '''         elements in the list are less than the specified key.  Note
        '''         that this guarantees that the return value will be &gt;= 0 if
        '''         and only if the key is found. 
        ''' </returns>
        Public Function binarySearch(Of T As IComparable(Of T))(list As T(), key As T) As Integer
            If list.Length < BINARYSEARCH_THRESHOLD Then
                Return Collections.indexedBinarySearch(list, key)
            Else
                Return Collections.iteratorBinarySearch(list, key)
            End If
        End Function

        Private Function indexedBinarySearch(Of T, T1 As IComparable(Of T))(list As T1(), key As T) As Integer
            Dim low As Integer = 0
            Dim high As Integer = list.Length - 1

            Do While low <= high
                Dim mid As Integer = CInt(CUInt((low + high)) >> 1)
                Dim midVal As IComparable(Of T) = list(mid)
                Dim cmp As Integer = midVal.CompareTo(key)

                If cmp < 0 Then
                    low = mid + 1
                ElseIf cmp > 0 Then
                    high = mid - 1
                Else
                    Return mid ' key found
                End If
            Loop
            Return -(low + 1) ' key not found
        End Function

        Private Function iteratorBinarySearch(Of T, T1 As IComparable(Of T))(list As T1(), key As T) As Integer
            Dim low As Integer = 0
            Dim high As Integer = list.Length - 1
            Dim i As IEnumerator(Of IComparable(Of T)) = list.GetEnumerator()

            Do While low <= high
                Dim mid As Integer = CInt(CUInt((low + high)) >> 1)
                Dim midVal As IComparable(Of T) = [get](i, mid)
                Dim cmp As Integer = midVal.CompareTo(key)

                If cmp < 0 Then
                    low = mid + 1
                ElseIf cmp > 0 Then
                    high = mid - 1
                Else
                    Return mid ' key found
                End If
            Loop
            Return -(low + 1) ' key not found
        End Function

        ''' <summary>
        ''' Gets the ith element from the given list by repositioning the specified
        ''' list listIterator.
        ''' </summary>
        Private Function [get](Of T)(i As IEnumerator(Of T), index As Integer) As T
            Dim obj As T = Nothing
            Dim pos As Integer '= i.nextIndex()
            If pos <= index Then
                Dim tempVar As Boolean
                Do
                    obj = i.Next
                    tempVar = pos < index
                    pos += 1
                Loop While tempVar
            Else
                Do
                    obj = i.Previous()
                    pos -= 1
                Loop While pos > index
            End If
            Return obj
        End Function


        ''' <summary>
        ''' Searches the specified list for the specified object using the binary
        ''' search algorithm.  The list must be sorted into ascending order
        ''' according to the specified comparator (as by the
        ''' #sort(List, Comparator) sort(List, Comparator)
        ''' method), prior to making this call.  If it is
        ''' not sorted, the results are undefined.  If the list contains multiple
        ''' elements equal to the specified object, there is no guarantee which one
        ''' will be found.
        ''' 
        ''' &lt;p>This method runs in log(n) time for a "random access" list (which
        ''' provides near-constant-time positional access).  If the specified list
        ''' does not implement the RandomAccess interface and is large,
        ''' this method will do an iterator-based binary search that performs
        ''' O(n) link traversals and O(log n) element comparisons.
        ''' </summary>
        ''' <typeparam name="T">the class of the objects in the list</typeparam>
        ''' <param name="list"> the list to be searched. </param>
        ''' <param name="key"> the key to be searched for. </param>
        ''' <param name="c"> the comparator by which the list is ordered.
        '''         A <tt>null</tt> value indicates that the elements'
        '''         Comparable natural ordering should be used. </param>
        ''' <returns> the index of the search key, if it is contained in the list;
        '''         otherwise, <tt>(-(<i>insertion point</i>) - 1)</tt>.  The
        '''         <i>insertion point</i> is defined as the point at which the
        '''         key would be inserted into the list: the index of the first
        '''         element greater than the key, or <tt>list.size()</tt> if all
        '''         elements in the list are less than the specified key.  Note
        '''         that this guarantees that the return value will be &gt;= 0 if
        '''         and only if the key is found. </returns>
        <Extension>
        Public Function binarySearch(Of T As IComparable(Of T))(list As T(), key As T, c As IComparer(Of T)) As Integer
            If c Is Nothing Then Return binarySearch(list, key)

            If list.Length < BINARYSEARCH_THRESHOLD Then
                Return Collections.indexedBinarySearch(list, key, c)
            Else
                Return Collections.iteratorBinarySearch(list, key, c)
            End If
        End Function

        Private Function indexedBinarySearch(Of T)(l As T(), key As T, c As IComparer(Of T)) As Integer
            Dim low As Integer = 0
            Dim high As Integer = l.Length - 1

            Do While low <= high
                Dim mid As Integer = CInt(CUInt((low + high)) >> 1)
                Dim midVal As T = l(mid)
                Dim cmp As Integer = c.Compare(midVal, key)

                If cmp < 0 Then
                    low = mid + 1
                ElseIf cmp > 0 Then
                    high = mid - 1
                Else
                    Return mid ' key found
                End If
            Loop
            Return -(low + 1) ' key not found
        End Function

        Private Function iteratorBinarySearch(Of T)(l As T(), key As T, c As IComparer(Of T)) As Integer
            Dim low As Integer = 0
            Dim high As Integer = l.Length - 1

            Do While low <= high
                Dim mid As Integer = CInt(CUInt((low + high)) >> 1)
                Dim midVal As T = l(mid)
                Dim cmp As Integer = c.Compare(midVal, key)

                If cmp < 0 Then
                    low = mid + 1
                ElseIf cmp > 0 Then
                    high = mid - 1
                Else
                    Return mid ' key found
                End If
            Loop
            Return -(low + 1) ' key not found
        End Function

        ''' <summary>
        ''' 模拟 Java Collection 中的 retainAll 方法。
        ''' 仅保留当前集合中包含在指定集合中的元素（即求交集）。
        ''' 如果集合发生了更改，则返回 True。
        ''' </summary>
        ''' <typeparam name="T">集合元素类型</typeparam>
        ''' <param name="source">原集合（会被修改）</param>
        ''' <param name="itemsToKeep">要保留的元素集合</param>
        ''' <returns>如果原集合被修改了返回 True，否则返回 False</returns>
        <Extension()>
        Public Function retainAll(Of T)(source As IList(Of T), itemsToKeep As IEnumerable(Of T)) As Boolean
            ' 1. 检查 Null
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If itemsToKeep Is Nothing Then Throw New ArgumentNullException(NameOf(itemsToKeep))

            ' 2. 将 itemsToKeep 转换为 HashSet 以提高查找效率 (O(1) 查找时间)
            ' 这比在 List 中循环查找要快得多
            Dim keepSet As New HashSet(Of T)(itemsToKeep)

            ' 3. 记录原始数量，用于判断是否发生了变化
            Dim originalCount As Integer = source.Count

            ' 4. 倒序遍历原集合，移除不在 keepSet 中的元素
            ' 倒序是为了防止移除元素后索引变化导致的问题
            For i As Integer = source.Count - 1 To 0 Step -1
                If Not keepSet.Contains(source(i)) Then
                    source.RemoveAt(i)
                End If
            Next

            ' 5. 如果数量变了，说明发生了移除操作，返回 True
            Return source.Count <> originalCount
        End Function

        Public Sub shuffle(Of T)(list As IList(Of T), r As Random)

            ' 基本的空引用检查
            If list Is Nothing Then Return

            ' Fisher-Yates 洗牌算法
            ' 从列表的最后一个元素开始，向前遍历到第二个元素（索引 1）
            For i As Integer = list.Count - 1 To 1 Step -1

                ' 生成一个 0 到 i 之间的随机索引 j（包含 i）
                ' 注意：Random.Next(maxValue) 是不包含 maxValue 的，所以用 i + 1
                Dim j As Integer = r.Next(i + 1)

                ' 交换 list(i) 和 list(j)
                Dim temp As T = list(i)
                list(i) = list(j)
                list(j) = temp
            Next
        End Sub
    End Module

End Namespace
