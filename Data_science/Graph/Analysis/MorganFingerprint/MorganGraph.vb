
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