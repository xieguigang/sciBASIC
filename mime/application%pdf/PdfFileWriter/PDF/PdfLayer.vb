#Region "Microsoft.VisualBasic::3afa5ad35fa2c3df78c51b17a09902ea, mime\application%pdf\PdfFileWriter\PDF\PdfLayer.vb"

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

    '   Total Lines: 106
    '    Code Lines: 40
    ' Comment Lines: 49
    '   Blank Lines: 17
    '     File Size: 2.86 KB


    '     Enum LockLayer
    ' 
    '         Locked, Unlocked
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum LayerState
    ' 
    '         [On], Off
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class PdfLayer
    ' 
    '         Properties: Locked, Name, RadioButton, State
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareTo
    ' 
    ' /********************************************************************************/

#End Region

Imports System


    ''' <summary>
    ''' Lock/unlock layer enumeration
    ''' </summary>
    Public Enum LockLayer
        ''' <summary>
        ''' Unlock layer (default)
        ''' </summary>
        Unlocked

        ''' <summary>
        ''' Lock layer
        ''' </summary>
        Locked
    End Enum

    ''' <summary>
    ''' Layer state
    ''' </summary>
    Public Enum LayerState
        ''' <summary>
        ''' Layer state is ON
        ''' </summary>
        [On]

        ''' <summary>
        ''' Layer state is OFF
        ''' </summary>
        Off
    End Enum

    ''' <summary>
    ''' PdfLayer class
    ''' </summary>
    Public Class PdfLayer
        Inherits PdfObject
        Implements IComparable(Of PdfLayer)
        ''' <summary>
        ''' Layer name
        ''' </summary>
        Private _Name As String

        Public Property Name As String
            Get
                Return _Name
            End Get
            Private Set(value As String)
                _Name = value
            End Set
        End Property

        ''' <summary>
        ''' Layer locked or unlocked
        ''' </summary>
        Public Property Locked As LockLayer

        ''' <summary>
        ''' Initial layer state (on or off)
        ''' </summary>
        Public Property State As LayerState

        ''' <summary>
        ''' Layer is a radio button
        ''' </summary>
        Public Property RadioButton As String
        Friend LayersParent As PdfLayers

        ''' <summary>
        ''' Layer constructor
        ''' </summary>
        ''' <param name="LayersParent">Layers parent</param>
        ''' <param name="Name">Layer's name</param>
        Public Sub New(LayersParent As PdfLayers, Name As String)
            MyBase.New(LayersParent.Document, ObjectType.Dictionary, "/OCG")
            ' save arguments
            Me.Name = Name

            ' save layers parent
            Me.LayersParent = LayersParent

            ' create resource code
            ResourceCode = Document.GenerateResourceNumber("O"c)

            ' add layer name to the dictionary
            Dictionary.AddPdfString("/Name", Name)

            ' add to the list of all layers
            LayersParent.LayerList.Add(Me)

            ' exit
            Return
        End Sub

    ''' <summary>
    ''' CompareTo for IComparabler
    ''' </summary>
    ''' <param name="Other">Other layer</param>
    ''' <returns>Compare result</returns>
    Public Overloads Function CompareTo(Other As PdfLayer) As Integer Implements IComparable(Of PdfLayer).CompareTo
        Dim Cmp = String.Compare(RadioButton, Other.RadioButton)
        If Cmp <> 0 Then Return Cmp
        Return ObjectNumber - Other.ObjectNumber
    End Function
End Class
