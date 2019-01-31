Imports System
Imports java.lang

'
' * Copyright (c) 1997, 2013, Oracle and/or its affiliates. All rights reserved.
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

Namespace java.util

    ''' <summary>
    ''' This class provides a skeletal implementation of the <tt>Collection</tt>
    ''' interface, to minimize the effort required to implement this interface. <p>
    ''' 
    ''' To implement an unmodifiable collection, the programmer needs only to
    ''' extend this class and provide implementations for the <tt>iterator</tt> and
    ''' <tt>size</tt> methods.  (The iterator returned by the <tt>iterator</tt>
    ''' method must implement <tt>hasNext</tt> and <tt>next</tt>.)<p>
    ''' 
    ''' To implement a modifiable collection, the programmer must additionally
    ''' override this class's <tt>add</tt> method (which otherwise throws an
    ''' <tt>UnsupportedOperationException</tt>), and the iterator returned by the
    ''' <tt>iterator</tt> method must additionally implement its <tt>remove</tt>
    ''' method.<p>
    ''' 
    ''' The programmer should generally provide a  Sub  (no argument) and
    ''' <tt>Collection</tt> constructor, as per the recommendation in the
    ''' <tt>Collection</tt> interface specification.<p>
    ''' 
    ''' The documentation for each non-abstract method in this class describes its
    ''' implementation in detail.  Each of these methods may be overridden if
    ''' the collection being implemented admits a more efficient implementation.<p>
    ''' 
    ''' This class is a member of the
    ''' <a href="{@docRoot}/../technotes/guides/collections/index.html">
    ''' Java Collections Framework</a>.
    ''' 
    ''' @author  Josh Bloch
    ''' @author  Neal Gafter </summary>
    ''' <seealso cref= Collection
    ''' @since 1.2 </seealso>

    Public MustInherit Class AbstractCollection(Of E)
        Implements Collection(Of E)

        ''' <summary>
        ''' Sole constructor.  (For invocation by subclass constructors, typically
        ''' implicit.)
        ''' </summary>
        Protected Friend Sub New()
        End Sub

        ' Query Operations

        ''' <summary>
        ''' Returns an iterator over the elements contained in this collection.
        ''' </summary>
        ''' <returns> an iterator over the elements contained in this collection </returns>
        Public MustOverride Function [iterator]() As [Iterator](Of E) Implements Collection(Of E).iterator

        Public MustOverride Function size() As Integer Implements Collection(Of E).size

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation returns <tt>size() == 0</tt>.
        ''' </summary>
        Public Overridable ReadOnly Property empty As Boolean Implements Collection(Of E).empty
            Get
                Return size() = 0
            End Get
        End Property

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation iterates over the elements in the collection,
        ''' checking each element in turn for equality with the specified element.
        ''' </summary>
        ''' <exception cref="ClassCastException">   {@inheritDoc} </exception>
        ''' <exception cref="NullPointerException"> {@inheritDoc} </exception>
        Public Overridable Function contains(o As Object) As Boolean Implements Collection(Of E).contains
            Dim it As [Iterator](Of E) = [iterator]()
            If o Is Nothing Then
                Do While it.hasNext()
                    If it.next() Is Nothing Then Return True
                Loop
            Else
                Do While it.hasNext()
                    If o.Equals(it.next()) Then Return True
                Loop
            End If
            Return False
        End Function

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation returns an array containing all the elements
        ''' returned by this collection's iterator, in the same order, stored in
        ''' consecutive elements of the array, starting with index {@code 0}.
        ''' The length of the returned array is equal to the number of elements
        ''' returned by the iterator, even if the size of this collection changes
        ''' during iteration, as might happen if the collection permits
        ''' concurrent modification during iteration.  The {@code size} method is
        ''' called only as an optimization hint; the correct result is returned
        ''' even if the iterator returns a different number of elements.
        ''' 
        ''' <p>This method is equivalent to:
        ''' 
        '''  <pre> {@code
        ''' List<E> list = new ArrayList<E>(size());
        ''' for (E e : this)
        '''     list.add(e);
        ''' return list.toArray();
        ''' }</pre>
        ''' </summary>
        Public Overridable Function toArray() As Object() Implements Collection(Of E).toArray
            ' Estimate size of array; be prepared to see more or fewer elements
            Dim r As Object() = New Object(size() - 1) {}
            Dim it As [Iterator](Of E) = [iterator]()
            For i As Integer = 0 To r.Length - 1
                If Not it.hasNext() Then _' fewer elements than expected Return Arrays.copyOf(r, i)
                    r(i) = it.next()
            Next i
            Return If(it.hasNext(), finishToArray(r, it), r)
        End Function

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation returns an array containing all the elements
        ''' returned by this collection's iterator in the same order, stored in
        ''' consecutive elements of the array, starting with index {@code 0}.
        ''' If the number of elements returned by the iterator is too large to
        ''' fit into the specified array, then the elements are returned in a
        ''' newly allocated array with length equal to the number of elements
        ''' returned by the iterator, even if the size of this collection
        ''' changes during iteration, as might happen if the collection permits
        ''' concurrent modification during iteration.  The {@code size} method is
        ''' called only as an optimization hint; the correct result is returned
        ''' even if the iterator returns a different number of elements.
        ''' 
        ''' <p>This method is equivalent to:
        ''' 
        '''  <pre> {@code
        ''' List<E> list = new ArrayList<E>(size());
        ''' for (E e : this)
        '''     list.add(e);
        ''' return list.toArray(a);
        ''' }</pre>
        ''' </summary>
        ''' <exception cref="ArrayStoreException">  {@inheritDoc} </exception>
        ''' <exception cref="NullPointerException"> {@inheritDoc} </exception>
        'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        Public Overridable Function toArray(Of T)(a As T()) As T() Implements Collection(Of E).toArray
            ' Estimate size of array; be prepared to see more or fewer elements
            Dim size As Integer = size()
            Dim r As T() = If(a.Length >= size, a, CType(java.lang.reflect.Array.newInstance(a.GetType().GetElementType(), size), T()))
            Dim it As [Iterator](Of E) = [iterator]()

            For i As Integer = 0 To r.Length - 1
                If Not it.hasNext() Then ' fewer elements than expected
                    If a = r Then
                        r(i) = Nothing ' null-terminate
                    ElseIf a.Length < i Then
                        Return Arrays.copyOf(r, i)
                    Else
                        Array.Copy(r, 0, a, 0, i)
                        If a.Length > i Then a(i) = Nothing
                    End If
                    Return a
                End If
                r(i) = CType(it.next(), T)
            Next i
            ' more elements than expected
            Return If(it.hasNext(), finishToArray(r, it), r)
        End Function

        ''' <summary>
        ''' The maximum size of array to allocate.
        ''' Some VMs reserve some header words in an array.
        ''' Attempts to allocate larger arrays may result in
        ''' OutOfMemoryError: Requested array size exceeds VM limit
        ''' </summary>
        Private Shared ReadOnly MAX_ARRAY_SIZE As Integer = java.lang.[Integer].MAX_VALUE - 8

        ''' <summary>
        ''' Reallocates the array being used within toArray when the iterator
        ''' returned more elements than expected, and finishes filling it from
        ''' the iterator.
        ''' </summary>
        ''' <param name="r"> the array, replete with previously stored elements </param>
        ''' <param name="it"> the in-progress iterator over this collection </param>
        ''' <returns> array containing the elements in the given array, plus any
        '''         further elements returned by the iterator, trimmed to size </returns>
        'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        Private Shared Function finishToArray(Of T, T1)(r As T(), it As [Iterator](Of T1)) As T()
            Dim i As Integer = r.Length
            Do While it.MoveNext()
                Dim cap As Integer = r.Length
                If i = cap Then
                    Dim newCap As Integer = cap + (cap >> 1) + 1
                    ' overflow-conscious code
                    If newCap - MAX_ARRAY_SIZE > 0 Then newCap = hugeCapacity(cap + 1)
                    r = New T(newCap - 1) {}
                    Array.Copy(r, r, newCap)
                End If
                r(i) = CType(it.Current, T)
                i += 1
            Loop
            ' trim if overallocated
            Return If(i = r.Length, r, Arrays.copyOf(r, i))
        End Function

        Private Shared Function hugeCapacity(minCapacity As Integer) As Integer
            If minCapacity < 0 Then ' overflow Throw New OutOfMemoryError("Required array size too large")
                Return If(minCapacity > MAX_ARRAY_SIZE, java.lang.[Integer].Max_Value, MAX_ARRAY_SIZE)
        End Function

        ' Modification Operations

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation always throws an
        ''' <tt>UnsupportedOperationException</tt>.
        ''' </summary>
        ''' <exception cref="UnsupportedOperationException"> {@inheritDoc} </exception>
        ''' <exception cref="ClassCastException">            {@inheritDoc} </exception>
        ''' <exception cref="NullPointerException">          {@inheritDoc} </exception>
        ''' <exception cref="IllegalArgumentException">      {@inheritDoc} </exception>
        ''' <exception cref="IllegalStateException">         {@inheritDoc} </exception>
        Public Overridable Function add(e As E) As Boolean
            Throw New UnsupportedOperationException
        End Function

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation iterates over the collection looking for the
        ''' specified element.  If it finds the element, it removes the element
        ''' from the collection using the iterator's remove method.
        ''' 
        ''' <p>Note that this implementation throws an
        ''' <tt>UnsupportedOperationException</tt> if the iterator returned by this
        ''' collection's iterator method does not implement the <tt>remove</tt>
        ''' method and this collection contains the specified object.
        ''' </summary>
        ''' <exception cref="UnsupportedOperationException"> {@inheritDoc} </exception>
        ''' <exception cref="ClassCastException">            {@inheritDoc} </exception>
        ''' <exception cref="NullPointerException">          {@inheritDoc} </exception>
        Public Overridable Function remove(o As Object) As Boolean Implements Collection(Of E).remove
            Dim it As [Iterator](Of E) = [iterator]()
            If o Is Nothing Then
                Do While it.MoveNext()
                    If it.Current Is Nothing Then
                        it.remove()
                        Return True
                    End If
                Loop
            Else
                Do While it.MoveNext()
                    If o.Equals(it.Current) Then
                        it.remove()
                        Return True
                    End If
                Loop
            End If
            Return False
        End Function


        ' Bulk Operations

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation iterates over the specified collection,
        ''' checking each element returned by the iterator in turn to see
        ''' if it's contained in this collection.  If all elements are so
        ''' contained <tt>true</tt> is returned, otherwise <tt>false</tt>.
        ''' </summary>
        ''' <exception cref="ClassCastException">            {@inheritDoc} </exception>
        ''' <exception cref="NullPointerException">          {@inheritDoc} </exception>
        ''' <seealso cref= #contains(Object) </seealso>
        Public Overridable Function containsAll(Of T1)(c As Collection(Of T1)) As Boolean Implements Collection(Of E).containsAll
            For Each e As Object In c
                If Not contains(e) Then Return False
            Next e
            Return True
        End Function

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation iterates over the specified collection, and adds
        ''' each object returned by the iterator to this collection, in turn.
        ''' 
        ''' <p>Note that this implementation will throw an
        ''' <tt>UnsupportedOperationException</tt> unless <tt>add</tt> is
        ''' overridden (assuming the specified collection is non-empty).
        ''' </summary>
        ''' <exception cref="UnsupportedOperationException"> {@inheritDoc} </exception>
        ''' <exception cref="ClassCastException">            {@inheritDoc} </exception>
        ''' <exception cref="NullPointerException">          {@inheritDoc} </exception>
        ''' <exception cref="IllegalArgumentException">      {@inheritDoc} </exception>
        ''' <exception cref="IllegalStateException">         {@inheritDoc}
        ''' </exception>
        ''' <seealso cref= #add(Object) </seealso>
        Public Overridable Function addAll(Of T1 As E)(c As Collection(Of T1)) As Boolean Implements Collection(Of E).addAll
            Dim modified As Boolean = False
            For Each e As E In c
                If add(e) Then modified = True
            Next e
            Return modified
        End Function

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation iterates over this collection, checking each
        ''' element returned by the iterator in turn to see if it's contained
        ''' in the specified collection.  If it's so contained, it's removed from
        ''' this collection with the iterator's <tt>remove</tt> method.
        ''' 
        ''' <p>Note that this implementation will throw an
        ''' <tt>UnsupportedOperationException</tt> if the iterator returned by the
        ''' <tt>iterator</tt> method does not implement the <tt>remove</tt> method
        ''' and this collection contains one or more elements in common with the
        ''' specified collection.
        ''' </summary>
        ''' <exception cref="UnsupportedOperationException"> {@inheritDoc} </exception>
        ''' <exception cref="ClassCastException">            {@inheritDoc} </exception>
        ''' <exception cref="NullPointerException">          {@inheritDoc}
        ''' </exception>
        ''' <seealso cref= #remove(Object) </seealso>
        ''' <seealso cref= #contains(Object) </seealso>
        Public Overridable Function removeAll(Of T1)(c As Collection(Of T1)) As Boolean Implements Collection(Of E).removeAll
            Objects.requireNonNull(c)
            Dim modified As Boolean = False
            'JAVA TO VB CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
            Dim it As [Iterator](Of ?) = [iterator]()
            Do While it.MoveNext()
                If c.contains(it.Current) Then
                    it.remove()
                    modified = True
                End If
            Loop
            Return modified
        End Function

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation iterates over this collection, checking each
        ''' element returned by the iterator in turn to see if it's contained
        ''' in the specified collection.  If it's not so contained, it's removed
        ''' from this collection with the iterator's <tt>remove</tt> method.
        ''' 
        ''' <p>Note that this implementation will throw an
        ''' <tt>UnsupportedOperationException</tt> if the iterator returned by the
        ''' <tt>iterator</tt> method does not implement the <tt>remove</tt> method
        ''' and this collection contains one or more elements not present in the
        ''' specified collection.
        ''' </summary>
        ''' <exception cref="UnsupportedOperationException"> {@inheritDoc} </exception>
        ''' <exception cref="ClassCastException">            {@inheritDoc} </exception>
        ''' <exception cref="NullPointerException">          {@inheritDoc}
        ''' </exception>
        ''' <seealso cref= #remove(Object) </seealso>
        ''' <seealso cref= #contains(Object) </seealso>
        Public Overridable Function retainAll(Of T1)(c As Collection(Of T1)) As Boolean Implements Collection(Of E).retainAll
            Objects.requireNonNull(c)
            Dim modified As Boolean = False
            Dim it As [Iterator](Of E) = [iterator]()
            Do While it.MoveNext()
                If Not c.contains(it.Current) Then
                    it.remove()
                    modified = True
                End If
            Loop
            Return modified
        End Function

        ''' <summary>
        ''' {@inheritDoc}
        ''' 
        ''' <p>This implementation iterates over this collection, removing each
        ''' element using the <tt>Iterator.remove</tt> operation.  Most
        ''' implementations will probably choose to override this method for
        ''' efficiency.
        ''' 
        ''' <p>Note that this implementation will throw an
        ''' <tt>UnsupportedOperationException</tt> if the iterator returned by this
        ''' collection's <tt>iterator</tt> method does not implement the
        ''' <tt>remove</tt> method and this collection is non-empty.
        ''' </summary>
        ''' <exception cref="UnsupportedOperationException"> {@inheritDoc} </exception>
        Public Overridable Sub clear() Implements Collection(Of E).clear
            Dim it As [Iterator](Of E) = [iterator]()
            Do While it.MoveNext()
                it.Current
                it.remove()
            Loop
        End Sub


        '  String conversion

        ''' <summary>
        ''' Returns a string representation of this collection.  The string
        ''' representation consists of a list of the collection's elements in the
        ''' order they are returned by its iterator, enclosed in square brackets
        ''' (<tt>"[]"</tt>).  Adjacent elements are separated by the characters
        ''' <tt>", "</tt> (comma and space).  Elements are converted to strings as
        ''' by <seealso cref="String#valueOf(Object)"/>.
        ''' </summary>
        ''' <returns> a string representation of this collection </returns>
        Public Overrides Function ToString() As String
            Dim it As [Iterator](Of E) = [iterator]()
            If Not it.hasNext() Then Return "[]"

            Dim sb As New StringBuilder
            sb.append("["c)
            Do
                Dim e As E = it.next()
                sb.append(If(e Is Me, "(this Collection)", e))
                If Not it.hasNext() Then Return sb.append("]"c).ToString()
                sb.append(","c).append(" "c)
            Loop
        End Function

    End Class

End Namespace