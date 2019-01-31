Imports System
Imports System.Diagnostics

'
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

'
' *
' *
' *
' *
' *
' * Written by Josh Bloch of Google Inc. and released to the public domain,
' * as explained at http://creativecommons.org/publicdomain/zero/1.0/.
' 

Namespace java.util


	''' <summary>
	''' Resizable-array implementation of the <seealso cref="Deque"/> interface.  Array
	''' deques have no capacity restrictions; they grow as necessary to support
	''' usage.  They are not thread-safe; in the absence of external
	''' synchronization, they do not support concurrent access by multiple threads.
	''' Null elements are prohibited.  This class is likely to be faster than
	''' <seealso cref="Stack"/> when used as a stack, and faster than <seealso cref="LinkedList"/>
	''' when used as a queue.
	''' 
	''' <p>Most {@code ArrayDeque} operations run in amortized constant time.
	''' Exceptions include <seealso cref="#remove(Object) remove"/>, {@link
	''' #removeFirstOccurrence removeFirstOccurrence}, {@link #removeLastOccurrence
	''' removeLastOccurrence}, <seealso cref="#contains contains"/>, {@link #iterator
	''' iterator.remove()}, and the bulk operations, all of which run in linear
	''' time.
	''' 
	''' <p>The iterators returned by this class's {@code iterator} method are
	''' <i>fail-fast</i>: If the deque is modified at any time after the iterator
	''' is created, in any way except through the iterator's own {@code remove}
	''' method, the iterator will generally throw a {@link
	''' ConcurrentModificationException}.  Thus, in the face of concurrent
	''' modification, the iterator fails quickly and cleanly, rather than risking
	''' arbitrary, non-deterministic behavior at an undetermined time in the
	''' future.
	''' 
	''' <p>Note that the fail-fast behavior of an iterator cannot be guaranteed
	''' as it is, generally speaking, impossible to make any hard guarantees in the
	''' presence of unsynchronized concurrent modification.  Fail-fast iterators
	''' throw {@code ConcurrentModificationException} on a best-effort basis.
	''' Therefore, it would be wrong to write a program that depended on this
	''' exception for its correctness: <i>the fail-fast behavior of iterators
	''' should be used only to detect bugs.</i>
	''' 
	''' <p>This class and its iterator implement all of the
	''' <em>optional</em> methods of the <seealso cref="Collection"/> and {@link
	''' Iterator} interfaces.
	''' 
	''' <p>This class is a member of the
	''' <a href="{@docRoot}/../technotes/guides/collections/index.html">
	''' Java Collections Framework</a>.
	''' 
	''' @author  Josh Bloch and Doug Lea
	''' @since   1.6 </summary>
	''' @param <E> the type of elements held in this collection </param>
	<Serializable> _
	Public Class ArrayDeque(Of E)
		Inherits AbstractCollection(Of E)
        Implements Deque(Of E)

        ''' <summary>
        ''' The array in which the elements of the deque are stored.
        ''' The capacity of the deque is the length of this array, which is
        ''' always a power of two. The array is never allowed to become
        ''' full, except transiently within an addX method where it is
        ''' resized (see doubleCapacity) immediately upon becoming full,
        ''' thus avoiding head and tail wrapping around to equal each
        ''' other.  We also guarantee that all array cells not holding
        ''' deque elements are always null.
        ''' </summary>
        <NonSerialized> _
		Friend elements As Object() ' non-private to simplify nested class access

		''' <summary>
		''' The index of the element at the head of the deque (which is the
		''' element that would be removed by remove() or pop()); or an
		''' arbitrary number equal to tail if the deque is empty.
		''' </summary>
		<NonSerialized> _
		Friend head As Integer

		''' <summary>
		''' The index at which the next element would be added to the tail
		''' of the deque (via addLast(E), add(E), or push(E)).
		''' </summary>
		<NonSerialized> _
		Friend tail As Integer

		''' <summary>
		''' The minimum capacity that we'll use for a newly created deque.
		''' Must be a power of 2.
		''' </summary>
		Private Const MIN_INITIAL_CAPACITY As Integer = 8

		' ******  Array allocation and resizing utilities ******

		''' <summary>
		''' Allocates empty array to hold the given number of elements.
		''' </summary>
		''' <param name="numElements">  the number of elements to hold </param>
		Private Sub allocateElements(  numElements As Integer)
			Dim initialCapacity As Integer = MIN_INITIAL_CAPACITY
			' Find the best power of two to hold elements.
			' Tests "<=" because arrays aren't kept full.
			If numElements >= initialCapacity Then
				initialCapacity = numElements
				initialCapacity = initialCapacity Or (CInt(CUInt(initialCapacity) >> 1))
				initialCapacity = initialCapacity Or (CInt(CUInt(initialCapacity) >> 2))
				initialCapacity = initialCapacity Or (CInt(CUInt(initialCapacity) >> 4))
				initialCapacity = initialCapacity Or (CInt(CUInt(initialCapacity) >> 8))
				initialCapacity = initialCapacity Or (CInt(CUInt(initialCapacity) >> 16))
				initialCapacity += 1

				If initialCapacity < 0 Then ' Too many elements, must back off initialCapacity >>>= 1 ' Good luck allocating 2 ^ 30 elements
			End If
			elements = New Object(initialCapacity - 1){}
		End Sub

		''' <summary>
		''' Doubles the capacity of this deque.  Call only when full, i.e.,
		''' when head and tail have wrapped around to become equal.
		''' </summary>
		Private Sub doubleCapacity()
			Debug.Assert(head = tail)
			Dim p As Integer = head
			Dim n As Integer = elements.Length
			Dim r As Integer = n - p ' number of elements to the right of p
			Dim newCapacity As Integer = n << 1
			If newCapacity < 0 Then Throw New IllegalStateException("Sorry, deque too big")
			Dim a As Object() = New Object(newCapacity - 1){}
			Array.Copy(elements, p, a, 0, r)
			Array.Copy(elements, 0, a, r, p)
			elements = a
			head = 0
			tail = n
		End Sub

		''' <summary>
		''' Copies the elements from our element array into the specified array,
		''' in order (from first to last element in the deque).  It is assumed
		''' that the array is large enough to hold all elements in the deque.
		''' </summary>
		''' <returns> its argument </returns>
		Private Function copyElements(Of T)(  a As T()) As T()
			If head < tail Then
				Array.Copy(elements, head, a, 0, size())
			ElseIf head > tail Then
				Dim headPortionLen As Integer = elements.Length - head
				Array.Copy(elements, head, a, 0, headPortionLen)
				Array.Copy(elements, 0, a, headPortionLen, tail)
			End If
			Return a
		End Function

		''' <summary>
		''' Constructs an empty array deque with an initial capacity
		''' sufficient to hold 16 elements.
		''' </summary>
		Public Sub New()
			elements = New Object(15){}
		End Sub

		''' <summary>
		''' Constructs an empty array deque with an initial capacity
		''' sufficient to hold the specified number of elements.
		''' </summary>
		''' <param name="numElements">  lower bound on initial capacity of the deque </param>
		Public Sub New(  numElements As Integer)
			allocateElements(numElements)
		End Sub

		''' <summary>
		''' Constructs a deque containing the elements of the specified
		''' collection, in the order they are returned by the collection's
		''' iterator.  (The first element returned by the collection's
		''' iterator becomes the first element, or <i>front</i> of the
		''' deque.)
		''' </summary>
		''' <param name="c"> the collection whose elements are to be placed into the deque </param>
		''' <exception cref="NullPointerException"> if the specified collection is null </exception>
		Public Sub New(Of T1 As E)(  c As Collection(Of T1))
			allocateElements(c.size())
			addAll(c)
		End Sub

		' The main insertion and extraction methods are addFirst,
		' addLast, pollFirst, pollLast. The other methods are defined in
		' terms of these.

		''' <summary>
		''' Inserts the specified element at the front of this deque.
		''' </summary>
		''' <param name="e"> the element to add </param>
		''' <exception cref="NullPointerException"> if the specified element is null </exception>
		Public Overridable Sub addFirst(  e As E)
			If e Is Nothing Then Throw New NullPointerException
'JAVA TO VB CONVERTER TODO TASK: Assignments within expressions are not supported in VB
			elements(head = (head - 1) And (elements.Length - 1)) = e
			If head = tail Then doubleCapacity()
		End Sub

		''' <summary>
		''' Inserts the specified element at the end of this deque.
		''' 
		''' <p>This method is equivalent to <seealso cref="#add"/>.
		''' </summary>
		''' <param name="e"> the element to add </param>
		''' <exception cref="NullPointerException"> if the specified element is null </exception>
		Public Overridable Sub addLast(  e As E)
			If e Is Nothing Then Throw New NullPointerException
			elements(tail) = e
			tail = (tail + 1) And (elements.Length - 1)
			If tail = head Then doubleCapacity()
		End Sub

		''' <summary>
		''' Inserts the specified element at the front of this deque.
		''' </summary>
		''' <param name="e"> the element to add </param>
		''' <returns> {@code true} (as specified by <seealso cref="Deque#offerFirst"/>) </returns>
		''' <exception cref="NullPointerException"> if the specified element is null </exception>
		Public Overridable Function offerFirst(  e As E) As Boolean
			addFirst(e)
			Return True
		End Function

		''' <summary>
		''' Inserts the specified element at the end of this deque.
		''' </summary>
		''' <param name="e"> the element to add </param>
		''' <returns> {@code true} (as specified by <seealso cref="Deque#offerLast"/>) </returns>
		''' <exception cref="NullPointerException"> if the specified element is null </exception>
		Public Overridable Function offerLast(  e As E) As Boolean
			addLast(e)
			Return True
		End Function

		''' <exception cref="NoSuchElementException"> {@inheritDoc} </exception>
		Public Overridable Function removeFirst() As E Implements Deque(Of E).removeFirst
			Dim x As E = pollFirst()
			If x Is Nothing Then Throw New NoSuchElementException
			Return x
		End Function

		''' <exception cref="NoSuchElementException"> {@inheritDoc} </exception>
		Public Overridable Function removeLast() As E Implements Deque(Of E).removeLast
			Dim x As E = pollLast()
			If x Is Nothing Then Throw New NoSuchElementException
			Return x
		End Function

		Public Overridable Function pollFirst() As E Implements Deque(Of E).pollFirst
			Dim h As Integer = head
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
			Dim result As E = CType(elements(h), E)
			' Element is null if deque empty
			If result Is Nothing Then Return Nothing
			elements(h) = Nothing ' Must null out slot
			head = (h + 1) And (elements.Length - 1)
			Return result
		End Function

		Public Overridable Function pollLast() As E Implements Deque(Of E).pollLast
			Dim t As Integer = (tail - 1) And (elements.Length - 1)
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
			Dim result As E = CType(elements(t), E)
			If result Is Nothing Then Return Nothing
			elements(t) = Nothing
			tail = t
			Return result
		End Function

		''' <exception cref="NoSuchElementException"> {@inheritDoc} </exception>
		Public Overridable Property first As E Implements Deque(Of E).getFirst
			Get
	'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
				Dim result As E = CType(elements(head), E)
				If result Is Nothing Then Throw New NoSuchElementException
				Return result
			End Get
		End Property

		''' <exception cref="NoSuchElementException"> {@inheritDoc} </exception>
		Public Overridable Property last As E Implements Deque(Of E).getLast
			Get
	'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
				Dim result As E = CType(elements((tail - 1) And (elements.Length - 1)), E)
				If result Is Nothing Then Throw New NoSuchElementException
				Return result
			End Get
		End Property

'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Overridable Function peekFirst() As E Implements Deque(Of E).peekFirst
			' elements[head] is null if deque empty
			Return CType(elements(head), E)
		End Function

'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Overridable Function peekLast() As E Implements Deque(Of E).peekLast
			Return CType(elements((tail - 1) And (elements.Length - 1)), E)
		End Function

		''' <summary>
		''' Removes the first occurrence of the specified element in this
		''' deque (when traversing the deque from head to tail).
		''' If the deque does not contain the element, it is unchanged.
		''' More formally, removes the first element {@code e} such that
		''' {@code o.equals(e)} (if such an element exists).
		''' Returns {@code true} if this deque contained the specified element
		''' (or equivalently, if this deque changed as a result of the call).
		''' </summary>
		''' <param name="o"> element to be removed from this deque, if present </param>
		''' <returns> {@code true} if the deque contained the specified element </returns>
		Public Overridable Function removeFirstOccurrence(  o As Object) As Boolean Implements Deque(Of E).removeFirstOccurrence
			If o Is Nothing Then Return False
			Dim mask As Integer = elements.Length - 1
			Dim i As Integer = head
			Dim x As Object
			x = elements(i)
			Do While x IsNot Nothing
				If o.Equals(x) Then
					delete(i)
					Return True
				End If
				i = (i + 1) And mask
				x = elements(i)
			Loop
			Return False
		End Function

		''' <summary>
		''' Removes the last occurrence of the specified element in this
		''' deque (when traversing the deque from head to tail).
		''' If the deque does not contain the element, it is unchanged.
		''' More formally, removes the last element {@code e} such that
		''' {@code o.equals(e)} (if such an element exists).
		''' Returns {@code true} if this deque contained the specified element
		''' (or equivalently, if this deque changed as a result of the call).
		''' </summary>
		''' <param name="o"> element to be removed from this deque, if present </param>
		''' <returns> {@code true} if the deque contained the specified element </returns>
		Public Overridable Function removeLastOccurrence(  o As Object) As Boolean Implements Deque(Of E).removeLastOccurrence
			If o Is Nothing Then Return False
			Dim mask As Integer = elements.Length - 1
			Dim i As Integer = (tail - 1) And mask
			Dim x As Object
			x = elements(i)
			Do While x IsNot Nothing
				If o.Equals(x) Then
					delete(i)
					Return True
				End If
				i = (i - 1) And mask
				x = elements(i)
			Loop
			Return False
		End Function

		' *** Queue methods ***

		''' <summary>
		''' Inserts the specified element at the end of this deque.
		''' 
		''' <p>This method is equivalent to <seealso cref="#addLast"/>.
		''' </summary>
		''' <param name="e"> the element to add </param>
		''' <returns> {@code true} (as specified by <seealso cref="Collection#add"/>) </returns>
		''' <exception cref="NullPointerException"> if the specified element is null </exception>
		Public Overridable Function add(  e As E) As Boolean
			addLast(e)
			Return True
		End Function

		''' <summary>
		''' Inserts the specified element at the end of this deque.
		''' 
		''' <p>This method is equivalent to <seealso cref="#offerLast"/>.
		''' </summary>
		''' <param name="e"> the element to add </param>
		''' <returns> {@code true} (as specified by <seealso cref="Queue#offer"/>) </returns>
		''' <exception cref="NullPointerException"> if the specified element is null </exception>
		Public Overridable Function offer(  e As E) As Boolean
			Return offerLast(e)
		End Function

		''' <summary>
		''' Retrieves and removes the head of the queue represented by this deque.
		''' 
		''' This method differs from <seealso cref="#poll poll"/> only in that it throws an
		''' exception if this deque is empty.
		''' 
		''' <p>This method is equivalent to <seealso cref="#removeFirst"/>.
		''' </summary>
		''' <returns> the head of the queue represented by this deque </returns>
		''' <exception cref="NoSuchElementException"> {@inheritDoc} </exception>
		Public Overridable Function remove() As E Implements Deque(Of E).remove
			Return removeFirst()
		End Function

		''' <summary>
		''' Retrieves and removes the head of the queue represented by this deque
		''' (in other words, the first element of this deque), or returns
		''' {@code null} if this deque is empty.
		''' 
		''' <p>This method is equivalent to <seealso cref="#pollFirst"/>.
		''' </summary>
		''' <returns> the head of the queue represented by this deque, or
		'''         {@code null} if this deque is empty </returns>
		Public Overridable Function poll() As E Implements Deque(Of E).poll
			Return pollFirst()
		End Function

		''' <summary>
		''' Retrieves, but does not remove, the head of the queue represented by
		''' this deque.  This method differs from <seealso cref="#peek peek"/> only in
		''' that it throws an exception if this deque is empty.
		''' 
		''' <p>This method is equivalent to <seealso cref="#getFirst"/>.
		''' </summary>
		''' <returns> the head of the queue represented by this deque </returns>
		''' <exception cref="NoSuchElementException"> {@inheritDoc} </exception>
		Public Overridable Function element() As E Implements Deque(Of E).element
			Return first
		End Function

		''' <summary>
		''' Retrieves, but does not remove, the head of the queue represented by
		''' this deque, or returns {@code null} if this deque is empty.
		''' 
		''' <p>This method is equivalent to <seealso cref="#peekFirst"/>.
		''' </summary>
		''' <returns> the head of the queue represented by this deque, or
		'''         {@code null} if this deque is empty </returns>
		Public Overridable Function peek() As E Implements Deque(Of E).peek
			Return peekFirst()
		End Function

		' *** Stack methods ***

		''' <summary>
		''' Pushes an element onto the stack represented by this deque.  In other
		''' words, inserts the element at the front of this deque.
		''' 
		''' <p>This method is equivalent to <seealso cref="#addFirst"/>.
		''' </summary>
		''' <param name="e"> the element to push </param>
		''' <exception cref="NullPointerException"> if the specified element is null </exception>
		Public Overridable Sub push(  e As E)
			addFirst(e)
		End Sub

		''' <summary>
		''' Pops an element from the stack represented by this deque.  In other
		''' words, removes and returns the first element of this deque.
		''' 
		''' <p>This method is equivalent to <seealso cref="#removeFirst()"/>.
		''' </summary>
		''' <returns> the element at the front of this deque (which is the top
		'''         of the stack represented by this deque) </returns>
		''' <exception cref="NoSuchElementException"> {@inheritDoc} </exception>
		Public Overridable Function pop() As E Implements Deque(Of E).pop
			Return removeFirst()
		End Function

		Private Sub checkInvariants()
			Debug.Assert(elements(tail) Is Nothing)
			Debug.Assert(head = tail ? elements(head) Is Nothing, (elements(head) IsNot Nothing AndAlso elements((tail - 1) And (elements.Length - 1)) IsNot Nothing))
			Debug.Assert(elements((head - 1) And (elements.Length - 1)) Is Nothing)
		End Sub

		''' <summary>
		''' Removes the element at the specified position in the elements array,
		''' adjusting head and tail as necessary.  This can result in motion of
		''' elements backwards or forwards in the array.
		''' 
		''' <p>This method is called delete rather than remove to emphasize
		''' that its semantics differ from those of <seealso cref="List#remove(int)"/>.
		''' </summary>
		''' <returns> true if elements moved backwards </returns>
		Private Function delete(  i As Integer) As Boolean
			checkInvariants()
			Dim elements As Object() = Me.elements
			Dim mask As Integer = elements.Length - 1
			Dim h As Integer = head
			Dim t As Integer = tail
			Dim front As Integer = (i - h) And mask
			Dim back As Integer = (t - i) And mask

			' Invariant: head <= i < tail mod circularity
			If front >= ((t - h) And mask) Then Throw New ConcurrentModificationException

			' Optimize for least element motion
			If front < back Then
				If h <= i Then
					Array.Copy(elements, h, elements, h + 1, front) ' Wrap around
				Else
					Array.Copy(elements, 0, elements, 1, i)
					elements(0) = elements(mask)
					Array.Copy(elements, h, elements, h + 1, mask - h)
				End If
				elements(h) = Nothing
				head = (h + 1) And mask
				Return False
			Else
				If i < t Then ' Copy the null tail as well
					Array.Copy(elements, i + 1, elements, i, back)
					tail = t - 1 ' Wrap around
				Else
					Array.Copy(elements, i + 1, elements, i, mask - i)
					elements(mask) = elements(0)
					Array.Copy(elements, 1, elements, 0, t)
					tail = (t - 1) And mask
				End If
				Return True
			End If
		End Function

		' *** Collection Methods ***

		''' <summary>
		''' Returns the number of elements in this deque.
		''' </summary>
		''' <returns> the number of elements in this deque </returns>
		Public Overridable Function size() As Integer Implements Deque(Of E).size
			Return (tail - head) And (elements.Length - 1)
		End Function

		''' <summary>
		''' Returns {@code true} if this deque contains no elements.
		''' </summary>
		''' <returns> {@code true} if this deque contains no elements </returns>
		Public Overridable Property empty As Boolean
			Get
				Return head = tail
			End Get
		End Property

		''' <summary>
		''' Returns an iterator over the elements in this deque.  The elements
		''' will be ordered from first (head) to last (tail).  This is the same
		''' order that elements would be dequeued (via successive calls to
		''' <seealso cref="#remove"/> or popped (via successive calls to <seealso cref="#pop"/>).
		''' </summary>
		''' <returns> an iterator over the elements in this deque </returns>
		Public Overridable Function [iterator]() As [Iterator](Of E) Implements Deque(Of E).iterator
			Return New DeqIterator(Me)
		End Function

		Public Overridable Function descendingIterator() As [Iterator](Of E) Implements Deque(Of E).descendingIterator
			Return New DescendingIterator(Me)
		End Function

		Private Class DeqIterator
			Implements Iterator(Of E)

			Private ReadOnly outerInstance As ArrayDeque

			Public Sub New(  outerInstance As ArrayDeque)
				Me.outerInstance = outerInstance
			End Sub

			''' <summary>
			''' Index of element to be returned by subsequent call to next.
			''' </summary>
			Private cursor As Integer = outerInstance.head

			''' <summary>
			''' Tail recorded at construction (also in remove), to stop
			''' iterator and also to check for comodification.
			''' </summary>
			Private fence As Integer = outerInstance.tail

			''' <summary>
			''' Index of element returned by most recent call to next.
			''' Reset to -1 if element is deleted by a call to remove.
			''' </summary>
			Private lastRet As Integer = -1

			Public Overridable Function hasNext() As Boolean Implements Iterator(Of E).hasNext
				Return cursor <> fence
			End Function

			Public Overridable Function [next]() As E Implements Iterator(Of E).next
				If cursor = fence Then Throw New NoSuchElementException
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
				Dim result As E = CType(outerInstance.elements(cursor), E)
				' This check doesn't catch all possible comodifications,
				' but does catch the ones that corrupt traversal
				If outerInstance.tail <> fence OrElse result Is Nothing Then Throw New ConcurrentModificationException
				lastRet = cursor
				cursor = (cursor + 1) And (outerInstance.elements.Length - 1)
				Return result
			End Function

			Public Overridable Sub remove() Implements Iterator(Of E).remove
				If lastRet < 0 Then Throw New IllegalStateException
				If outerInstance.delete(lastRet) Then ' if left-shifted, undo increment in next()
					cursor = (cursor - 1) And (outerInstance.elements.Length - 1)
					fence = outerInstance.tail
				End If
				lastRet = -1
			End Sub

'JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
			Public Overridable Sub forEachRemaining(Of T1)(  action As java.util.function.Consumer(Of T1)) Implements Iterator(Of E).forEachRemaining
				Objects.requireNonNull(action)
				Dim a As Object() = outerInstance.elements
				Dim m As Integer = a.Length - 1, f As Integer = fence, i As Integer = cursor
				cursor = f
				Do While i <> f
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
					Dim e As E = CType(a(i), E)
					i = (i + 1) And m
					If e Is Nothing Then Throw New ConcurrentModificationException
					action.accept(e)
				Loop
			End Sub
		End Class

		Private Class DescendingIterator
			Implements Iterator(Of E)

			Private ReadOnly outerInstance As ArrayDeque

			Public Sub New(  outerInstance As ArrayDeque)
				Me.outerInstance = outerInstance
			End Sub

	'        
	'         * This class is nearly a mirror-image of DeqIterator, using
	'         * tail instead of head for initial cursor, and head instead of
	'         * tail for fence.
	'         
			Private cursor As Integer = outerInstance.tail
			Private fence As Integer = outerInstance.head
			Private lastRet As Integer = -1

			Public Overridable Function hasNext() As Boolean Implements Iterator(Of E).hasNext
				Return cursor <> fence
			End Function

			Public Overridable Function [next]() As E Implements Iterator(Of E).next
				If cursor = fence Then Throw New NoSuchElementException
				cursor = (cursor - 1) And (outerInstance.elements.Length - 1)
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
				Dim result As E = CType(outerInstance.elements(cursor), E)
				If outerInstance.head <> fence OrElse result Is Nothing Then Throw New ConcurrentModificationException
				lastRet = cursor
				Return result
			End Function

			Public Overridable Sub remove() Implements Iterator(Of E).remove
				If lastRet < 0 Then Throw New IllegalStateException
				If Not outerInstance.delete(lastRet) Then
					cursor = (cursor + 1) And (outerInstance.elements.Length - 1)
					fence = outerInstance.head
				End If
				lastRet = -1
			End Sub
		End Class

		''' <summary>
		''' Returns {@code true} if this deque contains the specified element.
		''' More formally, returns {@code true} if and only if this deque contains
		''' at least one element {@code e} such that {@code o.equals(e)}.
		''' </summary>
		''' <param name="o"> object to be checked for containment in this deque </param>
		''' <returns> {@code true} if this deque contains the specified element </returns>
		Public Overridable Function contains(  o As Object) As Boolean Implements Deque(Of E).contains
			If o Is Nothing Then Return False
			Dim mask As Integer = elements.Length - 1
			Dim i As Integer = head
			Dim x As Object
			x = elements(i)
			Do While x IsNot Nothing
				If o.Equals(x) Then Return True
				i = (i + 1) And mask
				x = elements(i)
			Loop
			Return False
		End Function

		''' <summary>
		''' Removes a single instance of the specified element from this deque.
		''' If the deque does not contain the element, it is unchanged.
		''' More formally, removes the first element {@code e} such that
		''' {@code o.equals(e)} (if such an element exists).
		''' Returns {@code true} if this deque contained the specified element
		''' (or equivalently, if this deque changed as a result of the call).
		''' 
		''' <p>This method is equivalent to <seealso cref="#removeFirstOccurrence(Object)"/>.
		''' </summary>
		''' <param name="o"> element to be removed from this deque, if present </param>
		''' <returns> {@code true} if this deque contained the specified element </returns>
		Public Overridable Function remove(  o As Object) As Boolean Implements Deque(Of E).remove
			Return removeFirstOccurrence(o)
		End Function

		''' <summary>
		''' Removes all of the elements from this deque.
		''' The deque will be empty after this call returns.
		''' </summary>
		Public Overridable Sub clear()
			Dim h As Integer = head
			Dim t As Integer = tail
			If h <> t Then ' clear all cells
					tail = 0
					head = tail
				Dim i As Integer = h
				Dim mask As Integer = elements.Length - 1
				Do
					elements(i) = Nothing
					i = (i + 1) And mask
				Loop While i <> t
			End If
		End Sub

		''' <summary>
		''' Returns an array containing all of the elements in this deque
		''' in proper sequence (from first to last element).
		''' 
		''' <p>The returned array will be "safe" in that no references to it are
		''' maintained by this deque.  (In other words, this method must allocate
		''' a new array).  The caller is thus free to modify the returned array.
		''' 
		''' <p>This method acts as bridge between array-based and collection-based
		''' APIs.
		''' </summary>
		''' <returns> an array containing all of the elements in this deque </returns>
		Public Overridable Function toArray() As Object()
			Return copyElements(New Object(size() - 1){})
		End Function

		''' <summary>
		''' Returns an array containing all of the elements in this deque in
		''' proper sequence (from first to last element); the runtime type of the
		''' returned array is that of the specified array.  If the deque fits in
		''' the specified array, it is returned therein.  Otherwise, a new array
		''' is allocated with the runtime type of the specified array and the
		''' size of this deque.
		''' 
		''' <p>If this deque fits in the specified array with room to spare
		''' (i.e., the array has more elements than this deque), the element in
		''' the array immediately following the end of the deque is set to
		''' {@code null}.
		''' 
		''' <p>Like the <seealso cref="#toArray()"/> method, this method acts as bridge between
		''' array-based and collection-based APIs.  Further, this method allows
		''' precise control over the runtime type of the output array, and may,
		''' under certain circumstances, be used to save allocation costs.
		''' 
		''' <p>Suppose {@code x} is a deque known to contain only strings.
		''' The following code can be used to dump the deque into a newly
		''' allocated array of {@code String}:
		''' 
		'''  <pre> {@code String[] y = x.toArray(new String[0]);}</pre>
		''' 
		''' Note that {@code toArray(new Object[0])} is identical in function to
		''' {@code toArray()}.
		''' </summary>
		''' <param name="a"> the array into which the elements of the deque are to
		'''          be stored, if it is big enough; otherwise, a new array of the
		'''          same runtime type is allocated for this purpose </param>
		''' <returns> an array containing all of the elements in this deque </returns>
		''' <exception cref="ArrayStoreException"> if the runtime type of the specified array
		'''         is not a supertype of the runtime type of every element in
		'''         this deque </exception>
		''' <exception cref="NullPointerException"> if the specified array is null </exception>
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Overridable Function toArray(Of T)(  a As T()) As T()
			Dim size As Integer = size()
			If a.Length < size Then a = CType(java.lang.reflect.Array.newInstance(a.GetType().GetElementType(), size), T())
			copyElements(a)
			If a.Length > size Then a(size) = Nothing
			Return a
		End Function

		' *** Object methods ***

		''' <summary>
		''' Returns a copy of this deque.
		''' </summary>
		''' <returns> a copy of this deque </returns>
		Public Overridable Function clone() As ArrayDeque(Of E)
			Try
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
				Dim result As ArrayDeque(Of E) = CType(MyBase.clone(), ArrayDeque(Of E))
				result.elements = New java.lang.Object(elements.length - 1){}
				Array.Copy(elements, result.elements, elements.Length)
				Return result
			Catch e As CloneNotSupportedException
				Throw New AssertionError
			End Try
		End Function

		Private Const serialVersionUID As Long = 2340985798034038923L

		''' <summary>
		''' Saves this deque to a stream (that is, serializes it).
		''' 
		''' @serialData The current size ({@code int}) of the deque,
		''' followed by all of its elements (each an object reference) in
		''' first-to-last order.
		''' </summary>
		Private Sub writeObject(  s As java.io.ObjectOutputStream)
			s.defaultWriteObject()

			' Write out size
			s.writeInt(size())

			' Write out elements in order.
			Dim mask As Integer = elements.Length - 1
			Dim i As Integer = head
			Do While i <> tail
				s.writeObject(elements(i))
				i = (i + 1) And mask
			Loop
		End Sub

		''' <summary>
		''' Reconstitutes this deque from a stream (that is, deserializes it).
		''' </summary>
		Private Sub readObject(  s As java.io.ObjectInputStream)
			s.defaultReadObject()

			' Read in size and allocate array
			Dim size As Integer = s.readInt()
			allocateElements(size)
			head = 0
			tail = size

			' Read in all elements in the proper order.
			For i As Integer = 0 To size - 1
				elements(i) = s.readObject()
			Next i
		End Sub

		''' <summary>
		''' Creates a <em><a href="Spliterator.html#binding">late-binding</a></em>
		''' and <em>fail-fast</em> <seealso cref="Spliterator"/> over the elements in this
		''' deque.
		''' 
		''' <p>The {@code Spliterator} reports <seealso cref="Spliterator#SIZED"/>,
		''' <seealso cref="Spliterator#SUBSIZED"/>, <seealso cref="Spliterator#ORDERED"/>, and
		''' <seealso cref="Spliterator#NONNULL"/>.  Overriding implementations should document
		''' the reporting of additional characteristic values.
		''' </summary>
		''' <returns> a {@code Spliterator} over the elements in this deque
		''' @since 1.8 </returns>
		Public Overridable Function spliterator() As Spliterator(Of E)
			Return New DeqSpliterator(Of E)(Me, -1, -1)
		End Function

		Friend NotInheritable Class DeqSpliterator(Of E)
			Implements Spliterator(Of E)

			Private ReadOnly deq As ArrayDeque(Of E)
			Private fence As Integer ' -1 until first use
			Private index As Integer ' current index, modified on traverse/split

			''' <summary>
			''' Creates new spliterator covering the given array and range </summary>
			Friend Sub New(  deq As ArrayDeque(Of E),   origin As Integer,   fence As Integer)
				Me.deq = deq
				Me.index = origin
				Me.fence = fence
			End Sub

			Private Property fence As Integer
				Get
					Dim t As Integer
					t = fence
					If t < 0 Then
							fence = deq.tail
							t = fence
						index = deq.head
					End If
					Return t
				End Get
			End Property

			Public Function trySplit() As DeqSpliterator(Of E)
				Dim t As Integer = fence, h As Integer = index, n As Integer = deq.elements.Length
				If h <> t AndAlso ((h + 1) And (n - 1)) <> t Then
					If h > t Then t += n
					Dim m As Integer = (CInt(CUInt((h + t)) >> 1)) And (n - 1)
'JAVA TO VB CONVERTER TODO TASK: Assignments within expressions are not supported in VB
					Return New DeqSpliterator(Of )(deq, h, index = m)
				End If
				Return Nothing
			End Function

'JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
			Public Sub forEachRemaining(Of T1)(  consumer As java.util.function.Consumer(Of T1)) Implements Spliterator(Of E).forEachRemaining
				If consumer Is Nothing Then Throw New NullPointerException
				Dim a As Object() = deq.elements
				Dim m As Integer = a.Length - 1, f As Integer = fence, i As Integer = index
				index = f
				Do While i <> f
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
					Dim e As E = CType(a(i), E)
					i = (i + 1) And m
					If e Is Nothing Then Throw New ConcurrentModificationException
					consumer.accept(e)
				Loop
			End Sub

'JAVA TO VB CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
			Public Function tryAdvance(Of T1)(  consumer As java.util.function.Consumer(Of T1)) As Boolean Implements Spliterator(Of E).tryAdvance
				If consumer Is Nothing Then Throw New NullPointerException
				Dim a As Object() = deq.elements
				Dim m As Integer = a.Length - 1, f As Integer = fence, i As Integer = index
				If i <> fence Then
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
					Dim e As E = CType(a(i), E)
					index = (i + 1) And m
					If e Is Nothing Then Throw New ConcurrentModificationException
					consumer.accept(e)
					Return True
				End If
				Return False
			End Function

			Public Function estimateSize() As Long Implements Spliterator(Of E).estimateSize
				Dim n As Integer = fence - index
				If n < 0 Then n += deq.elements.Length
				Return CLng(n)
			End Function

			Public Overrides Function characteristics() As Integer Implements Spliterator(Of E).characteristics
				Return Spliterator.ORDERED Or Spliterator.SIZED Or Spliterator.NONNULL Or Spliterator.SUBSIZED
			End Function
		End Class

	End Class

End Namespace