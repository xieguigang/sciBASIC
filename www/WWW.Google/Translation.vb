#Region "Microsoft.VisualBasic::4272319931ec20b6367f675bf14a26ca, sciBASIC#\www\WWW.Google\Translation.vb"

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

    '   Total Lines: 92
    '    Code Lines: 54
    ' Comment Lines: 23
    '   Blank Lines: 15
    '     File Size: 4.19 KB


    ' Class Translation
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: __seperator, __translation, ToString, Translate
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Windows.Forms
Imports System.Text

''' <summary>
''' Using Google Translation without Developer Account
''' invalid media, 10 Dec 2014
''' http://www.codeproject.com/Tips/851790/Using-Google-Translation-without-Developer-Account?msg=4960428#xx4960428xx
''' </summary>
''' <remarks></remarks>
Public Class Translation

    Dim wb As New WebBrowser With {
        .ScriptErrorsSuppressed = True
    }
    Dim _src_language, _sbj_language As String

    Const GOOGLE_TRANSLATION_URL As String = "https://translate.google.com/#{0}/{1}/{2}"
    Const TEXTBOX_SEPERATORS_TAG As String = "<SPAN id=result_box lang={0} class=short_text"

    Private Function __seperator() As String
        Return String.Format(TEXTBOX_SEPERATORS_TAG, _sbj_language)
    End Function

    Sub New(lang_src As String, lang_sbj As String)
        _src_language = lang_src
        _sbj_language = lang_sbj
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("{0} <==> {1}", _src_language, _sbj_language)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="mystring"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' '**********************************************************************************************
    ''' '* This program extract the result of English to Arabic word translated in google, 
    ''' '* no need to get AppID '* or license just to translate. 
    ''' '* You can change the language according to you needs.
    ''' '* Be sure you are connected to internet when running the program                                       
    ''' '* NOTE : This is my interpretation of conversion. 
    ''' '* This is subject to change because of google rendering'* implementation of html       
    ''' '* Copyright 2014 - Elmer Garcia                                                                        
    ''' '* Email me : elmerrgarcia@gmail.com
    ''' '************************************************************************************************
    ''' </remarks>
    Public Function Translate(MyString As String) As String
        Call wb.Navigate(String.Format(GOOGLE_TRANSLATION_URL, _src_language, _sbj_language, Strings.Replace(MyString, " ", "%20").ToLower))
        Return __translation()
    End Function

    Private Function __translation() As String
        While ((wb.IsBusy) Or (wb.ReadyState <> WebBrowserReadyState.Complete))
            Call Threading.Thread.Sleep(1) ' wait for the browser to fully load
        End While

        Dim htm As HtmlDocument
        Dim rawString As String = ""
        Dim finalstring As StringBuilder = New StringBuilder(2048)
        Dim st As String = ""
        Dim tag As String = __seperator()

        If wb.ReadyState = WebBrowserReadyState.Complete Then  ' decode the string returned
            htm = wb.Document

            If InStr(htm.Window.Document.Body.InnerHtml, tag) > 0 Then
                rawString = Mid(htm.Window.Document.Body.InnerHtml, InStr(htm.Window.Document.Body.InnerHtml, tag))   'extract to find the word starting the translation part
                st = Strings.Replace(rawString, tag, "")  'remove the string "<SPAN id=result_box lang=ar class=short_text" 
                st = Mid(st, InStr(st, ">") + 1, st.Length)       'find the first ">" to remove it
                st = Mid(st, 1, InStr(st, "</DIV>") - 1)     'remove the excess html on the variable

                For Each str As String In Strings.Split(st, "</SPAN>")   'split the final string with </span> and recombine the words to form the finalstring
                    Dim s = Mid(str, InStr(str, ">") + 1)
                    Call finalstring.Append(" " & s)
                Next

                st = finalstring.ToString.Trim

                If InStr(st, "...") = 0 Then 'check if still translating
                    Return st    'finalstring the translation
                Else
                    Return __translation() 'retry the translation
                End If
            End If
        End If

        Return ""
    End Function
End Class
