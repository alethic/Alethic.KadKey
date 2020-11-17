Launch two copies. Set the URL for each:

`Alethic.KeyShift.Console.exe --url=http://localhost:5000 Alethic.KeyShift:Uri=http://localhost:5000/host`

The `--url` command line argument sets the listen address. This is the hostname and port the application will bind to.
The `Alethic.KeyShift:Uri` configuration value sets the URL to be advertised to other nodes for host-host communications.

Change the port number for each copy.

GET and PUT values at `http://localhost:{port}/keys/{keyname}` to each service. They should see each other.