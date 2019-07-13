#Region "Microsoft.VisualBasic::3be5c17214536ce52cfb48a329ec834f, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threads\LQuerySchedule\Models.vb"

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

    '     Structure TimeoutModel
    ' 
    '         Function: Invoke
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Parallel.Linq

    Public Structure TimeoutModel(Of out)

        Dim timeout As Double
        Dim task As Func(Of out())

        Public Function Invoke() As out()
            Dim result As out() = Nothing

            If OperationTimeOut(task, result, timeout) Then
                Return Nothing
            Else
                Return result
            End If
        End Function
    End Structure
End Namespace
