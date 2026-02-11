#Region "Microsoft.VisualBasic::0c75a8440bf9939e91341d0e393e6213, Data_science\DataMining\DBNCode\dbn\LLScoringFunction.vb"

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

    '   Total Lines: 76
    '    Code Lines: 49 (64.47%)
    ' Comment Lines: 5 (6.58%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 22 (28.95%)
    '     File Size: 3.97 KB


    '     Class LLScoringFunction
    ' 
    '         Function: (+4 Overloads) evaluate, (+2 Overloads) evaluate_2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace dbn

    Public Class LLScoringFunction : Implements ScoringFunction

        Public Overridable Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, transition, parentNodesPast, Nothing, childNode)
        End Function

        Public Overridable Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double Implements ScoringFunction.evaluate

            Dim c As LocalConfiguration = New LocalConfiguration(observations.Attributes, observations.MarkovLag, parentNodesPast, parentNodePresent, childNode)

            Dim score As Double = 0

            Do

                c.ConsiderChild = False
                Dim Nij = observations.count(c, transition)
                '			System.out.println("Node: " + childNode + " Parents" + parentNodesPast.toString() + " ParentPresent: " +  parentNodePresent + " NIJ: " + Nij);
                c.ConsiderChild = True

                Do

                    Dim Nijk = observations.count(c, transition)
                    '				System.out.println("Configuration: " + c + " NIJK: " + Nijk);
                    If CLng(std.Round(Nijk * 1000.0R, MidpointRounding.AwayFromZero)) / 1000.0R <> 0 AndAlso Nijk <> Nij Then
                        score += Nijk * (std.Log(Nijk) - std.Log(Nij))
                        '					if(Double.isNaN(score)) {
                        '						System.out.println("nijk: " + Nijk + " nij:" + Nij);
                        '					}

                    End If
                Loop While c.nextChild()
            Loop While c.nextParents()

            Return score
        End Function

        Public Overridable Function evaluate_2(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate_2

            Dim c As LocalConfiguration = New LocalConfiguration(observations.Attributes, observations.MarkovLag, parentNodesPast, parentNodePresent, childNode)

            Dim score As Double = 0

            Do
                c.ConsiderChild = False
                Dim Nij = observations.count(c, transition)
                c.ConsiderChild = True
                Do
                    Dim Nijk = observations.count(c, transition)
                    If CLng(std.Round(Nijk * 1000.0R, MidpointRounding.AwayFromZero)) / 1000.0R <> 0 AndAlso Nijk <> Nij Then
                        score += Nijk * (std.Log(Nijk) - std.Log(Nij))
                    End If
                Loop While c.nextChild()
            Loop While c.nextParents()

            Return score
        End Function

        Public Overridable Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, parentNodesPast, Nothing, childNode)
        End Function

        Public Overridable Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, -1, parentNodesPast, parentNodePresent, childNode)
        End Function

        Public Overridable Function evaluate_2(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate_2
            Return evaluate_2(observations, -1, parentNodesPast, parentNodePresent, childNode)
        End Function

    End Class

End Namespace

