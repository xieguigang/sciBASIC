#Region "Microsoft.VisualBasic::9a4edd4868b5ae101741e523d2a2b21e, Data_science\Graph\test\ullmannTest.vb"

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

    '   Total Lines: 24
    '    Code Lines: 19 (79.17%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (20.83%)
    '     File Size: 805 B


    ' Module ullmannTest
    ' 
    '     Sub: identicalTest, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Module ullmannTest

    Sub Main()
        Call identicalTest()
        Call Pause()
    End Sub

    Sub identicalTest()
        Dim a As New NetworkGraph From {("A", "B"), ("B", "C"), ("C", "A"), ("A", "E")}
        Dim b As New NetworkGraph From {("A1", "B1"), ("B1", "C1"), ("C1", "A1"), ("A1", "E1")}
        Dim ta As String() = Nothing
        Dim tb As String() = Nothing
        Dim iso As New Ullmann(a.CreateEdgeMatrix(ta), b.CreateEdgeMatrix(tb))

        For Each map In Ullmann.ExplainNodeMapping(iso.FindIsomorphisms, ta, tb)
            Call Console.WriteLine(map)
        Next
    End Sub

End Module

