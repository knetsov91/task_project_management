namespace ProjectTaskManagement.Core.Exceptions;

public sealed class DuplicateEmailException()
    : Exception("A user with this email already exists.");
