#Region "Microsoft.VisualBasic::029b5b10261d6d2d4053ef0c25936d06, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Triangular.vb"

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

    '   Total Lines: 103
    '    Code Lines: 81 (78.64%)
    ' Comment Lines: 16 (15.53%)
    '    - Xml Docs: 18.75%
    ' 
    '   Blank Lines: 6 (5.83%)
    '     File Size: 4.23 KB


    '     Class Triangular
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetCDF, GetInvCDF, GetMax, GetMin, GetMostLikely
    '                   GetPDF, Validate
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
    Public Class Triangular
        Inherits Distributions.ContinuousDistribution

        Private _Min As Double
        Private _Max As Double
        Private _MostLikely As Double
        Public Overridable Function GetMin() As Double
            Return _Min
        End Function
        Public Overridable Function GetMax() As Double
            Return _Max
        End Function
        Public Overridable Function GetMostLikely() As Double
            Return _MostLikely
        End Function
        Public Sub New()
            '_SampleSize = 0; for reflection
            _Min = 0
            _Max = 0
            _MostLikely = 0
        End Sub
        Public Sub New(min As Double, max As Double, mostlikely As Double)
            '_SampleSize = 0;
            _Min = min
            _Max = max
            _MostLikely = mostlikely
        End Sub
        Public Sub New(data As Double())
            Dim PM As New MomentFunctions.ProductMoments(data)
            'Simple Method:
            '        _Min = PM.GetMin();
            '        _Max = PM.GetMax();
            '        _MostLikely = PM.GetMean();

            'Alternate Method from Ben Chaon
            Dim sqrt2 As Double = stdNum.Sqrt(2)
            Dim sqrt3 As Double = stdNum.Sqrt(3)
            Dim a3 As Double = PM.skewness()
            Dim b3 As Double
            Dim angle As Double
            Dim aa As Double
            Dim bb As Double
            If 8 - a3 * a3 < 0 Then
                a3 = stdNum.Sin(a3) * 2 * sqrt2
                b3 = 0
            Else
                b3 = stdNum.Sqrt(8 - a3 * a3)
            End If
            angle = stdNum.Atan2(b3, a3)
            aa = stdNum.Cos(angle / 3.0)
            bb = stdNum.Sin(angle / 3.0)
            _Min = (PM.Mean() + sqrt2 * PM.StandardDeviation() * (aa - sqrt3 * bb))
            _MostLikely = (PM.Mean() - 2 * sqrt2 * PM.StandardDeviation() * aa)
            _Max = (PM.Mean() + sqrt2 * PM.StandardDeviation() * (aa + sqrt3 * bb))
            PeriodOfRecord = (PM.SampleSize())
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Dim a As Double = _MostLikely - _Min
            Dim b As Double = _Max - _MostLikely
            If probability <= 0 Then
                Return _Min
            ElseIf probability < (a / (_Max - _Min)) Then
                Return _Min + stdNum.Sqrt(probability * (_Max - _Min) * a)
            ElseIf probability < 1 Then
                Return _Max - stdNum.Sqrt((1 - probability) * (_Max - _Min) * b)
            Else
                Return _Max
            End If
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            If value < _Min Then Return 0
            If value < _MostLikely Then Return (stdNum.Pow((value - _Min), 2) / (_Max - _Min) * (_MostLikely - _Min))
            If value <= _Max Then Return 1 - (stdNum.Pow((_Max - value), 2) / (_Max - _Min) * (_Max - _MostLikely))
            Return 1
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            If value < _Min Then Return 0
            If value < _MostLikely Then Return (2 * (value - _Min) / (_Max - _Min) * (_MostLikely - _Min))
            If value <= _Max Then Return (2 * (_Max - value) / (_Max - _Min) * (_Max - _MostLikely))
            Return 0
        End Function

        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Min > _Max Then Yield New Exception("Min cannot be greater than Max")
            If _Min > _MostLikely Then Yield New Exception("Min cannot be greater than MostLikely")
            If _MostLikely > _Max Then Yield New Exception("MostLikely cannot be greater than Max")
        End Function
    End Class

End Namespace
