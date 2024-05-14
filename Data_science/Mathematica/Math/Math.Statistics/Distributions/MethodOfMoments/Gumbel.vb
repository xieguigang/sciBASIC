#Region "Microsoft.VisualBasic::1e524d44895bd7e4d882390dff2e2092, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Gumbel.vb"

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

    '   Total Lines: 49
    '    Code Lines: 35
    ' Comment Lines: 9
    '   Blank Lines: 5
    '     File Size: 1.77 KB


    '     Class Gumbel
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
    Public Class Gumbel
        Inherits Distributions.ContinuousDistribution

        Private _Mu As Double
        Private _Beta As Double
        Public Sub New()
            _Mu = 0
            _Beta = 0
        End Sub
        Public Sub New(mu As Double, beta As Double)
            _Mu = mu
            _Beta = beta
        End Sub
        Public Sub New(data As Double())
            Dim BPM As New MomentFunctions.BasicProductMoments(data)
            _Beta = stdNum.PI / (BPM.StDev() * stdNum.Sqrt(6))
            _Mu = BPM.Mean() - _Beta * 0.57721566490153287
            PeriodOfRecord = (BPM.SampleSize())
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Return (_Mu - (_Beta * (stdNum.Log(stdNum.Log(probability)))))
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return stdNum.Exp(-stdNum.Exp(-(value - _Mu) / _Beta))
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Dim z As Double = (value - _Mu) / _Beta
            Return (1 / _Beta) * stdNum.Exp(-(z + stdNum.Exp(-z)))
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Beta <= 0 Then Yield New Exception("Beta must be greater than 0")
        End Function
    End Class

End Namespace
