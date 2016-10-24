Imports System.Drawing

Namespace Drawing3D

    Public Interface I3DModel : Inherits IEnumerable(Of Point3D)

        Function Copy(data As IEnumerable(Of Point3D)) As I3DModel
        Sub Draw(ByRef canvas As Graphics, camera As Camera)
    End Interface
End Namespace