#Region "Microsoft.VisualBasic::0960e1dc32ec5fdb0f8798145f67d96f, mime\text%yaml\1.2\Syntax\NodeProperty.vb"

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

    '     Class NodeProperty
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Syntax

    Public Class NodeProperty

        Public Tag As Tag
        Public Anchor As String

        Public Overrides Function ToString() As String
            Return $"({Anchor}) {Tag.ToString}"
        End Function
    End Class
End Namespace
