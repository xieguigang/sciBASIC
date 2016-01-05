Imports System.ComponentModel

Namespace Windows.Forms.Controls.TabControl.TabLabel

    Public Class TabControl : Inherits Windows.Forms.Controls.TabControl.ITabControl(Of Windows.Forms.Controls.TabControl.TabLabel.TabLabel)

        Dim _TabLabelWidth As Integer

        <DefaultValue(30)>
        Public Property TabLabelWidth As Integer
            Get
                Return Me._TabLabelWidth
            End Get
            Set(value As Integer)
                Me._TabLabelWidth = value
            End Set
        End Property

        Public Overrides Sub AddTabPage(Name As String, Control As Control, Optional TabCloseEventHandle As Action = Nothing)
            Dim Tab = MyBase.InternalAddTabPage(Name, Control, TabCloseEventHandle)
            Dim NewPanel = Tab._InternalTabPageControlItem

            NewPanel.Size = New Size With {.Width = Width - _TabLabelWidth - 5, .Height = Height}
            NewPanel.Location = New Point With {.X = _TabLabelWidth + 5, .Y = 0}
            NewPanel.BringToFront()

            Tab.Size = New Size With {.Width = _TabLabelWidth, .Height = Tab.Height}
            Tab.Location = New Point With {.X = 0, .Y = Tab.Height * (MyBase._Tabs - 1) + 5}

            Call Me.ActiveTabPage(Name)
        End Sub
    End Class
End Namespace