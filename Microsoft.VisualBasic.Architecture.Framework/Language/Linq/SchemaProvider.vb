Imports System.Dynamic
Imports System.Linq.Expressions
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Language

    ''' <summary>
    ''' Schema provider of the <see cref="VectorShadows(Of T)"/>
    ''' </summary>
    Public Class SchemaProvider

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
        Public Const integerDivision$ = "op_IntegerDivision"

        Sub New(type As Type)
            Me.Type = type
            Me.propertyList = type.Schema(PropertyAccess.NotSure, PublicProperty, True)
            Me.PropertyNames = propertyList _
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
            op_Concatenates = find(stringContract)
            op_Likes = find(objectLike)
            op_IntegerDivisions = find(integerDivision)

            For Each op As IGrouping(Of String, MethodInfo) In operators
#If DEBUG Then
                ' Call op.Key.EchoLine
#End If
                With op
                    If .Key = stringContract OrElse
                        .Key = objectLike OrElse
                        .Key = integerDivision Then

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

            Me.methods = methods _
                .Where(Function(m) Not m.IsStatic) _
                .GroupBy(Function(func) func.Name) _
                .Select(Function([overloads]) New OverloadsFunction([overloads].Key, [overloads])) _
                .ToDictionary(Function(g) g.Name)
        End Sub

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
    End Class
End Namespace