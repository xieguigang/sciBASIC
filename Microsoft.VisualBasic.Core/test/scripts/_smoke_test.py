# -*- coding: utf-8 -*-
'''Smoke test client for ftp_server.py (Python 2.7 compatible).'''
from __future__ import print_function
import socket

HOST = "127.0.0.1"
PORT = 2121


def connect():
    s = socket.create_connection((HOST, PORT), timeout=10)
    print("220:", s.recv(4096).strip())
    return s


def send(s, cmd):
    s.sendall((cmd + "\r\n").encode("utf-8"))


def recvline(s):
    buf = b""
    while not buf.endswith(b"\n"):
        buf += s.recv(1)
    return buf.decode("utf-8").strip()


def recv_until(s, expect_codes):
    while True:
        line = recvline(s)
        print("  <-", line)
        code = line[:3]
        if code in expect_codes:
            return line


def read_all(s):
    data = b""
    while True:
        chunk = s.recv(4096)
        if not chunk:
            break
        data += chunk
    return data


def main():
    s = connect()
    send(s, "USER anonymous")
    recv_until(s, ["331"])
    send(s, "PASS anonymous@localhost")
    recv_until(s, ["230"])

    send(s, "TYPE I")
    recv_until(s, ["200"])
    send(s, "OPTS UTF8 ON")
    recv_until(s, ["200"])

    send(s, "SIZE /test_1MB.bin")
    size_line = recv_until(s, ["213"])
    size = int(size_line.split()[1])
    print("SIZE =", size, "(expect 1048576)")

    # EPSV + RETR download
    send(s, "EPSV")
    epsv = recv_until(s, ["229"])
    inner = epsv[epsv.find("(") + 1:epsv.rfind(")")]
    data_port = int(inner.strip("|").split("|")[-1])
    data_sock = socket.create_connection((HOST, data_port), timeout=10)
    send(s, "RETR /test_1MB.bin")
    recv_until(s, ["150"])
    content = read_all(data_sock)
    data_sock.close()
    recv_until(s, ["226"])
    print("DOWNLOADED bytes =", len(content), "match =", len(content) == size)

    # NLST listing
    send(s, "EPSV")
    epsv2 = recv_until(s, ["229"])
    inner2 = epsv2[epsv2.find("(") + 1:epsv2.rfind(")")]
    data_port2 = int(inner2.strip("|").split("|")[-1])
    data_sock2 = socket.create_connection((HOST, data_port2), timeout=10)
    send(s, "NLST /")
    recv_until(s, ["150"])
    listing = read_all(data_sock2).decode("utf-8").strip().splitlines()
    data_sock2.close()
    recv_until(s, ["226"])
    print("LISTING =", listing)

    send(s, "QUIT")
    recv_until(s, ["221"])
    s.close()
    print("SMOKE TEST OK")


if __name__ == "__main__":
    main()
