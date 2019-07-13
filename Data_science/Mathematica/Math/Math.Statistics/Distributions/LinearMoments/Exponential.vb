#Region "Microsoft.VisualBasic::3b1d81fe8afb62c2ebf6fd7a7f653a12, Data_science\Mathematica\Math\Math.Statistics\Distributions\LinearMoments\Exponential.vb"

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

    '     Class Exponential
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetCDF, GetInvCDF, GetPDF, Validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace Distributions.LinearMoments


    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class Exponential
        Inherits Distributions.ContinuousDistribution

        Private _Alpha As Double
        Private _Xi As Double
        Public Sub New()
            'for reflection
            _Alpha = 0
            _Xi = 0
        End Sub
        Public Sub New(data As Double())
            Dim LM As New MomentFunctions.LinearMoments(data)
            _Alpha = 2 * LM.L2()
            _Xi = LM.L1() - _Alpha
            PeriodOfRecord = (LM.SampleSize())
        End Sub
        Public Sub New(Alpha As Double, Xi As Double)
            _Alpha = Alpha
            _Xi = Xi
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Return _Xi - _Alpha * Math.Log(1 - probability)
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return 1 - Math.Exp(-(value - _Xi) / _Alpha)
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Return (1 / _Alpha) * Math.Exp(-(value - _Xi) / _Alpha)
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Alpha = 0 Then Yield New Exception("Alpha cannot be zero")
        End Function
    End Class

End Namespace
