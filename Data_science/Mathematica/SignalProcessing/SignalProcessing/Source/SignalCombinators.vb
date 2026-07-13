#Region "Microsoft.VisualBasic::55cb689fc2db80a94421c299bb39d1f5, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\SignalCombinators.vb"

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

    '   Total Lines: 345
    '    Code Lines: 235 (68.12%)
    ' Comment Lines: 54 (15.65%)
    '    - Xml Docs: 98.15%
    ' 
    '   Blank Lines: 56 (16.23%)
    '     File Size: 11.82 KB


    '     Class Sum
    ' 
    '         Properties: A, B
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, Sample
    ' 
    '     Class Difference
    ' 
    '         Properties: A, B
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, Sample
    ' 
    '     Class Product
    ' 
    '         Properties: A, B
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, Sample
    ' 
    '     Class Quotient
    ' 
    '         Properties: A, B
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, Sample
    ' 
    '     Class Convolution
    ' 
    '         Properties: GridMax, GridMin, GridN, Kernel, Signal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Evaluate, lookup, Sample
    ' 
    '         Sub: ensureCache
    ' 
    '     Class SignalGenerator
    ' 
    ' 
    '         Enum Operations
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: Seed
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Add, Divide, Evaluate, Multiply, Sample
    '               Subtract
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math
Imports Microsoft.VisualBasic.Math.RandomExtensions

Namespace Source.Generators

    ''' <summary>
    ''' point-wise addition of two basis functions.
    ''' </summary>
    ''' <remarks>The most common combination, e.g.
    ''' Signal = LinearTrend + SineSeasonality + GaussianPeak + Noise.</remarks>
    Public Class Sum : Inherits BasisFunction

        Public Property A As BasisFunction
        Public Property B As BasisFunction

        Public Sub New(a As BasisFunction, b As BasisFunction)
            Me.A = a
            Me.B = b
        End Sub

        Public Overrides Function Evaluate(x As Double) As Double
            Return A.Evaluate(x) + B.Evaluate(x)
        End Function

        Public Overrides Function Sample(x As IEnumerable(Of Double)) As Double()
            Dim xs = x.ToArray
            Dim ya = A.Sample(xs)
            Dim yb = B.Sample(xs)
            Dim out(xs.Length - 1) As Double
            For i As Integer = 0 To xs.Length - 1
                out(i) = ya(i) + yb(i)
            Next
            Return out
        End Function
    End Class

    ''' <summary>
    ''' point-wise subtraction: A - B.
    ''' </summary>
    Public Class Difference : Inherits BasisFunction

        Public Property A As BasisFunction
        Public Property B As BasisFunction

        Public Sub New(a As BasisFunction, b As BasisFunction)
            Me.A = a
            Me.B = b
        End Sub

        Public Overrides Function Evaluate(x As Double) As Double
            Return A.Evaluate(x) - B.Evaluate(x)
        End Function

        Public Overrides Function Sample(x As IEnumerable(Of Double)) As Double()
            Dim xs = x.ToArray
            Dim ya = A.Sample(xs)
            Dim yb = B.Sample(xs)
            Dim out(xs.Length - 1) As Double
            For i As Integer = 0 To xs.Length - 1
                out(i) = ya(i) - yb(i)
            Next
            Return out
        End Function
    End Class

    ''' <summary>
    ''' point-wise multiplication (modulation).
    ''' </summary>
    ''' <remarks>
    ''' e.g. Sine * ExponentialDecay gives a damped sine; or multiply by a
    ''' Sigmoid / Step to fade a signal in or out.
    ''' </remarks>
    Public Class Product : Inherits BasisFunction

        Public Property A As BasisFunction
        Public Property B As BasisFunction

        Public Sub New(a As BasisFunction, b As BasisFunction)
            Me.A = a
            Me.B = b
        End Sub

        Public Overrides Function Evaluate(x As Double) As Double
            Return A.Evaluate(x) * B.Evaluate(x)
        End Function

        Public Overrides Function Sample(x As IEnumerable(Of Double)) As Double()
            Dim xs = x.ToArray
            Dim ya = A.Sample(xs)
            Dim yb = B.Sample(xs)
            Dim out(xs.Length - 1) As Double
            For i As Integer = 0 To xs.Length - 1
                out(i) = ya(i) * yb(i)
            Next
            Return out
        End Function
    End Class

    ''' <summary>
    ''' point-wise division: A / B (0 where B is 0).
    ''' </summary>
    Public Class Quotient : Inherits BasisFunction

        Public Property A As BasisFunction
        Public Property B As BasisFunction

        Public Sub New(a As BasisFunction, b As BasisFunction)
            Me.A = a
            Me.B = b
        End Sub

        Public Overrides Function Evaluate(x As Double) As Double
            Dim d = B.Evaluate(x)
            If d = 0 Then
                Return 0
            End If
            Return A.Evaluate(x) / d
        End Function

        Public Overrides Function Sample(x As IEnumerable(Of Double)) As Double()
            Dim xs = x.ToArray
            Dim ya = A.Sample(xs)
            Dim yb = B.Sample(xs)
            Dim out(xs.Length - 1) As Double
            For i As Integer = 0 To xs.Length - 1
                out(i) = If(yb(i) = 0, 0.0, ya(i) / yb(i))
            Next
            Return out
        End Function
    End Class

    ''' <summary>
    ''' convolution of a signal with a kernel, simulating a system's impulse
    ''' response to the input signal.
    ''' </summary>
    ''' <remarks>
    ''' The signal and the kernel are both sampled on the same time grid
    ''' (defined by <see cref="GridMin"/>, <see cref="GridMax"/>, <see cref="GridN"/>),
    ''' then convolved with <see cref="MathUtils.Convolve"/>. The result is
    ''' cached so repeated evaluation is a cheap table lookup.
    ''' </remarks>
    Public Class Convolution : Inherits BasisFunction

        Public Property Signal As BasisFunction
        Public Property Kernel As BasisFunction

        Public Property GridMin As Double = 0.0
        Public Property GridMax As Double = 100.0
        Public Property GridN As Integer = 1000

        Private cacheX As Double()
        Private cacheY As Double()

        Public Sub New(signal As BasisFunction, kernel As BasisFunction,
                       Optional gridMin As Double = 0.0,
                       Optional gridMax As Double = 100.0,
                       Optional gridN As Integer = 1000)
            Me.Signal = signal
            Me.Kernel = kernel
            Me.GridMin = gridMin
            Me.GridMax = gridMax
            Me.GridN = gridN
            Call ensureCache()
        End Sub

        Private Sub ensureCache()
            If cacheY IsNot Nothing AndAlso cacheY.Length = GridN Then
                Return
            End If

            Dim xs(GridN - 1) As Double
            Dim stepX = (GridMax - GridMin) / (GridN - 1)

            For i As Integer = 0 To GridN - 1
                xs(i) = GridMin + i * stepX
            Next

            Dim sig = Signal.Sample(xs)
            Dim ker = Kernel.Sample(xs)
            Dim conv = MathUtils.Convolve(sig, ker)

            ' keep the first GridN samples (drop the kernel-length transient tail)
            Dim out(GridN - 1) As Double
            Array.Copy(conv, out, GridN)

            cacheX = xs
            cacheY = out
        End Sub

        Public Overrides Function Sample(x As IEnumerable(Of Double)) As Double()
            Call ensureCache()
            Return x.Select(AddressOf lookup).ToArray
        End Function

        Public Overrides Function Evaluate(x As Double) As Double
            Call ensureCache()
            Return lookup(x)
        End Function

        Private Function lookup(x As Double) As Double
            If x < cacheX(0) OrElse x > cacheX(cacheX.Length - 1) Then
                Return 0.0
            End If
            Dim idx = CInt(std.Round((x - cacheX(0)) / (cacheX(1) - cacheX(0))))
            If idx < 0 Then idx = 0
            If idx >= cacheY.Length Then idx = cacheY.Length - 1
            Return cacheY(idx)
        End Function
    End Class

    ''' <summary>
    ''' the fluent builder / combiner for synthesizing complex signals out of
    ''' basis functions. It is itself a <see cref="BasisFunction"/> so it can be
    ''' nested inside other combinators, and it can be sampled into a
    ''' <see cref="GeneralSignal"/> or a <see cref="Double"/> array.
    ''' </summary>
    ''' <remarks>
    ''' Example:
    ''' <code>
    ''' Dim sig = New SignalGenerator() _
    '''     .Add(New Linear With {.Amp = 0.1, .Scale = 10}) _
    '''     .Add(New Sine With {.Amp = 2, .Scale = 5, .Center = 1}) _
    '''     .Add(New GaussianNoise With {.Amp = 0.2})
    ''' Dim data = sig.Sample(0, 100, 1000)
    ''' </code>
    ''' </remarks>
    Public Class SignalGenerator : Inherits BasisFunction

        Public Enum Operations As Integer
            [Add] = 0
            Subtract = 1
            Multiply = 2
            Divide = 3
        End Enum

        Private terms As New List(Of (op As Operations, f As BasisFunction))

        ''' <summary>the seed of the global RNG, for reproducible noise.</summary>
        Public Shared Property Seed As Integer?
            Get
                Return _seed
            End Get
            Set(value As Integer?)
                _seed = value
                If value.HasValue Then
                    Call SetSeed(value.Value)
                End If
            End Set
        End Property

        Private Shared _seed As Integer?

        ''' <summary>
        ''' start a generator from an optional initial basis function.
        ''' </summary>
        ''' <param name="initial">the first term (defaults to a zero signal).</param>
        Public Sub New(Optional initial As BasisFunction = Nothing)
            If initial IsNot Nothing Then
                terms.Add((Operations.Add, initial))
            End If
        End Sub

        ''' <summary>add a basis function (point-wise +).</summary>
        Public Function Add(f As BasisFunction) As SignalGenerator
            terms.Add((Operations.Add, f))
            Return Me
        End Function

        ''' <summary>subtract a basis function (point-wise -).</summary>
        Public Function Subtract(f As BasisFunction) As SignalGenerator
            terms.Add((Operations.Subtract, f))
            Return Me
        End Function

        ''' <summary>multiply by a basis function (point-wise *).</summary>
        Public Function Multiply(f As BasisFunction) As SignalGenerator
            terms.Add((Operations.Multiply, f))
            Return Me
        End Function

        ''' <summary>divide by a basis function (point-wise /).</summary>
        Public Function Divide(f As BasisFunction) As SignalGenerator
            terms.Add((Operations.Divide, f))
            Return Me
        End Function

        Public Overrides Function Evaluate(x As Double) As Double
            If terms.Count = 0 Then
                Return Offset
            End If

            Dim result = terms(0).f.Evaluate(x)

            For i As Integer = 1 To terms.Count - 1
                Dim v = terms(i).f.Evaluate(x)

                Select Case terms(i).op
                    Case Operations.Add
                        result += v
                    Case Operations.Subtract
                        result -= v
                    Case Operations.Multiply
                        result *= v
                    Case Operations.Divide
                        If v <> 0 Then result /= v
                End Select
            Next

            Return result
        End Function

        Public Overrides Function Sample(x As IEnumerable(Of Double)) As Double()
            Dim xs = x.ToArray

            If terms.Count = 0 Then
                Dim zero(xs.Length - 1) As Double
                For i As Integer = 0 To xs.Length - 1
                    zero(i) = Offset
                Next
                Return zero
            End If

            Dim result = terms(0).f.Sample(xs)

            For i As Integer = 1 To terms.Count - 1
                Dim y = terms(i).f.Sample(xs)

                For j As Integer = 0 To xs.Length - 1
                    Select Case terms(i).op
                        Case Operations.Add
                            result(j) += y(j)
                        Case Operations.Subtract
                            result(j) -= y(j)
                        Case Operations.Multiply
                            result(j) *= y(j)
                        Case Operations.Divide
                            If y(j) <> 0 Then result(j) /= y(j)
                    End Select
                Next
            Next

            Return result
        End Function
    End Class
End Namespace
