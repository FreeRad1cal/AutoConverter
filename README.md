# AutoConverter
A simple console application that watches a list of folders for new files with certain extensions, and automatically converts any new files by invoking handbrakecli.exe with the specified arguments. The configuration is provided in the config.json in the application root:
```json
{
  "WatchedPaths": [
    "c:/test",
    "c:/test2"
  ],
  "HandbrakeCliPath": "c:/handbrakecli/handbrakecli.exe",
  "Extensions": [
    ".mp4",
    ".mkv"
  ],
  "MinKb": 500,
  "Quality":  18
}
```
The `WatchedPaths` value is an array of folders that should be watched. The `HandbrakeCliPath` value is the path to the handbrakecli binary. The `Extensions` value is an array of file extensions that should be monitored. The `MinKb` value is the minimum size in kilobytes for monitored files. The `Quality` value is the constant rate factor (quality) for output files.

Alternatively, the configuration values can be supplied via command line arguments, which override the values in config.json.
