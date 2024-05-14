#Region "Microsoft.VisualBasic::f3dbc12b77997e43463d58f22c4c4aea, mime\text%yaml\1.1\Base\YAMLNode.vb"

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

    '   Total Lines: 40
    '    Code Lines: 33
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.28 KB


    '     Class YAMLNode
    ' 
    '         Properties: Anchor, CustomTag, Tag
    ' 
    '         Sub: Emit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Grammar11

    Public MustInherit Class YAMLNode

        Friend Overridable Sub Emit(emitter As Emitter)
            Dim isWrote As Boolean = False
            If Not CustomTag.IsEmpty Then
                emitter.Write(CustomTag.ToString()).WriteWhitespace()
                isWrote = True
            End If
            If Anchor <> String.Empty Then
                emitter.Write("&").Write(Anchor).WriteWhitespace()
                isWrote = True
            End If

            If isWrote Then
                If IsMultyline Then
                    emitter.WriteLine()
                End If
            End If
        End Sub

        Public MustOverride ReadOnly Property NodeType As YAMLNodeType
        Public MustOverride ReadOnly Property IsMultyline As Boolean
        Public MustOverride ReadOnly Property IsIndent As Boolean

        Public Property Tag() As String
            Get
                Return CustomTag.Content
            End Get
            Set
                CustomTag = New YAMLTag(YAMLWriter.DefaultTagHandle, Value)
            End Set
        End Property

        Public Property CustomTag() As YAMLTag
        Public ReadOnly Property Anchor As String = String.Empty

    End Class
End Namespace
