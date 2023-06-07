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
