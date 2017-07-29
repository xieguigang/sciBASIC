Imports System.Runtime.CompilerServices
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Scripting.Runtime

    Module Extensions

        Public ReadOnly Property Numerics As Index(Of TypeCode)

        Sub New()
            Numerics = {
                TypeCode.Byte,
                TypeCode.Decimal,
                TypeCode.Double,
                TypeCode.Int16,
                TypeCode.Int32,
                TypeCode.Int64,
                TypeCode.SByte,
                TypeCode.Single,
                TypeCode.UInt16,
                TypeCode.UInt32,
                TypeCode.UInt64
            }.Indexing
        End Sub

        <Extension>
        Public Function OverloadsBinaryOperator(methods As IEnumerable(Of MethodInfo)) As BinaryOperator
            Return BinaryOperator.CreateOperator(methods?.ToArray)
        End Function

        <Extension>
        Public Function CreateArray(data As IEnumerable, type As Type) As Object
            Dim src = data.Cast(Of Object).ToArray
            Dim array As Array = Array.CreateInstance(type, src.Length)

            For i As Integer = 0 To src.Length - 1
                array.SetValue(src(i), i)
            Next

            Return array
        End Function
    End Module
End Namespace