#Region "Microsoft.VisualBasic::d445cef547ce840494d44dac58c9d12e, Microsoft.VisualBasic.Core\src\Extensions\WebServices\WebResult.vb"

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

    '   Total Lines: 45
    '    Code Lines: 17
    ' Comment Lines: 23
    '   Blank Lines: 5
    '     File Size: 1.38 KB


    '     Class WebResult
    ' 
    '         Properties: BriefText, Site, Title, Update, URL
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions

Namespace Net.Http

    ''' <summary>
    ''' The data structure for represents the search result of the Web search egine.
    ''' </summary>
    Public Class WebResult

        ''' <summary>
        ''' Specifies the Title element of the result string. 
        ''' </summary>
        ''' <returns></returns>
        Public Property Title As String
        ''' <summary>
        ''' In short description of the link produced. 
        ''' </summary>
        ''' <returns></returns>
        Public Property BriefText As String
        ''' <summary>
        ''' Url that points to the Current result.
        ''' </summary>
        ''' <returns></returns>
        Public Property URL As String
        ''' <summary>
        ''' Update time.
        ''' </summary>
        ''' <returns></returns>
        Public Property Update As String

        ''' <summary>
        ''' Returns the root domain name.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Site As String
            Get
                Return Regex.Match(URL, "https?://(www.)?[^/]+").Value
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("{0}  [{1}]", Title, URL)
        End Function
    End Class
End Namespace
