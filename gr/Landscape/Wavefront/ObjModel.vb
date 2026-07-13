#Region "Microsoft.VisualBasic::c649bfb05a5369f7d57cf6b5f703d827, gr\Landscape\Wavefront\OBJ.vb"

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

    '   Total Lines: 49
    '    Code Lines: 35 (71.43%)
    ' Comment Lines: 4 (8.16%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (20.41%)
    '     File Size: 1.49 KB


    '     Class OBJ
    ' 
    '         Properties: comment, mtllib, parts
    ' 
    '         Function: ReadFile
    ' 
    '     Class ObjectPart
    ' 
    '         Properties: f, g, IsEmpty, usemtl, vertex
    '                     vn
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language

Namespace Wavefront

    Public Class OBJ

        ''' <summary>
        ''' lib file name of mtl data
        ''' </summary>
        ''' <returns></returns>
        Public Property mtllib As String
        Public Property parts As ObjectPart()
        Public Property comment As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Function ReadFile(file As StreamReader) As OBJ
            Return TextParser.ParseFile(file)
        End Function

    End Class

    Public Class ObjectPart

        Public Property g As String
        Public Property vertex As Point3D()
        Public Property vn As Point3D()
        Public Property usemtl As String
        Public Property f As Triangle()

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return g.StringEmpty AndAlso
                    vertex.IsNullOrEmpty AndAlso
                    vn.IsNullOrEmpty AndAlso
                    usemtl.StringEmpty AndAlso
                    f.IsNullOrEmpty
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{g Or "no_label".AsDefault}: {vertex.Length} vertexs and {f.Length} triangles"
        End Function

    End Class
End Namespace
