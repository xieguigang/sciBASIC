#Region "Microsoft.VisualBasic::453cb0fdd8209f09065dce6e76bff3be, mime\text%html\CSS\CssUnit.vb"

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

'   Total Lines: 28
'    Code Lines: 13 (46.43%)
' Comment Lines: 6 (21.43%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 9 (32.14%)
'     File Size: 426 B


'     Enum CssUnit
' 
'         Centimeters, Ems, Ex, Inches, Milimeters
'         None, Picas, Pixels, Points
' 
'  
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace CSS

    ''' <summary>
    ''' Represents the possible units of the CSS lengths
    ''' </summary>
    ''' <remarks>
    ''' http://www.w3.org/TR/CSS21/syndata.html#length-units
    ''' </remarks>
    Public Enum CssUnit

        ''' <summary>
        ''' Unknown
        ''' </summary>
        <Description("")> None
        ''' <summary>
        ''' em: the 'font-size' of the relevant font
        ''' </summary>
        <Description("em")> Ems
        ''' <summary>
        ''' ex: the 'x-height' of the relevant font
        ''' </summary>
        <Description("ex")> Ex

        ''' <summary>
        ''' px: pixel units — 1px is equal to 0.75pt.
        ''' </summary>
        <Description("px")> Pixels

        ''' <summary>
        ''' in: inches — 1in is equal to 2.54cm.
        ''' </summary>
        <Description("in")> Inches

        ''' <summary>
        ''' cm: centimeters
        ''' </summary>
        <Description("cm")> Centimeters

        ''' <summary>
        ''' mm: millimeters
        ''' </summary>
        <Description("mm")> Milimeters
        ''' <summary>
        ''' pt: points — the points used by CSS are equal to 1/72nd of 1in.
        ''' </summary>
        <Description("pt")> Points

        ''' <summary>
        ''' pc: picas — 1pc is equal to 12pt.
        ''' </summary>
        <Description("pc")> Picas

        ''' <summary>
        ''' The format of a percentage value (denoted by &lt;percentage> in this specification) is a &lt;number> immediately followed by '%'.
        ''' 
        ''' Percentage values are always relative to another value, for example a length. Each property 
        ''' that allows percentages also defines the value to which the percentage refers. The value may 
        ''' be that of another property for the same element, a property for an ancestor element, or a 
        ''' value of the formatting context (e.g., the width of a containing block). When a percentage 
        ''' value is set for a property of the root element and the percentage is defined as referring 
        ''' to the inherited value of some property, the resultant value is the percentage times the 
        ''' initial value of that property.
        ''' 
        ''' Since child elements (generally) inherit the computed values of their parent, in the following 
        ''' example, the children of the P element will inherit a value of 12px for 'line-height', not 
        ''' the percentage value (120%):
        ''' 
        ''' ```css
        ''' p { font-size: 10px }
        ''' p { line-height: 120% }  /* 120% of 'font-size' */
        ''' ```
        ''' </summary>
        <Description("%")> Percent

    End Enum
End Namespace
