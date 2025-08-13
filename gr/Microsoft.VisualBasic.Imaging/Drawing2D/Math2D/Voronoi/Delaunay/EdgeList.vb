
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class EdgeList

        Private deltaX As Single
        Private xmin As Single

        Private hashSize As Integer
        Private hash As Halfedge()
        Private leftEndField As Halfedge
        Public ReadOnly Property LeftEnd As Halfedge
            Get
                Return leftEndField
            End Get
        End Property
        Private rightEndField As Halfedge
        Public ReadOnly Property RightEnd As Halfedge
            Get
                Return rightEndField
            End Get
        End Property

        Public Sub Dispose()
            Dim halfedge = leftEndField
            Dim prevHe As Halfedge
            While halfedge IsNot rightEndField
                prevHe = halfedge
                halfedge = halfedge.edgeListRightNeighbor
                prevHe.Dispose()
            End While
            leftEndField = Nothing
            rightEndField.Dispose()
            rightEndField = Nothing

            hash = Nothing
        End Sub

        Public Sub New(xmin As Single, deltaX As Single, sqrtSitesNb As Integer)
            Me.xmin = xmin
            Me.deltaX = deltaX
            hashSize = 2 * sqrtSitesNb

            hash = New Halfedge(hashSize - 1) {}

            ' Two dummy Halfedges:
            leftEndField = Halfedge.CreateDummy()
            rightEndField = Halfedge.CreateDummy()
            leftEndField.edgeListLeftNeighbor = Nothing
            leftEndField.edgeListRightNeighbor = rightEndField
            rightEndField.edgeListLeftNeighbor = leftEndField
            rightEndField.edgeListRightNeighbor = Nothing
            hash(0) = leftEndField
            hash(hashSize - 1) = rightEndField
        End Sub

        ' 
        ' * Insert newHalfedge to the right of lb
        ' * @param lb
        ' * @param newHalfedge

        Public Sub Insert(lb As Halfedge, newHalfedge As Halfedge)
            newHalfedge.edgeListLeftNeighbor = lb
            newHalfedge.edgeListRightNeighbor = lb.edgeListRightNeighbor
            lb.edgeListRightNeighbor.edgeListLeftNeighbor = newHalfedge
            lb.edgeListRightNeighbor = newHalfedge
        End Sub

        ' 
        ' * This function only removes the Halfedge from the left-right list.
        ' * We cannot dispose it yet because we are still using it.
        ' * @param halfEdge

        Public Sub Remove(halfedge As Halfedge)
            halfedge.edgeListLeftNeighbor.edgeListRightNeighbor = halfedge.edgeListRightNeighbor
            halfedge.edgeListRightNeighbor.edgeListLeftNeighbor = halfedge.edgeListLeftNeighbor
            halfedge.edge = Edge.DELETED
            halfedge.edgeListRightNeighbor = Nothing
            halfedge.edgeListLeftNeighbor = Nothing
        End Sub

        ' 
        ' * Find the rightmost Halfedge that is still elft of p
        ' * @param p
        ' * @return

        Public Function EdgeListLeftNeighbor(p As Vector2D) As Halfedge
            Dim bucket As Integer
            Dim halfedge As Halfedge

            ' Use hash table to get close to desired halfedge
            bucket = CInt((p.x - xmin) / deltaX * hashSize)
            If bucket < 0 Then
                bucket = 0
            End If
            If bucket >= hashSize Then
                bucket = hashSize - 1
            End If
            halfedge = GetHash(bucket)
            If halfedge Is Nothing Then
                Dim i = 0

                While True
                    halfedge = GetHash(bucket - i)
                    If halfedge IsNot Nothing Then Exit While
                    halfedge = GetHash(bucket + i)
                    If halfedge IsNot Nothing Then Exit While
                    i += 1
                End While
            End If
            ' Now search linear list of haledges for the correct one
            If halfedge Is leftEndField OrElse halfedge IsNot rightEndField AndAlso halfedge.IsLeftOf(p) Then
                Do
                    halfedge = halfedge.edgeListRightNeighbor
                Loop While halfedge IsNot rightEndField AndAlso halfedge.IsLeftOf(p)
                halfedge = halfedge.edgeListLeftNeighbor
            Else
                Do
                    halfedge = halfedge.edgeListLeftNeighbor
                Loop While halfedge IsNot leftEndField AndAlso Not halfedge.IsLeftOf(p)
            End If

            ' Update hash table and reference counts
            If bucket > 0 AndAlso bucket < hashSize - 1 Then
                hash(bucket) = halfedge
            End If
            Return halfedge
        End Function

        ' Get entry from the has table, pruning any deleted nodes
        Private Function GetHash(b As Integer) As Halfedge
            Dim halfedge As Halfedge

            If b < 0 OrElse b >= hashSize Then
                Return Nothing
            End If
            halfedge = hash(b)
            If halfedge IsNot Nothing AndAlso halfedge.edge Is Edge.DELETED Then
                ' Hash table points to deleted halfedge. Patch as necessary
                hash(b) = Nothing
                ' Still can't dispose halfedge yet!
                Return Nothing
            Else
                Return halfedge
            End If
        End Function
    End Class
End Namespace
