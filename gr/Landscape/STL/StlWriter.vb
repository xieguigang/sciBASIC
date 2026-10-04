Option Strict On
Option Explicit On

Imports System.IO
Imports std = System.Math

Namespace Stl

    ''' <summary>
    ''' Writes a <see cref="MeshBuilder"/> mesh as a binary STL file
    ''' (80-byte header, uint32 triangle count, then 50 bytes per triangle).
    ''' Floats are little-endian as required by the format.
    ''' </summary>
    Public NotInheritable Class StlWriter

        Private Sub New()
        End Sub

        ''' <summary>Writes the mesh to a file (overwrites).</summary>
        Public Shared Sub Write(mesh As MeshBuilder, path As String, headerText As String)
            Using fs As New FileStream(path, FileMode.Create, FileAccess.Write)
                Write(mesh, fs, headerText)
            End Using
        End Sub

        ''' <summary>Writes the mesh to any writable stream.</summary>
        Public Shared Sub Write(mesh As MeshBuilder, output As Stream, headerText As String)
            If mesh Is Nothing Then Throw New ArgumentNullException(NameOf(mesh))
            If output Is Nothing Then Throw New ArgumentNullException(NameOf(output))

            Dim header(79) As Byte
            If Not String.IsNullOrEmpty(headerText) Then
                Dim text = System.Text.Encoding.ASCII.GetBytes(headerText)
                Array.Copy(text, header, std.Min(text.Length, 80))
            End If

            Using w As New BinaryWriter(output, System.Text.Encoding.ASCII, leaveOpen:=True)
                w.Write(header)
                w.Write(CUInt(mesh.TriangleCount))
                For i = 0 To mesh.TriangleCount - 1
                    Dim t = mesh.GetTriangle(i)
                    For k = 0 To MeshBuilder.FloatsPerTriangle - 1
                        w.Write(t(k))
                    Next
                    w.Write(CUShort(0))    ' attribute byte count: always zero
                Next
                w.Flush()
            End Using
        End Sub

    End Class
End Namespace