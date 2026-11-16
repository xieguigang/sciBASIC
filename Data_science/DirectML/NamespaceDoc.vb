''' <summary>
''' Pure interop DirectML compute framework for sciBASIC#: hand written COM vtable bindings over
''' <c>dxgi.dll</c>, <c>d3d12.dll</c> and <c>DirectML.dll</c>, exposing device enumeration, command
''' recording, buffer/descriptor management, DML operator descriptors, graph compilation and
''' execution. No third party NuGet package is referenced.
''' </summary>
''' <remarks>
''' <para>
''' The framework mirrors the layering of the CUDA stack (<c>Microsoft.VisualBasic.Computing.ILCuda</c>):
''' </para>
''' <list type="table">
''' <item><term><see cref="Interop"/></term><description>raw exports and COM vtable slots, HRESULT handling, DML enums and descriptors.</description></item>
''' <item><term><see cref="Runtime"/></term><description>device/queue/fence/resource lifecycle wrapped as <see cref="System.IDisposable"/> objects.</description></item>
''' <item><term><see cref="Operators"/></term><description>ONNX operator names to DML operator descriptor factories plus shape inference.</description></item>
''' <item><term><see cref="Diagnostics"/></term><description>plain data reports and actionable fix suggestions, no Console dependency.</description></item>
''' </list>
''' </remarks>
Friend Class NamespaceDoc
End Class
