Imports Microsoft.VisualBasic.Math.Scripting

Public Class Config

    Public Property learnRate As Double = 0.1
    Public Property momentum As Double = 0.9
    Public Property iterations As Integer = 10000

    ''' <summary>
    ''' ``func(args)``, using parser <see cref="FuncParser.TryParse(String)"/>
    ''' </summary>
    ''' <returns></returns>
    Public Property default_active As String = "Sigmoid()"

    Public Property input_active As String
    Public Property hiddens_active As String
    Public Property output_active As String

End Class
