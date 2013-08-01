## How to Compile proto

We need the same proto file for server side and client side (unity3d).

At server side, you can choose any language. C++, Java, Python, Ruby, Nodejs, Erlang, Or whatever you like.

There are two implemention of Proto For .NET. As methoned above, You'd better keep the proto file in same at server/client both side.

So, using http://code.google.com/p/protobuf-csharp-port/ .
It using the origin format to define proto message. This can be compatible with any languages.

1.  Define your proto.
2.  Compile it for server/client both side
    *   Server side. It's depends what language you are using.
    
        For example, Python:

            protoc --python_out=. <YOUR PROTO FILE>

    *   Client side.
    
            protoc --descriptor_set_out=msg.protobin --include_imports msg.proto
            protogen msg.protobin

        then you will get a `msg.cs` file, copy it in you unity3d protject's assert folder.

        **NOTE** copy `Google.ProtocolBuffers.dll` too.

3.  Done and test.

#### Checkout the example, and have fun.
