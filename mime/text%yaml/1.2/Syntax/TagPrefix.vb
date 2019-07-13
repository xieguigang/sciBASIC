#Region "Microsoft.VisualBasic::72072e9b61fb1a9b5bafc5755fd8bedb, mime\text%yaml\1.2\Syntax\TagPrefix.vb"

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

    '     Class TagPrefix
    ' 
    '         Function: ToString
    ' 
    '     Class GlobalTagPrefix
    ' 
    ' 
    ' 
    '     Class LocalTagPrefix
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Syntax

    Public Class TagPrefix

        Public Prefix As New List(Of Char)()

        Public Overrides Function ToString() As String
            Return $"<{Me.GetType.Name}> {Prefix.CharString}"
        End Function
    End Class

    Public Class GlobalTagPrefix
        Inherits TagPrefix
    End Class

    Public Class LocalTagPrefix
        Inherits TagPrefix
    End Class
End Namespace
