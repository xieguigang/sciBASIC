Namespace util.FVecArray

    <Serializable>
    Friend Class FVecFloatArrayImpl
        Implements FVec

        Friend ReadOnly values As Single()
        Friend ReadOnly treatsZeroAsNA As Boolean

        Friend Sub New(values As Single(), treatsZeroAsNA As Boolean)
            Me.values = values
            Me.treatsZeroAsNA = treatsZeroAsNA
        End Sub

        Public Overridable Function fvalue(index As Integer) As Double Implements FVec.fvalue
            If values.Length <= index Then
                Return Double.NaN
            End If

            Dim result As Double = values(index)

            If treatsZeroAsNA AndAlso result = 0 Then
                Return Double.NaN
            End If

            Return result
        End Function
    End Class

    <Serializable>
    Friend Class FVecFloatArrayImplement
        Implements FVec

        Friend ReadOnly values As Single()
        Friend ReadOnly treatsValueAsNA As Single

        Friend Sub New(values As Single(), treatsValueAsNA As Single)
            Me.values = values
            Me.treatsValueAsNA = treatsValueAsNA
        End Sub

        Public Overridable Function fvalue(index As Integer) As Double Implements FVec.fvalue
            If values.Length <= index Then
                Return Double.NaN
            End If

            Dim result As Double = values(index)

            If treatsValueAsNA = result Then
                Return Double.NaN
            End If

            Return result
        End Function
    End Class

    <Serializable>
    Friend Class FVecDoubleArrayImpl
        Implements FVec

        Friend ReadOnly values As Double()
        Friend ReadOnly treatsZeroAsNA As Boolean

        Friend Sub New(values As Double(), treatsZeroAsNA As Boolean)
            Me.values = values
            Me.treatsZeroAsNA = treatsZeroAsNA
        End Sub

        Public Overridable Function fvalue(index As Integer) As Double Implements FVec.fvalue
            If values.Length <= index Then
                Return Double.NaN
            End If

            Dim result = values(index)

            If treatsZeroAsNA AndAlso result = 0 Then
                Return Double.NaN
            End If

            Return values(index)
        End Function
    End Class

    <Serializable>
    Friend Class FVecDoubleArrayImplement
        Implements FVec

        Friend ReadOnly values As Double()
        Friend ReadOnly treatsValueAsNA As Double

        Friend Sub New(values As Double(), treatsValueAsNA As Double)
            Me.values = values
            Me.treatsValueAsNA = treatsValueAsNA
        End Sub

        Public Overridable Function fvalue(index As Integer) As Double Implements FVec.fvalue
            If values.Length <= index Then
                Return Double.NaN
            End If

            Dim result = values(index)

            If treatsValueAsNA = result Then
                Return Double.NaN
            End If

            Return values(index)
        End Function
    End Class

End Namespace