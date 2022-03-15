#Region "Microsoft.VisualBasic::fea48b0200e4934c1ed5e96010a98c43, sciBASIC#\Data\OCR\GlobalMatch.vb"

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

    '   Total Lines: 26
    '    Code Lines: 23
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 870.00 B


    ' Module GlobalMatch
    ' 
    '     Function: Similarity
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Levenshtein
Imports Microsoft.VisualBasic.GenericLambda(Of Double)

Public Module GlobalMatch

    Public Function Similarity(a As Vector, b As Vector, threshold#) As Double
        Dim equals As IEquals = Function(x, y)
                                    x = threshold <= x
                                    y = threshold <= y
                                    Return x = y
                                End Function
        Dim dist = LevenshteinDistance.ComputeDistance(
            a.ToArray,
            b.ToArray,
            equals,
            Function(x) If(x <= threshold, "0"c, "1"c)
        )

        If dist Is Nothing Then
            Return 0
        Else
            Return dist.MatchSimilarity
        End If
    End Function
End Module
