#Region "Microsoft.VisualBasic::3a5b25aff8add8fbfbc0bf4d3d3f2b4a, mime\application%pdf\PdfFileWriter\LocationMarker.vb"

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

    '   Total Lines: 181
    '    Code Lines: 49 (27.07%)
    ' Comment Lines: 114 (62.98%)
    '    - Xml Docs: 78.07%
    ' 
    '   Blank Lines: 18 (9.94%)
    '     File Size: 7.59 KB


    ' Enum LocMarkerScope
    ' 
    '     LocalDest, NamedDest
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum DestFit
    ' 
    '     Fit, FitB, FitBH, FitBV, FitH
    '     FitR, FitV
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class LocationMarker
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: CompareTo
    ' 
    '     Sub: Create
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	Location marker
'	Internal class for managing document's location markers. 
'
'	Uzi Granot
'	Version: 1.0
'	Date: April 1, 2013
'	Copyright (C) 2013-2019 Uzi Granot. All Rights Reserved
'
'	PdfFileWriter C# class library and TestPdfFileWriter test/demo
'  application are free software.
'	They is distributed under the Code Project Open License (CPOL).
'	The document PdfFileWriterReadmeAndLicense.pdf contained within
'	the distribution specify the license agreement and other
'	conditions and notes. You must read this document and agree
'	with the conditions specified in order to use this software.
'
'	For version history please refer to PdfDocument.cs
'
'

Imports System.Text

''' <summary>
''' Location marker scope
''' </summary>
Public Enum LocMarkerScope
    ''' <summary>
    ''' Location marker is local
    ''' </summary>
    LocalDest

    ''' <summary>
    ''' Location marker is global and local
    ''' </summary>
    NamedDest
End Enum

''' <summary>
''' Named destination fit constants
''' </summary>
Public Enum DestFit
    ''' <summary>
    ''' Fit entire page. No arguments.
    ''' </summary>
    ''' <remarks>
    ''' Display the page designated by page, with its contents magnified
    ''' just enough to fit the entire page within the window both horizontally
    ''' and vertically. If the required horizontal and vertical magnification
    ''' factors are different, use the smaller of the two, centering the page
    ''' within the window in the other dimension.
    ''' </remarks>
    Fit

    ''' <summary>
    ''' Display the page with top coordinate at top of the window. One argument.
    ''' </summary>
    ''' <remarks>
    ''' Display the page designated by page, with the vertical coordinate top
    ''' positioned at the top edge of the window and the contents of the page
    ''' magnified just enough to fit the entire width of the page within the
    ''' window. A null value for top specifies that the current value of that
    ''' parameter is to be retained unchanged.
    ''' </remarks>
    FitH

    ''' <summary>
    ''' Display the page with left coordinate at left side of the window. One argument.
    ''' </summary>
    ''' <remarks>
    ''' Display the page designated by page, with the horizontal coordinate left
    ''' positioned at the left edge of the window and the contents of the page
    ''' magnified just enough to fit the entire height of the page within the
    ''' window. A null value for left specifies that the current value of that
    ''' parameter is to be retained unchanged.
    ''' </remarks>
    FitV

    ''' <summary>
    ''' Display the page within rectangle. Four argument.
    ''' </summary>
    ''' <remarks>
    ''' Display the page designated by page, with its contents magnified just enough
    ''' to fit the rectangle specified by the coordinates left, bottom, right, and
    ''' topentirely within the window both horizontally and vertically. If the required
    ''' horizontal and vertical magnification factors are different, use the smaller
    ''' of the two, centering the rectangle within the window in the other dimension.
    ''' A null value for any of the parameters may result in unpredictable behavior.
    ''' </remarks>
    FitR

    ''' <summary>
    ''' Fit entire page including boundig box. No arguments.
    ''' </summary>
    ''' <remarks>
    ''' Display the page designated by page, with its contents magnified just enough
    ''' to fit its bounding box entirely within the window both horizontally and
    ''' vertically. If the required horizontal and vertical magnification factors are
    ''' different, use the smaller of the two, centering the bounding box within the
    ''' window in the other dimension.
    ''' </remarks>
    FitB

    ''' <summary>
    ''' Display the page with top coordinate at top of the window. One argument.
    ''' </summary>
    ''' <remarks>
    ''' Display the page designated by page, with the vertical coordinate top positioned
    ''' at the top edge of the window and the contents of the page magnified just enough
    ''' to fit the entire width of its bounding box within the window. A null value for
    ''' top specifies that the current value of that parameter is to be retained unchanged.
    ''' </remarks>
    FitBH

    ''' <summary>
    ''' Display the page with left coordinate at left side of the window. One argument.
    ''' </summary>
    ''' <remarks>
    ''' Display the page designated by page, with the horizontal coordinate left positioned
    ''' at the left edge of the window and the contents of the page magnified just enough
    ''' to fit the entire height of its bounding box within the window. A null value for
    ''' left specifies that the current value of that parameter is to be retained unchanged.
    ''' </remarks>
    FitBV
End Enum

Friend Class LocationMarker
    Implements IComparable(Of LocationMarker)

    Friend Shared FitString As String() = {"/Fit", "/FitH", "/FitV", "/FitR", "/FitB", "/FitBH", "/FitBV"}
    Friend Shared FitArguments As Integer() = {0, 1, 1, 4, 0, 1, 1}
    Friend LocMarkerName As String
    Friend Scope As LocMarkerScope
    Friend DestStr As String

    ' Do not call this constructor from your code
    Private Sub New(LocMarkerName As String, LoctionMarkerPage As PdfPage, Scope As LocMarkerScope, FitArg As DestFit, ParamArray SideArg As Double())
        If SideArg.Length <> FitArguments(FitArg) Then Throw New ApplicationException("AddDestination invalid number of arguments")
        Me.LocMarkerName = LocMarkerName
        Me.Scope = Scope
        Dim BuildDest As StringBuilder = New StringBuilder()
        BuildDest.AppendFormat("[{0} 0 R {1}", LoctionMarkerPage.ObjectNumber, FitString(FitArg))

        For Each Side In SideArg
            BuildDest.AppendFormat(PeriodDecSep, " {0}", LoctionMarkerPage.ToPt(Side))
        Next

        BuildDest.Append("]")
        DestStr = BuildDest.ToString()
        Return
    End Sub

    Friend Sub New(LocMarkerName As String)
        Me.LocMarkerName = LocMarkerName
        Return
    End Sub

    ''' <summary>
    ''' Create unique location marker
    ''' </summary>
    ''' <param name="LocMarkerName">Location marker name (case sensitive)</param>
    ''' <param name="LocMarkerPage">Location marker page</param>
    ''' <param name="Scope">Location marker scope</param>
    ''' <param name="FitArg">Fit enumeration</param>
    ''' <param name="SideArg">Fit optional arguments</param>
    Public Shared Sub Create(LocMarkerName As String, LocMarkerPage As PdfPage, Scope As LocMarkerScope, FitArg As DestFit, ParamArray SideArg As Double())
        If LocMarkerPage.Document.LocMarkerArray Is Nothing Then LocMarkerPage.Document.LocMarkerArray = New List(Of LocationMarker)()
        Dim Index As Integer = LocMarkerPage.Document.LocMarkerArray.BinarySearch(New LocationMarker(LocMarkerName))
        If Index >= 0 Then Throw New ApplicationException("Duplicate location marker")
        LocMarkerPage.Document.LocMarkerArray.Insert(Not Index, New LocationMarker(LocMarkerName, LocMarkerPage, Scope, FitArg, SideArg))
        Return
    End Sub

    Public Function CompareTo(Other As LocationMarker) As Integer Implements IComparable(Of LocationMarker).CompareTo
        Return String.CompareOrdinal(LocMarkerName, Other.LocMarkerName)
    End Function
End Class
