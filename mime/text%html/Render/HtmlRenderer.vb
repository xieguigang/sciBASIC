#Region "Microsoft.VisualBasic::4aa1deeb87d85088b5200b4fd9477577, mime\text%html\Render\HtmlRenderer.vb"

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

    '   Total Lines: 84
    '    Code Lines: 40
    ' Comment Lines: 32
    '   Blank Lines: 12
    '     File Size: 2.97 KB


    '     Module HtmlRenderer
    ' 
    '         Properties: References
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: AddReference, (+2 Overloads) Render
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Render

    ''' <summary>
    ''' A Professional HTML Renderer You Will Use
    ''' 
    ''' > https://www.codeproject.com/Articles/32376/A-Professional-HTML-Renderer-You-Will-Use
    ''' </summary>
    Public Module HtmlRenderer

#Region "References"

        ''' <summary>
        ''' Gets a list of Assembly references used to search for external references
        ''' </summary>
        ''' <remarks>
        ''' This references are used when loading images and other content, when
        ''' rendering a piece of HTML/CSS
        ''' </remarks>
        Public ReadOnly Property References() As List(Of Assembly)

        ''' <summary>
        ''' Adds a reference to the References list if not yet listed
        ''' </summary>
        ''' <param name="assembly"></param>
        Public Sub AddReference(assembly As Assembly)
            If Not References.Contains(assembly) Then
                References.Add(assembly)
            End If
        End Sub

        Sub New()
            'Initialize references list
            _References = New List(Of Assembly)()
            'Add this assembly as a reference
            References.Add(Assembly.GetExecutingAssembly())
        End Sub
#End Region

#Region "Methods"

        ''' <summary>
        ''' Draws the HTML on the specified point using the specified width.
        ''' </summary>
        ''' <param name="g">Device to draw</param>
        ''' <param name="html">HTML source</param>
        ''' <param name="location">Point to start drawing</param>
        ''' <param name="width">Width to fit HTML drawing</param>
        <Extension>
        Public Sub Render(g As Graphics, html As String, location As PointF, width As Single)
            Call Render(g, html, New RectangleF(location, New SizeF(width, 0)), False)
        End Sub

        ''' <summary>
        ''' Renders the specified HTML source on the specified area clipping if specified
        ''' </summary>
        ''' <param name="g">Device to draw</param>
        ''' <param name="html">HTML source</param>
        ''' <param name="area">Area where HTML should be drawn</param>
        ''' <param name="clip">If true, it will only paint on the specified area</param>
        <Extension>
        Public Sub Render(g As Graphics, html As String, area As RectangleF, clip As Boolean)
            Dim container As New InitialContainer(html)
            Dim prevClip As Region = g.Clip

            If clip Then
                g.SetClip(area)
            End If

            container.SetBounds(area)
            container.MeasureBounds(g)
            container.Paint(g)

            If clip Then
                g.SetClip(prevClip, CombineMode.Replace)
            End If
        End Sub
#End Region
    End Module
End Namespace
