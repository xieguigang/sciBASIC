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
