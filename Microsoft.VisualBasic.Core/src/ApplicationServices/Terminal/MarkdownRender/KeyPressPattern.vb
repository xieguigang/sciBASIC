Imports System.Collections.Immutable

Namespace ApplicationServices.Terminal


    'INSTANT VB WARNING: VB has no equivalent to the C# readonly struct:
    'ORIGINAL LINE: public readonly struct KeyPressPattern
    Public Structure KeyPressPattern
        Private ReadOnly type As KeyPressPatternType

        Public ReadOnly Modifiers As ConsoleModifiers
        Public ReadOnly Key As ConsoleKey
        Public ReadOnly Character As Char

        'INSTANT VB NOTE: The variable key was renamed since Visual Basic does not handle local variables named the same as class members well:
        Public Sub New(ByVal key_Conflict As ConsoleKey)
            Me.New(modifiers:= Default, key_Conflict)
        End Sub

        'INSTANT VB NOTE: The variable modifiers was renamed since Visual Basic does not handle local variables named the same as class members well:
        'INSTANT VB NOTE: The variable key was renamed since Visual Basic does not handle local variables named the same as class members well:
        Public Sub New(ByVal modifiers_Conflict As ConsoleModifiers, ByVal key_Conflict As ConsoleKey)
            type = KeyPressPatternType.ConsoleKey
            Me.Modifiers = modifiers_Conflict
            Me.Key = key_Conflict
            Character = MapToCharacter(key_Conflict)
        End Sub

        'INSTANT VB NOTE: The variable character was renamed since Visual Basic does not handle local variables named the same as class members well:
        Public Sub New(ByVal character_Conflict As Char)
            type = KeyPressPatternType.Character
            Modifiers = Nothing
            Key = Nothing
            Me.Character = character_Conflict
        End Sub

        Public Function Matches(ByVal keyInfo As ConsoleKeyInfo) As Boolean
            Dim tempVar As Boolean = TypeOf keyInfo.Modifiers Is CType(Nothing, ConsoleModifiers)
		Dim [or] As CType(Nothing, ConsoleModifiers) = If(tempVar, CType(keyInfo.Modifiers, CType(Nothing, ConsoleModifiers)), Nothing)
		Return If(type = KeyPressPatternType.ConsoleKey, keyInfo.Modifiers = Modifiers AndAlso keyInfo.Key = Key, (tempVar ConsoleModifiers.Shift) AndAlso keyInfo.KeyChar = Character) 'Shift is ok, it only determines casing of letter
	End Function

        'INSTANT VB NOTE: The variable key was renamed since Visual Basic does not handle local variables named the same as class members well:
        Private Shared Function MapToCharacter(ByVal key_Conflict As ConsoleKey) As Char
            Dim keyString = key_Conflict.ToString()
            'INSTANT VB TODO TASK: 'switch expressions' are not converted by Instant VB:
            '		return keyString switch
            '		{
            '			{ Length: 1 } => keyString[0],
            '			"Enter" => '\n',
            '			"Spacebar" => ' ',
            '			"Escape" => '\x1b',
            '			"Tab" => '\t',
            '			_ => '\0'
            '		};
        End Function

        Private Function GetDebuggerDisplay() As String
            If type = KeyPressPatternType.ConsoleKey Then
                'INSTANT VB WARNING: VB does not allow comparing non-nullable value types with 'null' - they are never equal to 'null':
                'ORIGINAL LINE: return Modifiers == default ? string.Format("{0}", Key, TangibleStringInterpolationMarker) : string.Format("{0}+{1}", Modifiers, Key, TangibleStringInterpolationMarker);
                Return If(False, $"{Key}", $"{Modifiers}+{Key}")
            Else
                Return $"{Character}"
            End If
        End Function
    End Structure

    Public Enum KeyPressPatternType
        ConsoleKey
        Character
    End Enum

    'INSTANT VB WARNING: VB has no equivalent to the C# readonly struct:
    'ORIGINAL LINE: public readonly struct KeyPressPatterns
    Public Structure KeyPressPatterns
        Public ReadOnly DefinedPatterns As KeyPressPattern()

        Public ReadOnly Property HasAny() As Boolean
            Get
                Return DefinedPatterns?.Length > 0
            End Get
        End Property

        Public Sub New(ParamArray ByVal definedPatterns As KeyPressPattern())
            Me.DefinedPatterns = definedPatterns
        End Sub

        Public Shared Widening Operator CType(ByVal definedPatterns As KeyPressPattern()) As KeyPressPatterns
            Return New KeyPressPatterns(definedPatterns)
        End Operator

        Public Function Matches(ByVal keyInfo As ConsoleKeyInfo) As Boolean
            If DefinedPatterns Is Nothing Then
                Return False
            End If
            For Each pattern In DefinedPatterns
                If pattern.Matches(keyInfo) Then
                    Return True
                End If
            Next pattern
            Return False
        End Function

        Public Function Matches(ByVal keyInfo As ConsoleKeyInfo, ByVal modificationRules As ImmutableArray(Of CharacterSetModificationRule)) As Boolean
            For Each rule In modificationRules
                Select Case rule.Kind
                    Case CharacterSetModificationKind.Add
                        If rule.Characters.Contains(keyInfo.KeyChar) Then
                            Return True
                        End If
                        Continue For
                    Case CharacterSetModificationKind.Remove
                        If rule.Characters.Contains(keyInfo.KeyChar) Then
                            Return False
                        End If
                        Continue For
                    Case CharacterSetModificationKind.Replace
                        Return rule.Characters.Contains(keyInfo.KeyChar)
                End Select
            Next rule

            Return Matches(keyInfo)
        End Function
    End Structure


    ''' <summary>
    ''' A rule that modifies a set of characters.
    ''' </summary>
    'INSTANT VB WARNING: VB has no equivalent to the C# readonly struct:
    'ORIGINAL LINE: public readonly struct CharacterSetModificationRule
    Public Structure CharacterSetModificationRule
        ''' <summary>
        ''' The kind of modification.
        ''' </summary>
        Public ReadOnly Property Kind() As CharacterSetModificationKind

        ''' <summary>
        ''' One or more characters.
        ''' </summary>
        Public ReadOnly Property Characters() As ImmutableArray(Of Char)

        'INSTANT VB NOTE: The variable kind was renamed since Visual Basic does not handle local variables named the same as class members well:
        'INSTANT VB NOTE: The variable characters was renamed since Visual Basic does not handle local variables named the same as class members well:
        Public Sub New(ByVal kind_Conflict As CharacterSetModificationKind, ByVal characters_Conflict As ImmutableArray(Of Char))
            Me.Kind = kind_Conflict
            Me.Characters = characters_Conflict
        End Sub
    End Structure

    ''' <summary>
    ''' The kind of character set modification.
    ''' </summary>
    Public Enum CharacterSetModificationKind
        ''' <summary>
        ''' The rule adds new characters onto the existing set of characters.
        ''' </summary>
        Add

        ''' <summary>
        ''' The rule removes characters from the existing set of characters.
        ''' </summary>
        Remove

        ''' <summary>
        ''' The rule replaces the existing set of characters.
        ''' </summary>
        Replace
    End Enum
End Namespace