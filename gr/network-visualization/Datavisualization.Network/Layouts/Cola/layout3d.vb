Imports any = System.Object
Imports number = System.Double

Namespace Layouts.Cola

    Class Link3D
        Private length As number
        Public source As number
        Public target As number

        Public Sub New(source As number, target As number)
            Me.source = source
            Me.target = target
        End Sub

        Public Function actualLength(x As number()()) As number
            Return Math.sqrt(x.reduce(Function(c As number, v As number())
                                          Dim dx = v(Me.target) - v(Me.source)
                                          Return c + dx * dx

                                      End Function, 0))
        End Function
    End Class
    Class Node3D
        Inherits GraphNode
        ' if fixed, layout will not move the node from its specified starting position
        Public fixed As [Boolean]
        Public width As number
        Public height As number
        Public px As number
        Public py As number
        Public bounds As Rectangle
        Public variable As Variable

        Public x As number, y As number, z As number

        Public Sub New(Optional x As number = 0, Optional y As number = 0, Optional z As number = 0)
            Me.x = x
            Me.y = y
            Me.z = z
        End Sub
    End Class
    Class Layout3D
        Shared dims As String() = {"x", "y", "z"}
        Shared k As number = Layout3D.dims.Length
        Private result As number()()
        Public constraints As any() = Nothing
        Public nodes As Node3D()
        Public links As Link3D()
        Public idealLinkLength As number = 1

        Public Sub New(nodes As Node3D(), links As Link3D(), Optional idealLinkLength As number = 1)
            Me.result = New Array(Layout3D.k)
            Me.nodes = nodes
            Me.links = links
            Me.idealLinkLength = idealLinkLength

            For i As var = 0 To Layout3D.k - 1
                Me.result(i) = New Array(nodes.Length)
            Next
            nodes.forEach(Function(v, i)
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

        Public Function linkLength(l As Link3D) As number
            Return l.actualLength(Me.result)
        End Function

        Public useJaccardLinkLengths As Boolean = True

        Public descent As Descent
        Public Function start(Optional iterations As number = 100) As Layout3D
            Dim n = Me.nodes.length

            Dim linkAccessor = New LinkAccessor()

            If Me.useJaccardLinkLengths Then
                jaccardLinkLengths(Me.links, linkAccessor, 1.5)
            End If

            Me.links.forEach(Function(e) e.length *= Me.idealLinkLength)

            ' Create the distance matrix that Cola needs
            Dim distanceMatrix = (New Calculator(n, Me.links, Function(e) e.source, Function(e) e.target, Function(e) e.length)).DistanceMatrix()

            Dim D = Descent.createSquareMatrix(n, Function(i, j) distanceMatrix(i)(j))

            ' G is a square matrix with G[i][j] = 1 iff there exists an edge between node i and node j
            ' otherwise 2.
            Dim G = Descent.createSquareMatrix(n, Function() 2)
            Me.links.forEach(Function(source, target) InlineAssignHelper(G(source)(target), InlineAssignHelper(G(target)(source), 1)))

            Me.descent = New Descent(Me.result, D)
            Me.descent.threshold = 0.001
            Me.descent.G = G
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

        Public Function tick() As number
            Me.descent.locks.clear()
            For i As var = 0 To Me.nodes.length - 1
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
        Public Function getSourceIndex(e As any) As number
            Return e.source
        End Function
        Public Function getTargetIndex(e As any) As number
            Return e.target
        End Function
        Public Function getLength(e As any) As number
            Return e.length
        End Function
        Public Sub setLength(e As any, l As number)
            e.length = l
        End Sub
    End Class
End Namespace