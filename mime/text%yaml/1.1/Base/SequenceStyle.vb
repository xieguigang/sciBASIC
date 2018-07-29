Namespace Exporter.YAML
	''' <summary>
	''' Specifies the style of a sequence.
	''' </summary>
	Public Enum SequenceStyle
		''' <summary>
		''' The block sequence style.
		''' </summary>
		Block

		''' <summary>
		''' The flow sequence style.
		''' </summary>
		Flow

		''' <summary>
		''' SIngle line with hex data
		''' </summary>
		Raw
	End Enum

	Public NotInheritable Class SequenceStyleExtensions
		Private Sub New()
		End Sub
		''' <summary>
		''' Get scalar style corresponding to current sequence style
		''' </summary>
		''' <param name="_this">Sequence style</param>
		''' <returns>Corresponding scalar style</returns>
		<System.Runtime.CompilerServices.Extension> _
		Public Shared Function ToScalarStyle(_this As SequenceStyle) As ScalarStyle
			If _this = SequenceStyle.Raw Then
				Return ScalarStyle.Hex
			End If
			Return ScalarStyle.Plain
		End Function
	End Class
End Namespace
