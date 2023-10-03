#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports PrettyPrompt.Highlighting

Private PrettyPrompt As namespace

''' <summary>
''' An overload item in the Overload Menu Pane.
''' </summary>
<DebuggerDisplay("{Signature}")>
Public Class OverloadItem
	Public ReadOnly Property Signature() As FormattedString
	Public ReadOnly Property Summary() As FormattedString
	Public ReadOnly Property [Return]() As FormattedString
	Public ReadOnly Property Parameters() As IReadOnlyList(Of Parameter)

'INSTANT VB NOTE: The variable signature was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable summary was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable parameters was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Sub New(ByVal signature_Conflict As FormattedString, ByVal summary_Conflict As FormattedString, ByVal returnDescription As FormattedString, ByVal parameters_Conflict As IReadOnlyList(Of Parameter))
		ArgumentNullException.ThrowIfNull(parameters_Conflict)

		Me.Signature = signature_Conflict
		Me.Summary = summary_Conflict
		[Return] = returnDescription
		Me.Parameters = parameters_Conflict
	End Sub

'INSTANT VB WARNING: VB has no equivalent to the C# readonly struct:
'ORIGINAL LINE: public readonly struct Parameter
	<DebuggerDisplay("{Name}: {Description}")>
	Public Structure Parameter
		Public ReadOnly Name As String
		Public ReadOnly Description As FormattedString

'INSTANT VB NOTE: The variable name was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable description was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Sub New(ByVal name_Conflict As String, ByVal description_Conflict As FormattedString)
			ArgumentNullException.ThrowIfNull(name_Conflict)

			Me.Name = name_Conflict
			Me.Description = description_Conflict
		End Sub
	End Structure
End Class