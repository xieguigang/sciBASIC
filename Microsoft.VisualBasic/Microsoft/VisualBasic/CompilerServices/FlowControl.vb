Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Threading

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Public NotInheritable Class FlowControl
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function CheckContinueLoop(LoopFor As ObjectFor) As Boolean
            Dim flag As Boolean
            Try
                Dim num As Integer = DirectCast(LoopFor.Counter, IComparable).CompareTo(LoopFor.Limit)
                If LoopFor.PositiveStep Then
                    Return (num <= 0)
                End If
                If (num >= 0) Then
                    Return True
                End If
                flag = False
            Catch exception As InvalidCastException
                Dim args As String() = New String() {"loop control variable", Utils.VBFriendlyName(LoopFor.Counter)}
                Throw New ArgumentException(Utils.GetResourceString("Argument_IComparable2", args))
            End Try
            Return flag
        End Function

        Public Shared Sub CheckForSyncLockOnValueType(obj As Object)
            If ((Not obj Is Nothing) AndAlso obj.GetType.IsValueType) Then
                Dim args As String() = New String() {Utils.VBFriendlyName(obj.GetType)}
                Throw New ArgumentException(Utils.GetResourceString("SyncLockRequiresReferenceType1", args))
            End If
        End Sub

        Public Shared Function ForEachInArr(ary As Array) As IEnumerator
            Dim enumerator As IEnumerator = ary.GetEnumerator
            If (enumerator Is Nothing) Then
                Throw ExceptionUtils.VbMakeException(&H5C)
            End If
            Return enumerator
        End Function

        Public Shared Function ForEachInObj(obj As Object) As IEnumerator
            Dim enumerable As IEnumerable
            If (obj Is Nothing) Then
                Throw ExceptionUtils.VbMakeException(&H5B)
            End If
            Try
                enumerable = DirectCast(obj, IEnumerable)
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception6 As Exception
                Throw ExceptionUtils.MakeException1(100, obj.GetType.ToString)
            End Try
            Dim enumerator As IEnumerator = enumerable.GetEnumerator
            If (enumerator Is Nothing) Then
                Throw ExceptionUtils.MakeException1(100, obj.GetType.ToString)
            End If
            Return enumerator
        End Function

        Public Shared Function ForEachNextObj(ByRef obj As Object, enumerator As IEnumerator) As Boolean
            If enumerator.MoveNext Then
                obj = enumerator.Current
                Return True
            End If
            obj = Nothing
            Return False
        End Function

        Public Shared Function ForLoopInitObj(Counter As Object, Start As Object, Limit As Object, StepValue As Object, ByRef LoopForResult As Object, ByRef CounterResult As Object) As Boolean
            If (Start Is Nothing) Then
                Dim args As String() = New String() {"Start"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", args))
            End If
            If (Limit Is Nothing) Then
                Dim textArray2 As String() = New String() {"Limit"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray2))
            End If
            If (StepValue Is Nothing) Then
                Dim textArray3 As String() = New String() {"Step"}
                Throw New ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", textArray3))
            End If
            Dim typ As Type = Start.GetType
            Dim type As Type = Limit.GetType
            Dim type3 As Type = StepValue.GetType
            Dim widestType As TypeCode = ObjectType.GetWidestType(Start, Limit, False)
            widestType = ObjectType.GetWidestType(StepValue, widestType)
            Select Case widestType
                Case typeCode.String
                    widestType = typeCode.Double
                    Exit Select
                Case typeCode.Object
                    Dim textArray4 As String() = New String() {Utils.VBFriendlyName(typ), Utils.VBFriendlyName(type), Utils.VBFriendlyName(StepValue)}
                    Throw New ArgumentException(Utils.GetResourceString("ForLoop_CommonType3", textArray4))
            End Select
            Dim loopFor As New ObjectFor
            Dim typeCode As TypeCode = Type.GetTypeCode(typ)
            Dim code3 As TypeCode = Type.GetTypeCode(type3)
            Dim type4 As Type = Nothing
            If ((typeCode = widestType) AndAlso typ.IsEnum) Then
                type4 = typ
            End If
            If ((Type.GetTypeCode(type) = widestType) AndAlso type.IsEnum) Then
                If ((Not type4 Is Nothing) AndAlso (Not type4 Is type)) Then
                    type4 = Nothing
                    GoTo Label_013A
                End If
                type4 = type
            End If
            If ((code3 = widestType) AndAlso type3.IsEnum) Then
                If ((Not type4 Is Nothing) AndAlso (Not type4 Is type3)) Then
                    type4 = Nothing
                Else
                    type4 = type3
                End If
            End If
Label_013A:
            loopFor.EnumType = type4
            Try
                loopFor.Counter = ObjectType.CTypeHelper(Start, widestType)
            Catch exception As StackOverflowException
                Throw exception
            Catch exception2 As OutOfMemoryException
                Throw exception2
            Catch exception3 As ThreadAbortException
                Throw exception3
            Catch exception12 As Exception
                Dim textArray5 As String() = New String() {"Start", Utils.VBFriendlyName(typ), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType))}
                Throw New ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", textArray5))
            End Try
            Try
                loopFor.Limit = ObjectType.CTypeHelper(Limit, widestType)
            Catch exception4 As StackOverflowException
                Throw exception4
            Catch exception5 As OutOfMemoryException
                Throw exception5
            Catch exception6 As ThreadAbortException
                Throw exception6
            Catch exception16 As Exception
                Dim textArray6 As String() = New String() {"Limit", Utils.VBFriendlyName(type), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType))}
                Throw New ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", textArray6))
            End Try
            Try
                loopFor.StepValue = ObjectType.CTypeHelper(StepValue, widestType)
            Catch exception7 As StackOverflowException
                Throw exception7
            Catch exception8 As OutOfMemoryException
                Throw exception8
            Catch exception9 As ThreadAbortException
                Throw exception9
            Catch exception20 As Exception
                Dim textArray7 As String() = New String() {"Step", Utils.VBFriendlyName(type3), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType))}
                Throw New ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", textArray7))
            End Try
            Dim obj2 As Object = ObjectType.CTypeHelper(0, widestType)
            If (DirectCast(loopFor.StepValue, IComparable).CompareTo(obj2) >= 0) Then
                loopFor.PositiveStep = True
            Else
                loopFor.PositiveStep = False
            End If
            LoopForResult = loopFor
            If (Not loopFor.EnumType Is Nothing) Then
                CounterResult = [Enum].ToObject(loopFor.EnumType, loopFor.Counter)
            Else
                CounterResult = loopFor.Counter
            End If
            Return FlowControl.CheckContinueLoop(loopFor)
        End Function

        Public Shared Function ForNextCheckDec(count As Decimal, limit As Decimal, StepValue As Decimal) As Boolean
            If (StepValue < Decimal.Zero) Then
                Return (count >= limit)
            End If
            Return (count <= limit)
        End Function

        Public Shared Function ForNextCheckObj(Counter As Object, LoopObj As Object, ByRef CounterResult As Object) As Boolean
            Dim widestType As TypeCode
            Dim code4 As TypeCode
            If (LoopObj Is Nothing) Then
                Throw ExceptionUtils.VbMakeException(&H5C)
            End If
            If (Counter Is Nothing) Then
                Dim args As String() = New String() {"Counter"}
                Throw New NullReferenceException(Utils.GetResourceString("Argument_InvalidNullValue1", args))
            End If
            Dim loopFor As ObjectFor = DirectCast(LoopObj, ObjectFor)
            Dim typeCode As TypeCode = DirectCast(Counter, IConvertible).GetTypeCode
            Dim code2 As TypeCode = DirectCast(loopFor.StepValue, IConvertible).GetTypeCode
            If ((typeCode = code2) AndAlso (typeCode <> TypeCode.String)) Then
                widestType = typeCode
            Else
                widestType = ObjectType.GetWidestType(typeCode, code2)
                If (widestType = TypeCode.String) Then
                    widestType = TypeCode.Double
                End If
                If (code4 = TypeCode.Object) Then
                    Dim textArray2 As String() = New String() {Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(typeCode)), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(code2))}
                    Throw New ArgumentException(Utils.GetResourceString("ForLoop_CommonType2", textArray2))
                End If
                Try
                    Counter = ObjectType.CTypeHelper(Counter, widestType)
                Catch exception As StackOverflowException
                    Throw exception
                Catch exception2 As OutOfMemoryException
                    Throw exception2
                Catch exception3 As ThreadAbortException
                    Throw exception3
                Catch exception12 As Exception
                    Dim textArray3 As String() = New String() {"Start", Utils.VBFriendlyName(Counter.GetType), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType))}
                    Throw New ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", textArray3))
                End Try
                Try
                    loopFor.Limit = ObjectType.CTypeHelper(loopFor.Limit, widestType)
                Catch exception4 As StackOverflowException
                    Throw exception4
                Catch exception5 As OutOfMemoryException
                    Throw exception5
                Catch exception6 As ThreadAbortException
                    Throw exception6
                Catch exception16 As Exception
                    Dim textArray4 As String() = New String() {"Limit", Utils.VBFriendlyName(loopFor.Limit.GetType), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType))}
                    Throw New ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", textArray4))
                End Try
                Try
                    loopFor.StepValue = ObjectType.CTypeHelper(loopFor.StepValue, widestType)
                Catch exception7 As StackOverflowException
                    Throw exception7
                Catch exception8 As OutOfMemoryException
                    Throw exception8
                Catch exception9 As ThreadAbortException
                    Throw exception9
                Catch exception20 As Exception
                    Dim textArray5 As String() = New String() {"Step", Utils.VBFriendlyName(loopFor.StepValue.GetType), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType))}
                    Throw New ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", textArray5))
                End Try
            End If
            loopFor.Counter = ObjectType.AddObj(Counter, loopFor.StepValue)
            code4 = DirectCast(loopFor.Counter, IConvertible).GetTypeCode
            If (Not loopFor.EnumType Is Nothing) Then
                CounterResult = [Enum].ToObject(loopFor.EnumType, loopFor.Counter)
            Else
                CounterResult = loopFor.Counter
            End If
            If (code4 <> widestType) Then
                loopFor.Limit = ObjectType.CTypeHelper(loopFor.Limit, code4)
                loopFor.StepValue = ObjectType.CTypeHelper(loopFor.StepValue, code4)
                Return False
            End If
            Return FlowControl.CheckContinueLoop(loopFor)
        End Function

        Public Shared Function ForNextCheckR4(count As Single, limit As Single, StepValue As Single) As Boolean
            If (StepValue > 0!) Then
                Return (count <= limit)
            End If
            Return (count >= limit)
        End Function

        Public Shared Function ForNextCheckR8(count As Double, limit As Double, StepValue As Double) As Boolean
            If (StepValue > 0) Then
                Return (count <= limit)
            End If
            Return (count >= limit)
        End Function


        ' Nested Types
        Private NotInheritable Class ObjectFor
            ' Methods
            Friend Sub New()
            End Sub


            ' Fields
            Public Counter As Object
            Public EnumType As Type
            Public Limit As Object
            Public PositiveStep As Boolean
            Public StepValue As Object
        End Class
    End Class
End Namespace

