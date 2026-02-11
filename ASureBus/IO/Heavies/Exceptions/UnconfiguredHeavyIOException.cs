namespace ASureBus.IO.Heavies.Exceptions;

internal sealed class UnconfiguredHeavyIOException() 
    : Exception("Heavies not configured. Please configure a heavy IO service to use this feature.");