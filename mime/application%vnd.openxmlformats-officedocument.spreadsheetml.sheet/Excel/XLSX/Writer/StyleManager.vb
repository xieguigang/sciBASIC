#Region "Microsoft.VisualBasic::67250af42603d795b7b6d5bcc9524cc4, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\StyleManager.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 489
    '    Code Lines: 294
    ' Comment Lines: 156
    '   Blank Lines: 39
    '     File Size: 20.36 KB


    '     Class StyleManager
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: AddStyle, (+2 Overloads) AddStyleComponent, GetBorderByHash, GetBorders, GetBorderStyleNumber
    '                   GetCellXfByHash, GetCellXfs, GetCellXfStyleNumber, GetComponentByHash, GetFillByHash
    '                   GetFills, GetFillStyleNumber, GetFontByHash, GetFonts, GetFontStyleNumber
    '                   GetManagedStyles, GetNumberFormatByHash, GetNumberFormats, GetNumberFormatStyleNumber, GetStyleByHash
    '                   GetStyleByName, GetStyleNumber, GetStyles, IsUsedByStyle
    ' 
    '         Sub: CleanupStyleComponents, RemoveStyle, Reorganize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  PicoXLSX is a small .NET library to generate XLSX (Microsoft Excel 2007 or newer) files in an easy and native way
'  Copyright Raphael Stoeckli © 2023
'  This library is licensed under the MIT License.
'  You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
' 

Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer.Style

Namespace XLSX.Writer

    ''' <summary>
    ''' Class representing a style manager to maintain all styles and its components of a workbook
    ''' </summary>
    Public Class StyleManager

        ''' <summary>
        ''' Defines the borders
        ''' </summary>
        Private borders As List(Of AbstractStyle)

        ''' <summary>
        ''' Defines the cellXfs
        ''' </summary>
        Private cellXfs As List(Of AbstractStyle)

        ''' <summary>
        ''' Defines the fills
        ''' </summary>
        Private fills As List(Of AbstractStyle)

        ''' <summary>
        ''' Defines the fonts
        ''' </summary>
        Private fonts As List(Of AbstractStyle)

        ''' <summary>
        ''' Defines the numberFormats
        ''' </summary>
        Private numberFormats As List(Of AbstractStyle)

        ''' <summary>
        ''' Defines the styles
        ''' </summary>
        Private styles As List(Of AbstractStyle)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="StyleManager"/> class
        ''' </summary>
        Public Sub New()
            borders = New List(Of AbstractStyle)()
            cellXfs = New List(Of AbstractStyle)()
            fills = New List(Of AbstractStyle)()
            fonts = New List(Of AbstractStyle)()
            numberFormats = New List(Of AbstractStyle)()
            styles = New List(Of AbstractStyle)()
        End Sub

        ''' <summary>
        ''' Gets a component by its hash
        ''' </summary>
        ''' <param name="list">List to check.</param>
        ''' <param name="hash">Hash of the component.</param>
        ''' <returns>Determined component. If not found, null will be returned.</returns>
        Private Function GetComponentByHash(ByRef list As List(Of AbstractStyle), hash As Integer) As AbstractStyle
            Dim len = list.Count
            For i = 0 To len - 1
                If list(i).GetHashCode() = hash Then
                    Return list(i)
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets a border by its hash
        ''' </summary>
        ''' <param name="hash">Hash of the border.</param>
        ''' <returns>Determined border.</returns>
        Public Function GetBorderByHash(hash As Integer) As Border
            Dim component = GetComponentByHash(borders, hash)
            If component Is Nothing Then
                Throw New StyleException("MissingReferenceException", "The style component with the hash '" & hash.ToString() & "' was not found")
            End If
            Return CType(component, Border)
        End Function

        ''' <summary>
        ''' Gets all borders of the style manager
        ''' </summary>
        ''' <returns>Array of borders.</returns>
        Public Function GetBorders() As Border()
            Return Array.ConvertAll(borders.ToArray(), Function(x) CType(x, Border))
        End Function

        ''' <summary>
        ''' Gets the number of borders in the style manager
        ''' </summary>
        ''' <returns>Number of stored borders.</returns>
        Public Function GetBorderStyleNumber() As Integer
            Return borders.Count
        End Function

        ''' <summary>
        ''' Gets a cellXf by its hash
        ''' </summary>
        ''' <param name="hash">Hash of the cellXf.</param>
        ''' <returns>Determined cellXf.</returns>
        Public Function GetCellXfByHash(hash As Integer) As CellXf
            Dim component = GetComponentByHash(cellXfs, hash)
            If component Is Nothing Then
                Throw New StyleException("MissingReferenceException", "The style component with the hash '" & hash.ToString() & "' was not found")
            End If
            Return CType(component, CellXf)
        End Function

        ''' <summary>
        ''' Gets all cellXfs of the style manager
        ''' </summary>
        ''' <returns>Array of cellXfs.</returns>
        Public Function GetCellXfs() As CellXf()
            Return Array.ConvertAll(cellXfs.ToArray(), Function(x) CType(x, CellXf))
        End Function

        ''' <summary>
        ''' Gets the number of cellXfs in the style manager
        ''' </summary>
        ''' <returns>Number of stored cellXfs.</returns>
        Public Function GetCellXfStyleNumber() As Integer
            Return cellXfs.Count
        End Function

        ''' <summary>
        ''' Gets a fill by its hash
        ''' </summary>
        ''' <param name="hash">Hash of the fill.</param>
        ''' <returns>Determined fill.</returns>
        Public Function GetFillByHash(hash As Integer) As Fill
            Dim component = GetComponentByHash(fills, hash)
            If component Is Nothing Then
                Throw New StyleException("MissingReferenceException", "The style component with the hash '" & hash.ToString() & "' was not found")
            End If
            Return CType(component, Fill)
        End Function

        ''' <summary>
        ''' Gets all fills of the style manager
        ''' </summary>
        ''' <returns>Array of fills.</returns>
        Public Function GetFills() As Fill()
            Return Array.ConvertAll(fills.ToArray(), Function(x) CType(x, Fill))
        End Function

        ''' <summary>
        ''' Gets the number of fills in the style manager
        ''' </summary>
        ''' <returns>Number of stored fills.</returns>
        Public Function GetFillStyleNumber() As Integer
            Return fills.Count
        End Function

        ''' <summary>
        ''' Gets a font by its hash
        ''' </summary>
        ''' <param name="hash">Hash of the font.</param>
        ''' <returns>Determined font.</returns>
        Public Function GetFontByHash(hash As Integer) As Font
            Dim component = GetComponentByHash(fonts, hash)
            If component Is Nothing Then
                Throw New StyleException("MissingReferenceException", "The style component with the hash '" & hash.ToString() & "' was not found")
            End If
            Return CType(component, Font)
        End Function

        ''' <summary>
        ''' Gets all fonts of the style manager
        ''' </summary>
        ''' <returns>Array of fonts.</returns>
        Public Function GetFonts() As Font()
            Return Array.ConvertAll(fonts.ToArray(), Function(x) CType(x, Font))
        End Function

        ''' <summary>
        ''' Gets the number of fonts in the style manager
        ''' </summary>
        ''' <returns>Number of stored fonts.</returns>
        Public Function GetFontStyleNumber() As Integer
            Return fonts.Count
        End Function

        ''' <summary>
        ''' Gets a numberFormat by its hash
        ''' </summary>
        ''' <param name="hash">Hash of the numberFormat.</param>
        ''' <returns>Determined numberFormat.</returns>
        Public Function GetNumberFormatByHash(hash As Integer) As NumberFormat
            Dim component = GetComponentByHash(numberFormats, hash)
            If component Is Nothing Then
                Throw New StyleException("MissingReferenceException", "The style component with the hash '" & hash.ToString() & "' was not found")
            End If
            Return CType(component, NumberFormat)
        End Function

        ''' <summary>
        ''' Gets all numberFormats of the style manager
        ''' </summary>
        ''' <returns>Array of numberFormats.</returns>
        Public Function GetNumberFormats() As NumberFormat()
            Return Array.ConvertAll(numberFormats.ToArray(), Function(x) CType(x, NumberFormat))
        End Function

        ''' <summary>
        ''' Gets the number of numberFormats in the style manager
        ''' </summary>
        ''' <returns>Number of stored numberFormats.</returns>
        Public Function GetNumberFormatStyleNumber() As Integer
            Return numberFormats.Count
        End Function

        ''' <summary>
        ''' Gets a style by its name
        ''' </summary>
        ''' <param name="name">Name of the style.</param>
        ''' <returns>Determined style.</returns>
        Public Function GetStyleByName(name As String) As Style
            Dim len = styles.Count
            For i = 0 To len - 1
                If Equals(CType(styles(i), Style).Name, name) Then
                    Return CType(styles(i), Style)
                End If
            Next
            Throw New StyleException("MissingReferenceException", "The style with the name '" & name & "' was not found")
        End Function

        ''' <summary>
        ''' Gets a style by its hash
        ''' </summary>
        ''' <param name="hash">Hash of the style.</param>
        ''' <returns>Determined style.</returns>
        Public Function GetStyleByHash(hash As Integer) As Style
            Dim component = GetComponentByHash(styles, hash)
            If component Is Nothing Then
                Throw New StyleException("MissingReferenceException", "The style component with the hash '" & hash.ToString() & "' was not found")
            End If
            Return CType(component, Style)
        End Function

        ''' <summary>
        ''' Gets all styles of the style manager
        ''' </summary>
        ''' <returns>Array of styles.</returns>
        Public Function GetStyles() As Style()
            Return Array.ConvertAll(styles.ToArray(), Function(x) CType(x, Style))
        End Function

        ''' <summary>
        ''' Gets the number of styles in the style manager
        ''' </summary>
        ''' <returns>Number of stored styles.</returns>
        Public Function GetStyleNumber() As Integer
            Return styles.Count
        End Function

        ''' <summary>
        ''' Adds a style component to the manager
        ''' </summary>
        ''' <param name="style">Style to add.</param>
        ''' <returns>Added or determined style in the manager.</returns>
        Public Function AddStyle(style As Style) As Style
            Dim hash = AddStyleComponent(style)
            Return CType(GetComponentByHash(styles, hash), Style)
        End Function

        ''' <summary>
        ''' Adds a style component to the manager with an ID
        ''' </summary>
        ''' <param name="style">Component to add.</param>
        ''' <param name="id">Id of the component.</param>
        ''' <returns>Hash of the added or determined component.</returns>
        Private Function AddStyleComponent(style As AbstractStyle, id As Integer?) As Integer
            style.InternalID = id
            Return AddStyleComponent(style)
        End Function

        ''' <summary>
        ''' Adds a style component to the manager
        ''' </summary>
        ''' <param name="style">Component to add.</param>
        ''' <returns>Hash of the added or determined component.</returns>
        Private Function AddStyleComponent(style As AbstractStyle) As Integer
            Dim hash As Integer = style.GetHashCode()
            If style.GetType() Is GetType(Border) Then
                If GetComponentByHash(borders, hash) Is Nothing Then
                    borders.Add(style)
                End If
                Reorganize(borders)
            ElseIf style.GetType() Is GetType(CellXf) Then
                If GetComponentByHash(cellXfs, hash) Is Nothing Then
                    cellXfs.Add(style)
                End If
                Reorganize(cellXfs)
            ElseIf style.GetType() Is GetType(Fill) Then
                If GetComponentByHash(fills, hash) Is Nothing Then
                    fills.Add(style)
                End If
                Reorganize(fills)
            ElseIf style.GetType() Is GetType(Font) Then
                If GetComponentByHash(fonts, hash) Is Nothing Then
                    fonts.Add(style)
                End If
                Reorganize(fonts)
            ElseIf style.GetType() Is GetType(NumberFormat) Then
                If GetComponentByHash(numberFormats, hash) Is Nothing Then
                    numberFormats.Add(style)
                End If
                Reorganize(numberFormats)
            ElseIf style.GetType() Is GetType(Style) Then
                Dim s = CType(style, Style)
                If GetComponentByHash(styles, hash) Is Nothing Then
                    Dim id As Integer?
                    If Not s.InternalID.HasValue Then
                        id = Integer.MaxValue
                        s.InternalID = id
                    Else
                        id = s.InternalID.Value
                    End If
                    Dim temp = AddStyleComponent(s.CurrentBorder, id)
                    s.CurrentBorder = CType(GetComponentByHash(borders, temp), Border)
                    temp = AddStyleComponent(s.CurrentCellXf, id)
                    s.CurrentCellXf = CType(GetComponentByHash(cellXfs, temp), CellXf)
                    temp = AddStyleComponent(s.CurrentFill, id)
                    s.CurrentFill = CType(GetComponentByHash(fills, temp), Fill)
                    temp = AddStyleComponent(s.CurrentFont, id)
                    s.CurrentFont = CType(GetComponentByHash(fonts, temp), Font)
                    temp = AddStyleComponent(s.CurrentNumberFormat, id)
                    s.CurrentNumberFormat = CType(GetComponentByHash(numberFormats, temp), NumberFormat)
                    styles.Add(s)
                End If
                Reorganize(styles)
                hash = s.GetHashCode()
            End If
            Return hash
        End Function

        ''' <summary>
        ''' Removes a style and all its components from the style manager
        ''' </summary>
        ''' <param name="styleName">Name of the style to remove.</param>
        Public Sub RemoveStyle(styleName As String)
            Dim match = False
            Dim len = styles.Count
            Dim index = -1
            For i = 0 To len - 1
                If Equals(CType(styles(i), Style).Name, styleName) Then
                    match = True
                    index = i
                    Exit For
                End If
            Next
            If Not match Then
                Throw New StyleException("MissingReferenceException", "The style with the name '" & styleName & "' was not found in the style manager")
            End If
            styles.RemoveAt(index)
            CleanupStyleComponents()
        End Sub

        ''' <summary>
        ''' Method to gather all styles of the cells in all worksheets
        ''' </summary>
        ''' <param name="workbook">Workbook to get all cells with possible style definitions.</param>
        ''' <returns>StyleManager object, to be processed by the save methods.</returns>
        Friend Shared Function GetManagedStyles(workbook As Workbook) As StyleManager
            Dim styleManager As StyleManager = New StyleManager()
            styleManager.AddStyle(New Style("default", 0, True))
            Dim borderStyle As Style = New Style("default_border_style", 1, True)
            borderStyle.CurrentBorder = BasicStyles.DottedFill_0_125.CurrentBorder
            borderStyle.CurrentFill = BasicStyles.DottedFill_0_125.CurrentFill
            styleManager.AddStyle(borderStyle)

            For i = 0 To workbook.Worksheets.Count - 1
                For Each cell In workbook.Worksheets(i).Cells
                    If cell.Value.CellStyle IsNot Nothing Then
                        Dim resolvedStyle = styleManager.AddStyle(cell.Value.CellStyle)
                        workbook.Worksheets(i).Cells(cell.Key).SetStyle(resolvedStyle, True)
                    End If
                Next
            Next
            Return styleManager
        End Function

        ''' <summary>
        ''' Method to reorganize / reorder a list of style components
        ''' </summary>
        ''' <param name="list">List to reorganize as reference.</param>
        Private Sub Reorganize(ByRef list As List(Of AbstractStyle))
            Dim len = list.Count
            list.Sort()
            Dim id = 0
            For i = 0 To len - 1
                list(i).InternalID = id
                id += 1
            Next
        End Sub

        ''' <summary>
        ''' Method to cleanup style components in the style manager
        ''' </summary>
        Private Sub CleanupStyleComponents()
            Dim border As Border
            Dim cellXf As CellXf
            Dim fill As Fill
            Dim font As Font
            Dim numberFormat As NumberFormat
            Dim len = borders.Count - 1
            Dim i As Integer
            For i = len To 0 Step -1
                border = CType(borders(i), Border)
                If Not IsUsedByStyle(border) Then
                    borders.RemoveAt(i)
                End If
            Next
            len = cellXfs.Count - 1
            For i = len To 0 Step -1
                cellXf = CType(cellXfs(i), CellXf)
                If Not IsUsedByStyle(cellXf) Then
                    cellXfs.RemoveAt(i)
                End If
            Next
            len = fills.Count - 1
            For i = len To 0 Step -1
                fill = CType(fills(i), Fill)
                If Not IsUsedByStyle(fill) Then
                    fills.RemoveAt(i)
                End If
            Next
            len = fonts.Count - 1
            For i = len To 0 Step -1
                font = CType(fonts(i), Font)
                If Not IsUsedByStyle(font) Then
                    fonts.RemoveAt(i)
                End If
            Next
            len = numberFormats.Count - 1
            For i = len To 0 Step -1
                numberFormat = CType(numberFormats(i), NumberFormat)
                If Not IsUsedByStyle(numberFormat) Then
                    numberFormats.RemoveAt(i)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Checks whether a style component in the style manager is used by a style
        ''' </summary>
        ''' <param name="component">Component to check.</param>
        ''' <returns>If true, the component is in use.</returns>
        Private Function IsUsedByStyle(component As AbstractStyle) As Boolean
            Dim s As Style
            Dim match = False
            Dim hash As Integer = component.GetHashCode()
            Dim len = styles.Count
            For i = 0 To len - 1
                s = CType(styles(i), Style)
                If component.GetType() Is GetType(Border) Then
                    If s.CurrentBorder.GetHashCode() = hash Then
                        match = True
                        Exit For
                    End If
                ElseIf component.GetType() Is GetType(CellXf) AndAlso s.CurrentCellXf.GetHashCode() = hash Then
                    match = True
                    Exit For
                End If
                If component.GetType() Is GetType(Fill) AndAlso s.CurrentFill.GetHashCode() = hash Then
                    match = True
                    Exit For
                End If
                If component.GetType() Is GetType(Font) AndAlso s.CurrentFont.GetHashCode() = hash Then
                    match = True
                    Exit For
                End If
                If component.GetType() Is GetType(NumberFormat) AndAlso s.CurrentNumberFormat.GetHashCode() = hash Then
                    match = True
                    Exit For
                End If
            Next
            Return match
        End Function
    End Class
End Namespace
