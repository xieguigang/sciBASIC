Namespace Data.Repository

    ''' <summary>
    ''' a unique Snowflake ID generator
    ''' </summary>
    Public Class SnowflakeIdGenerator

        Public Const DefaultEpoch As Long = 1288834974657L

        ReadOnly _epoch As Long
        ReadOnly _machineId As Long

        Dim _sequence As Long = 0L

        Public Sub New(machineId As Long, Optional epoch As Long = DefaultEpoch, Optional seqId As Long = 0L)
            _machineId = machineId
            _epoch = epoch
            _sequence = seqId
        End Sub

        Public Function GenerateId() As Long
            SyncLock Me
                Dim currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                _sequence = _sequence + 1 And 4095
                Return currentTimestamp - _epoch << 22 Or _machineId << 12 Or _sequence
            End SyncLock
        End Function
    End Class
End Namespace