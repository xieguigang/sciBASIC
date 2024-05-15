#Region "Microsoft.VisualBasic::782f465cba6b0112d4ed3e2d49902120, mime\application%pdf\PdfReader\Parser\ParseObjectReference.vb"

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

    '   Total Lines: 30
    '    Code Lines: 26
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 766 B


    '     Class ParseObjectReference
    ' 
    '         Properties: Gen, Id
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace PdfReader
    Public Class ParseObjectReference
        Inherits ParseObjectBase

        Private _Id As Integer, _Gen As Integer

        Public Sub New(id As TokenInteger, gen As TokenInteger)
            Me.Id = id.Value
            Me.Gen = gen.Value
        End Sub

        Public Property Id As Integer
            Get
                Return _Id
            End Get
            Private Set(value As Integer)
                _Id = value
            End Set
        End Property

        Public Property Gen As Integer
            Get
                Return _Gen
            End Get
            Private Set(value As Integer)
                _Gen = value
            End Set
        End Property
    End Class
End Namespace
