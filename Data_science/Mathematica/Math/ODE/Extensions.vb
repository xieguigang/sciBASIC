#Region "Microsoft.VisualBasic::a813ff39dbc3234e2bf11e9e7eb0fd80, Data_science\Mathematica\Math\ODE\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: Let, Solve
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

<HideModuleName> Public Module Extensions

    ''' <summary>
    ''' Solve the target ODEs dynamics system by using the RK4 solver.
    ''' </summary>
    ''' <param name="system"></param>
    ''' <returns></returns>
    <Extension> Public Function Solve(system As IEnumerable(Of var), dt As (from#, to#, step#)) As ODEsOut
        Dim vector As var() = system.ToArray
        Dim df = Sub(dx#, ByRef dy As Vector)
                     For Each x As var In vector
                         dy(x) = x.Evaluate()
                     Next
                 End Sub
        Dim ODEs As New GenericODEs(system.ToArray, df)

        With dt
            Dim result As ODEsOut = ODEs.Solve((.to - .from) / .step, .from, .to)
            Return result
        End With
    End Function

    ''' <summary>
    ''' Create VisualBasic variables.(使用这个函数来进行初始化是为了在赋值的同时还对新创建的对象赋予名称，方便将结果写入数据集)
    ''' </summary>
    ''' <param name="list"></param>
    ''' <returns></returns>
    <Extension> Public Function Let$(list As Expression(Of Func(Of var())))
        Dim unaryExpression As NewArrayExpression = DirectCast(list.Body, NewArrayExpression)
        Dim arrayData = unaryExpression _
            .Expressions _
            .Select(Function(b) DirectCast(b, BinaryExpression)) _
            .ToArray
        Dim var As New Dictionary(Of String, Double)

        For Each expr As BinaryExpression In arrayData
            Dim member = DirectCast(expr.Left, MemberExpression)
            Dim name As String = member.Member.Name.Replace("$VB$Local_", "")
            Dim field As FieldInfo = DirectCast(member.Member, FieldInfo)
            Dim value As Object = DirectCast(expr.Right, ConstantExpression).Value
            Dim obj = DirectCast(member.Expression, ConstantExpression).Value

            Call field.SetValue(obj, New var(name, CDbl(value)))
        Next

        Return var.GetJson
    End Function
End Module
