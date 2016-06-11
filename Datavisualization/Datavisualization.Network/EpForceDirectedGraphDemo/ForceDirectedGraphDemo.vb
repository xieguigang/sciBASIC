Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports System.Diagnostics
Imports System.Xml
Imports System.IO
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph

Namespace EpForceDirectedGraphDemo
    Partial Public Class ForceDirectedGraphForm
        Inherits Form
        Const Iwidth As Integer = 64
        Const Iheight As Integer = 32
        Private stopwatch As New Stopwatch()

        Private paper As Graphics
        Private panelTop As Integer
        Private panelBottom As Integer
        Private panelLeft As Integer
        Private panelRight As Integer


        Private m_fdgBoxes As Dictionary(Of Node, GridBox)
        Private m_fdgLines As Dictionary(Of Edge, GridLine)
        Private m_fdgGraph As NetworkGraph
        Private m_fdgPhysics As ForceDirected2D
        Private m_fdgRenderer As Renderer


        Private timer As New System.Timers.Timer(30)


        Public Sub New()
            InitializeComponent()

            Me.DoubleBuffered = True
            Me.Width = (Iwidth + 1) * 20
            Me.Height = (Iheight + 1) * 20 + 100
            Me.MaximumSize = New Size(Me.width, Me.height)
            Me.MaximizeBox = False

            tbStiffness.Text = "81.76"
            tbRepulsion.Text = "40000.0"
            tbDamping.Text = "0.5"
            panelTop = 0
            panelBottom = pDrawPanel.Size.Height
            panelLeft = 0
            panelRight = pDrawPanel.Size.Width

            m_fdgBoxes = New Dictionary(Of Node, GridBox)()
            m_fdgLines = New Dictionary(Of Edge, GridLine)()
            m_fdgGraph = New NetworkGraph()
            m_fdgPhysics = New ForceDirected2D(m_fdgGraph, 81.76F, 40000.0F, 0.5F)
            m_fdgRenderer = New Renderer(Me, m_fdgPhysics)


            AddHandler pDrawPanel.Paint, New PaintEventHandler(AddressOf DrawPanel_Paint)

            timer.Interval = 30
            AddHandler timer.Elapsed, New System.Timers.ElapsedEventHandler(AddressOf timer_Elapsed)

            timer.Start()
        End Sub

        Private Sub timer_Elapsed(sender As Object, e As System.Timers.ElapsedEventArgs)
            pDrawPanel.Invalidate()
        End Sub
        Private Sub DrawPanel_Paint(sender As Object, e As PaintEventArgs)
            stopwatch.[Stop]()
            Dim p As Panel = TryCast(sender, Panel)
            paper = e.Graphics

            Dim box As New GridBox((panelRight - panelLeft) \ 2, (panelBottom - panelTop) \ 2, BoxType.Pinned)
            box.DrawBox(paper)

            m_fdgRenderer.Draw(0.05F)
            ' TODO: Check Time
            stopwatch.Reset()
            stopwatch.Start()


        End Sub

        Private Sub ForceDirectedGraph_Paint(sender As Object, e As PaintEventArgs)


        End Sub
        Public Function GraphToScreen(iPos As FDGVector2) As Point
            Dim retPair As New Point
            retPair.X = CInt(Math.Truncate(iPos.x + (CSng(panelRight - panelLeft) / 2.0F)))
            retPair.Y = CInt(Math.Truncate(iPos.y + (CSng(panelBottom - panelTop) / 2.0F)))
            Return retPair
        End Function

        Public Function ScreenToGraph(iScreenPos As Point) As FDGVector2
            Dim retVec As New FDGVector2()
            retVec.x = CSng(iScreenPos.X) - (CSng(panelRight - panelLeft) / 2.0F)
            retVec.y = CSng(iScreenPos.Y) - (CSng(panelBottom - panelTop) / 2.0F)
            Return retVec
        End Function

        Public Sub DrawLine(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
            Dim pos1 As Point = GraphToScreen(TryCast(iPosition1, FDGVector2))
            Dim pos2 As Point = GraphToScreen(TryCast(iPosition2, FDGVector2))
            m_fdgLines(iEdge).[Set](pos1.X, pos1.Y, pos2.X, pos2.Y)
            m_fdgLines(iEdge).DrawLine(paper)

        End Sub

        Public Sub DrawBox(iNode As Node, iPosition As AbstractVector)
            Dim pos As Point = GraphToScreen(TryCast(iPosition, FDGVector2))
            m_fdgBoxes(iNode).[Set](pos.X, pos.Y)
            m_fdgBoxes(iNode).DrawBox(paper)
        End Sub

        Private Sub btnChangeProperties_Click(sender As Object, e As EventArgs)
            Dim stiffNess As Single = System.Convert.ToSingle(tbStiffness.Text)
            m_fdgPhysics.Stiffness = stiffNess
            Dim repulsion As Single = System.Convert.ToSingle(tbRepulsion.Text)
            m_fdgPhysics.Repulsion = repulsion
            Dim damping As Single = System.Convert.ToSingle(tbDamping.Text)
            m_fdgPhysics.Damping = damping
        End Sub

        Private Function addNode(nodeName As String) As Boolean
            nodeName = nodeName.Trim()
            If m_fdgGraph.GetNode(tbNodeName.Text) IsNot Nothing Then
                Return False
            End If
            Dim newNode As Node = m_fdgGraph.CreateNode(nodeName)
            m_fdgBoxes(newNode) = New GridBox(0, 0, BoxType.Normal)

            cbbFromNode.Items.Add(nodeName)
            cbbToNode.Items.Add(nodeName)
            lbNode.Items.Add(nodeName)
            Return True
        End Function
        Private Sub btnAddNode_Click(sender As Object, e As EventArgs)
            tbNodeName.Text = tbNodeName.Text.Trim()
            If tbNodeName.Text = "" Then
                MessageBox.Show("Please type in the node name to insert!")
                Return
            End If
            If Not addNode(tbNodeName.Text) Then
                MessageBox.Show("Node already exists in the graph!")
                Return
            End If
        End Sub
        Private Function addEdge(nodeName1 As String, nodeName2 As String) As Boolean
            nodeName1 = nodeName1.Trim()
            nodeName2 = nodeName2.Trim()
            If nodeName1 = nodeName2 Then
                Return False
            End If
            Dim node1 As Node = m_fdgGraph.GetNode(nodeName1)
            Dim node2 As Node = m_fdgGraph.GetNode(nodeName2)
            Dim data As New EdgeData()

            Dim label As String = nodeName1 & "-" & nodeName2
            data.label = label
            data.length = 60.0F

            Dim newEdge As Edge = m_fdgGraph.CreateEdge(node1, node2, data)
            m_fdgLines(newEdge) = New GridLine(0, 0, 0, 0)

            lbEdge.Items.Add(label)
            Return True
        End Function
        Private Sub btnAddEdge_Click(sender As Object, e As EventArgs)
            Dim nodeName1 As String = cbbFromNode.Text
            Dim nodeName2 As String = cbbToNode.Text
            If Not addEdge(nodeName1, nodeName2) Then
                MessageBox.Show("Edge cannot be connected to same node!")
                Return
            End If
        End Sub

        Private Sub btnRemoveNode_Click(sender As Object, e As EventArgs)
            If lbNode.SelectedIndex <> -1 Then
                Dim selectedIdx As Integer = lbNode.SelectedIndex
                Dim nodeName As String = DirectCast(lbNode.SelectedItem, String)
                Dim removeNode As Node = m_fdgGraph.GetNode(nodeName)

                m_fdgBoxes.Remove(removeNode)
                Dim edgeList As List(Of Edge) = m_fdgGraph.GetEdges(removeNode)
                For Each edge As Edge In edgeList
                    m_fdgLines.Remove(edge)
                    Dim edgeIndex As Integer = lbEdge.FindString(edge.Data.label)
                    lbEdge.Items.RemoveAt(edgeIndex)
                Next
                m_fdgGraph.RemoveNode(removeNode)


                cbbFromNode.Items.RemoveAt(lbNode.SelectedIndex)
                cbbToNode.Items.RemoveAt(lbNode.SelectedIndex)

                lbNode.Items.RemoveAt(lbNode.SelectedIndex)
                If selectedIdx <> 0 Then
                    lbNode.SelectedIndex = selectedIdx - 1
                End If
                lbNode.Focus()
            Else
                MessageBox.Show("Please select a node to remove!")
            End If
        End Sub

        Private Sub btnRemoveEdge_Click(sender As Object, e As EventArgs)
            If lbEdge.SelectedIndex <> -1 Then
                Dim selectedIdx As Integer = lbEdge.SelectedIndex
                Dim edgeName As String = DirectCast(lbEdge.SelectedItem, String)
                Dim removeEdge As Edge = m_fdgGraph.GetEdge(edgeName)
                m_fdgLines.Remove(removeEdge)
                m_fdgGraph.RemoveEdge(removeEdge)
                lbEdge.Items.RemoveAt(lbEdge.SelectedIndex)
                If selectedIdx <> 0 Then
                    lbEdge.SelectedIndex = selectedIdx - 1
                End If
                lbEdge.Focus()
            Else
                MessageBox.Show("Please select an edge to remove!")
            End If
        End Sub

        Private clickedNode As Node
        Private clickedGrid As GridBox
        Private Sub pDrawPanel_MouseDown(sender As Object, e As MouseEventArgs)
            For Each keyPair As KeyValuePair(Of Node, GridBox) In m_fdgBoxes
                If keyPair.Value.boxRec.IntersectsWith(New Rectangle(e.Location, New Size(1, 1))) Then
                    clickedNode = keyPair.Key
                    clickedGrid = keyPair.Value
                    clickedGrid.boxType = BoxType.Pinned
                End If
            Next
        End Sub

        Private Sub pDrawPanel_MouseMove(sender As Object, e As MouseEventArgs)
            If clickedNode IsNot Nothing Then

                Dim vec As FDGVector2 = ScreenToGraph(New Point(e.Location.X, e.Location.Y))
                clickedNode.Pinned = True
                m_fdgPhysics.GetPoint(clickedNode).position = vec
            Else
                For Each keyPair As KeyValuePair(Of Node, GridBox) In m_fdgBoxes
                    If keyPair.Value.boxRec.IntersectsWith(New Rectangle(e.Location, New Size(1, 1))) Then
                        keyPair.Value.boxType = BoxType.Pinned
                    Else
                        keyPair.Value.boxType = BoxType.Normal
                    End If

                Next
            End If
        End Sub

        Private Sub pDrawPanel_MouseUp(sender As Object, e As MouseEventArgs)
            If clickedNode IsNot Nothing Then
                clickedGrid.boxType = BoxType.Normal
                clickedNode.Pinned = False
                clickedNode = Nothing
                clickedGrid = Nothing
            End If
        End Sub

        Private Sub tbNodeName_KeyDown(sender As Object, e As KeyEventArgs)
            If e.KeyCode = Keys.Enter Then
                btnAddNode_Click(sender, e)
                tbNodeName.Focus()
            End If
        End Sub

        Private Sub tbStiffness_KeyDown(sender As Object, e As KeyEventArgs)
            If e.KeyCode = Keys.Enter Then
                btnChangeProperties_Click(sender, e)
                tbStiffness.Focus()
            End If
        End Sub

        Private Sub tbRepulsion_KeyDown(sender As Object, e As KeyEventArgs)
            If e.KeyCode = Keys.Enter Then
                btnChangeProperties_Click(sender, e)
                tbRepulsion.Focus()
            End If
        End Sub

        Private Sub tbDamping_KeyDown(sender As Object, e As KeyEventArgs)
            If e.KeyCode = Keys.Enter Then
                btnChangeProperties_Click(sender, e)
                tbDamping.Focus()
            End If
        End Sub


        Private Sub btnLoad_Click(sender As Object, e As EventArgs)
            Dim fileDialog As New OpenFileDialog()
            Dim result As DialogResult = fileDialog.ShowDialog()
            ' Show the dialog.
            If result = DialogResult.OK Then
                ' Test result.
                Dim file__1 As String = fileDialog.FileName
                Try
                    Dim text As String = File.ReadAllText(file__1)
                    Dim size As Int32 = text.Length

                    Dim mapXML As New StringReader(text)
                    Dim xmlReader As New XmlTextReader(mapXML)
                    While xmlReader.Read()
                        Select Case xmlReader.NodeType
                            Case XmlNodeType.Element
                                ' The node is an Element.
                                If xmlReader.Name = "Node" Then
                                    loadNode(xmlReader)
                                ElseIf xmlReader.Name = "Edge" Then
                                    loadEdge(xmlReader)
                                End If
                                Exit Select

                            Case XmlNodeType.Text
                                'Display the text in each element.
                                Exit Select
                            Case XmlNodeType.EndElement
                                'Display end of element.
                                Exit Select
                        End Select
                    End While
                Catch generatedExceptionName As IOException
                End Try
            End If


        End Sub

        Private Sub loadNode(iXmlReader As XmlTextReader)
            While iXmlReader.MoveToNextAttribute()
                ' Read attributes.
                If iXmlReader.Name = "nodeName" Then
                    addNode(iXmlReader.Value)
                End If
            End While
        End Sub
        Private Sub loadEdge(iXmlReader As XmlTextReader)
            Dim nodeName1 As String = Nothing
            Dim nodeName2 As String = Nothing
            While iXmlReader.MoveToNextAttribute()
                ' Read attributes.
                If iXmlReader.Name = "nodeName1" Then
                    nodeName1 = iXmlReader.Value
                End If
                If iXmlReader.Name = "nodeName2" Then
                    nodeName2 = iXmlReader.Value
                End If
            End While
            addEdge(nodeName1, nodeName2)
        End Sub

        Private Sub btnClear_Click(sender As Object, e As EventArgs)
            m_fdgPhysics.Clear()
            m_fdgGraph.Clear()
            m_fdgBoxes.Clear()
            m_fdgLines.Clear()
            lbEdge.Items.Clear()
            lbNode.Items.Clear()
            cbbFromNode.Items.Clear()
            cbbToNode.Items.Clear()
        End Sub
    End Class
End Namespace
