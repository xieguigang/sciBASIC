#Region "Microsoft.VisualBasic::ee50458d0a734abe5791b95add19f118, Data_science\MachineLearning\MachineLearning\SVM\Cache\head_t.vb"

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

    '   Total Lines: 27
    '    Code Lines: 16 (59.26%)
    ' Comment Lines: 6 (22.22%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (18.52%)
    '     File Size: 696 B


    '     Class head_t
    ' 
    '         Properties: EnclosingInstance
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SVM

    Friend NotInheritable Class head_t

        Dim m_enclosingInstance As Cache

        Public ReadOnly Property EnclosingInstance As Cache
            Get
                Return m_enclosingInstance
            End Get
        End Property

        ''' <summary>
        ''' a cicular list
        ''' </summary>
        Friend prev, [next] As head_t
        ''' <summary>
        ''' data[0,len) is cached in this entry
        ''' </summary>
        Friend data As Single()
        Friend len As Integer

        Public Sub New(enclosingInstance As Cache)
            m_enclosingInstance = enclosingInstance
        End Sub
    End Class
End Namespace
