#Region "Microsoft.VisualBasic::f6b0e7311bbd742e92731c1f0e00928c, Microsoft.VisualBasic.Core\src\Serialization\BEncoding\BencodingException.vb"

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

    '   Total Lines: 33
    '    Code Lines: 13 (39.39%)
    ' Comment Lines: 15 (45.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (15.15%)
    '     File Size: 927 B


    '     Class BencodingException
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A bencoding exception.
    ''' </summary>
    Public Class BencodingException
        Inherits FormatException

        ''' <summary>
        ''' Creates a new BencodingException.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates a new BencodingException.
        ''' </summary>
        ''' <param name="message">The message.</param>
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        ''' <summary>
        ''' Creates a new BencodingException.
        ''' </summary>
        ''' <param name="message">The message.</param>
        ''' <param name="inner">The inner exception.</param>
        Public Sub New(message As String, inner As Exception)
            MyBase.New(message, inner)
        End Sub
    End Class

End Namespace
