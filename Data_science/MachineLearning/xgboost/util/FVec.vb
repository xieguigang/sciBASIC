Imports System
Imports System.Collections.Generic

Namespace util

    ''' <summary>
    ''' Interface of feature vector.
    ''' </summary>
    Public Interface FVec
        ''' <summary>
        ''' Gets index-th value.
        ''' </summary>
        ''' <paramname="index"> index </param>
        ''' <returns> value </returns>
        Function fvalue(ByVal index As Integer) As Double
    End Interface

    Public Class FVec_Transformer
        Friend Sub New()
            ' do nothing
        End Sub

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <paramname="values">         float values </param>
        ''' <paramname="treatsZeroAsNA"> treat zero as N/A if true </param>
        ''' <returns> FVec </returns>
        Public Shared Function fromArray(ByVal values As Single(), ByVal treatsZeroAsNA As Boolean) As FVec
            Return New FVecArrayImpl.FVecFloatArrayImpl(values, treatsZeroAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <paramname="values">         double values </param>
        ''' <paramname="treatsZeroAsNA"> treat zero as N/A if true </param>
        ''' <returns> FVec </returns>
        Public Shared Function fromArray(ByVal values As Double(), ByVal treatsZeroAsNA As Boolean) As FVec
            Return New FVecArrayImpl.FVecDoubleArrayImpl(values, treatsZeroAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <paramname="values">          float values </param>
        ''' <paramname="treatsValueAsNA"> treat specify value as N/A </param>
        ''' <returns> FVec </returns>
        Public Shared Function fromArray(ByVal values As Single(), ByVal treatsValueAsNA As Single) As FVec
            Return New FVecArrayImpl.FVecFloatArrayImplement(values, treatsValueAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <paramname="values">          double values </param>
        ''' <paramname="treatsValueAsNA"> treat specify value as N/A </param>
        ''' <returns> FVec </returns>
        Public Shared Function fromArray(ByVal values As Double(), ByVal treatsValueAsNA As Double) As FVec
            Return New FVecArrayImpl.FVecDoubleArrayImplement(values, treatsValueAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from map.
        ''' </summary>
        ''' <paramname="map"> map containing non-zero values </param>
        ''' <returns> FVec </returns>
        Public Shared Function fromMap(Of T1 As Number)(ByVal map As IDictionary(Of Integer, T1)) As FVec
            Return New FVecMapImpl(Of T1)(map)
        End Function
    End Class

    <Serializable>
    Friend Class FVecMapImpl(Of T1)
        Implements FVec

        Private ReadOnly values As IDictionary(Of Integer, T1)

        Public Sub New(ByVal values As IDictionary(Of Integer, T1))
            Me.values = values
        End Sub

        Public Overridable Function fvalue(ByVal index As Integer) As Double Implements FVec.fvalue
            Dim number As Number = values.GetValueOrNull(index)

            If number Is Nothing Then
                Return Double.NaN
            End If

            Return number.doubleValue()
        End Function
    End Class

    Friend Class FVecArrayImpl
        <Serializable>
        Friend Class FVecFloatArrayImpl
            Implements FVec

            Friend ReadOnly values As Single()
            Friend ReadOnly treatsZeroAsNA As Boolean

            Friend Sub New(ByVal values As Single(), ByVal treatsZeroAsNA As Boolean)
                Me.values = values
                Me.treatsZeroAsNA = treatsZeroAsNA
            End Sub

            Public Overridable Function fvalue(ByVal index As Integer) As Double Implements FVec.fvalue
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

            Friend Sub New(ByVal values As Single(), ByVal treatsValueAsNA As Single)
                Me.values = values
                Me.treatsValueAsNA = treatsValueAsNA
            End Sub

            Public Overridable Function fvalue(ByVal index As Integer) As Double Implements FVec.fvalue
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

            Friend Sub New(ByVal values As Double(), ByVal treatsZeroAsNA As Boolean)
                Me.values = values
                Me.treatsZeroAsNA = treatsZeroAsNA
            End Sub

            Public Overridable Function fvalue(ByVal index As Integer) As Double Implements FVec.fvalue
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

            Friend Sub New(ByVal values As Double(), ByVal treatsValueAsNA As Double)
                Me.values = values
                Me.treatsValueAsNA = treatsValueAsNA
            End Sub

            Public Overridable Function fvalue(ByVal index As Integer) As Double Implements FVec.fvalue
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
    End Class
End Namespace
