import struct

import gevent
from gevent.server import StreamServer

from utest_pb2 import Marine


def pack_data(i):
    m = Marine()
    m.id = i
    m.hp = 299
    m.position.x = 5
    m.position.y = 6
    m.position.z = 7
    return m.SerializeToString()


def send(sock):
    def _send():
        data = pack_data(i)
        data_len = len(data)
        fmt = '!i%ds' % data_len
        s = struct.Struct(fmt)
        data = s.pack(data_len, data)
        sock.sendall(data)

    for i in range(10):
        _send()
        gevent.sleep(1)

    print "Send Complete!"



def recv(sock):
    len_type = struct.Struct('!i')
    while True:
        try:
            data = sock.recv(4)
        except:
            print "sock recv error"
            break

        if not data:
            print "lost connection"
            break


        data_len = len_type.unpack(data)
        data = sock.recv(data_len[0])
        if not data:
            print "lost connection"
            break

        m = Marine()
        m.ParseFromString(data)

        print "recv: ",  m


def handler(client, address):
    print "new connections", address

    _r = gevent.spawn(recv, client)
    _s = gevent.spawn(send, client)

    def _clear(glet):
        client.close()
        glet.unlink(_clear)
        gevent.killall([_r, _s])
        print "Shutdown"

    _r.link(_clear)
    _s.link(_clear)
    gevent.joinall([_r, _s])


s = StreamServer(('0.0.0.0', 8888), handler)
s.serve_forever()

