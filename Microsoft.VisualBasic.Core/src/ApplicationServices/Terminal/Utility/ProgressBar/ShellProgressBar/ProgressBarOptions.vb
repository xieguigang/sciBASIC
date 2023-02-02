Imports System
Imports System.Runtime.InteropServices

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
	''' <summary>
	''' Control the behaviour of your progressbar
	''' </summary>
	Public Class ProgressBarOptions
		Private _enableTaskBarProgress As Boolean
		Public Shared ReadOnly [Default] As ProgressBarOptions = New ProgressBarOptions()

		Public Shared Property ProgressMessageEncodingName As String

		Public Property MessageEncodingName As String
			Get
				Return ProgressMessageEncodingName
			End Get
			Set(value As String)
				ProgressMessageEncodingName = value
			End Set
		End Property

		''' <summary> The foreground color of the progress bar, message and time</summary>
		Public Property ForegroundColor As ConsoleColor = ConsoleColor.Green

		''' <summary> The foreground color the progressbar has reached a 100 percent</summary>
		Public Property ForegroundColorDone As ConsoleColor?

		''' <summary>
		''' The foreground color the progressbar when it has observed an error
		''' <para>If set this takes priority over any other color as soon as an error is observed</para>
		''' Use either <see cref="ProgressBarBase.ObservedError"/> or <see cref="ProgressBarBase.WriteErrorLine"/> to
		''' put the progressbar in errored state
		''' </summary>
		Public Property ForegroundColorError As ConsoleColor?

		''' <summary> The background color of the remainder of the progressbar</summary>
		Public Property BackgroundColor As ConsoleColor?

		''' <summary> The character to use to draw the progressbar</summary>
		Public Property ProgressCharacter As Char = "█"c

		''' <summary>
		''' The character to use for the background of the progress defaults to <see cref="ProgressCharacter"/>
		''' </summary>
		Public Property BackgroundCharacter As Char?

		''' <summary>
		''' When true will redraw the progressbar using a timer, otherwise only update when
		''' <see cref="ProgressBarBase.Tick"/> is called.
		''' Defaults to true
		'''  </summary>
		Public Property DisplayTimeInRealTime As Boolean = True

		''' <summary>
		''' Collapse the progressbar when done, very useful for child progressbars
		''' Defaults to true
		''' </summary>
		Public Property CollapseWhenFinished As Boolean = False

		''' <summary>
		''' By default the text and time information is displayed at the bottom and the progress bar at the top.
		''' This setting swaps their position
		''' </summary>
		Public Property ProgressBarOnBottom As Boolean

		''' <summary>
		''' Progressbar is written on a single line
		''' </summary>
		Public Property DenseProgressBar As Boolean

		''' <summary>
		''' Whether to show the estimated time. It can be set when
		''' <see cref="ProgressBarBase.Tick"/> is called or the property
		''' <see cref="ProgressBarBase.EstimatedDuration"/> is set.
		''' </summary>
		Public Property ShowEstimatedDuration As Boolean

		''' <summary>
		''' Whether to show the percentage number
		''' </summary>
		Public Property DisableBottomPercentage As Boolean = False

		''' <summary> Set percentage decimal format. By default is {0:N2}. </summary>
		Public Property PercentageFormat As String = "{0:N2}% "

		''' <summary>
		''' Use Windows' task bar to display progress.
		''' </summary>
		''' <remarks>
		''' This feature is available on the Windows platform.
		''' </remarks>
		Public Property EnableTaskBarProgress As Boolean
			Get
				Return _enableTaskBarProgress
			End Get
			Set(value As Boolean)
				If value AndAlso Not RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then Throw New NotSupportedException("Task bar progress only works on Windows")

				_enableTaskBarProgress = value
			End Set
		End Property

		''' <summary>
		''' Take ownership of writing a message that is intended to be displayed above the progressbar.
		''' The delegate is expected to return the number of messages written to the console as a result of the string argument.
		''' <para>Use case: pretty print or change the console colors, the progressbar will reset back</para>
		''' </summary>
		Public Property WriteQueuedMessage As Func(Of ConsoleOutLine, Integer)

	End Class
End Namespace
