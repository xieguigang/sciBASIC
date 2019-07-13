#Region "Microsoft.VisualBasic::b351871f9bb3b28d49e5cea2bb3e9fc7, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Emperical.vb"

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

    '     Class Emperical
    ' 
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
    Public Class Emperical
        Inherits Distributions.ContinuousDistribution

        Private _CumulativeProbabilities As Double()
        Private _ExceedanceValues As Double()
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Dim index As Integer = Array.BinarySearch(_CumulativeProbabilities, probability)
            'interpolate or step?
            'check for array out of bounds...
            Return _ExceedanceValues(index)
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Dim index As Integer = Array.BinarySearch(_ExceedanceValues, value)
            'interpolate or step?
            Return _CumulativeProbabilities(index)
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Throw New System.NotSupportedException("Not supported yet.") 'To change body of generated methods, choose Tools | Templates.
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _CumulativeProbabilities.Length <> _ExceedanceValues.Length Then
                Yield New Exception("Cumulative Probability values and Emperical Exceedance values are different lengths.")
            End If
        End Function

    End Class

End Namespace
