#Region "Microsoft.VisualBasic::5b13833050374e55863c9c09d0733dc6, Data_science\Graph\Analysis\FastUnfolding\KeyMaps.vb"

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
    '    Code Lines: 23
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 762 B


    '     Class KeyMaps
    ' 
    '         Properties: Keys
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Analysis.FastUnfolding

    Public Class KeyMaps

        ReadOnly maps As New Dictionary(Of String, List(Of String))

        Default Public Property Item(key As String) As List(Of String)
            Get
                If Not maps.ContainsKey(key) Then
                    maps.Add(key, New List(Of String))
                End If

                Return maps(key)
            End Get
            Set(value As List(Of String))
                maps(key) = value
            End Set
        End Property

        Public ReadOnly Property Keys As IEnumerable(Of String)
            Get
                Return maps.Keys
            End Get
        End Property

        Sub New()
        End Sub

    End Class
End Namespace
