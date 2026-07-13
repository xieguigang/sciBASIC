#Region "Microsoft.VisualBasic::9098f5b95050811d3ad9fb7518a9e878, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\BasisFunction.vb"

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

    '   Total Lines: 134
    '    Code Lines: 48 (35.82%)
    ' Comment Lines: 73 (54.48%)
    '    - Xml Docs: 97.26%
    ' 
    '   Blank Lines: 13 (9.70%)
    '     File Size: 5.90 KB


    '     Class BasisFunction
    ' 
    '         Properties: Amp, Center, Offset, Scale
    ' 
    '         Function: CopyParametersTo, Evaluate, (+2 Overloads) Sample, Shape, ToGeneralSignal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.SignalProcessing

Namespace Source.Generators

    ''' <summary>
    ''' The abstract base class for all deterministic basis signal functions.
    ''' </summary>
    ''' <remarks>
    ''' Every basis function is modeled as a function of a single time variable
    ''' <c>x</c> with four universal parameters:
    ''' 
    ''' <list type="bullet">
    ''' <item><description><see cref="Amp"/> — amplitude / scaling factor</description></item>
    ''' <item><description><see cref="Center"/> — center / horizontal shift (x0)</description></item>
    ''' <item><description><see cref="Scale"/> — width / frequency / steepness factor</description></item>
    ''' <item><description><see cref="Offset"/> — vertical offset (baseline)</description></item>
    ''' </list>
    ''' 
    ''' The default evaluation is <c>Offset + Amp * Shape((x - Center) / Scale)</c>.
    ''' A concrete basis function only needs to override <see cref="Shape"/>, while
    ''' combinators (sum, product, convolution, ...) override <see cref="Evaluate"/>
    ''' instead. This makes both primitive functions and composite generators
    ''' interchangeable as building blocks.
    ''' </remarks>
    Public MustInherit Class BasisFunction

        ''' <summary>
        ''' amplitude / scaling factor. Default 1.
        ''' </summary>
        Public Property Amp As Double = 1.0
        ''' <summary>
        ''' center / horizontal shift (x0). Controls when the feature occurs.
        ''' </summary>
        Public Property Center As Double = 0.0
        ''' <summary>
        ''' width / frequency / steepness factor. Interpreted by each subclass.
        ''' </summary>
        Public Property Scale As Double = 1.0
        ''' <summary>
        ''' vertical offset (baseline). Default 0.
        ''' </summary>
        Public Property Offset As Double = 0.0

        ''' <summary>
        ''' the normalized shape of the basis function, evaluated at the
        ''' shifted and scaled coordinate <paramref name="u"/> which equals
        ''' <c>(x - Center) / Scale</c> at call time.
        ''' </summary>
        ''' <param name="u">the normalized coordinate</param>
        ''' <returns>the shape value (before amp / offset applied)</returns>
        Protected Overridable Function Shape(u As Double) As Double
            Return u
        End Function

        ''' <summary>
        ''' evaluate the basis function at a given time point <paramref name="x"/>.
        ''' Combinators override this to combine their operands.
        ''' </summary>
        ''' <param name="x">the time coordinate</param>
        ''' <returns>the signal value</returns>
        Public Overridable Function Evaluate(x As Double) As Double
            Return Offset + Amp * Shape((x - Center) / Scale)
        End Function

        ''' <summary>
        ''' sample the signal over a given time grid. The default implementation
        ''' simply evaluates each point; combinators such as convolution may
        ''' override this to perform whole-signal operations.
        ''' </summary>
        ''' <param name="x">the time grid to sample on</param>
        ''' <returns>the sampled signal values</returns>
        Public Overridable Function Sample(x As IEnumerable(Of Double)) As Double()
            Return x.Select(AddressOf Evaluate).ToArray
        End Function

        ''' <summary>
        ''' sample the signal on an evenly spaced time grid.
        ''' </summary>
        ''' <param name="min">the start time</param>
        ''' <param name="max">the end time</param>
        ''' <param name="n">the number of sample points (must be &gt; 0)</param>
        ''' <returns>the sampled signal values</returns>
        Public Function Sample(min As Double, max As Double, n As Integer) As Double()
            If n <= 0 Then
                Throw New ArgumentException("n must be positive", NameOf(n))
            End If

            Dim xs(n - 1) As Double
            Dim stepX = (max - min) / (n - 1)

            For i As Integer = 0 To n - 1
                xs(i) = min + i * stepX
            Next

            Return Sample(xs)
        End Function

        ''' <summary>
        ''' sample the signal and wrap it into a <see cref="GeneralSignal"/>
        ''' so that it can be consumed by the downstream processing pipeline.
        ''' </summary>
        ''' <param name="x">the time grid</param>
        ''' <param name="reference">the signal id / variable name</param>
        ''' <param name="unit">the measure unit of the x axis</param>
        ''' <returns>the sampled signal as a <see cref="GeneralSignal"/></returns>
        Public Function ToGeneralSignal(x As IEnumerable(Of Double),
                                        Optional reference As String = "",
                                        Optional unit As String = "") As GeneralSignal
            Dim xs = x.ToArray
            Dim ys = Sample(xs)

            Return New GeneralSignal With {
                .reference = reference,
                .measureUnit = unit,
                .Measures = xs,
                .Strength = ys
            }
        End Function

        ''' <summary>
        ''' create a deep-ish copy of the universal parameters onto another
        ''' basis function (useful when building a generator programmatically).
        ''' </summary>
        ''' <param name="other">the target basis function</param>
        ''' <returns>the target, for chaining</returns>
        Public Function CopyParametersTo(other As BasisFunction) As BasisFunction
            other.Amp = Amp
            other.Center = Center
            other.Scale = Scale
            other.Offset = Offset
            Return other
        End Function
    End Class
End Namespace
