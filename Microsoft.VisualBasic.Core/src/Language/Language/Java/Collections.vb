#Region "Microsoft.VisualBasic::92789fc3f159cba49e8155950fb25b0d, Microsoft.VisualBasic.Core\src\Language\Language\Java\Collections.vb"

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

    '   Total Lines: 720
    '    Code Lines: 126
    ' Comment Lines: 551
    '   Blank Lines: 43
    '     File Size: 36.63 KB


    '     Module Collections
    ' 
    '         Function: [get], (+2 Overloads) binarySearch, (+2 Overloads) indexedBinarySearch, (+2 Overloads) iteratorBinarySearch, put
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Diagnostics
Imports System.Collections
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Linq
Imports System.Runtime.CompilerServices

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
        Private Const BINARYSEARCH_THRESHOLD As Integer = 5000
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
        Public Function binarySearch(Of T As IComparable(Of T))(list As List(Of T), key As T) As Integer
            If list.Count < BINARYSEARCH_THRESHOLD Then
                Return Collections.indexedBinarySearch(list, key)
            Else
                Return Collections.iteratorBinarySearch(list, key)
            End If
        End Function

        'JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
        'JAVA TO VB CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
        Private Function indexedBinarySearch(Of T, T1 As IComparable(Of T))(list As List(Of T1), key As T) As Integer
            Dim low As Integer = 0
            Dim high As Integer = list.Count - 1

            Do While low <= high
                Dim mid As Integer = CInt(CUInt((low + high)) >> 1)
                'JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
                'JAVA TO VB CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
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

        Private Function iteratorBinarySearch(Of T, T1 As IComparable(Of T))(list As List(Of T1), key As T) As Integer
            Dim low As Integer = 0
            Dim high As Integer = list.Count - 1
            'JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
            'JAVA TO VB CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
            Dim i As IEnumerator(Of IComparable(Of T)) = list.GetEnumerator()

            Do While low <= high
                Dim mid As Integer = CInt(CUInt((low + high)) >> 1)
                'JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
                'JAVA TO VB CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
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
        Public Function binarySearch(Of T As IComparable(Of T))(list As List(Of T), key As T, c As IComparer(Of T)) As Integer
            If c Is Nothing Then Return binarySearch(list, key)

            If list.Count < BINARYSEARCH_THRESHOLD Then
                Return Collections.indexedBinarySearch(list, key, c)
            Else
                Return Collections.iteratorBinarySearch(list, key, c)
            End If
        End Function

        'JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
        Private Function indexedBinarySearch(Of T)(l As List(Of T), key As T, c As IComparer(Of T)) As Integer
            Dim low As Integer = 0
            Dim high As Integer = l.Count - 1

            Do While low <= high
                Dim mid As Integer = CInt(CUInt((low + high)) >> 1)
                Dim midVal As T = l.Get(mid)
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

        'JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
        Private Function iteratorBinarySearch(Of T)(l As List(Of T), key As T, c As IComparer(Of T)) As Integer
            Dim low As Integer = 0
            Dim high As Integer = l.Count - 1

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

        '		''' <summary>
        '		''' Reverses the order of the elements in the specified list.<p>
        '		''' 
        '		''' This method runs in linear time.
        '		''' </summary>
        '		''' <param name="list"> the list whose elements are to be reversed. </param>
        '		''' <exception cref="UnsupportedOperationException"> if the specified list or
        '		'''         its list-iterator does not support the <tt>set</tt> operation. </exception>
        ''JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        '		Public  Sub reverse(Of T1)(  list As List(Of T1))
        '            Dim size As Integer = list.Count
        '            If size < REVERSE_THRESHOLD OrElse TypeOf list Is RandomAccess Then
        '				Dim i As Integer=0
        '				Dim mid As Integer=size>>1
        '				Dim j As Integer=size-1
        '				Do While i<mid
        '                    list.Swap(i, j)
        '                    i += 1
        '					j -= 1
        '				Loop
        '			Else
        '				' instead of using a raw type here, it's possible to capture
        '				' the wildcard but it will require a call to a supplementary
        '				' private method
        '				Dim fwd As ListIterator = list.GetEnumerator()
        '				Dim rev As ListIterator = list.listIterator(size)
        '				Dim i As Integer=0
        '                Dim mid As Integer = list.Count >> 1
        '                Do While i<mid
        '					Dim tmp As Object = fwd.next()
        '					fwd.set(rev.previous())
        '					rev.set(tmp)
        '					i += 1
        '				Loop
        '			End If
        '		End Sub

        '        ''' <summary>
        '        ''' Randomly permutes the specified list using a default source of
        '        ''' randomness.  All permutations occur with approximately equal
        '        ''' likelihood.
        '        ''' 
        '        ''' <p>The hedge "approximately" is used in the foregoing description because
        '        ''' default source of randomness is only approximately an unbiased source
        '        ''' of independently chosen bits. If it were a perfect source of randomly
        '        ''' chosen bits, then the algorithm would choose permutations with perfect
        '        ''' uniformity.
        '        ''' 
        '        ''' <p>This implementation traverses the list backwards, from the last
        '        ''' element up to the second, repeatedly swapping a randomly selected element
        '        ''' into the "current position".  Elements are randomly selected from the
        '        ''' portion of the list that runs from the first element to the current
        '        ''' position, inclusive.
        '        ''' 
        '        ''' <p>This method runs in linear time.  If the specified list does not
        '        ''' implement the <seealso cref="RandomAccess"/> interface and is large, this
        '        ''' implementation dumps the specified list into an array before shuffling
        '        ''' it, and dumps the shuffled array back into the list.  This avoids the
        '        ''' quadratic behavior that would result from shuffling a "sequential
        '        ''' access" list in place.
        '        ''' </summary>
        '        ''' <param name="list"> the list to be shuffled. </param>
        '        ''' <exception cref="UnsupportedOperationException"> if the specified list or
        '        '''         its list-iterator does not support the <tt>set</tt> operation. </exception>
        '        Public  Sub shuffle(Of T1)(  list As List(Of T1))
        '			Dim rnd As Random = r
        '			If rnd Is Nothing Then
        '					rnd = New Random
        '					r = rnd
        '			End If
        '			shuffle(list, rnd)
        '		End Sub

        '		Private  r As Random

        '		''' <summary>
        '		''' Randomly permute the specified list using the specified source of
        '		''' randomness.  All permutations occur with equal likelihood
        '		''' assuming that the source of randomness is fair.<p>
        '		''' 
        '		''' This implementation traverses the list backwards, from the last element
        '		''' up to the second, repeatedly swapping a randomly selected element into
        '		''' the "current position".  Elements are randomly selected from the
        '		''' portion of the list that runs from the first element to the current
        '		''' position, inclusive.<p>
        '		''' 
        '		''' This method runs in linear time.  If the specified list does not
        '		''' implement the <seealso cref="RandomAccess"/> interface and is large, this
        '		''' implementation dumps the specified list into an array before shuffling
        '		''' it, and dumps the shuffled array back into the list.  This avoids the
        '		''' quadratic behavior that would result from shuffling a "sequential
        '		''' access" list in place.
        '		''' </summary>
        '		''' <param name="list"> the list to be shuffled. </param>
        '		''' <param name="rnd"> the source of randomness to use to shuffle the list. </param>
        '		''' <exception cref="UnsupportedOperationException"> if the specified list or its
        '		'''         list-iterator does not support the <tt>set</tt> operation. </exception>
        ''JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        '		Public  Sub shuffle(Of T1)(  list As List(Of T1),   rnd As Random)
        '			Dim size As Integer = list.size()
        '			If size < SHUFFLE_THRESHOLD OrElse TypeOf list Is RandomAccess Then
        '				For i As Integer = size To 2 Step -1
        '					swap(list, i-1, rnd.Next(i))
        '				Next i
        '			Else
        '				Dim arr As Object() = list.ToArray()

        '				' Shuffle array
        '				For i As Integer = size To 2 Step -1
        '					swap(arr, i-1, rnd.Next(i))
        '				Next i

        '				' Dump array back into list
        '				' instead of using a raw type here, it's possible to capture
        '				' the wildcard but it will require a call to a supplementary
        '				' private method
        '				Dim it As ListIterator = list.GetEnumerator()
        '				For i As Integer = 0 To arr.Length - 1
        '					it.next()
        '					it.set(arr(i))
        '				Next i
        '			End If
        '		End Sub

        '        ''' <summary>
        '        ''' Replaces all of the elements of the specified list with the specified
        '        ''' element. <p>
        '        ''' 
        '        ''' This method runs in linear time.
        '        ''' </summary>
        '        ''' @param  <T> the class of the objects in the list </param>
        '        ''' <param name="list"> the list to be filled with the specified element. </param>
        '        ''' <param name="obj"> The element with which to fill the specified list. </param>
        '        ''' <exception cref="UnsupportedOperationException"> if the specified list or its
        '        '''         list-iterator does not support the <tt>set</tt> operation. </exception>
        '        'JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
        '        Public  Sub fill(Of T, T1)(  list As List(Of T1),   obj As T)
        '            Dim size As Integer = list.Count

        '            If size < FILL_THRESHOLD OrElse TypeOf list Is RandomAccess Then
        '                For i As Integer = 0 To size - 1
        '                    list(i) = obj
        '                Next
        '            Else
        ''JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
        ''JAVA TO VB CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
        '				Dim itr As ListIterator(Of ?) = list.GetEnumerator()
        '                For i As Integer = 0 To size - 1
        '                    itr.next()
        '                    itr.set(obj)
        '                Next
        '            End If
        '		End Sub

        '        ''' <summary>
        '        ''' Rotates the elements in the specified list by the specified distance.
        '        ''' After calling this method, the element at index <tt>i</tt> will be
        '        ''' the element previously at index <tt>(i - distance)</tt> mod
        '        ''' <tt>list.size()</tt>, for all values of <tt>i</tt> between <tt>0</tt>
        '        ''' and <tt>list.size()-1</tt>, inclusive.  (This method has no effect on
        '        ''' the size of the list.)
        '        ''' 
        '        ''' <p>For example, suppose <tt>list</tt> comprises<tt> [t, a, n, k, s]</tt>.
        '        ''' After invoking <tt>Collections.rotate(list, 1)</tt> (or
        '        ''' <tt>Collections.rotate(list, -4)</tt>), <tt>list</tt> will comprise
        '        ''' <tt>[s, t, a, n, k]</tt>.
        '        ''' 
        '        ''' <p>Note that this method can usefully be applied to sublists to
        '        ''' move one or more elements within a list while preserving the
        '        ''' order of the remaining elements.  For example, the following idiom
        '        ''' moves the element at index <tt>j</tt> forward to position
        '        ''' <tt>k</tt> (which must be greater than or equal to <tt>j</tt>):
        '        ''' <pre>
        '        '''     Collections.rotate(list.subList(j, k+1), -1);
        '        ''' </pre>
        '        ''' To make this concrete, suppose <tt>list</tt> comprises
        '        ''' <tt>[a, b, c, d, e]</tt>.  To move the element at index <tt>1</tt>
        '        ''' (<tt>b</tt>) forward two positions, perform the following invocation:
        '        ''' <pre>
        '        '''     Collections.rotate(l.subList(1, 4), -1);
        '        ''' </pre>
        '        ''' The resulting list is <tt>[a, c, d, b, e]</tt>.
        '        ''' 
        '        ''' <p>To move more than one element forward, increase the absolute value
        '        ''' of the rotation distance.  To move elements backward, use a positive
        '        ''' shift distance.
        '        ''' 
        '        ''' <p>If the specified list is small or implements the {@link
        '        ''' RandomAccess} interface, this implementation exchanges the first
        '        ''' element into the location it should go, and then repeatedly exchanges
        '        ''' the displaced element into the location it should go until a displaced
        '        ''' element is swapped into the first element.  If necessary, the process
        '        ''' is repeated on the second and successive elements, until the rotation
        '        ''' is complete.  If the specified list is large and doesn't implement the
        '        ''' <tt>RandomAccess</tt> interface, this implementation breaks the
        '        ''' list into two sublist views around index <tt>-distance mod size</tt>.
        '        ''' Then the <seealso cref="#reverse(List)"/> method is invoked on each sublist view,
        '        ''' and finally it is invoked on the entire list.  For a more complete
        '        ''' description of both algorithms, see Section 2.3 of Jon Bentley's
        '        ''' <i>Programming Pearls</i> (Addison-Wesley, 1986).
        '        ''' </summary>
        '        ''' <param name="list"> the list to be rotated. </param>
        '        ''' <param name="distance"> the distance to rotate the list.  There are no
        '        '''        constraints on this value; it may be zero, negative, or
        '        '''        greater than <tt>list.size()</tt>. </param>
        '        ''' <exception cref="UnsupportedOperationException"> if the specified list or
        '        '''         its list-iterator does not support the <tt>set</tt> operation.
        '        ''' @since 1.4 </exception>
        '        Public  Sub rotate(Of T1)(  list As List(Of T1),   distance As Integer)
        '			If TypeOf list Is RandomAccess OrElse list.size() < ROTATE_THRESHOLD Then
        '				rotate1(list, distance)
        '			Else
        '				rotate2(list, distance)
        '			End If
        '		End Sub

        '		Private  Sub rotate1(Of T)(  list As List(Of T),   distance As Integer)
        '			Dim size As Integer = list.size()
        '			If size = 0 Then Return
        '			distance = distance Mod size
        '			If distance < 0 Then distance += size
        '			If distance = 0 Then Return

        '			Dim cycleStart As Integer = 0
        '			Dim nMoved As Integer = 0
        '			Do While nMoved <> size
        '				Dim displaced As T = list.get(cycleStart)
        '				Dim i As Integer = cycleStart
        '				Do
        '					i += distance
        '					If i >= size Then i -= size
        '					displaced = list.set(i, displaced)
        '					nMoved += 1
        '				Loop While i <> cycleStart
        '				cycleStart += 1
        '			Loop
        '		End Sub

        '		Private  Sub rotate2(Of T1)(  list As List(Of T1),   distance As Integer)
        '			Dim size As Integer = list.size()
        '			If size = 0 Then Return
        '			Dim mid As Integer = -distance Mod size
        '			If mid < 0 Then mid += size
        '			If mid = 0 Then Return

        '			reverse(list.subList(0, mid))
        '			reverse(list.subList(mid, size))
        '			reverse(list)
        '		End Sub

        '		''' <summary>
        '		''' Replaces all occurrences of one specified value in a list with another.
        '		''' More formally, replaces with <tt>newVal</tt> each element <tt>e</tt>
        '		''' in <tt>list</tt> such that
        '		''' <tt>(oldVal==null ? e==null : oldVal.equals(e))</tt>.
        '		''' (This method has no effect on the size of the list.)
        '		''' </summary>
        '		''' @param  <T> the class of the objects in the list </param>
        '		''' <param name="list"> the list in which replacement is to occur. </param>
        '		''' <param name="oldVal"> the old value to be replaced. </param>
        '		''' <param name="newVal"> the new value with which <tt>oldVal</tt> is to be
        '		'''        replaced. </param>
        '		''' <returns> <tt>true</tt> if <tt>list</tt> contained one or more elements
        '		'''         <tt>e</tt> such that
        '		'''         <tt>(oldVal==null ?  e==null : oldVal.equals(e))</tt>. </returns>
        '		''' <exception cref="UnsupportedOperationException"> if the specified list or
        '		'''         its list-iterator does not support the <tt>set</tt> operation.
        '		''' @since  1.4 </exception>
        '		Public  Function replaceAll(Of T)(  list As List(Of T),   oldVal As T,   newVal As T) As Boolean
        '			Dim result As Boolean = False
        '			Dim size As Integer = list.size()
        '			If size < REPLACEALL_THRESHOLD OrElse TypeOf list Is RandomAccess Then
        '				If oldVal Is Nothing Then
        '					For i As Integer = 0 To size - 1
        '						If list.get(i) Is Nothing Then
        '							list.set(i, newVal)
        '							result = True
        '						End If
        '					Next i
        '				Else
        '					For i As Integer = 0 To size - 1
        '						If oldVal.Equals(list.get(i)) Then
        '							list.set(i, newVal)
        '							result = True
        '						End If
        '					Next i
        '				End If
        '			Else
        '				Dim itr As ListIterator(Of T)=list.GetEnumerator()
        '				If oldVal Is Nothing Then
        '					For i As Integer = 0 To size - 1
        '						If itr.next() Is Nothing Then
        '							itr.set(newVal)
        '							result = True
        '						End If
        '					Next i
        '				Else
        '					For i As Integer = 0 To size - 1
        '						If oldVal.Equals(itr.next()) Then
        '							itr.set(newVal)
        '							result = True
        '						End If
        '					Next i
        '				End If
        '			End If
        '			Return result
        '		End Function

        '		''' <summary>
        '		''' Returns the starting position of the first occurrence of the specified
        '		''' target list within the specified source list, or -1 if there is no
        '		''' such occurrence.  More formally, returns the lowest index <tt>i</tt>
        '		''' such that {@code source.subList(i, i+target.size()).equals(target)},
        '		''' or -1 if there is no such index.  (Returns -1 if
        '		''' {@code target.size() > source.size()})
        '		''' 
        '		''' <p>This implementation uses the "brute force" technique of scanning
        '		''' over the source list, looking for a match with the target at each
        '		''' location in turn.
        '		''' </summary>
        '		''' <param name="source"> the list in which to search for the first occurrence
        '		'''        of <tt>target</tt>. </param>
        '		''' <param name="target"> the list to search for as a subList of <tt>source</tt>. </param>
        '		''' <returns> the starting position of the first occurrence of the specified
        '		'''         target list within the specified source list, or -1 if there
        '		'''         is no such occurrence.
        '		''' @since  1.4 </returns>
        '		Public  Function indexOfSubList(Of T1, T2)(  source As List(Of T1),   target As List(Of T2)) As Integer
        '			Dim sourceSize As Integer = source.size()
        '			Dim targetSize As Integer = target.size()
        '			Dim maxCandidate As Integer = sourceSize - targetSize

        '			If sourceSize < INDEXOFSUBLIST_THRESHOLD OrElse (TypeOf source Is RandomAccess AndAlso TypeOf target Is RandomAccess) Then
        '			nextCand:
        '				For candidate As Integer = 0 To maxCandidate
        '					Dim i As Integer=0
        '					Dim j As Integer=candidate
        '					Do While i<targetSize
        '						If Not eq(target.get(i), source.get(j)) Then GoTo nextCand ' Element mismatch, try next cand
        '						i += 1
        '						j += 1
        '					Loop
        '					Return candidate ' All elements of candidate matched target
        '				Next candidate ' Iterator version of above algorithm
        '			Else
        ''JAVA TO VB CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
        '				Dim si As ListIterator(Of ?) = source.GetEnumerator()
        '			nextCand:
        '				For candidate As Integer = 0 To maxCandidate
        ''JAVA TO VB CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
        '					Dim ti As ListIterator(Of ?) = target.GetEnumerator()
        '					For i As Integer = 0 To targetSize - 1
        '						If Not eq(ti.next(), si.next()) Then
        '							' Back up source iterator to next candidate
        '							For j As Integer = 0 To i - 1
        '								si.previous()
        '							Next j
        '							GoTo nextCand
        '						End If
        '					Next i
        '					Return candidate
        '				Next candidate
        '			End If
        '			Return -1 ' No candidate matched the target
        '		End Function

        '		''' <summary>
        '		''' Returns the starting position of the last occurrence of the specified
        '		''' target list within the specified source list, or -1 if there is no such
        '		''' occurrence.  More formally, returns the highest index <tt>i</tt>
        '		''' such that {@code source.subList(i, i+target.size()).equals(target)},
        '		''' or -1 if there is no such index.  (Returns -1 if
        '		''' {@code target.size() > source.size()})
        '		''' 
        '		''' <p>This implementation uses the "brute force" technique of iterating
        '		''' over the source list, looking for a match with the target at each
        '		''' location in turn.
        '		''' </summary>
        '		''' <param name="source"> the list in which to search for the last occurrence
        '		'''        of <tt>target</tt>. </param>
        '		''' <param name="target"> the list to search for as a subList of <tt>source</tt>. </param>
        '		''' <returns> the starting position of the last occurrence of the specified
        '		'''         target list within the specified source list, or -1 if there
        '		'''         is no such occurrence.
        '		''' @since  1.4 </returns>
        '		Public  Function lastIndexOfSubList(Of T1, T2)(  source As List(Of T1),   target As List(Of T2)) As Integer
        '			Dim sourceSize As Integer = source.size()
        '			Dim targetSize As Integer = target.size()
        '			Dim maxCandidate As Integer = sourceSize - targetSize

        '			If sourceSize < INDEXOFSUBLIST_THRESHOLD OrElse TypeOf source Is RandomAccess Then ' Index access version
        '			nextCand:
        '				For candidate As Integer = maxCandidate To 0 Step -1
        '					Dim i As Integer=0
        '					Dim j As Integer=candidate
        '					Do While i<targetSize
        '						If Not eq(target.get(i), source.get(j)) Then GoTo nextCand ' Element mismatch, try next cand
        '						i += 1
        '						j += 1
        '					Loop
        '					Return candidate ' All elements of candidate matched target
        '				Next candidate ' Iterator version of above algorithm
        '			Else
        '				If maxCandidate < 0 Then Return -1
        ''JAVA TO VB CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
        '				Dim si As ListIterator(Of ?) = source.listIterator(maxCandidate)
        '			nextCand:
        '				For candidate As Integer = maxCandidate To 0 Step -1
        ''JAVA TO VB CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
        '					Dim ti As ListIterator(Of ?) = target.GetEnumerator()
        '					For i As Integer = 0 To targetSize - 1
        '						If Not eq(ti.next(), si.next()) Then
        '							If candidate <> 0 Then
        '								' Back up source iterator to next candidate
        '								For j As Integer = 0 To i+1
        '									si.previous()
        '								Next j
        '							End If
        '							GoTo nextCand
        '						End If
        '					Next i
        '					Return candidate
        '				Next candidate
        '			End If
        '			Return -1 ' No candidate matched the target
        '		End Function

    End Module

End Namespace
