Option Strict On
Option Explicit On
Option Infer On

Imports System.Text

Namespace Encodings

    ''' <summary>
    ''' INT96 时间戳（8 字节纳秒 + 4 字节儒略日）与 <see cref="DateTime"/> 之间的转换。
    ''' </summary>
    Public Module Int96

        ''' <summary>
        ''' 1970-01-01 对应的儒略日。
        ''' </summary>
        Public Const JulianEpoch As Integer = 2440588

        Public Function ToDateTime(bytes As Byte()) As DateTime
            If bytes Is Nothing OrElse bytes.Length < 12 Then
                Throw New ParquetException("the INT96 value should be 12 bytes!")
            End If

            Dim nanoseconds As Long = ReadInt64(bytes, 0)
            Dim julianDay As Integer = ReadInt32(bytes, 8)
            Dim ticks As Long = nanoseconds \ 100L

            If ticks < 0 OrElse ticks > TimeSpan.TicksPerDay Then
                ticks = ((ticks Mod TimeSpan.TicksPerDay) + TimeSpan.TicksPerDay) Mod TimeSpan.TicksPerDay
            End If

            Return DateTime.UnixEpoch.AddDays(julianDay - JulianEpoch).AddTicks(ticks)
        End Function

        Public Function FromDateTime(time As DateTime) As Byte()
            Dim utc As DateTime = time.ToUniversalTime()
            Dim days As Integer = CInt((utc.Date - DateTime.UnixEpoch.Date).TotalDays)
            Dim nanoseconds As Long = (utc - utc.Date).Ticks * 100L
            Dim buffer As Byte() = New Byte(11) {}

            Call WriteInt64(buffer, 0, nanoseconds)
            Call WriteInt32(buffer, 8, days + JulianEpoch)

            Return buffer
        End Function

        Private Function ReadInt64(bytes As Byte(), offset As Integer) As Long
            If BitConverter.IsLittleEndian Then
                Return BitConverter.ToInt64(bytes, offset)
            End If

            Dim temp As Byte() = New Byte(7) {}

            System.Buffer.BlockCopy(bytes, offset, temp, 0, 8)
            Array.Reverse(temp)

            Return BitConverter.ToInt64(temp, 0)
        End Function

        Private Function ReadInt32(bytes As Byte(), offset As Integer) As Integer
            If BitConverter.IsLittleEndian Then
                Return BitConverter.ToInt32(bytes, offset)
            End If

            Dim temp As Byte() = New Byte(3) {}

            System.Buffer.BlockCopy(bytes, offset, temp, 0, 4)
            Array.Reverse(temp)

            Return BitConverter.ToInt32(temp, 0)
        End Function

        Private Sub WriteInt64(bytes As Byte(), offset As Integer, value As Long)
            Dim temp As Byte() = BitConverter.GetBytes(value)

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(temp)
            End If

            System.Buffer.BlockCopy(temp, 0, bytes, offset, 8)
        End Sub

        Private Sub WriteInt32(bytes As Byte(), offset As Integer, value As Integer)
            Dim temp As Byte() = BitConverter.GetBytes(value)

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(temp)
            End If

            System.Buffer.BlockCopy(temp, 0, bytes, offset, 4)
        End Sub

    End Module

End Namespace
