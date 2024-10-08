# Template reference

This repo contains the code for generating template reference documentation. It uses files from the **generated** folder in [bicep-types-az](https://github.com/Azure/bicep-types-az) as the source files.

## Development

### Debugging

In VSCode, use the "Launch CLI" debug target to start the application in debug mode.

### Running tests

Run `dotnet test` to run the test suite.

### Updating baselines

If a test failure requires updating the baselines, follow the instructions and run the commands supplied in the error message.

If you want to update all of the baselines in one go, run:
```sh
./scripts/update_baselines.sh
```

### Building

Run `dotnet build` to build this project.

## Using the CLI

After building the .NET solution, you can use the CLI by running:

```sh
./src/TemplateRefGenerator/bin/Debug/net8.0/TemplateRefGenerator
```

If you run this command without supplying any arguments, you will see a help message giving information on the supported arguments.

Here's an example of how you can run the CLI tool:
```sh
src/TemplateRefGenerator/bin/Debug/net8.0/TemplateRefGenerator --source-folder ../bicep-types-az/generated --output-folder ./generated --include-folder ./includes --use-bicep-types
```

To get detailed logging, use the `--verbose` flag.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
