Imports System.Windows.Controls

Namespace Content
    ''' <summary>
    ''' Interaction logic for SettingsAppearance.xaml
    ''' </summary>
    Public Class SettingsAppearance
        Inherits UserControl
        Public Sub New()
            InitializeComponent()

            ' a simple view model for appearance configuration
            Me.DataContext = New SettingsAppearanceViewModel()
        End Sub
    End Class
End Namespace
