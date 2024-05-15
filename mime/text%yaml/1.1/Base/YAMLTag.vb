#Region "Microsoft.VisualBasic::a8f583e2966f8d1d501e5664ae0b34c6, mime\text%yaml\1.1\Base\YAMLTag.vb"

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
    '    Code Lines: 23
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 859 B


    '     Structure YAMLTag
    ' 
    '         Properties: Content, Handle, IsEmpty
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToHeaderString, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics.Contracts

Namespace Grammar11

    Public Structure YAMLTag

        Public Sub New(handle As String, content As String)
            Me.Handle = handle
            Me.Content = content
        End Sub

        Public Overrides Function ToString() As String
            Return If(IsEmpty, String.Empty, $"{Handle}{Content}")
        End Function

        <Pure>
        Public Function ToHeaderString() As String
            Return If(IsEmpty, String.Empty, $"{Handle} {Content}")
        End Function

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Return String.IsNullOrEmpty(Handle)
            End Get
        End Property

        Public ReadOnly Property Handle() As String
        Public ReadOnly Property Content() As String
    End Structure
End Namespace
