#Region "Microsoft.VisualBasic::73fc5ccb77bbfc410ddaca2a5df52771, mime\text%html\Render\CSS\CssAnonymousBlockBox.vb"

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

    '   Total Lines: 44
    '    Code Lines: 27 (61.36%)
    ' Comment Lines: 10 (22.73%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (15.91%)
    '     File Size: 1.41 KB


    '     Class CssAnonymousBlockBox
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class CssAnonymousSpaceBlockBox
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Render.CSS

    ''' <summary>
    ''' Represents an anonymous block box
    ''' </summary>
    ''' <remarks>
    ''' To learn more about anonymous block boxes visit CSS spec:
    ''' http://www.w3.org/TR/CSS21/visuren.html#anonymous-block-level
    ''' </remarks>
    Public Class CssAnonymousBlockBox : Inherits CssBox

        Public Sub New(parent As CssBox)
            MyBase.New(parent)
            Display = CssConstants.Block
        End Sub

        Public Sub New(parent As CssBox, insertBefore As CssBox)
            Me.New(parent)
            Dim index As Integer = parent.Boxes.IndexOf(insertBefore)

            If index < 0 Then
                Throw New Exception("insertBefore box doesn't exist on parent")
            End If
            parent.Boxes.Remove(Me)
            parent.Boxes.Insert(index, Me)
        End Sub
    End Class

    ''' <summary>
    ''' Represents an AnonymousBlockBox which contains only blank spaces
    ''' </summary>
    Public Class CssAnonymousSpaceBlockBox : Inherits CssAnonymousBlockBox

        Public Sub New(parent As CssBox)
            MyBase.New(parent)
            Display = CssConstants.None
        End Sub

        Public Sub New(parent As CssBox, insertBefore As CssBox)
            MyBase.New(parent, insertBefore)
            Display = CssConstants.None
        End Sub
    End Class
End Namespace
