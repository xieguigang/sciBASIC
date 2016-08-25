#Region "Microsoft.VisualBasic::dc5252ecf6f2bb70c9cea68e7a42baaa, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\SongeListItem.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

'Public Class SongeListItem : Inherits ListControlItem
'    Friend WithEvents RatingBar2 As ListControlProject_Example.RatingBar
'    Friend WithEvents Label1 As System.Windows.Forms.Label

'    Public Property Rating() As Integer
'        Get
'            Return RatingBar2.Stars
'        End Get
'        Set(ByVal value As Integer)
'            RatingBar2.Stars = value
'        End Set
'    End Property

'    Dim mSong As String = "[Song Name]"
'    Public Property Song() As String
'        Get
'            Return mSong
'        End Get
'        Set(ByVal value As String)
'            mSong = value
'            Refresh()
'        End Set
'    End Property

'    Dim mArtist As String = "[Artist]"
'    Public Property Artist() As String
'        Get
'            Return mArtist
'        End Get
'        Set(ByVal value As String)
'            mArtist = value
'            Refresh()
'        End Set
'    End Property

'    Dim mAlbum As String = "[Album]"
'    Public Property Album() As String
'        Get
'            Return mAlbum
'        End Get
'        Set(ByVal value As String)
'            mAlbum = value
'            Refresh()
'        End Set
'    End Property

'    Dim mDuration As String
'    Public Property Duration() As String
'        Get
'            Return Me.Label1.Text
'        End Get
'        Set(ByVal value As String)
'            Me.Label1.Text = value
'        End Set
'    End Property

'    Private Sub InitializeComponent()
'        Me.Label1 = New System.Windows.Forms.Label()
'        Me.RatingBar2 = New ListControlProject_Example.RatingBar()
'        Me.SuspendLayout()
'        '
'        'Label1
'        '
'        Me.Label1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
'        Me.Label1.AutoSize = True
'        Me.Label1.BackColor = System.Drawing.Color.Transparent
'        Me.Label1.Location = New System.Drawing.Point(433, 34)
'        Me.Label1.Name = "Label1"
'        Me.Label1.Size = New System.Drawing.Size(39, 17)
'        Me.Label1.TabIndex = 5
'        Me.Label1.Text = "00:00"
'        '
'        'RatingBar2
'        '
'        Me.RatingBar2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
'        Me.RatingBar2.BackColor = System.Drawing.Color.Transparent
'        Me.RatingBar2.Location = New System.Drawing.Point(397, 10)
'        Me.RatingBar2.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
'        Me.RatingBar2.MaximumSize = New System.Drawing.Size(75, 15)
'        Me.RatingBar2.MinimumSize = New System.Drawing.Size(75, 15)
'        Me.RatingBar2.Name = "RatingBar2"
'        Me.RatingBar2.Size = New System.Drawing.Size(75, 15)
'        Me.RatingBar2.Stars = 3
'        Me.RatingBar2.TabIndex = 4
'        '
'        'SongeListItem
'        '
'        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 17.0!)
'        Me.Controls.Add(Me.Label1)
'        Me.Controls.Add(Me.RatingBar2)
'        Me.Name = "SongeListItem"
'        Me.Controls.SetChildIndex(Me.RatingBar2, 0)
'        Me.Controls.SetChildIndex(Me.Label1, 0)
'        Me.ResumeLayout(False)
'        Me.PerformLayout()

'    End Sub

'    Protected Overrides Sub Paint_DrawButton(gfx As Graphics)

'        Dim fnt As Font = Nothing
'        Dim sz As SizeF = Nothing
'        Dim layoutRect As RectangleF
'        Dim SF As New StringFormat With {.Trimming = StringTrimming.EllipsisCharacter}
'        Dim workingRect As New Rectangle(40, 0, RatingBar2.Left - 40 - 6, Me.Height)

'        ' Draw song name
'        fnt = New Font("Segoe UI Light", 14)
'        sz = gfx.MeasureString(mSong, fnt)
'        layoutRect = New RectangleF(40, 0, workingRect.Width, sz.Height)
'        gfx.DrawString(mSong, fnt, Brushes.Black, layoutRect, SF)

'        ' Draw artist name
'        fnt = New Font("Segoe UI Light", 10)
'        sz = gfx.MeasureString(mArtist, fnt)
'        layoutRect = New RectangleF(42, 30, workingRect.Width, sz.Height)
'        gfx.DrawString(mArtist, fnt, Brushes.Black, layoutRect, SF)

'        ' Draw album name
'        fnt = New Font("Segoe UI Light", 10)
'        sz = gfx.MeasureString(mAlbum, fnt)
'        layoutRect = New RectangleF(42, 49, workingRect.Width, sz.Height)
'        gfx.DrawString(mAlbum, fnt, Brushes.Black, layoutRect, SF)

'        Call MyBase.Paint_DrawButton(gfx)
'    End Sub

'End Class
