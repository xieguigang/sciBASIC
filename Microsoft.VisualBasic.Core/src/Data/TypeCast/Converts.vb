
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.ValueTypes

Namespace ComponentModel.DataSourceModel.TypeCast

    Public Module Converts

        Public Function CTypeDynamics(from As Type, [to] As TypeCode) As Func(Of Object, Object)
            If from Is [to].CreatePrimitiveType Then
                Return Function(o) o
            End If

            Select Case from
                Case GetType(Double) : Return CastFromDouble([to])
                Case GetType(Integer) : Return CastFromInteger([to])
                Case Else
                    Throw New NotImplementedException(from.ToString)
            End Select
        End Function

        Private Function CastFromInteger([to] As TypeCode) As Func(Of Object, Object)
            Select Case [to]
                Case TypeCode.Boolean
                    Return Function(d) CInt(d) <> 0
                Case TypeCode.Byte,
                     TypeCode.Decimal,
                     TypeCode.Int16,
                     TypeCode.Int64,
                     TypeCode.SByte,
                     TypeCode.Single,
                     TypeCode.UInt16,
                     TypeCode.UInt32,
                     TypeCode.UInt64

                    Dim to_type As Type = [to].CreatePrimitiveType

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
        End Function

        Private Function CastFromDouble([to] As TypeCode) As Func(Of Object, Object)
            Select Case [to]
                Case TypeCode.Boolean
                    Return Function(d) CDbl(d) <> 0
                Case TypeCode.Byte,
                     TypeCode.Decimal,
                     TypeCode.Int16,
                     TypeCode.Int32,
                     TypeCode.Int64,
                     TypeCode.SByte,
                     TypeCode.Single,
                     TypeCode.UInt16,
                     TypeCode.UInt32,
                     TypeCode.UInt64

                    Dim to_type As Type = [to].CreatePrimitiveType

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
        End Function
    End Module
End Namespace