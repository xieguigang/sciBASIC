Namespace EpForceDirectedGraphDemo
	Partial Class ForceDirectedGraphForm
		''' <summary>
		''' Required designer variable.
		''' </summary>
		Private components As System.ComponentModel.IContainer = Nothing

		''' <summary>
		''' Clean up any resources being used.
		''' </summary>
		''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		Protected Overrides Sub Dispose(disposing As Boolean)
			If disposing AndAlso (components IsNot Nothing) Then
				components.Dispose()
			End If
			MyBase.Dispose(disposing)
		End Sub

		#Region "Windows Form Designer generated code"

		''' <summary>
		''' Required method for Designer support - do not modify
		''' the contents of this method with the code editor.
		''' </summary>
		Private Sub InitializeComponent()
			Me.tbStiffness = New System.Windows.Forms.TextBox()
			Me.label1 = New System.Windows.Forms.Label()
			Me.label2 = New System.Windows.Forms.Label()
			Me.tbRepulsion = New System.Windows.Forms.TextBox()
			Me.label3 = New System.Windows.Forms.Label()
			Me.tbDamping = New System.Windows.Forms.TextBox()
			Me.cbbFromNode = New System.Windows.Forms.ComboBox()
			Me.label4 = New System.Windows.Forms.Label()
			Me.tbNodeName = New System.Windows.Forms.TextBox()
			Me.btnAddNode = New System.Windows.Forms.Button()
			Me.cbbToNode = New System.Windows.Forms.ComboBox()
			Me.label5 = New System.Windows.Forms.Label()
			Me.btnAddEdge = New System.Windows.Forms.Button()
			Me.lbNode = New System.Windows.Forms.ListBox()
			Me.lbEdge = New System.Windows.Forms.ListBox()
			Me.btnRemoveNode = New System.Windows.Forms.Button()
			Me.btnRemoveEdge = New System.Windows.Forms.Button()
			Me.btnChangeProperties = New System.Windows.Forms.Button()
			Me.pDrawPanel = New EpForceDirectedGraphDemo.DoubleBufferPanel()
			Me.btnLoad = New System.Windows.Forms.Button()
			Me.btnClear = New System.Windows.Forms.Button()
			Me.SuspendLayout()
			' 
			' tbStiffness
			' 
			Me.tbStiffness.Location = New System.Drawing.Point(92, 29)
			Me.tbStiffness.Name = "tbStiffness"
			Me.tbStiffness.Size = New System.Drawing.Size(100, 21)
			Me.tbStiffness.TabIndex = 6
			AddHandler Me.tbStiffness.KeyDown, New System.Windows.Forms.KeyEventHandler(AddressOf Me.tbStiffness_KeyDown)
			' 
			' label1
			' 
			Me.label1.AutoSize = True
			Me.label1.Location = New System.Drawing.Point(24, 32)
			Me.label1.Name = "label1"
			Me.label1.Size = New System.Drawing.Size(53, 12)
			Me.label1.TabIndex = 1
			Me.label1.Text = "Stiffness"
			' 
			' label2
			' 
			Me.label2.AutoSize = True
			Me.label2.Location = New System.Drawing.Point(198, 32)
			Me.label2.Name = "label2"
			Me.label2.Size = New System.Drawing.Size(61, 12)
			Me.label2.TabIndex = 3
			Me.label2.Text = "Repulsion"
			' 
			' tbRepulsion
			' 
			Me.tbRepulsion.Location = New System.Drawing.Point(265, 29)
			Me.tbRepulsion.Name = "tbRepulsion"
			Me.tbRepulsion.Size = New System.Drawing.Size(100, 21)
			Me.tbRepulsion.TabIndex = 7
			AddHandler Me.tbRepulsion.KeyDown, New System.Windows.Forms.KeyEventHandler(AddressOf Me.tbRepulsion_KeyDown)
			' 
			' label3
			' 
			Me.label3.AutoSize = True
			Me.label3.Location = New System.Drawing.Point(371, 32)
			Me.label3.Name = "label3"
			Me.label3.Size = New System.Drawing.Size(55, 12)
			Me.label3.TabIndex = 5
			Me.label3.Text = "Damping"
			' 
			' tbDamping
			' 
			Me.tbDamping.Location = New System.Drawing.Point(432, 29)
			Me.tbDamping.Name = "tbDamping"
			Me.tbDamping.Size = New System.Drawing.Size(100, 21)
			Me.tbDamping.TabIndex = 8
			AddHandler Me.tbDamping.KeyDown, New System.Windows.Forms.KeyEventHandler(AddressOf Me.tbDamping_KeyDown)
			' 
			' cbbFromNode
			' 
			Me.cbbFromNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
			Me.cbbFromNode.FormattingEnabled = True
			Me.cbbFromNode.Location = New System.Drawing.Point(279, 2)
			Me.cbbFromNode.Name = "cbbFromNode"
			Me.cbbFromNode.Size = New System.Drawing.Size(121, 20)
			Me.cbbFromNode.TabIndex = 3
			' 
			' label4
			' 
			Me.label4.AutoSize = True
			Me.label4.Location = New System.Drawing.Point(13, 5)
			Me.label4.Name = "label4"
			Me.label4.Size = New System.Drawing.Size(73, 12)
			Me.label4.TabIndex = 8
			Me.label4.Text = "Node Name"
			' 
			' tbNodeName
			' 
			Me.tbNodeName.Location = New System.Drawing.Point(92, 2)
			Me.tbNodeName.Name = "tbNodeName"
			Me.tbNodeName.Size = New System.Drawing.Size(100, 21)
			Me.tbNodeName.TabIndex = 1
			AddHandler Me.tbNodeName.KeyDown, New System.Windows.Forms.KeyEventHandler(AddressOf Me.tbNodeName_KeyDown)
			' 
			' btnAddNode
			' 
			Me.btnAddNode.Location = New System.Drawing.Point(198, 0)
			Me.btnAddNode.Name = "btnAddNode"
			Me.btnAddNode.Size = New System.Drawing.Size(75, 23)
			Me.btnAddNode.TabIndex = 2
			Me.btnAddNode.Text = "Add Node"
			Me.btnAddNode.UseVisualStyleBackColor = True
			AddHandler Me.btnAddNode.Click, New System.EventHandler(AddressOf Me.btnAddNode_Click)
			' 
			' cbbToNode
			' 
			Me.cbbToNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
			Me.cbbToNode.FormattingEnabled = True
			Me.cbbToNode.Location = New System.Drawing.Point(425, 2)
			Me.cbbToNode.Name = "cbbToNode"
			Me.cbbToNode.Size = New System.Drawing.Size(121, 20)
			Me.cbbToNode.TabIndex = 4
			' 
			' label5
			' 
			Me.label5.AutoSize = True
			Me.label5.Location = New System.Drawing.Point(406, 5)
			Me.label5.Name = "label5"
			Me.label5.Size = New System.Drawing.Size(11, 12)
			Me.label5.TabIndex = 11
			Me.label5.Text = "-"
			' 
			' btnAddEdge
			' 
			Me.btnAddEdge.Location = New System.Drawing.Point(552, 0)
			Me.btnAddEdge.Name = "btnAddEdge"
			Me.btnAddEdge.Size = New System.Drawing.Size(75, 23)
			Me.btnAddEdge.TabIndex = 5
			Me.btnAddEdge.Text = "Add Edge"
			Me.btnAddEdge.UseVisualStyleBackColor = True
			AddHandler Me.btnAddEdge.Click, New System.EventHandler(AddressOf Me.btnAddEdge_Click)
			' 
			' lbNode
			' 
			Me.lbNode.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
			Me.lbNode.FormattingEnabled = True
			Me.lbNode.ItemHeight = 12
			Me.lbNode.Location = New System.Drawing.Point(12, 79)
			Me.lbNode.Name = "lbNode"
			Me.lbNode.Size = New System.Drawing.Size(120, 436)
			Me.lbNode.TabIndex = 10
			' 
			' lbEdge
			' 
			Me.lbEdge.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
			Me.lbEdge.FormattingEnabled = True
			Me.lbEdge.ItemHeight = 12
			Me.lbEdge.Location = New System.Drawing.Point(141, 79)
			Me.lbEdge.Name = "lbEdge"
			Me.lbEdge.Size = New System.Drawing.Size(120, 436)
			Me.lbEdge.TabIndex = 12
			' 
			' btnRemoveNode
			' 
			Me.btnRemoveNode.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
			Me.btnRemoveNode.Location = New System.Drawing.Point(12, 526)
			Me.btnRemoveNode.Name = "btnRemoveNode"
			Me.btnRemoveNode.Size = New System.Drawing.Size(120, 23)
			Me.btnRemoveNode.TabIndex = 11
			Me.btnRemoveNode.Text = "Remove Node"
			Me.btnRemoveNode.UseVisualStyleBackColor = True
			AddHandler Me.btnRemoveNode.Click, New System.EventHandler(AddressOf Me.btnRemoveNode_Click)
			' 
			' btnRemoveEdge
			' 
			Me.btnRemoveEdge.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
			Me.btnRemoveEdge.Location = New System.Drawing.Point(141, 526)
			Me.btnRemoveEdge.Name = "btnRemoveEdge"
			Me.btnRemoveEdge.Size = New System.Drawing.Size(120, 23)
			Me.btnRemoveEdge.TabIndex = 13
			Me.btnRemoveEdge.Text = "Remove Edge"
			Me.btnRemoveEdge.UseVisualStyleBackColor = True
			AddHandler Me.btnRemoveEdge.Click, New System.EventHandler(AddressOf Me.btnRemoveEdge_Click)
			' 
			' btnChangeProperties
			' 
			Me.btnChangeProperties.Location = New System.Drawing.Point(552, 27)
			Me.btnChangeProperties.Name = "btnChangeProperties"
			Me.btnChangeProperties.Size = New System.Drawing.Size(127, 23)
			Me.btnChangeProperties.TabIndex = 9
			Me.btnChangeProperties.Text = "Change Properties"
			Me.btnChangeProperties.UseVisualStyleBackColor = True
			AddHandler Me.btnChangeProperties.Click, New System.EventHandler(AddressOf Me.btnChangeProperties_Click)
			' 
			' pDrawPanel
			' 
			Me.pDrawPanel.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
			Me.pDrawPanel.BackColor = System.Drawing.Color.FromArgb(CInt(CByte(224)), CInt(CByte(224)), CInt(CByte(224)))
			Me.pDrawPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
			Me.pDrawPanel.Location = New System.Drawing.Point(268, 79)
			Me.pDrawPanel.Name = "pDrawPanel"
			Me.pDrawPanel.Size = New System.Drawing.Size(411, 470)
			Me.pDrawPanel.TabIndex = 17
			AddHandler Me.pDrawPanel.MouseDown, New System.Windows.Forms.MouseEventHandler(AddressOf Me.pDrawPanel_MouseDown)
			AddHandler Me.pDrawPanel.MouseMove, New System.Windows.Forms.MouseEventHandler(AddressOf Me.pDrawPanel_MouseMove)
			AddHandler Me.pDrawPanel.MouseUp, New System.Windows.Forms.MouseEventHandler(AddressOf Me.pDrawPanel_MouseUp)
			' 
			' btnLoad
			' 
			Me.btnLoad.Location = New System.Drawing.Point(12, 50)
			Me.btnLoad.Name = "btnLoad"
			Me.btnLoad.Size = New System.Drawing.Size(129, 23)
			Me.btnLoad.TabIndex = 18
			Me.btnLoad.Text = "Load from XML..."
			Me.btnLoad.UseVisualStyleBackColor = True
			AddHandler Me.btnLoad.Click, New System.EventHandler(AddressOf Me.btnLoad_Click)
			' 
			' btnClear
			' 
			Me.btnClear.Location = New System.Drawing.Point(147, 50)
			Me.btnClear.Name = "btnClear"
			Me.btnClear.Size = New System.Drawing.Size(75, 23)
			Me.btnClear.TabIndex = 19
			Me.btnClear.Text = "Clear"
			Me.btnClear.UseVisualStyleBackColor = True
			AddHandler Me.btnClear.Click, New System.EventHandler(AddressOf Me.btnClear_Click)
			' 
			' ForceDirectedGraphForm
			' 
			Me.AutoScaleDimensions = New System.Drawing.SizeF(7F, 12F)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.BackColor = System.Drawing.Color.FromArgb(CInt(CByte(235)), CInt(CByte(235)), CInt(CByte(235)))
			Me.ClientSize = New System.Drawing.Size(691, 561)
			Me.Controls.Add(Me.btnClear)
			Me.Controls.Add(Me.btnLoad)
			Me.Controls.Add(Me.btnChangeProperties)
			Me.Controls.Add(Me.pDrawPanel)
			Me.Controls.Add(Me.btnRemoveEdge)
			Me.Controls.Add(Me.btnRemoveNode)
			Me.Controls.Add(Me.lbEdge)
			Me.Controls.Add(Me.lbNode)
			Me.Controls.Add(Me.btnAddEdge)
			Me.Controls.Add(Me.label5)
			Me.Controls.Add(Me.cbbToNode)
			Me.Controls.Add(Me.btnAddNode)
			Me.Controls.Add(Me.label4)
			Me.Controls.Add(Me.tbNodeName)
			Me.Controls.Add(Me.cbbFromNode)
			Me.Controls.Add(Me.label3)
			Me.Controls.Add(Me.tbDamping)
			Me.Controls.Add(Me.label2)
			Me.Controls.Add(Me.tbRepulsion)
			Me.Controls.Add(Me.label1)
			Me.Controls.Add(Me.tbStiffness)
			Me.DoubleBuffered = True
			Me.Name = "ForceDirectedGraphForm"
			Me.Text = "EpForceDirectedGraph.cs Demo"
			AddHandler Me.Paint, New System.Windows.Forms.PaintEventHandler(AddressOf Me.ForceDirectedGraph_Paint)
			Me.ResumeLayout(False)
			Me.PerformLayout()

		End Sub

		#End Region

		Private tbStiffness As System.Windows.Forms.TextBox
		Private label1 As System.Windows.Forms.Label
		Private label2 As System.Windows.Forms.Label
		Private tbRepulsion As System.Windows.Forms.TextBox
		Private label3 As System.Windows.Forms.Label
		Private tbDamping As System.Windows.Forms.TextBox
		Private cbbFromNode As System.Windows.Forms.ComboBox
		Private label4 As System.Windows.Forms.Label
		Private tbNodeName As System.Windows.Forms.TextBox
		Private btnAddNode As System.Windows.Forms.Button
		Private cbbToNode As System.Windows.Forms.ComboBox
		Private label5 As System.Windows.Forms.Label
		Private btnAddEdge As System.Windows.Forms.Button
		Private lbNode As System.Windows.Forms.ListBox
		Private lbEdge As System.Windows.Forms.ListBox
		Private btnRemoveNode As System.Windows.Forms.Button
		Private btnRemoveEdge As System.Windows.Forms.Button
		Private btnChangeProperties As System.Windows.Forms.Button
		Private pDrawPanel As DoubleBufferPanel
		Private btnLoad As System.Windows.Forms.Button
		Private btnClear As System.Windows.Forms.Button
	End Class
End Namespace

