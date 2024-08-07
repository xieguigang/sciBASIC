#Region "Microsoft.VisualBasic::6f68bea61b202015690c19a0891139f8, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Set\EnumSet.vb"

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

    '   Total Lines: 34
    '    Code Lines: 22 (64.71%)
    ' Comment Lines: 4 (11.76%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (23.53%)
    '     File Size: 961 B


    '     Class EnumSet
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [of], allOf, noneOf
    ' 
    '         Sub: add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataStructures

    Public Class EnumSet(Of E As {Structure, IComparable, IComparable(Of E)})

        ReadOnly enums As List(Of E)

        Sub New(enums As E())
            Me.enums = enums.AsList
        End Sub

        Public Sub add(e As E)
            If enums.IndexOf(e) = -1 Then
                Call enums.Add(e)
            End If
        End Sub

        Public Shared Function [of](ParamArray vec As E()) As EnumSet(Of E)
            Return New EnumSet(Of E)(vec)
        End Function

        ''' <summary>
        ''' create a new empty enum set
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function noneOf() As EnumSet(Of E)
            Return New EnumSet(Of E)({})
        End Function

        Public Shared Function allOf() As EnumSet(Of E)
            Return New EnumSet(Of E)(EnumHelpers.Enums(Of E))
        End Function

    End Class
End Namespace
