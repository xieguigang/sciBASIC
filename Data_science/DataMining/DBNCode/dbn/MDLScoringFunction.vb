#Region "Microsoft.VisualBasic::111dca337d6bfc06ba83364679db88a2, Data_science\DataMining\DBNCode\dbn\MDLScoringFunction.vb"

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

    '   Total Lines: 34
    '    Code Lines: 18 (52.94%)
    ' Comment Lines: 2 (5.88%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (41.18%)
    '     File Size: 1.55 KB


    '     Class MDLScoringFunction
    ' 
    '         Function: evaluate, evaluate_2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace dbn

    Public Class MDLScoringFunction : Inherits LLScoringFunction

        Private epsilon As Double = 0.0000000001

        Public Overrides Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double

            Dim c As LocalConfiguration = New LocalConfiguration(observations.Attributes, observations.MarkovLag, parentNodesPast, parentNodePresent, childNode)

            Dim score = MyBase.evaluate(observations, transition, parentNodesPast, parentNodePresent, childNode)

            ' regularizer term
            score -= 0.5 * std.Log(observations.numObservations(transition) + epsilon) * c.NumParameters
            Return score
        End Function

        Public Overrides Function evaluate_2(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double

            Dim c As LocalConfiguration = New LocalConfiguration(observations.Attributes, observations.MarkovLag, parentNodesPast, parentNodePresent, childNode)

            Dim score = MyBase.evaluate_2(observations, transition, parentNodesPast, parentNodePresent, childNode)

            ' regularizer term
            score -= 0.5 * std.Log(observations.numObservations(transition) + epsilon) * c.NumParameters

            Return score
        End Function

    End Class

End Namespace

