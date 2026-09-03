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
            Using es As Stream = $"{outputdir}/".Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                Call New MetaData With {.title = gs.name, .keywords = {gs.id}}.GetJson(indent:=True).SaveTo($"{outputdir}/meta.json")
                Call gs.vertex _
                    .ToArray _
                    .CreateNodesMetaData({"*"}, is2Dlayout) _
                    .SaveTo($"{outputdir}/nodes.csv", silent:=True)

                Using stream As New WriteStream(Of NetworkEdge)(New StreamWriter(es))
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