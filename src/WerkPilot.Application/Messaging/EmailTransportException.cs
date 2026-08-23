namespace WerkPilot.Application.Messaging;

public sealed class EmailTransportException(string message, Exception? innerException = null)
    : Exception(message, innerException);
