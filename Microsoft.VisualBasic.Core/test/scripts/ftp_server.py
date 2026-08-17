#!/usr/bin/env python
# -*- coding: utf-8 -*-
'''
ftp_server.py - 简易明文 FTP 测试服务器 (仅用于本地单元测试)

用于验证 Microsoft.VisualBasic.Net.FTP.FtpClient 模块的下载/文件信息/列目录等功能。
兼容 Python 2.7 与 Python 3 (仅标准库, 零第三方依赖)。

特性:
  - 被动模式: EPSV (IPv4/IPv6) 与 PASV (IPv4)
  - 认证: USER/PASS, 支持匿名 (anonymous) 与指定账号密码
  - 命令: TYPE / OPTS UTF8 ON / SYST / PWD / CWD / SIZE / MDTM
          RETR (下载) / NLST (列目录) / QUIT / ABOR / PBSZ / PROT
  - 启动时自动在 root 目录生成测试文件:
      test_1MB.bin   (约 1MB 随机字节, 用于内容一致性校验)
      test_small.txt (文本文件)

用法:
  python ftp_server.py [--host 127.0.0.1] [--port 2121]
                       [--root <dir>] [--user user] [--password pass]
'''

from __future__ import print_function

import argparse
import os
import random
import socket
import sys
import threading
import time

CRLF = "\r\n"
PY3 = sys.version_info[0] >= 3


def to_bytes(s):
    """将 str 编码为 bytes (Python2 中 str 已是 bytes, 无需转换)。"""
    if PY3:
        return s.encode("utf-8")
    return s


def to_str(b):
    """将 bytes 解码为 str (Python2 中 bytes 即 str)。"""
    if PY3:
        return b.decode("utf-8", errors="replace")
    return b


def rand_bytes(n):
    """生成 n 字节伪随机数据 (种子固定, 保证内容一致性)。"""
    rnd = random.Random(42)
    if PY3:
        return bytes(rnd.randrange(256) for _ in range(n))
    # Python 2: 通过 bytearray 生成再转 str (bytes)
    return b"".join(chr(rnd.randint(0, 255)) for _ in range(n))


class FtpServer(object):
    def __init__(self, host, port, root, user, password):
        self.host = host
        self.port = port
        self.root = os.path.abspath(root)
        self.user = user
        self.password = password
        if not os.path.isdir(self.root):
            os.makedirs(self.root)

    def generate_test_files(self):
        """生成测试用文件 (若不存在)。"""
        big = os.path.join(self.root, "test_1MB.bin")
        if not os.path.exists(big):
            with open(big, "wb") as f:
                f.write(rand_bytes(1024 * 1024))
        small = os.path.join(self.root, "test_small.txt")
        if not os.path.exists(small):
            with open(small, "wb") as f:
                f.write(to_bytes("hello ftp test\nline two\nline three\n"))
        print("[server] test files ready in:", self.root)
        for name in os.listdir(self.root):
            print("[server]   -", name)

    def resolve(self, name):
        """将远程路径解析为 root 下的绝对路径, 返回 None 表示非法/越界。"""
        if name in ("/", ""):
            return self.root
        rel = name.lstrip("/")
        full = os.path.abspath(os.path.join(self.root, rel))
        if os.path.commonprefix([self.root + os.sep, full]) != self.root + os.sep:
            return None
        return full

    def make_passive_socket(self):
        """建立被动数据监听 socket, 返回 (socket, port)。"""
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        s.bind((self.host, 0))
        s.listen(1)
        s.settimeout(10)
        return s, s.getsockname()[1]

    def accept_data(self, data_socket):
        conn, _ = data_socket.accept()
        data_socket.close()
        return conn

    def send(self, conn, line):
        conn.sendall(to_bytes(line + CRLF))

    def handle(self, conn, addr):
        try:
            self.send(conn, "220 Welcome to FtpTest Server")
            data_socket = None
            binary = True
            authed = False
            username = None

            buf = []
            while True:
                chunk = conn.recv(4096)
                if not chunk:
                    break
                buf.append(chunk)
                while b"\n" in b"".join(buf) or (b"\n" in buf[-1]):
                    joined = b"".join(buf)
                    idx = joined.find(b"\n")
                    if idx < 0:
                        break
                    raw = joined[:idx].rstrip(b"\r")
                    buf = [joined[idx + 1:]]
                    line = to_str(raw).rstrip("\r\n")
                    if not line.strip():
                        continue
                    cmd, _, arg = line.partition(" ")
                    arg = arg.strip()
                    ucmd = cmd.upper()

                    if ucmd == "USER":
                        username = arg
                        authed = False
                        self.send(conn, "331 Password required.")
                    elif ucmd == "PASS":
                        if self.user is None:
                            ok = True
                        elif username == self.user and arg == self.password:
                            ok = True
                        elif username and username.lower() == "anonymous":
                            ok = True
                        else:
                            ok = False
                        if ok:
                            authed = True
                            self.send(conn, "230 Login successful.")
                        else:
                            self.send(conn, "530 Login incorrect.")
                    elif ucmd == "QUIT":
                        self.send(conn, "221 Goodbye.")
                        return
                    elif ucmd == "TYPE":
                        binary = arg.upper().startswith("I")
                        self.send(conn, "200 Switching to Binary mode.")
                    elif ucmd in ("OPTS", "PBSZ", "PROT", "NOOP"):
                        self.send(conn, "200 OK.")
                    elif ucmd == "SYST":
                        self.send(conn, "215 UNIX Type: L8")
                    elif ucmd == "PWD":
                        self.send(conn, '257 "/" is current directory.')
                    elif ucmd == "CWD":
                        target = self.resolve(arg or "/")
                        if target is not None and os.path.isdir(target):
                            self.send(conn, "250 Directory successfully changed.")
                        else:
                            self.send(conn, "550 Failed to change directory.")
                    elif ucmd == "EPSV":
                        data_socket, data_port = self.make_passive_socket()
                        self.send(conn, "229 Entering Extended Passive Mode (|||%d|)" % data_port)
                    elif ucmd == "PASV":
                        data_socket, data_port = self.make_passive_socket()
                        p1, p2 = data_port // 256, data_port % 256
                        self.send(conn, "227 Entering Passive Mode (127,0,0,1,%d,%d)." % (p1, p2))
                    elif ucmd == "ABOR":
                        self.send(conn, "226 ABOR successful.")
                    elif ucmd == "SIZE":
                        target = self.resolve(arg)
                        if target is not None and os.path.isfile(target):
                            self.send(conn, "213 %d" % os.path.getsize(target))
                        else:
                            self.send(conn, "550 Could not get file size.")
                    elif ucmd == "MDTM":
                        target = self.resolve(arg)
                        if target is not None and os.path.isfile(target):
                            t = time.gmtime(os.path.getmtime(target))
                            ts = time.strftime("%Y%m%d%H%M%S", t)
                            self.send(conn, "213 %s" % ts)
                        else:
                            self.send(conn, "550 Could not get file modification time.")
                    elif ucmd in ("RETR", "NLST"):
                        if data_socket is None:
                            self.send(conn, "425 Use PORT or PASV first.")
                            continue
                        if ucmd == "RETR":
                            target = self.resolve(arg)
                            if target is None or not os.path.isfile(target):
                                self.send(conn, "550 File not found.")
                                data_socket.close()
                                data_socket = None
                                continue
                            self.send(conn, "150 Opening BINARY mode data connection.")
                            dconn = self.accept_data(data_socket)
                            try:
                                with open(target, "rb") as fsrc:
                                    while True:
                                        chunk = fsrc.read(65536)
                                        if not chunk:
                                            break
                                        dconn.sendall(chunk)
                            finally:
                                dconn.close()
                            self.send(conn, "226 Transfer complete.")
                        else:  # NLST
                            target = self.resolve(arg or "/")
                            if target is None or not os.path.isdir(target):
                                self.send(conn, "550 Directory not found.")
                                data_socket.close()
                                data_socket = None
                                continue
                            names = [n for n in os.listdir(target)
                                     if not n.startswith(".")]
                            self.send(conn, "150 Here comes the directory listing.")
                            dconn = self.accept_data(data_socket)
                            try:
                                for n in names:
                                    dconn.sendall(to_bytes(n + CRLF))
                            finally:
                                dconn.close()
                            self.send(conn, "226 Directory send OK.")
                        data_socket = None
                    else:
                        self.send(conn, "502 Command not implemented.")
        except Exception as e:
            import traceback
            try:
                conn.sendall(to_bytes("421 Service not available, closing control connection." + CRLF))
            except Exception:
                pass
            print("[server] ERROR in handler:", repr(e), file=sys.stderr)
            traceback.print_exc(file=sys.stderr)
            sys.stderr.flush()
        finally:
            try:
                conn.close()
            except OSError:
                pass

    def serve_forever(self):
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        s.bind((self.host, self.port))
        s.listen(5)
        print("[server] listening on %s:%d (root=%s)" % (self.host, self.port, self.root))
        sys.stdout.flush()
        while True:
            conn, addr = s.accept()
            t = threading.Thread(target=self.handle, args=(conn, addr))
            t.daemon = True
            t.start()


def main():
    parser = argparse.ArgumentParser(description="Simplified plaintext FTP test server")
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=2121)
    parser.add_argument("--root", default="./_ftp_root")
    parser.add_argument("--user", default=None, help="login username; None => anonymous only")
    parser.add_argument("--password", default=None)
    args = parser.parse_args()

    server = FtpServer(args.host, args.port, args.root, args.user, args.password)
    server.generate_test_files()
    server.serve_forever()


if __name__ == "__main__":
    main()
