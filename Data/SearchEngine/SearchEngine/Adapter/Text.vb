#Region "Microsoft.VisualBasic::408d5f9d8f0b610734baa01728c56287, sciBASIC#\Data\SearchEngine\SearchEngine\Adapter\Text.vb"

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

    '   Total Lines: 12
    '    Code Lines: 6
    ' Comment Lines: 4
    '   Blank Lines: 2
    '     File Size: 262.00 B


    ' Structure Text
    ' 
    '     Properties: Text
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Public Structure Text

    ''' <summary>
    ''' The string value.
    ''' </summary>
    ''' <returns></returns>
    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Text
    End Function
End Structure
