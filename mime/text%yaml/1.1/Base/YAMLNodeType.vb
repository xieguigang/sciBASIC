#Region "Microsoft.VisualBasic::4efc758f3f1c45982f3bc04e05a3137a, mime\text%yaml\1.1\Base\YAMLNodeType.vb"

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
    '    Code Lines: 7
    ' Comment Lines: 9
    '   Blank Lines: 3
    '     File Size: 450 B


    '     Enum YAMLNodeType
    ' 
    '         Mapping, Scalar, Sequence
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Grammar11

    Public Enum YAMLNodeType
        ''' <summary>
        ''' The node is a <see cref="YamlMappingNode"/>.
        ''' </summary>
        Mapping

        ''' <summary>
        ''' The node is a <see cref="YamlScalarNode"/>.
        ''' </summary>
        Scalar

        ''' <summary>
        ''' The node is a <see cref="YamlSequenceNode"/>.
        ''' </summary>
        Sequence
    End Enum
End Namespace
