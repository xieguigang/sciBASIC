#Region "Microsoft.VisualBasic::a1ee81b93d09d6e9094deac4b0faf157, Data_science\Mathematica\Math\Math\Scripting\Helpers\MetaExpression.vb"

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

    '     Class MetaExpression
    ' 
    '         Properties: [Operator], LEFT
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T">Token type</typeparam>
    ''' <typeparam name="O">Operator type</typeparam>
    Public Class MetaExpression(Of T, O)

        Public Property [Operator] As O

        ''' <summary>
        ''' 自动根据类型来计算出结果
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property LEFT As T

        Sub New()
        End Sub

        Sub New(x As T)
            LEFT = x
        End Sub
    End Class
End Namespace
