Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports number = System.Double

Namespace Layouts.Cola.GridRouter

    Public Interface NodeAccessor(Of Node)
        Function getChildren(v As Node) As number()
        Function getBounds(v As Node) As Rectangle2D
    End Interface
    Public Class NodeWrapper {
        leaf: Boolean;
        parent: NodeWrapper;
        ports: Vert[];
        constructor(public id: number, public rect: Rectangle, public children: number[]) {
            this.leaf = TypeOf children === 'undefined' || children.length === 0;
        }
    End Class 

    export Class Vert {
        constructor(public id: number, public x:number, public y: number, public node: NodeWrapper = null, public line = null) {}
    }
End Namespace