Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Linq

Namespace KMeans.Parallel

    Public Module StreamAPI

        <Extension> Public Function GetRaw(x As Entity) As Byte()
            Dim name As Byte() = Encoding.Unicode.GetBytes(x.uid)
            Dim buffer As Byte() =
                New Byte(name.Length + RawStream.INT32 + (x.Properties.Length * RawStream.DblFloat) - 1) {}
            Dim i As Integer
            Dim nameLen As Byte() = BitConverter.GetBytes(name.Length)

            Call Array.ConstrainedCopy(nameLen, Scan0, buffer, i.Move(nameLen.Length), nameLen.Length)
            Call Array.ConstrainedCopy(name, Scan0, buffer, i.Move(name.Length), name.Length)

            For Each d As Double In x.Properties
                Call Array.ConstrainedCopy(BitConverter.GetBytes(d), Scan0, buffer, i.Move(RawStream.DblFloat), RawStream.DblFloat)
            Next

            Return buffer
        End Function

        <Extension> Public Function GetObject(buffer As Byte()) As Entity
            Dim nameLen As Byte() = New Byte(RawStream.INT32 - 1) {}
            Dim p As Integer
            Call Array.ConstrainedCopy(buffer, p.Move(nameLen.Length), nameLen, Scan0, nameLen.Length)

            Dim name As Byte() = New Byte(BitConverter.ToInt32(nameLen, Scan0) - 1) {}
            Call Array.ConstrainedCopy(buffer, p.Move(name.Length), name, Scan0, name.Length)

            Dim props As Double() =
                buffer.Skip(nameLen.Length + name.Length).Split(RawStream.DblFloat) _
                      .ToArray(Function(buf) BitConverter.ToDouble(buf, Scan0))

            Return New Entity With {
                .Properties = props,
                .uid = Encoding.Unicode.GetString(name)
            }
        End Function
    End Module
End Namespace