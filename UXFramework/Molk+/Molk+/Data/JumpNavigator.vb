#Region "Microsoft.VisualBasic::d5e017798bbad72595e508960a0902b2, ..\visualbasic_App\UXFramework\Molk+\Molk+\Data\JumpNavigator.vb"

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

Public Class JumpNavigator

    Public Event StartNavigation(Index As String)

    Public Property IndexList As String()
        Get
            Return _IndexBuffer
        End Get
        Set(value As String())
            _IndexBuffer = value
            Call InitialIndex()
        End Set
    End Property

    Public Property IndexSize As Size

    Dim _IndexBuffer As String()

    Private Sub InitialIndex()
        Call Me.FlowLayoutPanel1.Controls.Clear()

        If _IndexBuffer.IsNullOrEmpty Then
            Return
        End If

        Dim LQuery = (From i As Integer
                      In _IndexBuffer.Sequence.AsParallel
                      Let IndexLabel As String = _IndexBuffer(i)
                      Select i, idx = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Checkbox With
                          {.LabelText = IndexLabel}
                      Order By i Ascending).ToArray
        Dim render = New PNButton

        If IndexSize.Height = 0 OrElse IndexSize.Width = 0 Then
            IndexSize = New Size(24, 24)
        End If

        For Each IndexValue In LQuery
            Call Me.FlowLayoutPanel1.Controls.Add(IndexValue.idx)
            Call render.RenderButton(IndexValue.idx, IndexSize, Font)

            AddHandler IndexValue.idx.CheckStateChanged, Sub() Call __StartNavigation(IndexValue.idx)
        Next

        _IndexEntryBuffer = (From obj In LQuery Select obj.idx).ToArray
    End Sub

    Dim _IndexEntryBuffer As Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Checkbox()
    Dim _PreChecked As Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Checkbox

    Private Sub __StartNavigation(idx As Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.Checkbox)
        If Not _PreChecked Is Nothing Then
            _PreChecked.Checked = False
        End If

        _PreChecked = idx

        RaiseEvent StartNavigation(idx.Text)
    End Sub

End Class
