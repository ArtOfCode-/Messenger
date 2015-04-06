# Messenger

This is a simple C# server-client messenger I wrote.

### Source

Source code can be found in the src/ folder, with the client source at src/client/ and the server source at src/server/.

### Download

To download and use, download both MessengerClient.exe and MessengerServer.exe from the bin/ folder. Run MessengerServer; if you want to use a port other than the default 1100, run it from the command line with the `--port` switch followed by your desired port, i.e.

```C:\Users\You\Downloads\> MessengerServer --port 2250```

You can then run MessengerClient. To connect to a server on the same machine, use `127.0.0.1:port` as the IP; if you want to open the server to the wider Internet, you'll need to set up a portforward from your router. The client also has a command-line option: use the `--debug` switch to turn debug messages on, i.e.

```C:\Users\You\Downloads\> MessengerClient --debug```