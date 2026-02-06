namespace ASureBus.IO.SagaPersistence.Exceptions;

internal sealed class UnconfiguredSagaPersistenceException() 
    : Exception("Saga persistence service is not configured. Please configure a saga persistence service to use this feature.");