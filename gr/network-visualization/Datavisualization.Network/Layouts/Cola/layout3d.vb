Imports any = System.Object
Imports number = System.Double

Namespace Layouts.Cola

    Class Layout3D
        Shared dims As String() = {"x", "y", "z"}
        Shared k As Double = Layout3D.dims.Length
        Private result As Double()()
        Public constraints As any() = Nothing
        Public nodes As Node3D()
        Public links As Link3D()
        Public idealLinkLength As Double = 1

        Public Sub New(nodes As Node3D(), links As Link3D(), Optional idealLinkLength As Double = 1)
            Me.result = New Array(Layout3D.k)
            Me.nodes = nodes
            Me.links = links
            Me.idealLinkLength = idealLinkLength

            For i As var = 0 To Layout3D.k - 1
                Me.result(i) = New Array(nodes.Length)
            Next
            nodes.ForEach(Function(v, i)
                              For Each [dim] As var In Layout3D.dims
                                  If v([dim]) Is Nothing Then
                                      v([dim]) = Math.random()
                                  End If
                              Next
                              Me.result(0)(i) = v.x
                              Me.result(1)(i) = v.y
                              Me.result(2)(i) = v.z

                          End Function)
        End Sub

        Public Function linkLength(l As Link3D) As Double
            Return l.actualLength(Me.result)
        End Function

        Public useJaccardLinkLengths As Boolean = True

        Public descent As Descent
        Public Function start(Optional iterations As Double = 100) As Layout3D
            Dim n = Me.nodes.Length

            Dim linkAccessor = New LinkAccessor()

            If Me.useJaccardLinkLengths Then
                jaccardLinkLengths(Me.links, linkAccessor, 1.5)
            End If

            Me.links.ForEach(Function(e) e.length *= Me.idealLinkLength)

            ' Create the distance matrix that Cola needs
            Dim distanceMatrix = (New Calculator(n, Me.links, Function(e) e.source, Function(e) e.target, Function(e) e.length)).DistanceMatrix()

            Dim D = Descent.createSquareMatrix(n, Function(i, j) distanceMatrix(i)(j))

            ' G is a square matrix with G[i][j] = 1 iff there exists an edge between node i and node j
            ' otherwise 2.
            Dim G = Descent.createSquareMatrix(n, Function() 2)
            Me.links.ForEach(Function(source, target) InlineAssignHelper(G(source)(target), InlineAssignHelper(G(target)(source), 1)))

            Me.descent = New Descent(Me.result, D)
            Me.descent.threshold = 0.001
            Me.descent.g = G
            'let constraints = this.links.map(e=> <any>{
            '    axis: 'y', left: e.source, right: e.target, gap: e.length*1.5
            '});
            If Me.constraints Then
                Me.descent.project = New Projection(DirectCast(Me.nodes, GraphNode()), Nothing, Nothing, Me.constraints).projectFunctions()
            End If

            For i As var = 0 To Me.nodes.Length - 1
                Dim v = Me.nodes(i)
                If v.fixed Then
                    Me.descent.locks.add(i, New number() {v.x, v.y, v.z})
                End If
            Next

            Me.descent.run(iterations)
            Return Me
        End Function

        Public Function tick() As Double
            Me.descent.locks.clear()
            For i As var = 0 To Me.nodes.Length - 1
                Dim v = Me.nodes(i)
                If v.fixed Then
                    Me.descent.locks.add(i, New number() {v.x, v.y, v.z})
                End If
            Next
            Return Me.descent.rungeKutta()
        End Function
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class

    Class LinkAccessor
        Inherits LinkLengthAccessor(Of any)
        Public Function getSourceIndex(e As any) As Double
            Return e.source
        End Function
        Public Function getTargetIndex(e As any) As Double
            Return e.target
        End Function
        Public Function getLength(e As any) As Double
            Return e.length
        End Function
        Public Sub setLength(e As any, l As Double)
            e.length = l
        End Sub
    End Class
End Namespace