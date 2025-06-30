Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Interpolation

''' <summary>
''' make the signal data interpolation
''' </summary>
Public Module Interpolation

    <Extension>
    Public Function BSpline(Of T As ITimeSignal)(source As IEnumerable(Of T),
                                                 Optional degree As Double = 2,
                                                 Optional res As Double = 5) As IEnumerable(Of TimeSignal)
        Dim points As PointF() = source _
            .SafeQuery _
            .Select(Function(ti)
                        Return New PointF(ti.time, ti.intensity)
                    End Function) _
            .ToArray
        Dim interpolate = B_Spline.Compute(points, degree, res).ToArray

        Return From i As PointF In interpolate Select New TimeSignal(i)
    End Function

    <Extension>
    Public Function BSpline(Of T As ITimeSignal)(source As IEnumerable(Of T), activator As Func(Of Single, Single, T), Optional degree As Double = 2, Optional res As Double = 5) As IEnumerable(Of T)
        Dim points As PointF() = source _
           .SafeQuery _
           .Select(Function(ti)
                       Return New PointF(ti.time, ti.intensity)
                   End Function) _
           .ToArray
        Dim interpolate = B_Spline.Compute(points, degree, res).ToArray

        Return From i As PointF In interpolate Select activator(i.X, i.Y)
    End Function
End Module
