##iPhone Remote for Iver2 AUV
==========

### Requirements
- [TCP Proxy Server] (http://www.partow.net/programming/tcpproxy/index.html)
- [Microsoft Visual C++ Redistributable] (http://www.microsoft.com/en-us/download/details.aspx?id=5555)
- [Microsoft .NET Framework 4] (http://www.microsoft.com/en-us/download/details.aspx?id=17718)

### Setup

#### Iver Primary CPU

Run the TCP proxy server to forward the iPhone's connection from the primary CPU
to the secondary CPU.

For example, if the wireless IP of the primary CPU is `192.168.1.117` and the
local IP of the secondary CPU is `192.168.2.101`, then
```
tcpproxy_server.exe 192.168.1.117 9000 192.168.2.101 9000
```
will forward all connections to port 9000 of the primary CPU to port 9000 on the
secondary CPU.

#### Iver Secondary CPU
On the secondary CPU of the Iver, run `IverRemoteServer.exe`. A `SETTINGS.txt`
file is optional. For example, if you want the server to use network port 9000
and the serial port is `COM1`, then `SETTINGS.txt` will look like
```
9000COM1
```

#### iPhone
Make sure to be on the same wireless network as the Iver. Connect using the
wireless IP of the primary CPU. Make sure the port matches that of the TCP proxy
server.
