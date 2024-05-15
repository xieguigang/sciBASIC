#Region "Microsoft.VisualBasic::a7259a7b2c71d301c2ac968176e4962b, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Exponential.vb"

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

    '   Total Lines: 50
    '    Code Lines: 34
    ' Comment Lines: 10
    '   Blank Lines: 6
    '     File Size: 1.64 KB


    '     Class Exponential
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetCDF, GetInvCDF, GetPDF, Validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

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
            Return stdNum.Log(probability) / _Lambda
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return 1 - stdNum.Exp(-_Lambda * value)
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            If value < 0 Then
                Return 0
            Else
                Return _Lambda * stdNum.Exp(-_Lambda * value)
            End If
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Lambda <= 0 Then Yield New Exception("Lambda must be greater than 0")
        End Function

    End Class

End Namespace
