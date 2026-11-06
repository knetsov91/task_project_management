using ProjectTaskManagement.Core.Enums;

namespace ProjectTaskManagement.Core.Exceptions;

public class InvalidTaskStatusTransitionException(TaskItemStatus currentStatus, TaskItemStatus nextStatus)
    : Exception($"Cannot change task status from '{currentStatus}' to '{nextStatus}'.");
