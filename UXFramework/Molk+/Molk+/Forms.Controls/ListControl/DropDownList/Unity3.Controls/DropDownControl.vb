#Region "Microsoft.VisualBasic::8b8c7c5fd49a24ef52237e4fa85a522c, ..\VisualBasic_AppFramework\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\Unity3.Controls\DropDownControl.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.ComponentModel
Imports System.Text
Imports System.Windows.Forms
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace Unity3.Controls
    Partial Public Class DropDownControl : Inherits UserControl

        Public Enum DockSides
            Left
            Right
        End Enum

        Public Enum DropStates
            Closed
            Closing
            Dropping
            Dropped
        End Enum

        Dim dropContainer As DropDownContainer
        Dim _dropDownItem As Control
        Dim closedWhileInControl As Boolean
        Dim storedSize As Size
        Friend WithEvents TextBox1 As TextBox
        Protected ReadOnly Property DropState() As DropStates

        Dim _Text As String
        Public Overrides Property Text() As String
            Get
                Return _Text
            End Get
            Set
                _Text = Value
                Me.Invalidate()
            End Set
        End Property

        Public Sub New()
            InitializeComponent()

            Me.storedSize = Me.Size
            Me.BackColor = Color.White
            Me.Text = Me.Name
        End Sub

        Public Sub InitializeDropDown(dropDownItem As Control)
            If _dropDownItem IsNot Nothing Then
                Throw New Exception("The drop down item has already been implemented!")
            End If
            _DesignView = False
            _DropState = DropStates.Closed
            Me.Size = New Size(237, 26) ' _AnchorSize
            Me._AnchorClientBounds = New Rectangle(2, 2, Size.Width - 21, Size.Height - 4)
            'removes the dropDown item from the controls list so it 
            'won't be seen until the drop-down window is active
            If Me.Controls.Contains(dropDownItem) Then
                Me.Controls.Remove(dropDownItem)
            End If
            _dropDownItem = dropDownItem

            Call Update()
        End Sub

        'Dim _AnchorSize As New Size(235, 21)
        'Public Property AnchorSize() As Size
        '    Get
        '        Return _AnchorSize
        '    End Get
        '    Set
        '        _AnchorSize = Value
        '        Me.Invalidate()
        '    End Set
        'End Property

        Public Property DockSide() As DockSides

        Dim _DesignView As Boolean = True

        <DefaultValue(False)>
        Protected Property DesignView() As Boolean
            Get
                Return _DesignView
            End Get
            Set
                If _DesignView = Value Then
                    Return
                End If

                _DesignView = Value
                If _DesignView Then
                    Me.Size = storedSize
                Else
                    storedSize = Me.Size
                    ' Me.Size = _AnchorSize

                End If
            End Set
        End Property

        Public Event PropertyChanged As EventHandler
        Protected Sub OnPropertyChanged()
            RaiseEvent PropertyChanged(Nothing, Nothing)
        End Sub

        Public ReadOnly Property AnchorClientBounds() As Rectangle

        Protected Overrides Sub OnResize(e As EventArgs)
            'MyBase.OnResize(e)
            'If _DesignView Then
            '    storedSize = Me.Size
            'End If
            '_AnchorSize.Width = Me.Width
            'If Not _DesignView Then
            '    _AnchorSize.Height = Me.Height
            '    Me._AnchorClientBounds = New Rectangle(2, 2, _AnchorSize.Width - 21, _AnchorSize.Height - 4)
            'End If
        End Sub

        Protected mousePressed As Boolean

        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            MyBase.OnMouseDown(e)
            mousePressed = True
            OpenDropDown()
        End Sub

        Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
            MyBase.OnMouseUp(e)
            mousePressed = False
            Me.Invalidate()
        End Sub

        Protected Overridable ReadOnly Property CanDrop() As Boolean
            Get
                If dropContainer IsNot Nothing Then
                    Return False
                End If

                If dropContainer Is Nothing AndAlso closedWhileInControl Then
                    closedWhileInControl = False
                    Return False
                End If

                Return Not closedWhileInControl
            End Get
        End Property

        Protected Sub OpenDropDown()
            If _dropDownItem Is Nothing Then
                Throw New NotImplementedException("The drop down item has not been initialized!  Use the InitializeDropDown() method to do so.")
            End If

            If Not CanDrop Then
                Return
            End If

            dropContainer = New DropDownContainer(_dropDownItem)
            dropContainer.Bounds = GetDropDownBounds()
            AddHandler dropContainer.DropStateChange, New DropDownContainer.DropWindowArgs(AddressOf dropContainer_DropStateChange)
            AddHandler dropContainer.FormClosed, New FormClosedEventHandler(AddressOf dropContainer_Closed)
            AddHandler Me.ParentForm.Move, New EventHandler(AddressOf ParentForm_Move)
            _DropState = DropStates.Dropping
            dropContainer.Show(Me)
            _DropState = DropStates.Dropped
            Me.Invalidate()
        End Sub

        Private Sub ParentForm_Move(sender As Object, e As EventArgs)
            dropContainer.Bounds = GetDropDownBounds()
        End Sub


        Public Sub CloseDropDown()

            If dropContainer IsNot Nothing Then
                _DropState = DropStates.Closing
                dropContainer.Freeze = False
                dropContainer.Close()
            End If
        End Sub

        Private Sub dropContainer_DropStateChange(state As DropDownControl.DropStates)
            _DropState = state
        End Sub
        Private Sub dropContainer_Closed(sender As Object, e As FormClosedEventArgs)
            If Not dropContainer.IsDisposed Then
                RemoveHandler dropContainer.DropStateChange, AddressOf dropContainer_DropStateChange
                RemoveHandler dropContainer.FormClosed, AddressOf dropContainer_Closed
                RemoveHandler Me.ParentForm.Move, AddressOf ParentForm_Move
                dropContainer.Dispose()
            End If
            dropContainer = Nothing
            closedWhileInControl = (Me.RectangleToScreen(Me.ClientRectangle).Contains(Cursor.Position))
            _DropState = DropStates.Closed
            Me.Invalidate()
        End Sub

        Protected Overridable Function GetDropDownBounds() As Rectangle
            Dim inflatedDropSize As New Size(_dropDownItem.Width + 2, _dropDownItem.Height + 2)
            Dim screenBounds As Rectangle = If(_DockSide = DockSides.Left, New Rectangle(Me.Parent.PointToScreen(New Point(Me.Bounds.X, Me.Bounds.Bottom)), inflatedDropSize), New Rectangle(Me.Parent.PointToScreen(New Point(Me.Bounds.Right - _dropDownItem.Width, Me.Bounds.Bottom)), inflatedDropSize))
            Dim workingArea As Rectangle = Screen.GetWorkingArea(screenBounds)
            'make sure we're completely in the top-left working area
            If screenBounds.X < workingArea.X Then
                screenBounds.X = workingArea.X
            End If
            If screenBounds.Y < workingArea.Y Then
                screenBounds.Y = workingArea.Y
            End If

            'make sure we're not extended past the working area's right /bottom edge
            If screenBounds.Right > workingArea.Right AndAlso workingArea.Width > screenBounds.Width Then
                screenBounds.X = workingArea.Right - screenBounds.Width
            End If
            If screenBounds.Bottom > workingArea.Bottom AndAlso workingArea.Height > screenBounds.Height Then
                screenBounds.Y = workingArea.Bottom - screenBounds.Height
            End If

            Return screenBounds
        End Function

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)

            Dim res As Image = Nothing

            'Check if VisualStyles are supported...
            'Thanks to codeproject member: Mathiyazhagan for catching this. :)
            If ComboBoxRenderer.IsSupported Then
                ' ComboBoxRenderer.DrawTextBox(e.Graphics, New Rectangle(New Point(0, 0), Size), getState())
                Select Case getState()
                    Case VisualStyles.ComboBoxState.Disabled
                        res = DropDownUI.InSensitive
                    Case VisualStyles.ComboBoxState.Hot
                        res = DropDownUI.PreLight
                    Case VisualStyles.ComboBoxState.Normal
                        res = DropDownUI.Normal
                    Case VisualStyles.ComboBoxState.Pressed
                        res = DropDownUI.Active
                End Select
                ' ComboBoxRenderer.DrawDropDownButton(e.Graphics, New Rectangle(Size.Width - 19, 2, 18, Size.Height - 4), getState())
            Else
                res = If(Me.Enabled, DropDownUI.Normal, DropDownUI.InSensitive)

                ' ControlPaint.DrawComboButton(e.Graphics, New Rectangle(Size.Width - 19, 2, 18, Size.Height - 4), If((Me.Enabled), ButtonState.Normal, ButtonState.Inactive))
            End If

            e.Graphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality

            Using b As Brush = New SolidBrush(Me.BackColor)
                e.Graphics.FillRectangle(b, Me.AnchorClientBounds)
            End Using

            Call e.Graphics.DrawImage(res, Size.Width - res.Width - 2, CSng(Height - res.Height) / 2, res.Width, res.Height)
            TextBox1.Text = Text
            '  TextRenderer.DrawText(e.Graphics, _Text, Me.Font, Me.AnchorClientBounds, Me.ForeColor, TextFormatFlags.WordEllipsis)
        End Sub

        Private Function getState() As System.Windows.Forms.VisualStyles.ComboBoxState
            If mousePressed OrElse dropContainer IsNot Nothing Then
                Return System.Windows.Forms.VisualStyles.ComboBoxState.Pressed
            Else
                Return System.Windows.Forms.VisualStyles.ComboBoxState.Normal
            End If
        End Function

        Public Sub FreezeDropDown(remainVisible As Boolean)
            If dropContainer IsNot Nothing Then
                dropContainer.Freeze = True
                If Not remainVisible Then
                    dropContainer.Visible = False
                End If
            End If
        End Sub

        Public Sub UnFreezeDropDown()
            If dropContainer IsNot Nothing Then
                dropContainer.Freeze = False
                If Not dropContainer.Visible Then
                    dropContainer.Visible = True
                End If
            End If
        End Sub

        Friend NotInheritable Class DropDownContainer
            Inherits Form
            Implements IMessageFilter
            Public Freeze As Boolean


            Public Sub New(dropDownItem As Control)
                Me.FormBorderStyle = FormBorderStyle.None
                dropDownItem.Location = New Point(1, 1)
                Me.Controls.Add(dropDownItem)
                Me.StartPosition = FormStartPosition.Manual
                Me.ShowInTaskbar = False
                Application.AddMessageFilter(Me)
            End Sub

            Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
                If Not Freeze AndAlso Me.Visible AndAlso (Form.ActiveForm Is Nothing OrElse Not Form.ActiveForm.Equals(Me)) Then
                    OnDropStateChange(DropStates.Closing)
                    Me.Close()
                End If


                Return False
            End Function

            Public Delegate Sub DropWindowArgs(state As DropStates)
            Public Event DropStateChange As DropWindowArgs
            Protected Sub OnDropStateChange(state As DropStates)
                RaiseEvent DropStateChange(state)
            End Sub

            Protected Overrides Sub OnPaint(e As PaintEventArgs)
                MyBase.OnPaint(e)
                e.Graphics.DrawRectangle(Pens.Gray, New Rectangle(0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1))
            End Sub

            Protected Overrides Sub OnClosing(e As CancelEventArgs)
                Application.RemoveMessageFilter(Me)
                Me.Controls.RemoveAt(0)
                'prevent the control from being disposed
                MyBase.OnClosing(e)
            End Sub
        End Class

        Private Sub InitializeComponent()
            Me.TextBox1 = New System.Windows.Forms.TextBox()
            Me.SuspendLayout()
            '
            'TextBox1
            '
            Me.TextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None
            Me.TextBox1.Font = New System.Drawing.Font("Microsoft YaHei", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.TextBox1.Location = New System.Drawing.Point(3, 3)
            Me.TextBox1.Name = "TextBox1"
            Me.TextBox1.Size = New System.Drawing.Size(0, 20)
            Me.TextBox1.TabIndex = 1
            '
            'DropDownControl
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.TextBox1)
            Me.Name = "DropDownControl"
            Me.Size = New System.Drawing.Size(233, 26)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Protected Shared _DropDownUI As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource

        Public Shared ReadOnly Property DropDownUI As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.ButtonResource
            Get
                If _DropDownUI Is Nothing Then
                    _DropDownUI = New Visualise.Elements.ButtonResource

                    Dim Size As New Size(24, 24)
                    _DropDownUI.Normal = DropDownButtonDrawer(Size, state:=0)
                    _DropDownUI.PreLight = DropDownButtonDrawer(Size, state:=1)
                    _DropDownUI.Active = DropDownButtonDrawer(Size, state:=2)
                    _DropDownUI.InSensitive = _DropDownUI.Normal
                End If

                Return _DropDownUI
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="state">Normal, HighLight, Press</param>
        ''' <returns></returns>
        Private Shared Function DropDownButtonDrawer(size As Size, state As Integer) As Image
            Dim graph = size.CreateGDIDevice(Color.White)
            Dim res As Image = If(state = 0, My.Resources.DropDownNormal,
                If(state = 1, My.Resources.DropDownHighLight, My.Resources.DropDownPress))
            Call graph.Graphics.DrawImage(res, New Point((size.Width - res.Width) / 2, (size.Height - res.Height) / 2))

            Return graph.ImageResource
        End Function

        Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
            _Text = TextBox1.Text
        End Sub

        Private Sub DropDownControl_Resize(sender As Object, e As EventArgs) Handles Me.Resize
            TextBox1.Width = Width - Height - 2
        End Sub

        Public Overloads Sub Update()
            Call DropDownControl_Resize(Nothing, Nothing)
        End Sub
    End Class
End Namespace

