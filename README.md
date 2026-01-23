# CueCompanion

An open source live production utility.

## User Guide

### Concepts

A **show** is an event, a collection of information such as location and time and detailed information about what
happens during the show, called cues.

A **cue** is anything that happens during a show. Whenever something needs to change, be it an operator interacting with
a console, or turning lights on or off, or playing music, these are events that occur during the show. Each cue can have
different tasks for different roles.

A **connection** is a way for a client or an API to interact with the server. Before clients or APIs can get any
information about the show or make any changes to it, they must be connected. Only 1 client (1 web browser OR 1 external
API) can use a single connection at a time.

The **master connection** has elevated permissions such as configuration and creating other connections, and can only be
accessed from the computer running the CueCompanion server, and only from the `localhost` address.

### User Guide

To get started, run the `listConnections` command in the application command line, which will initially list out the
Master Connection and its passkey. Anyone that has access to this passkey will have full control over the application.

Connect to the web interface (using `localhost`) and enter the connection name (which in this case is "Master
Connection") and the passkey. You may need to wait for a few seconds for the client and server to establish the
connection.

To access the various functions of the app, use the drawer menu on the left. To create additional connections (for other
people that may wish to connect), use the config page.