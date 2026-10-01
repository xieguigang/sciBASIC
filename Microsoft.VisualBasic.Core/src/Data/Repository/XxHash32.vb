Option Strict On
Option Explicit On

Namespace Data.Repository

    ''' <summary>
    ''' XXH32 (32-bit xxHash) — the checksum algorithm used by the LZ4 frame format
    ''' for its block checksums and content checksums.
    ''' Pure BCL implementation, streaming-capable.
    ''' </summary>
    Public NotInheritable Class XxHash32

        Private Const P1 As UInteger = 2654435761UI   ' 0x9E3779B1
        Private Const P2 As UInteger = 2246822519UI   ' 0x85EBCA77
        Private Const P3 As UInteger = 3266489917UI   ' 0xC2B2AE3D
        Private Const P4 As UInteger = 668265263UI    ' 0x27D4EB2F
        Private Const P5 As UInteger = 374761393UI    ' 0x165667B1

        Private _seed As UInteger
        Private _v1, _v2, _v3, _v4 As UInteger
        Private _totalLength As ULong
        Private ReadOnly _mem(15) As Byte
        Private _memSize As Integer

        ''' <summary>Creates a streaming XXH32 state with the given seed (LZ4 always uses zero).</summary>
        Public Sub New(Optional seed As UInteger = 0UI)
            Reset(seed)
        End Sub

        ''' <summary>Resets the streaming state as if it had just been created.</summary>
        Public Sub Reset(Optional seed As UInteger = 0UI)
            _seed = seed
            _v1 = seed + P1 + P2
            _v2 = seed + P2
            _v3 = seed
            _v4 = seed - P1
            _totalLength = 0UL
            Array.Clear(_mem, 0, _mem.Length)
            _memSize = 0
        End Sub

        ''' <summary>Absorbs more bytes into the streaming state.</summary>
        Public Sub Update(data As Byte(), offset As Integer, count As Integer)
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If offset < 0 OrElse count < 0 OrElse offset + count > data.Length Then
                Throw New ArgumentOutOfRangeException("offset/count are outside of data.")
            End If
            If count = 0 Then Return

            Dim pos = offset
            Dim [end] = offset + count
            _totalLength += CULng(count)

            ' Not enough data to fill the 16-byte staging buffer yet.
            If _memSize + count < 16 Then
                Buffer.BlockCopy(data, pos, _mem, _memSize, count)
                _memSize += count
                Return
            End If

            ' Complete a pending partial block first.
            If _memSize > 0 Then
                Dim fill = 16 - _memSize
                Buffer.BlockCopy(data, pos, _mem, _memSize, fill)
                _v1 = Round(_v1, ReadLE32(_mem, 0))
                _v2 = Round(_v2, ReadLE32(_mem, 4))
                _v3 = Round(_v3, ReadLE32(_mem, 8))
                _v4 = Round(_v4, ReadLE32(_mem, 12))
                pos += fill
                _memSize = 0
            End If

            ' Consume 16-byte stripes.
            While [end] - pos >= 16
                _v1 = Round(_v1, ReadLE32(data, pos))
                _v2 = Round(_v2, ReadLE32(data, pos + 4))
                _v3 = Round(_v3, ReadLE32(data, pos + 8))
                _v4 = Round(_v4, ReadLE32(data, pos + 12))
                pos += 16
            End While

            ' Stash the remainder.
            If [end] > pos Then
                Buffer.BlockCopy(data, pos, _mem, 0, [end] - pos)
                _memSize = [end] - pos
            End If
        End Sub

        ''' <summary>Finishes the streaming state and returns the 32-bit digest.</summary>
        Public Function GetValue() As UInteger
            Dim h As UInteger
            If _totalLength >= 16UL Then
                h = Rotl(_v1, 1) + Rotl(_v2, 7) + Rotl(_v3, 12) + Rotl(_v4, 18)
            Else
                h = _seed + P5
            End If
            h += CUInt(_totalLength)          ' length is taken modulo 2^32

            Dim pos = 0
            While pos + 4 <= _memSize
                h = Rotl(h + (ReadLE32(_mem, pos) * P3), 17) * P4
                pos += 4
            End While
            While pos < _memSize
                h = Rotl(h + (CUInt(_mem(pos)) * P5), 11) * P1
                pos += 1
            End While

            h = Avalanche(h)
            Return h
        End Function

        ''' <summary>Computes the XXH32 digest of a byte range in one shot.</summary>
        Public Shared Function ComputeHash(data As Byte(), offset As Integer, count As Integer, Optional seed As UInteger = 0UI) As UInteger
            Dim h = New XxHash32(seed)
            h.Update(data, offset, count)
            Return h.GetValue()
        End Function

        Private Shared Function Round(acc As UInteger, lane As UInteger) As UInteger
            Return Rotl(acc + (lane * P2), 13) * P1
        End Function

        Private Shared Function Avalanche(h As UInteger) As UInteger
            h = h Xor (h >> 15)
            h *= P2
            h = h Xor (h >> 13)
            h *= P3
            h = h Xor (h >> 16)
            Return h
        End Function

        Private Shared Function Rotl(x As UInteger, r As Integer) As UInteger
            Return (x << r) Or (x >> (32 - r))
        End Function

        Private Shared Function ReadLE32(b As Byte(), i As Integer) As UInteger
            Return CUInt(b(i)) Or (CUInt(b(i + 1)) << 8) Or (CUInt(b(i + 2)) << 16) Or (CUInt(b(i + 3)) << 24)
        End Function

    End Class
End Namespace