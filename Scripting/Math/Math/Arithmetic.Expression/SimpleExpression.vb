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

        Public Sub Add(meta As MetaExpression)
            MetaList += meta
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
            If Me.MetaList.Count = 0 Then
                Return 0R
            End If

            Dim metaList As List(Of MetaExpression) = New List(Of MetaExpression)(Me.MetaList)

            If metaList.Count = 1 Then 'When the list object only contains one element, that means this class object only stands for a number, return this number directly. 
                Return metaList.First.LEFT
            Else
                Calculator("^", metaList)
                Calculator("*/\%", metaList)
                Calculator("+-", metaList)

                Return metaList.First.LEFT
            End If
        End Function

        Private Shared Sub Calculator(OperatorList As String, ByRef metaList As List(Of MetaExpression))
            Dim ct As Integer = (From mep As MetaExpression
                                 In metaList
                                 Where InStr(OperatorList, mep.Operator) > 0
                                 Select mep).Count  'Defines a LINQ query use for select the meta element that contains target operator..Count
            Dim M, mNext As MetaExpression
            Dim x As Double

            For index As Integer = 0 To metaList.Count - 1  'Scan the expression object and do the calculation at the mean time
                If ct = 0 OrElse metaList.Count = 1 Then
                    Return      'No more calculation could be done since there is only one number in the expression, break at this situation.
                ElseIf OperatorList.IndexOf(metaList(index).Operator) <> -1 Then 'We find a meta expression element that contains target operator, then we do calculation on this element and the element next to it.  
                    M = metaList(index)  'Get current element and the element that next to him
                    mNext = metaList(index + 1)
                    x = Arithmetic.Evaluate(M.LEFT, mNext.LEFT, M.Operator)  'Do some calculation of type target operator 
                    metaList.RemoveAt(index) 'When the current element is calculated, it is no use anymore, we remove it
                    metaList(index) = New MetaExpression(x, mNext.Operator)
                    index -= 1  'Keep the reading position order

                    ct -= 1  'If the target operator is position at the front side of the expression, using this flag will make the for loop exit when all of the target operator is calculated to improve the performance as no needs to scan all of the expression at this situation. 
                End If
            Next
        End Sub

        Public Shared Function Evaluate(s As String) As Double
            Return SimpleParser.TryParse(s).Evaluate
        End Function
    End Class
End Namespace