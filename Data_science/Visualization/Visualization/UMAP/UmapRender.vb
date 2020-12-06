Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module UmapRender

    <Extension>
    Public Function DrawUmap2D(umap As Umap, Optional labels As IEnumerable(Of String) = Nothing) As GraphicsData
        Dim labelList As String() = Nothing

        If umap.dimension <> 2 Then
            Throw New InvalidProgramException($"the given umap projection result(dimension={umap.dimension}) is not a 2D data!")
        End If
        If Not labels Is Nothing Then
            labelList = labels.ToArray
        End If

        Dim embeddings = umap.GetEmbedding() _
            .Select(Function(vec)
                        Return New PointF With {.X = vec(0), .Y = vec(1)}
                    End Function) _
            .ToArray()


    End Function
End Module
