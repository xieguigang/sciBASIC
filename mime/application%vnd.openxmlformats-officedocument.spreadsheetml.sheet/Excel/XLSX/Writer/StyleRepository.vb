#Region "Microsoft.VisualBasic::acea349aaf11bb8cd57e5884dd4c085c, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\StyleRepository.vb"

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

    '   Total Lines: 80
    '    Code Lines: 36
    ' Comment Lines: 35
    '   Blank Lines: 9
    '     File Size: 2.94 KB


    '     Class StyleRepository
    ' 
    '         Properties: Instance, Styles
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: AddStyle
    ' 
    '         Sub: FlushStyles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  PicoXLSX is a small .NET library to generate and read XLSX (Microsoft Excel 2007 or newer) files in an easy and native way
'  Copyright Raphael Stoeckli © 2023
'  This library is licensed under the MIT License.
'  You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
' 

Namespace XLSX.Writer

    ''' <summary>
    ''' Class to manage all styles at runtime, before writing XLSX files. The main purpose is deduplication and decoupling of styles from workbooks at runtime
    ''' </summary>
    Public Class StyleRepository
        ''' <summary>
        ''' Defines the lockObject
        ''' </summary>
        Private ReadOnly lockObject As Object = New Object()

        ''' <summary>
        ''' Defines the instance
        ''' </summary>
        Private Shared instanceField As StyleRepository

        ''' <summary>
        ''' Gets the singleton instance of the repository
        ''' </summary>
        Public Shared ReadOnly Property Instance As StyleRepository
            Get
                instanceField = If(instanceField, New StyleRepository())
                Return instanceField
            End Get
        End Property

        ''' <summary>
        ''' Defines the styles
        ''' </summary>
        Private stylesField As Dictionary(Of Integer, Style)

        ''' <summary>
        ''' Gets the currently managed styles of the repository
        ''' </summary>
        Public ReadOnly Property Styles As Dictionary(Of Integer, Style)
            Get
                Return stylesField
            End Get
        End Property

        ''' <summary>
        ''' Prevents a default instance of the <see cref="StyleRepository"/> class from being created
        ''' </summary>
        Private Sub New()
            stylesField = New Dictionary(Of Integer, Style)()
        End Sub

        ''' <summary>
        ''' Adds a style to the repository and returns the actual reference
        ''' </summary>
        ''' <param name="style">Style to add.</param>
        ''' <returns>Reference from the repository. If the style to add already existed, the existing object is returned, otherwise the newly added one.</returns>
        Public Function AddStyle(style As Style) As Style
            SyncLock lockObject
                If style Is Nothing Then
                    Return Nothing
                End If
                Dim hashCode As Integer = style.GetHashCode()
                If Not stylesField.ContainsKey(hashCode) Then
                    stylesField.Add(hashCode, style)
                End If
                Return stylesField(hashCode)
            End SyncLock
        End Function

        ''' <summary>
        ''' Empties the static repository
        ''' </summary>
        Public Sub FlushStyles()
            stylesField.Clear()
        End Sub
    End Class
End Namespace
