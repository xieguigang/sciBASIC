#Region "Microsoft.VisualBasic::767689f9597a8d66a00b6f279c6a05ca, Data_science\Mathematica\Math\Math\Spline\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module Extensions
    ' 
    '         Function: CubicSpline
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Interpolation

    <HideModuleName>
    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CubicSpline(points As IEnumerable(Of PointF), Optional expected# = 100) As PointF()
            Return Interpolation.CubicSpline.RecalcSpline(points, expected).ToArray
        End Function
    End Module
End Namespace
