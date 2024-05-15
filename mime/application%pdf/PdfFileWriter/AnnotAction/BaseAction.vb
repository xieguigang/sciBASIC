#Region "Microsoft.VisualBasic::86d4f30611284182c17604370a11a6b7, mime\application%pdf\PdfFileWriter\AnnotAction\BaseAction.vb"

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

    '   Total Lines: 33
    '    Code Lines: 23
    ' Comment Lines: 6
    '   Blank Lines: 4
    '     File Size: 1.06 KB


    ' Class AnnotAction
    ' 
    '     Properties: Subtype
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) IsEqual
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Annotation action base class
''' </summary>
Public Class AnnotAction
    ''' <summary>
    ''' Gets the PDF PdfAnnotation object subtype
    ''' </summary>
    Private _Subtype As String

    Public Property Subtype As String
        Get
            Return _Subtype
        End Get
        Friend Set(value As String)
            _Subtype = value
        End Set
    End Property

    Friend Sub New(Subtype As String)
        Me.Subtype = Subtype
        Return
    End Sub

    Friend Overridable Function IsEqual(Other As AnnotAction) As Boolean
        Throw New ApplicationException("AnnotAction IsEqual not implemented")
    End Function

    Friend Shared Function IsEqual(One As AnnotAction, Two As AnnotAction) As Boolean
        If One Is Nothing AndAlso Two Is Nothing Then Return True
        If One Is Nothing AndAlso Two IsNot Nothing OrElse One IsNot Nothing AndAlso Two Is Nothing OrElse One.GetType() IsNot Two.GetType() Then Return False
        Return One.IsEqual(Two)
    End Function
End Class
