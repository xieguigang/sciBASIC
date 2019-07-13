#Region "Microsoft.VisualBasic::06c7574d3cd53920f317b43465ce1d60, vs_solutions\installer\Installer\FormTemplate.vb"

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

    ' Class FormTemplate
    ' 
    '     Sub: ButtonNext_Click, Highlight
    ' 
    ' /********************************************************************************/

#End Region

Public Class FormTemplate

    Public Shared ReadOnly TemplateColor As Color = Color.FromArgb(0, 99, 177)

    Protected Overridable Sub ButtonNext_Click(sender As Object, e As EventArgs) Handles ButtonNext.Click

    End Sub

    Public Sub Highlight(label As Label)
        label.BackColor = Color.FromArgb(34, 118, 173)
        label.ForeColor = Color.White
    End Sub
End Class
