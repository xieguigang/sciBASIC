#Region "Microsoft.VisualBasic::c0676baeab434ca2454f60e06388f008, Data_science\Mathematica\Math\DataFittings\Linear\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: X, Y, Yfit
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function X(fit As IFitted) As Vector
        Return fit.ErrorTest.Select(Function(point) point.X).AsVector
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Y(fit As IFitted) As Vector
        Return fit.ErrorTest.Select(Function(point) point.Y).AsVector
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function Yfit(fit As IFitted) As Vector
        Return fit.ErrorTest.Select(Function(point) point.Yfit).AsVector
    End Function
End Module
