#Region "Microsoft.VisualBasic::f3c2ddf604a3f903b7ce6781615aaef1, Data\BinaryData\BinaryData\Stream\Offset.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class Offset
    ' 
    '     Properties: Position, Writer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Satisfy
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' Represents a space of 4 bytes reserved in the underlying stream of a <see cref="BinaryDataWriter"/> which can
''' be comfortably satisfied later on.
''' </summary>
Public Class Offset

    ''' <summary>
    ''' Initializes a new instance of the <see cref="Offset"/> class reserving an offset with the specified
    ''' <see cref="BinaryDataWriter"/> at the current position.
    ''' </summary>
    ''' <param name="writer">The <see cref="BinaryDataWriter"/> holding the stream in which the offset will be
    ''' reserved.</param>
    Public Sub New(writer As BinaryDataWriter)
        Me.Writer = writer
        Position = CUInt(Me.Writer.Position)
        Me.Writer.Position += 4
    End Sub

    ''' <summary>
    ''' Gets the <see cref="BinaryDataWriter"/> in which underlying stream the allocation is made.
    ''' </summary>
    Public Property Writer() As BinaryDataWriter
    ''' <summary>
    ''' Gets the address at which the allocation is made.
    ''' </summary>
    Public Property Position() As UInteger

    ''' <summary>
    ''' Satisfies the offset by writing the current position of the underlying stream at the reserved
    ''' <see cref="Position"/>, then seeking back to the current position.
    ''' </summary>
    Public Sub Satisfy()
		' Temporarily seek back to the allocation offset and write the previous address there, then seek back.
		Dim oldPosition As UInteger = CUInt(Writer.Position)
		Writer.Position = Position
		Writer.Write(oldPosition)
		Writer.Position = oldPosition
	End Sub
End Class
