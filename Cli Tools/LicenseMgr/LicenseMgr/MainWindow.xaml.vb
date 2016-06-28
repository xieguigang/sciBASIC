Imports FirstFloor.ModernUI.Windows.Controls
'Imports Microsoft.VisualBasic.Windows.Forms

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow
    Inherits ModernWindow
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub ModernWindow_Initialized(sender As Object, e As EventArgs)
        '  If VistaSecurity.IsAdmin() Then
        '  Me.Title += " (Elevated)"
        '  End If
    End Sub
End Class
