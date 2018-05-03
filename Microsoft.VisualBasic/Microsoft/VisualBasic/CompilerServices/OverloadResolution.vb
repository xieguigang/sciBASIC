Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text

Namespace Microsoft.VisualBasic.CompilerServices
    Friend Class OverloadResolution
        ' Methods
        Private Sub New()
        End Sub

        Private Shared Function CanConvert(TargetType As Type, SourceType As Type, RejectNarrowingConversion As Boolean, Errors As List(Of String), ParameterName As String, IsByRefCopyBackContext As Boolean, ByRef RequiresNarrowingConversion As Boolean, ByRef AllNarrowingIsFromObject As Boolean) As Boolean
            Dim operatorMethod As Method = Nothing
            Dim class2 As ConversionClass = ConversionResolution.ClassifyConversion(TargetType, SourceType, operatorMethod)
            Select Case class2
                Case ConversionClass.Identity, ConversionClass.Widening
                    Return True
                Case ConversionClass.Narrowing
                    If Not RejectNarrowingConversion Then
                        RequiresNarrowingConversion = True
                        If (Not SourceType Is GetType(Object)) Then
                            AllNarrowingIsFromObject = False
                        End If
                        Return True
                    End If
                    If (Not Errors Is Nothing) Then
                        OverloadResolution.ReportError(Errors, Interaction.IIf(Of String)(IsByRefCopyBackContext, "ArgumentNarrowingCopyBack3", "ArgumentNarrowing3"), ParameterName, SourceType, TargetType)
                    End If
                    Return False
            End Select
            If (Not Errors Is Nothing) Then
                OverloadResolution.ReportError(Errors, Interaction.IIf(Of String)((class2 = ConversionClass.Ambiguous), Interaction.IIf(Of String)(IsByRefCopyBackContext, "ArgumentMismatchAmbiguousCopyBack3", "ArgumentMismatchAmbiguous3"), Interaction.IIf(Of String)(IsByRefCopyBackContext, "ArgumentMismatchCopyBack3", "ArgumentMismatch3")), ParameterName, SourceType, TargetType)
            End If
            Return False
        End Function

        Private Shared Function CandidateIsNarrowing(Candidate As Method) As Boolean
            Return (Not Candidate.NotCallable AndAlso Candidate.RequiresNarrowingConversion)
        End Function

        Private Shared Function CandidateIsNotCallable(Candidate As Method) As Boolean
            Return Candidate.NotCallable
        End Function

        Private Shared Function CandidateIsUnspecific(Candidate As Method) As Boolean
            Return ((Not Candidate.NotCallable AndAlso Not Candidate.RequiresNarrowingConversion) AndAlso Not Candidate.LessSpecific)
        End Function

        Friend Shared Function CanMatchArguments(TargetProcedure As Method, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), RejectNarrowingConversions As Boolean, Errors As List(Of String)) As Boolean
            Dim flag2 As Boolean = (Errors IsNot Nothing)
            TargetProcedure.ArgumentsValidated = True
            If (TargetProcedure.IsMethod AndAlso Symbols.IsRawGeneric(TargetProcedure.AsMethod)) Then
                If (TypeArguments.Length = 0) Then
                    TypeArguments = New Type(((TargetProcedure.TypeParameters.Length - 1) + 1) - 1) {}
                    TargetProcedure.TypeArguments = TypeArguments
                    If Not OverloadResolution.InferTypeArguments(TargetProcedure, Arguments, ArgumentNames, TypeArguments, Errors) Then
                        Return False
                    End If
                Else
                    TargetProcedure.TypeArguments = TypeArguments
                End If
                If Not OverloadResolution.InstantiateGenericMethod(TargetProcedure, TypeArguments, Errors) Then
                    Return False
                End If
            End If
            Dim parameters As ParameterInfo() = TargetProcedure.Parameters
            Dim length As Integer = ArgumentNames.Length
            Dim index As Integer = 0
            Do While (length < Arguments.Length)
                If (index = TargetProcedure.ParamArrayIndex) Then
                    Exit Do
                End If
                If (Not OverloadResolution.CanPassToParameter(TargetProcedure, Arguments(length), parameters(index), False, RejectNarrowingConversions, Errors, TargetProcedure.RequiresNarrowingConversion, TargetProcedure.AllNarrowingIsFromObject) AndAlso Not flag2) Then
                    Return False
                End If
                length += 1
                index += 1
            Loop
            If TargetProcedure.HasParamArray Then
                If Not TargetProcedure.ParamArrayExpanded Then
                    If ((Arguments.Length - length) <> 1) Then
                        Return False
                    End If
                    If Not OverloadResolution.CanPassToParamArray(TargetProcedure, Arguments(length), parameters(index)) Then
                        If flag2 Then
                            OverloadResolution.ReportError(Errors, "ArgumentMismatch3", parameters(index).Name, OverloadResolution.GetArgumentTypeInContextOfParameterType(Arguments(length), parameters(index).ParameterType), parameters(index).ParameterType)
                        End If
                        Return False
                    End If
                ElseIf ((length <> (Arguments.Length - 1)) OrElse (Not Arguments(length) Is Nothing)) Then
                    Do While (length < Arguments.Length)
                        If (Not OverloadResolution.CanPassToParameter(TargetProcedure, Arguments(length), parameters(index), True, RejectNarrowingConversions, Errors, TargetProcedure.RequiresNarrowingConversion, TargetProcedure.AllNarrowingIsFromObject) AndAlso Not flag2) Then
                            Return False
                        End If
                        length += 1
                    Loop
                Else
                    Return False
                End If
                index += 1
            End If
            Dim flagArray As Boolean() = Nothing
            If ((ArgumentNames.Length > 0) OrElse (index < parameters.Length)) Then
                flagArray = OverloadResolution.CreateMatchTable(parameters.Length, (index - 1))
            End If
            If (ArgumentNames.Length > 0) Then
                Dim numArray As Integer() = New Integer(((ArgumentNames.Length - 1) + 1) - 1) {}
                length
                For length = 0 To ArgumentNames.Length - 1
                    If Not OverloadResolution.FindParameterByName(parameters, ArgumentNames(length), index) Then
                        If Not flag2 Then
                            Return False
                        End If
                        OverloadResolution.ReportError(Errors, "NamedParamNotFound2", ArgumentNames(length), TargetProcedure)
                    ElseIf (index = TargetProcedure.ParamArrayIndex) Then
                        If Not flag2 Then
                            Return False
                        End If
                        OverloadResolution.ReportError(Errors, "NamedParamArrayArgument1", ArgumentNames(length))
                    ElseIf flagArray(index) Then
                        If Not flag2 Then
                            Return False
                        End If
                        OverloadResolution.ReportError(Errors, "NamedArgUsedTwice2", ArgumentNames(length), TargetProcedure)
                    Else
                        If (Not OverloadResolution.CanPassToParameter(TargetProcedure, Arguments(length), parameters(index), False, RejectNarrowingConversions, Errors, TargetProcedure.RequiresNarrowingConversion, TargetProcedure.AllNarrowingIsFromObject) AndAlso Not flag2) Then
                            Return False
                        End If
                        flagArray(index) = True
                        numArray(length) = index
                    End If
                Next length
                TargetProcedure.NamedArgumentMapping = numArray
            End If
            If (Not flagArray Is Nothing) Then
                Dim num3 As Integer = (flagArray.Length - 1)
                Dim i As Integer = 0
                Do While (i <= num3)
                    If Not flagArray(i) Then
                        If parameters(i).IsOptional Then
                            TargetProcedure.UsedDefaultForAnOptionalParameter = True
                        Else
                            If Not flag2 Then
                                Return False
                            End If
                            OverloadResolution.ReportError(Errors, "OmittedArgument1", parameters(i).Name)
                        End If
                    End If
                    i += 1
                Loop
            End If
            If ((Not Errors Is Nothing) AndAlso (Errors.Count > 0)) Then
                Return False
            End If
            Return True
        End Function

        Private Shared Function CanPassToParamArray(TargetProcedure As Method, Argument As Object, Parameter As ParameterInfo) As Boolean
            If (Argument Is Nothing) Then
                Return True
            End If
            Dim parameterType As Type = Parameter.ParameterType
            Dim argumentTypeInContextOfParameterType As Type = OverloadResolution.GetArgumentTypeInContextOfParameterType(Argument, parameterType)
            Dim operatorMethod As Method = Nothing
            Dim class2 As ConversionClass = ConversionResolution.ClassifyConversion(parameterType, argumentTypeInContextOfParameterType, operatorMethod)
            Return ((class2 = ConversionClass.Widening) OrElse (class2 = ConversionClass.Identity))
        End Function

        Friend Shared Function CanPassToParameter(TargetProcedure As Method, Argument As Object, Parameter As ParameterInfo, IsExpandedParamArray As Boolean, RejectNarrowingConversions As Boolean, Errors As List(Of String), ByRef RequiresNarrowingConversion As Boolean, ByRef AllNarrowingIsFromObject As Boolean) As Boolean
            If (Argument Is Nothing) Then
                Return True
            End If
            Dim parameterType As Type = Parameter.ParameterType
            Dim isByRef As Boolean = parameterType.IsByRef
            If (isByRef OrElse IsExpandedParamArray) Then
                parameterType = Symbols.GetElementType(parameterType)
            End If
            Dim argumentTypeInContextOfParameterType As Type = OverloadResolution.GetArgumentTypeInContextOfParameterType(Argument, parameterType)
            If (Argument Is Missing.Value) Then
                If Parameter.IsOptional Then
                    Return True
                End If
                If (Not Symbols.IsRootObjectType(parameterType) OrElse Not IsExpandedParamArray) Then
                    If (Not Errors Is Nothing) Then
                        If IsExpandedParamArray Then
                            OverloadResolution.ReportError(Errors, "OmittedParamArrayArgument")
                        Else
                            OverloadResolution.ReportError(Errors, "OmittedArgument1", Parameter.Name)
                        End If
                    End If
                    Return False
                End If
            End If
            Dim flag3 As Boolean = OverloadResolution.CanConvert(parameterType, argumentTypeInContextOfParameterType, RejectNarrowingConversions, Errors, Parameter.Name, False, RequiresNarrowingConversion, AllNarrowingIsFromObject)
            If (Not isByRef OrElse Not flag3) Then
                Return flag3
            End If
            Return OverloadResolution.CanConvert(argumentTypeInContextOfParameterType, parameterType, RejectNarrowingConversions, Errors, Parameter.Name, True, RequiresNarrowingConversion, AllNarrowingIsFromObject)
        End Function

        Friend Shared Function CollectOverloadCandidates(Members As MemberInfo(), Arguments As Object(), ArgumentCount As Integer, ArgumentNames As String(), TypeArguments As Type(), CollectOnlyOperators As Boolean, TerminatingScope As Type, ByRef RejectedForArgumentCount As Integer, ByRef RejectedForTypeArgumentCount As Integer, BaseReference As Container) As List(Of Method)
            Dim type As Type
            Dim info As MemberInfo
            Dim length As Integer = 0
            If (Not TypeArguments Is Nothing) Then
                length = TypeArguments.Length
            End If
            Dim candidates As New List(Of Method)(Members.Length)
            If (Members.Length = 0) Then
                Return candidates
            End If
            Dim flag As Boolean = True
            Dim index As Integer = 0
Label_0025:
            type = Members(index).DeclaringType
            If ((Not TerminatingScope Is Nothing) AndAlso Symbols.IsOrInheritsFrom(TerminatingScope, type)) Then
                GoTo Label_020E
            End If
Label_0042:
            info = Members(index)
            Dim destinationArray As ParameterInfo() = Nothing
            Dim num3 As Integer = 0
            Select Case info.MemberType
                Case MemberTypes.TypeInfo, MemberTypes.Custom, MemberTypes.NestedType, MemberTypes.Event, MemberTypes.Field
                    If Not CollectOnlyOperators Then
                        flag = False
                    End If
                    GoTo Label_01E4
                Case MemberTypes.Constructor, MemberTypes.Method
                    Dim method As MethodBase = DirectCast(info, MethodBase)
                    If (CollectOnlyOperators AndAlso Not Symbols.IsUserDefinedOperator(method)) Then
                        GoTo Label_01E4
                    End If
                    destinationArray = method.GetParameters
                    num3 = Symbols.GetTypeParameters(method).Length
                    If Symbols.IsShadows(method) Then
                        flag = False
                    End If
                    Exit Select
                Case MemberTypes.Property
                    If CollectOnlyOperators Then
                        GoTo Label_01E4
                    End If
                    Dim info2 As PropertyInfo = DirectCast(info, PropertyInfo)
                    Dim getMethod As MethodInfo = info2.GetGetMethod
                    If (Not getMethod Is Nothing) Then
                        destinationArray = getMethod.GetParameters
                        If Symbols.IsShadows(getMethod) Then
                            flag = False
                        End If
                    Else
                        Dim setMethod As MethodInfo = info2.GetSetMethod
                        Dim parameters As ParameterInfo() = setMethod.GetParameters
                        destinationArray = New ParameterInfo(((parameters.Length - 2) + 1) - 1) {}
                        Array.Copy(parameters, destinationArray, destinationArray.Length)
                        If Symbols.IsShadows(setMethod) Then
                            flag = False
                        End If
                    End If
                    Exit Select
                Case Else
                    GoTo Label_01E4
            End Select
            Dim requiredParameterCount As Integer = 0
            Dim maximumParameterCount As Integer = 0
            Dim paramArrayIndex As Integer = -1
            Symbols.GetAllParameterCounts(destinationArray, requiredParameterCount, maximumParameterCount, paramArrayIndex)
            Dim flag2 As Boolean = (paramArrayIndex >= 0)
            If ((ArgumentCount < requiredParameterCount) OrElse (Not flag2 AndAlso (ArgumentCount > maximumParameterCount))) Then
                RejectedForArgumentCount += 1
            ElseIf ((length > 0) AndAlso (length <> num3)) Then
                RejectedForTypeArgumentCount += 1
            Else
                If (Not flag2 OrElse (ArgumentCount = maximumParameterCount)) Then
                    OverloadResolution.InsertIfMethodAvailable(info, destinationArray, paramArrayIndex, False, Arguments, ArgumentCount, ArgumentNames, TypeArguments, CollectOnlyOperators, candidates, BaseReference)
                End If
                If flag2 Then
                    OverloadResolution.InsertIfMethodAvailable(info, destinationArray, paramArrayIndex, True, Arguments, ArgumentCount, ArgumentNames, TypeArguments, CollectOnlyOperators, candidates, BaseReference)
                End If
            End If
Label_01E4:
            index += 1
            If ((index < Members.Length) AndAlso (Members(index).DeclaringType Is type)) Then
                GoTo Label_0042
            End If
            If (flag AndAlso (index < Members.Length)) Then
                GoTo Label_0025
            End If
Label_020E:
            index = 0
            Do While (index < candidates.Count)
                If (candidates.Item(index) Is Nothing) Then
                    Dim num7 As Integer = (index + 1)
                    Do While ((num7 < candidates.Count) AndAlso (candidates.Item(num7) Is Nothing))
                        num7 += 1
                    Loop
                    candidates.RemoveRange(index, (num7 - index))
                End If
                index += 1
            Loop
            Return candidates
        End Function

        Private Shared Sub CompareGenericityBasedOnMethodGenericParams(LeftParameter As ParameterInfo, RawLeftParameter As ParameterInfo, LeftMember As Method, ExpandLeftParamArray As Boolean, RightParameter As ParameterInfo, RawRightParameter As ParameterInfo, RightMember As Method, ExpandRightParamArray As Boolean, ByRef LeftIsLessGeneric As Boolean, ByRef RightIsLessGeneric As Boolean, ByRef SignatureMismatch As Boolean)
            If (LeftMember.IsMethod AndAlso RightMember.IsMethod) Then
                Dim parameterType As Type = LeftParameter.ParameterType
                Dim type As Type = RightParameter.ParameterType
                Dim elementType As Type = RawLeftParameter.ParameterType
                Dim type4 As Type = RawRightParameter.ParameterType
                If parameterType.IsByRef Then
                    parameterType = Symbols.GetElementType(parameterType)
                    elementType = Symbols.GetElementType(elementType)
                End If
                If type.IsByRef Then
                    type = Symbols.GetElementType(type)
                    type4 = Symbols.GetElementType(type4)
                End If
                If (ExpandLeftParamArray AndAlso Symbols.IsParamArray(LeftParameter)) Then
                    parameterType = Symbols.GetElementType(parameterType)
                    elementType = Symbols.GetElementType(elementType)
                End If
                If (ExpandRightParamArray AndAlso Symbols.IsParamArray(RightParameter)) Then
                    type = Symbols.GetElementType(type)
                    type4 = Symbols.GetElementType(type4)
                End If
                If ((Not parameterType Is type) AndAlso Not Symbols.IsEquivalentType(parameterType, type)) Then
                    SignatureMismatch = True
                Else
                    Dim method As MethodBase = LeftMember.AsMethod
                    Dim genericMethodDefinition As MethodBase = RightMember.AsMethod
                    If Symbols.IsGeneric(method) Then
                        method = DirectCast(method, MethodInfo).GetGenericMethodDefinition
                    End If
                    If Symbols.IsGeneric(genericMethodDefinition) Then
                        genericMethodDefinition = DirectCast(genericMethodDefinition, MethodInfo).GetGenericMethodDefinition
                    End If
                    If Symbols.RefersToGenericParameter(elementType, method) Then
                        If Not Symbols.RefersToGenericParameter(type4, genericMethodDefinition) Then
                            RightIsLessGeneric = True
                        End If
                    ElseIf (Symbols.RefersToGenericParameter(type4, genericMethodDefinition) AndAlso Not Symbols.RefersToGenericParameter(elementType, method)) Then
                        LeftIsLessGeneric = True
                    End If
                End If
            End If
        End Sub

        Private Shared Sub CompareGenericityBasedOnTypeGenericParams(LeftParameter As ParameterInfo, RawLeftParameter As ParameterInfo, LeftMember As Method, ExpandLeftParamArray As Boolean, RightParameter As ParameterInfo, RawRightParameter As ParameterInfo, RightMember As Method, ExpandRightParamArray As Boolean, ByRef LeftIsLessGeneric As Boolean, ByRef RightIsLessGeneric As Boolean, ByRef SignatureMismatch As Boolean)
            Dim parameterType As Type = LeftParameter.ParameterType
            Dim type As Type = RightParameter.ParameterType
            Dim elementType As Type = RawLeftParameter.ParameterType
            Dim type4 As Type = RawRightParameter.ParameterType
            If parameterType.IsByRef Then
                parameterType = Symbols.GetElementType(parameterType)
                elementType = Symbols.GetElementType(elementType)
            End If
            If type.IsByRef Then
                type = Symbols.GetElementType(type)
                type4 = Symbols.GetElementType(type4)
            End If
            If (ExpandLeftParamArray AndAlso Symbols.IsParamArray(LeftParameter)) Then
                parameterType = Symbols.GetElementType(parameterType)
                elementType = Symbols.GetElementType(elementType)
            End If
            If (ExpandRightParamArray AndAlso Symbols.IsParamArray(RightParameter)) Then
                type = Symbols.GetElementType(type)
                type4 = Symbols.GetElementType(type4)
            End If
            If ((Not parameterType Is type) AndAlso Not Symbols.IsEquivalentType(parameterType, type)) Then
                SignatureMismatch = True
            Else
                Dim rawDeclaringType As Type = LeftMember.RawDeclaringType
                Dim typ As Type = RightMember.RawDeclaringType
                If Symbols.RefersToGenericParameterCLRSemantics(elementType, rawDeclaringType) Then
                    If Not Symbols.RefersToGenericParameterCLRSemantics(type4, typ) Then
                        RightIsLessGeneric = True
                    End If
                ElseIf Symbols.RefersToGenericParameterCLRSemantics(type4, typ) Then
                    LeftIsLessGeneric = True
                End If
            End If
        End Sub

        Private Shared Sub CompareNumericTypeSpecificity(LeftType As Type, RightType As Type, ByRef LeftWins As Boolean, ByRef RightWins As Boolean)
            If (Not LeftType Is RightType) Then
                If (ConversionResolution.NumericSpecificityRank(CInt(Symbols.GetTypeCode(LeftType))) < ConversionResolution.NumericSpecificityRank(CInt(Symbols.GetTypeCode(RightType)))) Then
                    LeftWins = True
                Else
                    RightWins = True
                End If
            End If
        End Sub

        Private Shared Sub CompareParameterSpecificity(ArgumentType As Type, LeftParameter As ParameterInfo, LeftProcedure As MethodBase, ExpandLeftParamArray As Boolean, RightParameter As ParameterInfo, RightProcedure As MethodBase, ExpandRightParamArray As Boolean, ByRef LeftWins As Boolean, ByRef RightWins As Boolean, ByRef BothLose As Boolean)
            BothLose = False
            Dim parameterType As Type = LeftParameter.ParameterType
            Dim type As Type = RightParameter.ParameterType
            If parameterType.IsByRef Then
                parameterType = Symbols.GetElementType(parameterType)
            End If
            If type.IsByRef Then
                type = Symbols.GetElementType(type)
            End If
            If (ExpandLeftParamArray AndAlso Symbols.IsParamArray(LeftParameter)) Then
                parameterType = Symbols.GetElementType(parameterType)
            End If
            If (ExpandRightParamArray AndAlso Symbols.IsParamArray(RightParameter)) Then
                type = Symbols.GetElementType(type)
            End If
            If ((Symbols.IsNumericType(parameterType) AndAlso Symbols.IsNumericType(type)) AndAlso (Not Symbols.IsEnum(parameterType) AndAlso Not Symbols.IsEnum(type))) Then
                OverloadResolution.CompareNumericTypeSpecificity(parameterType, type, LeftWins, RightWins)
            Else
                If (((Not LeftProcedure Is Nothing) AndAlso (Not RightProcedure Is Nothing)) AndAlso (Symbols.IsRawGeneric(LeftProcedure) AndAlso Symbols.IsRawGeneric(RightProcedure))) Then
                    If (parameterType Is type) Then
                        Return
                    End If
                    Dim num As Integer = Symbols.IndexIn(parameterType, LeftProcedure)
                    Dim num2 As Integer = Symbols.IndexIn(type, RightProcedure)
                    If ((num = num2) AndAlso (num >= 0)) Then
                        Return
                    End If
                End If
                Dim operatorMethod As Method = Nothing
                Select Case ConversionResolution.ClassifyConversion(type, parameterType, operatorMethod)
                    Case ConversionClass.Identity
                        Exit Select
                    Case ConversionClass.Widening
                        If ((Not operatorMethod Is Nothing) AndAlso (ConversionResolution.ClassifyConversion(parameterType, type, operatorMethod) = ConversionClass.Widening)) Then
                            If ((Not ArgumentType Is Nothing) AndAlso (ArgumentType Is parameterType)) Then
                                LeftWins = True
                                Return
                            End If
                            If ((Not ArgumentType Is Nothing) AndAlso (ArgumentType Is type)) Then
                                RightWins = True
                                Return
                            End If
                            BothLose = True
                            Return
                        End If
                        LeftWins = True
                        Return
                    Case Else
                        If (ConversionResolution.ClassifyConversion(parameterType, type, operatorMethod) = ConversionClass.Widening) Then
                            RightWins = True
                            Return
                        End If
                        BothLose = True
                        Exit Select
                End Select
            End If
        End Sub

        Private Shared Function CreateMatchTable(Size As Integer, LastPositionalMatchIndex As Integer) As Boolean()
            Dim flagArray As Boolean() = New Boolean(((Size - 1) + 1) - 1) {}
            Dim num As Integer = LastPositionalMatchIndex
            Dim i As Integer = 0
            Do While (i <= num)
                flagArray(i) = True
                i += 1
            Loop
            Return flagArray
        End Function

        Private Shared Function DetectArgumentErrors(TargetProcedure As Method, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), Errors As List(Of String)) As Boolean
            Return OverloadResolution.CanMatchArguments(TargetProcedure, Arguments, ArgumentNames, TypeArguments, False, Errors)
        End Function

        Private Shared Function DetectArgumentNarrowing(TargetProcedure As Method, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), Errors As List(Of String)) As Boolean
            Return OverloadResolution.CanMatchArguments(TargetProcedure, Arguments, ArgumentNames, TypeArguments, True, Errors)
        End Function

        Private Shared Function DetectUnspecificity(TargetProcedure As Method, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), Errors As List(Of String)) As Boolean
            OverloadResolution.ReportError(Errors, "NotMostSpecificOverload")
            Return False
        End Function

        Private Shared Function FindParameterByName(Parameters As ParameterInfo(), Name As String, ByRef Index As Integer) As Boolean
            Dim i As Integer
            For i = 0 To Parameters.Length - 1
                If (Operators.CompareString(Name, Parameters(i).Name, True) = 0) Then
                    Index = i
                    Return True
                End If
            Next i
            Return False
        End Function

        Private Shared Function GetArgumentType(Argument As Object) As Type
            If (Argument Is Nothing) Then
                Return Nothing
            End If
            Dim nothing As TypedNothing = TryCast(Argument, TypedNothing)
            If (Not [nothing] Is Nothing) Then
                Return [nothing].Type
            End If
            Return Argument.GetType
        End Function

        Private Shared Function GetArgumentTypeInContextOfParameterType(Argument As Object, ParameterType As Type) As Type
            Dim argumentType As Type = OverloadResolution.GetArgumentType(Argument)
            If ((argumentType Is Nothing) OrElse (ParameterType Is Nothing)) Then
                Return argumentType
            End If
            If (((Not ParameterType.IsImport OrElse Not ParameterType.IsInterface) OrElse Not ParameterType.IsInstanceOfType(Argument)) AndAlso Not Symbols.IsEquivalentType(argumentType, ParameterType)) Then
                Return argumentType
            End If
            Return ParameterType
        End Function

        Private Shared Function InferTypeArguments(TargetProcedure As Method, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), Errors As List(Of String)) As Boolean
            Dim flag2 As Boolean = (Errors IsNot Nothing)
            Dim rawParameters As ParameterInfo() = TargetProcedure.RawParameters
            Dim length As Integer = ArgumentNames.Length
            Dim index As Integer = 0
            Do While (length < Arguments.Length)
                If (index = TargetProcedure.ParamArrayIndex) Then
                    Exit Do
                End If
                If (Not OverloadResolution.InferTypeArgumentsFromArgument(TargetProcedure, Arguments(length), rawParameters(index), False, Errors) AndAlso Not flag2) Then
                    Return False
                End If
                length += 1
                index += 1
            Loop
            If TargetProcedure.HasParamArray Then
                If TargetProcedure.ParamArrayExpanded Then
                    Do While (length < Arguments.Length)
                        If (Not OverloadResolution.InferTypeArgumentsFromArgument(TargetProcedure, Arguments(length), rawParameters(index), True, Errors) AndAlso Not flag2) Then
                            Return False
                        End If
                        length += 1
                    Loop
                Else
                    If ((Arguments.Length - length) <> 1) Then
                        Return True
                    End If
                    If Not OverloadResolution.InferTypeArgumentsFromArgument(TargetProcedure, Arguments(length), rawParameters(index), False, Errors) Then
                        Return False
                    End If
                End If
                index += 1
            End If
            If (ArgumentNames.Length > 0) Then
                length
                For length = 0 To ArgumentNames.Length - 1
                    If ((OverloadResolution.FindParameterByName(rawParameters, ArgumentNames(length), index) AndAlso (index <> TargetProcedure.ParamArrayIndex)) AndAlso (Not OverloadResolution.InferTypeArgumentsFromArgument(TargetProcedure, Arguments(length), rawParameters(index), False, Errors) AndAlso Not flag2)) Then
                        Return False
                    End If
                Next length
            End If
            If ((Not Errors Is Nothing) AndAlso (Errors.Count > 0)) Then
                Return False
            End If
            Return True
        End Function

        Friend Shared Function InferTypeArgumentsFromArgument(TargetProcedure As Method, Argument As Object, Parameter As ParameterInfo, IsExpandedParamArray As Boolean, Errors As List(Of String)) As Boolean
            If (Not Argument Is Nothing) Then
                Dim parameterType As Type = Parameter.ParameterType
                If (parameterType.IsByRef OrElse IsExpandedParamArray) Then
                    parameterType = Symbols.GetElementType(parameterType)
                End If
                If Not OverloadResolution.InferTypeArgumentsFromArgument(OverloadResolution.GetArgumentTypeInContextOfParameterType(Argument, parameterType), parameterType, TargetProcedure.TypeArguments, TargetProcedure.AsMethod, True) Then
                    If (Not Errors Is Nothing) Then
                        OverloadResolution.ReportError(Errors, "TypeInferenceFails1", Parameter.Name)
                    End If
                    Return False
                End If
            End If
            Return True
        End Function

        Private Shared Function InferTypeArgumentsFromArgument(ArgumentType As Type, ParameterType As Type, TypeInferenceArguments As Type(), TargetProcedure As MethodBase, DigThroughToBasesAndImplements As Boolean) As Boolean
            Dim flag2 As Boolean = OverloadResolution.InferTypeArgumentsFromArgumentDirectly(ArgumentType, ParameterType, TypeInferenceArguments, TargetProcedure, DigThroughToBasesAndImplements)
            If ((flag2 OrElse Not DigThroughToBasesAndImplements) OrElse (Not Symbols.IsInstantiatedGeneric(ParameterType) OrElse (Not ParameterType.IsClass AndAlso Not ParameterType.IsInterface))) Then
                Return flag2
            End If
            Dim genericTypeDefinition As Type = ParameterType.GetGenericTypeDefinition
            If Symbols.IsArrayType(ArgumentType) Then
                If ((ArgumentType.GetArrayRank > 1) OrElse ParameterType.IsClass) Then
                    Return False
                End If
                Dim typeArguments As Type() = New Type() {ArgumentType.GetElementType}
                ArgumentType = GetType(IList(Of )).MakeGenericType(typeArguments)
                If (Not GetType(IList(Of )) Is genericTypeDefinition) Then
                    GoTo Label_00BA
                End If
                GoTo Label_0149
            End If
            If (Not ArgumentType.IsClass AndAlso Not ArgumentType.IsInterface) Then
                Return False
            End If
            If (Symbols.IsInstantiatedGeneric(ArgumentType) AndAlso (ArgumentType.GetGenericTypeDefinition Is genericTypeDefinition)) Then
                Return False
            End If
Label_00BA:
            If Not ParameterType.IsClass Then
                Dim type3 As Type = Nothing
                Dim type4 As Type
                For Each type4 In ArgumentType.GetInterfaces
                    If (Symbols.IsInstantiatedGeneric(type4) AndAlso (type4.GetGenericTypeDefinition Is genericTypeDefinition)) Then
                        If (Not type3 Is Nothing) Then
                            Return False
                        End If
                        type3 = type4
                    End If
                Next
                ArgumentType = type3
            Else
                If Not ArgumentType.IsClass Then
                    Return False
                End If
                Dim baseType As Type = ArgumentType.BaseType
                Do While (Not baseType Is Nothing)
                    If (Symbols.IsInstantiatedGeneric(baseType) AndAlso (baseType.GetGenericTypeDefinition Is genericTypeDefinition)) Then
                        Exit Do
                    End If
                    baseType = baseType.BaseType
                Loop
                ArgumentType = baseType
            End If
            If (ArgumentType Is Nothing) Then
                Return False
            End If
Label_0149:
            Return OverloadResolution.InferTypeArgumentsFromArgumentDirectly(ArgumentType, ParameterType, TypeInferenceArguments, TargetProcedure, DigThroughToBasesAndImplements)
        End Function

        Private Shared Function InferTypeArgumentsFromArgumentDirectly(ArgumentType As Type, ParameterType As Type, TypeInferenceArguments As Type(), TargetProcedure As MethodBase, DigThroughToBasesAndImplements As Boolean) As Boolean
            If Symbols.RefersToGenericParameter(ParameterType, TargetProcedure) Then
                If Symbols.IsGenericParameter(ParameterType) Then
                    If Symbols.AreGenericMethodDefsEqual(ParameterType.DeclaringMethod, TargetProcedure) Then
                        Dim genericParameterPosition As Integer = ParameterType.GenericParameterPosition
                        If (Not TypeInferenceArguments(genericParameterPosition) Is Nothing) Then
                            If (Not TypeInferenceArguments(genericParameterPosition) Is ArgumentType) Then
                                Return False
                            End If
                        Else
                            TypeInferenceArguments(genericParameterPosition) = ArgumentType
                        End If
                    End If
                Else
                    If Symbols.IsInstantiatedGeneric(ParameterType) Then
                        Dim type As Type = Nothing
                        If (Symbols.IsInstantiatedGeneric(ArgumentType) AndAlso (ArgumentType.GetGenericTypeDefinition Is ParameterType.GetGenericTypeDefinition)) Then
                            type = ArgumentType
                        End If
                        If ((type Is Nothing) AndAlso DigThroughToBasesAndImplements) Then
                            Dim type2 As Type
                            For Each type2 In ArgumentType.GetInterfaces
                                If (Symbols.IsInstantiatedGeneric(type2) AndAlso (type2.GetGenericTypeDefinition Is ParameterType.GetGenericTypeDefinition)) Then
                                    If (Not type Is Nothing) Then
                                        Return False
                                    End If
                                    type = type2
                                End If
                            Next
                        End If
                        If (Not type Is Nothing) Then
                            Dim typeArguments As Type() = Symbols.GetTypeArguments(ParameterType)
                            Dim typeArray3 As Type() = Symbols.GetTypeArguments(type)
                            Dim num3 As Integer = (typeArray3.Length - 1)
                            Dim i As Integer = 0
                            Do While (i <= num3)
                                If Not OverloadResolution.InferTypeArgumentsFromArgument(typeArray3(i), typeArguments(i), TypeInferenceArguments, TargetProcedure, False) Then
                                    Return False
                                End If
                                i += 1
                            Loop
                            Return True
                        End If
                        Return False
                    End If
                    If Symbols.IsArrayType(ParameterType) Then
                        Return ((Symbols.IsArrayType(ArgumentType) AndAlso (ParameterType.GetArrayRank = ArgumentType.GetArrayRank)) AndAlso OverloadResolution.InferTypeArgumentsFromArgument(Symbols.GetElementType(ArgumentType), Symbols.GetElementType(ParameterType), TypeInferenceArguments, TargetProcedure, DigThroughToBasesAndImplements))
                    End If
                End If
            End If
            Return True
        End Function

        Private Shared Sub InsertIfMethodAvailable(NewCandidate As MemberInfo, NewCandidateSignature As ParameterInfo(), NewCandidateParamArrayIndex As Integer, ExpandNewCandidateParamArray As Boolean, Arguments As Object(), ArgumentCount As Integer, ArgumentNames As String(), TypeArguments As Type(), CollectOnlyOperators As Boolean, Candidates As List(Of Method), BaseReference As Container)
            Dim candidate As Method = Nothing
            If Not CollectOnlyOperators Then
                Dim method As MethodBase = TryCast(NewCandidate, MethodBase)
                Dim flag As Boolean = False
                If ((NewCandidate.MemberType = MemberTypes.Method) AndAlso Symbols.IsRawGeneric(method)) Then
                    candidate = New Method(method, NewCandidateSignature, NewCandidateParamArrayIndex, ExpandNewCandidateParamArray)
                    OverloadResolution.RejectUncallableProcedure(candidate, Arguments, ArgumentNames, TypeArguments)
                    NewCandidate = candidate.AsMethod
                    NewCandidateSignature = candidate.Parameters
                End If
                If (((Not NewCandidate Is Nothing) AndAlso (NewCandidate.MemberType = MemberTypes.Method)) AndAlso Symbols.IsRawGeneric(TryCast(NewCandidate, MethodBase))) Then
                    flag = True
                End If
                Dim num As Integer = (Candidates.Count - 1)
                Dim i As Integer = 0
                Do While (i <= num)
                    Dim method2 As Method = Candidates.Item(i)
                    If (Not method2 Is Nothing) Then
                        Dim base3 As MethodBase
                        Dim parameters As ParameterInfo() = method2.Parameters
                        If method2.IsMethod Then
                            base3 = method2.AsMethod
                        Else
                            base3 = Nothing
                        End If
                        If (Not NewCandidate Is method2) Then
                            Dim index As Integer = 0
                            Dim num4 As Integer = 0
                            Dim num5 As Integer = ArgumentCount
                            Dim j As Integer = 1
                            Do While (j <= num5)
                                Dim bothLose As Boolean = False
                                Dim leftWins As Boolean = False
                                Dim rightWins As Boolean = False
                                OverloadResolution.CompareParameterSpecificity(Nothing, NewCandidateSignature(index), method, ExpandNewCandidateParamArray, parameters(num4), base3, method2.ParamArrayExpanded, leftWins, rightWins, bothLose)
                                If Not ((bothLose Or leftWins) Or rightWins) Then
                                    If ((index <> NewCandidateParamArrayIndex) OrElse Not ExpandNewCandidateParamArray) Then
                                        index += 1
                                    End If
                                    If ((num4 <> method2.ParamArrayIndex) OrElse Not method2.ParamArrayExpanded) Then
                                        num4 += 1
                                    End If
                                End If
                                j += 1
                            Loop
                            If Not OverloadResolution.IsExactSignatureMatch(NewCandidateSignature, Symbols.GetTypeParameters(NewCandidate).Length, method2.Parameters, method2.TypeParameters.Length) Then
                                If (Not flag AndAlso ((base3 Is Nothing) OrElse Not Symbols.IsRawGeneric(base3))) Then
                                    If (ExpandNewCandidateParamArray OrElse Not method2.ParamArrayExpanded) Then
                                        If (Not ExpandNewCandidateParamArray OrElse method2.ParamArrayExpanded) Then
                                            If (ExpandNewCandidateParamArray OrElse method2.ParamArrayExpanded) Then
                                                If (index <= num4) Then
                                                    If (num4 > index) Then
                                                        Return
                                                    End If
                                                Else
                                                    Candidates.Item(i) = Nothing
                                                End If
                                            End If
                                            Continue Do
                                        End If
                                        Return
                                    End If
                                    Candidates.Item(i) = Nothing
                                End If
                            Else
                                If ((NewCandidate.DeclaringType Is method2.DeclaringType) OrElse (((Not BaseReference Is Nothing) AndAlso BaseReference.IsWindowsRuntimeObject) AndAlso (Symbols.IsCollectionInterface(NewCandidate.DeclaringType) AndAlso Symbols.IsCollectionInterface(method2.DeclaringType)))) Then
                                    Exit Do
                                End If
                                If ((flag OrElse (base3 Is Nothing)) OrElse Not Symbols.IsRawGeneric(base3)) Then
                                    Return
                                End If
                            End If
                        End If
                    End If
                    i += 1
                Loop
            End If
            If (Not candidate Is Nothing) Then
                Candidates.Add(candidate)
            ElseIf (NewCandidate.MemberType = MemberTypes.Property) Then
                Candidates.Add(New Method(DirectCast(NewCandidate, PropertyInfo), NewCandidateSignature, NewCandidateParamArrayIndex, ExpandNewCandidateParamArray))
            Else
                Candidates.Add(New Method(DirectCast(NewCandidate, MethodBase), NewCandidateSignature, NewCandidateParamArrayIndex, ExpandNewCandidateParamArray))
            End If
        End Sub

        Private Shared Function InstantiateGenericMethod(TargetProcedure As Method, TypeArguments As Type(), Errors As List(Of String)) As Boolean
            Dim flag2 As Boolean = (Errors IsNot Nothing)
            Dim num As Integer = (TypeArguments.Length - 1)
            Dim i As Integer = 0
            Do While (i <= num)
                If (TypeArguments(i) Is Nothing) Then
                    If Not flag2 Then
                        Return False
                    End If
                    OverloadResolution.ReportError(Errors, "UnboundTypeParam1", TargetProcedure.TypeParameters(i).Name)
                End If
                i += 1
            Loop
            If (((Errors Is Nothing) OrElse (Errors.Count = 0)) AndAlso Not TargetProcedure.BindGenericArguments) Then
                If Not flag2 Then
                    Return False
                End If
                OverloadResolution.ReportError(Errors, "FailedTypeArgumentBinding")
            End If
            If ((Not Errors Is Nothing) AndAlso (Errors.Count > 0)) Then
                Return False
            End If
            Return True
        End Function

        Private Shared Function IsExactSignatureMatch(LeftSignature As ParameterInfo(), LeftTypeParameterCount As Integer, RightSignature As ParameterInfo(), RightTypeParameterCount As Integer) As Boolean
            Dim infoArray As ParameterInfo()
            Dim infoArray2 As ParameterInfo()
            If (LeftSignature.Length >= RightSignature.Length) Then
                infoArray = LeftSignature
                infoArray2 = RightSignature
            Else
                infoArray = RightSignature
                infoArray2 = LeftSignature
            End If
            Dim num As Integer = (infoArray.Length - 1)
            Dim i As Integer = infoArray2.Length
            Do While (i <= num)
                If Not infoArray(i).IsOptional Then
                    Return False
                End If
                i += 1
            Loop
            Dim num3 As Integer = (infoArray2.Length - 1)
            Dim j As Integer = 0
            Do While (j <= num3)
                Dim parameterType As Type = infoArray2(j).ParameterType
                Dim elementType As Type = infoArray(j).ParameterType
                If parameterType.IsByRef Then
                    parameterType = parameterType.GetElementType
                End If
                If elementType.IsByRef Then
                    elementType = elementType.GetElementType
                End If
                If ((Not parameterType Is elementType) AndAlso (Not infoArray2(j).IsOptional OrElse Not infoArray(j).IsOptional)) Then
                    Return False
                End If
                j += 1
            Loop
            Return True
        End Function

        Friend Shared Function LeastGenericProcedure(Left As Method, Right As Method) As Method
            If ((Not Left.IsGeneric AndAlso Not Right.IsGeneric) AndAlso (Not Symbols.IsGeneric(Left.DeclaringType) AndAlso Not Symbols.IsGeneric(Right.DeclaringType))) Then
                Return Nothing
            End If
            Dim signatureMismatch As Boolean = False
            Dim method2 As Method = OverloadResolution.LeastGenericProcedure(Left, Right, ComparisonType.GenericSpecificityBasedOnMethodGenericParams, signatureMismatch)
            If ((method2 Is Nothing) AndAlso Not signatureMismatch) Then
                method2 = OverloadResolution.LeastGenericProcedure(Left, Right, ComparisonType.GenericSpecificityBasedOnTypeGenericParams, signatureMismatch)
            End If
            Return method2
        End Function

        Private Shared Function LeastGenericProcedure(Left As Method, Right As Method, CompareGenericity As ComparisonType, ByRef SignatureMismatch As Boolean) As Method
            Dim leftIsLessGeneric As Boolean = False
            Dim rightIsLessGeneric As Boolean = False
            SignatureMismatch = False
            If (Left.IsMethod AndAlso Right.IsMethod) Then
                Dim index As Integer = 0
                Dim length As Integer = Left.Parameters.Length
                Dim num3 As Integer = Right.Parameters.Length
                Do While ((index < length) AndAlso (index < num3))
                    Select Case CompareGenericity
                        Case ComparisonType.GenericSpecificityBasedOnMethodGenericParams
                            OverloadResolution.CompareGenericityBasedOnMethodGenericParams(Left.Parameters(index), Left.RawParameters(index), Left, Left.ParamArrayExpanded, Right.Parameters(index), Right.RawParameters(index), Right, False, leftIsLessGeneric, rightIsLessGeneric, SignatureMismatch)
                            Exit Select
                        Case ComparisonType.GenericSpecificityBasedOnTypeGenericParams
                            OverloadResolution.CompareGenericityBasedOnTypeGenericParams(Left.Parameters(index), Left.RawParameters(index), Left, Left.ParamArrayExpanded, Right.Parameters(index), Right.RawParameters(index), Right, False, leftIsLessGeneric, rightIsLessGeneric, SignatureMismatch)
                            Exit Select
                    End Select
                    If (SignatureMismatch OrElse (leftIsLessGeneric AndAlso rightIsLessGeneric)) Then
                        Return Nothing
                    End If
                    index += 1
                Loop
                If ((index < length) OrElse (index < num3)) Then
                    Return Nothing
                End If
                If leftIsLessGeneric Then
                    Return Left
                End If
                If rightIsLessGeneric Then
                    Return Right
                End If
            End If
            Return Nothing
        End Function

        Friend Shared Sub MatchArguments(TargetProcedure As Method, Arguments As Object(), MatchedArguments As Object())
            Dim parameters As ParameterInfo() = TargetProcedure.Parameters
            Dim namedArgumentMapping As Integer() = TargetProcedure.NamedArgumentMapping
            Dim index As Integer = 0
            If (Not namedArgumentMapping Is Nothing) Then
                index = namedArgumentMapping.Length
            End If
            Dim num2 As Integer = 0
            Do While (index < Arguments.Length)
                If (num2 = TargetProcedure.ParamArrayIndex) Then
                    Exit Do
                End If
                MatchedArguments(num2) = OverloadResolution.PassToParameter(Arguments(index), parameters(num2), parameters(num2).ParameterType)
                index += 1
                num2 += 1
            Loop
            If TargetProcedure.HasParamArray Then
                If TargetProcedure.ParamArrayExpanded Then
                    Dim length As Integer = (Arguments.Length - index)
                    Dim parameter As ParameterInfo = parameters(num2)
                    Dim elementType As Type = parameter.ParameterType.GetElementType
                    Dim array As Array = Array.CreateInstance(elementType, length)
                    Dim i As Integer = 0
                    Do While (index < Arguments.Length)
                        array.SetValue(OverloadResolution.PassToParameter(Arguments(index), parameter, elementType), i)
                        index += 1
                        i += 1
                    Loop
                    MatchedArguments(num2) = array
                Else
                    MatchedArguments(num2) = OverloadResolution.PassToParameter(Arguments(index), parameters(num2), parameters(num2).ParameterType)
                End If
                num2 += 1
            End If
            Dim flagArray As Boolean() = Nothing
            If ((Not namedArgumentMapping Is Nothing) OrElse (num2 < parameters.Length)) Then
                flagArray = OverloadResolution.CreateMatchTable(parameters.Length, (num2 - 1))
            End If
            If (Not namedArgumentMapping Is Nothing) Then
                index
                For index = 0 To namedArgumentMapping.Length - 1
                    num2 = namedArgumentMapping(index)
                    MatchedArguments(num2) = OverloadResolution.PassToParameter(Arguments(index), parameters(num2), parameters(num2).ParameterType)
                    flagArray(num2) = True
                Next index
            End If
            If (Not flagArray Is Nothing) Then
                Dim num5 As Integer = (flagArray.Length - 1)
                Dim j As Integer = 0
                Do While (j <= num5)
                    If Not flagArray(j) Then
                        MatchedArguments(j) = OverloadResolution.PassToParameter(Missing.Value, parameters(j), parameters(j).ParameterType)
                    End If
                    j += 1
                Loop
            End If
        End Sub

        Private Shared Function MoreSpecificProcedure(Left As Method, Right As Method, Arguments As Object(), ArgumentNames As String(), CompareGenericity As ComparisonType, ByRef Optional BothLose As Boolean = False, Optional ContinueWhenBothLose As Boolean = False) As Method
            Dim base2 As MethodBase
            Dim base3 As MethodBase
            Dim num3 As Integer
            BothLose = False
            Dim leftWins As Boolean = False
            Dim rightWins As Boolean = False
            If Left.IsMethod Then
                base2 = Left.AsMethod
            Else
                base2 = Nothing
            End If
            If Right.IsMethod Then
                base3 = Right.AsMethod
            Else
                base3 = Nothing
            End If
            Dim index As Integer = 0
            Dim num2 As Integer = 0
            num3
            For num3 = ArgumentNames.Length To Arguments.Length - 1
                Dim argumentType As Type = OverloadResolution.GetArgumentType(Arguments(num3))
                Select Case CompareGenericity
                    Case ComparisonType.ParameterSpecificty
                        OverloadResolution.CompareParameterSpecificity(argumentType, Left.Parameters(index), base2, Left.ParamArrayExpanded, Right.Parameters(num2), base3, Right.ParamArrayExpanded, leftWins, rightWins, BothLose)
                        Exit Select
                    Case ComparisonType.GenericSpecificityBasedOnMethodGenericParams
                        OverloadResolution.CompareGenericityBasedOnMethodGenericParams(Left.Parameters(index), Left.RawParameters(index), Left, Left.ParamArrayExpanded, Right.Parameters(num2), Right.RawParameters(num2), Right, Right.ParamArrayExpanded, leftWins, rightWins, BothLose)
                        Exit Select
                    Case ComparisonType.GenericSpecificityBasedOnTypeGenericParams
                        OverloadResolution.CompareGenericityBasedOnTypeGenericParams(Left.Parameters(index), Left.RawParametersFromType(index), Left, Left.ParamArrayExpanded, Right.Parameters(num2), Right.RawParametersFromType(num2), Right, Right.ParamArrayExpanded, leftWins, rightWins, BothLose)
                        Exit Select
                End Select
                If ((BothLose AndAlso Not ContinueWhenBothLose) OrElse (leftWins AndAlso rightWins)) Then
                    Return Nothing
                End If
                If (index <> Left.ParamArrayIndex) Then
                    index += 1
                End If
                If (num2 <> Right.ParamArrayIndex) Then
                    num2 += 1
                End If
            Next num3
            num3
            For num3 = 0 To ArgumentNames.Length - 1
                Dim flag3 As Boolean = OverloadResolution.FindParameterByName(Right.Parameters, ArgumentNames(num3), num2)
                If (Not OverloadResolution.FindParameterByName(Left.Parameters, ArgumentNames(num3), index) OrElse Not flag3) Then
                    Throw New InternalErrorException
                End If
                Dim type3 As Type = OverloadResolution.GetArgumentType(Arguments(num3))
                Select Case CompareGenericity
                    Case ComparisonType.ParameterSpecificty
                        OverloadResolution.CompareParameterSpecificity(type3, Left.Parameters(index), base2, True, Right.Parameters(num2), base3, True, leftWins, rightWins, BothLose)
                        Exit Select
                    Case ComparisonType.GenericSpecificityBasedOnMethodGenericParams
                        OverloadResolution.CompareGenericityBasedOnMethodGenericParams(Left.Parameters(index), Left.RawParameters(index), Left, True, Right.Parameters(num2), Right.RawParameters(num2), Right, True, leftWins, rightWins, BothLose)
                        Exit Select
                    Case ComparisonType.GenericSpecificityBasedOnTypeGenericParams
                        OverloadResolution.CompareGenericityBasedOnTypeGenericParams(Left.Parameters(index), Left.RawParameters(index), Left, True, Right.Parameters(num2), Right.RawParameters(num2), Right, True, leftWins, rightWins, BothLose)
                        Exit Select
                End Select
                If ((BothLose AndAlso Not ContinueWhenBothLose) OrElse (leftWins AndAlso rightWins)) Then
                    Return Nothing
                End If
            Next num3
            If leftWins Then
                Return Left
            End If
            If rightWins Then
                Return Right
            End If
            Return Nothing
        End Function

        Private Shared Function MostSpecificProcedure(Candidates As List(Of Method), ByRef CandidateCount As Integer, Arguments As Object(), ArgumentNames As String()) As Method
            Dim method2 As Method
            For Each method2 In Candidates
                If (Not method2.NotCallable AndAlso Not method2.RequiresNarrowingConversion) Then
                    Dim flag As Boolean = True
                    Dim method3 As Method
                    For Each method3 In Candidates
                        If ((Not method3.NotCallable AndAlso Not method3.RequiresNarrowingConversion) AndAlso ((Not method3 Is method2) OrElse (method3.ParamArrayExpanded <> method2.ParamArrayExpanded))) Then
                            Dim bothLose As Boolean = False
                            Dim method4 As Method = OverloadResolution.MoreSpecificProcedure(method2, method3, Arguments, ArgumentNames, ComparisonType.ParameterSpecificty, bothLose, True)
                            If (((method4 Is Nothing) AndAlso Not bothLose) AndAlso (method2.UsedDefaultForAnOptionalParameter <> method3.UsedDefaultForAnOptionalParameter)) Then
                                If method3.UsedDefaultForAnOptionalParameter Then
                                    method4 = method2
                                Else
                                    method4 = method3
                                End If
                            End If
                            If (method4 Is method2) Then
                                If Not method3.LessSpecific Then
                                    method3.LessSpecific = True
                                    CandidateCount -= 1
                                End If
                            Else
                                flag = False
                                If ((method4 Is method3) AndAlso Not method2.LessSpecific) Then
                                    method2.LessSpecific = True
                                    CandidateCount -= 1
                                End If
                            End If
                        End If
                    Next
                    If flag Then
                        Return method2
                    End If
                End If
            Next
            Return Nothing
        End Function

        Friend Shared Function PassToParameter(Argument As Object, Parameter As ParameterInfo, ParameterType As Type) As Object
            Dim isByRef As Boolean = ParameterType.IsByRef
            If isByRef Then
                ParameterType = ParameterType.GetElementType
            End If
            If TypeOf Argument Is TypedNothing Then
                Argument = Nothing
            End If
            If ((Argument Is Missing.Value) AndAlso Parameter.IsOptional) Then
                Argument = Parameter.DefaultValue
            End If
            If isByRef Then
                Dim argumentTypeInContextOfParameterType As Type = OverloadResolution.GetArgumentTypeInContextOfParameterType(Argument, ParameterType)
                If ((Not argumentTypeInContextOfParameterType Is Nothing) AndAlso Symbols.IsValueType(argumentTypeInContextOfParameterType)) Then
                    Argument = Conversions.ForceValueCopy(Argument, argumentTypeInContextOfParameterType)
                End If
            End If
            Return Conversions.ChangeType(Argument, ParameterType)
        End Function

        Private Shared Sub RejectUncallableProcedure(Candidate As Method, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type())
            If Not OverloadResolution.CanMatchArguments(Candidate, Arguments, ArgumentNames, TypeArguments, False, Nothing) Then
                Candidate.NotCallable = True
            End If
            Candidate.ArgumentMatchingDone = True
        End Sub

        Private Shared Function RejectUncallableProcedures(Candidates As List(Of Method), Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), ByRef CandidateCount As Integer, ByRef SomeCandidatesAreGeneric As Boolean) As Method
            Dim method As Method = Nothing
            Dim num As Integer = (Candidates.Count - 1)
            Dim i As Integer = 0
            Do While (i <= num)
                Dim candidate As Method = Candidates.Item(i)
                If Not candidate.ArgumentMatchingDone Then
                    OverloadResolution.RejectUncallableProcedure(candidate, Arguments, ArgumentNames, TypeArguments)
                End If
                If candidate.NotCallable Then
                    CandidateCount -= 1
                Else
                    method = candidate
                    If (candidate.IsGeneric OrElse Symbols.IsGeneric(candidate.DeclaringType)) Then
                        SomeCandidatesAreGeneric = True
                    End If
                End If
                i += 1
            Loop
            Return method
        End Function

        Private Shared Function RemoveRedundantGenericProcedures(Candidates As List(Of Method), ByRef CandidateCount As Integer, Arguments As Object(), ArgumentNames As String()) As Method
            Dim num As Integer = (Candidates.Count - 1)
            Dim i As Integer = 0
            Do While (i <= num)
                Dim left As Method = Candidates.Item(i)
                If Not left.NotCallable Then
                    Dim num3 As Integer = (Candidates.Count - 1)
                    Dim j As Integer = (i + 1)
                    Do While (j <= num3)
                        Dim right As Method = Candidates.Item(j)
                        If (Not right.NotCallable AndAlso (left.RequiresNarrowingConversion = right.RequiresNarrowingConversion)) Then
                            Dim method4 As Method = Nothing
                            Dim bothLose As Boolean = False
                            If (left.IsGeneric OrElse right.IsGeneric) Then
                                method4 = OverloadResolution.MoreSpecificProcedure(left, right, Arguments, ArgumentNames, ComparisonType.GenericSpecificityBasedOnMethodGenericParams, bothLose, False)
                                If (Not method4 Is Nothing) Then
                                    CandidateCount -= 1
                                    If (CandidateCount Is 1) Then
                                        Return method4
                                    End If
                                    If (method4 Is left) Then
                                        right.NotCallable = True
                                    Else
                                        left.NotCallable = True
                                        Exit Do
                                    End If
                                End If
                            End If
                            If ((Not bothLose AndAlso (method4 Is Nothing)) AndAlso (Symbols.IsGeneric(left.DeclaringType) OrElse Symbols.IsGeneric(right.DeclaringType))) Then
                                method4 = OverloadResolution.MoreSpecificProcedure(left, right, Arguments, ArgumentNames, ComparisonType.GenericSpecificityBasedOnTypeGenericParams, bothLose, False)
                                If (Not method4 Is Nothing) Then
                                    CandidateCount -= 1
                                    If (CandidateCount Is 1) Then
                                        Return method4
                                    End If
                                    If (method4 Is left) Then
                                        right.NotCallable = True
                                    Else
                                        left.NotCallable = True
                                        Exit Do
                                    End If
                                End If
                            End If
                        End If
                        j += 1
                    Loop
                End If
                i += 1
            Loop
            Return Nothing
        End Function

        Friend Shared Sub ReorderArgumentArray(TargetProcedure As Method, ParameterResults As Object(), Arguments As Object(), CopyBack As Boolean(), LookupFlags As BindingFlags)
            If (Not CopyBack Is Nothing) Then
                Dim num3 As Integer = (CopyBack.Length - 1)
                Dim i As Integer = 0
                Do While (i <= num3)
                    CopyBack(i) = False
                    i += 1
                Loop
                If (Not Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty) AndAlso TargetProcedure.HasByRefParameter) Then
                    Dim parameters As ParameterInfo() = TargetProcedure.Parameters
                    Dim namedArgumentMapping As Integer() = TargetProcedure.NamedArgumentMapping
                    Dim index As Integer = 0
                    If (Not namedArgumentMapping Is Nothing) Then
                        index = namedArgumentMapping.Length
                    End If
                    Dim num2 As Integer = 0
                    Do While (index < Arguments.Length)
                        If (num2 = TargetProcedure.ParamArrayIndex) Then
                            Exit Do
                        End If
                        If parameters(num2).ParameterType.IsByRef Then
                            Arguments(index) = ParameterResults(num2)
                            CopyBack(index) = True
                        End If
                        index += 1
                        num2 += 1
                    Loop
                    If (Not namedArgumentMapping Is Nothing) Then
                        index
                        For index = 0 To namedArgumentMapping.Length - 1
                            num2 = namedArgumentMapping(index)
                            If parameters(num2).ParameterType.IsByRef Then
                                Arguments(index) = ParameterResults(num2)
                                CopyBack(index) = True
                            End If
                        Next index
                    End If
                End If
            End If
        End Sub

        Private Shared Sub ReportError(Errors As List(Of String), ResourceID As String)
            Errors.Add(Utils.GetResourceString(ResourceID))
        End Sub

        Private Shared Sub ReportError(Errors As List(Of String), ResourceID As String, Substitution1 As String)
            Dim args As String() = New String() {Substitution1}
            Errors.Add(Utils.GetResourceString(ResourceID, args))
        End Sub

        Private Shared Sub ReportError(Errors As List(Of String), ResourceID As String, Substitution1 As String, Substitution2 As Method)
            Dim args As String() = New String() {Substitution1, Substitution2.ToString}
            Errors.Add(Utils.GetResourceString(ResourceID, args))
        End Sub

        Private Shared Sub ReportError(Errors As List(Of String), ResourceID As String, Substitution1 As String, Substitution2 As Type, Substitution3 As Type)
            Dim args As String() = New String() {Substitution1, Utils.VBFriendlyName(Substitution2), Utils.VBFriendlyName(Substitution3)}
            Errors.Add(Utils.GetResourceString(ResourceID, args))
        End Sub

        Private Shared Function ReportNarrowingProcedures(OverloadedProcedureName As String, Candidates As List(Of Method), Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), Failure As ResolutionFailure) As Exception
            Return OverloadResolution.ReportOverloadResolutionFailure(OverloadedProcedureName, Candidates, Arguments, ArgumentNames, TypeArguments, "NoNonNarrowingOverloadCandidates2", Failure, New ArgumentDetector(AddressOf OverloadResolution.DetectArgumentNarrowing), New CandidateProperty(AddressOf OverloadResolution.CandidateIsNarrowing))
        End Function

        Private Shared Function ReportOverloadResolutionFailure(OverloadedProcedureName As String, Candidates As List(Of Method), Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), ErrorID As String, Failure As ResolutionFailure, Detector As ArgumentDetector, CandidateFilter As CandidateProperty) As Exception
            Dim builder As New StringBuilder
            Dim errors As New List(Of String)
            Dim num As Integer = 0
            Dim num2 As Integer = (Candidates.Count - 1)
            Dim i As Integer = 0
            Do While (i <= num2)
                Dim candidate As Method = Candidates.Item(i)
                If CandidateFilter.Invoke(candidate) Then
                    If candidate.HasParamArray Then
                        Dim j As Integer
                        For j = (i + 1) To Candidates.Count - 1
                            If (CandidateFilter.Invoke(Candidates.Item(j)) AndAlso (Candidates.Item(j) Is candidate)) Then
                                Continue For
                            End If
                        Next j
                    End If
                    num += 1
                    errors.Clear()
                    Detector.Invoke(candidate, Arguments, ArgumentNames, TypeArguments, errors)
                    builder.Append(ChrW(13) & ChrW(10) & "    '")
                    builder.Append(candidate.ToString)
                    builder.Append("':")
                    Dim str2 As String
                    For Each str2 In errors
                        builder.Append(ChrW(13) & ChrW(10) & "        ")
                        builder.Append(str2)
                    Next
                End If
                i += 1
            Loop
            Dim args As String() = New String() {OverloadedProcedureName, builder.ToString}
            Dim resourceString As String = Utils.GetResourceString(ErrorID, args)
            If (num = 1) Then
                Return New InvalidCastException(resourceString)
            End If
            Return New AmbiguousMatchException(resourceString)
        End Function

        Private Shared Function ReportUncallableProcedures(OverloadedProcedureName As String, Candidates As List(Of Method), Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), Failure As ResolutionFailure) As Exception
            Return OverloadResolution.ReportOverloadResolutionFailure(OverloadedProcedureName, Candidates, Arguments, ArgumentNames, TypeArguments, "NoCallableOverloadCandidates2", Failure, New ArgumentDetector(AddressOf OverloadResolution.DetectArgumentErrors), New CandidateProperty(AddressOf OverloadResolution.CandidateIsNotCallable))
        End Function

        Private Shared Function ReportUnspecificProcedures(OverloadedProcedureName As String, Candidates As List(Of Method), Failure As ResolutionFailure) As Exception
            Return OverloadResolution.ReportOverloadResolutionFailure(OverloadedProcedureName, Candidates, Nothing, Nothing, Nothing, "NoMostSpecificOverload2", Failure, New ArgumentDetector(AddressOf OverloadResolution.DetectUnspecificity), New CandidateProperty(AddressOf OverloadResolution.CandidateIsUnspecific))
        End Function

        Friend Shared Function ResolveOverloadedCall(MethodName As String, Candidates As List(Of Method), Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), LookupFlags As BindingFlags, ReportErrors As Boolean, ByRef Failure As ResolutionFailure) As Method
            Failure = ResolutionFailure.None
            Dim count As Integer = Candidates.Count
            Dim someCandidatesAreGeneric As Boolean = False
            Dim method2 As Method = OverloadResolution.RejectUncallableProcedures(Candidates, Arguments, ArgumentNames, TypeArguments, count, someCandidatesAreGeneric)
            Select Case count
                Case 1
                    Return method2
                Case 0
                    Failure = ResolutionFailure.InvalidArgument
                    If ReportErrors Then
                        Throw OverloadResolution.ReportUncallableProcedures(MethodName, Candidates, Arguments, ArgumentNames, TypeArguments, Failure)
                    End If
                    Return Nothing
            End Select
            If someCandidatesAreGeneric Then
                method2 = OverloadResolution.RemoveRedundantGenericProcedures(Candidates, count, Arguments, ArgumentNames)
                If (count = 1) Then
                    Return method2
                End If
            End If
            Dim num2 As Integer = 0
            Dim method3 As Method = Nothing
            Dim method4 As Method
            For Each method4 In Candidates
                If Not method4.NotCallable Then
                    If method4.RequiresNarrowingConversion Then
                        count -= 1
                        If method4.AllNarrowingIsFromObject Then
                            num2 += 1
                            method3 = method4
                        End If
                    Else
                        method2 = method4
                    End If
                End If
            Next
            Select Case count
                Case 1
                    Return method2
                Case 0
                    If (num2 = 1) Then
                        Return method3
                    End If
                    Failure = ResolutionFailure.AmbiguousMatch
                    If ReportErrors Then
                        Throw OverloadResolution.ReportNarrowingProcedures(MethodName, Candidates, Arguments, ArgumentNames, TypeArguments, Failure)
                    End If
                    Return Nothing
            End Select
            method2 = OverloadResolution.MostSpecificProcedure(Candidates, count, Arguments, ArgumentNames)
            If (Not method2 Is Nothing) Then
                Return method2
            End If
            Failure = ResolutionFailure.AmbiguousMatch
            If ReportErrors Then
                Throw OverloadResolution.ReportUnspecificProcedures(MethodName, Candidates, Failure)
            End If
            Return Nothing
        End Function

        Friend Shared Function ResolveOverloadedCall(MethodName As String, Members As MemberInfo(), Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), LookupFlags As BindingFlags, ReportErrors As Boolean, ByRef Failure As ResolutionFailure, BaseReference As Container) As Method
            Dim rejectedForArgumentCount As Integer = 0
            Dim rejectedForTypeArgumentCount As Integer = 0
            Dim candidates As List(Of Method) = OverloadResolution.CollectOverloadCandidates(Members, Arguments, Arguments.Length, ArgumentNames, TypeArguments, False, Nothing, rejectedForArgumentCount, rejectedForTypeArgumentCount, BaseReference)
            If ((candidates.Count = 1) AndAlso Not candidates.Item(0).NotCallable) Then
                Return candidates.Item(0)
            End If
            If (candidates.Count = 0) Then
                Failure = ResolutionFailure.MissingMember
                If Not ReportErrors Then
                    Return Nothing
                End If
                Dim resourceKey As String = "NoViableOverloadCandidates1"
                If (rejectedForArgumentCount > 0) Then
                    resourceKey = "NoArgumentCountOverloadCandidates1"
                ElseIf (rejectedForTypeArgumentCount > 0) Then
                    resourceKey = "NoTypeArgumentCountOverloadCandidates1"
                End If
                Dim args As String() = New String() {MethodName}
                Throw New MissingMemberException(Utils.GetResourceString(resourceKey, args))
            End If
            Return OverloadResolution.ResolveOverloadedCall(MethodName, candidates, Arguments, ArgumentNames, TypeArguments, LookupFlags, ReportErrors, Failure)
        End Function


        ' Nested Types
        Private Delegate Function ArgumentDetector(TargetProcedure As Method, Arguments As Object(), ArgumentNames As String(), TypeArguments As Type(), Errors As List(Of String)) As Boolean

        Private Delegate Function CandidateProperty(Candidate As Method) As Boolean

        Private Enum ComparisonType
            ' Fields
            GenericSpecificityBasedOnMethodGenericParams = 1
            GenericSpecificityBasedOnTypeGenericParams = 2
            ParameterSpecificty = 0
        End Enum

        Friend Enum ResolutionFailure
            ' Fields
            AmbiguousMatch = 3
            InvalidArgument = 2
            InvalidTarget = 4
            MissingMember = 1
            None = 0
        End Enum
    End Class
End Namespace

