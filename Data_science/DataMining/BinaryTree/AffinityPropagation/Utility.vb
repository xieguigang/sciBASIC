Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Distance
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations

Namespace AffinityPropagation

    Public Class Edge : Implements IComparable(Of Edge)
        Public Property Source As Integer
        Public Property Destination As Integer
        Public Property Similarity As Single
        Public Property Responsability As Double
        Public Property Availability As Single

        Public Sub New()
            Destination = 0
            Source = 0
            Availability = 0.0F
            Responsability = 0
            Similarity = 0.0F
        End Sub
        Public Sub New(Source As Integer, Destination As Integer, Similarity As Single)
            Me.Source = Source
            Me.Destination = Destination
            Me.Similarity = Similarity
            Responsability = 0
            Availability = 0
        End Sub
        Public Function CompareTo(obj As Edge) As Integer Implements IComparable(Of Edge).CompareTo
            Return Similarity.CompareTo(obj.Similarity)
        End Function
    End Class

    Public Class Graph

        Public ReadOnly Property VerticesCount As Integer

        Public SimMatrixElementsCount As Integer

        Public outEdges As Edge()()
        Public inEdges As Edge()()
        Public Edges As Edge()

        Public Sub New(vertices As Integer)
            VerticesCount = If(vertices < 0, 0, vertices)
            SimMatrixElementsCount = (VerticesCount - 1) * VerticesCount + VerticesCount

            outEdges = New Edge(VerticesCount - 1)() {}
            inEdges = New Edge(VerticesCount - 1)() {}
            Edges = New Edge(SimMatrixElementsCount - 1) {}

            Dim i = 0

            While i < VerticesCount
                outEdges(i) = New Edge(VerticesCount - 1) {}
                inEdges(i) = New Edge(VerticesCount - 1) {}
                i += 1
            End While
        End Sub

    End Class

    Public Module SimilarityMatrix

        ''' <summary>
        ''' Create the similarity matrix with a user defined distance measure
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <param name="distance"></param>
        ''' <returns></returns>
        Public Function SparseSimilarityMatrix(ptr As ClusterEntity(), distance As IMetric) As Edge()
            Dim items = New Edge(ptr.Length * ptr.Length - 1) {}
            Dim p = 0
            For i = 0 To ptr.Length - 1 - 1
                For j = i + 1 To ptr.Length - 1
                    items(p) = New Edge(i, j, distance(ptr(i), ptr(j)))
                    items(p + 1) = New Edge(j, i, distance(ptr(i), ptr(j)))
                    p += 2
                Next
            Next
            Return items
        End Function
        Public Function SparseSimilarityMatrix(ptr As ClusterEntity()) As Edge()
            Dim items = New Edge(ptr.Length * ptr.Length - 1) {}
            Dim p = 0
            For i = 0 To ptr.Length - 1 - 1
                For j = i + 1 To ptr.Length - 1
                    items(p) = New Edge(i, j, -ptr(i).DistanceTo(ptr(j)))
                    items(p + 1) = New Edge(j, i, -ptr(i).DistanceTo(ptr(j)))
                    p += 2
                Next
            Next
            Return items
        End Function
    End Module

End Namespace
