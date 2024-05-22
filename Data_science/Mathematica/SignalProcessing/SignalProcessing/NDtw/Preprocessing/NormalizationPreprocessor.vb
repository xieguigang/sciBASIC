#Region "Microsoft.VisualBasic::49d66cf073edf9990ac70a60e61494af, Data_science\Mathematica\SignalProcessing\SignalProcessing\NDtw\Preprocessing\NormalizationPreprocessor.vb"

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

    '   Total Lines: 37
    '    Code Lines: 22 (59.46%)
    ' Comment Lines: 7 (18.92%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 8 (21.62%)
    '     File Size: 1.26 KB


    '     Class NormalizationPreprocessor
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Preprocess, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace NDtw.Preprocessing

    Public Class NormalizationPreprocessor : Inherits IPreprocessor

        Private ReadOnly _minBoundary As Double
        Private ReadOnly _maxBoundary As Double

        ''' <summary>
        ''' Initialize to use normalization to range [0, 1]
        ''' </summary>
        Public Sub New()
            Me.New(0, 1)
        End Sub

        ''' <summary>
        ''' Initialize to use normalization to range [minBoundary, maxBoundary]
        ''' </summary>
        Public Sub New(minBoundary As Double, maxBoundary As Double)
            _minBoundary = minBoundary
            _maxBoundary = maxBoundary
        End Sub

        Public Overrides Function Preprocess(data As Double()) As Double()
            ' x = ((x - min_x) / (max_x - min_x)) * (maxBoundary - minBoundary) + minBoundary

            Dim min = data.Min()
            Dim max = data.Max()
            Dim constFactor = (_maxBoundary - _minBoundary) / (max - min)

            Return data.[Select](Function(x) (x - min) * constFactor + _minBoundary).ToArray()
        End Function

        Public Overrides Function ToString() As String
            Return "Normalization"
        End Function
    End Class
End Namespace
