#Region "Microsoft.VisualBasic::3cfc22142c8860b77fc7fda9baded77d, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Workbook.vb"

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

'   Total Lines: 941
'    Code Lines: 467 (49.63%)
' Comment Lines: 382 (40.60%)
'    - Xml Docs: 97.91%
' 
'   Blank Lines: 92 (9.78%)
'     File Size: 45.12 KB


'     Class Workbook
' 
'         Properties: CurrentWorksheet, Filename, Hidden, LockStructureIfProtected, LockWindowsIfProtected
'                     SelectedWorksheet, UseWorkbookProtection, WorkbookMetadata, WorkbookProtectionPassword, WorkbookProtectionPasswordHash
'                     Worksheets, WS
' 
'         Constructor: (+5 Overloads) Sub New
' 
'         Function: AddStyle, AddStyleComponent, (+3 Overloads) CopyWorksheetIntoThis, (+3 Overloads) CopyWorksheetTo, GetMruColors
'                   GetNextWorksheetId, (+2 Overloads) GetWorksheet, (+2 Overloads) SetCurrentWorksheet
' 
'         Sub: AddMruColor, (+4 Overloads) AddWorksheet, ClearMruColors, Init, (+4 Overloads) RemoveStyle
'              (+3 Overloads) RemoveWorksheet, ResolveMergedCells, Save, SaveAs, SaveAsStream
'              SetCurrentWorksheet, (+3 Overloads) SetSelectedWorksheet, SetWorkbookProtection, ValidateWorksheets
'         Class Shortener
' 
'             Constructor: (+1 Overloads) Sub New
'             Sub: (+2 Overloads) Down, (+2 Overloads) Formula, (+2 Overloads) Left, NullCheck, (+2 Overloads) Right
'                  SetCurrentWorksheet, SetCurrentWorksheetInternal, (+2 Overloads) Up, (+2 Overloads) Value
' 
' 
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

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.FileIO
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer.Style

Namespace XLSX.Writer

    ''' <summary>
    ''' Class representing a workbook
    ''' </summary>
    Public Class Workbook

        ''' <summary>
        ''' Gets or sets the WorkbookProtectionPasswordHash
        ''' Hash of the protected workbook, originated from <see cref="WorkbookProtectionPassword"/>
        ''' </summary>
        Private _WorkbookProtectionPasswordHash As String
        ''' <summary>
        ''' Defines the filename
        ''' </summary>
        Private m_filename As String

        ''' <summary>
        ''' Defines the currentWorksheet
        ''' </summary>
        Private m_currentWorksheet As Worksheet

        ''' <summary>
        ''' Defines the workbookMetadata
        ''' </summary>
        Private m_workbookMetadata As Metadata

        ''' <summary>
        ''' Defines the workbookProtectionPassword
        ''' </summary>
        Private workbookProtectionPasswordField As String

        ''' <summary>
        ''' Defines the lockWindowsIfProtected
        ''' </summary>
        Private lockWindowsIfProtectedField As Boolean

        ''' <summary>
        ''' Defines the lockStructureIfProtected
        ''' </summary>
        Private lockStructureIfProtectedField As Boolean

        ''' <summary>
        ''' Defines the selectedWorksheet
        ''' </summary>
        Private selectedWorksheetField As Integer

        ''' <summary>
        ''' Defines the shortener
        ''' </summary>
        Private m_shortener As Shortener

        ''' <summary>
        ''' Defines the mruColors
        ''' </summary>
        Private mruColors As List(Of String) = New List(Of String)()

        ''' <summary>
        ''' Gets the shortener object for the current worksheet
        ''' </summary>
        Public ReadOnly Property WS As Shortener
            Get
                Return m_shortener
            End Get
        End Property

        ''' <summary>
        ''' Gets the current worksheet
        ''' </summary>
        Public ReadOnly Property CurrentWorksheet As Worksheet
            Get
                Return m_currentWorksheet
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the filename of the workbook
        ''' </summary>
        Public Property Filename As String
            Get
                Return m_filename
            End Get
            Set(value As String)
                m_filename = value
            End Set
        End Property

        ''' <summary>
        ''' Gets a value indicating whether LockStructureIfProtected
        ''' Gets whether the structure are locked if workbook is protected. See also <see cref="SetWorkbookProtection"/>
        ''' </summary>
        Public ReadOnly Property LockStructureIfProtected As Boolean
            Get
                Return lockStructureIfProtectedField
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether LockWindowsIfProtected
        ''' Gets whether the windows are locked if workbook is protected. See also <see cref="SetWorkbookProtection"/>
        ''' </summary>
        Public ReadOnly Property LockWindowsIfProtected As Boolean
            Get
                Return lockWindowsIfProtectedField
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the WorkbookMetadata
        ''' Meta data object of the workbook
        ''' </summary>
        Public Property WorkbookMetadata As Metadata
            Get
                Return m_workbookMetadata
            End Get
            Set(value As Metadata)
                m_workbookMetadata = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the selected worksheet. The selected worksheet is not the current worksheet while design time but the selected sheet in the output file
        ''' </summary>
        Public ReadOnly Property SelectedWorksheet As Integer
            Get
                Return selectedWorksheetField
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether UseWorkbookProtection
        ''' Gets or sets whether the workbook is protected
        ''' </summary>
        Public Property UseWorkbookProtection As Boolean

        ''' <summary>
        ''' Gets the password used for workbook protection. See also <see cref="SetWorkbookProtection"/>
        ''' </summary>
        Public ReadOnly Property WorkbookProtectionPassword As String
            Get
                Return workbookProtectionPasswordField
            End Get
        End Property

        Public Property WorkbookProtectionPasswordHash As String
            Get
                Return _WorkbookProtectionPasswordHash
            End Get
            Friend Set(value As String)
                _WorkbookProtectionPasswordHash = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the list of worksheets in the workbook
        ''' </summary>
        Public ReadOnly Property Worksheets As New List(Of Worksheet)

        ''' <summary>
        ''' Gets or sets a value indicating whether Hidden
        ''' Gets or sets whether the whole workbook is hidden
        ''' </summary>
        Public Property Hidden As Boolean

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Workbook"/> class
        ''' </summary>
        Public Sub New()
            Init()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Workbook"/> class
        ''' </summary>
        ''' <param name="createWorkSheet">If true, a default worksheet with the name 'Sheet1' will be crated and set as current worksheet.</param>
        Public Sub New(createWorkSheet As Boolean)
            Call Init()

            If createWorkSheet Then
                AddWorksheet("Sheet1")
            End If
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Workbook"/> class
        ''' </summary>
        ''' <param name="sheetName">Filename of the workbook.  The name will be sanitized automatically according to the specifications of Excel.</param>
        Public Sub New(sheetName As String)
            Init()
            AddWorksheet(sheetName, True)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Workbook"/> class
        ''' </summary>
        ''' <param name="filename">Filename of the workbook.  The name will be sanitized automatically according to the specifications of Excel.</param>
        ''' <param name="sheetName">Name of the first worksheet. The name will be sanitized automatically according to the specifications of Excel.</param>
        Public Sub New(filename As String, sheetName As String)
            Init()
            m_filename = filename

            If Not sheetName.StringEmpty() Then
                Call AddWorksheet(sheetName, True)
            End If
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Workbook"/> class
        ''' </summary>
        ''' <param name="filename">Filename of the workbook.</param>
        ''' <param name="sheetName">Name of the first worksheet.</param>
        ''' <param name="sanitizeSheetName">If true, the name of the worksheet will be sanitized automatically according to the specifications of Excel.</param>
        Public Sub New(filename As String, sheetName As String, sanitizeSheetName As Boolean)
            Init()
            m_filename = filename
            If sanitizeSheetName Then
                AddWorksheet(Worksheet.SanitizeWorksheetName(sheetName, Me))
            Else
                AddWorksheet(sheetName)
            End If
        End Sub

        ''' <summary>
        ''' Adds a color value (HEX; 6-digit RGB or 8-digit ARGB) to the MRU list
        ''' </summary>
        ''' <param name="color">RGB code in hex format (either 6 characters, e.g. FF00AC or 8 characters with leading alpha value). Alpha will be set to full opacity (FF) in case of 6 characters.</param>
        Public Sub AddMruColor(color As String)
            If color IsNot Nothing AndAlso color.Length = 6 Then
                color = "FF" & color
            End If
            Style.Fill.ValidateColor(color, True)
            mruColors.Add(color.ToUpper())
        End Sub

        ''' <summary>
        ''' Gets the MRU color list
        ''' </summary>
        ''' <returns>Immutable list of color values.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMruColors() As IReadOnlyList(Of String)
            Return mruColors
        End Function

        ''' <summary>
        ''' Clears the MRU color list
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub ClearMruColors()
            mruColors.Clear()
        End Sub

        ''' <summary>
        ''' Adds a style to the style repository. This method is deprecated since it has no direct impact on the generated file
        ''' </summary>
        ''' <param name="style">Style to add.</param>
        ''' <returns>Returns the managed style of the style repository.</returns>
        <Obsolete("This method has no direct impact on the generated file and is deprecated.")>
        Public Function AddStyle(style As Style) As Style
            Return StyleRepository.Instance.AddStyle(style)
        End Function

        ''' <summary>
        ''' Adds a style component to a style. This method is deprecated since it has no direct impact on the generated file
        ''' </summary>
        ''' <param name="baseStyle">Style to append a component.</param>
        ''' <param name="newComponent">Component to add to the baseStyle.</param>
        ''' <returns>Returns the modified style of the style repository.</returns>
        <Obsolete("This method has no direct impact on the generated file and is deprecated.")>
        Public Function AddStyleComponent(baseStyle As Style, newComponent As AbstractStyle) As Style

            If newComponent.GetType() Is GetType(Border) Then
                baseStyle.CurrentBorder = CType(newComponent, Border)
            ElseIf newComponent.GetType() Is GetType(CellXf) Then
                baseStyle.CurrentCellXf = CType(newComponent, CellXf)
            ElseIf newComponent.GetType() Is GetType(Fill) Then
                baseStyle.CurrentFill = CType(newComponent, Fill)
            ElseIf newComponent.GetType() Is GetType(Font) Then
                baseStyle.CurrentFont = CType(newComponent, Font)
            ElseIf newComponent.GetType() Is GetType(NumberFormat) Then
                baseStyle.CurrentNumberFormat = CType(newComponent, NumberFormat)
            End If
            Return StyleRepository.Instance.AddStyle(baseStyle)
        End Function

        ''' <summary>
        ''' Adding a new Worksheet. The new worksheet will be defined as current worksheet
        ''' </summary>
        ''' <param name="name">Name of the new worksheet.</param>
        Public Sub AddWorksheet(name As String)
            For Each item In Worksheets
                If Equals(item.SheetName, name) Then
                    Throw New WorksheetException("The worksheet with the name '" & name & "' already exists.")
                End If
            Next
            Dim number As Integer = GetNextWorksheetId()
            Dim newWs As Worksheet = New Worksheet(name, number, Me)
            m_currentWorksheet = newWs
            Worksheets.Add(newWs)
            m_shortener.SetCurrentWorksheetInternal(m_currentWorksheet)
        End Sub

        ''' <summary>
        ''' Adding a new Worksheet with a sanitizing option. The new worksheet will be defined as current worksheet
        ''' </summary>
        ''' <param name="name">Name of the new worksheet.</param>
        ''' <param name="sanitizeSheetName">If true, the name of the worksheet will be sanitized automatically according to the specifications of Excel.</param>
        Public Sub AddWorksheet(name As String, sanitizeSheetName As Boolean)
            If sanitizeSheetName Then
                Dim sanitized = Worksheet.SanitizeWorksheetName(name, Me)
                AddWorksheet(sanitized)
            Else
                AddWorksheet(name)
            End If
        End Sub

        ''' <summary>
        ''' Adding a new Worksheet. The new worksheet will be defined as current worksheet
        ''' </summary>
        ''' <param name="worksheet">Prepared worksheet object.</param>
        Public Sub AddWorksheet(worksheet As Worksheet)
            AddWorksheet(worksheet, False)
        End Sub

        ''' <summary>
        ''' Adding a new Worksheet. The new worksheet will be defined as current worksheet
        ''' </summary>
        ''' <param name="worksheet">Prepared worksheet object.</param>
        ''' <param name="sanitizeSheetName">If true, the name of the worksheet will be sanitized automatically according to the specifications of Excel.</param>
        Public Sub AddWorksheet(worksheet As Worksheet, sanitizeSheetName As Boolean)
            If sanitizeSheetName Then
                Dim name = worksheet.SanitizeWorksheetName(worksheet.SheetName, Me)
                worksheet.SheetName = name
            Else
                If String.IsNullOrEmpty(worksheet.SheetName) Then
                    Throw New WorksheetException("The name of the passed worksheet is null or empty.")
                End If
                For i = 0 To Worksheets.Count - 1
                    If Equals(Worksheets.Item(i).SheetName, worksheet.SheetName) Then
                        Throw New WorksheetException("The worksheet with the name '" & worksheet.SheetName & "' already exists.")
                    End If
                Next
            End If
            worksheet.SheetID = GetNextWorksheetId()
            m_currentWorksheet = worksheet
            Worksheets.Add(worksheet)
            worksheet.WorkbookReference = Me
        End Sub

        ''' <summary>
        ''' Removes the passed style from the style sheet. This method is deprecated since it has no direct impact on the generated file
        ''' </summary>
        ''' <param name="style">Style to remove.</param>
        <Obsolete("This method has no direct impact on the generated file and is deprecated.")>
        Public Sub RemoveStyle(style As Style)
            RemoveStyle(style, False)
        End Sub

        ''' <summary>
        ''' Removes the defined style from the style sheet of the workbook. This method is deprecated since it has no direct impact on the generated file
        ''' </summary>
        ''' <param name="styleName">Name of the style to be removed.</param>
        <Obsolete("This method has no direct impact on the generated file and is deprecated.")>
        Public Sub RemoveStyle(styleName As String)
            RemoveStyle(styleName, False)
        End Sub

        ''' <summary>
        ''' Removes the defined style from the style sheet of the workbook
        ''' </summary>
        ''' <param name="style">Style to remove.</param>
        ''' <param name="onlyIfUnused">If true, the style will only be removed if not used in any cell.</param>
        <Obsolete("This method has no direct impact on the generated file and is deprecated.")>
        Public Sub RemoveStyle(style As Style, onlyIfUnused As Boolean)
            If style Is Nothing Then
                Throw New StyleException("MissingReferenceException", "The style to remove is not defined")
            End If
            RemoveStyle(style.Name, onlyIfUnused)
        End Sub

        ''' <summary>
        ''' Removes the defined style from the style sheet of the workbook. This method is deprecated since it has no direct impact on the generated file
        ''' </summary>
        ''' <param name="styleName">Name of the style to be removed.</param>
        ''' <param name="onlyIfUnused">If true, the style will only be removed if not used in any cell.</param>
        <Obsolete("This method has no direct impact on the generated file and is deprecated.")>
        Public Sub RemoveStyle(styleName As String, onlyIfUnused As Boolean)
            If String.IsNullOrEmpty(styleName) Then
                Throw New StyleException("MissingReferenceException", "The style to remove is not defined (no name specified)")
            End If
        End Sub

        ''' <summary>
        ''' Removes the defined worksheet based on its name. If the worksheet is the current or selected worksheet, the current and / or the selected worksheet will be set to the last worksheet of the workbook
        ''' Removes the defined worksheet based on its name. If the worksheet is the current or selected worksheet, the current and / or the selected worksheet will be set to the last worksheet of the workbook
        ''' </summary>
        ''' <param name="name">Name of the worksheet.</param>
        Public Sub RemoveWorksheet(name As String)
            Dim worksheetToRemove = Worksheets.FindLast(Function(w) Equals(w.SheetName, name))
            If worksheetToRemove Is Nothing Then
                Throw New WorksheetException("The worksheet with the name '" & name & "' does not exist.")
            End If
            Dim index = Worksheets.IndexOf(worksheetToRemove)
            Dim resetCurrentWorksheet = worksheetToRemove Is m_currentWorksheet
            RemoveWorksheet(index, resetCurrentWorksheet)
        End Sub

        ''' <summary>
        ''' Removes the defined worksheet based on its index. If the worksheet is the current or selected worksheet, the current and / or the selected worksheet will be set to the last worksheet of the workbook
        ''' Removes the defined worksheet based on its index. If the worksheet is the current or selected worksheet, the current and / or the selected worksheet will be set to the last worksheet of the workbook
        ''' </summary>
        ''' <param name="index">Index within the worksheets list.</param>
        Public Sub RemoveWorksheet(index As Integer)
            If index < 0 OrElse index >= Worksheets.Count Then
                Throw New WorksheetException("The worksheet index " & index.ToString() & " is out of range")
            End If
            Dim resetCurrentWorksheet = Worksheets.Item(index) Is m_currentWorksheet
            RemoveWorksheet(index, resetCurrentWorksheet)
        End Sub

        ''' <summary>
        ''' Method to resolve all merged cells in all worksheets. Only the value of the very first cell of the locked cells range will be visible. The other values are still present (set to EMPTY) but will not be stored in the worksheet.<br/>
        ''' This is an internal method. There is no need to use it
        ''' </summary>
        Friend Sub ResolveMergedCells()
            For Each worksheet As Worksheet In Worksheets
                Call worksheet.ResolveMergedCells()
            Next
        End Sub

        ''' <summary>
        ''' Saves the workbook
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Save()
            Call New LowLevel(Me).Save()
        End Sub

        ''' <summary>
        ''' Saves the workbook asynchronous
        ''' </summary>
        ''' <returns>Task object (void).</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Async Function SaveAsync() As Task
            Await New LowLevel(Me).SaveAsync()
        End Function

        ''' <summary>
        ''' Saves the workbook with the defined name
        ''' </summary>
        ''' <param name="fileName">filename of the saved workbook.</param>
        Public Sub SaveAs(fileName As String)
            Dim backup = fileName
            m_filename = fileName
            Dim l As LowLevel = New LowLevel(Me)
            l.Save()
            m_filename = backup
        End Sub

        ''' <summary>
        ''' Saves the workbook with the defined name asynchronous
        ''' </summary>
        ''' <param name="fileName">filename of the saved workbook.</param>
        ''' <returns>Task object (void).</returns>
        Public Async Function SaveAsAsync(fileName As String) As Task
            Dim backup = fileName
            m_filename = fileName
            Dim l As LowLevel = New LowLevel(Me)
            Await l.SaveAsync()
            m_filename = backup
        End Function

        ''' <summary>
        ''' Save the workbook to a writable stream
        ''' </summary>
        ''' <param name="stream">Writable stream.</param>
        ''' <param name="leaveOpen">Optional parameter to keep the stream open after writing (used for MemoryStreams; default is false).</param>
        Public Sub SaveAsStream(stream As Stream, Optional leaveOpen As Boolean = False)
            Call New LowLevel(Me).SaveAsStream(stream, leaveOpen)
        End Sub

        ''' <summary>
        ''' Save the workbook to a writable stream asynchronous
        ''' </summary>
        ''' <param name="stream">>Writable stream.</param>
        ''' <param name="leaveOpen">Optional parameter to keep the stream open after writing (used for MemoryStreams; default is false).</param>
        ''' <returns>Task object (void).</returns>
        Public Async Function SaveAsStreamAsync(stream As Stream, Optional leaveOpen As Boolean = False) As Task
            Await New LowLevel(Me).SaveAsStreamAsync(stream, leaveOpen)
        End Function

        ''' <summary>
        ''' Sets the current worksheet
        ''' </summary>
        ''' <param name="name">Name of the worksheet.</param>
        ''' <returns>Returns the current worksheet.</returns>
        Public Function SetCurrentWorksheet(name As String) As Worksheet
            m_currentWorksheet = Worksheets.FirstOrDefault(Function(w) Equals(w.SheetName, name))
            If m_currentWorksheet Is Nothing Then
                Throw New WorksheetException("The worksheet with the name '" & name & "' does not exist.")
            End If
            m_shortener.SetCurrentWorksheetInternal(m_currentWorksheet)
            Return m_currentWorksheet
        End Function

        ''' <summary>
        ''' Sets the current worksheet
        ''' </summary>
        ''' <param name="worksheetIndex">Zero-based worksheet index.</param>
        ''' <returns>Returns the current worksheet.</returns>
        Public Function SetCurrentWorksheet(worksheetIndex As Integer) As Worksheet
            If worksheetIndex < 0 OrElse worksheetIndex > Worksheets.Count - 1 Then
                Throw New RangeException("OutOfRangeException", "The worksheet index " & worksheetIndex.ToString() & " is out of range")
            End If
            m_currentWorksheet = Worksheets.Item(worksheetIndex)
            m_shortener.SetCurrentWorksheetInternal(m_currentWorksheet)
            Return m_currentWorksheet
        End Function

        ''' <summary>
        ''' Sets the current worksheet
        ''' </summary>
        ''' <param name="worksheet">Worksheet object (must be in the collection of worksheets).</param>
        Public Sub SetCurrentWorksheet(worksheet As Worksheet)
            Dim index = Worksheets.IndexOf(worksheet)
            If index < 0 Then
                Throw New WorksheetException("The passed worksheet object is not in the worksheet collection.")
            End If
            m_currentWorksheet = Worksheets.Item(index)
            m_shortener.SetCurrentWorksheetInternal(worksheet)
        End Sub

        ''' <summary>
        ''' Sets the selected worksheet in the output workbook
        ''' </summary>
        ''' <param name="name">Name of the worksheet.</param>
        Public Sub SetSelectedWorksheet(name As String)
            selectedWorksheetField = Worksheets.FindIndex(Function(w) Equals(w.SheetName, name))
            If selectedWorksheetField < 0 Then
                Throw New WorksheetException("The worksheet with the name '" & name & "' does not exist.")
            End If
            ValidateWorksheets()
        End Sub

        ''' <summary>
        ''' Sets the selected worksheet in the output workbook
        ''' </summary>
        ''' <param name="worksheetIndex">Zero-based worksheet index.</param>
        Public Sub SetSelectedWorksheet(worksheetIndex As Integer)
            If worksheetIndex < 0 OrElse worksheetIndex > Worksheets.Count - 1 Then
                Throw New RangeException("OutOfRangeException", "The worksheet index " & worksheetIndex.ToString() & " is out of range")
            End If
            selectedWorksheetField = worksheetIndex
            ValidateWorksheets()
        End Sub

        ''' <summary>
        ''' Sets the selected worksheet in the output workbook
        ''' </summary>
        ''' <param name="worksheet">Worksheet object (must be in the collection of worksheets).</param>
        Public Sub SetSelectedWorksheet(worksheet As Worksheet)
            selectedWorksheetField = Worksheets.IndexOf(worksheet)
            If selectedWorksheetField < 0 Then
                Throw New WorksheetException("The passed worksheet object is not in the worksheet collection.")
            End If
            ValidateWorksheets()
        End Sub

        ''' <summary>
        ''' Gets a worksheet from this workbook by name
        ''' </summary>
        ''' <param name="name">Name of the worksheet.</param>
        ''' <returns>Worksheet with the passed name.</returns>
        Public Function GetWorksheet(name As String) As Worksheet
            Dim index = Worksheets.FindIndex(Function(w) w.SheetName = name)
            If index < 0 Then
                Throw New WorksheetException("No worksheet with the name '" & name & "' was found in this workbook.")
            End If
            Return Worksheets.Item(index)
        End Function

        ''' <summary>
        ''' Gets a worksheet from this workbook by index
        ''' </summary>
        ''' <param name="index">Index of the worksheet.</param>
        ''' <returns>Worksheet with the passed index.</returns>
        Public Function GetWorksheet(index As Integer) As Worksheet
            If index < 0 OrElse index > Worksheets.Count - 1 Then
                Throw New RangeException("OutOfRangeException", "The worksheet index " & index.ToString() & " is out of range")
            End If
            Return Worksheets.Item(index)
        End Function

        ''' <summary>
        ''' Sets or removes the workbook protection. If protectWindows and protectStructure are both false, the workbook will not be protected
        ''' </summary>
        ''' <param name="state">If true, the workbook will be protected, otherwise not.</param>
        ''' <param name="protectWindows">If true, the windows will be locked if the workbook is protected.</param>
        ''' <param name="protectStructure">If true, the structure will be locked if the workbook is protected.</param>
        ''' <param name="password">Optional password. If null or empty, no password will be set in case of protection.</param>
        Public Sub SetWorkbookProtection(state As Boolean, protectWindows As Boolean, protectStructure As Boolean, password As String)
            lockWindowsIfProtectedField = protectWindows
            lockStructureIfProtectedField = protectStructure
            workbookProtectionPasswordField = password
            WorkbookProtectionPasswordHash = LowLevel.GeneratePasswordHash(password)
            If protectWindows = False AndAlso protectStructure = False Then
                UseWorkbookProtection = False
            Else
                UseWorkbookProtection = state
            End If
        End Sub

        ''' <summary>
        ''' Copies a worksheet of the current workbook by its name
        ''' </summary>
        ''' <param name="sourceWorksheetName">Name of the worksheet to copy, originated in this workbook.</param>
        ''' <param name="newWorksheetName">Name of the new worksheet (copy).</param>
        ''' <param name="sanitizeSheetName">If true, the new name will be automatically sanitized if a name collision occurs.</param>
        ''' <returns>Copied worksheet.</returns>
        Public Function CopyWorksheetIntoThis(sourceWorksheetName As String, newWorksheetName As String, Optional sanitizeSheetName As Boolean = True) As Worksheet
            Dim sourceWorksheet = GetWorksheet(sourceWorksheetName)
            Return CopyWorksheetTo(sourceWorksheet, newWorksheetName, Me, sanitizeSheetName)
        End Function

        ''' <summary>
        ''' Copies a worksheet of the current workbook by its index
        ''' </summary>
        ''' <param name="sourceWorksheetIndex">Index of the worksheet to copy, originated in this workbook.</param>
        ''' <param name="newWorksheetName">Name of the new worksheet (copy).</param>
        ''' <param name="sanitizeSheetName">If true, the new name will be automatically sanitized if a name collision occurs.</param>
        ''' <returns>Copied worksheet.</returns>
        Public Function CopyWorksheetIntoThis(sourceWorksheetIndex As Integer, newWorksheetName As String, Optional sanitizeSheetName As Boolean = True) As Worksheet
            Dim sourceWorksheet = GetWorksheet(sourceWorksheetIndex)
            Return CopyWorksheetTo(sourceWorksheet, newWorksheetName, Me, sanitizeSheetName)
        End Function

        ''' <summary>
        ''' Copies a worksheet of any workbook into the current workbook
        ''' </summary>
        ''' <param name="sourceWorksheet">Worksheet to copy.</param>
        ''' <param name="newWorksheetName">Name of the new worksheet (copy).</param>
        ''' <param name="sanitizeSheetName">If true, the new name will be automatically sanitized if a name collision occurs.</param>
        ''' <returns>Copied worksheet.</returns>
        Public Function CopyWorksheetIntoThis(sourceWorksheet As Worksheet, newWorksheetName As String, Optional sanitizeSheetName As Boolean = True) As Worksheet
            Return CopyWorksheetTo(sourceWorksheet, newWorksheetName, Me, sanitizeSheetName)
        End Function

        ''' <summary>
        ''' Copies a worksheet of the current workbook by its name into another workbook
        ''' </summary>
        ''' <param name="sourceWorksheetName">Name of the worksheet to copy, originated in this workbook.</param>
        ''' <param name="newWorksheetName">Name of the new worksheet (copy).</param>
        ''' <param name="targetWorkbook">Workbook to copy the worksheet into.</param>
        ''' <param name="sanitizeSheetName">If true, the new name will be automatically sanitized if a name collision occurs.</param>
        ''' <returns>Copied worksheet.</returns>
        Public Function CopyWorksheetTo(sourceWorksheetName As String,
                                        newWorksheetName As String,
                                        targetWorkbook As Workbook,
                                        Optional sanitizeSheetName As Boolean = True) As Worksheet

            Dim sourceWorksheet = GetWorksheet(sourceWorksheetName)
            Return CopyWorksheetTo(sourceWorksheet, newWorksheetName, targetWorkbook, sanitizeSheetName)
        End Function

        ''' <summary>
        ''' Copies a worksheet of the current workbook by its index into another workbook
        ''' </summary>
        ''' <param name="sourceWorksheetIndex">Index of the worksheet to copy, originated in this workbook.</param>
        ''' <param name="newWorksheetName">Name of the new worksheet (copy).</param>
        ''' <param name="targetWorkbook">Workbook to copy the worksheet into.</param>
        ''' <param name="sanitizeSheetName">If true, the new name will be automatically sanitized if a name collision occurs.</param>
        ''' <returns>Copied worksheet.</returns>
        Public Function CopyWorksheetTo(sourceWorksheetIndex As Integer,
                                        newWorksheetName As String,
                                        targetWorkbook As Workbook,
                                        Optional sanitizeSheetName As Boolean = True) As Worksheet

            Dim sourceWorksheet = GetWorksheet(sourceWorksheetIndex)
            Return CopyWorksheetTo(sourceWorksheet, newWorksheetName, targetWorkbook, sanitizeSheetName)
        End Function

        ''' <summary>
        ''' Copies a worksheet of any workbook into the another workbook
        ''' </summary>
        ''' <param name="sourceWorksheet">Worksheet to copy.</param>
        ''' <param name="newWorksheetName">Name of the new worksheet (copy).</param>
        ''' <param name="targetWorkbook">Workbook to copy the worksheet into.</param>
        ''' <param name="sanitizeSheetName">If true, the new name will be automatically sanitized if a name collision occurs.</param>
        ''' <returns>Copied worksheet.</returns>
        Public Shared Function CopyWorksheetTo(sourceWorksheet As Worksheet,
                                               newWorksheetName As String,
                                               targetWorkbook As Workbook,
                                               Optional sanitizeSheetName As Boolean = True) As Worksheet

            If targetWorkbook Is Nothing Then
                Throw New WorksheetException("The target workbook cannot be null")
            End If
            If sourceWorksheet Is Nothing Then
                Throw New WorksheetException("The source worksheet cannot be null")
            End If
            Dim copy As Worksheet = sourceWorksheet.Copy()
            copy.SetSheetName(newWorksheetName)
            Dim currentWorksheet = targetWorkbook.CurrentWorksheet
            targetWorkbook.AddWorksheet(copy, sanitizeSheetName)
            targetWorkbook.SetCurrentWorksheet(currentWorksheet)
            Return copy
        End Function

        ''' <summary>
        ''' Validates the worksheets regarding several conditions that must be met:<br/>
        ''' - At least one worksheet must be defined<br/>
        ''' - A hidden worksheet cannot be the selected one<br/>
        ''' - At least one worksheet must be visible<br/>
        ''' If one of the conditions is not met, an exception is thrown
        ''' </summary>
        Friend Sub ValidateWorksheets()
            Dim woksheetCount = Worksheets.Count
            If woksheetCount = 0 Then
                Throw New WorksheetException("The workbook must contain at least one worksheet")
            End If
            For i = 0 To woksheetCount - 1
                If Worksheets.Item(i).Hidden Then
                    If i = selectedWorksheetField Then
                        Throw New WorksheetException("The worksheet with the index " & selectedWorksheetField.ToString() & " cannot be set as selected, since it is set hidden")
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' Removes the worksheet at the defined index and relocates current and selected worksheet references
        ''' </summary>
        ''' <param name="index">Index within the worksheets list.</param>
        ''' <param name="resetCurrentWorksheet">If true, the current worksheet will be relocated to the last worksheet in the list.</param>
        Private Sub RemoveWorksheet(index As Integer, resetCurrentWorksheet As Boolean)
            Worksheets.RemoveAt(index)
            If Worksheets.Count > 0 Then
                For i = 0 To Worksheets.Count - 1
                    Worksheets.Item(i).SheetID = i + 1
                Next
                If resetCurrentWorksheet Then
                    m_currentWorksheet = Worksheets.Item(Worksheets.Count - 1)
                End If
                If selectedWorksheetField = index OrElse selectedWorksheetField > Worksheets.Count - 1 Then
                    selectedWorksheetField = Worksheets.Count - 1
                End If
            Else
                m_currentWorksheet = Nothing
                selectedWorksheetField = 0
            End If
            ValidateWorksheets()
        End Sub

        ''' <summary>
        ''' Gets the next free worksheet ID
        ''' </summary>
        ''' <returns>Worksheet ID.</returns>
        Private Function GetNextWorksheetId() As Integer
            If Worksheets.Count = 0 Then
                Return 1
            End If
            Return Worksheets.Max(Function(w) w.SheetID) + 1
        End Function

        ''' <summary>
        ''' Init method called in the constructors
        ''' </summary>
        Private Sub Init()
            _Worksheets = New List(Of Worksheet)()
            m_workbookMetadata = New Metadata()
            m_shortener = New Shortener(Me)
        End Sub

        ''' <summary>
        ''' Class to provide access to the current worksheet with a shortened syntax. Note: The WS object can be null if the workbook was created without a worksheet. The object will be available as soon as the current worksheet is defined
        ''' </summary>
        Public Class Shortener
            ''' <summary>
            ''' Defines the currentWorksheet
            ''' </summary>
            Private currentWorksheet As Worksheet

            ''' <summary>
            ''' Defines the workbookReference
            ''' </summary>
            Private ReadOnly workbookReference As Workbook

            ''' <summary>
            ''' Initializes a new instance of the <see cref="Shortener"/> class
            ''' </summary>
            ''' <param name="reference">Workbook reference.</param>
            Public Sub New(reference As Workbook)
                workbookReference = reference
                currentWorksheet = reference.CurrentWorksheet
            End Sub

            ''' <summary>
            ''' Sets the worksheet accessed by the shortener
            ''' </summary>
            ''' <param name="worksheet">Current worksheet.</param>
            Public Sub SetCurrentWorksheet(worksheet As Worksheet)
                workbookReference.SetCurrentWorksheet(worksheet)
                currentWorksheet = worksheet
            End Sub

            ''' <summary>
            ''' Sets the worksheet accessed by the shortener, invoked by the workbook
            ''' </summary>
            ''' <param name="worksheet">Current worksheet.</param>
            Friend Sub SetCurrentWorksheetInternal(worksheet As Worksheet)
                currentWorksheet = worksheet
            End Sub

            ''' <summary>
            ''' Sets a value into the current cell and moves the cursor to the next cell (column or row depending on the defined cell direction)
            ''' </summary>
            ''' <param name="pValue">Value to set.</param>
            Public Sub Value(pValue As Object)
                NullCheck()
                currentWorksheet.AddNextCell(pValue)
            End Sub

            ''' <summary>
            ''' Sets a value with style into the current cell and moves the cursor to the next cell (column or row depending on the defined cell direction)
            ''' </summary>
            ''' <param name="pValue">Value to set.</param>
            ''' <param name="style">Style to apply.</param>
            Public Sub Value(pValue As Object, style As Style)
                NullCheck()
                currentWorksheet.AddNextCell(pValue, style)
            End Sub

            ''' <summary>
            ''' Sets a formula into the current cell and moves the cursor to the next cell (column or row depending on the defined cell direction)
            ''' </summary>
            ''' <param name="pFormula">Formula to set.</param>
            Public Sub Formula(pFormula As String)
                NullCheck()
                currentWorksheet.AddNextCellFormula(pFormula)
            End Sub

            ''' <summary>
            ''' Sets a formula with style into the current cell and moves the cursor to the next cell (column or row depending on the defined cell direction)
            ''' </summary>
            ''' <param name="pFormula">Formula to set.</param>
            ''' <param name="style">Style to apply.</param>
            Public Sub Formula(pFormula As String, style As Style)
                NullCheck()
                currentWorksheet.AddNextCellFormula(pFormula, style)
            End Sub

            ''' <summary>
            ''' Moves the cursor one row down
            ''' </summary>
            Public Sub Down()
                NullCheck()
                currentWorksheet.GoToNextRow()
            End Sub

            ''' <summary>
            ''' Moves the cursor the number of defined rows down
            ''' </summary>
            ''' <param name="numberOfRows">Number of rows to move.</param>
            ''' <param name="keepColumnPosition">If true, the column position is preserved, otherwise set to 0.</param>
            Public Sub Down(numberOfRows As Integer, Optional keepColumnPosition As Boolean = False)
                NullCheck()
                currentWorksheet.GoToNextRow(numberOfRows, keepColumnPosition)
            End Sub

            ''' <summary>
            ''' Moves the cursor one row up
            ''' </summary>
            Public Sub Up()
                NullCheck()
                currentWorksheet.GoToNextRow(-1)
            End Sub

            ''' <summary>
            ''' Moves the cursor the number of defined rows up
            ''' </summary>
            ''' <param name="numberOfRows">Number of rows to move.</param>
            ''' <param name="keepColumnosition">If true, the column position is preserved, otherwise set to 0.</param>
            Public Sub Up(numberOfRows As Integer, Optional keepColumnosition As Boolean = False)
                NullCheck()
                currentWorksheet.GoToNextRow(-1 * numberOfRows, keepColumnosition)
            End Sub

            ''' <summary>
            ''' Moves the cursor one column to the right
            ''' </summary>
            Public Sub Right()
                NullCheck()
                currentWorksheet.GoToNextColumn()
            End Sub

            ''' <summary>
            ''' Moves the cursor the number of defined columns to the right
            ''' </summary>
            ''' <param name="numberOfColumns">Number of columns to move.</param>
            ''' <param name="keepRowPosition">If true, the row position is preserved, otherwise set to 0.</param>
            Public Sub Right(numberOfColumns As Integer, Optional keepRowPosition As Boolean = False)
                NullCheck()
                currentWorksheet.GoToNextColumn(numberOfColumns, keepRowPosition)
            End Sub

            ''' <summary>
            ''' Moves the cursor one column to the left
            ''' </summary>
            Public Sub Left()
                NullCheck()
                currentWorksheet.GoToNextColumn(-1)
            End Sub

            ''' <summary>
            ''' Moves the cursor the number of defined columns to the left
            ''' </summary>
            ''' <param name="numberOfColumns">Number of columns to move.</param>
            ''' <param name="keepRowRowPosition">If true, the row position is preserved, otherwise set to 0.</param>
            Public Sub Left(numberOfColumns As Integer, Optional keepRowRowPosition As Boolean = False)
                NullCheck()
                currentWorksheet.GoToNextColumn(-1 * numberOfColumns, keepRowRowPosition)
            End Sub

            ''' <summary>
            ''' Internal method to check whether the worksheet is null
            ''' </summary>
            Private Sub NullCheck()
                If currentWorksheet Is Nothing Then
                    Throw New WorksheetException("No worksheet was defined")
                End If
            End Sub
        End Class
    End Class
End Namespace
