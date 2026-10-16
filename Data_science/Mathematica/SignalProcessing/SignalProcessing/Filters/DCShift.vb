Imports System.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Filters

    ''' <summary>
    ''' the D.C. offset shift helpers, they move the amplitude of a signal into the non
    ''' negative range so that the rectifying based analysis (envelope, modulation
    ''' measurement) would not mix the negative excursion into the positive one.
    ''' 
    ''' ### two shift conventions of current library
    ''' 
    ''' the two functions of this module are **not** the same thing, the caller must pick
    ''' the one which matches its signal convention:
    ''' 
    ''' | function | shift value | input | in place |
    ''' |----------|-------------|-------|-----------|
    ''' | <see cref="ShiftToPositive"/> | the max amplitude of the signal, applied unconditionally | ``Single()`` wave samples | yes, the input array is modified |
    ''' | <see cref="PositiveShift"/> | the abs of the minimal value, applied only when the signal has a negative sample | ``Double()`` signal curve | no, a new array is returned when the shift is applied |
    ''' </summary>
    Public Module DCShift

        ''' <summary>
        ''' shift the single precision wave samples into the non negative range.
        ''' 
        ''' the shift value is the max amplitude of the signal, and the input array is
        ''' modified in place and returned back (the same array instance is returned).
        ''' </summary>
        ''' <param name="data">the wave sample data, it is modified in place</param>
        Public Function ShiftToPositive(data As Single()) As Single()
            Dim neg As Vector = -New Vector(data)
            Dim base As Single = std.Abs(neg.Min)

            For i = 0 To data.Length - 1
                data(i) += base
            Next

            Return data
        End Function

        ''' <summary>
        ''' shift a double precision signal curve into the non negative range.
        ''' 
        ''' the shift value is the abs of the minimal value, and it is only applied when the
        ''' signal has a negative sample, otherwise the input array is returned as it is.
        ''' 
        ''' this is the shared implementation of ``Alignment.Normalize``.
        ''' </summary>
        ''' <param name="data">the signal curve, it is not modified</param>
        Public Function PositiveShift(data As Double()) As Double()
            If data.Any(Function(vi) vi < 0) Then
                Dim minY As Double = std.Abs(data.Min)
                Dim norm As Double() = data _
                    .Select(Function(v) v + minY) _
                    .ToArray

                Return norm
            End If

            Return data
        End Function
    End Module
End Namespace
