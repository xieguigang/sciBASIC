Imports System.Text
Imports System.Runtime.CompilerServices

Module Common
    ''' <summary>
    ''' The first element that in a list or array object.(列表中的第一个元素)
    ''' </summary>
    ''' <remarks></remarks>
    Public Const Scan0 As Integer = 0

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="expression"></param>
    ''' <param name="dict"></param>
    ''' <param name="varList"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Replace(expression As String, dict As Dictionary(Of String, String), varList As String()) As String
        Dim IsVariable As System.Func(Of Char, Char, Boolean) =
            Function(A As Char, B As Char) _
                InStr(Helpers.Constants.LEFT_OPERATOR_TOKENS, A) AndAlso InStr(Helpers.Constants.RIGHT_OPERATOR_TOKENS, B)
        expression = String.Format("0+{0}+0", expression)
        Dim sBuilder As StringBuilder = New StringBuilder(expression)

        For Each var In varList
            Dim p As Integer = InStr(expression, var)
            Dim l As Integer = Len(var) 'The length of the constant name
            Dim value As String = dict(var)

            Do While p
                Dim Right As Char = expression(p + l - 1)
                Dim Left As Char = expression(p - 2)

                If IsVariable(Left, Right) Then 'If this tokens is surrounded by two operators then it's a constant name, 
                    'not part of the function name or other user define 
                    'constant name.
                    expression = Mid(expression, p - 1, l + 2)
                    Call sBuilder.Replace(expression, Left & value & Right)
                    expression = sBuilder.ToString

                    p = InStr(p + Len(value), expression, var)
                Else
                    p = InStr(p + l, expression, var)
                End If
            Loop
        Next

        Return sBuilder.ToString
    End Function
End Module
