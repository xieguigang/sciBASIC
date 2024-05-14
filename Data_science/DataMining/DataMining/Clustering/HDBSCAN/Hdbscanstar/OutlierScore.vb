#Region "Microsoft.VisualBasic::7ab89772408bb09e526ac036fd63f8d5, Data_science\DataMining\DataMining\Clustering\HDBSCAN\Hdbscanstar\OutlierScore.vb"

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

    '   Total Lines: 41
    '    Code Lines: 21
    ' Comment Lines: 11
    '   Blank Lines: 9
    '     File Size: 1.55 KB


    '     Class OutlierScore
    ' 
    '         Properties: Id, Score
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareTo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace HDBSCAN.Hdbscanstar
    ''' <summary>
    ''' Simple storage class that keeps the outlier score, core distance, and id (index) for a single point.
    ''' OutlierScores are sorted in ascending order by outlier score, with core distances used to break
    ''' outlier score ties, and ids used to break core distance ties.
    ''' </summary>
    Public Class OutlierScore
        Implements IComparable(Of OutlierScore)

        Private ReadOnly _coreDistance As Double

        Public Property Score As Double
        Public Property Id As Integer

        ''' <summary>
        ''' Creates a new OutlierScore for a given point.
        ''' </summary>
        ''' <param name="score">The outlier score of the point</param>
        ''' <param name="coreDistance">The point's core distance</param>
        ''' <param name="id">The id (index) of the point</param>
        Public Sub New(score As Double, coreDistance As Double, id As Integer)
            Me.Score = score
            _coreDistance = coreDistance
            Me.Id = id
        End Sub

        Public Function CompareTo(other As OutlierScore) As Integer Implements IComparable(Of OutlierScore).CompareTo
            If Score > other.Score Then Return 1

            If Score < other.Score Then Return -1

            If _coreDistance > other._coreDistance Then Return 1

            If _coreDistance < other._coreDistance Then Return -1

            Return Id - other.Id
        End Function
    End Class
End Namespace
