#Region "Microsoft.VisualBasic::1fe5cc32e25d960842bf301f58b770c4, mime\application%pdf\PdfFileWriter\PDF\PdfLayers.vb"

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

    '   Total Lines: 209
    '    Code Lines: 119
    ' Comment Lines: 52
    '   Blank Lines: 38
    '     File Size: 7.59 KB


    '     Enum ListMode
    ' 
    '         AllPages, VisiblePages
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class PdfLayers
    ' 
    '         Properties: ListMode, Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: CreateDictionary, DisplayOrder, DisplayOrderEndGroup, DisplayOrderStartGroup
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Text


    ''' <summary>
    ''' Layers list mode enumeration
    ''' </summary>
    Public Enum ListMode
        ''' <summary>
        ''' List all layers
        ''' </summary>
        AllPages

        ''' <summary>
        ''' List layers for visible pages
        ''' </summary>
        VisiblePages
    End Enum

    ''' <summary>
    ''' PdfLayers control class
    ''' </summary>
    Public Class PdfLayers
        Inherits PdfObject
        ''' <summary>
        ''' Layers name
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' Layers list mode
        ''' </summary>
        Public Property ListMode As ListMode
        Friend LayerList As List(Of PdfLayer)
        Friend OrderList As List(Of Object)

        ''' <summary>
        ''' Layers constructor
        ''' </summary>
        ''' <param name="Document">PDF Document</param>
        ''' <param name="Name">Layers name</param>
        Public Sub New(Document As PdfDocument, Name As String)
            MyBase.New(Document)
            ' Make sure it is done only once
            If Document.Layers IsNot Nothing Then Throw New ApplicationException("PdfLayers is already defined")

            ' save arguments
            Me.Name = Name

            ' save this object within the document object
            Document.Layers = Me

            ' create layers empty list for all layers
            LayerList = New List(Of PdfLayer)()
            OrderList = New List(Of Object)()
            Return
        End Sub

        ''' <summary>
        ''' Layer's display order in layers panel
        ''' </summary>
        ''' <param name="Layer">Layer object</param>
        Public Sub DisplayOrder(Layer As PdfLayer)
            OrderList.Add(Layer)
            Return
        End Sub

        ''' <summary>
        ''' Layer's display order group start marker
        ''' </summary>
        ''' <param name="GroupName">Optional group name</param>
        Public Sub DisplayOrderStartGroup(Optional GroupName As String = "")
            OrderList.Add(GroupName)
            Return
        End Sub

        ''' <summary>
        ''' Layer's display order group end marker
        ''' </summary>
        Public Sub DisplayOrderEndGroup()
            OrderList.Add(0)
            Return
        End Sub

        ' add layers object to document catalog
        Friend Sub CreateDictionary()
            ' build array of all layers
            Dim AllLayers As StringBuilder = New StringBuilder("[")
            Dim LockedLayers As StringBuilder = New StringBuilder("[")
            Dim RadioButtons As List(Of PdfLayer) = New List(Of PdfLayer)()

            For Each Layer In LayerList
                AllLayers.AppendFormat("{0} 0 R ", Layer.ObjectNumber)
                If Layer.Locked = LockLayer.Locked Then LockedLayers.AppendFormat("{0} 0 R ", Layer.ObjectNumber)
                If Not String.IsNullOrWhiteSpace(Layer.RadioButton) Then RadioButtons.Add(Layer)
            Next

            AllLayers.Length -= 1
            AllLayers.Append("]")

            ' add all layers array to the dictionary
            Dictionary.Add("/OCGs", AllLayers.ToString())

            ' create default /D dictionary
            Dim DefaultDict As PdfDictionary = New PdfDictionary(Document)

            ' name
            DefaultDict.AddPdfString("/Name", Name)

            ' list mode
            DefaultDict.AddName("/ListMode", ListMode.ToString())

            ' build array of locked layers
            LockedLayers.Length -= 1

            If LockedLayers.Length <> 0 Then
                LockedLayers.Append("]")
                DefaultDict.Add("/Locked", LockedLayers.ToString())
            End If

            ' add array to OCPD
            If OrderList.Count = 0 Then
                DefaultDict.Add("/Order", AllLayers.ToString())
            Else
                Dim OrderArray As StringBuilder = New StringBuilder("[")

                For Each Item In OrderList

                    If Item.GetType() Is GetType(PdfLayer) Then
                        OrderArray.AppendFormat("{0} 0 R ", CType(Item, PdfLayer).ObjectNumber)
                    ElseIf Item.GetType() Is GetType(String) Then
                        If OrderArray(OrderArray.Length - 1) = " "c Then OrderArray.Length -= 1
                        OrderArray.Append("[")
                        If Not Equals(CStr(Item), "") Then OrderArray.Append(Document.TextToPdfString(CStr(Item), Me))
                    ElseIf Item.GetType() Is GetType(Integer) AndAlso CInt(Item) = 0 Then
                        If OrderArray(OrderArray.Length - 1) = " "c Then OrderArray.Length -= 1
                        OrderArray.Append("]")
                    End If
                Next

                If OrderArray(OrderArray.Length - 1) = " "c Then OrderArray.Length -= 1
                OrderArray.Append("]")
                DefaultDict.Add("/Order", OrderArray.ToString())
            End If

            ' radio buttons
            If RadioButtons.Count > 1 Then
                RadioButtons.Sort()
                Dim RBArray As StringBuilder = New StringBuilder("[")
                Dim [End] = RadioButtons.Count
                Dim Ptr1 As Integer
                Dim Ptr = 0

                While Ptr < [End]
                    ' count how many layers have the same radio button name
                    For Ptr1 = Ptr + 1 To [End] - 1
                        If String.Compare(RadioButtons(Ptr).RadioButton, RadioButtons(Ptr1).RadioButton) <> 0 Then Exit For
                    Next

                    ' single radio button, ignore as far as radio button property
                    If Ptr1 - Ptr < 2 Then Continue While

                    ' build array of layers with the same radio button name
                    RBArray.Append("[")
                    Dim Ptr3 = -1

                    For Ptr2 = Ptr To Ptr1 - 1
                        RBArray.AppendFormat("{0} 0 R ", RadioButtons(Ptr2).ObjectNumber)

                        If RadioButtons(Ptr2).State = LayerState.On Then
                            If Ptr3 < 0 Then
                                Ptr3 = Ptr2
                            Else
                                RadioButtons(Ptr2).State = LayerState.Off
                            End If
                        End If
                    Next

                    RBArray.Length -= 1
                    RBArray.Append("]")
                    Ptr = Ptr1
                End While

                If RBArray.Length > 1 Then
                    RBArray.Append("]")
                    DefaultDict.Add("/RBGroups", RBArray.ToString())
                End If
            End If

            Dim OffLayers As StringBuilder = New StringBuilder("[")

            For Each Layer In LayerList
                If Layer.State = LayerState.Off Then OffLayers.AppendFormat("{0} 0 R ", Layer.ObjectNumber)
            Next

            ' build array of all initially off
            OffLayers.Length -= 1

            If OffLayers.Length <> 0 Then
                OffLayers.Append("]")
                DefaultDict.Add("/OFF", OffLayers.ToString())
            End If

            ' add default dictionary
            Dictionary.AddDictionary("/D", DefaultDict)
            Return
        End Sub
    End Class
