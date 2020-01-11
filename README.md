# Open GL Helpers for .NET

Hobbyist mucking about with [OpenGL.Net](https://github.com/luca-piccioni/OpenGL.Net).

* **Core:** A few low-level classes that wrap around the static API classes presented by OpenGL.Net (OpenGL.Net - the source at least - does include some higher-level classes, but they aren't really to my taste)
* **ReactiveBuffers:** Building on top of core, some logic for managing buffer content via [ReactiveX](http://reactivex.io/).
* **Views:** Building on top of Core and ReactiveBuffers, a simple-to-the-point-of-naivety rendering engine.

## Usage

See the [example app code](./src/ExampleApps.WinForms).