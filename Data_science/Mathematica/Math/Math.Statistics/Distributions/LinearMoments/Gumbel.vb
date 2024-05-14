#Region "Microsoft.VisualBasic::62cc36d450bbdb5aa1285d295555834b, Data_science\Mathematica\Math\Math.Statistics\Distributions\LinearMoments\Gumbel.vb"

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
    '    Code Lines: 34
    ' Comment Lines: 10
    '   Blank Lines: 5
    '     File Size: 1.76 KB


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
            _Alpha = LM.L2() / stdNum.Log(2)
            _Xi = LM.L1() - 0.57721566490153287 * _Alpha
            PeriodOfRecord = (LM.SampleSize())
        End Sub
        Public Sub New(Alpha As Double, Xi As Double)
            _Alpha = Alpha
            _Xi = Xi
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Return _Xi - _Alpha * stdNum.Log(-stdNum.Log(probability))
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return stdNum.Exp(-stdNum.Exp(-(value - _Xi) / _Alpha))
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Return (1 / _Alpha) * stdNum.Exp(-(value - _Xi) / _Alpha) * stdNum.Exp(-stdNum.Exp(-(value - _Xi) / _Alpha))
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Alpha = 0 Then Yield New Exception("Alpha cannot be zero")
        End Function
    End Class

End Namespace
