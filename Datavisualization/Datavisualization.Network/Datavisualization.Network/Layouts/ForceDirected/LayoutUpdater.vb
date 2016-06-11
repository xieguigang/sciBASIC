Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts.Interfaces

Namespace Layouts

    Public Module LayoutUpdater

        Private Class __layoutGenerator : Inherits AbstractRenderer

            Public Sub New(iForceDirected As IForceDirected)
                MyBase.New(iForceDirected)
            End Sub

            Public Overrides Sub Clear()

            End Sub

            Protected Overrides Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
            End Sub

            ''' <summary>
            ''' 在这里更新节点的位置
            ''' </summary>
            ''' <param name="iNode"></param>
            ''' <param name="iPosition"></param>
            Protected Overrides Sub drawNode(iNode As Node, iPosition As AbstractVector)
                iNode.Data.initialPostion = iPosition
            End Sub
        End Class

        <Extension>
        Public Sub Updates(Of T As AbstractVector)(engine As ForceDirected(Of T), ByRef net As NetworkGraph, Optional loops As Integer = 100)
            Dim updater As New __layoutGenerator(engine)

            For i As Integer = 0 To loops - 1
                Call updater.Draw(0.05F)
            Next
        End Sub
    End Module
End Namespace