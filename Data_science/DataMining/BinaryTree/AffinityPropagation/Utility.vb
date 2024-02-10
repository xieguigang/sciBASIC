Namespace AffinityPropagation
    Public Class Point
        Protected _coordinates As Single()
        Public Property Center As Point
        Public Sub New(ParamArray coordinates As Single())
            _coordinates = New Single(coordinates.Length - 1) {}
            Dim i = 0

            While i < coordinates.Length
                _coordinates(i) = coordinates(i)
                i += 1
            End While

            Center = Nothing
        End Sub
        Public ReadOnly Property Dimension As Integer
            Get
                Return _coordinates.Length
            End Get
        End Property
        Public Function Coordinates(index As Integer) As Single
            Return _coordinates(index)
        End Function

        Public Overloads Function Equals(obj As Point) As Boolean
            ' Perform an equality check on two rectangles (Point object pairs).
            If obj Is Nothing OrElse [GetType]() IsNot obj.GetType() OrElse obj.Dimension <> Dimension Then Return False

            Dim i = 0

            While i < Dimension
                If _coordinates(i) <> obj._coordinates(i) Then Return False
                i += 1
            End While
            Return True
        End Function


    End Class
    Public Class Edge
        Implements IComparable(Of Edge)
        Public Property Source As Integer
        Public Property Destination As Integer
        Public Property Similarity As Single
        Public Property Responsability As Single
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


    Public Module Distance

        ''' <summary>
        ''' checking for dim x == dim y will hurt performance this should be done at init
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Function NegEuclidienDistance(x As Point, y As Point) As Single
            Dim f = 0.0F
            Dim i = 0

            While i < x.Dimension
                f += (y.Coordinates(i) - x.Coordinates(i)) * (y.Coordinates(i) - x.Coordinates(i))
                i += 1
            End While

            Return -1 * f

        End Function
    End Module
    Public Module SimilarityMatrix

        ''' <summary>
        ''' Create the similarity matrix with a user defined distance measure
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <param name="distance"></param>
        ''' <returns></returns>
        Public Function SparseSimilarityMatrix(ptr As Point(), distance As Func(Of Point, Point, Single)) As Edge()
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
        Public Function SparseSimilarityMatrix(ptr As Point()) As Edge()
            Dim items = New Edge(ptr.Length * ptr.Length - 1) {}
            Dim p = 0
            For i = 0 To ptr.Length - 1 - 1
                For j = i + 1 To ptr.Length - 1
                    items(p) = New Edge(i, j, NegEuclidienDistance(ptr(i), ptr(j)))
                    items(p + 1) = New Edge(j, i, NegEuclidienDistance(ptr(i), ptr(j)))
                    p += 2
                Next
            Next
            Return items
        End Function
    End Module

    Public Class ClusterUtility
        Public Shared Function GroupClusters(points As Point(), centers As Integer(), CentersIndecies As Integer()) As List(Of Point)()
            ''' Create an array of list that contains clusters
            ''' ie: The points are grouped together given their clusters

            Dim tmp = New List(Of Point)(CentersIndecies.Length - 1) {}

            Dim i = 0

            While i < tmp.Length
                tmp(i) = New List(Of Point)(points.Length / CentersIndecies.Length)
                i += 1
            End While

            i = 0

            While i < points.Length
                Dim j = 0

                While j < CentersIndecies.Length
                    If points(i).Center Is Nothing AndAlso points(i).Equals(points(CentersIndecies(j))) Then
                        tmp(j).Add(points(i))
                    ElseIf points(i).Center IsNot Nothing AndAlso points(i).Center.Equals(points(CentersIndecies(j))) Then
                        tmp(j).Add(points(i))
                    End If

                    j += 1
                End While

                i += 1
            End While
            Return tmp

        End Function
        Public Shared Sub AssignClusterCenters(input As Point(), result As Integer())
            ' assign the center for each point
            ' if the point itself is the center of the cluster then
            ' assign null for its center value

            Dim i = 0

            While i < result.Length
                If Not input(i).Equals(input(result(i))) Then
                    input(i).Center = input(result(i))
                Else
                    input(i).Center = Nothing
                End If

                i += 1
            End While
        End Sub
    End Class


End Namespace
