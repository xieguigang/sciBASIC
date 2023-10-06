Namespace ApplicationServices.Terminal


    Public Class PromptConfiguration
        ''' <summary>
        ''' <code>true</code> if the user opted out of color, via an environment variable as specified by https://no-color.org/.
        ''' PrettyPrompt will automatically disable colors in this case. You can read this property to control other colors in
        ''' your application.
        ''' </summary>
        Public Shared ReadOnly Property HasUserOptedOutFromColor() As Boolean = Environment.GetEnvironmentVariable("NO_COLOR") IsNot Nothing
        Public ReadOnly Property KeyBindings() As KeyBindings

        ''' <summary>
        ''' Formatted prompt string to draw (e.g. "> ")
        ''' </summary>
        Public Property Prompt() As FormattedString

        Public ReadOnly Property CompletionBoxBorderFormat As ConsoleFormat
        'INSTANT VB TODO TASK: 'ref return' methods are not converted by Instant VB:
        '	public ref readonly ConsoleFormat CompletionBoxBorderFormat
        '	{
        '		get
        '		{
        '			Return ref completionBoxBorderFormat;
        '		}
        '	}

        Public ReadOnly Property CompletionItemDescriptionPaneBackground() As AnsiColor?
        Public ReadOnly Property SelectedCompletionItemMarker() As FormattedString
        Public ReadOnly Property UnselectedCompletionItemMarker() As String
        Public ReadOnly Property SelectedCompletionItemBackground() As AnsiColor?
        Public ReadOnly Property SelectedTextBackground() As AnsiColor?

        ''' <summary>
        ''' How few completion items we are willing to render. If we do not have a space for rendering of
        ''' completion list with <see cref="MinCompletionItemsCount"/>
        ''' </summary>
        Public ReadOnly Property MinCompletionItemsCount() As Integer

        Public ReadOnly Property MaxCompletionItemsCount() As Integer

        ''' <summary>
        ''' Determines maximum verical space allocated under current input line for completion pane.
        ''' </summary>
        Public ReadOnly Property ProportionOfWindowHeightForCompletionPane() As Double

        Public ReadOnly Property TabSize() As Integer

        'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
        'ORIGINAL LINE: public PromptConfiguration(KeyBindings? keyBindings = null, System.Nullable<FormattedString> prompt = null, System.Nullable<ConsoleFormat> completionBoxBorderFormat = null, System.Nullable<AnsiColor> completionItemDescriptionPaneBackground = null, System.Nullable<FormattedString> selectedCompletionItemMarkSymbol = null, System.Nullable<AnsiColor> selectedCompletionItemBackground = null, System.Nullable<AnsiColor> selectedTextBackground = null, int minCompletionItemsCount = 1, int maxCompletionItemsCount = 9, double proportionOfWindowHeightForCompletionPane = 0.9, int tabSize = 4)
        'INSTANT VB NOTE: The variable keyBindings was renamed since Visual Basic does not handle local variables named the same as class members well:
        'INSTANT VB NOTE: The parameter prompt was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
        'INSTANT VB NOTE: The variable completionItemDescriptionPaneBackground was renamed since Visual Basic does not handle local variables named the same as class members well:
        'INSTANT VB NOTE: The variable selectedCompletionItemBackground was renamed since Visual Basic does not handle local variables named the same as class members well:
        'INSTANT VB NOTE: The variable selectedTextBackground was renamed since Visual Basic does not handle local variables named the same as class members well:
        'INSTANT VB NOTE: The variable minCompletionItemsCount was renamed since Visual Basic does not handle local variables named the same as class members well:
        'INSTANT VB NOTE: The variable maxCompletionItemsCount was renamed since Visual Basic does not handle local variables named the same as class members well:
        'INSTANT VB NOTE: The variable proportionOfWindowHeightForCompletionPane was renamed since Visual Basic does not handle local variables named the same as class members well:
        'INSTANT VB NOTE: The variable tabSize was renamed since Visual Basic does not handle local variables named the same as class members well:
        Public Sub New(Optional ByVal keyBindings_Conflict As KeyBindings = Nothing, Optional ByVal prompt_Conflict? As FormattedString = Nothing, Optional ByVal completionBoxBorderFormat? As ConsoleFormat = Nothing, Optional ByVal completionItemDescriptionPaneBackground_Conflict? As AnsiColor = Nothing, Optional ByVal selectedCompletionItemMarkSymbol? As FormattedString = Nothing, Optional ByVal selectedCompletionItemBackground_Conflict? As AnsiColor = Nothing, Optional ByVal selectedTextBackground_Conflict? As AnsiColor = Nothing, Optional ByVal minCompletionItemsCount_Conflict As Integer = 1, Optional ByVal maxCompletionItemsCount_Conflict As Integer = 9, Optional ByVal proportionOfWindowHeightForCompletionPane_Conflict As Double = 0.9, Optional ByVal tabSize_Conflict As Integer = 4)
            If minCompletionItemsCount_Conflict < 1 Then
                Throw New ArgumentException("must be >=1", NameOf(minCompletionItemsCount_Conflict))
            End If
            If maxCompletionItemsCount_Conflict < minCompletionItemsCount_Conflict Then
                Throw New ArgumentException("must be >=minCompletionItemsCount", NameOf(maxCompletionItemsCount_Conflict))
            End If
            If proportionOfWindowHeightForCompletionPane_Conflict Is <= 0 [or] >= 1 Then
                Throw New ArgumentException("must be >0 and <1", NameOf(proportionOfWindowHeightForCompletionPane_Conflict))
            End If

            'INSTANT VB TODO TASK: Local functions are not converted by Instant VB:
            Dim GetFormat = Function(format As ConsoleFormat) As ConsoleFormat
                                Return If(HasUserOptedOutFromColor, ConsoleFormat.None, format)
                            End Function
            'INSTANT VB TODO TASK: Local functions are not converted by Instant VB:
            Dim GetColor = Function(color As AnsiColor?) As AnsiColor?
                               '		{
                               Return If(HasUserOptedOutFromColor, Nothing, color)
                           End Function


            Me.KeyBindings = If(keyBindings_Conflict, New KeyBindings())
            Me.Prompt = If(prompt_Conflict, "> ")

            Me.CompletionBoxBorderFormat = GetFormat(If(completionBoxBorderFormat, New ConsoleFormat(Foreground:=AnsiColor.Blue)))
            Me.CompletionItemDescriptionPaneBackground = GetColor(completionItemDescriptionPaneBackground_Conflict)

            SelectedCompletionItemMarker = If(selectedCompletionItemMarkSymbol, New FormattedString(">", New FormatSpan(0, 1, AnsiColor.Cyan)))
            UnselectedCompletionItemMarker = New String(" "c, SelectedCompletionItemMarker.Length)
            Me.SelectedCompletionItemBackground = GetColor(selectedCompletionItemBackground_Conflict)
            Me.SelectedTextBackground = GetColor(selectedTextBackground_Conflict)

            Me.MinCompletionItemsCount = minCompletionItemsCount_Conflict
            Me.MaxCompletionItemsCount = maxCompletionItemsCount_Conflict
            Me.ProportionOfWindowHeightForCompletionPane = proportionOfWindowHeightForCompletionPane_Conflict

            Me.TabSize = tabSize_Conflict
        End Sub
    End Class
End Namespace