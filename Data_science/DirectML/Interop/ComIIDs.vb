' ------------------------------------------------------------------------
' 本框架用到的接口 IID 常量
'
' 取值全部来自 Windows SDK 头文件（dxgi.h / dxgi1_4.h / d3d12.h / DirectML.h）中的
' MIDL_INTERFACE / DML_DECLARE_INTERFACE 声明，不要凭记忆修改。
' ------------------------------------------------------------------------

Namespace Interop

    ''' <summary>本框架用到的 COM 接口 IID。</summary>
    Public Module ComIIDs

        ' ---- DXGI ----

        ''' <summary>IDXGIFactory1</summary>
        Public ReadOnly IDXGIFactory1 As New Guid("770aae78-f26f-4dba-a829-253c83d1b387")
        ''' <summary>IDXGIFactory2</summary>
        Public ReadOnly IDXGIFactory2 As New Guid("50c83a1c-e072-4c48-87b0-3630fa36a6d0")
        ''' <summary>IDXGIFactory4</summary>
        Public ReadOnly IDXGIFactory4 As New Guid("1bc6ea02-ef36-464f-bf0c-21ca39e5168a")
        ''' <summary>IDXGIFactory6（按 GPU 偏好枚举适配器）</summary>
        Public ReadOnly IDXGIFactory6 As New Guid("c1b6694f-ff09-44a9-b03c-77900a0a1d17")
        ''' <summary>IDXGIAdapter1</summary>
        Public ReadOnly IDXGIAdapter1 As New Guid("29038f61-3839-4626-91fd-086879011a05")
        ''' <summary>IDXGIAdapter3（显存预算查询）</summary>
        Public ReadOnly IDXGIAdapter3 As New Guid("645967a4-1392-4310-a798-8053ce3e93fd")

        ' ---- D3D12 ----

        ''' <summary>ID3D12Device</summary>
        Public ReadOnly ID3D12Device As New Guid("189819f1-1db6-4b57-be54-1821339b85f7")
        ''' <summary>ID3D12CommandQueue</summary>
        Public ReadOnly ID3D12CommandQueue As New Guid("0ec870a6-5d7e-4c22-8cfc-5baae07616ed")
        ''' <summary>ID3D12CommandAllocator</summary>
        Public ReadOnly ID3D12CommandAllocator As New Guid("6102dee4-af59-4b09-b999-b44d73f09b24")
        ''' <summary>ID3D12GraphicsCommandList</summary>
        Public ReadOnly ID3D12GraphicsCommandList As New Guid("5b160d0f-ac1b-4185-8ba8-b3ae42a5a455")
        ''' <summary>ID3D12Resource</summary>
        Public ReadOnly ID3D12Resource As New Guid("696442be-a72e-4059-bc79-5b5c98040fad")
        ''' <summary>ID3D12Fence</summary>
        Public ReadOnly ID3D12Fence As New Guid("0a753dcf-c4d8-4b91-adf6-be5a60d95a76")
        ''' <summary>ID3D12DescriptorHeap</summary>
        Public ReadOnly ID3D12DescriptorHeap As New Guid("8efb471d-616c-4f49-90f7-127bb763fa51")
        ''' <summary>ID3D12Heap（placed 资源的显存堆）</summary>
        Public ReadOnly ID3D12Heap As New Guid("6b3b2502-6e51-45b3-90ee-9884265e8df3")

        ' ---- D3D12 调试层（d3d12sdklayers.h）----

        ''' <summary>ID3D12Debug</summary>
        Public ReadOnly ID3D12Debug As New Guid("344488b7-6846-474b-b989-f027448245e0")
        ''' <summary>ID3D12InfoQueue</summary>
        Public ReadOnly ID3D12InfoQueue As New Guid("0742a90b-c387-483f-b946-30a7e4e61458")

        ' ---- DirectML ----

        ''' <summary>IDMLDevice</summary>
        Public ReadOnly IDMLDevice As New Guid("6dbd6437-96fd-423f-a98c-ae5e7c2a573f")
        ''' <summary>IDMLOperator</summary>
        Public ReadOnly IDMLOperator As New Guid("26caae7a-3081-4633-9581-226fbe57695d")
        ''' <summary>IDMLCompiledOperator</summary>
        Public ReadOnly IDMLCompiledOperator As New Guid("6b15e56a-bf5c-4902-92d8-da3a650afea4")
        ''' <summary>IDMLOperatorInitializer</summary>
        Public ReadOnly IDMLOperatorInitializer As New Guid("427c1113-435c-469c-8676-4d5dd072f813")
        ''' <summary>IDMLBindingTable</summary>
        Public ReadOnly IDMLBindingTable As New Guid("29c687dc-de74-4e3b-ab00-1168f2fc3cfc")
        ''' <summary>IDMLCommandRecorder</summary>
        Public ReadOnly IDMLCommandRecorder As New Guid("e6857a76-2e3e-4fdd-bff4-5d2ba10fb453")

        ' ---- 其它 ----

        ''' <summary>IUnknown</summary>
        Public ReadOnly IUnknown As New Guid("00000000-0000-0000-c000-000000000046")

    End Module

End Namespace
