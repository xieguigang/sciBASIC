#Region "Microsoft.VisualBasic::39013ea2985734bb926bee6cfb4047f9, Data_science\DataMining\DBNCode\dbn\RandomScoringFunction.vb"

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

    '   Total Lines: 38
    '    Code Lines: 27 (71.05%)
    ' Comment Lines: 1 (2.63%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (26.32%)
    '     File Size: 2.23 KB


    '     Class RandomScoringFunction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+4 Overloads) evaluate, (+2 Overloads) evaluate_2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace dbn

    Public Class RandomScoringFunction
        Implements ScoringFunction

        Public Sub New()
            ' TODO Auto-generated constructor stub
        End Sub

        Public Overridable Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, transition, parentNodesPast, Nothing, childNode)
        End Function

        Public Overridable Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double Implements ScoringFunction.evaluate
            Dim r As Random = New Random()
            Return -100 + (0 + 100) * r.NextDouble()
        End Function

        Public Overridable Function evaluate_2(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate_2
            Dim r As Random = New Random()
            Return -100 + (0 + 100) * r.NextDouble()
        End Function

        Public Overridable Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, -1, parentNodesPast, Nothing, childNode)
        End Function

        Public Overridable Function evaluate_2(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate_2
            Return evaluate_2(observations, -1, parentNodesPast, parentNodePresent, childNode)
        End Function

        Public Overridable Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, parentNodesPast, Nothing, childNode)
        End Function

    End Class

End Namespace

