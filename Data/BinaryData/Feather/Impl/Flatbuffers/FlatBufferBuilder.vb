' 
'* Copyright 2014 Google Inc. All rights reserved.
'*
'* Licensed under the Apache License, Version 2.0 (the "License");
'* you may not use this file except in compliance with the License.
'* You may obtain a copy of the License at
'*
'*     http://www.apache.org/licenses/LICENSE-2.0
'*
'* Unless required by applicable law or agreed to in writing, software
'* distributed under the License is distributed on an "AS IS" BASIS,
'* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'* See the License for the specific language governing permissions and
'* limitations under the License.



Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports std = System.Math

' @file
' @addtogroup flatbuffers_csharp_api
' @{

Namespace FlatBuffers
    ''' <summary>
    ''' Responsible for building up and accessing a FlatBuffer formatted byte
    ''' array (via ByteBuffer).
    ''' </summary>
    Friend Class FlatBufferBuilder
        Private _space As Integer
        Private _bb As ByteBuffer
        Private _minAlign As Integer = 1

        ' The vtable for the current table (if _vtableSize >= 0)
        Private _vtable As Integer() = New Integer(15) {}
        ' The size of the vtable. -1 indicates no vtable
        Private _vtableSize As Integer = -1
        ' Starting offset of the current struct/table.
        Private _objectStart As Integer
        ' List of offsets of all vtables.
        Private _vtables As Integer() = New Integer(15) {}
        ' Number of entries in `vtables` in use.
        Private _numVtables As Integer = 0
        ' For the current vector being built.
        Private _vectorNumElems As Integer = 0

        ''' <summary>
        ''' Create a FlatBufferBuilder with a given initial size.
        ''' </summary>
        ''' <paramname="initialSize">
        ''' The initial size to use for the internal buffer.
        ''' </param>
        Public Sub New(initialSize As Integer)
            If initialSize <= 0 Then Throw New ArgumentOutOfRangeException("initialSize", initialSize, "Must be greater than zero")
            _space = initialSize
            _bb = New ByteBuffer(New Byte(initialSize - 1) {})
        End Sub

        ''' <summary>
        ''' Reset the FlatBufferBuilder by purging all data that it holds.
        ''' </summary>
        Public Sub Clear()
            _space = _bb.Length
            _bb.Reset()
            _minAlign = 1
            While _vtableSize > 0
                _vtable(Threading.Interlocked.Decrement(_vtableSize)) = 0
            End While
            _vtableSize = -1
            _objectStart = 0
            _numVtables = 0
            _vectorNumElems = 0
        End Sub

        ''' <summary>
        ''' Gets and sets a Boolean to disable the optimization when serializing
        ''' default values to a Table.
        '''
        ''' In order to save space, fields that are set to their default value
        ''' don't get serialized into the buffer.
        ''' </summary>
        Public Property ForceDefaults As Boolean

        ''' @cond FLATBUFFERS_INTERNAL

        Public ReadOnly Property Offset As Integer
            Get
                Return _bb.Length - _space
            End Get
        End Property

        Public Sub Pad(size As Integer)
            _space = size
            _bb.PutByte(_space, 0, size)
        End Sub

        ' Doubles the size of the ByteBuffer, and copies the old data towards
        ' the end of the new buffer (since we build the buffer backwards).
        Private Sub GrowBuffer()
            Dim oldBuf = _bb.Data
            Dim oldBufSize = oldBuf.Length
            If (oldBufSize And &HC0000000UI) <> 0 Then Throw New Exception("FlatBuffers: cannot grow buffer beyond 2 gigabytes.")

            Dim newBufSize = oldBufSize << 1
            Dim newBuf = New Byte(newBufSize - 1) {}

            Buffer.BlockCopy(oldBuf, 0, newBuf, newBufSize - oldBufSize, oldBufSize)
            _bb = New ByteBuffer(newBuf, newBufSize)
        End Sub

        ' Prepare to write an element of `size` after `additional_bytes`
        ' have been written, e.g. if you write a string, you need to align
        ' such the int length field is aligned to SIZEOF_INT, and the string
        ' data follows it directly.
        ' If all you need to do is align, `additional_bytes` will be 0.
        Public Sub Prep(size As Integer, additionalBytes As Integer)
            ' Track the biggest thing we've ever aligned to.
            If size > _minAlign Then _minAlign = size
            ' Find the amount of alignment needed such that `size` is properly
            ' aligned after `additional_bytes`
            Dim alignSize = (Not _bb.Length - _space + additionalBytes) + 1 And size - 1
            ' Reallocate the buffer if needed.
            While _space < alignSize + size + additionalBytes
                Dim oldBufSize = _bb.Length
                GrowBuffer()
                _space += _bb.Length - oldBufSize

            End While
            If alignSize > 0 Then Pad(alignSize)
        End Sub

        Public Sub PutBool(x As Boolean)
            _space = HeapSizeOf.byte
            _bb.PutByte(_space, CByte(If(x, 1, 0)))
        End Sub

        Public Sub PutSbyte(x As SByte)
            _space = HeapSizeOf.sbyte
            _bb.PutSbyte(_space, x)
        End Sub

        Public Sub PutByte(x As Byte)
            _space = HeapSizeOf.byte
            _bb.PutByte(_space, x)
        End Sub

        Public Sub PutShort(x As Short)
            _space = HeapSizeOf.short
            _bb.PutShort(_space, x)
        End Sub

        Public Sub PutUshort(x As UShort)
            _space = HeapSizeOf.ushort
            _bb.PutUshort(_space, x)
        End Sub

        Public Sub PutInt(x As Integer)
            _space = HeapSizeOf.int
            _bb.PutInt(_space, x)
        End Sub

        Public Sub PutUint(x As UInteger)
            _space = HeapSizeOf.uint
            _bb.PutUint(_space, x)
        End Sub

        Public Sub PutLong(x As Long)
            _space = HeapSizeOf.long
            _bb.PutLong(_space, x)
        End Sub

        Public Sub PutUlong(x As ULong)
            _space = HeapSizeOf.ulong
            _bb.PutUlong(_space, x)
        End Sub

        Public Sub PutFloat(x As Single)
            _space = HeapSizeOf.float
            _bb.PutFloat(_space, x)
        End Sub

        Public Sub PutDouble(x As Double)
            _space = HeapSizeOf.double
            _bb.PutDouble(_space, x)
        End Sub
        ''' @endcond

        ''' <summary>
        ''' Add a `bool` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `bool` to add to the buffer.</param>
        Public Sub AddBool(x As Boolean)
            Prep(HeapSizeOf.byte, 0)
            PutBool(x)
        End Sub

        ''' <summary>
        ''' Add a `sbyte` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `sbyte` to add to the buffer.</param>
        Public Sub AddSbyte(x As SByte)
            Prep(HeapSizeOf.sbyte, 0)
            PutSbyte(x)
        End Sub

        ''' <summary>
        ''' Add a `byte` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `byte` to add to the buffer.</param>
        Public Sub AddByte(x As Byte)
            Prep(HeapSizeOf.byte, 0)
            PutByte(x)
        End Sub

        ''' <summary>
        ''' Add a `short` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `short` to add to the buffer.</param>
        Public Sub AddShort(x As Short)
            Prep(HeapSizeOf.short, 0)
            PutShort(x)
        End Sub

        ''' <summary>
        ''' Add an `ushort` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `ushort` to add to the buffer.</param>
        Public Sub AddUshort(x As UShort)
            Prep(HeapSizeOf.ushort, 0)
            PutUshort(x)
        End Sub

        ''' <summary>
        ''' Add an `int` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `int` to add to the buffer.</param>
        Public Sub AddInt(x As Integer)
            Prep(HeapSizeOf.int, 0)
            PutInt(x)
        End Sub

        ''' <summary>
        ''' Add an `uint` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `uint` to add to the buffer.</param>
        Public Sub AddUint(x As UInteger)
            Prep(HeapSizeOf.uint, 0)
            PutUint(x)
        End Sub

        ''' <summary>
        ''' Add a `long` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `long` to add to the buffer.</param>
        Public Sub AddLong(x As Long)
            Prep(HeapSizeOf.long, 0)
            PutLong(x)
        End Sub

        ''' <summary>
        ''' Add an `ulong` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `ulong` to add to the buffer.</param>
        Public Sub AddUlong(x As ULong)
            Prep(HeapSizeOf.ulong, 0)
            PutUlong(x)
        End Sub

        ''' <summary>
        ''' Add a `float` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `float` to add to the buffer.</param>
        Public Sub AddFloat(x As Single)
            Prep(HeapSizeOf.float, 0)
            PutFloat(x)
        End Sub

        ''' <summary>
        ''' Add a `double` to the buffer (aligns the data and grows if necessary).
        ''' </summary>
        ''' <paramname="x">The `double` to add to the buffer.</param>
        Public Sub AddDouble(x As Double)
            Prep(HeapSizeOf.double, 0)
            PutDouble(x)
        End Sub

        ''' <summary>
        ''' Adds an offset, relative to where it will be written.
        ''' </summary>
        ''' <paramname="off">The offset to add to the buffer.</param>
        Public Sub AddOffset(off As Integer)
            Prep(HeapSizeOf.int, 0)  ' Ensure alignment is already done.
            If off > Offset Then Throw New ArgumentException()

            off = Offset - off + HeapSizeOf.int
            PutInt(off)
        End Sub

        ''' @cond FLATBUFFERS_INTERNAL
        Public Sub StartVector(elemSize As Integer, count As Integer, alignment As Integer)
            NotNested()
            _vectorNumElems = count
            Prep(HeapSizeOf.int, elemSize * count)
            Prep(alignment, elemSize * count) ' Just in case alignment > int.
        End Sub
        ''' @endcond

        ''' <summary>
        ''' Writes data necessary to finish a vector construction.
        ''' </summary>
        Public Function EndVector() As VectorOffset
            PutInt(_vectorNumElems)
            Return New VectorOffset(Offset)
        End Function

        ''' <summary>
        ''' Creates a vector of tables.
        ''' </summary>
        ''' <paramname="offsets">Offsets of the tables.</param>
        Public Function CreateVectorOfTables(Of T As Structure)(offsets As Offset(Of T)()) As VectorOffset
            NotNested()
            StartVector(HeapSizeOf.int, offsets.Length, HeapSizeOf.int)
            For i = offsets.Length - 1 To 0 Step -1
                AddOffset(offsets(i).Value)
            Next
            Return EndVector()
        End Function

        ''' @cond FLATBUFFERS_INTENRAL
        Public Sub Nested(obj As Integer)
            ' Structs are always stored inline, so need to be created right
            ' where they are used. You'll get this assert if you created it
            ' elsewhere.
            If obj <> Offset Then Throw New Exception("FlatBuffers: class must be serialized inline.")
        End Sub

        Public Sub NotNested()
            ' You should not be creating any other objects or strings/vectors
            ' while an object is being constructed
            If _vtableSize >= 0 Then Throw New Exception("FlatBuffers: object serialization must not be nested.")
        End Sub

        Public Sub StartObject(numfields As Integer)
            If numfields < 0 Then Throw New ArgumentOutOfRangeException("Flatbuffers: invalid numfields")

            NotNested()

            If _vtable.Length < numfields Then _vtable = New Integer(numfields - 1) {}

            _vtableSize = numfields
            _objectStart = Offset
        End Sub


        ' Set the current vtable at `voffset` to the current location in the
        ' buffer.
        Public Sub Slot(voffset As Integer)
            If voffset >= _vtableSize Then Throw New IndexOutOfRangeException("Flatbuffers: invalid voffset")

            _vtable(voffset) = Offset
        End Sub

        ''' <summary>
        ''' Adds a Boolean to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddBool(o As Integer, x As Boolean, d As Boolean)
            If ForceDefaults OrElse x <> d Then
                AddBool(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds a SByte to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddSbyte(o As Integer, x As SByte, d As SByte)
            If ForceDefaults OrElse x <> d Then
                AddSbyte(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds a Byte to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddByte(o As Integer, x As Byte, d As Byte)
            If ForceDefaults OrElse x <> d Then
                AddByte(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds a Int16 to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddShort(o As Integer, x As Short, d As Integer)
            If ForceDefaults OrElse x <> d Then
                AddShort(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds a UInt16 to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddUshort(o As Integer, x As UShort, d As UShort)
            If ForceDefaults OrElse x <> d Then
                AddUshort(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds an Int32 to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddInt(o As Integer, x As Integer, d As Integer)
            If ForceDefaults OrElse x <> d Then
                AddInt(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds a UInt32 to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddUint(o As Integer, x As UInteger, d As UInteger)
            If ForceDefaults OrElse x <> d Then
                AddUint(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds an Int64 to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddLong(o As Integer, x As Long, d As Long)
            If ForceDefaults OrElse x <> d Then
                AddLong(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds a UInt64 to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddUlong(o As Integer, x As ULong, d As ULong)
            If ForceDefaults OrElse x <> d Then
                AddUlong(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds a Single to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddFloat(o As Integer, x As Single, d As Double)
            If ForceDefaults OrElse x <> d Then
                AddFloat(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds a Double to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddDouble(o As Integer, x As Double, d As Double)
            If ForceDefaults OrElse x <> d Then
                AddDouble(x)
                Slot(o)
            End If
        End Sub

        ''' <summary>
        ''' Adds a buffer offset to the Table at index `o` in its vtable using the value `x` and default `d`
        ''' </summary>
        ''' <paramname="o">The index into the vtable</param>
        ''' <paramname="x">The value to put into the buffer. If the value is equal to the default
        ''' and <see cref="ForceDefaults"/> is false, the value will be skipped.</param>
        ''' <paramname="d">The default value to compare the value against</param>
        Public Sub AddOffset(o As Integer, x As Integer, d As Integer)
            If ForceDefaults OrElse x <> d Then
                AddOffset(x)
                Slot(o)
            End If
        End Sub
        ''' @endcond

        ''' <summary>
        ''' Encode the string `s` in the buffer using UTF-8.
        ''' </summary>
        ''' <paramname="s">The string to encode.</param>
        ''' <returns>
        ''' The offset in the buffer where the encoded string starts.
        ''' </returns>
        Public Function CreateString(s As String) As StringOffset
            NotNested()
            AddByte(0)
            Dim utf8StringLen = Encoding.UTF8.GetByteCount(s)
            StartVector(1, utf8StringLen, 1)
            _space = utf8StringLen
            Encoding.UTF8.GetBytes(s, 0, s.Length, _bb.Data, _space)
            Return New StringOffset(EndVector().Value)
        End Function

        ''' @cond FLATBUFFERS_INTERNAL
        ' Structs are stored inline, so nothing additional is being added.
        ' `d` is always 0.
        Public Sub AddStruct(voffset As Integer, x As Integer, d As Integer)
            If x <> d Then
                Nested(x)
                Slot(voffset)
            End If
        End Sub

        Public Function EndObject() As Integer
            If _vtableSize < 0 Then Throw New InvalidOperationException("Flatbuffers: calling endObject without a startObject")

            AddInt(0)
            Dim vtableloc = Offset
            ' Write out the current vtable.
            For i = _vtableSize - 1 To 0 Step -1
                ' Offset relative to the start of the table.
                Dim off As Short = If(_vtable(i) <> 0, vtableloc - _vtable(i), 0)
                AddShort(off)

                ' clear out written entry
                _vtable(i) = 0
            Next

            Const standardFields = 2 ' The fields below:
            AddShort(vtableloc - _objectStart)
            AddShort((_vtableSize + standardFields) * HeapSizeOf.short)

            ' Search for an existing vtable that matches the current one.
            Dim existingVtable = 0
            For i = 0 To _numVtables - 1
                Dim vt1 = _bb.Length - _vtables(i)
                Dim vt2 = _space
                Dim len = _bb.GetShort(vt1)
                If len = _bb.GetShort(vt2) Then
                    Dim j = HeapSizeOf.short

                    While j < len
                        If _bb.GetShort(vt1 + j) <> _bb.GetShort(vt2 + j) Then
                            GoTo endLoop
                        End If

                        j += HeapSizeOf.short
                    End While
                    existingVtable = _vtables(i)
                    Exit For
                End If

endLoop:
            Next

            If existingVtable <> 0 Then
                ' Found a match:
                ' Remove the current vtable.
                _space = _bb.Length - vtableloc
                ' Point table to existing vtable.
                _bb.PutInt(_space, existingVtable - vtableloc)
            Else
                ' No match:
                ' Add the location of the current vtable to the list of
                ' vtables.
                If _numVtables = _vtables.Length Then
                    ' Arrays.CopyOf(vtables num_vtables * 2);
                    Dim newvtables = New Integer(_numVtables * 2 - 1) {}
                    Array.Copy(_vtables, newvtables, _vtables.Length)

                    _vtables = newvtables
                End If
                _vtables(std.Min(Threading.Interlocked.Increment(_numVtables), _numVtables - 1)) = Offset
                ' Point table to current vtable.
                _bb.PutInt(_bb.Length - vtableloc, Offset - vtableloc)
            End If

            _vtableSize = -1
            Return vtableloc
        End Function

        ' This checks a required field has been set in a given table that has
        ' just been constructed.
        Public Sub Required(table As Integer, field As Integer)
            Dim table_start = _bb.Length - table
            Dim vtable_start = table_start - _bb.GetInt(table_start)
            Dim ok = _bb.GetShort(vtable_start + field) <> 0
            ' If this fails, the caller will show what field needs to be set.
            If Not ok Then Throw New InvalidOperationException("FlatBuffers: field " & field.ToString() & " must be set")
        End Sub
        ''' @endcond

        ''' <summary>
        ''' Finalize a buffer, pointing to the given `root_table`.
        ''' </summary>
        ''' <paramname="rootTable">
        ''' An offset to be added to the buffer.
        ''' </param>
        Public Sub Finish(rootTable As Integer)
            Prep(_minAlign, HeapSizeOf.int)
            AddOffset(rootTable)
            _bb.Position = _space
        End Sub

        ''' <summary>
        ''' Get the ByteBuffer representing the FlatBuffer.
        ''' </summary>
        ''' <remarks>
        ''' This is typically only called after you call `Finish()`.
        ''' The actual data starts at the ByteBuffer's current position,
        ''' not necessarily at `0`.
        ''' </remarks>
        ''' <returns>
        ''' Returns the ByteBuffer for this FlatBuffer.
        ''' </returns>
        Public ReadOnly Property DataBuffer As ByteBuffer
            Get
                Return _bb
            End Get
        End Property

        ''' <summary>
        ''' A utility function to copy and return the ByteBuffer data as a
        ''' `byte[]`.
        ''' </summary>
        ''' <returns>
        ''' A full copy of the FlatBuffer data.
        ''' </returns>
        Public Function SizedByteArray() As Byte()
            Dim newArray = New Byte(_bb.Data.Length - _bb.Position - 1) {}
            Buffer.BlockCopy(_bb.Data, _bb.Position, newArray, 0, _bb.Data.Length - _bb.Position)
            Return newArray
        End Function

        ''' <summary>
        ''' Finalize a buffer, pointing to the given `rootTable`.
        ''' </summary>
        ''' <paramname="rootTable">
        ''' An offset to be added to the buffer.
        ''' </param>
        ''' <paramname="fileIdentifier">
        ''' A FlatBuffer file identifier to be added to the buffer before
        ''' `root_table`.
        ''' </param>
        Public Sub Finish(rootTable As Integer, fileIdentifier As String)
            Prep(_minAlign, HeapSizeOf.int + FileIdentifierLength)
            If fileIdentifier.Length <> FileIdentifierLength Then Throw New ArgumentException("FlatBuffers: file identifier must be length " & FileIdentifierLength.ToString(), "fileIdentifier")
            For i = FileIdentifierLength - 1 To 0 Step -1
                AddByte(Microsoft.VisualBasic.AscW(fileIdentifier(i)))
            Next
            Finish(rootTable)
        End Sub
    End Class
End Namespace

' @}
