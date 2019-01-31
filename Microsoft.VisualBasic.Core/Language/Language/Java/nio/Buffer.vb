'
' * Copyright (c) 2000, 2013, Oracle and/or its affiliates. All rights reserved.
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

Namespace java.nio


	''' <summary>
	''' A container for data of a specific primitive type.
	''' 
	''' <p> A buffer is a linear, finite sequence of elements of a specific
	''' primitive type.  Aside from its content, the essential properties of a
	''' buffer are its capacity, limit, and position: </p>
	''' 
	''' <blockquote>
	''' 
	'''   <p> A buffer's <i>capacity</i> is the number of elements it contains.  The
	'''   capacity of a buffer is never negative and never changes.  </p>
	''' 
	'''   <p> A buffer's <i>limit</i> is the index of the first element that should
	'''   not be read or written.  A buffer's limit is never negative and is never
	'''   greater than its capacity.  </p>
	''' 
	'''   <p> A buffer's <i>position</i> is the index of the next element to be
	'''   read or written.  A buffer's position is never negative and is never
	'''   greater than its limit.  </p>
	''' 
	''' </blockquote>
	''' 
	''' <p> There is one subclass of this class for each non-boolean primitive type.
	''' 
	''' 
	''' <h2> Transferring data </h2>
	''' 
	''' <p> Each subclass of this class defines two categories of <i>get</i> and
	''' <i>put</i> operations: </p>
	''' 
	''' <blockquote>
	''' 
	'''   <p> <i>Relative</i> operations read or write one or more elements starting
	'''   at the current position and then increment the position by the number of
	'''   elements transferred.  If the requested transfer exceeds the limit then a
	'''   relative <i>get</i> operation throws a <seealso cref="BufferUnderflowException"/>
	'''   and a relative <i>put</i> operation throws a {@link
	'''   BufferOverflowException}; in either case, no data is transferred.  </p>
	''' 
	'''   <p> <i>Absolute</i> operations take an explicit element index and do not
	'''   affect the position.  Absolute <i>get</i> and <i>put</i> operations throw
	'''   an <seealso cref="IndexOutOfBoundsException"/> if the index argument exceeds the
	'''   limit.  </p>
	''' 
	''' </blockquote>
	''' 
	''' <p> Data may also, of course, be transferred in to or out of a buffer by the
	''' I/O operations of an appropriate channel, which are always relative to the
	''' current position.
	''' 
	''' 
	''' <h2> Marking and resetting </h2>
	''' 
	''' <p> A buffer's <i>mark</i> is the index to which its position will be reset
	''' when the <seealso cref="#reset reset"/> method is invoked.  The mark is not always
	''' defined, but when it is defined it is never negative and is never greater
	''' than the position.  If the mark is defined then it is discarded when the
	''' position or the limit is adjusted to a value smaller than the mark.  If the
	''' mark is not defined then invoking the <seealso cref="#reset reset"/> method causes an
	''' <seealso cref="InvalidMarkException"/> to be thrown.
	''' 
	''' 
	''' <h2> Invariants </h2>
	''' 
	''' <p> The following invariant holds for the mark, position, limit, and
	''' capacity values:
	''' 
	''' <blockquote>
	'''     <tt>0</tt> <tt>&lt;=</tt>
	'''     <i>mark</i> <tt>&lt;=</tt>
	'''     <i>position</i> <tt>&lt;=</tt>
	'''     <i>limit</i> <tt>&lt;=</tt>
	'''     <i>capacity</i>
	''' </blockquote>
	''' 
	''' <p> A newly-created buffer always has a position of zero and a mark that is
	''' undefined.  The initial limit may be zero, or it may be some other value
	''' that depends upon the type of the buffer and the manner in which it is
	''' constructed.  Each element of a newly-allocated buffer is initialized
	''' to zero.
	''' 
	''' 
	''' <h2> Clearing, flipping, and rewinding </h2>
	''' 
	''' <p> In addition to methods for accessing the position, limit, and capacity
	''' values and for marking and resetting, this class also defines the following
	''' operations upon buffers:
	''' 
	''' <ul>
	''' 
	'''   <li><p> <seealso cref="#clear"/> makes a buffer ready for a new sequence of
	'''   channel-read or relative <i>put</i> operations: It sets the limit to the
	'''   capacity and the position to zero.  </p></li>
	''' 
	'''   <li><p> <seealso cref="#flip"/> makes a buffer ready for a new sequence of
	'''   channel-write or relative <i>get</i> operations: It sets the limit to the
	'''   current position and then sets the position to zero.  </p></li>
	''' 
	'''   <li><p> <seealso cref="#rewind"/> makes a buffer ready for re-reading the data that
	'''   it already contains: It leaves the limit unchanged and sets the position
	'''   to zero.  </p></li>
	''' 
	''' </ul>
	''' 
	''' 
	''' <h2> Read-only buffers </h2>
	''' 
	''' <p> Every buffer is readable, but not every buffer is writable.  The
	''' mutation methods of each buffer class are specified as <i>optional
	''' operations</i> that will throw a <seealso cref="ReadOnlyBufferException"/> when
	''' invoked upon a read-only buffer.  A read-only buffer does not allow its
	''' content to be changed, but its mark, position, and limit values are mutable.
	''' Whether or not a buffer is read-only may be determined by invoking its
	''' <seealso cref="#isReadOnly isReadOnly"/> method.
	''' 
	''' 
	''' <h2> Thread safety </h2>
	''' 
	''' <p> Buffers are not safe for use by multiple concurrent threads.  If a
	''' buffer is to be used by more than one thread then access to the buffer
	''' should be controlled by appropriate synchronization.
	''' 
	''' 
	''' <h2> Invocation chaining </h2>
	''' 
	''' <p> Methods in this class that do not otherwise have a value to return are
	''' specified to return the buffer upon which they are invoked.  This allows
	''' method invocations to be chained; for example, the sequence of statements
	''' 
	''' <blockquote><pre>
	''' b.flip();
	''' b.position(23);
	''' b.limit(42);</pre></blockquote>
	''' 
	''' can be replaced by the single, more compact statement
	''' 
	''' <blockquote><pre>
	''' b.flip().position(23).limit(42);</pre></blockquote>
	''' 
	''' 
	''' @author Mark Reinhold
	''' @author JSR-51 Expert Group
	''' @since 1.4
	''' </summary>

	Public MustInherit Class Buffer

		''' <summary>
		''' The characteristics of Spliterators that traverse and split elements
		''' maintained in Buffers.
		''' </summary>
		Friend Shared ReadOnly SPLITERATOR_CHARACTERISTICS As Integer = java.util.Spliterator.SIZED Or java.util.Spliterator.SUBSIZED Or java.util.Spliterator.ORDERED

		' Invariants: mark <= position <= limit <= capacity
		Private mark_Renamed As Integer = -1
		Private position_Renamed As Integer = 0
		Private limit_Renamed As Integer
		Private capacity_Renamed As Integer

		' Used only by direct buffers
		' NOTE: hoisted here for speed in JNI GetDirectBufferAddress
		Friend address As Long

		' Creates a new buffer with the given mark, position, limit, and capacity,
		' after checking invariants.
		'
		Friend Sub New(  mark As Integer,   pos As Integer,   lim As Integer,   cap As Integer) ' package-private
			If cap < 0 Then Throw New IllegalArgumentException("Negative capacity: " & cap)
			Me.capacity_Renamed = cap
			limit(lim)
			position(pos)
			If mark >= 0 Then
				If mark > pos Then Throw New IllegalArgumentException("mark > position: (" & mark & " > " & pos & ")")
				Me.mark_Renamed = mark
			End If
		End Sub

		''' <summary>
		''' Returns this buffer's capacity.
		''' </summary>
		''' <returns>  The capacity of this buffer </returns>
		Public Function capacity() As Integer
			Return capacity_Renamed
		End Function

		''' <summary>
		''' Returns this buffer's position.
		''' </summary>
		''' <returns>  The position of this buffer </returns>
		Public Function position() As Integer
			Return position_Renamed
		End Function

		''' <summary>
		''' Sets this buffer's position.  If the mark is defined and larger than the
		''' new position then it is discarded.
		''' </summary>
		''' <param name="newPosition">
		'''         The new position value; must be non-negative
		'''         and no larger than the current limit
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="IllegalArgumentException">
		'''          If the preconditions on <tt>newPosition</tt> do not hold </exception>
		Public Function position(  newPosition As Integer) As Buffer
			If (newPosition > limit_Renamed) OrElse (newPosition < 0) Then Throw New IllegalArgumentException
			position_Renamed = newPosition
			If mark_Renamed > position_Renamed Then mark_Renamed = -1
			Return Me
		End Function

		''' <summary>
		''' Returns this buffer's limit.
		''' </summary>
		''' <returns>  The limit of this buffer </returns>
		Public Function limit() As Integer
			Return limit_Renamed
		End Function

		''' <summary>
		''' Sets this buffer's limit.  If the position is larger than the new limit
		''' then it is set to the new limit.  If the mark is defined and larger than
		''' the new limit then it is discarded.
		''' </summary>
		''' <param name="newLimit">
		'''         The new limit value; must be non-negative
		'''         and no larger than this buffer's capacity
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="IllegalArgumentException">
		'''          If the preconditions on <tt>newLimit</tt> do not hold </exception>
		Public Function limit(  newLimit As Integer) As Buffer
			If (newLimit > capacity_Renamed) OrElse (newLimit < 0) Then Throw New IllegalArgumentException
			limit_Renamed = newLimit
			If position_Renamed > limit_Renamed Then position_Renamed = limit_Renamed
			If mark_Renamed > limit_Renamed Then mark_Renamed = -1
			Return Me
		End Function

		''' <summary>
		''' Sets this buffer's mark at its position.
		''' </summary>
		''' <returns>  This buffer </returns>
		Public Function mark() As Buffer
			mark_Renamed = position_Renamed
			Return Me
		End Function

		''' <summary>
		''' Resets this buffer's position to the previously-marked position.
		''' 
		''' <p> Invoking this method neither changes nor discards the mark's
		''' value. </p>
		''' </summary>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="InvalidMarkException">
		'''          If the mark has not been set </exception>
		Public Function reset() As Buffer
			Dim m As Integer = mark_Renamed
			If m < 0 Then Throw New InvalidMarkException
			position_Renamed = m
			Return Me
		End Function

		''' <summary>
		''' Clears this buffer.  The position is set to zero, the limit is set to
		''' the capacity, and the mark is discarded.
		''' 
		''' <p> Invoke this method before using a sequence of channel-read or
		''' <i>put</i> operations to fill this buffer.  For example:
		''' 
		''' <blockquote><pre>
		''' buf.clear();     // Prepare buffer for reading
		''' in.read(buf);    // Read data</pre></blockquote>
		''' 
		''' <p> This method does not actually erase the data in the buffer, but it
		''' is named as if it did because it will most often be used in situations
		''' in which that might as well be the case. </p>
		''' </summary>
		''' <returns>  This buffer </returns>
		Public Function clear() As Buffer
			position_Renamed = 0
			limit_Renamed = capacity_Renamed
			mark_Renamed = -1
			Return Me
		End Function

		''' <summary>
		''' Flips this buffer.  The limit is set to the current position and then
		''' the position is set to zero.  If the mark is defined then it is
		''' discarded.
		''' 
		''' <p> After a sequence of channel-read or <i>put</i> operations, invoke
		''' this method to prepare for a sequence of channel-write or relative
		''' <i>get</i> operations.  For example:
		''' 
		''' <blockquote><pre>
		''' buf.put(magic);    // Prepend header
		''' in.read(buf);      // Read data into rest of buffer
		''' buf.flip();        // Flip buffer
		''' out.write(buf);    // Write header + data to channel</pre></blockquote>
		''' 
		''' <p> This method is often used in conjunction with the {@link
		''' java.nio.ByteBuffer#compact compact} method when transferring data from
		''' one place to another.  </p>
		''' </summary>
		''' <returns>  This buffer </returns>
		Public Function flip() As Buffer
			limit_Renamed = position_Renamed
			position_Renamed = 0
			mark_Renamed = -1
			Return Me
		End Function

		''' <summary>
		''' Rewinds this buffer.  The position is set to zero and the mark is
		''' discarded.
		''' 
		''' <p> Invoke this method before a sequence of channel-write or <i>get</i>
		''' operations, assuming that the limit has already been set
		''' appropriately.  For example:
		''' 
		''' <blockquote><pre>
		''' out.write(buf);    // Write remaining data
		''' buf.rewind();      // Rewind buffer
		''' buf.get(array);    // Copy data into array</pre></blockquote>
		''' </summary>
		''' <returns>  This buffer </returns>
		Public Function rewind() As Buffer
			position_Renamed = 0
			mark_Renamed = -1
			Return Me
		End Function

		''' <summary>
		''' Returns the number of elements between the current position and the
		''' limit.
		''' </summary>
		''' <returns>  The number of elements remaining in this buffer </returns>
		Public Function remaining() As Integer
			Return limit_Renamed - position_Renamed
		End Function

		''' <summary>
		''' Tells whether there are any elements between the current position and
		''' the limit.
		''' </summary>
		''' <returns>  <tt>true</tt> if, and only if, there is at least one element
		'''          remaining in this buffer </returns>
		Public Function hasRemaining() As Boolean
			Return position_Renamed < limit_Renamed
		End Function

		''' <summary>
		''' Tells whether or not this buffer is read-only.
		''' </summary>
		''' <returns>  <tt>true</tt> if, and only if, this buffer is read-only </returns>
		Public MustOverride ReadOnly Property [readOnly] As Boolean

		''' <summary>
		''' Tells whether or not this buffer is backed by an accessible
		''' array.
		''' 
		''' <p> If this method returns <tt>true</tt> then the <seealso cref="#array() array"/>
		''' and <seealso cref="#arrayOffset() arrayOffset"/> methods may safely be invoked.
		''' </p>
		''' </summary>
		''' <returns>  <tt>true</tt> if, and only if, this buffer
		'''          is backed by an array and is not read-only
		''' 
		''' @since 1.6 </returns>
		Public MustOverride Function hasArray() As Boolean

		''' <summary>
		''' Returns the array that backs this
		''' buffer&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> This method is intended to allow array-backed buffers to be
		''' passed to native code more efficiently. Concrete subclasses
		''' provide more strongly-typed return values for this method.
		''' 
		''' <p> Modifications to this buffer's content will cause the returned
		''' array's content to be modified, and vice versa.
		''' 
		''' <p> Invoke the <seealso cref="#hasArray hasArray"/> method before invoking this
		''' method in order to ensure that this buffer has an accessible backing
		''' array.  </p>
		''' </summary>
		''' <returns>  The array that backs this buffer
		''' </returns>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is backed by an array but is read-only
		''' </exception>
		''' <exception cref="UnsupportedOperationException">
		'''          If this buffer is not backed by an accessible array
		''' 
		''' @since 1.6 </exception>
		Public MustOverride Function array() As Object

		''' <summary>
		''' Returns the offset within this buffer's backing array of the first
		''' element of the buffer&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> If this buffer is backed by an array then buffer position <i>p</i>
		''' corresponds to array index <i>p</i>&nbsp;+&nbsp;<tt>arrayOffset()</tt>.
		''' 
		''' <p> Invoke the <seealso cref="#hasArray hasArray"/> method before invoking this
		''' method in order to ensure that this buffer has an accessible backing
		''' array.  </p>
		''' </summary>
		''' <returns>  The offset within this buffer's array
		'''          of the first element of the buffer
		''' </returns>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is backed by an array but is read-only
		''' </exception>
		''' <exception cref="UnsupportedOperationException">
		'''          If this buffer is not backed by an accessible array
		''' 
		''' @since 1.6 </exception>
		Public MustOverride Function arrayOffset() As Integer

		''' <summary>
		''' Tells whether or not this buffer is
		''' <a href="ByteBuffer.html#direct"><i>direct</i></a>.
		''' </summary>
		''' <returns>  <tt>true</tt> if, and only if, this buffer is direct
		''' 
		''' @since 1.6 </returns>
		Public MustOverride ReadOnly Property direct As Boolean


		' -- Package-private methods for bounds checking, etc. --

		''' <summary>
		''' Checks the current position against the limit, throwing a {@link
		''' BufferUnderflowException} if it is not smaller than the limit, and then
		''' increments the position.
		''' </summary>
		''' <returns>  The current position value, before it is incremented </returns>
		Friend Function nextGetIndex() As Integer ' package-private
			If position_Renamed >= limit_Renamed Then Throw New BufferUnderflowException
				Dim tempVar As Integer = position_Renamed
				position_Renamed += 1
				Return tempVar
		End Function

		Friend Function nextGetIndex(  nb As Integer) As Integer ' package-private
			If limit_Renamed - position_Renamed < nb Then Throw New BufferUnderflowException
			Dim p As Integer = position_Renamed
			position_Renamed += nb
			Return p
		End Function

		''' <summary>
		''' Checks the current position against the limit, throwing a {@link
		''' BufferOverflowException} if it is not smaller than the limit, and then
		''' increments the position.
		''' </summary>
		''' <returns>  The current position value, before it is incremented </returns>
		Friend Function nextPutIndex() As Integer ' package-private
			If position_Renamed >= limit_Renamed Then Throw New BufferOverflowException
				Dim tempVar As Integer = position_Renamed
				position_Renamed += 1
				Return tempVar
		End Function

		Friend Function nextPutIndex(  nb As Integer) As Integer ' package-private
			If limit_Renamed - position_Renamed < nb Then Throw New BufferOverflowException
			Dim p As Integer = position_Renamed
			position_Renamed += nb
			Return p
		End Function

		''' <summary>
		''' Checks the given index against the limit, throwing an {@link
		''' IndexOutOfBoundsException} if it is not smaller than the limit
		''' or is smaller than zero.
		''' </summary>
		Friend Function checkIndex(  i As Integer) As Integer ' package-private
			If (i < 0) OrElse (i >= limit_Renamed) Then Throw New IndexOutOfBoundsException
			Return i
		End Function

		Friend Function checkIndex(  i As Integer,   nb As Integer) As Integer ' package-private
			If (i < 0) OrElse (nb > limit_Renamed - i) Then Throw New IndexOutOfBoundsException
			Return i
		End Function

		Friend Function markValue() As Integer ' package-private
			Return mark_Renamed
		End Function

		Friend Sub truncate() ' package-private
			mark_Renamed = -1
			position_Renamed = 0
			limit_Renamed = 0
			capacity_Renamed = 0
		End Sub

		Friend Sub discardMark() ' package-private
			mark_Renamed = -1
		End Sub

		Friend Shared Sub checkBounds(  [off] As Integer,   len As Integer,   size As Integer) ' package-private
			If ([off] Or len Or ([off] + len) Or (size - ([off] + len))) < 0 Then Throw New IndexOutOfBoundsException
		End Sub

	End Class

End Namespace