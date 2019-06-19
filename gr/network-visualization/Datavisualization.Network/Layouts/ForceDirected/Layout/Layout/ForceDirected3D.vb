Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Interfaces

Namespace Layouts

    Public Class ForceDirected3D
        Inherits ForceDirected(Of FDGVector3)

        Public Sub New(iGraph As NetworkGraph, iStiffness As Single, iRepulsion As Single, iDamping As Single)
            MyBase.New(iGraph, iStiffness, iRepulsion, iDamping)
        End Sub

        Public Overrides Function GetPoint(iNode As Node) As LayoutPoint
            If Not (m_nodePoints.ContainsKey(iNode.Label)) Then
                Dim iniPosition As FDGVector3 = TryCast(iNode.data.initialPostion, FDGVector3)
                If iniPosition Is Nothing Then
                    iniPosition = TryCast(FDGVector3.Random(), FDGVector3)
                End If
                m_nodePoints(iNode.Label) = New LayoutPoint(iniPosition, FDGVector3.Zero(), FDGVector3.Zero(), iNode)
            End If
            Return m_nodePoints(iNode.Label)
        End Function

        Public Overrides Function GetBoundingBox() As BoundingBox
            Dim boundingBox As New BoundingBox()
            Dim bottomLeft As FDGVector3 = TryCast(FDGVector3.Identity().Multiply(BoundingBox.defaultBB * -1.0F), FDGVector3)
            Dim topRight As FDGVector3 = TryCast(FDGVector3.Identity().Multiply(BoundingBox.defaultBB), FDGVector3)

            For Each n As Node In graph.vertex
                Dim position As FDGVector3 = TryCast(GetPoint(n).position, FDGVector3)
                If position.x < bottomLeft.x Then
                    bottomLeft.x = position.x
                End If
                If position.y < bottomLeft.y Then
                    bottomLeft.y = position.y
                End If
                If position.z < bottomLeft.z Then
                    bottomLeft.z = position.z
                End If
                If position.x > topRight.x Then
                    topRight.x = position.x
                End If
                If position.y > topRight.y Then
                    topRight.y = position.y
                End If
                If position.z > topRight.z Then
                    topRight.z = position.z
                End If
            Next

            Dim padding As AbstractVector = (topRight - bottomLeft).Multiply(BoundingBox.defaultPadding)

            boundingBox.bottomLeftFront = bottomLeft.Subtract(padding)
            boundingBox.topRightBack = topRight.Add(padding)

            Return boundingBox
        End Function
    End Class
End Namespace