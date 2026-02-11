#Region "Microsoft.VisualBasic::49f0f4887b5eebffbc531ec2865dec8f, Data_science\DataMining\DBNCode\dbn\ScoringFunction.vb"

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

    '   Total Lines: 36
    '    Code Lines: 10 (27.78%)
    ' Comment Lines: 9 (25.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (47.22%)
    '     File Size: 1.41 KB


    '     Interface ScoringFunction
    ' 
    '         Function: (+4 Overloads) evaluate, (+2 Overloads) evaluate_2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace dbn

    Public Interface ScoringFunction

        Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), childNode As Integer) As Double

        Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double


        Function evaluate_2(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double




        ''' <summary>
        ''' Calculate score when process is stationary.
        ''' </summary>
        Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double


        ''' <summary>
        ''' Calculate score when process is stationary.
        ''' </summary>
        Function evaluate_2(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double




        ''' <summary>
        ''' Calculate score when process is stationary.
        ''' </summary>
        Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), childNode As Integer) As Double

    End Interface

End Namespace

