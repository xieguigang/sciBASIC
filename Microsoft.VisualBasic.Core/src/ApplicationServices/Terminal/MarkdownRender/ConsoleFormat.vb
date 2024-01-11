Namespace ApplicationServices.Terminal

	Public Structure ConsoleFormat

		Public Shared ReadOnly Property None() As ConsoleFormat
			Get
				Return Nothing
			End Get
		End Property

		Public ReadOnly Property ForegroundCode() As String
			Get
				Return Foreground?.GetCode(AnsiColor.Type.Foreground)
			End Get
		End Property
		Public ReadOnly Property BackgroundCode() As String
			Get
				Return Background?.GetCode(AnsiColor.Type.Background)
			End Get
		End Property

		ReadOnly Foreground As AnsiColor?
		ReadOnly Background As AnsiColor?
		ReadOnly Bold As Boolean
		ReadOnly Underline As Boolean
		ReadOnly Inverted As Boolean

		Public ReadOnly Property IsDefault() As Boolean
			Get
				Return Not Foreground.HasValue AndAlso Not Background.HasValue AndAlso Not Bold AndAlso Not Underline AndAlso Not Inverted
			End Get
		End Property

		Sub New(Optional Foreground As AnsiColor = Nothing,
				Optional Background As AnsiColor = Nothing,
				Optional Bold As Boolean = False,
				Optional Underline As Boolean = False,
				Optional Inverted As Boolean = False)

			Me.Foreground = Foreground
			Me.Background = Background
			Me.Bold = Bold
			Me.Underline = Underline
			Me.Inverted = Inverted
		End Sub

		Public Overloads Function Equals(other As ConsoleFormat) As Boolean
			'this is hot from IncrementalRendering.CalculateDiff, so we want to use custom Equals where 'other' is by-ref
			Return Foreground = other.Foreground AndAlso
				Background = other.Background AndAlso
				Bold = other.Bold AndAlso
				Underline = other.Underline AndAlso
				Inverted = other.Inverted
		End Function
	End Structure
End Namespace