'Imports System.Text

'Namespace BasicR.Helpers

'    ''' <summary>
'    ''' Matrix calculation define in the programming meanning, not mathematics.
'    ''' (非数学意义上的矩阵运算)
'    ''' </summary>
'    ''' <remarks></remarks>
'    Public Class MatrixArithmetic

'        ''' <summary>
'        ''' 枚举矩阵对象之间的所有操作
'        ''' </summary>
'        ''' <remarks></remarks>
'        Public ReadOnly Arithmetics As Dictionary(Of String, System.Func(Of BasicR.MATRIX, BasicR.MATRIX, BasicR.MATRIX)) =
'            New Dictionary(Of String, Func(Of BasicR.MATRIX, BasicR.MATRIX, BasicR.MATRIX)) From {
'                {"+", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a + b},
'                {"-", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a - b},
'                {"*", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a * b},
'                {"/", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a / b},
'                {"\", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a \ b},
'                {"%", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a Mod b},
'                {"^", Function(a As BasicR.MATRIX, b As BasicR.MATRIX) a.Pow(b)},
'                {"!", AddressOf MatrixArithmetic.Factorial}}

'        Public Function Evaluate(a As BasicR.MATRIX, b As BasicR.MATRIX, op As String) As BasicR.MATRIX
'            Dim Method = Arithmetics(op)
'            Return Method(a, b)
'        End Function

'        ''' <summary>
'        ''' 
'        ''' </summary>
'        ''' <param name="a"></param>
'        ''' <param name="b">Useless parameter</param>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        Public Shared Function Factorial(a As BasicR.MATRIX, b As BasicR.MATRIX) As BasicR.MATRIX
'            Dim result As BasicR.MATRIX = New BasicR.MATRIX(Width:=a.Width, Height:=a.Height)

'            For row As Integer = 0 To a.Height - 1
'                For col As Integer = 0 To a.Width - 1
'                    result(row, col) = Mathematical.Helpers.Arithmetic.Factorial(a(row, col), -1)
'                Next
'            Next
'            Return result
'        End Function
'    End Class
'End Namespace