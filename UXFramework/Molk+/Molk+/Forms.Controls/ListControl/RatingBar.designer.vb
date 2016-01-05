<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RatingBar
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RatingBar))
        Me.Star1 = New System.Windows.Forms.PictureBox()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.Star2 = New System.Windows.Forms.PictureBox()
        Me.Star3 = New System.Windows.Forms.PictureBox()
        Me.Star4 = New System.Windows.Forms.PictureBox()
        Me.Star5 = New System.Windows.Forms.PictureBox()
        CType(Me.Star1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Star2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Star3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Star4, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Star5, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Star1
        '
        Me.Star1.Cursor = System.Windows.Forms.Cursors.Hand
        Me.Star1.Location = New System.Drawing.Point(0, 0)
        Me.Star1.Margin = New System.Windows.Forms.Padding(0)
        Me.Star1.Name = "Star1"
        Me.Star1.Size = New System.Drawing.Size(13, 13)
        Me.Star1.TabIndex = 0
        Me.Star1.TabStop = False
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "empty")
        Me.ImageList1.Images.SetKeyName(1, "full")
        '
        'Star2
        '
        Me.Star2.Cursor = System.Windows.Forms.Cursors.Hand
        Me.Star2.Location = New System.Drawing.Point(13, 0)
        Me.Star2.Margin = New System.Windows.Forms.Padding(0)
        Me.Star2.Name = "Star2"
        Me.Star2.Size = New System.Drawing.Size(13, 13)
        Me.Star2.TabIndex = 0
        Me.Star2.TabStop = False
        '
        'Star3
        '
        Me.Star3.Cursor = System.Windows.Forms.Cursors.Hand
        Me.Star3.Location = New System.Drawing.Point(26, 0)
        Me.Star3.Margin = New System.Windows.Forms.Padding(0)
        Me.Star3.Name = "Star3"
        Me.Star3.Size = New System.Drawing.Size(13, 13)
        Me.Star3.TabIndex = 0
        Me.Star3.TabStop = False
        '
        'Star4
        '
        Me.Star4.Cursor = System.Windows.Forms.Cursors.Hand
        Me.Star4.Location = New System.Drawing.Point(39, 0)
        Me.Star4.Margin = New System.Windows.Forms.Padding(0)
        Me.Star4.Name = "Star4"
        Me.Star4.Size = New System.Drawing.Size(13, 13)
        Me.Star4.TabIndex = 0
        Me.Star4.TabStop = False
        '
        'Star5
        '
        Me.Star5.Cursor = System.Windows.Forms.Cursors.Hand
        Me.Star5.Location = New System.Drawing.Point(52, 0)
        Me.Star5.Margin = New System.Windows.Forms.Padding(0)
        Me.Star5.Name = "Star5"
        Me.Star5.Size = New System.Drawing.Size(13, 13)
        Me.Star5.TabIndex = 0
        Me.Star5.TabStop = False
        '
        'RatingBar
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Transparent
        Me.Controls.Add(Me.Star5)
        Me.Controls.Add(Me.Star4)
        Me.Controls.Add(Me.Star3)
        Me.Controls.Add(Me.Star2)
        Me.Controls.Add(Me.Star1)
        Me.DoubleBuffered = True
        Me.MaximumSize = New System.Drawing.Size(75, 15)
        Me.MinimumSize = New System.Drawing.Size(75, 15)
        Me.Name = "RatingBar"
        Me.Size = New System.Drawing.Size(75, 15)
        CType(Me.Star1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Star2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Star3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Star4, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Star5, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Star1 As System.Windows.Forms.PictureBox
    Friend WithEvents ImageList1 As System.Windows.Forms.ImageList
    Friend WithEvents Star2 As System.Windows.Forms.PictureBox
    Friend WithEvents Star3 As System.Windows.Forms.PictureBox
    Friend WithEvents Star4 As System.Windows.Forms.PictureBox
    Friend WithEvents Star5 As System.Windows.Forms.PictureBox

End Class
