#Region "Microsoft.VisualBasic::bc4ffc18f140dd411ea0bfadff59444a, Data_science\Mathematica\SignalProcessing\SignalProcessing\NDtw\Preprocessing\StandardizationPreprocessor.vb"

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

    '   Total Lines: 27
    '    Code Lines: 14 (51.85%)
    ' Comment Lines: 7 (25.93%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 6 (22.22%)
    '     File Size: 1.09 KB


    '     Class StandardizationPreprocessor
    ' 
    '         Function: Preprocess, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace NDtw.Preprocessing

    ''' <summary>
    ''' f(x) = (x - mean) / std dev
    ''' </summary>
    Public Class StandardizationPreprocessor : Inherits IPreprocessor

        Public Overrides Function Preprocess(data As Double()) As Double()
            'http://stats.stackexchange.com/questions/1944/what-is-the-name-of-this-normalization
            'http://stats.stackexchange.com/questions/13412/what-are-the-primary-differences-between-z-scores-and-t-scores-and-are-they-bot
            'http://mathworld.wolfram.com/StandardDeviation.html

            ' x = (x - mean) / std dev
            Dim mean = data.Average()
            Dim N As Integer = (data.Length - 1)
            Dim stdDev = std.Sqrt(SIMD.Subtract.f64_op_subtract_f64_scalar(data, mean).Sum(Function(x) x ^ 2) / N)

            Return data.[Select](Function(x) (x - mean) / stdDev).ToArray()
        End Function

        Public Overrides Function ToString() As String
            Return "Standardization"
        End Function
    End Class
End Namespace
