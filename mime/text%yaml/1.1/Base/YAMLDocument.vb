#Region "Microsoft.VisualBasic::c5e21d34583e5876b30f42bf2aeeecd7, mime\text%yaml\1.1\Base\YAMLDocument.vb"

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

    '   Total Lines: 37
    '    Code Lines: 28 (75.68%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (24.32%)
    '     File Size: 985 B


    '     Class YAMLDocument
    ' 
    '         Properties: Root
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateMappingRoot, CreateScalarRoot, CreateSequenceRoot
    ' 
    '         Sub: Emit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Grammar11

    Public NotInheritable Class YAMLDocument

        Friend Sub New()
        End Sub

        Public Function CreateScalarRoot() As YAMLScalarNode
            Dim root As New YAMLScalarNode()
            Me.Root = root
            Return root
        End Function

        Public Function CreateSequenceRoot() As YAMLSequenceNode
            Dim root As New YAMLSequenceNode()
            Me.Root = root
            Return root
        End Function

        Public Function CreateMappingRoot() As YAMLMappingNode
            Dim root As New YAMLMappingNode()
            Me.Root = root
            Return root
        End Function

        Friend Sub Emit(emitter As Emitter, isSeparator As Boolean)
            If isSeparator Then
                emitter.Write("---").WriteWhitespace()
            End If

            Root.Emit(emitter)
        End Sub

        Public Property Root() As YAMLNode

    End Class
End Namespace
