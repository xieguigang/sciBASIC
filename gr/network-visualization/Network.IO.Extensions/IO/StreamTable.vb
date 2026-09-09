#Region "Microsoft.VisualBasic::f77ee549aebfa14e625827577b4a5a9e, gr\network-visualization\Network.IO.Extensions\IO\StreamTable.vb"

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

    '   Total Lines: 39
    '    Code Lines: 33 (84.62%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (15.38%)
    '     File Size: 1.73 KB


    '     Module StreamTable
    ' 
    '         Function: SaveStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.Framework.IO.Linq
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FileStream

    Public Module StreamTable

        <Extension>
        Public Function SaveStream(gs As NetworkGraphStream, outputdir As String, Optional is2Dlayout As Boolean = True) As Boolean
            Using es As Stream = $"{outputdir}/network-edges.csv".Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Call New MetaData With {.title = gs.name, .keywords = {gs.id}}.GetJson(indent:=True).SaveTo($"{outputdir}/meta.json")
                Call gs.vertex _
                    .ToArray _
                    .CreateNodesMetaData({"*"}, is2Dlayout) _
                    .SaveTo($"{outputdir}/nodes.csv", silent:=True)

                Using stream As New WriteStream(Of NetworkEdge)(New StreamWriter(es))
                    Call stream.CacheMetaIndex({})

                    For Each edge As Edge In gs.graphEdges
                        Call stream.Flush(New NetworkEdge With {
                            .fromNode = edge.U.label,
                            .toNode = edge.V.label,
                            .interaction = edge(NamesOf.REFLECTION_ID_MAPPING_INTERACTION_TYPE),
                            .value = edge.weight
                        })
                    Next
                End Using
            End Using

            Return True
        End Function
    End Module
End Namespace
