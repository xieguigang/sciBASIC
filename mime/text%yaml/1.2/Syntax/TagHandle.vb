#Region "Microsoft.VisualBasic::3be8c22ae75013087ba1f8baa2338ab4, mime\text%yaml\1.2\Syntax\TagHandle.vb"

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

    '     Class TagHandle
    ' 
    ' 
    ' 
    '     Class PrimaryTagHandle
    ' 
    ' 
    ' 
    '     Class SecondaryTagHandle
    ' 
    ' 
    ' 
    '     Class NamedTagHandle
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Syntax

    Public Class TagHandle
    End Class

    Public Class PrimaryTagHandle
        Inherits TagHandle
    End Class

    Public Class SecondaryTagHandle
        Inherits TagHandle
    End Class

    Public Class NamedTagHandle
        Inherits TagHandle

        Public Name As New List(Of Char)()

        Public Overrides Function ToString() As String
            Return Name.CharString
        End Function
    End Class
End Namespace
