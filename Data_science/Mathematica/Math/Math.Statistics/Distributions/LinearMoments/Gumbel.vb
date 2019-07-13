#Region "Microsoft.VisualBasic::3c69aac9a604c21ed3d5aba409d43cb2, Data_science\Mathematica\Math\Math.Statistics\Distributions\LinearMoments\Gumbel.vb"

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

    '     Class Gumbel
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
    Public Class Gumbel
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
            _Alpha = LM.L2() / Math.Log(2)
            _Xi = LM.L1() - 0.57721566490153287 * _Alpha
            PeriodOfRecord = (LM.SampleSize())
        End Sub
        Public Sub New(Alpha As Double, Xi As Double)
            _Alpha = Alpha
            _Xi = Xi
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Return _Xi - _Alpha * Math.Log(-Math.Log(probability))
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return Math.Exp(-Math.Exp(-(value - _Xi) / _Alpha))
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Return (1 / _Alpha) * Math.Exp(-(value - _Xi) / _Alpha) * Math.Exp(-Math.Exp(-(value - _Xi) / _Alpha))
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Alpha = 0 Then Yield New Exception("Alpha cannot be zero")
        End Function
    End Class

End Namespace
