#Region "Microsoft.VisualBasic::2733c468a8c1386c08e71f6e98dde6b4, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Gamma.vb"

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

    '     Class Gamma
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
Namespace Distributions.MethodOfMoments


    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class Gamma
        Inherits Distributions.ContinuousDistribution

        Private _Alpha As Double
        Private _Beta As Double
        Public Sub New()
            'for reflection
            _Alpha = 0
            _Beta = 0
        End Sub
        Public Sub New(data As Double())
            'http://www.itl.nist.gov/div898/handbook/eda/section3/eda366b.htm
            Dim BPM As New MomentFunctions.BasicProductMoments(data)
            _Alpha = Math.Pow((BPM.Mean() / BPM.StDev()), 2)
            _Beta = 1 / (BPM.StDev() / BPM.Mean())
            PeriodOfRecord = (BPM.SampleSize())
        End Sub
        Public Sub New(Alpha As Double, Beta As Double)
            _Alpha = Alpha
            _Beta = Beta
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Dim xn As Double = _Alpha / _Beta
            Dim testvalue As Double = GetCDF(xn)
            Dim i As Integer = 0
            Do
                xn = xn - ((testvalue - probability) / GetPDF(xn))
                testvalue = GetCDF(xn)
                i += 1
            Loop While Math.Abs(testvalue - probability) <= 0.00000000000001 Or i = 100
            Return xn
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return SpecialFunctions.IncompleteGamma(_Alpha, _Beta * value) / Math.Exp(SpecialFunctions.gammaln(_Alpha))
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Return (((Math.Pow(_Beta, _Alpha)) * ((Math.Pow(value, _Alpha - 1)) * Math.Exp(-_Beta * value)) / Math.Exp(SpecialFunctions.gammaln(_Alpha))))
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Beta <= 0 Then Yield New Exception("Beta must be greater than 0")
        End Function
    End Class

End Namespace
