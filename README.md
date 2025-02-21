# Overview

This is a simple client-server network application. Built using the C# language with NetworkStream
and TcpClient for broadcasting, this program offers a single-file solution for simple CLI-Based
communication.

[Software Demo Video](https://youtu.be/40bsnjnBWtA)

# Network Communication

This program is designed using a Client-Server model. Communication is accomplished using TCP over
port 8888, a common TCP port for software testing that avoids conflicting with other network services.
The messages sent between clients are unencrypted NetworkStream packets transmitted using TCP.

# Development Environment

This program was developed in JetBrains Rider using the C# programming language with NetworkStream
and TcpClient.

# Useful Websites

* [NetworkStream docs](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream?view=net-9.0)
* [TcpClient docs](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-9.0)

# Future Work

* Add public-key encryption
* Add file transfer capability.
* Add a simple GUI using WinUI 3
