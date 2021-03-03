Imports System
Imports System.Reflection
Imports System.Linq.Expressions

Namespace Xdr.EmitContexts
    Public Class FieldDesc
        Public ReadOnly FieldType As Type
        Public ReadOnly MInfo As MemberInfo
        Protected _isOption As Boolean = False
        Protected _isMany As Boolean = False
        Protected _isFix As Boolean = False
        Protected _len As UInteger = 0

        Public Sub New(mi As MemberInfo)
            MInfo = mi
            Dim fi As FieldInfo = TryCast(mi, FieldInfo)

            If fi IsNot Nothing Then
                FieldType = fi.FieldType
                ExtractAttributes()
                Return
            End If

            Dim pi As PropertyInfo = TryCast(mi, PropertyInfo)

            If pi IsNot Nothing Then
                FieldType = pi.PropertyType
                ExtractAttributes()
                Return
            End If

            Throw New NotImplementedException("only PropertyInfo or FieldInfo")
        End Sub

        Private Sub ExtractAttributes()
            Dim optAttr = MInfo.GetAttr(Of OptionAttribute)()

            If optAttr IsNot Nothing Then
                If FieldType.IsValueType Then Throw New InvalidOperationException("ValueType not supported Option attribute (use Nullable<> type)")
                _isOption = True
            End If

            Dim fixAttr = MInfo.GetAttr(Of FixAttribute)()
            Dim varAttr = MInfo.GetAttr(Of VarAttribute)()
            If fixAttr IsNot Nothing AndAlso varAttr IsNot Nothing Then Throw New InvalidOperationException("can not use Fix and Var attributes both")

            If fixAttr IsNot Nothing Then
                _isMany = True
                _isFix = True
                _len = fixAttr.Length
            End If

            If varAttr IsNot Nothing Then
                _isMany = True
                _isFix = False
                _len = varAttr.MaxLength
            End If

            If _isOption AndAlso _isMany Then Throw New InvalidOperationException("can not use Fix and Option attributes both or Var and Option attributes both")
        End Sub

        Friend Function BuildRead(pReader As Expression) As Expression
            If _isMany Then
                Return Expression.Call(pReader, GetType(Reader).GetMethod(If(_isFix, "ReadFix", "ReadVar")).MakeGenericMethod(FieldType), Expression.Constant(_len))
            Else
                Return Expression.Call(pReader, GetType(Reader).GetMethod(If(_isOption, "ReadOption", "Read")).MakeGenericMethod(FieldType))
            End If
        End Function

        Friend Function BuildReadOne(pReader As Expression) As Expression
            Return Expression.Call(pReader, GetType(Reader).GetMethod("Read").MakeGenericMethod(FieldType))
        End Function

        Friend Function BuildWriteOne(pWriter As Expression, key As Object) As Expression
            Dim inTryBody As Expression = Expression.Call(pWriter, GetType(Writer).GetMethod("Write").MakeGenericMethod(FieldType), Expression.Constant(key))
            Dim exParam = Expression.Parameter(GetType(SystemException), "ex")
            Dim inCatchBody = Expression.Throw(Expression.[New](GetType(FormatException).GetConstructor(New Type() {GetType(String), GetType(Exception)}), Expression.Constant("can't write '" & MInfo.Name & "' field"), exParam))
            Return Expression.TryCatch(inTryBody, Expression.Catch(exParam, inCatchBody))
        End Function

        Friend Function BuildWrite(pWriter As Expression, pItem As Expression) As Expression
            Dim field As Expression = Expression.PropertyOrField(pItem, MInfo.Name)
            Dim inTryBody As Expression

            If _isMany Then
                inTryBody = Expression.Call(pWriter, GetType(Writer).GetMethod(If(_isFix, "WriteFix", "WriteVar")).MakeGenericMethod(FieldType), Expression.Constant(_len), field)
            Else
                inTryBody = Expression.Call(pWriter, GetType(Writer).GetMethod(If(_isOption, "WriteOption", "Write")).MakeGenericMethod(FieldType), field)
            End If

            Dim exParam = Expression.Parameter(GetType(SystemException), "ex")
            Dim inCatchBody = Expression.Throw(Expression.[New](GetType(FormatException).GetConstructor(New Type() {GetType(String), GetType(Exception)}), Expression.Constant("can't write '" & MInfo.Name & "' field"), exParam))
            Return Expression.TryCatch(inTryBody, Expression.Catch(exParam, inCatchBody))
        End Function

        Friend Function BuildAssign(readed As Expression, result As ParameterExpression) As Expression
            Dim inTryBody = Expression.Assign(Expression.PropertyOrField(result, MInfo.Name), readed)
            Dim exParam = Expression.Parameter(GetType(SystemException), "ex")
            Dim inCatchBody = Expression.Throw(Expression.[New](GetType(FormatException).GetConstructor(New Type() {GetType(String), GetType(Exception)}), Expression.Constant("can't read '" & MInfo.Name & "' field"), exParam))

            Return Expression.TryCatch(Expression.Block(GetType(Void), inTryBody), Expression.Catch(exParam, inCatchBody))
        End Function
    End Class
End Namespace
