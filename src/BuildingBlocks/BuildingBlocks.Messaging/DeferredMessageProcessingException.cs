namespace BuildingBlocks.Messaging;

public sealed class DeferredMessageProcessingException(string message) : Exception(message);
