#Region "Microsoft.VisualBasic::372ca7f6bacc3a4cf907325fc409eaf5, Microsoft.VisualBasic.Core\CommandLine\InteropService\SharedORM\R.vb"

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

    '     Class RLanguage
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetSourceCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CommandLine.InteropService.SharedORM

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>因为会和Unix bash之中的``ls``命令之中的``-r``参数冲突，所以在这里命名为``Rlanguage``</remarks>
    Public Class RLanguage : Inherits CodeGenerator

        Public Sub New(CLI As Type)
            MyBase.New(CLI)
        End Sub

        Public Overrides Function GetSourceCode() As String
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
