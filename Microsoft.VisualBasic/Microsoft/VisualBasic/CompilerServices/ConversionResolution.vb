Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Reflection
Imports Microsoft.VisualBasic.CompilerServices.Symbols

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class ConversionResolution
        ' Methods
        Shared Sub New()
            ConversionResolution.NumericSpecificityRank(6) = 1
            ConversionResolution.NumericSpecificityRank(5) = 2
            ConversionResolution.NumericSpecificityRank(7) = 3
            ConversionResolution.NumericSpecificityRank(8) = 4
            ConversionResolution.NumericSpecificityRank(9) = 5
            ConversionResolution.NumericSpecificityRank(10) = 6
            ConversionResolution.NumericSpecificityRank(11) = 7
            ConversionResolution.NumericSpecificityRank(12) = 8
            ConversionResolution.NumericSpecificityRank(15) = 9
            ConversionResolution.NumericSpecificityRank(13) = 10
            ConversionResolution.NumericSpecificityRank(14) = 11
            ConversionResolution.ForLoopWidestTypeCode = New TypeCode()() {New TypeCode(&H13 - 1) {}, New TypeCode(&H13 - 1) {}, New TypeCode(&H13 - 1) {}, New TypeCode() {TypeCode.Empty}, New TypeCode(&H13 - 1) {}, New TypeCode() {TypeCode.Empty}, New TypeCode() {TypeCode.Empty}, New TypeCode() {TypeCode.Empty}, New TypeCode() {TypeCode.Empty}, New TypeCode() {TypeCode.Empty}, New TypeCode() {TypeCode.Empty}, New TypeCode() {TypeCode.Empty}, New TypeCode() {TypeCode.Empty}, New TypeCode() {TypeCode.Empty}, New TypeCode() {TypeCode.Empty}, New TypeCode() {TypeCode.Empty}, New TypeCode(&H13 - 1) {}, New TypeCode(&H13 - 1) {}, New TypeCode(&H13 - 1) {}}
        End Sub

        Private Sub New()
        End Sub

        Private Shared Function ClassifyCLRArrayToInterfaceConversion(TargetInterface As Type, SourceArrayType As Type) As ConversionClass
            If Symbols.Implements(SourceArrayType, TargetInterface) Then
                Return ConversionClass.Widening
            End If
            If (SourceArrayType.GetArrayRank <= 1) Then
                Dim elementType As Type = SourceArrayType.GetElementType
                Dim none As ConversionClass = ConversionClass.None
                If (TargetInterface.IsGenericType AndAlso Not TargetInterface.IsGenericTypeDefinition) Then
                    Dim genericTypeDefinition As Type = TargetInterface.GetGenericTypeDefinition
                    If (((genericTypeDefinition Is GetType(IList(Of ))) OrElse (genericTypeDefinition Is GetType(ICollection(Of )))) OrElse (genericTypeDefinition Is GetType(IEnumerable(Of )))) Then
                        none = ConversionResolution.ClassifyCLRConversionForArrayElementTypes(TargetInterface.GetGenericArguments(0), elementType)
                    End If
                Else
                    Dim typeArguments As Type() = New Type() {elementType}
                    none = ConversionResolution.ClassifyPredefinedCLRConversion(TargetInterface, GetType(IList(Of )).MakeGenericType(typeArguments))
                End If
                Select Case none
                    Case ConversionClass.Identity, ConversionClass.Widening
                        Return ConversionClass.Widening
                End Select
            End If
            Return ConversionClass.Narrowing
        End Function

        Private Shared Function ClassifyCLRConversionForArrayElementTypes(TargetElementType As Type, SourceElementType As Type) As ConversionClass
            If (Symbols.IsReferenceType(SourceElementType) AndAlso Symbols.IsReferenceType(TargetElementType)) Then
                Return ConversionResolution.ClassifyPredefinedCLRConversion(TargetElementType, SourceElementType)
            End If
            If (Symbols.IsValueType(SourceElementType) AndAlso Symbols.IsValueType(TargetElementType)) Then
                Return ConversionResolution.ClassifyPredefinedCLRConversion(TargetElementType, SourceElementType)
            End If
            If (Symbols.IsGenericParameter(SourceElementType) AndAlso Symbols.IsGenericParameter(TargetElementType)) Then
                If (SourceElementType Is TargetElementType) Then
                    Return ConversionClass.Identity
                End If
                If (Symbols.IsReferenceType(SourceElementType) AndAlso Symbols.IsOrInheritsFrom(SourceElementType, TargetElementType)) Then
                    Return ConversionClass.Widening
                End If
                If (Symbols.IsReferenceType(TargetElementType) AndAlso Symbols.IsOrInheritsFrom(TargetElementType, SourceElementType)) Then
                    Return ConversionClass.Narrowing
                End If
            End If
            Return ConversionClass.None
        End Function

        Friend Shared Function ClassifyConversion(TargetType As Type, SourceType As Type, ByRef OperatorMethod As Method) As ConversionClass
            Dim class2 As ConversionClass = ConversionResolution.ClassifyPredefinedConversion(TargetType, SourceType)
            If ((((class2 <> ConversionClass.None) OrElse Symbols.IsInterface(SourceType)) OrElse Symbols.IsInterface(TargetType)) OrElse (Not Symbols.IsClassOrValueType(SourceType) AndAlso Not Symbols.IsClassOrValueType(TargetType))) Then
                Return class2
            End If
            If (Symbols.IsIntrinsicType(SourceType) AndAlso Symbols.IsIntrinsicType(TargetType)) Then
                Return class2
            End If
            Return ConversionResolution.ClassifyUserDefinedConversion(TargetType, SourceType, OperatorMethod)
        End Function

        Friend Shared Function ClassifyIntrinsicConversion(TargetTypeCode As TypeCode, SourceTypeCode As TypeCode) As ConversionClass
            Return ConversionResolution.ConversionTable(CInt(TargetTypeCode))(CInt(SourceTypeCode))
        End Function

        Friend Shared Function ClassifyPredefinedCLRConversion(TargetType As Type, SourceType As Type) As ConversionClass
            If (TargetType Is SourceType) Then
                Return ConversionClass.Identity
            End If
            If (Symbols.IsRootObjectType(TargetType) OrElse Symbols.IsOrInheritsFrom(SourceType, TargetType)) Then
                Return ConversionClass.Widening
            End If
            If (Symbols.IsRootObjectType(SourceType) OrElse Symbols.IsOrInheritsFrom(TargetType, SourceType)) Then
                Return ConversionClass.Narrowing
            End If
            If Symbols.IsInterface(SourceType) Then
                If ((Not Symbols.IsClass(TargetType) AndAlso Not Symbols.IsArrayType(TargetType)) AndAlso Not Symbols.IsGenericParameter(TargetType)) Then
                    If Symbols.IsInterface(TargetType) Then
                        Return ConversionClass.Narrowing
                    End If
                    If Symbols.IsValueType(TargetType) Then
                        If Symbols.Implements(TargetType, SourceType) Then
                            Return ConversionClass.Narrowing
                        End If
                        Return ConversionClass.None
                    End If
                End If
                Return ConversionClass.Narrowing
            End If
            If Symbols.IsInterface(TargetType) Then
                If Symbols.IsArrayType(SourceType) Then
                    Return ConversionResolution.ClassifyCLRArrayToInterfaceConversion(TargetType, SourceType)
                End If
                If Symbols.IsValueType(SourceType) Then
                    If Symbols.Implements(SourceType, TargetType) Then
                        Return ConversionClass.Widening
                    End If
                    Return ConversionClass.None
                End If
                If Symbols.IsClass(SourceType) Then
                    If Symbols.Implements(SourceType, TargetType) Then
                        Return ConversionClass.Widening
                    End If
                    Return ConversionClass.Narrowing
                End If
            End If
            If (Symbols.IsEnum(SourceType) OrElse Symbols.IsEnum(TargetType)) Then
                If (Symbols.GetTypeCode(SourceType) = Symbols.GetTypeCode(TargetType)) Then
                    If Symbols.IsEnum(TargetType) Then
                        Return ConversionClass.Narrowing
                    End If
                    Return ConversionClass.Widening
                End If
                Return ConversionClass.None
            End If
            If Symbols.IsGenericParameter(SourceType) Then
                If Not Symbols.IsClassOrInterface(TargetType) Then
                    Return ConversionClass.None
                End If
                Dim type2 As Type
                For Each type2 In Symbols.GetInterfaceConstraints(SourceType)
                    Select Case ConversionResolution.ClassifyPredefinedConversion(TargetType, type2)
                        Case ConversionClass.Widening, ConversionClass.Identity
                            Return ConversionClass.Widening
                    End Select
                Next
                Dim classConstraint As Type = Symbols.GetClassConstraint(SourceType)
                If (Not classConstraint Is Nothing) Then
                    Select Case ConversionResolution.ClassifyPredefinedConversion(TargetType, classConstraint)
                        Case ConversionClass.Widening, ConversionClass.Identity
                            Return ConversionClass.Widening
                    End Select
                End If
                Return Interaction.IIf(Of ConversionClass)(Symbols.IsInterface(TargetType), ConversionClass.Narrowing, ConversionClass.None)
            End If
            If Symbols.IsGenericParameter(TargetType) Then
                Dim derived As Type = Symbols.GetClassConstraint(TargetType)
                If ((Not derived Is Nothing) AndAlso Symbols.IsOrInheritsFrom(derived, SourceType)) Then
                    Return ConversionClass.Narrowing
                End If
                Return ConversionClass.None
            End If
            If (Symbols.IsArrayType(SourceType) AndAlso Symbols.IsArrayType(TargetType)) Then
                If (SourceType.GetArrayRank = TargetType.GetArrayRank) Then
                    Return ConversionResolution.ClassifyCLRConversionForArrayElementTypes(TargetType.GetElementType, SourceType.GetElementType)
                End If
                Return ConversionClass.None
            End If
            Return ConversionClass.None
        End Function

        Friend Shared Function ClassifyPredefinedConversion(TargetType As Type, SourceType As Type) As ConversionClass
            If (TargetType Is SourceType) Then
                Return ConversionClass.Identity
            End If
            Dim typeCode As TypeCode = Symbols.GetTypeCode(SourceType)
            Dim code2 As TypeCode = Symbols.GetTypeCode(TargetType)
            If (Symbols.IsIntrinsicType(typeCode) AndAlso Symbols.IsIntrinsicType(code2)) Then
                If ((Symbols.IsEnum(TargetType) AndAlso Symbols.IsIntegralType(typeCode)) AndAlso Symbols.IsIntegralType(code2)) Then
                    Return ConversionClass.Narrowing
                End If
                If ((typeCode = code2) AndAlso Symbols.IsEnum(SourceType)) Then
                    Return ConversionClass.Widening
                End If
                Return ConversionResolution.ClassifyIntrinsicConversion(code2, typeCode)
            End If
            If (Symbols.IsCharArrayRankOne(SourceType) AndAlso Symbols.IsStringType(TargetType)) Then
                Return ConversionClass.Widening
            End If
            If (Symbols.IsCharArrayRankOne(TargetType) AndAlso Symbols.IsStringType(SourceType)) Then
                Return ConversionClass.Narrowing
            End If
            Return ConversionResolution.ClassifyPredefinedCLRConversion(TargetType, SourceType)
        End Function

        Friend Shared Function ClassifyUserDefinedConversion(TargetType As Type, SourceType As Type, ByRef OperatorMethod As Method) As ConversionClass
            Dim class3 As ConversionClass
            Dim conversionCache As Object = OperatorCaches.ConversionCache
            SyncLock conversionCache
                If (OperatorCaches.UnconvertibleTypeCache.Lookup(TargetType) AndAlso OperatorCaches.UnconvertibleTypeCache.Lookup(SourceType)) Then
                    Return ConversionClass.None
                End If
                If OperatorCaches.ConversionCache.Lookup(TargetType, SourceType, class3, OperatorMethod) Then
                    Return class3
                End If
            End SyncLock
            Dim foundTargetTypeOperators As Boolean = False
            Dim foundSourceTypeOperators As Boolean = False
            class3 = ConversionResolution.DoClassifyUserDefinedConversion(TargetType, SourceType, OperatorMethod, foundTargetTypeOperators, foundSourceTypeOperators)
            Dim obj3 As Object = OperatorCaches.ConversionCache
            SyncLock obj3
                If Not foundTargetTypeOperators Then
                    OperatorCaches.UnconvertibleTypeCache.Insert(TargetType)
                End If
                If Not foundSourceTypeOperators Then
                    OperatorCaches.UnconvertibleTypeCache.Insert(SourceType)
                End If
                If (foundTargetTypeOperators OrElse foundSourceTypeOperators) Then
                    OperatorCaches.ConversionCache.Insert(TargetType, SourceType, class3, OperatorMethod)
                End If
            End SyncLock
            Return class3
        End Function

        Private Shared Function CollectConversionOperators(TargetType As Type, SourceType As Type, ByRef FoundTargetTypeOperators As Boolean, ByRef FoundSourceTypeOperators As Boolean) As List(Of Method)
            If Symbols.IsIntrinsicType(TargetType) Then
                TargetType = GetType(Object)
            End If
            If Symbols.IsIntrinsicType(SourceType) Then
                SourceType = GetType(Object)
            End If
            Dim collection As List(Of Method) = Operators.CollectOperators(UserDefinedOperator.Narrow, TargetType, SourceType, FoundTargetTypeOperators, FoundSourceTypeOperators)
            Dim list1 As List(Of Method) = Operators.CollectOperators(UserDefinedOperator.Widen, TargetType, SourceType, FoundTargetTypeOperators, FoundSourceTypeOperators)
            list1.AddRange(collection)
            Return list1
        End Function

        Private Shared Function DoClassifyUserDefinedConversion(TargetType As Type, SourceType As Type, ByRef OperatorMethod As Method, ByRef FoundTargetTypeOperators As Boolean, ByRef FoundSourceTypeOperators As Boolean) As ConversionClass
            OperatorMethod = Nothing
            Dim operatorSet As List(Of Method) = ConversionResolution.CollectConversionOperators(TargetType, SourceType, FoundTargetTypeOperators, FoundSourceTypeOperators)
            If (operatorSet.Count = 0) Then
                Return ConversionClass.None
            End If
            Dim resolutionIsAmbiguous As Boolean = False
            Dim list2 As List(Of Method) = ConversionResolution.ResolveConversion(TargetType, SourceType, operatorSet, True, resolutionIsAmbiguous)
            If (list2.Count = 1) Then
                OperatorMethod = list2.Item(0)
                OperatorMethod.ArgumentsValidated = True
                Return ConversionClass.Widening
            End If
            If ((list2.Count = 0) AndAlso Not resolutionIsAmbiguous) Then
                list2 = ConversionResolution.ResolveConversion(TargetType, SourceType, operatorSet, False, resolutionIsAmbiguous)
                If (list2.Count = 1) Then
                    OperatorMethod = list2.Item(0)
                    OperatorMethod.ArgumentsValidated = True
                    Return ConversionClass.Narrowing
                End If
                If (list2.Count = 0) Then
                    Return ConversionClass.None
                End If
            End If
            Return ConversionClass.Ambiguous
        End Function

        Private Shared Function Encompasses(Larger As Type, Smaller As Type) As Boolean
            Dim class2 As ConversionClass = ConversionResolution.ClassifyPredefinedConversion(Larger, Smaller)
            If (class2 <> ConversionClass.Widening) Then
                Return (class2 = ConversionClass.Identity)
            End If
            Return True
        End Function

        Private Shared Sub FindBestMatch(TargetType As Type, SourceType As Type, SearchList As List(Of Method), ResultList As List(Of Method), ByRef GenericMembersExistInList As Boolean)
            Dim enumerator As Enumerator(Of Method)
            Try
                enumerator = SearchList.GetEnumerator
                Do While enumerator.MoveNext
                    Dim current As Method = enumerator.Current
                    Dim base1 As MethodBase = current.AsMethod
                    Dim parameterType As Type = base1.GetParameters(0).ParameterType
                    Dim returnType As Type = DirectCast(base1, MethodInfo).ReturnType
                    If ((parameterType Is SourceType) AndAlso (returnType Is TargetType)) Then
                        ConversionResolution.InsertInOperatorListIfLessGenericThanExisting(current, ResultList, GenericMembersExistInList)
                    End If
                Loop
            Finally
                enumerator.Dispose
            End Try
        End Sub

        Private Shared Sub InsertInOperatorListIfLessGenericThanExisting(OperatorToInsert As Method, OperatorList As List(Of Method), ByRef GenericMembersExistInList As Boolean)
            If Symbols.IsGeneric(OperatorToInsert.DeclaringType) Then
                GenericMembersExistInList = True
            End If
            If GenericMembersExistInList Then
                Dim i As Integer = (OperatorList.Count - 1)
                Do While (i >= 0)
                    Dim left As Method = OperatorList.Item(i)
                    Dim method2 As Method = OverloadResolution.LeastGenericProcedure(left, OperatorToInsert)
                    If (method2 Is left) Then
                        Return
                    End If
                    If (Not method2 Is Nothing) Then
                        OperatorList.Remove(left)
                    End If
                    i = (i + -1)
                Loop
            End If
            OperatorList.Add(OperatorToInsert)
        End Sub

        Private Shared Function MostEncompassed(Types As List(Of Type)) As Type
            Dim larger As Type = Types.Item(0)
            Dim num As Integer = (Types.Count - 1)
            Dim i As Integer = 1
            Do While (i <= num)
                Dim smaller As Type = Types.Item(i)
                If ConversionResolution.Encompasses(larger, smaller) Then
                    larger = smaller
                ElseIf Not ConversionResolution.Encompasses(smaller, larger) Then
                    Return Nothing
                End If
                i += 1
            Loop
            Return larger
        End Function

        Private Shared Function MostEncompassing(Types As List(Of Type)) As Type
            Dim smaller As Type = Types.Item(0)
            Dim num As Integer = (Types.Count - 1)
            Dim i As Integer = 1
            Do While (i <= num)
                Dim larger As Type = Types.Item(i)
                If ConversionResolution.Encompasses(larger, smaller) Then
                    smaller = larger
                ElseIf Not ConversionResolution.Encompasses(smaller, larger) Then
                    Return Nothing
                End If
                i += 1
            Loop
            Return smaller
        End Function

        Private Shared Function NotEncompasses(Larger As Type, Smaller As Type) As Boolean
            Dim class2 As ConversionClass = ConversionResolution.ClassifyPredefinedConversion(Larger, Smaller)
            If (class2 <> ConversionClass.Narrowing) Then
                Return (class2 = ConversionClass.Identity)
            End If
            Return True
        End Function

        Private Shared Function ResolveConversion(TargetType As Type, SourceType As Type, OperatorSet As List(Of Method), WideningOnly As Boolean, ByRef ResolutionIsAmbiguous As Boolean) As List(Of Method)
            ResolutionIsAmbiguous = False
            Dim sourceType As Type = Nothing
            Dim targetType As Type = Nothing
            Dim genericMembersExistInList As Boolean = False
            Dim operatorList As New List(Of Method)(OperatorSet.Count)
            Dim searchList As New List(Of Method)(OperatorSet.Count)
            Dim types As New List(Of Type)(OperatorSet.Count)
            Dim list5 As New List(Of Type)(OperatorSet.Count)
            Dim list6 As List(Of Type) = Nothing
            Dim list7 As List(Of Type) = Nothing
            If Not WideningOnly Then
                list6 = New List(Of Type)(OperatorSet.Count)
                list7 = New List(Of Type)(OperatorSet.Count)
            End If
            Dim method As Method
            For Each method In OperatorSet
                Dim base2 As MethodBase = method.AsMethod
                If (WideningOnly AndAlso Symbols.IsNarrowingConversionOperator(base2)) Then
                    Exit For
                End If
                Dim parameterType As Type = base2.GetParameters(0).ParameterType
                Dim returnType As Type = DirectCast(base2, MethodInfo).ReturnType
                If ((Not Symbols.IsGeneric(base2) AndAlso Not Symbols.IsGeneric(base2.DeclaringType)) OrElse (ConversionResolution.ClassifyPredefinedConversion(returnType, parameterType) = ConversionClass.None)) Then
                    If ((parameterType Is sourceType) AndAlso (returnType Is targetType)) Then
                        ConversionResolution.InsertInOperatorListIfLessGenericThanExisting(method, operatorList, genericMembersExistInList)
                    ElseIf (operatorList.Count = 0) Then
                        If (ConversionResolution.Encompasses(parameterType, sourceType) AndAlso ConversionResolution.Encompasses(targetType, returnType)) Then
                            searchList.Add(method)
                            If (parameterType Is sourceType) Then
                                sourceType = parameterType
                            Else
                                types.Add(parameterType)
                            End If
                            If (returnType Is targetType) Then
                                targetType = returnType
                            Else
                                list5.Add(returnType)
                            End If
                        ElseIf ((Not WideningOnly AndAlso ConversionResolution.Encompasses(parameterType, sourceType)) AndAlso ConversionResolution.NotEncompasses(targetType, returnType)) Then
                            searchList.Add(method)
                            If (parameterType Is sourceType) Then
                                sourceType = parameterType
                            Else
                                types.Add(parameterType)
                            End If
                            If (returnType Is targetType) Then
                                targetType = returnType
                            Else
                                list7.Add(returnType)
                            End If
                        ElseIf ((Not WideningOnly AndAlso ConversionResolution.NotEncompasses(parameterType, sourceType)) AndAlso ConversionResolution.NotEncompasses(targetType, returnType)) Then
                            searchList.Add(method)
                            If (parameterType Is sourceType) Then
                                sourceType = parameterType
                            Else
                                list6.Add(parameterType)
                            End If
                            If (returnType Is targetType) Then
                                targetType = returnType
                            Else
                                list7.Add(returnType)
                            End If
                        End If
                    End If
                End If
            Next
            If ((operatorList.Count = 0) AndAlso (searchList.Count > 0)) Then
                If (sourceType Is Nothing) Then
                    If (types.Count > 0) Then
                        sourceType = ConversionResolution.MostEncompassed(types)
                    Else
                        sourceType = ConversionResolution.MostEncompassing(list6)
                    End If
                End If
                If (targetType Is Nothing) Then
                    If (list5.Count > 0) Then
                        targetType = ConversionResolution.MostEncompassing(list5)
                    Else
                        targetType = ConversionResolution.MostEncompassed(list7)
                    End If
                End If
                If ((sourceType Is Nothing) OrElse (targetType Is Nothing)) Then
                    ResolutionIsAmbiguous = True
                    Return New List(Of Method)
                End If
                ConversionResolution.FindBestMatch(targetType, sourceType, searchList, operatorList, genericMembersExistInList)
            End If
            If (operatorList.Count > 1) Then
                ResolutionIsAmbiguous = True
            End If
            Return operatorList
        End Function

        <Conditional("DEBUG")>
        Private Shared Sub VerifyTypeCodeEnum()
        End Sub


        ' Fields
        Private Shared ReadOnly ConversionTable As ConversionClass()() = New ConversionClass()() {New ConversionClass(&H13 - 1) {}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass(&H13 - 1) {}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass() {ConversionClass.Bad}, New ConversionClass(&H13 - 1) {}, New ConversionClass() {ConversionClass.Bad}}
        Friend Shared ReadOnly ForLoopWidestTypeCode As TypeCode()()
        Friend Shared ReadOnly NumericSpecificityRank As Integer() = New Integer(&H13 - 1) {}

        ' Nested Types
        Friend Enum ConversionClass As SByte
            ' Fields
            Ambiguous = 5
            Bad = 0
            Identity = 1
            [Narrowing] = 3
            None = 4
            [Widening] = 2
        End Enum
    End Class
End Namespace

