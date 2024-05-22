#Region "Microsoft.VisualBasic::72072e9b61fb1a9b5bafc5755fd8bedb, mime\text%yaml\1.2\Syntax\TagPrefix.vb"

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
    '    Code Lines: 14 (73.68%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (26.32%)
    '     File Size: 424 B


    '     Class TagPrefix
    ' 
    '         Function: ToString
    ' 
    '     Class GlobalTagPrefix
    ' 
    ' 
    ' 
    '     Class LocalTagPrefix
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Syntax

    Public Class TagPrefix

        Public Prefix As New List(Of Char)()

        Public Overrides Function ToString() As String
            Return $"<{Me.GetType.Name}> {Prefix.CharString}"
        End Function
    End Class

    Public Class GlobalTagPrefix
        Inherits TagPrefix
    End Class

    Public Class LocalTagPrefix
        Inherits TagPrefix
    End Class
End Namespace
