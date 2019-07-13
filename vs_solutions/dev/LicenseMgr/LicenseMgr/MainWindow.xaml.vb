#Region "Microsoft.VisualBasic::19cb1e767ff1e798f4b608f4f4e83f72, vs_solutions\dev\LicenseMgr\LicenseMgr\MainWindow.xaml.vb"

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

    ' Class MainWindow
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: ModernWindow_Initialized
    ' 
    ' /********************************************************************************/

#End Region

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
