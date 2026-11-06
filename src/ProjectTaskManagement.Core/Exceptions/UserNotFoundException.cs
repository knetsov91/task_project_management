namespace ProjectTaskManagement.Core.Exceptions;

public class UserNotFoundException(Guid userId)
    : Exception($"No user with ID '{userId}' exists.");
