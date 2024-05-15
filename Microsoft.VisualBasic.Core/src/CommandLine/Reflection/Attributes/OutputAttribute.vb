#Region "Microsoft.VisualBasic::c920223387a0eb7997fe9918647ef48c, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\Attributes\OutputAttribute.vb"

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
    '    Code Lines: 11
    ' Comment Lines: 4
    '   Blank Lines: 4
    '     File Size: 576 B


    '     Class OutputAttribute
    ' 
    '         Properties: extension, result
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CommandLine.Reflection

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class OutputAttribute : Inherits Attribute

        Public ReadOnly Property result As Type
        ''' <summary>
        ''' The file extension name, like ``*.csv``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property extension As String

        Sub New(resultType As Type, fileExt$)
            result = resultType
            extension = fileExt
        End Sub

    End Class
End Namespace
