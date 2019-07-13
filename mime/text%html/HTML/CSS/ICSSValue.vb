#Region "Microsoft.VisualBasic::849619d95cb6d2ebe6b575a201507728, mime\text%html\HTML\CSS\ICSSValue.vb"

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

    '     Class ICSSValue
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace HTML.CSS

    Public MustInherit Class ICSSValue

        Public MustOverride ReadOnly Property CSSValue As String

        Public Overrides Function ToString() As String
            Return CSSValue
        End Function
    End Class
End Namespace
