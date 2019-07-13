#Region "Microsoft.VisualBasic::5386687f6a3b1945115dc69dea2fbb24, vs_solutions\dev\LicenseMgr\LicenseMgr\Content\SettingsAppearance.xaml.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class SettingsAppearance
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
