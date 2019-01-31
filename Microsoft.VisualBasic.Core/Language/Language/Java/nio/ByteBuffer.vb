Imports System

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

' -- This file was mechanically generated: Do not edit! -- //

Namespace java.nio










	''' <summary>
	''' A byte buffer.
	''' 
	''' <p> This class defines six categories of operations upon
	''' byte buffers:
	''' 
	''' <ul>
	''' 
	'''   <li><p> Absolute and relative <seealso cref="#get() <i>get</i>"/> and
	'''   <seealso cref="#put(byte) <i>put</i>"/> methods that read and write
	'''   single bytes; </p></li>
	''' 
	'''   <li><p> Relative <seealso cref="#get(byte[]) <i>bulk get</i>"/>
	'''   methods that transfer contiguous sequences of bytes from this buffer
	'''   into an array; </p></li>
	''' 
	'''   <li><p> Relative <seealso cref="#put(byte[]) <i>bulk put</i>"/>
	'''   methods that transfer contiguous sequences of bytes from a
	'''   byte array or some other byte
	'''   buffer into this buffer; </p></li>
	''' 
	''' 
	''' 
	'''   <li><p> Absolute and relative <seealso cref="#getChar() <i>get</i>"/>
	'''   and <seealso cref="#putChar(char) <i>put</i>"/> methods that read and
	'''   write values of other primitive types, translating them to and from
	'''   sequences of bytes in a particular byte order; </p></li>
	''' 
	'''   <li><p> Methods for creating <i><a href="#views">view buffers</a></i>,
	'''   which allow a byte buffer to be viewed as a buffer containing values of
	'''   some other primitive type; and </p></li>
	''' 
	''' 
	''' 
	'''   <li><p> Methods for <seealso cref="#compact compacting"/>, {@link
	'''   #duplicate duplicating}, and <seealso cref="#slice slicing"/>
	'''   a byte buffer.  </p></li>
	''' 
	''' </ul>
	''' 
	''' <p> Byte buffers can be created either by {@link #allocate
	''' <i>allocation</i>}, which allocates space for the buffer's
	''' 
	''' 
	''' 
	''' content, or by <seealso cref="#wrap(byte[]) <i>wrapping</i>"/> an
	''' existing byte array  into a buffer.
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' <a name="direct"></a>
	''' <h2> Direct <i>vs.</i> non-direct buffers </h2>
	''' 
	''' <p> A byte buffer is either <i>direct</i> or <i>non-direct</i>.  Given a
	''' direct byte buffer, the Java virtual machine will make a best effort to
	''' perform native I/O operations directly upon it.  That is, it will attempt to
	''' avoid copying the buffer's content to (or from) an intermediate buffer
	''' before (or after) each invocation of one of the underlying operating
	''' system's native I/O operations.
	''' 
	''' <p> A direct byte buffer may be created by invoking the {@link
	''' #allocateDirect(int) allocateDirect} factory method of this class.  The
	''' buffers returned by this method typically have somewhat higher allocation
	''' and deallocation costs than non-direct buffers.  The contents of direct
	''' buffers may reside outside of the normal garbage-collected heap, and so
	''' their impact upon the memory footprint of an application might not be
	''' obvious.  It is therefore recommended that direct buffers be allocated
	''' primarily for large, long-lived buffers that are subject to the underlying
	''' system's native I/O operations.  In general it is best to allocate direct
	''' buffers only when they yield a measureable gain in program performance.
	''' 
	''' <p> A direct byte buffer may also be created by {@link
	''' java.nio.channels.FileChannel#map mapping} a region of a file
	''' directly into memory.  An implementation of the Java platform may optionally
	''' support the creation of direct byte buffers from native code via JNI.  If an
	''' instance of one of these kinds of buffers refers to an inaccessible region
	''' of memory then an attempt to access that region will not change the buffer's
	''' content and will cause an unspecified exception to be thrown either at the
	''' time of the access or at some later time.
	''' 
	''' <p> Whether a byte buffer is direct or non-direct may be determined by
	''' invoking its <seealso cref="#isDirect isDirect"/> method.  This method is provided so
	''' that explicit buffer management can be done in performance-critical code.
	''' 
	''' 
	''' <a name="bin"></a>
	''' <h2> Access to binary data </h2>
	''' 
	''' <p> This class defines methods for reading and writing values of all other
	''' primitive types, except <tt>boolean</tt>.  Primitive values are translated
	''' to (or from) sequences of bytes according to the buffer's current byte
	''' order, which may be retrieved and modified via the <seealso cref="#order order"/>
	''' methods.  Specific byte orders are represented by instances of the {@link
	''' ByteOrder} class.  The initial order of a byte buffer is always {@link
	''' ByteOrder#BIG_ENDIAN BIG_ENDIAN}.
	''' 
	''' <p> For access to heterogeneous binary data, that is, sequences of values of
	''' different types, this class defines a family of absolute and relative
	''' <i>get</i> and <i>put</i> methods for each type.  For 32-bit floating-point
	''' values, for example, this class defines:
	''' 
	''' <blockquote><pre>
	''' float  <seealso cref="#getFloat()"/>
	''' float  <seealso cref="#getFloat(int) getFloat(int index)"/>
	'''   Sub   <seealso cref="#putFloat(float) putFloat(float f)"/>
	'''   Sub   <seealso cref="#putFloat(int,float) putFloat(int index, float f)"/></pre></blockquote>
	''' 
	''' <p> Corresponding methods are defined for the types <tt>char</tt>,
	''' <tt>short</tt>, <tt>int</tt>, <tt>long</tt>, and <tt>double</tt>.  The index
	''' parameters of the absolute <i>get</i> and <i>put</i> methods are in terms of
	''' bytes rather than of the type being read or written.
	''' 
	''' <a name="views"></a>
	''' 
	''' <p> For access to homogeneous binary data, that is, sequences of values of
	''' the same type, this class defines methods that can create <i>views</i> of a
	''' given byte buffer.  A <i>view buffer</i> is simply another buffer whose
	''' content is backed by the byte buffer.  Changes to the byte buffer's content
	''' will be visible in the view buffer, and vice versa; the two buffers'
	''' position, limit, and mark values are independent.  The {@link
	''' #asFloatBuffer() asFloatBuffer} method, for example, creates an instance of
	''' the <seealso cref="FloatBuffer"/> class that is backed by the byte buffer upon which
	''' the method is invoked.  Corresponding view-creation methods are defined for
	''' the types <tt>char</tt>, <tt>short</tt>, <tt>int</tt>, <tt>long</tt>, and
	''' <tt>double</tt>.
	''' 
	''' <p> View buffers have three important advantages over the families of
	''' type-specific <i>get</i> and <i>put</i> methods described above:
	''' 
	''' <ul>
	''' 
	'''   <li><p> A view buffer is indexed not in terms of bytes but rather in terms
	'''   of the type-specific size of its values;  </p></li>
	''' 
	'''   <li><p> A view buffer provides relative bulk <i>get</i> and <i>put</i>
	'''   methods that can transfer contiguous sequences of values between a buffer
	'''   and an array or some other buffer of the same type; and  </p></li>
	''' 
	'''   <li><p> A view buffer is potentially much more efficient because it will
	'''   be direct if, and only if, its backing byte buffer is direct.  </p></li>
	''' 
	''' </ul>
	''' 
	''' <p> The byte order of a view buffer is fixed to be that of its byte buffer
	''' at the time that the view is created.  </p>
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' <h2> Invocation chaining </h2>
	''' 
	''' 
	''' <p> Methods in this class that do not otherwise have a value to return are
	''' specified to return the buffer upon which they are invoked.  This allows
	''' method invocations to be chained.
	''' 
	''' 
	''' 
	''' The sequence of statements
	''' 
	''' <blockquote><pre>
	''' bb.putInt(0xCAFEBABE);
	''' bb.putShort(3);
	''' bb.putShort(45);</pre></blockquote>
	''' 
	''' can, for example, be replaced by the single statement
	''' 
	''' <blockquote><pre>
	''' bb.putInt(0xCAFEBABE).putShort(3).putShort(45);</pre></blockquote>
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' 
	''' @author Mark Reinhold
	''' @author JSR-51 Expert Group
	''' @since 1.4
	''' </summary>

	Public MustInherit Class ByteBuffer
		Inherits Buffer
		Implements Comparable(Of ByteBuffer)

		' These fields are declared here rather than in Heap-X-Buffer in order to
		' reduce the number of virtual method invocations needed to access these
		' values, which is especially costly when coding small buffers.
		'
		Friend ReadOnly hb As SByte() ' Non-null only for heap buffers
		Friend ReadOnly offset As Integer
		Friend isReadOnly As Boolean ' Valid only for heap buffers

		' Creates a new buffer with the given mark, position, limit, capacity,
		' backing array, and array offset
		'
		Friend Sub New(  mark As Integer,   pos As Integer,   lim As Integer,   cap As Integer,   hb As SByte(),   offset As Integer) ' package-private
			MyBase.New(mark, pos, lim, cap)
			Me.hb = hb
			Me.offset = offset
		End Sub

		' Creates a new buffer with the given mark, position, limit, and capacity
		'
		Friend Sub New(  mark As Integer,   pos As Integer,   lim As Integer,   cap As Integer) ' package-private
			Me.New(mark, pos, lim, cap, Nothing, 0)
		End Sub



		''' <summary>
		''' Allocates a new direct byte buffer.
		''' 
		''' <p> The new buffer's position will be zero, its limit will be its
		''' capacity, its mark will be undefined, and each of its elements will be
		''' initialized to zero.  Whether or not it has a
		''' <seealso cref="#hasArray backing array"/> is unspecified.
		''' </summary>
		''' <param name="capacity">
		'''         The new buffer's capacity, in bytes
		''' </param>
		''' <returns>  The new byte buffer
		''' </returns>
		''' <exception cref="IllegalArgumentException">
		'''          If the <tt>capacity</tt> is a negative integer </exception>
		Public Shared Function allocateDirect(  capacity As Integer) As ByteBuffer
			Return New DirectByteBuffer(capacity)
		End Function



		''' <summary>
		''' Allocates a new byte buffer.
		''' 
		''' <p> The new buffer's position will be zero, its limit will be its
		''' capacity, its mark will be undefined, and each of its elements will be
		''' initialized to zero.  It will have a <seealso cref="#array backing array"/>,
		''' and its <seealso cref="#arrayOffset array offset"/> will be zero.
		''' </summary>
		''' <param name="capacity">
		'''         The new buffer's capacity, in bytes
		''' </param>
		''' <returns>  The new byte buffer
		''' </returns>
		''' <exception cref="IllegalArgumentException">
		'''          If the <tt>capacity</tt> is a negative integer </exception>
		Public Shared Function allocate(  capacity As Integer) As ByteBuffer
			If capacity < 0 Then Throw New IllegalArgumentException
			Return New HeapByteBuffer(capacity, capacity)
		End Function

		''' <summary>
		''' Wraps a byte array into a buffer.
		''' 
		''' <p> The new buffer will be backed by the given byte array;
		''' that is, modifications to the buffer will cause the array to be modified
		''' and vice versa.  The new buffer's capacity will be
		''' <tt>array.length</tt>, its position will be <tt>offset</tt>, its limit
		''' will be <tt>offset + length</tt>, and its mark will be undefined.  Its
		''' <seealso cref="#array backing array"/> will be the given array, and
		''' its <seealso cref="#arrayOffset array offset"/> will be zero.  </p>
		''' </summary>
		''' <param name="array">
		'''         The array that will back the new buffer
		''' </param>
		''' <param name="offset">
		'''         The offset of the subarray to be used; must be non-negative and
		'''         no larger than <tt>array.length</tt>.  The new buffer's position
		'''         will be set to this value.
		''' </param>
		''' <param name="length">
		'''         The length of the subarray to be used;
		'''         must be non-negative and no larger than
		'''         <tt>array.length - offset</tt>.
		'''         The new buffer's limit will be set to <tt>offset + length</tt>.
		''' </param>
		''' <returns>  The new byte buffer
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If the preconditions on the <tt>offset</tt> and <tt>length</tt>
		'''          parameters do not hold </exception>
		Public Shared Function wrap(  array As SByte(),   offset As Integer,   length As Integer) As ByteBuffer
			Try
				Return New HeapByteBuffer(array, offset, length)
			Catch x As IllegalArgumentException
				Throw New IndexOutOfBoundsException
			End Try
		End Function

		''' <summary>
		''' Wraps a byte array into a buffer.
		''' 
		''' <p> The new buffer will be backed by the given byte array;
		''' that is, modifications to the buffer will cause the array to be modified
		''' and vice versa.  The new buffer's capacity and limit will be
		''' <tt>array.length</tt>, its position will be zero, and its mark will be
		''' undefined.  Its <seealso cref="#array backing array"/> will be the
		''' given array, and its <seealso cref="#arrayOffset array offset>"/> will
		''' be zero.  </p>
		''' </summary>
		''' <param name="array">
		'''         The array that will back this buffer
		''' </param>
		''' <returns>  The new byte buffer </returns>
		Public Shared Function wrap(  array As SByte()) As ByteBuffer
			Return wrap(array, 0, array.Length)
		End Function






























































































		''' <summary>
		''' Creates a new byte buffer whose content is a shared subsequence of
		''' this buffer's content.
		''' 
		''' <p> The content of the new buffer will start at this buffer's current
		''' position.  Changes to this buffer's content will be visible in the new
		''' buffer, and vice versa; the two buffers' position, limit, and mark
		''' values will be independent.
		''' 
		''' <p> The new buffer's position will be zero, its capacity and its limit
		''' will be the number of bytes remaining in this buffer, and its mark
		''' will be undefined.  The new buffer will be direct if, and only if, this
		''' buffer is direct, and it will be read-only if, and only if, this buffer
		''' is read-only.  </p>
		''' </summary>
		''' <returns>  The new byte buffer </returns>
		Public MustOverride Function slice() As ByteBuffer

		''' <summary>
		''' Creates a new byte buffer that shares this buffer's content.
		''' 
		''' <p> The content of the new buffer will be that of this buffer.  Changes
		''' to this buffer's content will be visible in the new buffer, and vice
		''' versa; the two buffers' position, limit, and mark values will be
		''' independent.
		''' 
		''' <p> The new buffer's capacity, limit, position, and mark values will be
		''' identical to those of this buffer.  The new buffer will be direct if,
		''' and only if, this buffer is direct, and it will be read-only if, and
		''' only if, this buffer is read-only.  </p>
		''' </summary>
		''' <returns>  The new byte buffer </returns>
		Public MustOverride Function duplicate() As ByteBuffer

		''' <summary>
		''' Creates a new, read-only byte buffer that shares this buffer's
		''' content.
		''' 
		''' <p> The content of the new buffer will be that of this buffer.  Changes
		''' to this buffer's content will be visible in the new buffer; the new
		''' buffer itself, however, will be read-only and will not allow the shared
		''' content to be modified.  The two buffers' position, limit, and mark
		''' values will be independent.
		''' 
		''' <p> The new buffer's capacity, limit, position, and mark values will be
		''' identical to those of this buffer.
		''' 
		''' <p> If this buffer is itself read-only then this method behaves in
		''' exactly the same way as the <seealso cref="#duplicate duplicate"/> method.  </p>
		''' </summary>
		''' <returns>  The new, read-only byte buffer </returns>
		Public MustOverride Function asReadOnlyBuffer() As ByteBuffer


		' -- Singleton get/put methods --

		''' <summary>
		''' Relative <i>get</i> method.  Reads the byte at this buffer's
		''' current position, and then increments the position.
		''' </summary>
		''' <returns>  The byte at the buffer's current position
		''' </returns>
		''' <exception cref="BufferUnderflowException">
		'''          If the buffer's current position is not smaller than its limit </exception>
		Public MustOverride Function [get]() As SByte

		''' <summary>
		''' Relative <i>put</i> method&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes the given byte into this buffer at the current
		''' position, and then increments the position. </p>
		''' </summary>
		''' <param name="b">
		'''         The byte to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferOverflowException">
		'''          If this buffer's current position is not smaller than its limit
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function put(  b As SByte) As ByteBuffer

		''' <summary>
		''' Absolute <i>get</i> method.  Reads the byte at the given
		''' index.
		''' </summary>
		''' <param name="index">
		'''         The index from which the byte will be read
		''' </param>
		''' <returns>  The byte at the given index
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit </exception>
		Public MustOverride Function [get](  index As Integer) As SByte














		''' <summary>
		''' Absolute <i>put</i> method&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes the given byte into this buffer at the given
		''' index. </p>
		''' </summary>
		''' <param name="index">
		'''         The index at which the byte will be written
		''' </param>
		''' <param name="b">
		'''         The byte value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function put(  index As Integer,   b As SByte) As ByteBuffer


		' -- Bulk get operations --

		''' <summary>
		''' Relative bulk <i>get</i> method.
		''' 
		''' <p> This method transfers bytes from this buffer into the given
		''' destination array.  If there are fewer bytes remaining in the
		''' buffer than are required to satisfy the request, that is, if
		''' <tt>length</tt>&nbsp;<tt>&gt;</tt>&nbsp;<tt>remaining()</tt>, then no
		''' bytes are transferred and a <seealso cref="BufferUnderflowException"/> is
		''' thrown.
		''' 
		''' <p> Otherwise, this method copies <tt>length</tt> bytes from this
		''' buffer into the given array, starting at the current position of this
		''' buffer and at the given offset in the array.  The position of this
		''' buffer is then incremented by <tt>length</tt>.
		''' 
		''' <p> In other words, an invocation of this method of the form
		''' <tt>src.get(dst,&nbsp;off,&nbsp;len)</tt> has exactly the same effect as
		''' the loop
		''' 
		''' <pre>{@code
		'''     for (int i = off; i < off + len; i++)
		'''         dst[i] = src.get():
		''' }</pre>
		''' 
		''' except that it first checks that there are sufficient bytes in
		''' this buffer and it is potentially much more efficient.
		''' </summary>
		''' <param name="dst">
		'''         The array into which bytes are to be written
		''' </param>
		''' <param name="offset">
		'''         The offset within the array of the first byte to be
		'''         written; must be non-negative and no larger than
		'''         <tt>dst.length</tt>
		''' </param>
		''' <param name="length">
		'''         The maximum number of bytes to be written to the given
		'''         array; must be non-negative and no larger than
		'''         <tt>dst.length - offset</tt>
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferUnderflowException">
		'''          If there are fewer than <tt>length</tt> bytes
		'''          remaining in this buffer
		''' </exception>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If the preconditions on the <tt>offset</tt> and <tt>length</tt>
		'''          parameters do not hold </exception>
		Public Overridable Function [get](  dst As SByte(),   offset As Integer,   length As Integer) As ByteBuffer
			checkBounds(offset, length, dst.Length)
			If length > remaining() Then Throw New BufferUnderflowException
			Dim [end] As Integer = offset + length
			For i As Integer = offset To [end] - 1
				dst(i) = [get]()
			Next i
			Return Me
		End Function

		''' <summary>
		''' Relative bulk <i>get</i> method.
		''' 
		''' <p> This method transfers bytes from this buffer into the given
		''' destination array.  An invocation of this method of the form
		''' <tt>src.get(a)</tt> behaves in exactly the same way as the invocation
		''' 
		''' <pre>
		'''     src.get(a, 0, a.length) </pre>
		''' </summary>
		''' <param name="dst">
		'''          The destination array
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferUnderflowException">
		'''          If there are fewer than <tt>length</tt> bytes
		'''          remaining in this buffer </exception>
		Public Overridable Function [get](  dst As SByte()) As ByteBuffer
			Return [get](dst, 0, dst.Length)
		End Function


		' -- Bulk put operations --

		''' <summary>
		''' Relative bulk <i>put</i> method&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> This method transfers the bytes remaining in the given source
		''' buffer into this buffer.  If there are more bytes remaining in the
		''' source buffer than in this buffer, that is, if
		''' <tt>src.remaining()</tt>&nbsp;<tt>&gt;</tt>&nbsp;<tt>remaining()</tt>,
		''' then no bytes are transferred and a {@link
		''' BufferOverflowException} is thrown.
		''' 
		''' <p> Otherwise, this method copies
		''' <i>n</i>&nbsp;=&nbsp;<tt>src.remaining()</tt> bytes from the given
		''' buffer into this buffer, starting at each buffer's current position.
		''' The positions of both buffers are then incremented by <i>n</i>.
		''' 
		''' <p> In other words, an invocation of this method of the form
		''' <tt>dst.put(src)</tt> has exactly the same effect as the loop
		''' 
		''' <pre>
		'''     while (src.hasRemaining())
		'''         dst.put(src.get()); </pre>
		''' 
		''' except that it first checks that there is sufficient space in this
		''' buffer and it is potentially much more efficient.
		''' </summary>
		''' <param name="src">
		'''         The source buffer from which bytes are to be read;
		'''         must not be this buffer
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferOverflowException">
		'''          If there is insufficient space in this buffer
		'''          for the remaining bytes in the source buffer
		''' </exception>
		''' <exception cref="IllegalArgumentException">
		'''          If the source buffer is this buffer
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public Overridable Function put(  src As ByteBuffer) As ByteBuffer
			If src Is Me Then Throw New IllegalArgumentException
			If [readOnly] Then Throw New ReadOnlyBufferException
			Dim n As Integer = src.remaining()
			If n > remaining() Then Throw New BufferOverflowException
			For i As Integer = 0 To n - 1
				put(src.get())
			Next i
			Return Me
		End Function

		''' <summary>
		''' Relative bulk <i>put</i> method&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> This method transfers bytes into this buffer from the given
		''' source array.  If there are more bytes to be copied from the array
		''' than remain in this buffer, that is, if
		''' <tt>length</tt>&nbsp;<tt>&gt;</tt>&nbsp;<tt>remaining()</tt>, then no
		''' bytes are transferred and a <seealso cref="BufferOverflowException"/> is
		''' thrown.
		''' 
		''' <p> Otherwise, this method copies <tt>length</tt> bytes from the
		''' given array into this buffer, starting at the given offset in the array
		''' and at the current position of this buffer.  The position of this buffer
		''' is then incremented by <tt>length</tt>.
		''' 
		''' <p> In other words, an invocation of this method of the form
		''' <tt>dst.put(src,&nbsp;off,&nbsp;len)</tt> has exactly the same effect as
		''' the loop
		''' 
		''' <pre>{@code
		'''     for (int i = off; i < off + len; i++)
		'''         dst.put(a[i]);
		''' }</pre>
		''' 
		''' except that it first checks that there is sufficient space in this
		''' buffer and it is potentially much more efficient.
		''' </summary>
		''' <param name="src">
		'''         The array from which bytes are to be read
		''' </param>
		''' <param name="offset">
		'''         The offset within the array of the first byte to be read;
		'''         must be non-negative and no larger than <tt>array.length</tt>
		''' </param>
		''' <param name="length">
		'''         The number of bytes to be read from the given array;
		'''         must be non-negative and no larger than
		'''         <tt>array.length - offset</tt>
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferOverflowException">
		'''          If there is insufficient space in this buffer
		''' </exception>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If the preconditions on the <tt>offset</tt> and <tt>length</tt>
		'''          parameters do not hold
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public Overridable Function put(  src As SByte(),   offset As Integer,   length As Integer) As ByteBuffer
			checkBounds(offset, length, src.Length)
			If length > remaining() Then Throw New BufferOverflowException
			Dim [end] As Integer = offset + length
			For i As Integer = offset To [end] - 1
				Me.put(src(i))
			Next i
			Return Me
		End Function

		''' <summary>
		''' Relative bulk <i>put</i> method&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> This method transfers the entire content of the given source
		''' byte array into this buffer.  An invocation of this method of the
		''' form <tt>dst.put(a)</tt> behaves in exactly the same way as the
		''' invocation
		''' 
		''' <pre>
		'''     dst.put(a, 0, a.length) </pre>
		''' </summary>
		''' <param name="src">
		'''          The source array
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferOverflowException">
		'''          If there is insufficient space in this buffer
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public Function put(  src As SByte()) As ByteBuffer
			Return put(src, 0, src.Length)
		End Function































































































		' -- Other stuff --

		''' <summary>
		''' Tells whether or not this buffer is backed by an accessible byte
		''' array.
		''' 
		''' <p> If this method returns <tt>true</tt> then the <seealso cref="#array() array"/>
		''' and <seealso cref="#arrayOffset() arrayOffset"/> methods may safely be invoked.
		''' </p>
		''' </summary>
		''' <returns>  <tt>true</tt> if, and only if, this buffer
		'''          is backed by an array and is not read-only </returns>
		Public NotOverridable Overrides Function hasArray() As Boolean
			Return (hb IsNot Nothing) AndAlso Not isReadOnly
		End Function

		''' <summary>
		''' Returns the byte array that backs this
		''' buffer&nbsp;&nbsp;<i>(optional operation)</i>.
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
		'''          If this buffer is not backed by an accessible array </exception>
		Public NotOverridable Overrides Function array() As SByte()
			If hb Is Nothing Then Throw New UnsupportedOperationException
			If isReadOnly Then Throw New ReadOnlyBufferException
			Return hb
		End Function

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
		'''          If this buffer is not backed by an accessible array </exception>
		Public NotOverridable Overrides Function arrayOffset() As Integer
			If hb Is Nothing Then Throw New UnsupportedOperationException
			If isReadOnly Then Throw New ReadOnlyBufferException
			Return offset
		End Function

		''' <summary>
		''' Compacts this buffer&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> The bytes between the buffer's current position and its limit,
		''' if any, are copied to the beginning of the buffer.  That is, the
		''' byte at index <i>p</i>&nbsp;=&nbsp;<tt>position()</tt> is copied
		''' to index zero, the byte at index <i>p</i>&nbsp;+&nbsp;1 is copied
		''' to index one, and so forth until the byte at index
		''' <tt>limit()</tt>&nbsp;-&nbsp;1 is copied to index
		''' <i>n</i>&nbsp;=&nbsp;<tt>limit()</tt>&nbsp;-&nbsp;<tt>1</tt>&nbsp;-&nbsp;<i>p</i>.
		''' The buffer's position is then set to <i>n+1</i> and its limit is set to
		''' its capacity.  The mark, if defined, is discarded.
		''' 
		''' <p> The buffer's position is set to the number of bytes copied,
		''' rather than to zero, so that an invocation of this method can be
		''' followed immediately by an invocation of another relative <i>put</i>
		''' method. </p>
		''' 
		''' 
		''' 
		''' <p> Invoke this method after writing data from a buffer in case the
		''' write was incomplete.  The following loop, for example, copies bytes
		''' from one channel to another via the buffer <tt>buf</tt>:
		''' 
		''' <blockquote><pre>{@code
		'''   buf.clear();          // Prepare buffer for use
		'''   while (in.read(buf) >= 0 || buf.position != 0) {
		'''       buf.flip();
		'''       out.write(buf);
		'''       buf.compact();    // In case of partial write
		'''   }
		''' }</pre></blockquote>
		''' 
		''' 
		''' </summary>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function compact() As ByteBuffer

		''' <summary>
		''' Tells whether or not this byte buffer is direct.
		''' </summary>
		''' <returns>  <tt>true</tt> if, and only if, this buffer is direct </returns>
		Public MustOverride ReadOnly Property direct As Boolean



		''' <summary>
		''' Returns a string summarizing the state of this buffer.
		''' </summary>
		''' <returns>  A summary string </returns>
		Public Overrides Function ToString() As String
			Dim sb As New StringBuffer
			sb.append(Me.GetType().name)
			sb.append("[pos=")
			sb.append(position())
			sb.append(" lim=")
			sb.append(limit())
			sb.append(" cap=")
			sb.append(capacity())
			sb.append("]")
			Return sb.ToString()
		End Function






		''' <summary>
		''' Returns the current hash code of this buffer.
		''' 
		''' <p> The hash code of a byte buffer depends only upon its remaining
		''' elements; that is, upon the elements from <tt>position()</tt> up to, and
		''' including, the element at <tt>limit()</tt>&nbsp;-&nbsp;<tt>1</tt>.
		''' 
		''' <p> Because buffer hash codes are content-dependent, it is inadvisable
		''' to use buffers as keys in hash maps or similar data structures unless it
		''' is known that their contents will not change.  </p>
		''' </summary>
		''' <returns>  The current hash code of this buffer </returns>
		Public Overrides Function GetHashCode() As Integer
			Dim h As Integer = 1
			Dim p As Integer = position()
			For i As Integer = limit() - 1 To p Step -1



				h = 31 * h + CInt([get](i))
			Next i

			Return h
		End Function

		''' <summary>
		''' Tells whether or not this buffer is equal to another object.
		''' 
		''' <p> Two byte buffers are equal if, and only if,
		''' 
		''' <ol>
		''' 
		'''   <li><p> They have the same element type,  </p></li>
		''' 
		'''   <li><p> They have the same number of remaining elements, and
		'''   </p></li>
		''' 
		'''   <li><p> The two sequences of remaining elements, considered
		'''   independently of their starting positions, are pointwise equal.
		''' 
		''' 
		''' 
		''' 
		''' 
		''' 
		''' 
		'''   </p></li>
		''' 
		''' </ol>
		''' 
		''' <p> A byte buffer is not equal to any other type of object.  </p>
		''' </summary>
		''' <param name="ob">  The object to which this buffer is to be compared
		''' </param>
		''' <returns>  <tt>true</tt> if, and only if, this buffer is equal to the
		'''           given object </returns>
		Public Overrides Function Equals(  ob As Object) As Boolean
			If Me Is ob Then Return True
			If Not(TypeOf ob Is ByteBuffer) Then Return False
			Dim that As ByteBuffer = CType(ob, ByteBuffer)
			If Me.remaining() <> that.remaining() Then Return False
			Dim p As Integer = Me.position()
			Dim i As Integer = Me.limit() - 1
			Dim j As Integer = that.limit() - 1
			Do While i >= p
				If Not Equals(Me.get(i), that.get(j)) Then Return False
				i -= 1
				j -= 1
			Loop
			Return True
		End Function

		Private Shared Function Equals(  x As SByte,   y As SByte) As Boolean



			Return x = y

		End Function

		''' <summary>
		''' Compares this buffer to another.
		''' 
		''' <p> Two byte buffers are compared by comparing their sequences of
		''' remaining elements lexicographically, without regard to the starting
		''' position of each sequence within its corresponding buffer.
		''' 
		''' 
		''' 
		''' 
		''' 
		''' 
		''' 
		''' 
		''' Pairs of {@code byte} elements are compared as if by invoking
		''' <seealso cref="Byte#compare(byte,byte)"/>.
		''' 
		''' 
		''' <p> A byte buffer is not comparable to any other type of object.
		''' </summary>
		''' <returns>  A negative integer, zero, or a positive integer as this buffer
		'''          is less than, equal to, or greater than the given buffer </returns>
		Public Overridable Function compareTo(  that As ByteBuffer) As Integer Implements Comparable(Of ByteBuffer).compareTo
			Dim n As Integer = Me.position() + System.Math.Min(Me.remaining(), that.remaining())
			Dim i As Integer = Me.position()
			Dim j As Integer = that.position()
			Do While i < n
				Dim cmp As Integer = compare(Me.get(i), that.get(j))
				If cmp <> 0 Then Return cmp
				i += 1
				j += 1
			Loop
			Return Me.remaining() - that.remaining()
		End Function

		Private Shared Function compare(  x As SByte,   y As SByte) As Integer






			Return java.lang.[Byte].Compare(x, y)

		End Function

		' -- Other char stuff --


































































































































































































		' -- Other byte stuff: Access to binary data --





















		Friend bigEndian As Boolean = True ' package-private
		Friend nativeByteOrder As Boolean = (Bits.byteOrder() Is ByteOrder.BIG_ENDIAN) ' package-private

		''' <summary>
		''' Retrieves this buffer's byte order.
		''' 
		''' <p> The byte order is used when reading or writing multibyte values, and
		''' when creating buffers that are views of this byte buffer.  The order of
		''' a newly-created byte buffer is always {@link ByteOrder#BIG_ENDIAN
		''' BIG_ENDIAN}.  </p>
		''' </summary>
		''' <returns>  This buffer's byte order </returns>
		Public Function order() As ByteOrder
			Return If(bigEndian, ByteOrder.BIG_ENDIAN, ByteOrder.LITTLE_ENDIAN)
		End Function

		''' <summary>
		''' Modifies this buffer's byte order.
		''' </summary>
		''' <param name="bo">
		'''         The new byte order,
		'''         either <seealso cref="ByteOrder#BIG_ENDIAN BIG_ENDIAN"/>
		'''         or <seealso cref="ByteOrder#LITTLE_ENDIAN LITTLE_ENDIAN"/>
		''' </param>
		''' <returns>  This buffer </returns>
		Public Function order(  bo As ByteOrder) As ByteBuffer
			bigEndian = (bo Is ByteOrder.BIG_ENDIAN)
			nativeByteOrder = (bigEndian = (Bits.byteOrder() Is ByteOrder.BIG_ENDIAN))
			Return Me
		End Function

		' Unchecked accessors, for use by ByteBufferAs-X-Buffer classes
		'
		Friend MustOverride Function _get(  i As Integer) As SByte ' package-private
		Friend MustOverride Sub _put(  i As Integer,   b As SByte) ' package-private


		''' <summary>
		''' Relative <i>get</i> method for reading a char value.
		''' 
		''' <p> Reads the next two bytes at this buffer's current position,
		''' composing them into a char value according to the current byte order,
		''' and then increments the position by two.  </p>
		''' </summary>
		''' <returns>  The char value at the buffer's current position
		''' </returns>
		''' <exception cref="BufferUnderflowException">
		'''          If there are fewer than two bytes
		'''          remaining in this buffer </exception>
		Public MustOverride ReadOnly Property [char] As Char

		''' <summary>
		''' Relative <i>put</i> method for writing a char
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes two bytes containing the given char value, in the
		''' current byte order, into this buffer at the current position, and then
		''' increments the position by two.  </p>
		''' </summary>
		''' <param name="value">
		'''         The char value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferOverflowException">
		'''          If there are fewer than two bytes
		'''          remaining in this buffer
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putChar(  value As Char) As ByteBuffer

		''' <summary>
		''' Absolute <i>get</i> method for reading a char value.
		''' 
		''' <p> Reads two bytes at the given index, composing them into a
		''' char value according to the current byte order.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index from which the bytes will be read
		''' </param>
		''' <returns>  The char value at the given index
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus one </exception>
		Public MustOverride Function getChar(  index As Integer) As Char

		''' <summary>
		''' Absolute <i>put</i> method for writing a char
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes two bytes containing the given char value, in the
		''' current byte order, into this buffer at the given index.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index at which the bytes will be written
		''' </param>
		''' <param name="value">
		'''         The char value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus one
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putChar(  index As Integer,   value As Char) As ByteBuffer

		''' <summary>
		''' Creates a view of this byte buffer as a char buffer.
		''' 
		''' <p> The content of the new buffer will start at this buffer's current
		''' position.  Changes to this buffer's content will be visible in the new
		''' buffer, and vice versa; the two buffers' position, limit, and mark
		''' values will be independent.
		''' 
		''' <p> The new buffer's position will be zero, its capacity and its limit
		''' will be the number of bytes remaining in this buffer divided by
		''' two, and its mark will be undefined.  The new buffer will be direct
		''' if, and only if, this buffer is direct, and it will be read-only if, and
		''' only if, this buffer is read-only.  </p>
		''' </summary>
		''' <returns>  A new char buffer </returns>
		Public MustOverride Function asCharBuffer() As CharBuffer


		''' <summary>
		''' Relative <i>get</i> method for reading a short value.
		''' 
		''' <p> Reads the next two bytes at this buffer's current position,
		''' composing them into a short value according to the current byte order,
		''' and then increments the position by two.  </p>
		''' </summary>
		''' <returns>  The short value at the buffer's current position
		''' </returns>
		''' <exception cref="BufferUnderflowException">
		'''          If there are fewer than two bytes
		'''          remaining in this buffer </exception>
		Public MustOverride ReadOnly Property [short] As Short

		''' <summary>
		''' Relative <i>put</i> method for writing a short
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes two bytes containing the given short value, in the
		''' current byte order, into this buffer at the current position, and then
		''' increments the position by two.  </p>
		''' </summary>
		''' <param name="value">
		'''         The short value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferOverflowException">
		'''          If there are fewer than two bytes
		'''          remaining in this buffer
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putShort(  value As Short) As ByteBuffer

		''' <summary>
		''' Absolute <i>get</i> method for reading a short value.
		''' 
		''' <p> Reads two bytes at the given index, composing them into a
		''' short value according to the current byte order.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index from which the bytes will be read
		''' </param>
		''' <returns>  The short value at the given index
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus one </exception>
		Public MustOverride Function getShort(  index As Integer) As Short

		''' <summary>
		''' Absolute <i>put</i> method for writing a short
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes two bytes containing the given short value, in the
		''' current byte order, into this buffer at the given index.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index at which the bytes will be written
		''' </param>
		''' <param name="value">
		'''         The short value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus one
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putShort(  index As Integer,   value As Short) As ByteBuffer

		''' <summary>
		''' Creates a view of this byte buffer as a short buffer.
		''' 
		''' <p> The content of the new buffer will start at this buffer's current
		''' position.  Changes to this buffer's content will be visible in the new
		''' buffer, and vice versa; the two buffers' position, limit, and mark
		''' values will be independent.
		''' 
		''' <p> The new buffer's position will be zero, its capacity and its limit
		''' will be the number of bytes remaining in this buffer divided by
		''' two, and its mark will be undefined.  The new buffer will be direct
		''' if, and only if, this buffer is direct, and it will be read-only if, and
		''' only if, this buffer is read-only.  </p>
		''' </summary>
		''' <returns>  A new short buffer </returns>
		Public MustOverride Function asShortBuffer() As ShortBuffer


		''' <summary>
		''' Relative <i>get</i> method for reading an int value.
		''' 
		''' <p> Reads the next four bytes at this buffer's current position,
		''' composing them into an int value according to the current byte order,
		''' and then increments the position by four.  </p>
		''' </summary>
		''' <returns>  The int value at the buffer's current position
		''' </returns>
		''' <exception cref="BufferUnderflowException">
		'''          If there are fewer than four bytes
		'''          remaining in this buffer </exception>
		Public MustOverride ReadOnly Property int As Integer

		''' <summary>
		''' Relative <i>put</i> method for writing an int
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes four bytes containing the given int value, in the
		''' current byte order, into this buffer at the current position, and then
		''' increments the position by four.  </p>
		''' </summary>
		''' <param name="value">
		'''         The int value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferOverflowException">
		'''          If there are fewer than four bytes
		'''          remaining in this buffer
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putInt(  value As Integer) As ByteBuffer

		''' <summary>
		''' Absolute <i>get</i> method for reading an int value.
		''' 
		''' <p> Reads four bytes at the given index, composing them into a
		''' int value according to the current byte order.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index from which the bytes will be read
		''' </param>
		''' <returns>  The int value at the given index
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus three </exception>
		Public MustOverride Function getInt(  index As Integer) As Integer

		''' <summary>
		''' Absolute <i>put</i> method for writing an int
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes four bytes containing the given int value, in the
		''' current byte order, into this buffer at the given index.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index at which the bytes will be written
		''' </param>
		''' <param name="value">
		'''         The int value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus three
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putInt(  index As Integer,   value As Integer) As ByteBuffer

		''' <summary>
		''' Creates a view of this byte buffer as an int buffer.
		''' 
		''' <p> The content of the new buffer will start at this buffer's current
		''' position.  Changes to this buffer's content will be visible in the new
		''' buffer, and vice versa; the two buffers' position, limit, and mark
		''' values will be independent.
		''' 
		''' <p> The new buffer's position will be zero, its capacity and its limit
		''' will be the number of bytes remaining in this buffer divided by
		''' four, and its mark will be undefined.  The new buffer will be direct
		''' if, and only if, this buffer is direct, and it will be read-only if, and
		''' only if, this buffer is read-only.  </p>
		''' </summary>
		''' <returns>  A new int buffer </returns>
		Public MustOverride Function asIntBuffer() As IntBuffer


		''' <summary>
		''' Relative <i>get</i> method for reading a long value.
		''' 
		''' <p> Reads the next eight bytes at this buffer's current position,
		''' composing them into a long value according to the current byte order,
		''' and then increments the position by eight.  </p>
		''' </summary>
		''' <returns>  The long value at the buffer's current position
		''' </returns>
		''' <exception cref="BufferUnderflowException">
		'''          If there are fewer than eight bytes
		'''          remaining in this buffer </exception>
		Public MustOverride ReadOnly Property [long] As Long

		''' <summary>
		''' Relative <i>put</i> method for writing a long
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes eight bytes containing the given long value, in the
		''' current byte order, into this buffer at the current position, and then
		''' increments the position by eight.  </p>
		''' </summary>
		''' <param name="value">
		'''         The long value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferOverflowException">
		'''          If there are fewer than eight bytes
		'''          remaining in this buffer
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putLong(  value As Long) As ByteBuffer

		''' <summary>
		''' Absolute <i>get</i> method for reading a long value.
		''' 
		''' <p> Reads eight bytes at the given index, composing them into a
		''' long value according to the current byte order.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index from which the bytes will be read
		''' </param>
		''' <returns>  The long value at the given index
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus seven </exception>
		Public MustOverride Function getLong(  index As Integer) As Long

		''' <summary>
		''' Absolute <i>put</i> method for writing a long
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes eight bytes containing the given long value, in the
		''' current byte order, into this buffer at the given index.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index at which the bytes will be written
		''' </param>
		''' <param name="value">
		'''         The long value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus seven
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putLong(  index As Integer,   value As Long) As ByteBuffer

		''' <summary>
		''' Creates a view of this byte buffer as a long buffer.
		''' 
		''' <p> The content of the new buffer will start at this buffer's current
		''' position.  Changes to this buffer's content will be visible in the new
		''' buffer, and vice versa; the two buffers' position, limit, and mark
		''' values will be independent.
		''' 
		''' <p> The new buffer's position will be zero, its capacity and its limit
		''' will be the number of bytes remaining in this buffer divided by
		''' eight, and its mark will be undefined.  The new buffer will be direct
		''' if, and only if, this buffer is direct, and it will be read-only if, and
		''' only if, this buffer is read-only.  </p>
		''' </summary>
		''' <returns>  A new long buffer </returns>
		Public MustOverride Function asLongBuffer() As LongBuffer


		''' <summary>
		''' Relative <i>get</i> method for reading a float value.
		''' 
		''' <p> Reads the next four bytes at this buffer's current position,
		''' composing them into a float value according to the current byte order,
		''' and then increments the position by four.  </p>
		''' </summary>
		''' <returns>  The float value at the buffer's current position
		''' </returns>
		''' <exception cref="BufferUnderflowException">
		'''          If there are fewer than four bytes
		'''          remaining in this buffer </exception>
		Public MustOverride ReadOnly Property float As Single

		''' <summary>
		''' Relative <i>put</i> method for writing a float
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes four bytes containing the given float value, in the
		''' current byte order, into this buffer at the current position, and then
		''' increments the position by four.  </p>
		''' </summary>
		''' <param name="value">
		'''         The float value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferOverflowException">
		'''          If there are fewer than four bytes
		'''          remaining in this buffer
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putFloat(  value As Single) As ByteBuffer

		''' <summary>
		''' Absolute <i>get</i> method for reading a float value.
		''' 
		''' <p> Reads four bytes at the given index, composing them into a
		''' float value according to the current byte order.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index from which the bytes will be read
		''' </param>
		''' <returns>  The float value at the given index
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus three </exception>
		Public MustOverride Function getFloat(  index As Integer) As Single

		''' <summary>
		''' Absolute <i>put</i> method for writing a float
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes four bytes containing the given float value, in the
		''' current byte order, into this buffer at the given index.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index at which the bytes will be written
		''' </param>
		''' <param name="value">
		'''         The float value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus three
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putFloat(  index As Integer,   value As Single) As ByteBuffer

		''' <summary>
		''' Creates a view of this byte buffer as a float buffer.
		''' 
		''' <p> The content of the new buffer will start at this buffer's current
		''' position.  Changes to this buffer's content will be visible in the new
		''' buffer, and vice versa; the two buffers' position, limit, and mark
		''' values will be independent.
		''' 
		''' <p> The new buffer's position will be zero, its capacity and its limit
		''' will be the number of bytes remaining in this buffer divided by
		''' four, and its mark will be undefined.  The new buffer will be direct
		''' if, and only if, this buffer is direct, and it will be read-only if, and
		''' only if, this buffer is read-only.  </p>
		''' </summary>
		''' <returns>  A new float buffer </returns>
		Public MustOverride Function asFloatBuffer() As FloatBuffer


		''' <summary>
		''' Relative <i>get</i> method for reading a double value.
		''' 
		''' <p> Reads the next eight bytes at this buffer's current position,
		''' composing them into a double value according to the current byte order,
		''' and then increments the position by eight.  </p>
		''' </summary>
		''' <returns>  The double value at the buffer's current position
		''' </returns>
		''' <exception cref="BufferUnderflowException">
		'''          If there are fewer than eight bytes
		'''          remaining in this buffer </exception>
		Public MustOverride ReadOnly Property [double] As Double

		''' <summary>
		''' Relative <i>put</i> method for writing a double
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes eight bytes containing the given double value, in the
		''' current byte order, into this buffer at the current position, and then
		''' increments the position by eight.  </p>
		''' </summary>
		''' <param name="value">
		'''         The double value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="BufferOverflowException">
		'''          If there are fewer than eight bytes
		'''          remaining in this buffer
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putDouble(  value As Double) As ByteBuffer

		''' <summary>
		''' Absolute <i>get</i> method for reading a double value.
		''' 
		''' <p> Reads eight bytes at the given index, composing them into a
		''' double value according to the current byte order.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index from which the bytes will be read
		''' </param>
		''' <returns>  The double value at the given index
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus seven </exception>
		Public MustOverride Function getDouble(  index As Integer) As Double

		''' <summary>
		''' Absolute <i>put</i> method for writing a double
		''' value&nbsp;&nbsp;<i>(optional operation)</i>.
		''' 
		''' <p> Writes eight bytes containing the given double value, in the
		''' current byte order, into this buffer at the given index.  </p>
		''' </summary>
		''' <param name="index">
		'''         The index at which the bytes will be written
		''' </param>
		''' <param name="value">
		'''         The double value to be written
		''' </param>
		''' <returns>  This buffer
		''' </returns>
		''' <exception cref="IndexOutOfBoundsException">
		'''          If <tt>index</tt> is negative
		'''          or not smaller than the buffer's limit,
		'''          minus seven
		''' </exception>
		''' <exception cref="ReadOnlyBufferException">
		'''          If this buffer is read-only </exception>
		Public MustOverride Function putDouble(  index As Integer,   value As Double) As ByteBuffer

		''' <summary>
		''' Creates a view of this byte buffer as a double buffer.
		''' 
		''' <p> The content of the new buffer will start at this buffer's current
		''' position.  Changes to this buffer's content will be visible in the new
		''' buffer, and vice versa; the two buffers' position, limit, and mark
		''' values will be independent.
		''' 
		''' <p> The new buffer's position will be zero, its capacity and its limit
		''' will be the number of bytes remaining in this buffer divided by
		''' eight, and its mark will be undefined.  The new buffer will be direct
		''' if, and only if, this buffer is direct, and it will be read-only if, and
		''' only if, this buffer is read-only.  </p>
		''' </summary>
		''' <returns>  A new double buffer </returns>
		Public MustOverride Function asDoubleBuffer() As DoubleBuffer

	End Class

End Namespace