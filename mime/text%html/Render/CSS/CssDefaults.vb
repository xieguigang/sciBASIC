#Region "Microsoft.VisualBasic::e65599c41fbe94bb3a3f46a34bf6dfbc, mime\text%html\Render\CSS\CssDefaults.vb"

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

    '   Total Lines: 55
    '    Code Lines: 14 (25.45%)
    ' Comment Lines: 30 (54.55%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (20.00%)
    '     File Size: 9.41 KB


    '     Class CssDefaults
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Render.CSS

    Public Class CssDefaults

        ''' <summary>
        ''' CSS Specification's Default Style Sheet for HTML 4
        ''' </summary>
        ''' <remarks>
        ''' http://www.w3.org/TR/CSS21/sample.html
        ''' </remarks>
        Public Const DefaultStyleSheet As String = vbCr & vbLf & vbCr & vbLf & "        " & vbCr & vbLf & "        html, address," & vbCr & vbLf & "        blockquote," & vbCr & vbLf & "        body, dd, div," & vbCr & vbLf & "        dl, dt, fieldset, form," & vbCr & vbLf & "        frame, frameset," & vbCr & vbLf & "        h1, h2, h3, h4," & vbCr & vbLf & "        h5, h6, noframes," & vbCr & vbLf & "        ol, p, ul, center," & vbCr & vbLf & "        dir, hr, menu, pre   { display: block }" & vbCr & vbLf & "        li              { display: list-item }" & vbCr & vbLf & "        head            { display: none }" & vbCr & vbLf & "        table           { display: table }" & vbCr & vbLf & "        tr              { display: table-row }" & vbCr & vbLf & "        thead           { display: table-header-group }" & vbCr & vbLf & "        tbody           { display: table-row-group }" & vbCr & vbLf & "        tfoot           { display: table-footer-group }" & vbCr & vbLf & "        col             { display: table-column }" & vbCr & vbLf & "        colgroup        { display: table-column-group }" & vbCr & vbLf & "        td, th          { display: table-cell }" & vbCr & vbLf & "        caption         { display: table-caption }" & vbCr & vbLf & "        th              { font-weight: bolder; text-align: center }" & vbCr & vbLf & "        caption         { text-align: center }" & vbCr & vbLf & "        body            { margin: 8px }" & vbCr & vbLf & "        h1              { font-size: 2em; margin: .67em 0 }" & vbCr & vbLf & "        h2              { font-size: 1.5em; margin: .75em 0 }" & vbCr & vbLf & "        h3              { font-size: 1.17em; margin: .83em 0 }" & vbCr & vbLf & "        h4, p," & vbCr & vbLf & "        blockquote, ul," & vbCr & vbLf & "        fieldset, form," & vbCr & vbLf & "        ol, dl, dir," & vbCr & vbLf & "        menu            { margin: 1.12em 0 }" & vbCr & vbLf & "        h5              { font-size: .83em; margin: 1.5em 0 }" & vbCr & vbLf & "        h6              { font-size: .75em; margin: 1.67em 0 }" & vbCr & vbLf & "        h1, h2, h3, h4," & vbCr & vbLf & "        h5, h6, b," & vbCr & vbLf & "        strong          { font-weight: bolder; }" & vbCr & vbLf & "        blockquote      { margin-left: 40px; margin-right: 40px }" & vbCr & vbLf & "        i, cite, em," & vbCr & vbLf & "        var, address    { font-style: italic }" & vbCr & vbLf & "        pre, tt, code," & vbCr & vbLf & "        kbd, samp       { font-family: monospace }" & vbCr & vbLf & "        pre             { white-space: pre }" & vbCr & vbLf & "        button, textarea," & vbCr & vbLf & "        input, select   { display: inline-block }" & vbCr & vbLf & "        big             { font-size: 1.17em }" & vbCr & vbLf & "        small, sub, sup { font-size: .83em }" & vbCr & vbLf & "        sub             { vertical-align: sub }" & vbCr & vbLf & "        sup             { vertical-align: super }" & vbCr & vbLf & "        table           { border-spacing: 2px; }" & vbCr & vbLf & "        thead, tbody," & vbCr & vbLf & "        tfoot           { vertical-align: middle }" & vbCr & vbLf & "        td, th          { vertical-align: inherit }" & vbCr & vbLf & "        s, strike, del  { text-decoration: line-through }" & vbCr & vbLf & "        hr              { border: 1px inset }" & vbCr & vbLf & "        ol, ul, dir," & vbCr & vbLf & "        menu, dd        { margin-left: 40px }" & vbCr & vbLf & "        ol              { list-style-type: decimal }" & vbCr & vbLf & "        ol ul, ul ol," & vbCr & vbLf & "        ul ul, ol ol    { margin-top: 0; margin-bottom: 0 }" & vbCr & vbLf & "        u, ins          { text-decoration: underline }" & vbCr & vbLf & "        br:before       { content: ""\A"" }" & vbCr & vbLf & "        :before, :after { white-space: pre-line }" & vbCr & vbLf & "        center          { text-align: center }" & vbCr & vbLf & "        :link, :visited { text-decoration: underline }" & vbCr & vbLf & "        :focus          { outline: thin dotted invert }" & vbCr & vbLf & vbCr & vbLf & "        /* Begin bidirectionality settings (do not change) */" & vbCr & vbLf & "        BDO[DIR=""ltr""]  { direction: ltr; unicode-bidi: bidi-override }" & vbCr & vbLf & "        BDO[DIR=""rtl""]  { direction: rtl; unicode-bidi: bidi-override }" & vbCr & vbLf & vbCr & vbLf & "        *[DIR=""ltr""]    { direction: ltr; unicode-bidi: embed }" & vbCr & vbLf & "        *[DIR=""rtl""]    { direction: rtl; unicode-bidi: embed }" & vbCr & vbLf & vbCr & vbLf & "        @media print {" & vbCr & vbLf & "          h1            { page-break-before: always }" & vbCr & vbLf & "          h1, h2, h3," & vbCr & vbLf & "          h4, h5, h6    { page-break-after: avoid }" & vbCr & vbLf & "          ul, ol, dl    { page-break-before: avoid }" & vbCr & vbLf & "        }" & vbCr & vbLf & vbCr & vbLf & "        /* Not in the specification but necessary */" & vbCr & vbLf & "        a               { color:blue; text-decoration:underline }" & vbCr & vbLf & "        table           { border-color:#dfdfdf; border-style:outset; }" & vbCr & vbLf & "        td, th          { border-color:#dfdfdf; border-style:inset; }" & vbCr & vbLf & "        style, title," & vbCr & vbLf & "        script, link," & vbCr & vbLf & "        meta, area," & vbCr & vbLf & "        base, param     { display:none }" & vbCr & vbLf & "        hr              { border-color: #ccc }  " & vbCr & vbLf & "        pre             { font-size:10pt }" & vbCr & vbLf & "        " & vbCr & vbLf & "        /*This is the background of the HtmlToolTip*/" & vbCr & vbLf & "        .htmltooltipbackground {" & vbCr & vbLf & "              border:solid 1px #767676;" & vbCr & vbLf & "              corner-radius:3px;" & vbCr & vbLf & "              background-color:#white;" & vbCr & vbLf & "              background-gradient:#E4E5F0;" & vbCr & vbLf & "        }" & vbCr & vbLf & vbCr & vbLf & "        "

        ''' <summary>
        ''' Html Fragment used to draw the icon that shows an error on an IMG HTML element
        ''' </summary>
        Public Const ErrorOnImageIcon As String = vbCr & vbLf & "        <style>" & vbCr & vbLf & "          table { " & vbCr & vbLf & vbCr & vbLf & "               border-bottom:1px solid #bbb;" & vbCr & vbLf & "               border-right:1px solid #bbb;" & vbCr & vbLf & "               border-spacing:0;" & vbCr & vbLf & "          }" & vbCr & vbLf & "          td { " & vbCr & vbLf & "               border:1px solid #555;" & vbCr & vbLf & "               font:bold 9pt Arial;" & vbCr & vbLf & "               padding:3px;" & vbCr & vbLf & "               color:red;" & vbCr & vbLf & "               background-color:#fbfbfb;" & vbCr & vbLf & "           }" & vbCr & vbLf & "        </style>" & vbCr & vbLf & "        <table>" & vbCr & vbLf & "        <tr>" & vbCr & vbLf & "        <td>X</td>" & vbCr & vbLf & "        </tr>" & vbCr & vbLf & "        </table>"

        ''' <summary>
        ''' Html Fragment used to draw the icon that shows an error on an OBJECT HTML element
        ''' </summary>
        Public Const ErrorOnObjectIcon As String = vbCr & vbLf & "        <style>" & vbCr & vbLf & "          table { " & vbCr & vbLf & vbCr & vbLf & "               border-bottom:1px solid #bbb;" & vbCr & vbLf & "               border-right:1px solid #bbb;" & vbCr & vbLf & "               border-spacing:0;" & vbCr & vbLf & "          }" & vbCr & vbLf & "          td { " & vbCr & vbLf & "               border:1px solid #555;" & vbCr & vbLf & "               font:bold 7pt Arial;" & vbCr & vbLf & "               padding:3px;" & vbCr & vbLf & "               color:red;" & vbCr & vbLf & "               background-color:#fbfbfb;" & vbCr & vbLf & "           }" & vbCr & vbLf & "        </style>" & vbCr & vbLf & "        <table>" & vbCr & vbLf & "        <tr>" & vbCr & vbLf & "        <td>X</td>" & vbCr & vbLf & "        </tr>" & vbCr & vbLf & "        </table>"

        ''' <summary>
        ''' Default font size in points. Change this value to modify the default font size.
        ''' </summary>
        Public Shared FontSize As Single = 12.0F

        ''' <summary>
        ''' Default font used for the generic 'serif' family
        ''' </summary>
        Public Shared FontSerif As String = FontFamily.GenericSerif.Name

        ''' <summary>
        ''' Default font used for the generic 'sans-serif' family
        ''' </summary>
        Public Shared FontSansSerif As String = FontFamily.GenericSansSerif.Name

        ''' <summary>
        ''' Default font used for the generic 'cursive' family
        ''' </summary>
        Public Shared FontCursive As String = "Monotype Corsiva"

        ''' <summary>
        ''' Default font used for the generic 'fantasy' family
        ''' </summary>
        Public Shared FontFantasy As String = "Comic Sans MS"

        ''' <summary>
        ''' Default font used for the generic 'monospace' family
        ''' </summary>
        Public Shared FontMonospace As String = FontFamily.GenericMonospace.Name
    End Class
End Namespace
