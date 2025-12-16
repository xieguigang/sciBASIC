#Region "Microsoft.VisualBasic::3cbd4aafa3c0176e4382f69f51cab34c, Data_science\Mathematica\Math\Math\Algebra\Vector\Class\Math.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 227
    '    Code Lines: 134 (59.03%)
    ' Comment Lines: 62 (27.31%)
    '    - Xml Docs: 93.55%
    ' 
    '   Blank Lines: 31 (13.66%)
    '     File Size: 9.83 KB


    '     Class Vector
    ' 
    '         Function: (+2 Overloads) Abs, BesselI, (+2 Overloads) Exp, floor, (+2 Overloads) Log
    '                   (+2 Overloads) Log10, (+3 Overloads) Max, (+3 Overloads) Min, Order, pchisq
    '                   Quantile, round, Sign, Sinh, Sqrt
    '                   Trunc
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports sys = System.Math

Namespace LinearAlgebra

    Partial Class Vector

        ''' <summary>
        ''' abs(x) computes the absolute value of x, sqrt(x) computes the (principal) square root of x, √{x}.
        ''' </summary>
        ''' <param name="x">a numeric or complex vector or array.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Sqrt(x As Vector) As Vector
            Return New Vector(From n In x Select sys.Sqrt(n))
        End Function

        ''' <summary>
        ''' log computes logarithms, by default natural logarithms, log10 computes common (i.e., base 10) logarithms, 
        ''' and log2 computes binary (i.e., base 2) logarithms. 
        ''' The general form log(x, base) computes logarithms with base base.
        ''' log1p(x) computes log(1+x) accurately also for |x| &lt;&lt; 1.
        ''' exp computes the exponential function.
        ''' expm1(x) computes exp(x) - 1 accurately also for |x| &lt;&lt; 1.
        ''' </summary>
        ''' <param name="x">a numeric or complex vector.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Exp(x As Vector) As Vector
            Return New Vector(From n As Double In x Select sys.Exp(n))
        End Function

        ''' <summary>
        ''' log computes logarithms, by default natural logarithms, log10 computes common (i.e., base 10) logarithms, 
        ''' and log2 computes binary (i.e., base 2) logarithms. 
        ''' The general form log(x, base) computes logarithms with base base.
        ''' log1p(x) computes log(1+x) accurately also for |x| &lt;&lt; 1.
        ''' exp computes the exponential function.
        ''' expm1(x) computes exp(x) - 1 accurately also for |x| &lt;&lt; 1.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Exp() As Vector
            Dim vexp As Double() = New Double([Dim] - 1) {}

            For i As Integer = 0 To vexp.Length - 1
                vexp(i) = sys.Exp(buffer(i))
            Next

            Return New Vector(vexp)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Log(Optional base# = sys.E) As Vector
            Return Vector.Log(Me, base)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="percentage">``[0, 1]``之间</param>
        ''' <returns></returns>
        Public Function Quantile(ParamArray percentage As Double()) As Vector
            With GKQuantile
                If percentage.Length = 0 Then
                    ' 返回默认的0, 0.25, 0.5, 0.75, 1
                    percentage = {0, 0.25, 0.5, 0.75, 1}
                End If

                Return percentage _
                    .Select(Function(p) .Query(p)) _
                    .AsVector
            End With
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x">a numeric or complex vector.</param>
        ''' <returns></returns>
        ''' <param name="base">a positive or complex number: the base with respect to which logarithms are computed. Defaults to e=exp(1).</param>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Log(x As Vector, Optional base As Double = sys.E) As Vector
            Return New Vector(From n As Double In x Select sys.Log(n, base))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Log10(x As Vector) As Vector
            Return Log(x, base:=10)
        End Function

        Public Function Log10() As Vector
            Return Log(Me, base:=10)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Max(x As Vector) As Double
            Return x.Max
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Max(x As Vector, y#) As Vector
            Return x.Select(Function(xi) sys.Max(xi, y)).AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Max(x As Vector, y As Vector) As Vector
            Return x.SeqIterator _
                .Select(Function(i) sys.Max(i.value, y(i))) _
                .AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Min(x As Vector) As Double
            Return x.Min
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Min(x As Vector, y#) As Vector
            Return x.Select(Function(xi) sys.Min(xi, y)).AsVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Min(x As Vector, y As Vector) As Vector
            Return x.SeqIterator _
                .Select(Function(i) sys.Min(i.value, y(i))) _
                .AsVector
        End Function

        ''' <summary>
        ''' Rounding of Numbers
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Trunc(x As Vector) As Vector
            Return New Vector(x.Select(AddressOf sys.Truncate))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Abs(x As Vector) As Vector
            Return New Vector(From d As Double In x Select sys.Abs(d))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Abs(x As IEnumerable(Of Double)) As Vector
            Return New Vector(From d As Double In x Select sys.Abs(d))
        End Function

        ''' <summary>
        ''' Bessel Functions of integer and fractional order, of first and second kind, J(nu) and Y(nu), 
        ''' and Modified Bessel functions (of first and third kind), I(nu) and K(nu).
        ''' </summary>
        ''' <returns>
        ''' Numeric vector with the (scaled, if expon.scaled = TRUE) values of the corresponding Bessel function.
        ''' The length of the result is the maximum of the lengths of the parameters. All parameters are recycled to that length.
        ''' </returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Besseli")>
        Public Shared Function BesselI(x As Vector, nu As Vector,
                                <Parameter("expon.scaled", "logical; if TRUE, the results are exponentially scaled in order to avoid overflow (I(nu)) or underflow (K(nu)), respectively.")>
                                Optional ExponScaled As Boolean = False) As Vector
            Throw New NotImplementedException
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Floor")>
        Public Shared Function floor(x As Vector) As Vector
            Return New Vector(x.Select(AddressOf sys.Floor))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Round")>
        Public Shared Function round(x As Vector, Optional digits As Integer = 4) As Vector
            Return New Vector(x.Select(Function(n) sys.Round(n, digits)))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Sinh")>
        Public Shared Function Sinh(x As Vector) As Vector
            Return New Vector(x.Select(AddressOf sys.Sinh))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Sign")>
        Public Shared Function Sign(x As Vector) As Vector
            Return New Vector(x.Select(AddressOf sys.Sign))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("pchisq")>
        Public Shared Function pchisq(q As Vector, df As Vector, Optional ncp As Integer = 0, Optional lowertail As Boolean = True, Optional logp As Boolean = False) As Vector
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' order returns a permutation which rearranges its first argument into ascending or descending order, 
        ''' breaking ties by further arguments. sort.list is the same, using only one argument.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Order")>
        Public Shared Function Order(x As Vector, Optional nalast As Boolean = True, Optional decreasing As Boolean = False) As Vector
            Dim seq As IEnumerable(Of SeqValue(Of Double)) = x.SeqIterator(offset:=1).ToArray

            If decreasing Then
                seq = seq.OrderByDescending(Function(xi) xi.value)
            Else
                seq = seq.OrderBy(Function(xi) xi.value)
            End If

            Return seq.Select(Function(xi) xi.i).AsVector
        End Function
    End Class
End Namespace
