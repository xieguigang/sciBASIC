Imports Microsoft.VisualBasic.Engine.Physics

''' <summary>
''' the 3D volume container of the particles(the 3D counterpart of the 2D
''' <see cref="IContainer(Of T)"/>).
''' </summary>
Public Interface IContainer3D(Of T)

    ReadOnly Property Entity As IReadOnlyCollection(Of T)
    ''' <summary>
    ''' the size(x/y/z extent) of the box volume that holds the fluid.
    ''' </summary>
    Property BoxSize As Vector3

End Interface
