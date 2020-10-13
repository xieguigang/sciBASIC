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
            Private Set(ByVal value As String)
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
        Public Sub New(ByVal LayersParent As PdfLayers, ByVal Name As String)
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
        Public Function CompareTo(ByVal Other As PdfLayer) As Integer Implements IComparable(Of PdfLayer).CompareTo
            Dim Cmp = String.Compare(RadioButton, Other.RadioButton)
            If Cmp <> 0 Then Return Cmp
            Return ObjectNumber - Other.ObjectNumber
        End Function
    End Class

