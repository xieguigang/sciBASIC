#Region "Microsoft.VisualBasic::7e7586b54deaf21bcac83a0ba42dbd93, gr\network-visualization\Datavisualization.Network\Layouts\ForceDirected\LayoutUpdater.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module LayoutUpdater
    ' 
    '         Sub: Updates
    '         Class layoutGenerator
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Clear, drawEdge, drawNode
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Interfaces

Namespace Layouts

    Public Module LayoutUpdater

        ''' <summary>
        ''' Do nothing, just used for generate network layout
        ''' </summary>
        Private Class layoutGenerator : Inherits AbstractRenderer

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
                iNode.data.initialPostion = iPosition
            End Sub
        End Class

        <Extension>
        Public Sub Updates(Of T As AbstractVector)(engine As ForceDirected(Of T), ByRef net As NetworkGraph, Optional loops As Integer = 100)
            Dim updater As New layoutGenerator(engine)

            For i As Integer = 0 To loops - 1
                Call updater.Draw(0.05F)
            Next
        End Sub
    End Module
End Namespace
