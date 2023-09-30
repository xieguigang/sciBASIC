Imports System

Namespace Boids.Viewer
    Partial Class Form2
        ''' <summary>
        ''' Required designer variable.
        ''' </summary>
        Private components As ComponentModel.IContainer = Nothing

        ''' <summary>
        ''' Clean up any resources being used.
        ''' </summary>
        ''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
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
            components = New ComponentModel.Container()
            pictureBox1 = New Windows.Forms.PictureBox()
            timer1 = New Windows.Forms.Timer(components)
            CType(pictureBox1, ComponentModel.ISupportInitialize).BeginInit()
            SuspendLayout()
            ' 
            ' pictureBox1
            ' 
            pictureBox1.BackColor = Drawing.Color.Purple
            pictureBox1.Dock = Windows.Forms.DockStyle.Fill
            pictureBox1.Location = New Drawing.Point(0, 0)
            pictureBox1.Name = "pictureBox1"
            pictureBox1.Size = New Drawing.Size(584, 361)
            pictureBox1.TabIndex = 0
            pictureBox1.TabStop = False
            AddHandler pictureBox1.SizeChanged, New EventHandler(AddressOf pictureBox1_SizeChanged)
            AddHandler pictureBox1.Click, New EventHandler(AddressOf pictureBox1_Click)
            ' 
            ' timer1
            ' 
            timer1.Enabled = True
            timer1.Interval = 1
            AddHandler timer1.Tick, New EventHandler(AddressOf timer1_Tick)
            ' 
            ' Form2
            ' 
            AutoScaleDimensions = New Drawing.SizeF(6.0F, 13.0F)
            AutoScaleMode = Windows.Forms.AutoScaleMode.Font
            ClientSize = New Drawing.Size(584, 361)
            Controls.Add(pictureBox1)
            Name = "Form2"
            StartPosition = Windows.Forms.FormStartPosition.CenterScreen
            Text = "Boids - C# Data Visualization"
            CType(pictureBox1, ComponentModel.ISupportInitialize).EndInit()
            ResumeLayout(False)

        End Sub

#End Region

        Private pictureBox1 As Windows.Forms.PictureBox
        Private timer1 As Windows.Forms.Timer
    End Class
End Namespace
