#Region "Microsoft.VisualBasic::e9a472f70c4b67f180ff41a173669a91, gr\network-visualization\Datavisualization.Network\test\LouvainTest.vb"

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

    ' Module LouvainTest
    ' 
    '     Sub: Main, RunAnalysis
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON

Module LouvainTest

    Sub Main()

        Dim links As String()() = "E:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\Louvain\facebook_combined.txt".ReadAllLines.Skip(1).Select(Function(str) Strings.Trim(str).StringSplit("\s+")).ToArray
        Dim g As New NetworkGraph

        ' build network via links
        For Each line As String() In links
            If g.GetElementByID(line(0)) Is Nothing Then
                g.CreateNode(line(0))
            End If
            If g.GetElementByID(line(1)) Is Nothing Then
                g.CreateNode(line(1))
            End If

            g.CreateEdge(g.GetElementByID(line(0)), g.GetElementByID(line(1)), 1)
        Next

        ' the original network with communities labeled
        Dim clusters As NetworkGraph = Communities.Analysis(g)

        Call Console.WriteLine(Communities.Modularity(clusters))
        Call Console.WriteLine(Communities.Community(g).GetJson(indent:=True))

        Call clusters _
            .Tabular _
            .Save("E:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\Louvain\facebook_combined_graph")

        Pause()
    End Sub

    Sub RunAnalysis(filepath As String)

    End Sub

End Module

