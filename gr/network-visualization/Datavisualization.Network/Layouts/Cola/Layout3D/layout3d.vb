Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports any = System.Object
Imports number = System.Double

Namespace Layouts.Cola

    Public Class Layout3D

        Shared ReadOnly dims As String() = {"x", "y", "z"}
        Shared ReadOnly k As Integer = Layout3D.dims.Length

        Private result As Double()()
        Public constraints As any() = Nothing
        Public nodes As Node3D()
        Public links As Link3D()
        Public idealLinkLength As Double = 1

        Public Sub New(nodes As Node3D(), links As Link3D(), Optional idealLinkLength As Double = 1)
            Me.result = MAT(Of Double)(Layout3D.k, nodes.Length)
            Me.nodes = nodes
            Me.links = links
            Me.idealLinkLength = idealLinkLength

            nodes.ForEach(Sub(v, i)
                              For Each [dim] As String In Layout3D.dims
                                  If v([dim]) = 0 Then
                                      v([dim]) = Math.Seeds.NextDouble
                                  End If
                              Next
                              Me.result(0)(i) = v.x
                              Me.result(1)(i) = v.y
                              Me.result(2)(i) = v.z
                          End Sub)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

            Me.links.DoEach(Sub(e) e.length *= Me.idealLinkLength)

            ' Create the distance matrix that Cola needs
            Dim distanceMatrix = (New Dijkstra.Calculator(Of Link3D)(n, Me.links, Function(e) e.source, Function(e) e.target, Function(e) e.length)).DistanceMatrix()
            Dim D = Descent.createSquareMatrix(n, Function(i, j) distanceMatrix(i)(j))

            ' G is a square matrix with G[i][j] = 1 iff there exists an edge between node i and node j
            ' otherwise 2.
            Dim G = Descent.createSquareMatrix(n, Function() 2)
            Me.links.ForEach(Sub(source, target)
                                 G(target)(source) = 1
                                 G(source)(target) = G(target)(source)
                             End Sub)

            Me.descent = New Descent(Me.result, D)
            Me.descent.threshold = 0.001
            Me.descent.g = G
            'let constraints = this.links.map(e=> <any>{
            '    axis: 'y', left: e.source, right: e.target, gap: e.length*1.5
            '});
            If Me.constraints.IsNullOrEmpty Then
                Me.descent.project = New Projection(DirectCast(Me.nodes, GraphNode()), Nothing, Nothing, Me.constraints).projectFunctions()
            End If

            For i As Integer = 0 To Me.nodes.Length - 1
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
            For i As Integer = 0 To Me.nodes.Length - 1
                Dim v = Me.nodes(i)
                If v.fixed Then
                    Me.descent.locks.add(i, New number() {v.x, v.y, v.z})
                End If
            Next
            Return Me.descent.rungeKutta()
        End Function

    End Class
End Namespace