#Region "Microsoft.VisualBasic::d5519a2c49b159bbff5ad5c5d19d303a, Microsoft.VisualBasic.Core\src\Scripting\TokenIcer\LangModels\Func.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class ExprToken
    ' 
    ' 
    ' 
    '     Class Func
    ' 
    '         Properties: Args, Caller, IsFuncCalls
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToArray, ToString
    ' 
    '         Sub: __expand
    ' 
    '     Class InnerToken
    ' 
    '         Properties: InnerStack, obj
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToArray, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language

Namespace Scripting.TokenIcer

    Public MustInherit Class ExprToken(Of Tokens As IComparable)

        Public MustOverride Function ToArray(stackT As StackTokens(Of Tokens)) As Token(Of Tokens)()

    End Class

    Public Class Func(Of Tokens As IComparable) : Inherits ExprToken(Of Tokens)

        Public Property Caller As List(Of InnerToken(Of Tokens))
        Public Property Args As Func(Of Tokens)()

        Public ReadOnly Property IsFuncCalls As Boolean
            Get
                Return Not Args.IsNullOrEmpty
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(currStack As InnerToken(Of Tokens))
            Caller = New List(Of InnerToken(Of Tokens)) From {currStack}
        End Sub

        ''' <summary>
        ''' 将表达式的栈空间展开
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToArray(stackT As StackTokens(Of Tokens)) As Token(Of Tokens)()
            Dim list As New List(Of Token(Of Tokens))
            Call __expand(list, stackT)
            Return list.ToArray
        End Function

        Private Sub __expand(ByRef list As List(Of Token(Of Tokens)), stackT As StackTokens(Of Tokens))
            For Each x In Caller
                Call list.AddRange(x.ToArray(stackT))
            Next
            If Not Args.IsNullOrEmpty Then
                For Each x In Args
                    Call x.__expand(list, stackT)
                Next
            End If
        End Sub

        Public Overrides Function ToString() As String
            If Args.IsNullOrEmpty Then
                Return String.Join(" ", Caller.Select(Function(x) x.ToString).ToArray)
            Else
                Dim caller As String = String.Join(" ", Me.Caller.Select(Function(x) x.ToString).ToArray)
                Dim params As String() = Me.Args.Select(Function(x) x.ToString).ToArray
                Dim args As String = String.Join(", ", params)
                Return $"{caller}({args})"
            End If
        End Function
    End Class

    Public Class InnerToken(Of Tokens As IComparable) : Inherits ExprToken(Of Tokens)

        Public Property obj As Token(Of Tokens)
        Public Property InnerStack As Func(Of Tokens)()

        Sub New(x As Token(Of Tokens), stack As IEnumerable(Of Func(Of Tokens)))
            obj = x
            InnerStack = stack.ToArray
        End Sub

        Sub New(x As Token(Of Tokens))
            obj = x
        End Sub

        Sub New(pretend As Tokens, funcCall As Func(Of Tokens))
            obj = New Token(Of Tokens)(pretend, "FuncCalls")
            InnerStack = {funcCall}
        End Sub

        Public Overrides Function ToString() As String
            If InnerStack.IsNullOrEmpty Then
                Return obj.Value
            Else
                Dim inner As String() = InnerStack.Select(Function(x) x.ToString).ToArray
                Dim s As String = String.Join(" ", inner)
                Return $"({s})"
            End If
        End Function

        Public Overrides Function ToArray(stackT As StackTokens(Of Tokens)) As Token(Of Tokens)()
            If stackT.Equals(obj.name, stackT.Pretend) Then
                Dim list As New List(Of Token(Of Tokens))
                For Each x In InnerStack
                    Call list.AddRange(x.ToArray(stackT))
                Next

                Return list.ToArray
            Else
                Return {obj}
            End If
        End Function
    End Class
End Namespace
