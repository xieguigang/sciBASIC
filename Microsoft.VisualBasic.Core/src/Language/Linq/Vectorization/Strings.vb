#Region "Microsoft.VisualBasic::5922be49c16581d2f9acb65ba92610e2, Microsoft.VisualBasic.Core\src\Language\Linq\Vectorization\Strings.vb"

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
    '    Code Lines: 10 (33.33%)
    ' Comment Lines: 16 (53.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (13.33%)
    '     File Size: 1.20 KB


    '     Class Strings
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Len
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Base = Microsoft.VisualBasic.Strings

Namespace Language.Vectorization

    ''' <summary>
    ''' The <see cref="String"/> module contains procedures used to perform string operations.
    ''' </summary>
    Public NotInheritable Class Strings

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Returns an integer containing either the number of characters in a string or 
        ''' the nominal number of bytes required to store a variable.
        ''' </summary>
        ''' <param name="strings">
        ''' Any valid String expression or variable name. If Expression is of type Object, 
        ''' the Len function returns the size as it will be written to the file by the 
        ''' FilePut function.
        ''' </param>
        ''' <returns>
        ''' Returns an integer containing either the number of characters in a string or 
        ''' the nominal number of bytes required to store a variable.
        ''' </returns>
        Public Shared Function Len(strings As IEnumerable(Of String)) As IEnumerable(Of Integer)
            Return strings.Select(AddressOf Base.Len)
        End Function
    End Class
End Namespace
