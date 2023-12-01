# Usage

```csharp
    string[] args = {
        "-a",
        "-b", "b1",
        "-c", "c1", "c2", "c3",
        "-d", "d11", "d12", "d13",
        "-d", "d21", "d22"
    };

    var parseResult = new ArgsParser(
        new TagMatchGroup(tag: "-a", type: TagMatchType.Switch),
        new TagMatchGroup("-b", TagMatchType.Singal, require: true),
        new TagMatchGroup("-c", TagMatchType.Multi, false),
        new TagMatchGroup("-d", TagMatchType.ManyMulti)
        ).Parse(args);

    var a = parseResult.SwitchArguments["-a"];
    var b = parseResult.TryGet("-b") as string;
    var c = parseResult["-c"] as string[];
    var d = parseResult["-d"] as string[][];
```