#Region "Microsoft.VisualBasic::06a093e1b402d57e6b9d1dc365f80d7a, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Exponential.vb"

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
Namespace Distributions.MethodOfMoments


    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class Exponential
        Inherits Distributions.ContinuousDistribution

        Private _Lambda As Double
        Public Sub New()
            'for reflection
            _Lambda = 1
        End Sub
        Public Sub New(lambda As Double)
            _Lambda = lambda
        End Sub
        Public Sub New(data As Double())
            Dim BPM As New MomentFunctions.BasicProductMoments(data)
            _Lambda = 1 / BPM.Mean()
            PeriodOfRecord = (BPM.SampleSize())
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Return Math.Log(probability) / _Lambda
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return 1 - Math.Exp(-_Lambda * value)
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            If value < 0 Then
                Return 0
            Else
                Return _Lambda * Math.Exp(-_Lambda * value)
            End If
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Lambda <= 0 Then Yield New Exception("Lambda must be greater than 0")
        End Function

    End Class

End Namespace
