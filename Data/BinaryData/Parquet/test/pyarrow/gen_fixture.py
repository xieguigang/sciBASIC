"""Generate the parquet fixture files which are used for the interop test
between the pure VB.NET parquet library and pyarrow.

usage:
    python gen_fixture.py <output-directory>

The script prints a json manifest to the standard output, the test program
uses it only for logging purpose.
"""

import datetime
import decimal
import json
import os
import sys

import pyarrow as pa
import pyarrow.parquet as pq

UTC = datetime.timezone.utc


def write(table, out_dir, name, **options):
    path = os.path.join(out_dir, name)
    pq.write_table(table, path, **options)
    return table.num_rows


def flat_basic_table():
    return pa.table({
        "int32_null": pa.array([1, 2, None, 4, 5], type=pa.int32()),
        "int32": pa.array([1, 2, 3, 4, 5], type=pa.int32()),
        "int64": pa.array([10, 20, 30, 40, 50], type=pa.int64()),
        "float32": pa.array([0.5, 1.5, 2.5, 3.5, 4.5], type=pa.float32()),
        "float64": pa.array([0.1, 0.2, 0.3, 0.4, 0.5], type=pa.float64()),
        "logical": pa.array([True, False, True, False, True], type=pa.bool_()),
        "text": pa.array(["a", "b", None, "", "e"], type=pa.string()),
        "date": pa.array(
            [datetime.date(2020, 1, 1) + datetime.timedelta(days=i) for i in range(5)],
            type=pa.date32()),
        "ts_ms": pa.array(
            [datetime.datetime(2020, 1, 1, tzinfo=UTC) + datetime.timedelta(hours=i) for i in range(5)],
            type=pa.timestamp("ms")),
        "ts_us": pa.array(
            [datetime.datetime(2021, 6, 1, tzinfo=UTC) + datetime.timedelta(seconds=i, microseconds=250000)
             for i in range(5)],
            type=pa.timestamp("us")),
    })


def flat_multigroup_table(row_count=10):
    return pa.table({
        "id": pa.array([i for i in range(row_count)], type=pa.int32()),
        "score": pa.array([i * 0.25 for i in range(row_count)], type=pa.float64()),
        "text": pa.array([None if i % 3 == 0 else "text_%d" % i for i in range(row_count)], type=pa.string()),
    })


def flat_types_table():
    return pa.table({
        "u8": pa.array([1, 2, 200, 255, 0], type=pa.uint8()),
        "u16": pa.array([1, 2, 40000, 65535, 0], type=pa.uint16()),
        "u32": pa.array([1, 2, 4000000000, 4294967295, 0], type=pa.uint32()),
        "u64": pa.array([1, 2, 18000000000000000000, 18446744073709551615, 0], type=pa.uint64()),
        "i8": pa.array([-1, 0, 127, -128, 5], type=pa.int8()),
        "i16": pa.array([-1, 0, 32767, -32768, 5], type=pa.int16()),
        "amount": pa.array(
            [decimal.Decimal("12.34"), decimal.Decimal("-0.01"),
             decimal.Decimal("100.00"), decimal.Decimal("0.10"),
             decimal.Decimal("999.99")],
            type=pa.decimal128(12, 2)),
        "payload": pa.array([b"\x00\x01\x02", b"", None, b"\xff\xfe", b"PAR1"], type=pa.binary()),
    })


def nested_table():
    map_type = pa.map_(pa.string(), pa.int64())

    return pa.table({
        "values": pa.array([[1.0, 2.0, 3.0], [4.5], [], None, [6.0, 7.0]],
                           type=pa.list_(pa.float64())),
        "attrs": pa.array([[("a", 1), ("b", 2)], [], [("c", 3)], None, [("e", None), ("d", 4)]],
                          type=map_type),
        "points": pa.array(
            [[{"x": 1, "y": 1.5}, {"x": 2, "y": 2.5}], [], [{"x": 3, "y": 3.5}], None, [{"x": 4, "y": 4.5}]],
            type=pa.list_(pa.struct([("x", pa.int64()), ("y", pa.float64())]))),
        "record": pa.array(
            [{"a": 1, "b": 1.25}, {"a": 2, "b": None}, {"a": 3, "b": 3.75}, None, {"a": 5, "b": 5.25}],
            type=pa.struct([("a", pa.int64()), ("b", pa.float64())])),
    })


def empty_table():
    return pa.table({
        "id": pa.array([], type=pa.int32()),
        "text": pa.array([], type=pa.string()),
    })


def single_row_table():
    return pa.table({
        "id": pa.array([42], type=pa.int32()),
        "text": pa.array(["only"], type=pa.string()),
        "score": pa.array([1.5], type=pa.float64()),
    })


def main():
    if len(sys.argv) < 2:
        sys.stderr.write("usage: python gen_fixture.py <output-directory>\n")
        return 2

    out_dir = sys.argv[1]
    os.makedirs(out_dir, exist_ok=True)

    manifest = {}

    basic = flat_basic_table()

    manifest["flat_basic.parquet"] = write(basic, out_dir, "flat_basic.parquet")
    manifest["flat_plain.parquet"] = write(basic, out_dir, "flat_plain.parquet", use_dictionary=False)
    manifest["flat_pagev2.parquet"] = write(basic, out_dir, "flat_pagev2.parquet",
                                            data_page_version="2.0")
    manifest["flat_gzip.parquet"] = write(basic, out_dir, "flat_gzip.parquet", compression="gzip")
    manifest["flat_none.parquet"] = write(basic, out_dir, "flat_none.parquet", compression="none")
    manifest["flat_multigroup.parquet"] = write(flat_multigroup_table(), out_dir, "flat_multigroup.parquet",
                                                row_group_size=3, compression="gzip")
    manifest["flat_delta.parquet"] = write(
        pa.table({
            "id": pa.array([1000 + i * 7 for i in range(64)], type=pa.int64()),
            "score": pa.array([i * 0.5 for i in range(64)], type=pa.float64()),
        }),
        out_dir, "flat_delta.parquet",
        use_dictionary=False, column_encoding={"id": "DELTA_BINARY_PACKED"})
    manifest["flat_delta32.parquet"] = write(
        pa.table({
            # 交替的大数值会让 DELTA_BINARY_PACKED 的 miniblock bit width 达到 32 位，
            # 用于覆盖位打包解码的 32 位溢出问题
            "id": pa.array([0 if i % 2 == 0 else 2000000000 for i in range(32)], type=pa.int64()),
        }),
        out_dir, "flat_delta32.parquet",
        use_dictionary=False, column_encoding={"id": "DELTA_BINARY_PACKED"})
    manifest["flat_types.parquet"] = write(flat_types_table(), out_dir, "flat_types.parquet")
    manifest["flat_empty.parquet"] = write(empty_table(), out_dir, "flat_empty.parquet")
    manifest["flat_single.parquet"] = write(single_row_table(), out_dir, "flat_single.parquet")
    manifest["nested.parquet"] = write(nested_table(), out_dir, "nested.parquet")

    # 行名索引列：模拟 write.parquet 生成的 __index__ 列
    indexed = flat_multigroup_table(4)
    indexed = indexed.append_column("__index__", pa.array(["r0", "r1", "r2", "r3"], type=pa.string()))
    manifest["flat_indexed.parquet"] = write(indexed, out_dir, "flat_indexed.parquet")

    sys.stdout.write(json.dumps({
        "dir": out_dir,
        "files": manifest,
        "pyarrow": pa.__version__,
    }))
    sys.stdout.write("\n")

    return 0


if __name__ == "__main__":
    sys.exit(main())
