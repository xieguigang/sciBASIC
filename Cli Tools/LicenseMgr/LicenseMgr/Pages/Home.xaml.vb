Imports System.Windows
Imports System.Windows.Controls
'Imports Microsoft.VisualBasic.Windows.Forms

Namespace Pages

    ''' <summary>
    ''' Interaction logic for Home.xaml
    ''' </summary>
    Partial Public Class Home
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Admin_Rights(sender As Object, e As RoutedEventArgs)
            '   VistaSecurity.RestartElevated()
        End Sub

        Private Sub UAC_Initialized(sender As Object, e As EventArgs)
            'If VistaSecurity.IsAdmin() Then
            '    UAC.Visibility = System.Windows.Visibility.Collapsed
            '    UACNote.Visibility = System.Windows.Visibility.Collapsed
            'End If
        End Sub
    End Class
End Namespace
