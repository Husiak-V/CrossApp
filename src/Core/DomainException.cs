namespace Core;

/// <summary>
/// Порушення бізнес-правила (DoD 5). Некоректні аргументи лишаються
/// ArgumentException/ArgumentOutOfRangeException, технічні збої — IOException/JsonException.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception inner) : base(message, inner) { }
}
