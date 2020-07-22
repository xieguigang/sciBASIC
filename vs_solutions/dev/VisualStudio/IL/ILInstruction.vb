Imports System.Reflection
Imports System.Reflection.Emit

Namespace IL

    Public Class ILInstruction

        Public Property Code As OpCode
        Public Property Operand As Object
        Public Property OperandData As Byte()
        Public Property Offset As Integer

        Public Overrides Function ToString() As String
            Return GetCode()
        End Function

        ''' <summary>
        ''' Returns a friendly strign representation of this instruction
        ''' </summary>
        ''' <returns></returns>
        Public Function GetCode() As String
            Dim result = GetExpandedOffset(Offset) & " : " & Code.ToString

            If Operand IsNot Nothing Then
                result = result & GetOperandCode()
            End If

            Return result
        End Function

        Private Function buildInlineMethodCode() As String
            Dim result = " "

            Try
                Dim mOperand = CType(Operand, MethodInfo)

                If Not mOperand.IsStatic Then
                    result &= "instance "
                End If

                result &= ProcessSpecialTypes(mOperand.ReturnType.ToString()) & " " & ProcessSpecialTypes(mOperand.ReflectedType.ToString()) & "::" & mOperand.Name & "()"
            Catch
                Try
                    Dim mOperand = CType(Operand, ConstructorInfo)

                    If Not mOperand.IsStatic Then
                        result &= "instance "
                    End If

                    result &= "void " & ProcessSpecialTypes(mOperand.ReflectedType.ToString()) & "::" & mOperand.Name & "()"
                Catch

                End Try
            End Try

            Return result
        End Function

        Private Function GetOperandCode() As String
            Select Case Code.OperandType
                Case OperandType.InlineField
                    With CType(Operand, FieldInfo)
                        Return " " & ProcessSpecialTypes(.FieldType.ToString()) & " " & ProcessSpecialTypes(.ReflectedType.ToString()) & "::" & .Name & ""
                    End With
                Case OperandType.InlineMethod
                    Return buildInlineMethodCode()
                Case OperandType.ShortInlineBrTarget, OperandType.InlineBrTarget
                    Return " " & GetExpandedOffset(CInt(Operand))
                Case OperandType.InlineType
                    Return " " & ProcessSpecialTypes(Operand.ToString())
                Case OperandType.InlineString

                    If Equals(Operand.ToString(), vbCrLf) Then
                        Return " ""\r\n"""
                    Else
                        Return " """ & Operand.ToString() & """"
                    End If

                Case OperandType.ShortInlineVar
                    Return Operand.ToString()
                Case OperandType.InlineI, OperandType.InlineI8, OperandType.InlineR, OperandType.ShortInlineI, OperandType.ShortInlineR
                    Return Operand.ToString()
                Case OperandType.InlineTok

                    If TypeOf Operand Is Type Then
                        Return CType(Operand, Type).FullName
                    Else
                        Return "not supported"
                    End If

                Case Else
                    Return "not supported"
            End Select
        End Function

        ''' <summary>
        ''' Add enough zeros to a number as to be represented on 4 characters
        ''' </summary>
        ''' <param name="offset">
        ''' The number that must be represented on 4 characters
        ''' </param>
        ''' <returns>
        ''' </returns>
        Private Function GetExpandedOffset(offset As Long) As String
            Dim result As String = offset.ToString()
            Dim i = 0

            While result.Length < 4
                result = "0" & result
                i += 1
            End While

            Return result
        End Function
    End Class
End Namespace
