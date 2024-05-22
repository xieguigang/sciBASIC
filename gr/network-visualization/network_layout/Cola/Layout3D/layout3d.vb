#Region "Microsoft.VisualBasic::c6f0c36d883159aecd0d4c891ee25892, gr\network-visualization\network_layout\Cola\Layout3D\layout3d.vb"

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

    '   Total Lines: 106
    '    Code Lines: 80 (75.47%)
    ' Comment Lines: 6 (5.66%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 20 (18.87%)
    '     File Size: 4.33 KB


    '     Class Layout3D
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: linkLength, start, tick
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter
Imports any = System.Object
Imports number = System.Double
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Cola

    Public Class Layout3D

        Shared ReadOnly dims As String() = {"x", "y", "z"}
        Shared ReadOnly k As Integer = Layout3D.dims.Length

        Private result As Double()()
        Public constraints As any() = Nothing
        Public nodes As Node3D()
        Public links As Link(Of Integer)()
        Public idealLinkLength As Double = 1

        Public Sub New(nodes As Node3D(), links As Link(Of Integer)(), Optional idealLinkLength As Double = 1)
            Me.result = RectangularArray.Matrix(Of Double)(Layout3D.k, nodes.Length)
            Me.nodes = nodes
            Me.links = links
            Me.idealLinkLength = idealLinkLength

            nodes.ForEach(Sub(v, i)
                              For Each [dim] As String In Layout3D.dims
                                  If v([dim]) = 0 Then
                                      v([dim]) = randf.seeds.NextDouble
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

            Dim linkAccessor As New LinkAccessor(Of Link(Of Integer))

            If Me.useJaccardLinkLengths Then
                linkLengthExtensions.jaccardLinkLengths(Of Link(Of Integer))(Me.links, linkAccessor, 1.5)
            End If

            Me.links.DoEach(Sub(e) e.length *= Me.idealLinkLength)

            ' Create the distance matrix that Cola needs
            Dim distanceMatrix = (New Dijkstra.Calculator(Of Link3D)(n, Me.links, Function(e) e.source, Function(e) e.target, Function(e) e.length)).DistanceMatrix()
            Dim D = Descent.createSquareMatrix(n, Function(i, j) distanceMatrix(i)(j))

            ' G is a square matrix with G[i][j] = 1 iff there exists an edge between node i and node j
            ' otherwise 2.
            Dim G = Descent.createSquareMatrix(n, Function() 2)

            Me.links.DoEach(Sub(link)
                                G(link.target)(link.source) = 1
                                G(link.source)(link.target) = G(link.target)(link.source)
                            End Sub)

            Me.descent = New Descent(Me.result, D)
            Me.descent.threshold = 0.001
            Me.descent.g = G
            'let constraints = this.links.map(e=> <any>{
            '    axis: 'y', left: e.source, right: e.target, gap: e.length*1.5
            '});
            If Me.constraints.IsNullOrEmpty Then
                Me.descent.project = New Projection(Of Node3D)(Me.nodes, Nothing, Nothing, Me.constraints).projectFunctions()
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
