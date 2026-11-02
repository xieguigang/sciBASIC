Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf.test
'  Data\BinaryData\Protobuf\test\Models.vb
' /********************************************************************************/

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Serialization

''' <summary>
''' 电话号码类型。
''' </summary>
Public Enum PhoneKind

    Mobile = 0
    Home = 1
    Work = 2

End Enum

''' <summary>
''' 订单状态。
''' </summary>
Public Enum OrderStatus

    Pending = 0
    Paid = 1
    Shipped = 2
    Closed = 9

End Enum

<ProtoContract>
Public Class PhoneNumber

    <ProtoMember(1)>
    Public Property Number As String

    <ProtoMember(2)>
    Public Property Kind As PhoneKind

End Class

<ProtoContract>
Public Class Person

    <ProtoMember(1)>
    Public Property Name As String

    <ProtoMember(2)>
    Public Property Id As Integer

    <ProtoMember(3)>
    Public Property Email As String

    <ProtoMember(4)>
    Public Property Tags As List(Of String) = New List(Of String)

    <ProtoMember(5)>
    Public Property Scores As List(Of Integer) = New List(Of Integer)

    <ProtoMember(6)>
    Public Property Phones As List(Of PhoneNumber) = New List(Of PhoneNumber)

    <ProtoMember(7)>
    Public Property NickNames As String() = Array.Empty(Of String)

End Class

<ProtoContract>
Public Class OrderProfile

    <ProtoMember(1)>
    Public Property Attributes As Dictionary(Of String, String) = New Dictionary(Of String, String)

    <ProtoMember(2)>
    Public Property Status As OrderStatus

    <ProtoMember(3)>
    Public Property Score As Double?

    <ProtoMember(4)>
    Public Property Discount As Decimal?

    <ProtoMember(5)>
    Public Property CreatedAt As DateTime

    <ProtoMember(6)>
    Public Property Payload As Byte()

    <ProtoMember(7)>
    Public Property Owner As Person

    <ProtoMember(8)>
    Public Property Quantities As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)

End Class

''' <summary>
''' 用于验证黄金字节向量的最小消息类型。
''' </summary>
<ProtoContract>
Public Class WireVector

    <ProtoMember(1)>
    Public Property VarintValue As Integer

    <ProtoMember(2)>
    Public Property Text As String

    <ProtoMember(4)>
    Public Property Numbers As List(Of Integer) = New List(Of Integer)

    <ProtoMember(16)>
    Public Property Extra As String

End Class

''' <summary>
''' 会被以 packed 形式写出的字段类型，用于测试与 unpacked 编码的互通。
''' </summary>
<ProtoContract>
Public Class PackedVector

    <ProtoMember(4)>
    Public Property Values As List(Of Integer) = New List(Of Integer)

End Class

''' <summary>
''' 实现 <see cref="IProtoMessage"/>，用于验证未知字段的保留与重放。
''' </summary>
<ProtoContract>
Public Class UnknownTolerantMessage
    Implements IProtoMessage

    Private ReadOnly _unknown As New UnknownFieldSet()

    <ProtoMember(1)>
    Public Property Id As Integer

    Public ReadOnly Property UnknownFields As UnknownFieldSet Implements IProtoMessage.UnknownFields
        Get
            Return _unknown
        End Get
    End Property

End Class

''' <summary>
''' 演示用的 .proto 源文本。
''' </summary>
Public Module SampleProto

    Public Const Text As String = "
// 演示用 schema：覆盖 proto3 的主要语法
syntax = ""proto3"";

package demo.shop;

import ""common/types.proto"";

option csharp_namespace = ""Demo.Shop"";
option optimize_for = SPEED;

/* 订单状态 */
enum OrderStatus {
  STATUS_UNKNOWN = 0;
  STATUS_PAID = 1;
  STATUS_SHIPPED = 2;
  STATUS_CLOSED = 9 [deprecated = true];
}

message Customer {
  string name = 1;
  int32 id = 2;
  string email = 3;
  repeated string tags = 4;
  repeated int32 scores = 5 [packed = true];
  repeated PhoneNumber phones = 6;

  message PhoneNumber {
    string number = 1;
    PhoneKind kind = 2;
  }

  enum PhoneKind {
    MOBILE = 0;
    HOME = 1;
    WORK = 2;
  }

  reserved 100 to 199;
  reserved ""legacy_field"";
}

message Order {
  string order_id = 1;
  Customer customer = 2;
  repeated Item items = 3;
  map<string, string> attributes = 4;
  map<string, int32> quantities = 5;
  double total = 6;
  bool paid = 7;

  oneof payment {
    string card_number = 8;
    string wallet_id = 9;
  }

  message Item {
    string sku = 1;
    int32 quantity = 2;
    double price = 3;
  }
}

message KeywordSample {
  string class = 1;
  int32 operator = 2;
  string end = 3;
}

service OrderService {
  rpc GetOrder (Order) returns (Order);
  rpc WatchOrders (Order) returns (stream Order);
}
"

End Module
