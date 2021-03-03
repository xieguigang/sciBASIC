Imports System
Imports System.Linq
Imports System.Reflection
Imports System.Collections.Generic
Imports System.Linq.Expressions

Namespace Xdr.EmitContexts
    Public Class OrderModel
        Private _Fields As System.Collections.Generic.List(Of Xdr.EmitContexts.FieldDesc)

        Public Property Fields As List(Of FieldDesc)
            Get
                Return _Fields
            End Get
            Private Set(value As List(Of FieldDesc))
                _Fields = value
            End Set
        End Property

        Public Function BuildReader(targetType As Type) As [Delegate]
            Dim pReader = Expression.Parameter(GetType(Reader))
            Dim variables As List(Of ParameterExpression) = New List(Of ParameterExpression)()
            Dim body As List(Of Expression) = New List(Of Expression)()
            Dim resultVar = Expression.Variable(targetType, "result")
            variables.Add(resultVar)
            Dim assign = Expression.Assign(resultVar, Expression.[New](targetType))
            body.Add(assign)

            For Each fieldDesc In Fields
                body.Add(fieldDesc.BuildAssign(fieldDesc.BuildRead(pReader), resultVar))
            Next

            body.Add(resultVar)
            Dim block = Expression.Block(variables, body)
            Return Expression.Lambda(CType(GetType(ReadOneDelegate(Of)).MakeGenericType(targetType), Type), CType(block, Expression), pReader).Compile()
        End Function

        Public Function BuildWriter(targetType As Type) As [Delegate]
            Dim pWriter = Expression.Parameter(GetType(Writer))
            Dim pItem = Expression.Parameter(targetType)
            Dim variables As List(Of ParameterExpression) = New List(Of ParameterExpression)()
            Dim body As List(Of Expression) = New List(Of Expression)()

            For Each fieldDesc In Fields
                body.Add(fieldDesc.BuildWrite(pWriter, pItem))
            Next

            Dim block = Expression.Block(variables, body)
            Return Expression.Lambda(CType(GetType(WriteOneDelegate(Of)).MakeGenericType(targetType), Type), CType(block, Expression), CType(pWriter, ParameterExpression), CType(pItem, ParameterExpression)).Compile()
        End Function

        Public Shared Function Create(t As Type) As OrderModel
            Dim fields As SortedList(Of UInteger, FieldDesc) = New SortedList(Of UInteger, FieldDesc)()

            For Each f In t.GetFields().Where(Function(fi) fi.IsPublic AndAlso Not fi.IsStatic)
                AppendField(fields, f)
            Next

            For Each p In t.GetProperties().Where(Function(pi) pi.CanWrite AndAlso pi.CanRead)
                AppendField(fields, p)
            Next

            If fields.Count = 0 Then Return Nothing
            Dim result As OrderModel = New OrderModel()
            result.Fields = fields.Values.ToList()
            Return result
        End Function

        Private Shared Sub AppendField(fields As SortedList(Of UInteger, FieldDesc), mi As MemberInfo)
            Dim fAttr As OrderAttribute = mi.GetAttr(Of OrderAttribute)()
            If fAttr Is Nothing Then Return
            If fields.ContainsKey(fAttr.Order) Then Throw New InvalidOperationException("duplicate order " & fAttr.Order)
            fields.Add(fAttr.Order, New FieldDesc(mi))
        End Sub
    End Class
End Namespace
