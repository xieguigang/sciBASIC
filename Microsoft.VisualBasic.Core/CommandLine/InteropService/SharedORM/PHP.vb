#Region "Microsoft.VisualBasic::0288d0bb5b1e1276578f59b45c3a32af, Microsoft.VisualBasic.Core\CommandLine\InteropService\SharedORM\PHP.vb"

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

    '     Class PHP
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetSourceCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CommandLine.InteropService.SharedORM

    Public Class PHP : Inherits CodeGenerator

        Public Sub New(CLI As Type)
            MyBase.New(CLI)
        End Sub

        Public Overrides Function GetSourceCode() As String
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
