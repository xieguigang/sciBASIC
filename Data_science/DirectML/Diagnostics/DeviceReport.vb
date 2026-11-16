' ------------------------------------------------------------------------
' 设备报告：适配器、特性级别、显存（纯数据结构，不依赖 Console）
' ------------------------------------------------------------------------

Imports System.Text
Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Diagnostics

    ''' <summary>一个适配器的诊断信息。</summary>
    Public NotInheritable Class AdapterReport

        Public Property Index As Integer
        Public Property Description As String
        Public Property Vendor As String
        Public Property VendorId As UInteger
        Public Property DeviceId As UInteger
        Public Property DedicatedVideoMemory As ULong
        Public Property SharedSystemMemory As ULong
        Public Property IsSoftware As Boolean
        Public Property SupportsD3D12 As Boolean

        ''' <summary>专用显存（MB）。</summary>
        Public ReadOnly Property DedicatedVideoMemoryMB As Double
            Get
                Return DedicatedVideoMemory / 1024.0 / 1024.0
            End Get
        End Property

        Friend Shared Function FromAdapter(adapter As DmlAdapter) As AdapterReport
            Return New AdapterReport With {
                .Index = adapter.Index,
                .Description = adapter.Description,
                .Vendor = adapter.VendorName,
                .VendorId = adapter.VendorId,
                .DeviceId = adapter.DeviceId,
                .DedicatedVideoMemory = adapter.DedicatedVideoMemory,
                .SharedSystemMemory = adapter.SharedSystemMemory,
                .IsSoftware = adapter.IsSoftware,
                .SupportsD3D12 = adapter.SupportsD3D12
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Index}] {Description} ({Vendor}, {DedicatedVideoMemoryMB:F0} MB)"
        End Function

    End Class

    ''' <summary>设备报告。</summary>
    Public NotInheritable Class DeviceReport

        Public Property Adapter As AdapterReport
        Public Property FeatureLevel As String
        Public Property SupportsFloat32 As Boolean
        Public Property SupportsFloat16 As Boolean
        Public Property SupportsInt8 As Boolean
        Public Property SupportsInt32 As Boolean
        Public Property MemoryBudget As ULong
        Public Property MemoryCurrentUsage As ULong

        ''' <summary>生成可读文本。</summary>
        Public Function ToText() As String
            Dim sb As New StringBuilder()

            Call sb.AppendLine($"adapter           : {If(Adapter Is Nothing, "<none>", Adapter.ToString())}")
            Call sb.AppendLine($"directml level    : {FeatureLevel}")
            Call sb.AppendLine($"float32 / float16 : {SupportsFloat32} / {SupportsFloat16}")
            Call sb.AppendLine($"int8 / int32      : {SupportsInt8} / {SupportsInt32}")

            If MemoryBudget > 0UL Then
                Call sb.AppendLine($"video memory      : budget={MemoryBudget / 1024.0 / 1024.0:F0} MB, " &
                                   $"used={MemoryCurrentUsage / 1024.0 / 1024.0:F0} MB")
            End If

            Return sb.ToString()
        End Function

        ''' <summary>从设备生成报告。</summary>
        Public Shared Function FromDevice(device As DmlDevice) As DeviceReport
            If device Is Nothing Then
                Throw New ArgumentNullException(NameOf(device))
            End If

            Dim report As New DeviceReport With {
                .Adapter = AdapterReport.FromAdapter(device.Adapter),
                .FeatureLevel = device.FeatureLevelText,
                .SupportsFloat32 = device.IsDataTypeSupported(DML_TENSOR_DATA_TYPE.FLOAT32),
                .SupportsFloat16 = device.IsDataTypeSupported(DML_TENSOR_DATA_TYPE.FLOAT16),
                .SupportsInt8 = device.IsDataTypeSupported(DML_TENSOR_DATA_TYPE.INT8),
                .SupportsInt32 = device.IsDataTypeSupported(DML_TENSOR_DATA_TYPE.INT32)
            }

            Dim budget As ULong

            If device.Adapter.TryGetBudget(budget) Then
                report.MemoryBudget = budget
            End If

            Return report
        End Function

        Public Overrides Function ToString() As String
            Return ToText()
        End Function

    End Class

End Namespace
