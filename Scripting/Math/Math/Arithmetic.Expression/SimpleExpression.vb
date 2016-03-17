Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Mathematical.Helpers

Namespace Types

    ''' <summary>
    ''' A class object stand for a very simple mathematic expression that have no bracket or function.
    ''' It only contains limited operator such as +-*/\%!^ in it.
    ''' (一个用于表达非常简单的数学表达式的对象，在这个所表示的简单表达式之中不能够包含有任何括号或者函数，
    ''' 其仅包含有有限的计算符号在其中，例如：+-*/\%^!)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SimpleExpression

        ''' <summary>
        ''' 在<see cref="SimpleExpression.Calculator"></see>之中由于移位操作的需要，需要使用类对象可以修改属性的特性来进行正常的计算，所以请不要修改为Structure类型
        ''' </summary>
        ''' <remarks></remarks>
        Protected Class MetaExpression

            <XmlAttribute> Public [Operator] As Char
            <XmlAttribute> Public LEFT As Double

            Public Overrides Function ToString() As String
                Return String.Format("{0} {1}", LEFT, [Operator])
            End Function
        End Class

        ''' <summary>
        ''' A simple expression can be view as a list collection of meta expression.
        ''' (可以将一个简单表达式看作为一个元表达式的集合)
        ''' </summary>
        ''' <remarks></remarks>
        Dim MetaList As New List(Of MetaExpression)

        Public ReadOnly Property LastOperator As Char
            Get
                Return MetaList.Last.Operator
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(n As Double)
            MetaList += New MetaExpression With {
                .LEFT = n, .Operator = "+"
            }
        End Sub

        Public Sub Add(n As Double, o As Char)
            MetaList += New MetaExpression With {
                .LEFT = n,
                .Operator = o
            }
        End Sub

        ''' <summary>
        ''' Debugging displaying in VS IDE
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Return String.Join("", (From item In MetaList Let s As String = item.ToString Select s).ToArray)
        End Function

        ''' <summary>
        ''' Evaluate the specific simple expression class object.
        ''' (计算一个特定的简单表达式对象的值) 
        ''' </summary>
        ''' <returns>
        ''' Return the value of the specific simple expression object.
        ''' (返回目标简单表达式对象的值)
        ''' </returns>
        ''' <remarks></remarks>
        Public Function Evaluate() As Double
            If MetaList.Count = 1 Then 'When the list object only contains one element, that means this class object only stands for a number, return this number directly. 
                Return MetaList.First.LEFT
            Else
                Calculator("^")
                Calculator("*/\%")
                Calculator("+-")

                Return MetaList.First.LEFT
            End If
        End Function

        Private Sub Calculator(OperatorList As String)
            Dim LQuery As IEnumerable(Of MetaExpression) =
                From e As MetaExpression In MetaList
                Where InStr(OperatorList, e.Operator) > 0
                Select e 'Defines a LINQ query use for select the meta element that contains target operator.
            Dim Counts As Integer = LQuery.Count
            Dim M, NextElement As MetaExpression

            For index As Integer = 0 To MetaList.Count - 1  'Scan the expression object and do the calculation at the mean time
                If Counts = 0 OrElse MetaList.Count = 1 Then
                    Return      'No more calculation could be done since there is only one number in the expression, break at this situation.
                ElseIf OperatorList.IndexOf(MetaList(index).Operator) <> -1 Then 'We find a meta expression element that contains target operator, then we do calculation on this element and the element next to it.  
                    M = MetaList(index)  'Get current element and the element that next to him
                    NextElement = MetaList(index + 1)
                    NextElement.LEFT = Arithmetic.Evaluate(M.LEFT, NextElement.LEFT, M.Operator)  'Do some calculation of type target operator 
                    MetaList.RemoveAt(index) 'When the current element is calculated, it is no use anymore, we remove it
                    index -= 1  'Keep the reading position order

                    Counts -= 1  'If the target operator is position at the front side of the expression, using this flag will make the for loop exit when all of the target operator is calculated to improve the performance as no needs to scan all of the expression at this situation. 
                End If
            Next
        End Sub

        Public Shared Function Evaluate(s As String) As Double
            Return SimpleParser.TryParse(s).Evaluate
        End Function
    End Class
End Namespace