#Region "Microsoft.VisualBasic::9595bde639194e052d862dfe194e2033, Data_science\DataMining\DataMining\Evaluation\LabelEvaluate\RankPair.vb"

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
    '    Code Lines: 18 (36.00%)
    ' Comment Lines: 23 (46.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (18.00%)
    '     File Size: 1.69 KB


    '     Class RankPair
    ' 
    '         Properties: Label, Score
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareTo, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Evaluation

    ''' <summary>
    ''' Class encoding a member of a ranked set of labels.
    ''' </summary>
    Public Class RankPair : Implements IComparable(Of RankPair)

        ''' <summary>
        ''' The score for this pair.
        ''' </summary>
        Public ReadOnly Property Score As Double

        ''' <summary>
        ''' The Label for this pair.
        ''' </summary>
        Public ReadOnly Property Label As Double

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="score">Score for this pair</param>
        ''' <param name="label">Label associated with the given score</param>
        Public Sub New(score As Double, label As Double)
            _Score = score
            _Label = label
        End Sub

#Region "IComparable<RankPair> Members"

        ''' <summary>
        ''' Compares this pair to another.  It will end up in a sorted list in descending score order.
        ''' </summary>
        ''' <param name="other">The pair to compare to</param>
        ''' <returns>Whether this should come before or after the argument</returns>
        Public Function CompareTo(other As RankPair) As Integer Implements IComparable(Of RankPair).CompareTo
            Return other.Score.CompareTo(Score)
        End Function

#End Region

        ''' <summary>
        ''' Returns a string representation of this pair.
        ''' </summary>
        ''' <returns>A string in the for Score:Label</returns>
        Public Overrides Function ToString() As String
            Return String.Format("{0}:{1}", Score, Label)
        End Function
    End Class

End Namespace
