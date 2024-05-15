#Region "Microsoft.VisualBasic::1eb19152f3cf43fbd2f60d995542335e, mime\application%pdf\PdfFileWriter\TextBox\TextBoxLine.vb"

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

    '   Total Lines: 79
    '    Code Lines: 46
    ' Comment Lines: 25
    '   Blank Lines: 8
    '     File Size: 2.10 KB


    ' Class TextBoxLine
    ' 
    '     Properties: Ascent, Descent, EndOfParagraph, LineHeight, SegArray
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' TextBoxLine class
''' </summary>
Public Class TextBoxLine


    ''' <summary>
    ''' Gets array of line segments.
    ''' </summary>
    Private _Ascent As Double, _Descent As Double, _EndOfParagraph As Boolean, _SegArray As TextBoxSeg()

    ''' <summary>
    ''' Gets line ascent.
    ''' </summary>
    Public Property Ascent As Double
        Get
            Return _Ascent
        End Get
        Friend Set(value As Double)
            _Ascent = value
        End Set
    End Property

    ''' <summary>
    ''' Gets line descent.
    ''' </summary>
    Public Property Descent As Double
        Get
            Return _Descent
        End Get
        Friend Set(value As Double)
            _Descent = value
        End Set
    End Property

    ''' <summary>
    ''' Line is end of paragraph.
    ''' </summary>
    Public Property EndOfParagraph As Boolean
        Get
            Return _EndOfParagraph
        End Get
        Friend Set(value As Boolean)
            _EndOfParagraph = value
        End Set
    End Property

    Public Property SegArray As TextBoxSeg()
        Get
            Return _SegArray
        End Get
        Friend Set(value As TextBoxSeg())
            _SegArray = value
        End Set
    End Property

    ''' <summary>
    ''' Gets line height.
    ''' </summary>
    Public ReadOnly Property LineHeight As Double
        Get
            Return Ascent + Descent
        End Get
    End Property

    ''' <summary>
    ''' TextBoxLine constructor.
    ''' </summary>
    ''' <param name="Ascent">Line ascent.</param>
    ''' <param name="Descent">Line descent.</param>
    ''' <param name="EndOfParagraph">Line is end of paragraph.</param>
    ''' <param name="SegArray">Segments' array.</param>
    Public Sub New(Ascent As Double, Descent As Double, EndOfParagraph As Boolean, SegArray As TextBoxSeg())
        Me.Ascent = Ascent
        Me.Descent = Descent
        Me.EndOfParagraph = EndOfParagraph
        Me.SegArray = SegArray
    End Sub
End Class
