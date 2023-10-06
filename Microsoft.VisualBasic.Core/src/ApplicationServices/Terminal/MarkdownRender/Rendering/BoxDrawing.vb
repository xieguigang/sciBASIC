#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region


Namespace ApplicationServices.Terminal

	Friend Class BoxDrawing
		Private ReadOnly configuration As PromptConfiguration

		''' <summary>
		''' CompletionItems: Left border + marker + right padding + right border.
		''' TextLines: Left border + left padding + right padding + right border.
		''' </summary>
		Public Shared Function GetHorizontalBordersWidth(ByVal boxType As BoxType, ByVal configuration As PromptConfiguration) As Integer
			Return If(boxType = BoxType.CompletionItems, 3 + configuration.UnselectedCompletionItemMarker.Length, 4)
		End Function

		''' <inheritdoc cref="GetHorizontalBordersWidth(BoxType, PromptConfiguration)"/>
		Public Function GetHorizontalBordersWidth(ByVal boxType As BoxType) As Integer
			Return GetHorizontalBordersWidth(boxType, configuration)
		End Function

		''' <summary>
		''' Top border + bottom border.
		''' </summary>
		Public Const VerticalBordersHeight As Integer = 2

		''' <summary>Character: ┐</summary>
		Private ReadOnly CornerUpperRightCell As Cell

		''' <summary>Character: ┘</summary>
		Private ReadOnly CornerLowerRightCell As Cell

		''' <summary>Character: ┌</summary>
		Private ReadOnly CornerUpperLeftCell As Cell

		''' <summary>Character: └</summary>
		Private ReadOnly CornerLowerLeftCell As Cell

		''' <summary>Character: ─</summary>
		Private ReadOnly EdgeHorizontalCell As Cell

		''' <summary>Character: │</summary>
		Private ReadOnly EdgeVerticalCell As Cell

		''' <summary>Character: ┤</summary>
		Private ReadOnly EdgeVerticalAndLeftHorizontalCell As Cell

		''' <summary>Character: ├</summary>
		Private ReadOnly EdgeVerticalAndRightHorizontalCell As Cell

		''' <summary>Character: ┬</summary>
		Private ReadOnly EdgeHorizontalAndLowerVerticalCell As Cell

		''' <summary>Character: ┴</summary>
		Private ReadOnly EdgeHorizontalAndUpperVerticalCell As Cell

		''' <summary>Character: ┼</summary>
		Private ReadOnly CrossCell As Cell

		Public Sub New(ByVal configuration As PromptConfiguration)
			'INSTANT VB TODO TASK: 'ref locals' are not converted by Instant VB:
			'ORIGINAL LINE: ref readonly var format = ref configuration.CompletionBoxBorderFormat;
			Dim format = configuration.CompletionBoxBorderFormat
			CornerUpperRightCell = Cell.CreateSingleNonpoolableCell("┐"c, format)
			CornerLowerRightCell = Cell.CreateSingleNonpoolableCell("┘"c, format)
			CornerUpperLeftCell = Cell.CreateSingleNonpoolableCell("┌"c, format)
			CornerLowerLeftCell = Cell.CreateSingleNonpoolableCell("└"c, format)
			EdgeHorizontalCell = Cell.CreateSingleNonpoolableCell("─"c, format)
			EdgeVerticalCell = Cell.CreateSingleNonpoolableCell("│"c, format)
			EdgeVerticalAndLeftHorizontalCell = Cell.CreateSingleNonpoolableCell("┤"c, format)
			EdgeVerticalAndRightHorizontalCell = Cell.CreateSingleNonpoolableCell("├"c, format)
			EdgeHorizontalAndLowerVerticalCell = Cell.CreateSingleNonpoolableCell("┬"c, format)
			EdgeHorizontalAndUpperVerticalCell = Cell.CreateSingleNonpoolableCell("┴"c, format)
			CrossCell = Cell.CreateSingleNonpoolableCell("┼"c, format)
			Me.configuration = configuration
		End Sub

		Public Function BuildFromItemList(ByVal items As IEnumerable(Of FormattedString), ByVal configuration As PromptConfiguration, ByVal maxWidth As Integer, Optional ByVal selectedLineIndex? As Integer = Nothing) As Row()
			Return BuildInternal(items, configuration, maxWidth, selectedLineIndex, configuration.SelectedCompletionItemMarker, configuration.UnselectedCompletionItemMarker, background:=Nothing)
		End Function

		'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
		'ORIGINAL LINE: public Row[] BuildFromLines(IEnumerable<FormattedString> lines, PromptConfiguration configuration, in System.Nullable<AnsiColor> background)
		Public Function BuildFromLines(ByVal lines As IEnumerable(Of FormattedString), ByVal configuration As PromptConfiguration, ByVal background? As AnsiColor) As Row()
			Return BuildInternal(lines, configuration, maxWidth:=Integer.MaxValue, selectedLineIndex:=Nothing, selectedLineMarker:=" ", unselectedLineMarker:=" ", background)
		End Function

		'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
		'ORIGINAL LINE: private Row[] BuildInternal(IEnumerable<FormattedString> lines, PromptConfiguration configuration, int maxWidth, System.Nullable<int> selectedLineIndex, FormattedString selectedLineMarker, string unselectedLineMarker, in System.Nullable<AnsiColor> background)
		Private Function BuildInternal(ByVal lines As IEnumerable(Of FormattedString), ByVal configuration As PromptConfiguration, ByVal maxWidth As Integer, ByVal selectedLineIndex? As Integer, ByVal selectedLineMarker As FormattedString, ByVal unselectedLineMarker As String, ByVal background? As AnsiColor) As Row()
			Const Padding As String = " "
			Dim lineMarkerWidth As Integer = UnicodeWidth.GetWidth(unselectedLineMarker)
			Dim leftPaddingWidth = If(selectedLineIndex.HasValue, lineMarkerWidth, Padding.Length)
			If maxWidth < 1 + leftPaddingWidth + Padding.Length + 1 Then
				Return Array.Empty(Of Row)()
			End If

			Dim capacity As Object
			If Not lines.TryGetNonEnumeratedCount(capacity) Then
				capacity = 16
			End If

			Dim lineList = ListPool(Of FormattedString).Shared.Get(capacity)
			Dim maxLineWidth As Integer = 0
			For Each line In lines
				lineList.Add(line)
				Dim lineWidth = line.GetUnicodeWidth()
				If lineWidth > maxLineWidth Then
					maxLineWidth = lineWidth
				End If
			Next line

			Dim boxWidth = Math.Min(1 + leftPaddingWidth + maxLineWidth + Padding.Length + 1, maxWidth)

			Dim rows = ListPool(Of Row).Shared.Get(lineList.Count)

			'Top border.
			Dim row As New Row(boxWidth)
			row.Add(CornerUpperLeftCell)
			For i As Integer = 1 + 1 To boxWidth - 1
				row.Add(EdgeHorizontalCell)
			Next i
			row.Add(CornerUpperRightCell)
			rows.Add(row)

			'Lines.
			Dim lineAvailableWidth = boxWidth - 1 - leftPaddingWidth - Padding.Length - 1
			For i As Integer = 0 To lineList.Count - 1
				row = New Row(boxWidth)
				Dim line = lineList(i)
				FillLineRow(row, line.Substring(0, Math.Min(line.Length, lineAvailableWidth)), i, background)
				rows.Add(row)
			Next i

			'Bottom border.
			row = New Row(boxWidth)
			row.Add(CornerLowerLeftCell)
			For i As Integer = 1 + 1 To boxWidth - 1
				row.Add(EdgeHorizontalCell)
			Next i
			row.Add(CornerLowerRightCell)
			rows.Add(row)

			Dim result = rows.ToArray()
			ListPool(Of Row).Shared.Put(rows)
			ListPool(Of FormattedString).Shared.Put(lineList)
			Return result

			'INSTANT VB TODO TASK: Local functions are not converted by Instant VB:
			'		void FillLineRow(Row row, FormattedString line, int lineIndex, in System.Nullable(Of AnsiColor) background)
			'		{
			'			'Left border.
			'			row.Add(EdgeVerticalCell);
			'
			'			'Left padding.
			'			bool isSelected = False;
			'			if (selectedLineIndex.TryGet(out var selectedLineIndexValue))
			'			{
			'				if (selectedLineIndexValue == lineIndex)
			'				{
			'					isSelected = True;
			'					row.Add(selectedLineMarker);
			'				}
			'				else
			'				{
			'					row.Add(unselectedLineMarker);
			'				}
			'			}
			'			else
			'			{
			'				row.Add(Padding);
			'			}
			'
			'			'Line.
			'			row.Add(line);
			'
			'			'Right padding.
			'			var rightPaddingWidth = maxLineWidth - line.GetUnicodeWidth() + 1;
			'			for (int i = 0; i < rightPaddingWidth; i++)
			'			{
			'				row.Add(Padding);
			'			}
			'
			'			if (background != Nothing)
			'			{
			'				row.TransformBackground(background, startIndex: 1 + lineMarkerWidth, count: row.Length - 1 - lineMarkerWidth - 1);
			'			}
			'
			'			if (isSelected)
			'			{
			'				row.TransformBackground(configuration.SelectedCompletionItemBackground, startIndex: 1 + lineMarkerWidth, count: row.Length - 1 - lineMarkerWidth - 1);
			'			}
			'
			'			'Right border.
			'			row.Add(EdgeVerticalCell);
			'		}
		End Function

		Public Sub Connect(ByVal overloadBox() As Row, ByVal completionBox() As Row, ByVal documentationBox() As Row)
			If completionBox.Length = 0 Then
				Return
			End If

			If documentationBox.Length > 0 Then
				documentationBox(0).Replace(0, EdgeHorizontalAndLowerVerticalCell) ' ┬
				If completionBox.Length = documentationBox.Length Then
					'  ┌──────────────┬─────────────────────────────┐
					'  │ completion 1 │ documentation box with some |
					'  │ completion 2 │ docs that may wrap.         |
					'  │ completion 3 │ ............                │
					'  └──────────────┴─────────────────────────────┘

					documentationBox(^ 1).Replace(0, EdgeHorizontalAndUpperVerticalCell) ' ┴
				ElseIf completionBox.Length > documentationBox.Length Then
					'  ┌──────────────┬─────────────────────────────┐
					'  │ completion 1 │ documentation box with some |
					'  │ completion 2 │ docs that may wrap.         |
					'  │ completion 3 ├─────────────────────────────┘
					'  └──────────────┘

					documentationBox(^ 1).Replace(0, EdgeVerticalAndRightHorizontalCell) ' ├
				Else
					'  ┌──────────────┬─────────────────────────────┐
					'  │ completion 1 │ documentation box with some │
					'  │ completion 2 │ docs that may wrap.         │
					'  │ completion 3 │ .............               │
					'  └──────────────┤ .............               │
					'                 └─────────────────────────────┘

					documentationBox(completionBox.Length - 1).Replace(0, EdgeVerticalAndLeftHorizontalCell) ' ┤
				End If
			End If

			'////////////////////////////////////////////////////////////////////////////////////////////////////////////

			If overloadBox.Length > 0 Then
				Dim overloadBoxWidth = overloadBox(0).Length
				Dim completionBoxWidth = completionBox(0).Length
				Dim documentationBoxWidth = If(documentationBox.Length = 0, 0, documentationBox(0).Length)

				completionBox(0).Replace(0, EdgeVerticalAndRightHorizontalCell) ' ├
				If overloadBoxWidth = completionBoxWidth + documentationBoxWidth Then
					If documentationBox.Length > 0 Then
						'  ┌────────────────────────────────────────────┐
						'  │ overload help                              |
						'  │ ............                               │
						'  ├──────────────┬─────────────────────────────┤
						'  │ completion 1 │ documentation box with some |
						'  ......................

						documentationBox(0).Replace(documentationBoxWidth - 1, EdgeVerticalAndLeftHorizontalCell) ' ┤
					Else
						'  ┌──────────────┐
						'  │ overloadHelp |
						'  │ ............ │
						'  ├──────────────┤
						'  │ completion 1 │
						'  ................

						completionBox(0).Replace(documentationBoxWidth - 1, EdgeVerticalAndLeftHorizontalCell) ' ┤
					End If
				ElseIf overloadBoxWidth < completionBoxWidth + documentationBoxWidth Then
					If overloadBoxWidth > completionBoxWidth Then
						If documentationBox.Length > 0 Then
							'  ┌────────────────────────────┐
							'  │ overload help              |
							'  │ ............               │
							'  ├──────────────┬─────────────┴───────────────┐
							'  │ completion 1 │ documentation box with some |
							'  ......................

							documentationBox(0).Replace(overloadBoxWidth - completionBoxWidth, EdgeHorizontalAndUpperVerticalCell) ' ┴
						Else
							Debug.Fail("should not happen")
						End If
					ElseIf overloadBoxWidth = completionBoxWidth Then
						If documentationBox.Length > 0 Then
							'  ┌──────────────┐
							'  │ overloadHelp |
							'  │ ............ │
							'  ├──────────────┼─────────────────────────────┐
							'  │ completion 1 │ documentation box with some |
							'  ......................

							documentationBox(0).Replace(0, CrossCell) ' ┼
						Else
							Debug.Fail("This case should already be handled.")
						End If
					Else
						'  ┌──────────┐
						'  │ overHelp |
						'  │ .........│
						'  ├──────────┴───┬─────────────────────────────┐
						'  │ completion 1 │ documentation box with some |
						'  ......................

						completionBox(0).Replace(overloadBoxWidth - 1, EdgeHorizontalAndUpperVerticalCell) ' ┴
					End If
				Else
					If documentationBox.Length > 0 Then
						'  ┌───────────────────────────────────────────────────┐
						'  │ overload help                                     |
						'  │ ............                                      │
						'  ├──────────────┬─────────────────────────────┬──────┘
						'  │ completion 1 │ documentation box with some |
						'  ......................

						documentationBox(0).Replace(0, EdgeHorizontalAndLowerVerticalCell) ' ┬
						documentationBox(0).Replace(documentationBoxWidth - 1, EdgeHorizontalAndLowerVerticalCell) ' ┬
					Else
						'  ┌────────────────────────────┐
						'  │ overload help              |
						'  │ ............               │
						'  ├──────────────┬─────────────┘
						'  │ completion 1 │
						'  ......................

						completionBox(0).Replace(completionBoxWidth - 1, EdgeHorizontalAndLowerVerticalCell) ' ┬
					End If
				End If
			End If
		End Sub
	End Class

	Friend Enum BoxType
		CompletionItems
		TextLines
	End Enum

End Namespace