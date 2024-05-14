#Region "Microsoft.VisualBasic::047f69a452f7388430d1607ebda355e8, mime\application%rdf+xml\Turtle\Triple.vb"

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
    '    Code Lines: 16
    ' Comment Lines: 14
    '   Blank Lines: 7
    '     File Size: 935 B


    '     Class Triple
    ' 
    '         Properties: relations, subject
    ' 
    '         Function: ToString
    ' 
    '     Class Relation
    ' 
    '         Properties: objs, predicate
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Turtle

    ''' <summary>
    ''' object properties
    ''' </summary>
    Public Class Triple

        Public Property subject As String
        Public Property relations As Relation()

        Public Overrides Function ToString() As String
            Return $"<{subject}> {relations.JoinBy(" ; ")}."
        End Function

    End Class

    ''' <summary>
    ''' property data
    ''' </summary>
    Public Class Relation

        ''' <summary>
        ''' the property name
        ''' </summary>
        ''' <returns></returns>
        Public Property predicate As String
        ''' <summary>
        ''' the property value
        ''' </summary>
        ''' <returns></returns>
        Public Property objs As String()

        Public Overrides Function ToString() As String
            Return $"<{predicate}> <{objs.JoinBy(" , ")}>"
        End Function
    End Class
End Namespace
