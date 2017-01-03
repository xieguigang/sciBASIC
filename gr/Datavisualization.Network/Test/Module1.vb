#Region "Microsoft.VisualBasic::aba68e112e7c5c2aa65f0f11046f2e9b, ..\sciBASIC#\gr\Datavisualization.Network\Test\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()
        Dim g As New Network

        Call g.AddEdges("B", {"C"})
        Call g.AddEdges("C", {"B"})
        Call g.AddEdges("D", {"A", "B"})
        Call g.AddEdges("E", {"D", "B", "F"})
        Call g.AddEdges("F", {"E", "B"})
        Call g.AddEdges("G", {"E", "B"})
        Call g.AddEdges("H", {"E", "B"})
        Call g.AddEdges("I", {"E", "B"})
        Call g.AddEdges("J", {"E"})
        Call g.AddEdges("K", {"E"})

        Dim matrix As New GraphMatrix(g)
        Dim pr As New PageRank(matrix)

        Dim result = matrix.TranslateVector(pr.ComputePageRank)

        Call result.GetJson(True).EchoLine

        Pause()
    End Sub
End Module

