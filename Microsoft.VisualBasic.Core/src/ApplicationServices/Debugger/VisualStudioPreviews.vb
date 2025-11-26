#Region "Microsoft.VisualBasic::0eefb446bd1dceda8ecd58b21f07c44b, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\VisualStudioPreviews.vb"

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
    '    Code Lines: 29 (78.38%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (21.62%)
    '     File Size: 1.19 KB


    '     Interface IVisualStudioPreviews
    ' 
    '         Properties: Previews
    ' 
    '     Module InspectObject
    ' 
    '         Function: ToInspectString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace ApplicationServices.Debugging

    Public Interface IVisualStudioPreviews
        ReadOnly Property Previews As String
    End Interface

    Public Module InspectObject

        Public Function ToInspectString(obj As Object) As String
            If obj Is Nothing Then
                Return "null"
            End If

            Dim type As Type = obj.GetType

            If DataFramework.IsPrimitive(type) Then
                Return obj.ToString
            End If

            If type.IsArray Then
                If type.GetElementType Is Nothing Then
                    Return $"{DirectCast(obj, Array).Length} any objects"
                Else
                    Return $"{DirectCast(obj, Array).Length} {type.GetElementType.Name}[]"
                End If
            End If
            If type.ImplementInterface(Of ICollection) Then
                Return $"{DirectCast(obj, ICollection).Count} elements collection"
            End If

            Return obj.ToString
        End Function
    End Module
End Namespace
