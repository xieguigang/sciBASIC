#Region "Microsoft.VisualBasic::1ac788b8a6cb1a75187239dc2d56161d, Data_science\Graph\KNN\ScoreMetric.vb"

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

    '   Total Lines: 21
    '    Code Lines: 9 (42.86%)
    ' Comment Lines: 7 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (23.81%)
    '     File Size: 620 B


    '     Class ScoreMetric
    ' 
    '         Properties: cutoff
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace KNearNeighbors

    Public MustInherit Class ScoreMetric

        Public Property cutoff As Double

        ''' <summary>
        ''' the score function should produce a positive score value,
        ''' higher score value is better
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public MustOverride Function eval(x As Double(), y As Double()) As Double

        Public Overrides Function ToString() As String
            Return "knn_score_metric();"
        End Function

    End Class
End Namespace
