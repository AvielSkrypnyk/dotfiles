namespace Bunq.Core.Common;

public sealed record OperationResult<T>(
    bool Success,
    T? Data,
    string? Error,
    MessageTypeEnum MessageType = MessageTypeEnum.Success,
    string? Code = null)
{
    public static OperationResult<T> Ok(T data) => new(true, data, null, MessageTypeEnum.Success, null);
    public static OperationResult<T> Fail(string error) => new(false, default, error, MessageTypeEnum.Exception, null);
    public static OperationResult<T> Fail(MessageTypeEnum messageType, string code, string error) =>
        new(false, default, error, messageType, code);
}

public sealed record OperationResult(
    bool Success,
    string? Error,
    MessageTypeEnum MessageType = MessageTypeEnum.Success,
    string? Code = null)
{
    public static OperationResult Ok() => new(true, null, MessageTypeEnum.Success, null);
    public static OperationResult Fail(string error) => new(false, error, MessageTypeEnum.Exception, null);
    public static OperationResult Fail(MessageTypeEnum messageType, string code, string error) =>
        new(false, error, messageType, code);
}
