#Region "Microsoft.VisualBasic::fcd13a6ea45f1437f07f1c6c796a7ca5, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Uniform.vb"

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

    '     Class Uniform
    ' 
    '         Properties: Max, Min
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetCDF, GetInvCDF, GetPDF, Validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace Distributions.MethodOfMoments

    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class Uniform : Inherits Distributions.ContinuousDistribution

        Public ReadOnly Property Min As Double
        Public ReadOnly Property Max As Double

        Public Sub New()
            'for reflection
            Min = 0
            Max = 0
        End Sub

        Public Sub New(min As Double, max As Double)
            min = min
            max = max
        End Sub

        Public Sub New(data As Double())
            Dim BPM As New MomentFunctions.BasicProductMoments(data)
            _Min = BPM.Min()
            _Max = BPM.Max()
            PeriodOfRecord = (BPM.SampleSize())
            'alternative method
            'double dist = java.lang.Math.sqrt(3*BPM.GetSampleVariance());
            '_Min = BPM.GetMean() - dist;
            '_Max = BPM.GetMean() + dist;
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Return _Min + ((_Max - _Min) * probability)
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            If value < _Min Then
                Return 0
            ElseIf value <= _Max Then
                Return (value - _Min) / (_Min - _Max)
            Else
                Return 1
            End If
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            If value < _Min Then
                Return 0
            ElseIf value <= _Max Then
                Return 1 / (_Max - _Min)
            Else
                Return 0
            End If
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Min > _Max Then
                Yield New Exception("The min cannot be greater than the max in the uniform distribuiton.")
            End If
        End Function
    End Class

End Namespace
