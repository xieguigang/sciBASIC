#Region "Microsoft.VisualBasic::fcd13a6ea45f1437f07f1c6c796a7ca5, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Uniform.vb"

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

    '   Total Lines: 67
    '    Code Lines: 47
    ' Comment Lines: 13
    '   Blank Lines: 7
    '     File Size: 2.24 KB


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
