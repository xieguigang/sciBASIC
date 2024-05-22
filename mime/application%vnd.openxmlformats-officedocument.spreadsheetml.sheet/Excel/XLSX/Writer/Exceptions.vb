#Region "Microsoft.VisualBasic::729edc33db21e6350ca81bb4def9691a, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Exceptions.vb"

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

    '   Total Lines: 140
    '    Code Lines: 58 (41.43%)
    ' Comment Lines: 69 (49.29%)
    '    - Xml Docs: 91.30%
    ' 
    '   Blank Lines: 13 (9.29%)
    '     File Size: 4.84 KB


    '     Class RangeException
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class FormatException
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '     Class IOException
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '     Class WorksheetException
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class StyleException
    ' 
    '         Constructor: (+2 Overloads) Sub New
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

Namespace XLSX.Writer

    ''' <summary>
    ''' Class for exceptions regarding range incidents (e.g. out-of-range)
    ''' </summary>
    <Serializable>
    Public Class RangeException
        Inherits Exception
        ''' <summary>
        ''' Initializes a new instance of the <see cref="RangeException"/> class
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RangeException"/> class
        ''' </summary>
        ''' <param name="title">The title<see cref="String"/>.</param>
        ''' <param name="message">Message of the exception.</param>
        Public Sub New(title As String, message As String)
            MyBase.New(message)
        End Sub
    End Class

    ''' <summary>
    ''' Class for exceptions regarding format error incidents
    ''' </summary>
    <Serializable>
    Public Class FormatException
        Inherits Exception
        ''' <summary>
        ''' Initializes a new instance of the <see cref="FormatException"/> class
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="FormatException"/> class
        ''' </summary>
        ''' <param name="message">Message of the exception.</param>
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="FormatException"/> class
        ''' </summary>
        ''' <param name="title">Title of the exception.</param>
        ''' <param name="message">Message of the exception.</param>
        ''' <param name="inner">Inner exception.</param>
        Public Sub New(title As String, message As String, inner As Exception)
            MyBase.New(message, inner)
        End Sub
    End Class

    ''' <summary>
    ''' Class for exceptions regarding stream or save error incidents
    ''' </summary>
    <Serializable>
    Public Class IOException
        Inherits Exception
        ''' <summary>
        ''' Initializes a new instance of the <see cref="IOException"/> class
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="IOException"/> class
        ''' </summary>
        ''' <param name="message">Message of the exception.</param>
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="IOException"/> class
        ''' </summary>
        ''' <param name="message">Message of the exception.</param>
        ''' <param name="inner">Inner exception.</param>
        Public Sub New(message As String, inner As Exception)
            MyBase.New(message, inner)
        End Sub
    End Class

    ''' <summary>
    ''' Class for exceptions regarding worksheet incidents
    ''' </summary>
    <Serializable>
    Public Class WorksheetException
        Inherits Exception
        ''' <summary>
        ''' Initializes a new instance of the <see cref="WorksheetException"/> class
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="WorksheetException"/> class
        ''' </summary>
        ''' <param name="message">Message of the exception.</param>
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class

    ''' <summary>
    ''' Class for exceptions regarding Style incidents
    ''' </summary>
    <Serializable>
    Public Class StyleException
        Inherits Exception
        ''' <summary>
        ''' Initializes a new instance of the <see cref="StyleException"/> class
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="StyleException"/> class
        ''' </summary>
        ''' <param name="title">The title<see cref="String"/>.</param>
        ''' <param name="message">Message of the exception.</param>
        Public Sub New(title As String, message As String)
            MyBase.New(message)
        End Sub
    End Class
End Namespace
