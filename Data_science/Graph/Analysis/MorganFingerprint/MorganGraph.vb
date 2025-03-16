#Region "Microsoft.VisualBasic::c1e47242e55cd2f4c3ca6bdd39420705, Data_science\Graph\Analysis\MorganFingerprint\MorganGraph.vb"

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

    '   Total Lines: 20
    '    Code Lines: 7 (35.00%)
    ' Comment Lines: 8 (40.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (25.00%)
    '     File Size: 544 B


    '     Interface MorganGraph
    ' 
    '         Properties: Atoms, Graph
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.Data.GraphTheory.Network

Namespace Analysis.MorganFingerprint

    Public Interface MorganGraph(Of V As IMorganAtom, E As IndexEdge)

        ''' <summary>
        ''' vertex nodes
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Atoms As V()
        ''' <summary>
        ''' the graph structure for make morgan fingerprint embedding
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Graph As E()

    End Interface
End Namespace
