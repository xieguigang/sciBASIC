#Region "Microsoft.VisualBasic::0bc25f489efc3475020b9259c5ab993d, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Metadata.vb"

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

    '   Total Lines: 153
    '    Code Lines: 72 (47.06%)
    ' Comment Lines: 63 (41.18%)
    '    - Xml Docs: 90.48%
    ' 
    '   Blank Lines: 18 (11.76%)
    '     File Size: 6.77 KB


    '     Class Metadata
    ' 
    '         Properties: Application, ApplicationVersion, Category, Company, ContentStatus
    '                     Creator, Description, HyperlinkBase, Keywords, Manager
    '                     Subject, Title
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ParseVersion
    ' 
    '         Sub: CheckVersion
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

Imports System.Globalization
Imports System.Reflection

Namespace XLSX.Writer

    ''' <summary>
    ''' Class representing the meta data of a workbook
    ''' </summary>
    Public Class Metadata

        ''' <summary>
        ''' Defines the applicationVersion
        ''' </summary>
        Private m_applicationVersion As String

        ''' <summary>
        ''' Gets or sets the application which created the workbook. Default is PicoXLSX
        ''' </summary>
        Public Property Application As String

        ''' <summary>
        ''' Gets or sets the version of the creation application. Default is the library version of PicoXLSX. Use the format xxxxx.yyyyy (e.g. 1.0 or 55.9875) in case of a custom value.
        ''' </summary>
        Public Property ApplicationVersion As String
            Get
                Return m_applicationVersion
            End Get
            Set(value As String)
                m_applicationVersion = value
                CheckVersion()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the category of the document. There are no predefined values or restrictions about the content of this field
        ''' </summary>
        Public Property Category As String

        ''' <summary>
        ''' Gets or sets the company owning the document. This value is for organizational purpose. Add more than one manager by using the semicolon (;) between the words
        ''' </summary>
        Public Property Company As String

        ''' <summary>
        ''' Gets or sets the status of the document. There are no predefined values or restrictions about the content of this field
        ''' </summary>
        Public Property ContentStatus As String

        ''' <summary>
        ''' Gets or sets the creator of the workbook. Add more than one creator by using the semicolon (;) between the authors
        ''' </summary>
        Public Property Creator As String

        ''' <summary>
        ''' Gets or sets the description of the document or comment about it
        ''' </summary>
        Public Property Description As String

        ''' <summary>
        ''' Gets or sets the hyper-link base of the document.
        ''' </summary>
        Public Property HyperlinkBase As String

        ''' <summary>
        ''' Gets or sets the keywords of the workbook. Separate particular keywords with semicolons (;)
        ''' </summary>
        Public Property Keywords As String

        ''' <summary>
        ''' Gets or sets the responsible manager of the document. This value is for organizational purpose.
        ''' </summary>
        Public Property Manager As String

        ''' <summary>
        ''' Gets or sets the subject of the workbook
        ''' </summary>
        Public Property Subject As String

        ''' <summary>
        ''' Gets or sets the title of the workbook
        ''' </summary>
        Public Property Title As String

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Metadata"/> class
        ''' </summary>
        Public Sub New()
            Dim vi As Version = Assembly.GetExecutingAssembly().GetName().Version

            Application = "PicoXLSX(https://github.com/rabanti-github/PicoXLSX); scibasic.net(https://github.com/xieguigang/sciBASIC)"
            ApplicationVersion = ParseVersion(vi.Major, vi.Minor, vi.Revision, vi.Build)
        End Sub

        ''' <summary>
        ''' Checks the format of the passed version string. Allowed values are null, empty and fractions from 0.0  to 99999.99999 (max. number of digits before and after the period is 5)
        ''' </summary>
        Private Sub CheckVersion()
            If String.IsNullOrEmpty(m_applicationVersion) Then
                Return
            End If

            Dim split = m_applicationVersion.Split("."c)
            Dim state = True

            If split.Length <> 2 Then
                state = False
            Else
                If split(1).Length < 1 OrElse split(1).Length > 5 Then
                    state = False
                End If
                If split(0).Length < 1 OrElse split(0).Length > 5 Then
                    state = False
                End If
            End If
            If Not state Then
                Throw New FormatException("The format of the version in the meta data is wrong (" & m_applicationVersion & "). Should be in the format and a range from '0.0' to '99999.99999'")
            End If
        End Sub

        ''' <summary>
        ''' Method to parse a common version (major.minor.revision.build) into the compatible format (major.minor). 
        ''' The minimum value is 0.0 and the maximum value is 99999.99999<br></br>
        ''' The minor, revision and build number are joined if possible. If the number is too long, the additional 
        ''' characters will be removed from the right side down to five characters (e.g. 785563 will be 78556)
        ''' </summary>
        ''' <param name="major">Major number from 0 to 99999.</param>
        ''' <param name="minor">Minor number.</param>
        ''' <param name="build">Build number.</param>
        ''' <param name="revision">Revision number.</param>
        ''' <returns>Formatted version number (e.g. 1.0 or 55.987).</returns>
        Public Shared Function ParseVersion(major As Integer, minor As Integer, build As Integer, revision As Integer) As String
            If major < 0 OrElse minor < 0 OrElse build < 0 OrElse revision < 0 Then
                Throw New FormatException("The format of the passed version is wrong. No negative number allowed.")
            End If
            If major > 99999 Then
                Throw New FormatException("The major number may not be bigger than 99999. The passed value is " & major.ToString())
            End If
            Dim culture = CultureInfo.InvariantCulture
            Dim leftPart = major.ToString("G", culture)
            Dim rightPart = minor.ToString("G", culture) & build.ToString("G", culture) & revision.ToString("G", culture)
            rightPart = rightPart.TrimEnd("0"c)
            If Equals(rightPart, "") Then
                rightPart = "0"
            Else
                If rightPart.Length > 5 Then
                    rightPart = rightPart.Substring(0, 5)
                End If
            End If
            Return leftPart & "." & rightPart
        End Function
    End Class
End Namespace
