#Region "Microsoft.VisualBasic::37f62d8f6babc3846e1249794087a938, mime\application%pdf\PdfFileWriter\ObjectCode.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14
    ' Comment Lines: 4
    '   Blank Lines: 1
    '     File Size: 286 B


    ' Enum ResCode
    ' 
    '     ExtGState, Font, Length, OpContent, Pattern
    '     Shading, XObject
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum ObjectType
    ' 
    '     Dictionary, Other, Stream
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Resource code enumeration
''' </summary>
Friend Enum ResCode
    ' must be in this order
    Font
    Pattern
    Shading
    XObject
    ExtGState
    OpContent
    Length
End Enum

Friend Enum ObjectType
    Other
    Dictionary
    Stream
End Enum
