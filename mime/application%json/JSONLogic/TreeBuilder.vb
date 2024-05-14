#Region "Microsoft.VisualBasic::e3203c71c2ce16b64564efff3e606a26, mime\application%json\JSONLogic\TreeBuilder.vb"

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


    ' Code Statistics:

    '   Total Lines: 217
    '    Code Lines: 167
    ' Comment Lines: 17
    '   Blank Lines: 33
    '     File Size: 9.34 KB


    '     Class TreeBuilder
    ' 
    '         Properties: Parameters
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: and2, andalso2, checkBinary, equalsTo, greaterThan
    '                   greaterThanOrEquals, is2, lessThan, lessThanOrEquals, or2
    '                   orelse2, (+2 Overloads) Parse, ParseExpression, ParseLiteral, ParserInternal
    '                   symbolReference, tripleIif, workSingle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace JSONLogic

    Public Class TreeBuilder

        ReadOnly symbols As Dictionary(Of String, ParameterExpression)

        Public ReadOnly Property Parameters As IEnumerable(Of ParameterExpression)
            Get
                Return symbols.Values
            End Get
        End Property

        Sub New(symbols As IEnumerable(Of (name As String, type As Type)))
            Me.symbols = symbols _
                .SafeQuery _
                .ToDictionary(Function(i) i.name,
                              Function(n)
                                  Return Expression.Parameter(n.type, n.name)
                              End Function)
        End Sub

        ''' <summary>
        ''' convert the json logical tree as the .net clr lambda expression tree
        ''' </summary>
        ''' <param name="logic"></param>
        ''' <returns></returns>
        Public Shared Function Parse(logic As JsonElement, ParamArray symbols As (name As String, type As Type)()) As Expression
            Return New TreeBuilder(symbols).ParserInternal(logic)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Parse(exp As JsonElement) As Expression
            Return ParserInternal(exp)
        End Function

        Private Function ParserInternal(exp As JsonElement) As Expression
            If exp Is Nothing Then
                Return Expression.Constant(Nothing)
            ElseIf TypeOf exp Is JsonValue Then
                Return ParseLiteral(exp)
            ElseIf TypeOf exp Is JsonObject Then
                Return ParseExpression(exp)
            Else
                Throw New NotImplementedException
            End If
        End Function

        Private Function workSingle(key As String, val As JsonElement) As Expression
            Select Case key.ToLower
                Case "var", "let", "dim" : Return symbolReference(val)
                Case "if" : Return tripleIif(val)
                Case "<" : Return lessThan(checkBinary(val))
                Case ">" : Return greaterThan(checkBinary(val))
                Case "==" : Return equalsTo(checkBinary(val))
                Case "!=", "<>" : Return Expression.Not(equalsTo(checkBinary(val)))
                Case "<=" : Return lessThanOrEquals(checkBinary(val))
                Case ">=" : Return greaterThanOrEquals(checkBinary(val))
                Case "!", "not" : Return Expression.Not(ParserInternal(val))
                Case "or", "|" : Return or2(checkBinary(val))
                Case "and", "&" : Return and2(checkBinary(val))
                Case "is" : Return is2(checkBinary(val))
                Case "or else", "orelse", "||" : Return orelse2(checkBinary(val))
                Case "and also", "andalso", "&&" : Return andalso2(checkBinary(val))
                Case "+", "add"
                Case "-", "sub"
                Case "*"
                Case "/"
                Case "%", "mod"
            End Select

            Throw New NotImplementedException(key)
        End Function

        Private Function and2(binary As (left As Expression, right As Expression)) As Expression
            Return Expression.And(binary.left, binary.right)
        End Function

        Private Function or2(binary As (left As Expression, right As Expression)) As Expression
            Return Expression.Or(binary.left, binary.right)
        End Function

        Private Function is2(binary As (left As Expression, right As Expression)) As Expression
            Return Expression.ReferenceEqual(binary.left, binary.right)
        End Function

        Private Function andalso2(binary As (left As Expression, right As Expression)) As Expression
            Return Expression.AndAlso(binary.left, binary.right)
        End Function

        Private Function orelse2(binary As (left As Expression, right As Expression)) As Expression
            Return Expression.OrElse(binary.left, binary.right)
        End Function

        Private Function checkBinary(val As JsonElement) As (Expression, Expression)
            If Not TypeOf val Is JsonArray Then
                Throw New InvalidCastException
            Else
                Dim binary As JsonArray = val

                If binary.Length <> 2 Then
                    Throw New InvalidCastException
                Else
                    Return (ParserInternal(binary(0)), ParserInternal(binary(1)))
                End If
            End If
        End Function

        Private Function greaterThanOrEquals(binary As (left As Expression, right As Expression)) As Expression
            Return Expression.GreaterThanOrEqual(binary.left, binary.right)
        End Function

        Private Function lessThanOrEquals(binary As (left As Expression, right As Expression)) As Expression
            Return Expression.LessThanOrEqual(binary.left, binary.right)
        End Function

        Private Function equalsTo(binary As (left As Expression, right As Expression)) As Expression
            Return Expression.Equal(binary.left, binary.right)
        End Function

        Private Function greaterThan(binary As (left As Expression, right As Expression)) As Expression
            Return Expression.GreaterThan(binary.left, binary.right)
        End Function

        Private Function lessThan(binary As (left As Expression, right As Expression)) As Expression
            Return Expression.LessThan(binary.left, binary.right)
        End Function

        ''' <summary>
        ''' If(test, true, false)
        ''' </summary>
        ''' <returns></returns>
        Private Function tripleIif(val As JsonElement) As Expression
            If Not TypeOf val Is JsonArray Then
                Throw New InvalidCastException
            Else
                Dim triple As JsonArray = val

                If triple.Length = 3 Then
                    ' if(test,a,b)
                    Dim test As Expression = ParserInternal(triple(0))
                    Dim true1 As Expression = ParserInternal(triple(1))
                    Dim false1 As Expression = ParserInternal(triple(2))

                    Return Expression.Condition(test, true1, false1)
                ElseIf triple.Length = 2 Then
                    ' if(a,b)
                    ' -> if(a isnot nothing, a, b)
                    Dim true1 As Expression = ParserInternal(triple(0))
                    Dim false1 As Expression = ParserInternal(triple(1))

                    Return Expression.Condition(Expression.ReferenceEqual(true1, Expression.Constant(Nothing)), false1, true1)
                ElseIf triple.Length > 3 Then
                    ' [a,b,c,d,e]
                    ' if (a,b, if (c,d,e))
                    Dim exps As Expression() = triple.Select(Function(a) ParserInternal(a)).ToArray
                    Dim pairs = exps.Split(2)
                    Dim false2 As Expression = pairs.Last()(0)

                    ' skip the last
                    For Each part As Expression() In pairs.Reverse.Skip(1)
                        false2 = Expression.Condition(part(0), part(1), false2)
                    Next

                    Return false2
                Else
                    Throw New InvalidCastException
                End If
            End If
        End Function

        Private Function symbolReference(val As JsonElement) As Expression
            Dim names As String()

            ' symbol reference
            If TypeOf val Is JsonValue Then
                names = {CStr(DirectCast(val, JsonValue))}
            ElseIf TypeOf val Is JsonArray Then
                names = DirectCast(val, JsonArray).Select(Function(r) CStr(DirectCast(r, JsonValue))).ToArray
            Else
                Throw New InvalidCastException("the json object can not be used for variable name!")
            End If

            If names.Length = 1 Then
                Return symbols(names(0))
            Else
                Return Expression.RuntimeVariables(names.Select(Function(r) symbols(r)))
            End If
        End Function

        Private Function ParseExpression(exp As JsonObject) As Expression
            If exp.size = 1 Then
                Return workSingle(key:=exp.ObjectKeys.First, val:=exp(exp.ObjectKeys.First))
            ElseIf exp.size = 0 Then
                ' empty for null?
                Return Expression.Constant(Nothing)
            Else
                Throw New NotImplementedException
            End If
        End Function

        Private Function ParseLiteral(val As JsonValue) As Expression
            If val.value Is Nothing Then
                Return Expression.Constant(Nothing)
            ElseIf val.UnderlyingType Is GetType(String) Then
                Return Expression.Constant(CStr(val))
            Else
                Return Expression.Constant(val.value)
            End If
        End Function
    End Class
End Namespace
