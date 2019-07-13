#Region "Microsoft.VisualBasic::b3f75c4df164dd554ab694d461760b5a, mime\text%yaml\1.2\Syntax\Tag.vb"

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

    '     Class Tag
    ' 
    ' 
    ' 
    '     Class ShorthandTag
    ' 
    '         Function: ToString
    ' 
    '     Class VerbatimTag
    ' 
    '         Function: ToString
    ' 
    '     Class NonSpecificTag
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Syntax

    Public Class Tag
    End Class

    Public Class ShorthandTag
        Inherits Tag

        Public Chars As New List(Of Char)()

        Public Overrides Function ToString() As String
            Return $"<{Me.GetType.Name}> {Chars.CharString}"
        End Function
    End Class

    Public Class VerbatimTag
        Inherits Tag

        Public Chars As New List(Of Char)()

        Public Overrides Function ToString() As String
            Return $"<{Me.GetType.Name}> {Chars.CharString}"
        End Function
    End Class

    Public Class NonSpecificTag
        Inherits Tag

        Public Overrides Function ToString() As String
            Return $"<{Me.GetType.Name}>"
        End Function
    End Class
End Namespace
