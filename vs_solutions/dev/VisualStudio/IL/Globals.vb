Imports System.Reflection
Imports System.Reflection.Emit

Namespace IL
    'public enum AssemblyType
    '{
    '    None,
    '    Console,
    '    Application,
    '    Library
    '}

    'public enum BinaryOperator
    '{
    '    Add,
    '    Subtract,
    '    Multiply,
    '    Divide,
    '    Modulus,
    '    ShiftLeft,
    '    ShiftRight,
    '    IdentityEquality,
    '    IdentityInequality,
    '    ValueEquality,
    '    ValueInequality,
    '    BitwiseOr,
    '    BitwiseAnd,
    '    BitwiseExclusiveOr,
    '    BooleanOr,
    '    BooleanAnd,
    '    LessThan,
    '    LessThanOrEqual,
    '    GreaterThan,
    '    GreaterThanOrEqual
    '}

    'public enum ExceptionHandlerType
    '{
    '    Finally,
    '    Catch,
    '    Filter,
    '    Fault
    '}

    'public enum FieldVisibility
    '{
    '    Private,
    '    Public,
    '    Internal,
    '    Protected,
    '}

    'public enum MethodVisibility
    '{
    '    Private,
    '    Public,
    '    Internal,
    '    External,
    '    Protected,
    '}
    'public enum MethodModifier
    '{
    '    Static,
    '    Override,
    '    Abstract,
    '    Virtual,
    '    Final,
    '    None,
    '}


    'public enum ResourceVisibility
    '{
    '    Public,
    '    Private
    '}

    'public enum TypeVisibility
    '{
    '    vPublic,
    '    vProtected,
    '    vInternal,
    '    vProtectedInternal,
    '    vPrivate
    '}

    'public enum ClassModifiers
    '{
    '    mAbstract,
    '    mSealed,
    '    mStatic,
    '    mNone,
    '}

    'public enum UnaryOperator
    '{
    '    Negate,
    '    BooleanNot,
    '    BitwiseNot,
    '    PreIncrement,
    '    PreDecrement,
    '    PostIncrement,
    '    PostDecrement
    '}

    Module Globals

        Public Cache As Dictionary(Of Integer, Object) = New Dictionary(Of Integer, Object)()
        Public multiByteOpCodes As OpCode()
        Public singleByteOpCodes As OpCode()
        Public modules As [Module]() = Nothing

        Sub New()
            Call LoadOpCodes()
        End Sub

        Private Sub LoadOpCodes()
            singleByteOpCodes = New OpCode(255) {}
            multiByteOpCodes = New OpCode(255) {}
            Dim infoArray1 As FieldInfo() = GetType(OpCodes).GetFields()

            For num1 = 0 To infoArray1.Length - 1
                Dim info1 = infoArray1(num1)

                If info1.FieldType Is GetType(OpCode) Then
                    Dim code1 As OpCode = info1.GetValue(Nothing)
                    Dim num2 As UShort = code1.Value

                    If num2 < &H100 Then
                        singleByteOpCodes(num2) = code1
                    Else

                        If (num2 And &HFF00) <> &HFE00 Then
                            Throw New Exception("Invalid OpCode.")
                        End If

                        multiByteOpCodes(num2 And &HFF) = code1
                    End If
                End If
            Next
        End Sub


        ''' <summary>
        ''' Retrieve the friendly name of a type
        ''' </summary>
        ''' <param name="typeName">
        ''' The complete name to the type
        ''' </param>
        ''' <returns>
        ''' The simplified name of the type (i.e. "int" instead f System.Int32)
        ''' </returns>
        Public Function ProcessSpecialTypes(typeName As String) As String
            Dim result = typeName

            Select Case typeName
                Case "System.string", "System.String", "String"
                    result = "string"
                Case "System.Int32", "Int", "Int32"
                    result = "int"
            End Select

            Return result
        End Function

        'public static string SpaceGenerator(int count)
        '{
        '    string result = "";
        '    for (int i = 0; i < count; i++) result += " ";
        '    return result;
        '}

        'public static string AddBeginSpaces(string source, int count)
        '{
        '    string[] elems = source.Split('\n');
        '    string result = "";
        '    for (int i = 0; i < elems.Length; i++)
        '    {
        '        result += SpaceGenerator(count) + elems[i] + "\n";
        '    }
        '    return result;
        '}
    End Module
End Namespace
