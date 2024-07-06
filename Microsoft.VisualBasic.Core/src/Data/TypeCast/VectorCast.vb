
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.ValueTypes

Namespace ComponentModel.DataSourceModel.TypeCast

    Public Module VectorCast

        <Extension>
        Public Function [CType](col As IEnumerable(Of Object), type As TypeCode) As Array
            Dim pull As Object() = col.SafeQuery.ToArray

            If pull.Length = 0 Then
                Return VectorCast.Allocate(type, len:=0)
            End If

            Dim schema As Type() = pull _
                .Select(Function(o) If(o Is Nothing, Nothing, o.GetType)) _
                .Where(Function(t) Not t Is Nothing) _
                .Distinct _
                .ToArray
            Dim vec As Array = VectorCast.Allocate(type, len:=pull.Length)
            Dim castTo As Func(Of Object, Object)

            If schema.Length = 1 Then
                ' is generic
                castTo = CastToGeneric(schema(0), type)
            Else
                castTo = CastToGeneral(type)
            End If

            For i As Integer = 0 To pull.Length - 1
                If Not pull(i) Is Nothing Then
                    Call vec.SetValue(castTo(pull(i)), i)
                End If
            Next

            Return vec
        End Function

        Public Function Allocate(type As TypeCode, len As Integer) As Array
            Select Case type
                Case TypeCode.Boolean : Return New Boolean(len - 1) {}
                Case TypeCode.Byte : Return New Byte(len - 1) {}
                Case TypeCode.Char : Return New Char(len - 1) {}
                Case TypeCode.DateTime : Return New Date(len - 1) {}
                Case TypeCode.DBNull, TypeCode.Empty, TypeCode.Object : Return New Object(len - 1) {}
                Case TypeCode.Decimal : Return New Decimal(len - 1) {}
                Case TypeCode.Double : Return New Double(len - 1) {}
                Case TypeCode.Int16 : Return New Int16(len - 1) {}
                Case TypeCode.Int32 : Return New Int32(len - 1) {}
                Case TypeCode.Int64 : Return New Int64(len - 1) {}
                Case TypeCode.SByte : Return New SByte(len - 1) {}
                Case TypeCode.Single : Return New Single(len - 1) {}
                Case TypeCode.String : Return New String(len - 1) {}
                Case TypeCode.UInt16 : Return New UInt16(len - 1) {}
                Case TypeCode.UInt32 : Return New UInt32(len - 1) {}
                Case TypeCode.UInt64 : Return New UInt64(len - 1) {}
                Case Else
                    Throw New NotImplementedException(type.ToString)
            End Select
        End Function

        Private Function CastToGeneric(from As Type, [to] As TypeCode) As Func(Of Object, Object)
            Dim to_type As Type = [to].CreatePrimitiveType

            If [to] = TypeCode.Object OrElse
                [to] = TypeCode.Empty OrElse
                [to] = TypeCode.DBNull Then

                Return Function(o) o
            End If

            Select Case from
                Case GetType(Double)
                    Select Case [to]
                        Case TypeCode.Boolean
                            Return Function(d) CDbl(d) <> 0
                        Case TypeCode.Byte, TypeCode.Decimal, TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.SByte, TypeCode.Single, TypeCode.UInt16, TypeCode.UInt32, TypeCode.UInt64
                            Return Function(d) Conversion.CTypeDynamic(d, to_type)
                        Case TypeCode.Char
                            Return Function(d) ChrW(CInt(d))
                        Case TypeCode.DateTime
                            Return Function(d) DateTimeHelper.FromUnixTimeStamp(CLng(d))
                        Case TypeCode.String
                            Return Function(d) d.ToString
                        Case Else
                            Throw New NotImplementedException([to].ToString)
                    End Select
            End Select
        End Function

        Public Function CastToGeneral([to] As TypeCode) As Func(Of Object, Object)

        End Function
    End Module
End Namespace