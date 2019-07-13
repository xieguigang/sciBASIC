#Region "Microsoft.VisualBasic::e20ebf789813fbf19867990055a073ec, mime\text%yaml\1.2\Syntax\YamlVersion.vb"

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

    '     Class YamlVersion
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Syntax

    Public Class YamlVersion

        Public Major As String
        Public Minor As String

        Public Overrides Function ToString() As String
            Return $"{Major}.{Minor}"
        End Function
    End Class
End Namespace
