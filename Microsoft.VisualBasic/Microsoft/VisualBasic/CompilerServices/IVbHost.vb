Imports System
Imports System.ComponentModel
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public Interface IVbHost
        ' Methods
        Function GetParentWindow() As IWin32Window
        Function GetWindowTitle() As String
    End Interface
End Namespace

