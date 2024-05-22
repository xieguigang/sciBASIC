#Region "Microsoft.VisualBasic::1076376797fd86ea5ad40d3e609589bf, gr\network-visualization\Datavisualization.Network\test\LouvainTest.vb"

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

    '   Total Lines: 73
    '    Code Lines: 47 (64.38%)
    ' Comment Lines: 3 (4.11%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 23 (31.51%)
    '     File Size: 2.31 KB


    ' Module LouvainTest
    ' 
    '     Function: loadModel
    ' 
    '     Sub: analysis2, Main, RunAnalysis
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON

Module LouvainTest

    Const source As String = "D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\Louvain\testdata.txt"

    Sub analysis2()

        VBDebugger.Mute = True

        Dim g = loadModel()

        VBDebugger.Mute = False

        ' the original network with communities labeled
        Dim clusters As NetworkGraph = Communities.AnalysisUnweighted(g)

        Call Console.WriteLine(Communities.Modularity(clusters))
        Call Console.WriteLine(Communities.Community(g).GetJson(indent:=True))

        Call clusters _
            .Tabular _
            .Save("D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\UnweightedFastUnfolding\")

        Pause()
    End Sub

    Private Function loadModel() As NetworkGraph
        Dim links As String()() = source.ReadAllLines.Skip(1).Select(Function(str) Strings.Trim(str).StringSplit("\s+")).ToArray
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

        Return g
    End Function

    Sub Main()

        Call analysis2()

        Dim g = loadModel()
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
