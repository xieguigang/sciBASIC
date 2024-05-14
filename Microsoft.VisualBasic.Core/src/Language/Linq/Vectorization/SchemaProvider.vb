#Region "Microsoft.VisualBasic::7f486ad9602dcee63dc9d2ae7876cb7d, Microsoft.VisualBasic.Core\src\Language\Linq\Vectorization\SchemaProvider.vb"

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

    '   Total Lines: 325
    '    Code Lines: 233
    ' Comment Lines: 46
    '   Blank Lines: 46
    '     File Size: 12.55 KB


    '     Class VectorSchemaProvider
    ' 
    '         Properties: PropertyNames, Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [Concatenate], [IntegerDivision], [Like], binaryOperatorSelfLeft, CreateSchema
    '                   GetDynamicMemberNames, ToString, TryBinaryOperation, (+2 Overloads) TryGetMember, TryInvokeMember
    '                   TrySetMember, TryUnaryOperation
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Dynamic
Imports System.Linq.Expressions
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Language.Vectorization

    ''' <summary>
    ''' Schema provider of the <see cref="VectorShadows(Of T)"/>
    ''' </summary>
    Public Class VectorSchemaProvider

        ''' <summary>
        ''' 单目运算符无重名的问题
        ''' </summary>
        ReadOnly operatorsUnary As New Dictionary(Of ExpressionType, MethodInfo)
        ''' <summary>
        ''' 双目运算符重载会带来重名运算符的问题
        ''' </summary>
        ReadOnly operatorsBinary As New Dictionary(Of ExpressionType, BinaryOperator)

#Region "VisualBasic exclusive language features"
        ReadOnly op_Concatenates As BinaryOperator
        ReadOnly op_Likes As BinaryOperator
        ReadOnly op_IntegerDivisions As BinaryOperator
#End Region

        ''' <summary>
        ''' The overloads function
        ''' </summary>
        ReadOnly methods As New Dictionary(Of String, OverloadsFunction)
        ReadOnly propertyList As Dictionary(Of String, PropertyInfo)

        Public ReadOnly Property PropertyNames As Index(Of String)
        Public ReadOnly Property Type As Type

        Public Const stringContract$ = "op_Concatenate"
        Public Const objectLike$ = "op_Like"
        Public Const nameIntegerDivision$ = "op_IntegerDivision"

        Private Sub New(type As Type,
                        propertyList As Dictionary(Of String, PropertyInfo),
                        propertyNames As Index(Of String),
                        op_Concatenates As BinaryOperator,
                        op_Likes As BinaryOperator,
                        op_IntegerDivisions As BinaryOperator,
                        methods As Dictionary(Of String, OverloadsFunction),
                        operatorsBinary As Dictionary(Of ExpressionType, BinaryOperator),
                        operatorsUnary As Dictionary(Of ExpressionType, MethodInfo))

            Me.Type = type
            Me.propertyList = propertyList
            Me.PropertyNames = propertyNames
            Me.op_Concatenates = op_Concatenates
            Me.op_Likes = op_Likes
            Me.op_IntegerDivisions = op_IntegerDivisions
            Me.methods = methods
            Me.operatorsBinary = operatorsBinary
            Me.operatorsUnary = operatorsUnary
        End Sub

        Public Shared Function CreateSchema(type As Type) As VectorSchemaProvider
            Dim propertyList = type.Schema(PropertyAccess.NotSure, PublicProperty, True)
            Dim PropertyNames = propertyList _
                .Values _
                .Select(Function([property]) [property].Name) _
                .Indexing

            Dim methods = type.GetMethods()
            Dim operators = methods _
                .Where(Function(x) InStr(x.Name, "op_") = 1 AndAlso x.IsStatic) _
                .GroupBy(Function(op) op.Name) _
                .ToArray

            Dim find = Function(opName$)
                           Return operators _
                               .Where(Function(m) m.Key = opName) _
                               .FirstOrDefault _
                              ?.OverloadsBinaryOperator
                       End Function

            ' 因为字符串连接操作符在Linq表达式中并没有被定义，所以在这里需要特殊处理
            Dim op_Concatenates = find(stringContract)
            Dim op_Likes = find(objectLike)
            Dim op_IntegerDivisions = find(nameIntegerDivision)
            Dim operatorsBinary As New Dictionary(Of ExpressionType, BinaryOperator)
            Dim operatorsUnary As New Dictionary(Of ExpressionType, MethodInfo)

            For Each op As IGrouping(Of String, MethodInfo) In operators _
                .Where(Function(o)
                           ' 在这里将IsTrue/IsFalse/CType等表达式排除掉
                           Return OperatorExpression.opName2Linq.ContainsKey(o.Key)
                       End Function)
#If DEBUG Then
                ' Call op.Key.EchoLine
#End If
                With op
                    If .Key = stringContract OrElse
                        .Key = objectLike OrElse
                        .Key = nameIntegerDivision Then

                        ' 前面已经被处理过了，不需要再额外处理这个运算符了
                        Continue For
                    End If
                End With

                ' 将运算符字符串名称转换为Linq表达式类型名称
                Dim name As ExpressionType = OperatorExpression.opName2Linq(op.Key)

                If op.First.GetParameters.Length > 1 Then
                    operatorsBinary(name) = op.OverloadsBinaryOperator
                Else
                    operatorsUnary(name) = op.First
                End If
            Next

            Dim methodList = methods _
                .Where(Function(m) Not m.IsStatic) _
                .GroupBy(Function(func) func.Name) _
                .Select(Function([overloads]) New OverloadsFunction([overloads].Key, [overloads])) _
                .ToDictionary(Function(g) g.Name)

            Return New VectorSchemaProvider(
                type, propertyList, PropertyNames,
                op_Concatenates, op_Likes,
                op_IntegerDivisions, methodList,
                operatorsBinary, operatorsUnary
            )
        End Function

        ''' <summary>
        ''' Returns property names and function names
        ''' </summary>
        ''' <returns></returns>
        Public Function GetDynamicMemberNames() As IEnumerable(Of String)
            Return PropertyNames.Objects.AsList + methods.Keys
        End Function

#Region "Property Get/Set"

        ''' <summary>
        ''' Property Get
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <returns></returns>
        Public Function TryGetMember(binder As GetMemberBinder) As PropertyInfo
            If PropertyNames.IndexOf(binder.Name) = -1 Then
                Return Nothing
            Else
                Return propertyList(binder.Name)
            End If
        End Function

        Public Function TryGetMember(ByRef name$, caseSensitive As Boolean) As PropertyInfo
            If PropertyNames.IndexOf(name) = -1 Then
                If Not caseSensitive Then
                    name = PropertyNames _
                        .Objects _
                        .Where(AddressOf name.TextEquals) _
                        .FirstOrDefault

                    If name.StringEmpty Then
                        Return Nothing
                    Else
                        Return propertyList(name)
                    End If
                Else
                    Return Nothing
                End If
            Else
                Return propertyList(name)
            End If
        End Function

        ''' <summary>
        ''' Property Set
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <returns></returns>
        Public Function TrySetMember(binder As SetMemberBinder) As PropertyInfo
            If PropertyNames.IndexOf(binder.Name) = -1 Then
                Return Nothing
            Else
                Return propertyList(binder.Name)
            End If
        End Function
#End Region

        ''' <summary>
        ''' Function invoke
        ''' </summary>
        ''' <param name="binder"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function TryInvokeMember(binder As InvokeMemberBinder, args() As Object) As MethodInfo
            If Not methods.ContainsKey(binder.Name) Then
                Return Nothing
            End If

            Dim [overloads] = methods(binder.Name)
            Dim method As MethodInfo = [overloads].Match(args.Select(Function(o) o.GetType).ToArray)
            Return method
        End Function

        Public Function TryUnaryOperation(binder As UnaryOperationBinder) As MethodInfo
            If Not operatorsUnary.ContainsKey(binder.Operation) Then
                Return Nothing
            Else
                Dim method = operatorsUnary(binder.Operation)
                Return method
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Type.ToString
        End Function

#Region "Operator:Binary"

        ''' <summary>
        ''' Fix for &amp; operator not defined!
        ''' </summary>
        ''' <returns></returns>
        Public Function [Concatenate](type As Type, ByRef vector As Boolean) As MethodInfo
            If op_Concatenates Is Nothing Then
                Return Nothing
            Else
                Return binaryOperatorSelfLeft(op_Concatenates, type, vector)
            End If
        End Function

        Private Shared Function binaryOperatorSelfLeft(op As BinaryOperator, type As Type, ByRef vector As Boolean) As MethodInfo
            Dim method As MethodInfo = op.MatchRight(type)

            If Not method Is Nothing Then
                Return method
            End If

            If type.ImplementInterface(GetType(IEnumerable)) Then
                vector = True
                type = type.GetInterfaces _
                    .Where(Function(i) i.Name = NameOf(IEnumerable)) _
                    .First _
                    .GenericTypeArguments _
                    .First

                Return op.MatchRight(type)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Fix for Like operator not defined in Linq.
        ''' </summary>
        ''' <returns></returns>
        Public Function [Like](type As Type, ByRef vector As Boolean) As MethodInfo
            If op_Likes Is Nothing Then
                Return Nothing
            Else
                Return binaryOperatorSelfLeft(op_Likes, type, vector)
            End If
        End Function

        Public Function [IntegerDivision](type As Type, ByRef vector As Boolean) As MethodInfo
            If op_IntegerDivisions Is Nothing Then
                Return Nothing
            Else
                Return binaryOperatorSelfLeft(op_IntegerDivisions, type, vector)
            End If
        End Function

        Const left% = 0
        Const right% = 1

        Public Function TryBinaryOperation(binder As BinaryOperationBinder, type As Type, ByRef vector As Boolean) As MethodInfo
            If Not operatorsBinary.ContainsKey(binder.Operation) Then
                Return Nothing
            End If

            Dim op As BinaryOperator = operatorsBinary(binder.Operation)
            Dim target As MethodInfo = Nothing

            With op.MatchRight(type)
                If Not .IsNothing AndAlso .GetParameters(left).ParameterType Is Me.Type Then
                    Return .ByRef
                End If
            End With

            ' target还是空值的话，则尝试将目标参数转换为集合类型
            If Not type.ImplementInterface(GetType(IEnumerable)) Then
                Return Nothing
            Else
                type = type.GetInterfaces _
                    .Where(Function(i) i.Name = NameOf(IEnumerable)) _
                    .First _
                    .GenericTypeArguments _
                    .First
            End If

            With op.MatchRight(type)
                If Not .IsNothing AndAlso .GetParameters(left).ParameterType Is Me.Type Then
                    vector = True
                    Return .ByRef
                End If
            End With

            Return Nothing
        End Function
#End Region

        Public Shared Operator =(a As VectorSchemaProvider, b As Type) As Boolean
            Return a.Type Is b
        End Operator

        Public Shared Operator <>(a As VectorSchemaProvider, b As Type) As Boolean
            Return Not a.Type Is b
        End Operator
    End Class
End Namespace
