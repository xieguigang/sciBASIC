Imports System.IO
Imports System.Threading
Imports std = System.Math

Namespace Impl
    Friend Class BufferedStream
        Inherits Stream
        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property Length As Long
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides Property Position As Long

        Private Outer As MultiStreamProvider

        Public Sub New(outer As MultiStreamProvider, startingPosition As Long)
            Me.Outer = outer
        End Sub

        Public Overrides Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
            Throw New NotImplementedException()
        End Function

        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Dim newPosition As Long

            Select Case origin
                Case SeekOrigin.Begin
                    newPosition = offset
                Case SeekOrigin.Current
                    newPosition = Position + offset
                Case SeekOrigin.End
                    Throw New NotImplementedException()
                Case Else
                    Throw New Exception("Unexpected SeekOrigin: " & origin.ToString())
            End Select

            If newPosition < Position Then Throw New InvalidOperationException($"Cannot seek backwards in BufferedStream")

            Position = newPosition

            Return newPosition
        End Function

        Public Overrides Sub SetLength(value As Long)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Write(buffer As Byte(), offset As Integer, count As Integer)
            Outer.Push(Position, buffer, offset, count)
            Position += count
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            Outer.RemoveChild(Me)

            MyBase.Dispose(disposing)
        End Sub

        Public Overrides Sub Flush()
            Outer.RequestFlush(Me)
        End Sub
    End Class

    Friend Class MultiStreamProvider
        Const BUFFER_COUNT As Integer = 8
        Const INITIAL_BUFFER_SIZE As Integer = 32

        Friend Class PendingEntry
            Implements IEquatable(Of PendingEntry)
            Private _WriteAtPosition As Long, _Data As Byte(), _Count As Integer

            Public Property WriteAtPosition As Long
                Get
                    Return _WriteAtPosition
                End Get
                Private Set(value As Long)
                    _WriteAtPosition = value
                End Set
            End Property

            Public Property Data As Byte()
                Get
                    Return _Data
                End Get
                Private Set(value As Byte())
                    _Data = value
                End Set
            End Property

            Public Property Count As Integer
                Get
                    Return _Count
                End Get
                Private Set(value As Integer)
                    _Count = value
                End Set
            End Property

            Public Sub New(writeAt As Long, data As Byte(), count As Integer)
                WriteAtPosition = writeAt
                Me.Data = data
                Me.Count = count
            End Sub

            Public Overloads Function Equals(other As PendingEntry) As Boolean Implements IEquatable(Of PendingEntry).Equals
                Return WriteAtPosition = WriteAtPosition AndAlso Data Is Data AndAlso Count = Count
            End Function

            Public Overrides Function Equals(obj As Object) As Boolean
                If Not (TypeOf obj Is PendingEntry) Then Return False

                Return Equals(CType(obj, PendingEntry))
            End Function

            Public Overrides Function GetHashCode() As Integer
                Dim ret = 0
                ret += WriteAtPosition.GetHashCode()
                ret *= 17
                ret += If(Data?.GetHashCode(), 0)
                ret *= 17
                ret += Count.GetHashCode()

                Return ret
            End Function
        End Class

        Private Buffers As Byte()()

        Private TrueStream As Stream
        Private Pending As List(Of PendingEntry)
        Private Children As List(Of BufferedStream)
        Private WantsFlush As List(Of Boolean)
        Public Sub New(trueStream As Stream)
            Me.TrueStream = trueStream
            Pending = New List(Of PendingEntry)()
            Children = New List(Of BufferedStream)()
            WantsFlush = New List(Of Boolean)()
            Buffers = Enumerable.Range(0, BUFFER_COUNT).[Select](Function(b) New Byte(31) {}).ToArray()
        End Sub

        Public Function CreateChildStream() As BufferedStream
            Dim nearestPoint = If(Children.Count = 0, 0, Children.Min(Function(c) c.Position))

            Dim ret = New BufferedStream(Me, nearestPoint)
            Children.Add(ret)
            WantsFlush.Add(False)

            Return ret
        End Function

        Public Sub Push(position As Long, data As Byte(), offset As Integer, count As Integer)
            Dim inBuffer = PlaceInBuffer(data, offset, count)
            Array.Copy(data, offset, inBuffer, 0, count)

            Pending.Add(New PendingEntry(position, inBuffer, count))
        End Sub

        Public Sub RemoveChild(child As BufferedStream)
            If Not Children.Remove(child) Then
                Throw New InvalidOperationException("Removed same child twice, probably a double disposal")
            End If

            If Children.Count = 0 Then
                ' clear everything in the queue, now that we've torn it down
                WriteToStream()
            End If
        End Sub

        Public Sub RequestFlush(child As BufferedStream)
            Dim ix = Children.IndexOf(child)
            WantsFlush(ix) = True

            If WantsFlush.All(Function(__) __) Then
                WriteToStream()
                For i = 0 To Children.Count - 1
                    WantsFlush(i) = False
                Next
            End If
        End Sub

        Private Sub AdvanceTo(toPosition As Long)
            If TrueStream.Position < toPosition Then
                If TrueStream.CanSeek Then
                    TrueStream.Seek(toPosition, SeekOrigin.Begin)
                Else
                    If TrueStream.CanRead Then
                        While TrueStream.Position < toPosition
                            TrueStream.ReadByte()
                        End While
                    Else
                        If TrueStream.CanWrite Then
                            While TrueStream.Position < toPosition
                                TrueStream.WriteByte(0)
                            End While
                        End If
                    End If
                End If
            End If
        End Sub

        Private Sub WriteToStream()
            For Each write As PendingEntry In Pending.OrderBy(Function(p) p.WriteAtPosition)
                Dim atPosition = write.WriteAtPosition
                Me.AdvanceTo(atPosition)

                TrueStream.Write(write.Data, 0, write.Count)
            Next

            TrueStream.Flush()

            Pending.Clear()
        End Sub

        Private Function PlaceInBuffer(data As Byte(), offset As Integer, count As Integer) As Byte()
            Dim placeIn As Byte()

            For i = 0 To Buffers.Length - 1
                Dim candidateBuffer = Buffers(i)
                If candidateBuffer IsNot Nothing AndAlso candidateBuffer.Length >= count Then
                    Dim exRes = Interlocked.CompareExchange(Buffers(i), Nothing, candidateBuffer)
                    If ReferenceEquals(candidateBuffer, exRes) Then
                        placeIn = exRes
                        GoTo copy
                    End If
                End If
            Next

            placeIn = New Byte(std.Max(count, INITIAL_BUFFER_SIZE) - 1) {}

copy:
            Array.Copy(data, offset, placeIn, 0, count)

            Return placeIn
        End Function

        Private Sub ReturnBuffer(buffer As Byte())
            For i = 0 To Buffers.Length - 1
                Dim inBuffers = Buffers(i)
                If inBuffers Is Nothing OrElse inBuffers.Length < buffer.Length Then
                    Dim exRes = Interlocked.CompareExchange(Buffers(i), buffer, inBuffers)
                    If ReferenceEquals(exRes, inBuffers) Then
                        If exRes IsNot Nothing Then
                            ' try saving this buffer
                            ReturnBuffer(exRes)
                        End If

                        Return
                    End If
                End If
            Next
        End Sub
    End Class
End Namespace
